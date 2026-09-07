# Inventario

Sistema de inventario **multi-partner** para refaccionarias. Cliente web
(Angular), app móvil (Ionic) y API (.NET 10) sobre SQL Server.

Estado: **Hito 0 — Fundaciones**. El repositorio es un esqueleto que compila;
todavía sin funcionalidad de negocio. Ver [`docs/roadmap.md`](docs/roadmap.md).

## Documentación

- [`docs/PRD.md`](docs/PRD.md) — qué se construye y por qué.
- [`docs/roadmap.md`](docs/roadmap.md) — hitos del MVP1.
- [`docs/decisions/`](docs/decisions/) — decisiones técnicas (ADRs).
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
# Swagger en https://localhost:<puerto>/swagger
```

### 3. Frontend

```bash
cd frontend
pnpm install
pnpm nx serve web
```

## Pruebas

```bash
# Backend
cd backend && dotnet test Inventario.slnx

# Frontend
cd frontend && pnpm nx run-many -t lint test build
```

## Estructura

```
frontend/          workspace Nx (apps/web, apps/mobile, libs/)
backend/           solución .NET 10 (Domain / Application / Infrastructure / Api)
infra/             docker-compose (local) + Bicep (nube)
docs/              PRD, roadmap, ADRs
azure-pipelines/   CI/CD para Azure DevOps
```
