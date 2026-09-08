# Roadmap — MVP1

Orden de construcción del MVP1. Cada hito entrega algo demostrable al cliente.
Alcance detallado y fuera de alcance: ver [PRD.md](PRD.md).

| Hito | Entrega | Estado |
|---|---|---|
| **0 — Fundaciones** | Andamiaje: repo, Nx (`frontend/`: apps `web` y `mobile` + 6 libs), solución .NET 10 (`backend/`), docker-compose, pipelines CI, tooling, docs. Todo compila y las pruebas pasan; sin funcionalidad de negocio. La app `mobile` es Angular puro; Ionic + Capacitor se añaden en el Hito 7. | Hecho |
| **1 — Identidad y multi-tenant** | Autenticación JWT, usuarios, roles, permiso por sucursal, alta de partners, `TenantId` + global query filter operativos. Entidad `Sucursal` con CRUD end-to-end en web. Primera migración de EF Core. | Hecho. Verificado end-to-end contra SQL Server 2022 (Docker): login, `/me`, CRUD de sucursales, aislamiento entre partners y CORS del cliente web. Falta la pantalla de gestión de usuarios (se hará junto al Hito 2). |
| **D — Diseño** | Pase de diseño (sistema visual: paleta, tipografía, layout, componentes base) y maquetado de las pantallas del MVP1. Retoque de las pantallas ya construidas. Herramienta por decidir (lienzo de diseño / Figma). | Pendiente — **se hace más adelante, no ahora** (acordado con el autor) |
| **2 — Catálogo** | Categorías jerárquicas (con categoría "Otros" de sistema por partner), unidades de medida, artículos con SKU autogenerado, código de barras, códigos alternos, IVA por artículo y foto (`IAlmacenArchivos` local en dev; Azure Blob en el Hito 9). Sin importación masiva (MVP2). | Hecho. Verificado contra SQL: categorías (árbol, ciclos, "Otros"), artículos (SKU secuencial, búsqueda por marca/OEM/código alterno, filtros, paginación), imagen (subir/servir/borrar). |
| **U — Gestión de usuarios** | CRUD de usuarios del partner: rol, sucursales asignadas, contraseña inicial, reset, activar/desactivar. Pantalla solo para Administrador. Cierra lo que quedó del Hito 1. | Hecho. Verificado contra SQL: alta con sucursales, login del nuevo usuario, política 403 para no-admin, bloqueo de auto-desactivación, reset de contraseña, login bloqueado al desactivar. |
| **3 — Ubicaciones y existencias** | Ubicaciones por sucursal; existencia por artículo/sucursal (+ desglose por ubicación); mínimos, máximos y punto de reorden; costeo promedio ponderado (motor compartido); valorización; alerta de bajo mínimo; ajuste/carga inicial (genera movimiento). Ver [ADR 0006](decisions/0006-modelo-de-existencias.md). | Hecho. Verificado contra SQL: ubicaciones, ajuste, parámetros, valorización, bajo mínimo, desglose por ubicación. |
| **4 — Movimientos** | Entrada (recalcula promedio ponderado), salida, merma, ajuste. Folios consecutivos. Kardex por artículo con existencia y costo resultantes. Pantalla de movimientos (alta multi-renglón) y kardex en el detalle de existencias. | Hecho. Verificado contra SQL: entrada promedia (5@95 + 10@120 → 111.67), salida usa el promedio, merma insuficiente rechazada, kardex correcto. |
| **5 — Transferencias** | Entre sucursales con estados solicitada → en tránsito → recibida; costo de origen; recepción parcial (diferencias visibles enviado/recibido); cancelación con devolución al origen. Pantalla de transferencias. | Hecho. Verificado contra SQL: envío descuenta origen, recepción de 3 de 4 (1 perdida), cancelación en tránsito devuelve. |
| **6 — Conteos físicos** | Conteo por sucursal (opcionalmente por categoría); fotografía las existencias al iniciar; captura incremental; conciliación que ajusta a lo contado y genera el movimiento; cancelación. Un solo conteo en progreso por sucursal. Pantalla de conteos con captura y diferencias. | Hecho. Verificado contra SQL: iniciar/capturar/conciliar ajusta la existencia (10→7), sin diferencias no genera movimiento, doble conteo → 409. |
| **7 — App móvil** | Añadir Ionic + Capacitor a `apps/mobile`. Consulta de existencias, escaneo de código de barras, transferencias y conteos desde el piso. Solo online. | Pendiente |
| **8 — Reportes** | Existencias, valorización, kardex y diferencias de conteo, exportables a Excel (ClosedXML) y PDF (QuestPDF Community). Pantalla /reportes con filtros y descarga. Los datos ya venían de los hitos 3/4/6. | Hecho. Verificado contra SQL: los 4 reportes generan xlsx y pdf válidos. |
| **9 — Despliegue** | Bicep del entorno de pruebas, pipelines de CD, migración local → Azure, validación con el cliente. | Pendiente |

## Después del MVP1

Ver la sección "Fuera de alcance del MVP1" del PRD: **importación masiva de
catálogo (Excel/CSV)**, compras, ventas/ticket, modo offline en móvil,
equivalencias y aplicaciones por vehículo, listas de precios múltiples,
catálogo externo de refacciones por código de barras, onboarding self-service
de partners, Entra External ID, reportes avanzados y dashboards.
