# apps/api

ASP.NET Core (.NET 10) Web API + PostgreSQL 18. Backend for `apps/blog` and
`apps/admin`. Currently implements only what `apps/admin`'s Phase 1 needs — auth,
posts, media — matching its MSW mock contract exactly (see `Contracts/Dtos.cs`
and the endpoint files for the mapping notes). Categories, Author Applications,
Users & Roles, and Statistics exist as DB schema only — no endpoints yet — per
the phased build already underway on the frontend
(`docs/superpowers/specs/2026-07-13-admin-panel-phase1-design.md`).

## Run it (recommended: Docker Compose)

From the repo root — **first time only**, generate the initial migration (no
`Migrations/` folder exists yet in this repo):

```bash
docker compose -f docker-compose.dev.yml up postgres -d
docker compose -f docker-compose.dev.yml run --rm api dotnet ef migrations add InitialCreate
```

This runs inside the container, so it doesn't need the .NET SDK installed on
your machine — just Docker. It'll write the generated migration files back
into `apps/api/Migrations/` on your host (bind-mounted), which you should
commit.

Then, every time after (including this first time):

```bash
docker compose -f docker-compose.dev.yml up
```

API on `http://localhost:5080`, Postgres on `localhost:5432`. Startup applies
any pending EF Core migrations and seeds three demo accounts automatically
(dev-only, see `Data/DbSeeder.cs`):

| Email | Password | Role |
|---|---|---|
| `author@dd.local` | `password` | Author |
| `editor@dd.local` | `password` | Editor |
| `owner@dd.local` | `password` | Owner |

Same accounts as `apps/admin/mocks/fixtures/users.ts` — switching the admin
panel from mocks to this real API doesn't change who you log in as.

## Run it without Docker

Requires the .NET 10 SDK and a reachable Postgres 18 instance.

```bash
cd apps/api
dotnet restore
dotnet ef database update   # needs dotnet-ef: dotnet tool install --global dotnet-ef
dotnet watch run
```

If Postgres is running via `docker compose -f docker-compose.dev.yml up postgres`
only, update `ConnectionStrings:Default` in `appsettings.Development.json` (or an
untracked `appsettings.Local.json`) to `Host=localhost` instead of `Host=postgres`
— `postgres` as a hostname only resolves inside the Compose network.

## Connecting apps/admin to this instead of MSW

`apps/admin`'s Vite config already proxies `/api` to `http://localhost:5080` (see
`vite.config.ts`). Set `VITE_ENABLE_MOCKS=false` in an untracked
`.env.development.local` (**not** plain `.env.local` — Vite's env-file
precedence is `.env` < `.env.local` < `.env.[mode]` < `.env.[mode].local`,
so a mode-specific `.env.development` beats a mode-agnostic `.env.local` for
the same key; verified against `loadEnv`'s actual file order in
`node_modules/vite/dist/node/chunks/node.js`) to stop MSW intercepting
requests and hit this API for real. Requires a dev server restart — Vite
only reads env files at startup, not on file change.

## What's not built yet

- Migrations haven't been generated (no `Migrations/` folder yet — run
  `dotnet ef migrations add InitialCreate` once you've reviewed the entity
  shapes in `Models/`).
- No `dotnet build`/`dotnet test` has been run against this code — it was
  authored without a .NET SDK available in the scaffolding environment. Treat
  it as a strong first draft: build it, fix whatever the compiler flags, and
  add a migration before relying on it.
- Categories, Author Applications, Users & Roles, and Statistics endpoints —
  schema exists, routes don't yet.
- Real file storage for media uploads (currently stores whatever data URL the
  client sends, matching the mock — fine for dev, not for production; see the
  TODO on `Models/MediaAsset.cs`).
