---
name: deploy-migrations
description: Apply pending EF Core migrations to the Azure SQL production database for FishingMap. Generates an idempotent SQL script, shows the diff, requires explicit confirmation, then applies. Use when the user says "deploy migrations", "run migrations on prod", "migrate the Azure database", or similar.
---

# Deploy EF Core migrations to Azure SQL

This skill walks through applying pending migrations from the local branch to the production Azure SQL database. It is **destructive to schema** — every step before the apply step must complete cleanly, and the apply step requires explicit user confirmation.

## Prerequisites the user must have in place

Before running, verify (ask if unclear):

1. `$env:FISHINGMAP_PROD_CONN` is set in the current PowerShell session, holding the production Azure SQL connection string. **Do not** read it from `appsettings.json` or user secrets — the local connection string points at dev. If the env var is missing, stop and tell the user to set it: `$env:FISHINGMAP_PROD_CONN = "Server=tcp:...;Database=...;User ID=...;Password=...;Encrypt=True;..."`.
2. The user's current public IP is allowed in the Azure SQL firewall. The skill cannot check this directly — remind the user to confirm, and if the first `dotnet ef` call fails with a connection error, surface the firewall as the most likely cause.
3. The working tree is on the branch whose migrations should be deployed, and is built (`dotnet build --configuration Release`) without errors. Run the build as part of step 1 below.

## Steps

Execute these in order. Stop and report to the user on any failure — do not attempt to recover unilaterally.

### 1. Build and list pending migrations

```powershell
dotnet build --configuration Release
dotnet ef migrations list --project FishingMap.Data --startup-project FishingMap.API --connection $env:FISHINGMAP_PROD_CONN
```

`migrations list` marks applied migrations with `(Applied)`. Identify the highest applied migration on prod and the highest migration in the branch. Report both to the user and confirm the set of migrations that will be applied. If nothing is pending, stop and tell the user — nothing to do.

### 2. Generate an idempotent SQL script

```powershell
dotnet ef migrations script --idempotent --project FishingMap.Data --startup-project FishingMap.API --output migrate.sql
```

`--idempotent` makes the script safe to re-run; it skips migrations whose rows are already in `__EFMigrationsHistory`. The file `migrate.sql` is the artifact you will apply.

### 3. Review the script and flag risk

Read `migrate.sql`. Surface to the user:

- A short summary: how many migrations, how many `CREATE TABLE` / `ALTER TABLE` / `DROP` statements.
- **Every** `DROP TABLE`, `DROP COLUMN`, `ALTER COLUMN`, or rename. These are destructive and demand extra scrutiny on a populated production DB.
- Any `NOT NULL` column added to an existing table without a default — would fail on a non-empty table.
- Any unique index added to an existing table — would fail if existing rows violate uniqueness.

If anything in that list appears, **do not** proceed to step 5 without the user explicitly acknowledging the risk.

### 4. Wait for explicit confirmation

Stop and ask the user, verbatim:

> Ready to apply `migrate.sql` to the Azure SQL production database. Reply `APPLY` to proceed. Any other reply will abort.

Do not proceed unless the user replies with exactly `APPLY` (case-insensitive is fine). "yes", "go ahead", "ok" are **not** sufficient — the skill requires the literal word so the user has to read this message.

### 5. Apply the script

```powershell
dotnet ef database update --project FishingMap.Data --startup-project FishingMap.API --connection $env:FISHINGMAP_PROD_CONN
```

Using `database update` rather than executing `migrate.sql` via `sqlcmd` because `dotnet ef` is already allowlisted in `.claude/settings.local.json` and handles `__EFMigrationsHistory` correctly. If the apply fails partway, do not retry blindly — report the error and let the user inspect the DB state.

### 6. Verify

```powershell
dotnet ef migrations list --project FishingMap.Data --startup-project FishingMap.API --connection $env:FISHINGMAP_PROD_CONN
```

Every migration in the branch should now be marked `(Applied)`. Report the final list to the user.

### 7. Remind about the deploy

After a successful apply, remind the user:

> Schema is updated. The new app code is not yet running — merge `<current-branch>` to `master` to trigger the GitHub Actions deploy. The current production app continues running against the (now-expanded) schema until then.

This is the safe ordering: expand schema first, then deploy code. Only valid because additive migrations are backwards-compatible with the still-running old code. If step 3 surfaced destructive changes, this assumption breaks — flag it.

## What not to do

- Do **not** read or print the connection string. Refer to it as `$env:FISHINGMAP_PROD_CONN` only.
- Do **not** commit `migrate.sql` — it can be regenerated and may contain schema details that don't belong in git. Add it to `.gitignore` if it isn't already; otherwise just leave it untracked.
- Do **not** call `EnsureCreated()` or `Database.Migrate()` from anywhere — `DbInitializer` already calls `EnsureCreated()` at app startup, which is a no-op on the existing prod DB. Migration deploys go through this skill, not through app startup.
- Do **not** run `Down()` migrations against prod to "roll back". If the deploy needs to be reverted, revert the code; leave the (additive) schema in place and fix forward.
