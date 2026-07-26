# AGENTS.md

Single source of project guidance for AI coding agents working in this repository. Claude Code imports this file via `CLAUDE.md`; Codex and other agents read it directly. **Edit this file, not CLAUDE.md.**

## Active work

Multi-session features are tracked as GitHub issues in **`robwes/fishingmap.web`** (the frontend repo hosts them even when the work is backend-side, so there is one queue rather than two), labelled **`feature-plan`**. Each one records the decisions already made, the options already rejected, per-surface status, and the remaining plan.

Before starting on a feature, check for an open one:

```
gh issue list --repo robwes/fishingmap.web --label feature-plan
gh issue view <number> --repo robwes/fishingmap.web
```

Update the issue at the end of a session. When the feature ships, graduate its durable constraints into this file and close the issue.

## Project

ASP.NET Core (net10.0) backend for https://fishingmap.fi/ — a site that shows fishing locations in Uusimaa, Finland. Solution: `FishingMapCore.sln`.

## Common commands

Run from the repo root.

- Build: `dotnet build --configuration Release`
- Run API locally: `dotnet run --project FishingMap.API` — listens on `https://localhost:7299` and `http://localhost:5000` (from `launchSettings.json`). For the full agent recipe (readiness probe, curl examples, auth cookie jar, stop procedure, expected startup warnings), see `.claude/skills/run-fishingmap-server/SKILL.md` — Claude Code loads it as a skill; other agents can read the file directly.
- Run all tests: `dotnet test --configuration Release`
- Run a single test class: `dotnet test --filter "FullyQualifiedName~LocationServiceTests"`
- Run a single test method: `dotnet test --filter "FullyQualifiedName~LocationServiceTests.MethodName"`

EF Core migrations (must specify both projects — connection string lives in API user secrets):

- Add migration: `dotnet ef migrations add <Name> --project FishingMap.Data --startup-project FishingMap.API`
- Update database: `dotnet ef database update --project FishingMap.Data --startup-project FishingMap.API`

CI/CD: `.github/workflows/master_fishingmapapi.yml` builds, tests, and deploys to Azure Web App `fishingmapapi` on push to `master`.

## Deploying to production

Ordering rule: **expand the schema first, then ship the code** — additive migrations are compatible with the still-running old app, but new code that needs a missing table is not.

1. Commit. If migrations are pending on prod, run the `deploy-migrations` skill (`.claude/skills/deploy-migrations/`; idempotent script, shows diff, requires explicit confirmation). Check state read-only with `dotnet ef migrations list ... --connection $env:FISHINGMAP_PROD_CONN` (PowerShell; the env var holds the prod connection string — never read it from user secrets, those point at dev).
2. `git push origin master`, then watch with `gh run watch <id>` (gh CLI is installed and authenticated as robwes).
3. Verify: `curl -s https://api.fishingmap.fi/api/species` → 200, `/api/auth/whoami` → 401, login with bad creds → 400. First request after restart can be slow (cold start) — retry before concluding breakage.

The frontend repo (`fishingmap.web`) deploys the same way — push to its `master` triggers its workflow (re-enabled July 2026; the build's *contents* deploy to wwwroot root — see that repo's AGENTS.md). Rollback strategy is fix-forward or `git revert` + push; never run `Down()` migrations against prod.

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
- **Auth**: JWT bearer (60 min, HS256) with `ValidIssuer`/`ValidAudience`/`ValidateLifetime` all on. `OnMessageReceived` reads the token from a cookie named `token` (in addition to the `Authorization` header), so the SPA can use HTTP-only cookies. `NameIdentifier` claim is the user **id** (not username). Admin-only endpoints use `[Authorize(Roles = "Administrator")]`. Sessions stay alive via rotating **refresh tokens**: `POST /api/auth/refresh` reads the `refreshToken` cookie (path-scoped to `/api/auth`), validates it against the hashed `RefreshTokens` table via `RefreshTokenService`, rotates it, and re-issues both cookies; reuse of a rotated token revokes all of that user's tokens. Cookies are `SameSite=Lax` in Production (CSRF protection — fishingmap.fi and api.fishingmap.fi are same-site) but `None` in Development because http://localhost:3000 → https://localhost:7299 is schemeful-cross-site. Login is rate-limited per IP and burns a dummy PBKDF2 hash for unknown usernames so timing doesn't leak account existence.
- **Mapping**: Mapster, not AutoMapper (migrated in commits `8c16f85`/`eee9a0c`). `MapsterRegister.Register` is the single registration point — scanned in `Program.cs` and in test setup. Add new mappings there.
- **DB seeding**: `DbInitializer.InitializeAsync` runs at startup, calls `EnsureCreated()` (note: bypasses migrations on first run) and seeds an `Administrator` role, a `User` role, and an `admin/admin12` user if the Users table is empty. The local dev DB's admin password has since been changed — `admin/admin12` does not work there; ask the user for credentials when an admin-gated flow is needed.
- **PATCH semantics**: partial-update DTOs (e.g. `LocationInfoPatch`) use `Optional<T>` from `DTO/Common/Optional.cs` — a JSON property that is absent leaves the field untouched (`HasValue == false`), while an explicit `null` clears it. Follow this pattern for new PATCH endpoints instead of nullable properties.
- **Error handling**: the global `ApiExceptionFilter` (registered in `Program.cs`) maps `KeyNotFoundException → 404`, `ArgumentException → 400` with the message, anything else → generic 500 (logged). Controllers contain no try/catch — services throw `KeyNotFoundException` for missing entities and `ArgumentException` for validation failures, and the filter does the rest. Don't add per-action try/catch to new controllers.

## Configuration

`appsettings.json` ships placeholders only. Real secrets go in user secrets (`UserSecretsId` is set on `FishingMap.API.csproj` and `FishingMap.Domain.csproj`). Required keys:

- `ConnectionStrings:FishingMapDatabase` — SQL Server
- `ConnectionStrings:FishingMapStorage` — Azure Storage (file share for images)
- `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`
- `FileShare:Name`, `FileShare:LocationsImageFolderPath`, `FileShare:SpeciesImageFolderPath`

CORS origins are hard-coded in `Program.cs`: `https://fishingmap.fi` in Production, `http://localhost:3000` otherwise.
