---
name: run-fishingmap-server
description: Build, run, and probe the FishingMap ASP.NET Core API locally (dotnet run → https://localhost:7299). Use when asked to run or start the backend, hit an endpoint, or verify a change against the real running API.
---

ASP.NET Core (net10.0) API. It talks to a **real SQL Server database and Azure
File Share** configured in user secrets on this machine — there is no
in-memory/dev fake. Whatever `ConnectionStrings:FishingMapDatabase` points at
is what you read and mutate.

All commands below are Git Bash, run from the repo root.

## Prerequisites

Already satisfied on this machine — nothing to install:

- .NET 10 SDK (`dotnet --version`)
- User secrets for `FishingMap.API` (connection strings, JWT, file share)
- Trusted ASP.NET dev certificate (curl still needs `-k`, see Gotchas)

## Build / Test

```bash
dotnet build --configuration Release
dotnet test --configuration Release
```

98 xUnit tests (service-layer, mocked `IUnitOfWork`), passing as of July 2026.
Single class: `dotnet test --filter "FullyQualifiedName~LocationServiceTests"`.

## Run (agent path)

The API is **strict on its ports** (7299 https / 5000 http from
`launchSettings.json`) — check whether it's already up before starting one:

```bash
curl -sk -o /dev/null -w "%{http_code}" --max-time 3 https://localhost:7299/api/locations
# 200 → already running, don't start another; 000 → not running
```

Start it in the background (use run_in_background, not `&`):

```bash
dotnet run --project FishingMap.API
```

Ready when `/api/locations` answers 200 — typically 5–10 s (verified: first
probe succeeded 1 s after the build finished). Startup runs `DbInitializer`
against the real dev DB (`EnsureCreated()` + seeds the National region /
admin user if missing) — normal dev flow, but it does touch the DB.

Stop it (only if you started it): find the PID listening on 7299 and kill it.
Git Bash needs double slashes on taskkill flags:

```bash
netstat -ano | grep ":7299" | grep LISTENING
taskkill //PID <pid> //F
```

## Probe endpoints

GET endpoints are anonymous; all mutations require the Administrator role.

```bash
curl -sk https://localhost:7299/api/locations | head -c 500
curl -sk https://localhost:7299/api/species
curl -sk https://localhost:7299/api/locations/30
curl -sk "https://localhost:7299/api/locations?search=&radius=50&orgLat=60.17&orgLng=24.94"
curl -sk https://localhost:7299/api/regions
curl -sk https://localhost:7299/api/regulations/location/30
```

### Auth-gated endpoints

`POST /api/auth/login` sets an HttpOnly cookie named `token`; the JWT is also
accepted as an `Authorization: Bearer` header. Cookie-jar recipe:

```bash
CJ=/tmp/fm-cookies.txt
curl -sk -c "$CJ" -X POST https://localhost:7299/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"userName":"<user>","password":"<password>"}'
curl -sk -b "$CJ" https://localhost:7299/api/auth/whoami
# rotate the session (login also set a refreshToken cookie, path=/api/auth)
curl -sk -b "$CJ" -c "$CJ" -X POST https://localhost:7299/api/auth/refresh
```

Login is rate-limited to 5 attempts/min per IP (429 after that) — don't
loop login probes.

The `DbInitializer` default is admin/admin12, but **the local dev DB has a
different admin password** (verified July 2026: admin/admin12 → 400 "Invalid
credentials"). Ask the user for working credentials if you need an
admin-gated flow; don't burn attempts guessing.

## Run (human path)

```bash
dotnet run --project FishingMap.API   # Ctrl-C to stop
```

Or F5 in Visual Studio (profile `FishingMap.API`).

## Gotchas

- **curl needs `-k`** against `https://localhost:7299` (dev cert isn't in Git
  Bash curl's CA bundle). With `-k` it works reliably from this repo — the
  "don't trust curl for backend liveness" warning in the *web* repo's skill
  does not reproduce here.
- **`http://localhost:5000` 307-redirects to https** (`UseHttpsRedirection`);
  probe the 7299 origin directly.
- **Startup should be warning-free** (as of July 2026): the NU1901 NuGet
  warnings, EF decimal-precision warnings, and `MultipleCollectionIncludeWarning`
  were all fixed. If a build or startup warning appears, treat it as new
  breakage, not known noise.
- **Development env logs full SQL** for every request (EF `Debug` logging) —
  the background output file grows fast; grep it rather than reading whole.
- **CORS allows only `http://localhost:3000`** in dev — irrelevant for curl
  (no preflight), but browser-based frontends must run on that origin.
- The frontend repo (`fishingmap.web`) expects this API at
  `https://localhost:7299` via `VITE_BASE_URL` — starting this server is how
  you get real data into web-app screenshots.
