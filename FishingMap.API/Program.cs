using FishingMap.Domain.Data.Context;
using FishingMap.Domain.Interfaces;
using FishingMap.Domain.Services;
using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using FishingMap.Domain.AutoMapperProfiles;
using Microsoft.AspNetCore.HttpOverrides;
using FishingMap.API;
using FishingMap.API.ModelBinders;
using FishingMap.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.Tasks;
using FishingMap.API.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
    options.ModelBinderProviders.Insert(0, new FormDataJsonBinderProvider()));

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddCors();

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
                builder.Configuration["Jwt:Key"]))
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

builder.Services.AddAutoMapper(typeof(DomainProfile));

var connectionString = builder.Configuration.GetConnectionString("FishingMapDatabase");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString, opt => opt.UseNetTopologySuite()), ServiceLifetime.Scoped);

builder.Services.AddSingleton<IFishingMapConfiguration, FishingMapConfiguration>();
builder.Services.AddSingleton<IFileService, FishingMap.API.Services.AzureFileService>();

builder.Services.AddScoped<GeometryFactory>(provider => new GeometryFactory(new PrecisionModel(), 4326));

builder.Services.AddScoped<ILocationsService, LocationsService>();
builder.Services.AddScoped<ILocationOwnersService, LocationOwnersService>();
builder.Services.AddScoped<ISpeciesService, SpeciesService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPermitsService, PermitsService>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

app.UseForwardedHeaders();

app.UseCors(builder => builder
    .WithOrigins(new[] { "https://fishingmap.azurewebsites.net", "https://fishingmap.fi", "http://localhost:3000" })
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()
);

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

app.Run();