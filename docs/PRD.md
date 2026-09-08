# PRD — Sistema de Inventario Multi-Partner

- **Estado:** Vigente. MVP1 en construcción — backend funcionalmente completo
  (hitos 0–8 salvo el 7 móvil y el 9 despliegue). Ver [roadmap.md](roadmap.md).
- **Fecha:** 2026-09-07 (revisado 2026-09-08)
- **Autor:** israelcm250@gmail.com
- **Proyecto:** personal

---

## 1. Visión

Sistema de inventario para refaccionarias, diseñado desde el inicio como
**multi-partner** (multi-tenant): una sola instalación da servicio a varias
empresas cliente, cada una con su configuración, sus sucursales, sus usuarios y
sus datos aislados.

El primer cliente es una refaccionaria que hoy no tiene ningún sistema de
inventariado. El MVP1 se construye y valida con ese cliente, pero la
arquitectura ya contempla la incorporación de más partners.

## 2. Objetivo del MVP1

Que el cliente pueda:

1. Cargar su catálogo de refacciones (importación masiva incluida).
2. Llevar existencias por sucursal y por ubicación (anaquel).
3. Registrar entradas, salidas, ajustes y **transferencias entre sucursales**.
4. Hacer conteos físicos y conciliar diferencias.
5. Consultar y operar desde el navegador y desde una app móvil (con escaneo).
6. Sacar reportes de existencias, valorización, kardex y bajo mínimo.

Criterio de cierre del MVP1: el cliente opera su inventario real durante 2–4
semanas sin volver a su método anterior.

## 3. Usuarios y roles

| Rol | Puede |
|---|---|
| Administrador | Todo dentro de su partner: configuración, usuarios, catálogo, todas las sucursales |
| Encargado de almacén | Movimientos, transferencias, conteos y consulta en sus sucursales asignadas |
| Vendedor / Mostrador | Salida de inventario y consulta en su sucursal |
| Solo consulta | Ver existencias, fichas y reportes |

- Cada usuario pertenece a **un** partner.
- Cada usuario tiene una o más sucursales asignadas.
- Un rol de **superadmin** (interno, fuera del partner) da de alta partners.

## 4. Alcance MVP1 (incluido)

### 4.1 Multi-tenant / Partners
- Modelo de datos con `TenantId` en todas las tablas de negocio.
- Configuración por partner: nombre, logo, moneda, IVA %, zona horaria.
- Alta de partners por superadmin (pantalla simple o seed).

### 4.2 Usuarios y seguridad
- Login, logout, recuperación de contraseña.
- Roles (los 4 de arriba) y permiso por sucursal.
- Bitácora de auditoría (quién, qué, cuándo).

### 4.3 Catálogo de artículos
- SKU interno (autogenerado, editable), código de barras (opcional, único),
  nombre, descripción, marca, unidad de medida (catálogo).
- Número de parte OEM y códigos alternos.
- Categorías / familias jerárquicas. La categoría es **obligatoria**; cada partner
  tiene una categoría "Otros" de sistema (no borrable) que recibe los artículos
  sin categoría asignada.
- Foto del artículo (una; almacenamiento abstracto, local en dev / Blob en la nube).
- Costo de referencia y un precio de venta.
- IVA por artículo (porcentaje).
- Estado activo / descontinuado.
- *(La importación masiva desde Excel/CSV pasa a MVP2.)*

### 4.4 Sucursales y ubicaciones
- Sucursales del partner.
- Ubicaciones dentro de la sucursal (pasillo / anaquel / nivel).
- Distinción almacén vs piso de venta como ubicaciones.

### 4.5 Existencias
- Stock por artículo, por sucursal y **por ubicación**.
- Mínimo, máximo y punto de reorden por artículo/sucursal.
- Listado y alerta de artículos bajo mínimo.
- Costeo por **promedio ponderado**.
- Valorización del inventario (existencias × costo).

### 4.6 Movimientos
- Entrada manual con motivo.
- Salida manual con motivo (incluye "salida por venta" sin cobro).
- Ajuste (+/–) con motivo obligatorio.
- Merma / baja.
- **Transferencia entre sucursales** con estados: solicitada → en tránsito → recibida (registrando diferencias en recepción).
- Kardex por artículo (historial completo de movimientos).

### 4.7 Conteos físicos
- Conteo por sucursal o por categoría.
- Captura manual o por escaneo (app móvil).
- Conciliación contra sistema y ajuste automático de diferencias.

