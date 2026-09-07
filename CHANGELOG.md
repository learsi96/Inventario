# Changelog

Todos los cambios notables de este proyecto se documentan en este archivo.

El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/)
y el proyecto se adhiere a [Versionado Semántico](https://semver.org/lang/es/).

## [Unreleased]

### Added
- Andamiaje inicial del proyecto (Fase 1): repositorio, estructura de carpetas,
  workspace Nx (`frontend/`), solución .NET 10 en capas (`backend/`), infra local
  con Docker Compose, pipelines de CI para Azure DevOps, tooling y documentación
  viva (PRD, roadmap, ADRs, CLAUDE.md).
- **Hito 1 — Identidad y multi-tenant**: autenticación JWT (login, `/me`,
  superadmin), roles y políticas de autorización, filtro global por `TenantId` en
  EF Core con asignación automática, alta de partners, y CRUD de sucursales
  (backend + pantallas web con login, guard, interceptor y shell). Primera
  migración de EF Core (`Inicial`). CORS para el cliente web. Verificado
  end-to-end contra SQL Server 2022 en Docker: login, `/me`, CRUD de sucursales
  y aislamiento de datos entre partners.
