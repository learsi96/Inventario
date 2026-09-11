# CLAUDE.md

Guía para trabajar en este repositorio con Claude Code.

## Qué es

Sistema de inventario **multi-partner (multi-tenant)** para refaccionarias.
Cliente web (Angular) + app móvil (Ionic, pendiente) + API (.NET 10) sobre SQL Server.
Se construye y valida con un primer cliente, pero es configurable por partner
desde el inicio.

Documento maestro: **[docs/PRD.md](docs/PRD.md)**. Plan y estado por hito:
**[docs/roadmap.md](docs/roadmap.md)**. Decisiones técnicas (ADRs):
**[docs/decisions/](docs/decisions/)**. Historial: **[CHANGELOG.md](CHANGELOG.md)**.

## Estado (2026-09-11)

**Backend del MVP1 funcionalmente completo.** Hitos **0, 1, 2, U, 3, 4, 5, 6, 8**
y la **Fase D** (base de diseño + landing pública + rediseño de login) están en
`main`. 36 pruebas backend + 8 proyectos frontend en verde. Landing pública en
`/`, login de dos columnas, área autenticada en `/app`; tokens de diseño
compartidos en `libs/shared/ui` para el futuro móvil. Flujo login → shell
verificado en vivo contra Docker + backend + frontend reales.

Pendiente: **ajuste de diseño desde Figma**, **Hito 7 (app móvil)**,
**Hito 9 (despliegue a Azure)**, e importación masiva de catálogo (MVP2).

## Estructura

| Carpeta | Contenido |
|---|---|
| `frontend/` | Workspace Nx (pnpm). `apps/web` (Angular), `apps/mobile` (Angular; Ionic + Capacitor van en el Hito 7), `libs/shared/{domain,util,ui,api-client,auth}` + `libs/inventario/data-access` |
| `backend/` | Solución .NET 10 (`Inventario.slnx`). Clean Architecture: `Domain` → `Application` → `Infrastructure` → `Api` |
| `infra/` | `docker-compose.yml` (SQL Server 2022 local); `bicep/` (Azure, Hito 9) |
| `docs/` | PRD, roadmap, 7 ADRs |
| `azure-pipelines/` | YAML de CI/CD para Azure DevOps (sin vincular todavía) |

### Módulos del backend (`Inventario.Application/`)

| Módulo | Servicios clave |
|---|---|
| `Identidad` | `AutenticacionService` (login/`/me`/superadmin), `UsuariosService` |
| `Partners` | `PartnersService` (alta de tenant + admin) |
| `Sucursales` | `SucursalesService` |
| `Catalogo` | `CategoriasService`, `UnidadesMedidaService`, `ArticulosService` |
| `Existencias` | `UbicacionesService`, `ExistenciasService` |
| `Movimientos` | `MotorExistencias` (promedio ponderado — **núcleo**), `FoliosService`, `MovimientosService` (entrada/salida/merma/kardex), `TransferenciasService` |
| `Conteos` | `ConteosService` |
| `Reportes` | `ReportesService` + `IGeneradorReporte` (ClosedXML xlsx / QuestPDF pdf) |

Endpoints en `Inventario.Api/Endpoints/*.cs`, todos minimal API con grupos.
Políticas de autorización: `Superadmin`, `AdminPartner`, `OperadorAlmacen`
(Admin + EncargadoAlmacen), `UsuarioPartner` (`Inventario.Api/Seguridad/Autorizacion.cs`).

### Pantallas web (`apps/web/src/app/paginas/`)

`login`, `shell` (layout), y: `articulos`, `categorias`, `existencias`,
`movimientos`, `transferencias`, `conteos`, `ubicaciones`, `sucursales`,
`usuarios` (solo Admin), `reportes`. Servicios HTTP en `libs/inventario/data-access`.

## Entorno de desarrollo

