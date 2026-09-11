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
- **Hito 4 — Movimientos**: entrada (recalcula el promedio ponderado), salida,
  merma; folios consecutivos por partner; kardex por artículo con existencia y
  costo resultantes por renglón. Pantalla de movimientos (alta multi-renglón) y
  kardex embebido en el detalle de existencias.
- **Hito 5 — Transferencias**: entre sucursales con estados solicitada → en
  tránsito → recibida; el envío descuenta el origen al costo promedio de origen,
  la recepción abona el destino por la cantidad recibida (recepción parcial con
  diferencia visible); cancelación con devolución al origen. Pantalla de
  transferencias.
- **Hito 6 — Conteos físicos**: conteo por sucursal (opcional por categoría) que
  fotografía las existencias; captura incremental; conciliación que ajusta a lo
  contado (conservando el costo) y genera un único movimiento de ajuste;
  cancelación. Un solo conteo en progreso por sucursal. Pantalla de conteos.
- **Fase D — Diseño (base)**: sistema visual "limpio y profesional" en código —
  tokens CSS (paleta, tipografía, espaciado, sombras), componentes base
  (`.btn`, `.tabla`, `.panel`, `.campo`, `.badge`, `.tarjeta`), barra lateral
  oscura con navegación agrupada e iconos, layout responsivo y rediseño del
  login. Ajuste fino desde Figma queda pendiente.
- **Hito 8 — Reportes**: reportes de existencias, valorización, kardex y
  diferencias de conteo, exportables a Excel (ClosedXML) y PDF (QuestPDF
  Community). Pantalla `/reportes` con filtros y descarga autenticada (blob +
  object URL). Los datos ya venían de los hitos 3/4/6.
- **Fase D — Landing pública y rediseño de login**: nueva página pública en
  `/` (hero, propuesta de valor, características, módulos, "cómo está
  construido" y CTA a login), inspirada en patrones de landings SaaS pero con
  contenido propio y sin cifras/testimonios inventados. El área autenticada
  se movió de `/` a `/app` (rutas internas sin cambios relativos). Login
  rediseñado a dos columnas (panel de marca + formulario). Tokens de diseño
  (`:root`) extraídos de `apps/web` a `libs/shared/ui` para reutilizarse en
  el Hito 7 (móvil); nuevo acento dorado (`--c-accent`) usado con moderación
  en CTAs de landing y el enlace activo del sidebar.
