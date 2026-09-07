# CLAUDE.md

Guía para trabajar en este repositorio con Claude Code.

## Qué es

Sistema de inventario **multi-partner (multi-tenant)** para refaccionarias.
Cliente web (Angular) + app móvil (Ionic) + API (.NET 10). Se construye y valida
con un primer cliente, pero es configurable por partner desde el inicio.

Documento maestro: **[docs/PRD.md](docs/PRD.md)**. Plan de construcción:
**[docs/roadmap.md](docs/roadmap.md)**. Decisiones técnicas:
**[docs/decisions/](docs/decisions/)**.

Estado actual: **Hito 0 (Fundaciones)** — esqueleto sin funcionalidad de negocio.

## Estructura

| Carpeta | Contenido |
|---|---|
| `frontend/` | Workspace Nx (pnpm). `apps/web` (Angular), `apps/mobile` (Angular; Ionic + Capacitor se añaden en el Hito 7), `libs/shared/*` + `libs/inventario/*` |
| `backend/` | Solución .NET 10. Clean Architecture: `Domain` → `Application` → `Infrastructure` → `Api` |
| `infra/` | `docker-compose.yml` (SQL Server local); `bicep/` (Azure, Hito 9) |
| `docs/` | PRD, roadmap, ADRs |
| `azure-pipelines/` | YAML de CI/CD para Azure DevOps |

## Comandos

### Backend (`backend/`)
```bash
dotnet build Inventario.slnx
dotnet test Inventario.slnx
dotnet run --project src/Inventario.Api            # https://localhost:xxxx/swagger
dotnet ef migrations add <Nombre> --project src/Inventario.Infrastructure --startup-project src/Inventario.Api
dotnet ef database update  --project src/Inventario.Infrastructure --startup-project src/Inventario.Api
dotnet format Inventario.slnx
```

### Frontend (`frontend/`)
```bash
pnpm install
pnpm nx serve web                                   # http://localhost:4200
pnpm nx run-many -t lint test build typecheck
pnpm nx graph
```
Al crear una librería nueva, pásale `--tags` (`scope:*`, `type:*`) o el lint de
límites de módulo falla. Alias de import: `@inventario/<nombre-lib>`.

### Base de datos local
```bash
cp infra/.env.example infra/.env
docker compose -f infra/docker-compose.yml up -d
# Docker Desktop está instalado por-usuario: si `docker` no está en PATH, usar
# C:\Users\<user>\AppData\Local\Programs\DockerDesktop\resources\bin\docker.exe
```

### Datos de desarrollo (seed)
Al arrancar la API en `Development` se crea, si no existe, un partner demo:
- **Admin del partner:** `admin@demo.com` / `Demo1234!` (tenant `DEMO`, sucursal `MATRIZ`)
- **Superadmin de plataforma** (config, sin BD): `superadmin@inventario.local` / `Super1234!`

## Convenciones

- **Idioma:** el dominio se nombra en **español** (`Sucursal`, `Movimiento`,
  `Existencia`). El código de plataforma (infra, DI, helpers) puede ir en inglés.
- **Commits:** Conventional Commits (`feat:`, `fix:`, `chore:`, `docs:`…).
- **Ramas:** `feat/<hito>-<slug>`, `fix/<slug>`. PR contra `main`.
- **.NET:** nullable + warnings como errores + analizadores están activos
  (`backend/Directory.Build.props`). Versiones de paquetes centralizadas en
  `backend/Directory.Packages.props`.
- **Nx:** ejecutar tareas siempre vía `pnpm nx ...`, no la herramienta por debajo.
  Respetar los límites entre librerías (tags `scope:*` / `type:*`).

## Regla dura: multi-tenant

Toda entidad de negocio implementa `Inventario.Domain.Tenancy.ITenantEntity`
(`Guid TenantId`). El `AppDbContext` filtra por tenant automáticamente vía global
query filter. **Cualquier consulta con Dapper debe filtrar por `TenantId`
explícitamente.** Ver [docs/decisions/0002](docs/decisions/0002-estrategia-multi-tenant.md).

## Fuera de alcance del MVP1

Compras, ventas/ticket, modo offline móvil, equivalencias/cross-reference,
aplicaciones por vehículo, listas de precios múltiples, onboarding self-service.
No implementar sin actualizar antes el PRD y el roadmap.