**El entorno ya está montado.** SQL Server corre en el contenedor Docker
`inventario-sqlserver`; la BD `InventarioDev` tiene las migraciones aplicadas
(`EsquemaInicial` + `Conteos`) y el seed demo.

```bash
# Base de datos (si el contenedor no está arriba)
docker compose -f infra/docker-compose.yml up -d
# Docker Desktop está instalado por-usuario: si `docker` no está en PATH, usar
#   /c/Users/jicm_/AppData/Local/Programs/DockerDesktop/resources/bin/docker.exe

# Backend
cd backend
dotnet build Inventario.slnx
dotnet test Inventario.slnx
dotnet run --project src/Inventario.Api           # http://localhost:5027/swagger
dotnet ef migrations add <Nombre> --project src/Inventario.Infrastructure --startup-project src/Inventario.Api
dotnet ef database update      --project src/Inventario.Infrastructure --startup-project src/Inventario.Api
dotnet format Inventario.slnx

# Frontend
cd frontend
pnpm install
NX_DAEMON=false pnpm nx serve web                 # http://localhost:4200
NX_DAEMON=false pnpm nx run-many -t lint test build typecheck --exclude=web-e2e,mobile-e2e
```

Notas de tooling: el daemon de Nx cuelga en la primera ejecución → usar
`NX_DAEMON=false`. `git push` a veces cuelga si se encadena; reintentar solo.
SQLite (tests) no soporta `ORDER BY DateTimeOffset` → los servicios ordenan por
`Folio`. Ver [`~/.claude/.../memory/nx23-tooling-inventario.md`].

### Datos de desarrollo (seed)

Al arrancar la API en `Development` se crea, si no existe, un partner demo:
- **Admin del partner:** `admin@demo.com` / `Demo1234!` (tenant `DEMO`, sucursal `MATRIZ`, categoría `Otros`)
- **Superadmin de plataforma** (config, sin BD): `superadmin@inventario.local` / `Super1234!`

## Convenciones

- **Idioma:** el dominio se nombra en **español** (`Sucursal`, `Movimiento`,
  `Existencia`). El código de plataforma (infra, DI, helpers) puede ir en inglés.
- **Un hito = una rama** `feat/hitoN-<slug>`, commits Conventional Commits,
  merge `--no-ff` a `main`, `git push`. Verificar `dotnet build/test` +
  `nx run-many` antes de commitear.
- **.NET:** nullable + warnings como errores + analizadores activos
  (`backend/Directory.Build.props`). Paquetes centralizados en
  `backend/Directory.Packages.props` (Central Package Management).
- **Migraciones EF:** el código generado lleva su propio `.editorconfig` que
  desactiva analizadores.
- **Frontend:** tareas vía `pnpm nx ...`; respetar límites de librería (tags
  `scope:*` / `type:*`); alias `@inventario/<lib>`. Prettier reformatea el HTML
  de Angular de forma agresiva (aplana `@if`/`@for`) — es normal.
- **Pruebas de servicios:** xUnit + Shouldly + SQLite en memoria, con un
  `CurrentUserFake` por test (patrón repetido en `tests/`).

## Regla dura: multi-tenant

Toda entidad de negocio implementa `Inventario.Domain.Tenancy.ITenantEntity`
(`Guid TenantId`). El `AppDbContext` filtra por tenant automáticamente (global
query filter) y asigna el `TenantId` en `SaveChanges`. **Cualquier consulta con
Dapper debe filtrar por `TenantId` explícitamente.**
Ver [docs/decisions/0002](docs/decisions/0002-estrategia-multi-tenant.md) y
[0006](docs/decisions/0006-modelo-de-existencias.md).

## Fuera de alcance del MVP1

Importación masiva de catálogo (Excel/CSV), compras, ventas/ticket, modo offline
móvil, equivalencias/cross-reference, aplicaciones por vehículo, listas de precios
múltiples, catálogo externo por código de barras, onboarding self-service,
costeo PEPS. No implementar sin actualizar antes el PRD y el roadmap.