### 4.8 Búsqueda y escaneo
- Búsqueda por SKU, código de barras, número de parte o descripción.
- Escaneo con cámara del celular.

### 4.9 Reportes
- Existencias actuales (filtros por sucursal, categoría, bajo mínimo).
- Valorización de inventario.
- Kardex / movimientos por periodo.
- Diferencias de conteo.
- Exportación a Excel y PDF.

### 4.10 App móvil (Ionic + Angular + Capacitor)
- Consulta de existencias y ficha de artículo.
- Escaneo de código de barras.
- Transferencias y recepción desde el piso.
- Conteo físico.
- **Solo modo online en MVP1.**

### 4.11 Plataforma (API .NET)
- API REST versionada con OpenAPI/Swagger.
- Autenticación JWT; autorización por rol y por tenant.
- Migraciones EF Core + datos semilla / demo.
- Paginación, filtros y ordenamiento estándar en listados.
- Manejo de errores uniforme (ProblemDetails).
- Logs estructurados + Application Insights.
- Localización es-MX.
- Carga de imágenes a Blob Storage.

### 4.12 DevOps
- Repositorio Git en Azure Repos.
- Pipeline CI: build + pruebas + lint para frontend y backend.
- Pipeline CD a entorno de pruebas.
- Entornos: `dev` (local) y `staging/pruebas` (nube) en MVP1; `prod` después.
- Secretos en variables de pipeline / Key Vault.
- Migraciones de BD ejecutadas desde el pipeline.
- Infraestructura como código (Bicep) — versión ligera en MVP1.

## 5. Fuera de alcance del MVP1

### MVP2
- **Importación masiva de catálogo** desde Excel/CSV.
- **Compras**: proveedores completos, órdenes de compra, recepción de OC.
- **Ventas**: nota de venta / ticket.
- Modo **offline** en la app móvil (cache local, cola de sincronización, conflictos).
- Equivalencias / cross-reference entre marcas.
- Aplicaciones por vehículo (marca / modelo / año / motor).
- Múltiples listas de precios.
- Reservas / apartados.
- Devoluciones a proveedor / de cliente.
- Conteo cíclico programado.
- Reportes avanzados: rotación, análisis ABC, sin movimiento.
- Dashboard de indicadores.
- Onboarding self-service de partners.
- Autenticación con Entra External ID / AD B2C.
- IaC completa + monitoreo y alertas.

### Futuro
- Punto de venta completo (caja, cortes, formas de pago).
- Facturación CFDI.
- Impresión de etiquetas con código de barras.
- Notificaciones push.
- Planes y límites de uso por partner.
- Ambientes efímeros por PR.
- Costeo PEPS.
- 2FA.

## 6. Requerimientos no funcionales

- **Aislamiento de datos**: ninguna consulta puede devolver datos de otro tenant. Filtro por `TenantId` forzado a nivel de infraestructura (query filter global de EF Core).
- **Rendimiento**: listados con paginación; búsquedas de catálogo < 1 s con ~50k artículos.
- **Disponibilidad MVP1**: no crítica (entorno de pruebas).
- **Idioma**: es-MX.
- **Navegadores**: últimas 2 versiones de Chrome, Edge, Firefox, Safari.
- **Móvil**: Android primero; iOS si el cliente lo requiere.
- **Auditoría**: todo movimiento de inventario queda registrado con usuario y fecha/hora.
- **Backups**: automáticos del servicio de base de datos gestionado.

## 7. Stack y decisiones técnicas

| # | Decisión | Elección |
|---|---|---|
| 1 | Multi-tenancy | BD única compartida con `TenantId` |
| 2 | Hosting MVP1 | Desarrollo **local**; al terminar, migrar a: Angular en Azure Static Web Apps (free), API en Azure App Service F1 (free), datos en Azure SQL Database (free offer). Alternativa a evaluar: SQL Server en contenedor sobre Container Apps |
| 3 | Autenticación | Identity propio con JWT en MVP1; Entra External ID en MVP2 |
| 4 | Repo y CI/CD | Azure DevOps completo (Repos + Pipelines) |
| 5 | Estructura de código | Monorepo: workspace **Nx** para `frontend/` (web + mobile comparten `libs/`), solución .NET en `backend/`, todo en el mismo repo Git |
| 6 | Detalle de ubicaciones | Stock por ubicación (anaquel) **entra en MVP1** |
| 7 | Costeo de inventario | Promedio ponderado (MVP1); PEPS es Futuro |
| 8 | Acceso a datos | Híbrido EF Core (dominio/CRUD/migraciones/filtro de tenant) + Dapper (reportes y lectura pesada) |

