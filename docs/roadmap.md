# Roadmap — MVP1

Orden de construcción del MVP1. Cada hito entrega algo demostrable al cliente.
Alcance detallado y fuera de alcance: ver [PRD.md](PRD.md). Historial: [CHANGELOG.md](../CHANGELOG.md).

## Estado global (2026-09-08)

**Backend del MVP1 funcionalmente completo.** Hecho: **0, 1, 2, U, 3, 4, 5, 6, 8**
y la **base de diseño (D)**. 36 pruebas backend + 8 proyectos frontend en verde,
todo en `main` (GitHub `learsi96/Inventario`).

**Próximos pasos (para la siguiente sesión):**
1. **Ajuste de diseño desde Figma** (segunda parte de la Fase D) — cuando el autor
   tenga el diseño en Figma y se configure el MCP (requiere plan Dev/Full).
2. **Hito 7 — App móvil** (Ionic + Capacitor sobre `apps/mobile`, escaneo).
3. **Hito 9 — Despliegue** a Azure (Bicep, pipelines de CD, migrar la BD).

Entorno listo: contenedor `inventario-sqlserver` con la BD `InventarioDev`
migrada y seed demo; ver [CLAUDE.md](../CLAUDE.md).

## Hitos

| Hito | Entrega | Estado |
|---|---|---|
| **0 — Fundaciones** | Repo, Nx (`frontend/`), solución .NET 10 (`backend/`), docker-compose, pipelines CI, tooling, docs. | ✅ Hecho |
| **1 — Identidad y multi-tenant** | JWT (login / `/me` / superadmin), roles y políticas, `TenantId` + global query filter, alta de partners, CRUD de sucursales. Migración `Inicial` (luego consolidada en `EsquemaInicial`). CORS. | ✅ Verificado end-to-end |
| **2 — Catálogo** | Categorías jerárquicas (con "Otros" de sistema), unidades de medida (catálogo global), artículos (SKU autogenerado, código de barras único, códigos alternos, IVA por artículo, foto vía `IAlmacenArchivos`). Listado paginado + búsqueda + filtros. Sin importación masiva (→ MVP2). | ✅ Verificado |
| **U — Gestión de usuarios** | CRUD de usuarios del partner (rol, sucursales, contraseña inicial, reset, activar/desactivar); protecciones anti auto-bloqueo. Pantalla solo Admin. | ✅ Verificado |
| **3 — Ubicaciones y existencias** | Ubicaciones por sucursal; existencia (grano artículo×sucursal) + desglose por ubicación; mín/máx/reorden; **costeo promedio ponderado** (`MotorExistencias`); valorización; alerta de bajo mínimo; ajuste/carga inicial. [ADR 0006](decisions/0006-modelo-de-existencias.md). | ✅ Verificado |
| **4 — Movimientos** | Entrada (recalcula promedio), salida, merma. Folios consecutivos. **Kardex** por artículo con existencia y costo resultantes. Pantalla de movimientos + kardex embebido en existencias. | ✅ Verificado |
| **5 — Transferencias** | Entre sucursales: solicitada → en tránsito → recibida; costo de origen; recepción parcial (diferencia visible); cancelación con devolución. Pantalla de transferencias. | ✅ Verificado |
| **6 — Conteos físicos** | Conteo por sucursal (opcional por categoría); fotografía las existencias; captura incremental; conciliación que ajusta y genera movimiento; cancelación. Un solo conteo en progreso por sucursal. Pantalla de conteos. | ✅ Verificado |
| **8 — Reportes** | Existencias, valorización, kardex y diferencias de conteo → Excel (ClosedXML) y PDF (QuestPDF Community). Pantalla `/reportes` con filtros y descarga autenticada. | ✅ Verificado |
| **D — Diseño** | **Base:** sistema visual "limpio y profesional" en código (tokens, componentes `.btn/.tabla/.panel/.campo/.badge/.tarjeta`, barra lateral oscura agrupada). **Landing + login:** página pública en `/` (hero, características, módulos, CTA) y login rediseñado a dos columnas; área autenticada movida a `/app`; tokens compartidos vía `libs/shared/ui`. **Falta:** ajuste fino desde Figma (el autor lo hará más adelante). | 🟡 Base + landing hechas |
| **7 — App móvil** | Añadir Ionic + Capacitor a `apps/mobile`. Consulta de existencias, escaneo de código de barras, transferencias y conteos desde el piso. Solo online. | ⬜ Pendiente (el autor lo dejó para el final) |
| **9 — Despliegue** | Bicep del entorno de pruebas, pipelines de CD, migración local → Azure, validación con el cliente. | ⬜ Pendiente |

## Después del MVP1

Ver "Fuera de alcance del MVP1" en el PRD: **importación masiva de catálogo
(Excel/CSV)**, compras, ventas/ticket, modo offline en móvil, equivalencias y
aplicaciones por vehículo, listas de precios múltiples, catálogo externo por
código de barras, onboarding self-service de partners, Entra External ID,
reportes avanzados y dashboards, costeo PEPS.
