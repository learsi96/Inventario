# Inventario

Sistema de inventario **multi-partner** para refaccionarias. Cliente web
(Angular), app móvil (Ionic, pendiente) y API (.NET 10) sobre SQL Server.

**Estado:** backend del MVP1 funcionalmente completo — hitos 0, 1, 2, U, 3, 4, 5,
6, 8 y base de diseño. Pendiente: ajuste de diseño desde Figma, app móvil
(Hito 7) y despliegue a Azure (Hito 9). Ver [`docs/roadmap.md`](docs/roadmap.md).

## Documentación

- [`docs/PRD.md`](docs/PRD.md) — qué se construye y por qué.
- [`docs/roadmap.md`](docs/roadmap.md) — hitos del MVP1 y su estado.
- [`docs/decisions/`](docs/decisions/) — decisiones técnicas (ADRs).
- [`CHANGELOG.md`](CHANGELOG.md) — historial de cambios.
- [`CLAUDE.md`](CLAUDE.md) — guía de trabajo en el repo.

## Requisitos

| Herramienta | Versión | Para |
|---|---|---|
| .NET SDK | 10.x | Backend |
| Node.js | 22.x (ver `.nvmrc`) | Frontend |
| pnpm | 10+ | Frontend |
| Docker Desktop | reciente | SQL Server local |
| Azure CLI | — | Despliegue (Hito 9) |

## Puesta en marcha

### 1. Base de datos local

```bash
cp infra/.env.example infra/.env
docker compose -f infra/docker-compose.yml up -d
```

### 2. Backend

```bash
cd backend
dotnet build Inventario.slnx
dotnet ef database update --project src/Inventario.Infrastructure --startup-project src/Inventario.Api
dotnet run --project src/Inventario.Api
# Swagger en http://localhost:5027/swagger
```

Al arrancar en `Development` se crea el partner demo:
`admin@demo.com` / `Demo1234!`.

### 3. Frontend

```bash
cd frontend
pnpm install
pnpm nx serve web       # http://localhost:4200
```

## Pruebas

```bash
cd backend && dotnet test Inventario.slnx
cd frontend && pnpm nx run-many -t lint test build typecheck --exclude=web-e2e,mobile-e2e
```

## Estructura

```
frontend/          workspace Nx (apps/web, apps/mobile, libs/)
backend/           solución .NET 10 (Domain / Application / Infrastructure / Api)
infra/             docker-compose (local) + Bicep (nube, Hito 9)
docs/              PRD, roadmap, ADRs
azure-pipelines/   CI/CD para Azure DevOps
```