### Stack concreto
- **Frontend web**: Angular (última LTS) en workspace Nx.
- **Móvil**: Ionic + Angular + Capacitor, misma workspace Nx.
- **Backend**: .NET (última LTS), API REST.
- **Acceso a datos**: híbrido — **EF Core** para dominio, escrituras, CRUD, migraciones y el *global query filter* por `TenantId`; **Dapper** para reportes y consultas de lectura pesada (kardex, agregaciones). Se documenta en `docs/decisions/0003-acceso-a-datos.md`.
- **Base de datos**: SQL Server (local) → Azure SQL Database (nube).
- **Almacenamiento de imágenes**: Azure Blob Storage.
- **CI/CD**: Azure Pipelines.
- **IaC**: Bicep.
- **Observabilidad**: Application Insights.

### Estructura de repositorio

```
inventario/
  frontend/                    workspace Nx (pnpm)
    apps/web/  apps/web-e2e/
    apps/mobile/  apps/mobile-e2e/    (Ionic + Capacitor se añaden en el Hito 7)
    libs/shared/{domain,util,ui,api-client,auth}/
    libs/inventario/{data-access, feature-* por hito}/
  backend/
    Inventario.slnx
    Directory.Build.props   Directory.Packages.props   global.json
    src/Inventario.{Api,Application,Domain,Infrastructure}/
    tests/Inventario.{UnitTests,IntegrationTests}/
  infra/                       docker-compose.yml + bicep/ (Hito 9)
  docs/
  azure-pipelines/
```

## 8. Modelo de datos (borrador de alto nivel)

Entidades núcleo (todas con `TenantId` salvo `Tenant` y `Usuario` de superadmin):

- **Tenant** (Partner): configuración de la empresa cliente.
- **Usuario**, **Rol**, **UsuarioSucursal**.
- **Sucursal**, **Ubicacion** (jerárquica dentro de sucursal).
- **Categoria** (jerárquica).
- **Articulo**, **CodigoAlterno**, **ArticuloImagen**.
- **Existencia** (Articulo × Sucursal × Ubicacion, con cantidad y costo promedio).
- **ParametroReorden** (Articulo × Sucursal: mínimo, máximo, reorden).
- **Movimiento** (encabezado: tipo, motivo, sucursal origen/destino, estado, usuario, fecha).
- **MovimientoDetalle** (Articulo, cantidad, costo unitario).
- **Conteo**, **ConteoDetalle**.
- **Auditoria**.

Tipos de movimiento: `EntradaManual`, `SalidaManual`, `SalidaVenta`, `Ajuste`,
`Merma`, `TransferenciaSalida`, `TransferenciaEntrada`, `AjusteConteo`.

## 9. Supuestos y riesgos

| Riesgo / Supuesto | Mitigación |
|---|---|
| El free tier de Azure SQL puede quedarse corto en cómputo | Plan B: SQL Server en contenedor; medir antes de migrar |
| Diferencias entre desarrollo local y nube al migrar | Usar contenedores y Bicep desde temprano; probar despliegue pronto |
| Alcance del MVP1 aún amplio para un dev solo | Priorizar por hitos (ver roadmap); catálogo + existencias + movimientos primero |
| El cliente pide compras/ventas antes de tiempo | PRD explícito: es MVP2; renegociar hito si es bloqueante |
| Curva de aprendizaje de DevOps | Objetivo declarado del proyecto; reservar tiempo por hito para pipeline/infra |

## 10. Glosario

- **Partner / Tenant**: empresa cliente que usa el sistema de forma aislada.
- **Kardex**: historial cronológico de movimientos de un artículo.
- **Punto de reorden**: nivel de existencia que dispara la necesidad de reabastecer.
- **Promedio ponderado**: costo unitario recalculado con cada entrada.
- **Ubicación**: posición física dentro de una sucursal (pasillo/anaquel/nivel).

---

## Pendientes antes de cerrar Fase 0

- [ ] Revisión y aprobación de este PRD por el autor.
- [ ] Confirmar versiones LTS exactas de Angular y .NET al iniciar Fase 1.
- [ ] Definir campos exactos del catálogo con el cliente (plantilla de importación).
