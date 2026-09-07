# 0006 — Modelo de existencias

- **Estado:** Aceptada
- **Fecha:** 2026-09-07

## Contexto

El Hito 3 introduce el control de existencias: cuánto hay de cada artículo, en
qué sucursal y ubicación, con qué costo, y cuándo está bajo mínimo. Los cambios
de existencia por operación (entradas, salidas, ajustes) llegan en el Hito 4;
aquí se define el modelo de datos y las lecturas.

## Decisión

### Ubicación
`Ubicacion` (multi-tenant): `SucursalId`, `Codigo` (único por sucursal, p. ej.
`P1-A3-N2`), `Descripcion?`, `Activa`. Estructura plana — pasillo/anaquel/nivel
son solo la forma de nombrar el código, no entidades separadas.

### Existencia
`Existencia` con grano **(Articulo, Sucursal)** — una fila por par, creada bajo
demanda. Es el registro autoritativo de:

- `Cantidad` (decimal)
- `CostoPromedio` (decimal, promedio ponderado — ver [0003](0003-acceso-a-datos.md))
- `Minimo`, `Maximo`, `PuntoReorden` (parámetros de reorden, en la misma fila)

**Valorización** = Σ `Cantidad` × `CostoPromedio`.
**Bajo mínimo** = existencias con `Cantidad` ≤ `Minimo` y `Minimo` > 0.

### Existencia por ubicación
`ExistenciaUbicacion` con grano **(Existencia, Ubicacion)**: solo `Cantidad`. Es
el desglose de la cantidad de la sucursal entre estantes, para las sucursales que
lo llevan. En MVP1 no se fuerza la reconciliación (Σ ubicaciones = cantidad de
sucursal); se ofrece como ayuda visual y de captura.

### Costo promedio ponderado
Al registrar una entrada (Hito 4):
`nuevoCosto = (cantActual · costoActual + cantEntrada · costoEntrada) / (cantActual + cantEntrada)`
Las salidas no cambian el costo. Los ajustes pueden fijar cantidad y costo.

### Alcance del Hito 3
- CRUD de ubicaciones.
- Edición de parámetros de reorden por (artículo, sucursal).
- **Ajuste de existencia**: fija la `Cantidad` y el `CostoPromedio` absolutos de un
  (artículo, sucursal) con un motivo. Es la carga inicial de inventario y el
  precursor del sistema de movimientos del Hito 4 (donde el ajuste pasará a ser un
  movimiento con su registro en el kardex).
- Lecturas: listado de existencias, valorización, artículos bajo mínimo.

## Consecuencias

- El costo vive en un solo lugar por sucursal; sin ambigüedad entre estantes.
- `ExistenciaUbicacion` sin reconciliación estricta puede desalinearse; se acepta
  para MVP1 y se endurece si el cliente lo pide.
- El "ajuste de existencia" del Hito 3 se reimplementa como movimiento en el
  Hito 4 (deuda técnica conocida y acotada).

## Alternativas consideradas

- **Grano (Articulo, Sucursal, Ubicacion) con costo replicado:** evita una tabla
  pero duplica el costo y complica su actualización.
- **Costeo PEPS:** más preciso con inflación, requiere capas de costo por lote;
  es Futuro (ver PRD).
