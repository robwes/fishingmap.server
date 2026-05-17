# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

ASP.NET Core (net10.0) backend for https://fishingmap.fi/ — a site that shows fishing locations in Uusimaa, Finland. Solution: `FishingMapCore.sln`.

## Common commands

Run from the repo root.

- Build: `dotnet build --configuration Release`
- Run API locally: `dotnet run --project FishingMap.API`
- Run all tests: `dotnet test --configuration Release`
- Run a single test class: `dotnet test --filter "FullyQualifiedName~LocationServiceTests"`
- Run a single test method: `dotnet test --filter "FullyQualifiedName~LocationServiceTests.MethodName"`

EF Core migrations (must specify both projects — connection string lives in API user secrets):

- Add migration: `dotnet ef migrations add <Name> --project FishingMap.Data --startup-project FishingMap.API`
- Update database: `dotnet ef database update --project FishingMap.Data --startup-project FishingMap.API`

CI/CD: `.github/workflows/master_fishingmapapi.yml` builds, tests, and deploys to Azure Web App `fishingmapapi` on push to `master`.

## Architecture

Five projects in a layered dependency chain `API → Domain → Data → Common`:

- **FishingMap.API** — Composition root. `Program.cs` wires DI, JWT auth, CORS, Mapster, EF, GeometryFactory, services, and `DbInitializer`. Controllers are thin; they delegate to `I*Service` in Domain. `AzureFileService` (Azure File Share) and `AuthService` live here rather than Domain because they have ASP.NET / Azure dependencies.
- **FishingMap.Domain** — Business logic in `Services/`, DTOs in `DTO/`, Mapster mappings in `MapsterConfig/MapsterRegister.cs`. Services depend on `IUnitOfWork` (not individual repositories) and call `_unitOfWork.SaveChanges()` to commit.
- **FishingMap.Data** — EF Core. `ApplicationDbContext` declares all `DbSet`s and configures relationships in `OnModelCreating` (unique indexes, many-to-many `RoleUser`, `SpeciesRegulation` cascade). Generic `Repository<TEntity>` provides `Add/Find/GetAll/GetById/Update/Delete` with optional includes/filter/orderBy. Per-entity repositories (e.g. `LocationRepository`) inherit from it and add specialized queries. `UnitOfWork` aggregates them and owns `SaveChanges`. `FishingMapContextFactory` is the design-time factory used by `dotnet ef`.
- **FishingMap.Common** — Cross-cutting: `Cryptography` (PBKDF2 salt/hash via `Microsoft.AspNetCore.Cryptography.KeyDerivation`), `IEnumerableExtensions`, `NetTopologySuiteExtensions` (e.g. `ToGeoJsonFeature`, `GeoJsonFeatureToMultiPolygon`), and `IFishingMapConfiguration` (concrete impl in API). Domain references this via Data.
- **FishingMap.Domain.Tests** — xUnit + Moq + `Moq.EntityFrameworkCore`. Tests construct services with mocked `IUnitOfWork` and a real Mapster `IMapper` built from `MapsterRegister`.

### Cross-cutting conventions

- **Spatial data**: SQL Server with NetTopologySuite (`UseNetTopologySuite()`). A `GeometryFactory` is registered scoped with `PrecisionModel()` and **SRID 4326** — always use it (don't `new` a factory ad hoc) so geometries match the DB column SRID. GeoJSON serialization for both responses and inbound binding uses `NetTopologySuite.IO.Converters.GeoJsonConverterFactory` with the same factory; this is configured in `Program.cs` and again inside `FormDataJsonBinder`.
- **Form data with embedded JSON**: `FormDataJsonBinderProvider` is inserted at index 0 of the model binders. It lets `[FromForm]` DTOs (e.g. `LocationAdd`) carry GeoJSON / nested-object fields as JSON strings alongside file uploads in `multipart/form-data`. When adding a new form-bound DTO with JSON sub-fields, this binder handles them automatically — don't try to parse JSON manually in the controller.
- **Auth**: JWT bearer with `ValidIssuer`/`ValidAudience`/`ValidateLifetime` all on. `OnMessageReceived` reads the token from a cookie named `token` (in addition to the `Authorization` header), so the SPA can use HTTP-only cookies. Admin-only endpoints use `[Authorize(Roles = "Administrator")]`.
- **Mapping**: Mapster, not AutoMapper (migrated in commits `8c16f85`/`eee9a0c`). `MapsterRegister.Register` is the single registration point — scanned in `Program.cs` and in test setup. Add new mappings there.
- **DB seeding**: `DbInitializer.InitializeAsync` runs at startup, calls `EnsureCreated()` (note: bypasses migrations on first run) and seeds an `Administrator` role, a `User` role, and an `admin/admin12` user if the Users table is empty.
- **Error handling in controllers**: pattern is `try { ... } catch (KeyNotFoundException) → 404; catch (ArgumentException ex) → 400 with message; catch (Exception) → 500 generic`. Services throw `KeyNotFoundException` for missing entities and `ArgumentException` for validation failures.

## Configuration

`appsettings.json` ships placeholders only. Real secrets go in user secrets (`UserSecretsId` is set on `FishingMap.API.csproj` and `FishingMap.Domain.csproj`). Required keys:

- `ConnectionStrings:FishingMapDatabase` — SQL Server
- `ConnectionStrings:FishingMapStorage` — Azure Storage (file share for images)
- `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`
- `FileShare:Name`, `FileShare:LocationsImageFolderPath`, `FileShare:SpeciesImageFolderPath`

CORS origins are hard-coded in `Program.cs`: `https://fishingmap.fi` in Production, `http://localhost:3000` otherwise.
