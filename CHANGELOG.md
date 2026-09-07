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
- **Hito 2 — Catálogo**: categorías jerárquicas (con "Otros" de sistema por
  partner), catálogo global de unidades de medida, artículos (SKU autogenerado
  editable, código de barras único, códigos alternos, categoría obligatoria, IVA
  por artículo) con listado paginado + búsqueda + filtros, e imagen por artículo
  (`IAlmacenArchivos`, local en dev). Pantallas web de artículos y categorías;
  artículos pasa a ser la pantalla inicial. Enums JSON como texto.
- **Hito U — Gestión de usuarios**: CRUD de usuarios del partner (rol, sucursales
  asignadas, contraseña inicial, reset, activar/desactivar), con protecciones
  contra que el administrador se bloquee a sí mismo. Pantalla web solo para
  Administrador (enlace de nav condicional).
- **Hito 3 — Ubicaciones y existencias**: ubicaciones por sucursal; modelo de
  existencia con grano (artículo, sucursal), costo promedio ponderado mantenido
  por un motor compartido (`MotorExistencias`), parámetros de reorden,
  desglose por ubicación, valorización y alerta de bajo mínimo. Ajuste/carga
  inicial que genera un movimiento de inventario (`Movimiento`/`MovimientoRenglon`,
  base del kardex). Migraciones consolidadas en `EsquemaInicial`. Pantallas web
  de existencias (con valorización) y ubicaciones. ADR 0006.
