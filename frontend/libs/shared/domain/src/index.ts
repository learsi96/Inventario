// Modelos y contratos de dominio compartidos entre la web y la app móvil.
// Se poblará a partir del Hito 1: Tenant, Sucursal, Articulo, Existencia,
// Movimiento, Conteo, etc. Sin dependencias de framework (solo tipos y lógica pura).

/** Resultado paginado estándar que devuelven los listados de la API. */
export interface PagedResult<T> {
  readonly items: readonly T[];
  readonly total: number;
  readonly page: number;
  readonly pageSize: number;
}

/** Identificador de un partner (tenant). */
export type TenantId = string;
