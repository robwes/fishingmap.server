using FishingMap.API;
using FishingMap.API.Filters;
using FishingMap.API.Interfaces;
using FishingMap.API.ModelBinders;
using FishingMap.API.Services;
using FishingMap.Data.Context;
using FishingMap.Data.Interfaces;
using FishingMap.Data.Repositories;
using FishingMap.Domain.MapsterConfig;
using FishingMap.Domain.Interfaces;
using FishingMap.Domain.Services;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetTopologySuite.Geometries;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Single factory instance shared by JSON serialization and DI (SRID 4326, see CLAUDE.md)
var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);

builder.Services.AddControllers(options =>
    {
        options.ModelBinderProviders.Insert(0, new FormDataJsonBinderProvider());
        options.Filters.Add<ApiExceptionFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory(geometryFactory));
    });

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddCors();

// Brute-force protection: per-IP fixed window on the login endpoint
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"]!))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var cookies = context.HttpContext.Request.Cookies;
                if (cookies.TryGetValue("token", out var accessTokenValue))
                {
                    context.Token = accessTokenValue;
                }
                return Task.CompletedTask;
            }
        };
    });

// Configure Mapster
var config = TypeAdapterConfig.GlobalSettings;
config.Scan(typeof(MapsterRegister).Assembly);
builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();

// Register the database context
var connectionString = builder.Configuration.GetConnectionString("FishingMapDatabase");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString, opt => opt.UseNetTopologySuite()), ServiceLifetime.Scoped);
builder.Services.AddScoped<DbInitializer>();

// Register the configuration
builder.Services.AddSingleton<IFishingMapConfiguration, FishingMapConfiguration>();

// Register the repositories
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<ILocationOwnerRepository, LocationOwnerRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IPermitRepository, PermitRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IRegionRepository, RegionRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ISpeciesRepository, SpeciesRepository>();
builder.Services.AddScoped<ISpeciesRegulationRepository, SpeciesRegulationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register the UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register the GeometryFactory (same instance the JSON converters use)
builder.Services.AddSingleton(geometryFactory);

// Register the services
builder.Services.AddSingleton<IFileService, FishingMap.API.Services.AzureFileService>();

builder.Services.AddScoped<ILocationsService, LocationsService>();
builder.Services.AddScoped<ILocationOwnersService, LocationOwnersService>();
builder.Services.AddScoped<ISpeciesService, SpeciesService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IPermitsService, PermitsService>();
builder.Services.AddScoped<IRegionsService, RegionsService>();
builder.Services.AddScoped<IRegulationsService, RegulationsService>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbInitializer = services.GetRequiredService<DbInitializer>();
    await dbInitializer.InitializeAsync();
}

app.UseForwardedHeaders();

// Exception handling first so it wraps the rest of the pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(
            new { message = "An error occurred while processing your request." });
    }));
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Environment.IsProduction())
{
    app.UseCors(builder => builder
        .WithOrigins(new[] { "https://fishingmap.fi" })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
    );
} 
else
{
    app.UseCors(builder => builder
        .WithOrigins(new[] { "http://localhost:3000" })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
    );
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();