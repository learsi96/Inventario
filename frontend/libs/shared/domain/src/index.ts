// Modelos y contratos de dominio compartidos entre la web y la app móvil.
// Sin dependencias de framework (solo tipos y lógica pura).

/** Resultado paginado estándar que devuelven los listados de la API. */
export interface PagedResult<T> {
  readonly items: readonly T[];
  readonly total: number;
  readonly page: number;
  readonly pageSize: number;
}

/** Identificador de un partner (tenant). */
export type TenantId = string;

/** Roles del sistema dentro de un partner. */
export type RolUsuario =
  'Administrador' | 'EncargadoAlmacen' | 'Vendedor' | 'Consulta';

/** Datos del usuario autenticado (respuesta de /api/auth/me). */
export interface UsuarioActual {
  readonly id: string;
  readonly email: string;
  readonly nombreCompleto: string;
  readonly rol: RolUsuario;
  readonly tenantId: TenantId;
  readonly sucursalIds: readonly string[];
}

/** Sucursal de un partner. */
export interface Sucursal {
  readonly id: string;
  readonly nombre: string;
  readonly codigo: string;
  readonly direccion: string | null;
  readonly activa: boolean;
  readonly creadoEn: string;
}

export interface CrearSucursal {
  readonly nombre: string;
  readonly codigo: string;
  readonly direccion?: string | null;
}

export type ActualizarSucursal = CrearSucursal;
