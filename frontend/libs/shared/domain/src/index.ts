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

export const ROLES: readonly RolUsuario[] = [
  'Administrador',
  'EncargadoAlmacen',
  'Vendedor',
  'Consulta',
];

/** Usuario de un partner (gestión por Administrador). */
export interface Usuario {
  readonly id: string;
  readonly email: string;
  readonly nombreCompleto: string;
  readonly rol: RolUsuario;
  readonly activo: boolean;
  readonly sucursalIds: readonly string[];
  readonly esUsuarioActual: boolean;
}

export interface CrearUsuario {
  readonly email: string;
  readonly nombreCompleto: string;
  readonly rol: RolUsuario;
  readonly contrasena: string;
  readonly sucursalIds: readonly string[];
}

export interface ActualizarUsuario {
  readonly nombreCompleto: string;
  readonly rol: RolUsuario;
  readonly activo: boolean;
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

// ---- Catálogo ----

/** Nodo del árbol de categorías. */
export interface CategoriaNodo {
  readonly id: string;
  readonly nombre: string;
  readonly categoriaPadreId: string | null;
  readonly esSistema: boolean;
  readonly activa: boolean;
  readonly subcategorias: readonly CategoriaNodo[];
}

export interface CrearCategoria {
  readonly nombre: string;
  readonly categoriaPadreId: string | null;
}

export type ActualizarCategoria = CrearCategoria;

/** Unidad de medida (catálogo global). */
export interface UnidadMedida {
  readonly id: string;
  readonly codigo: string;
  readonly nombre: string;
  readonly activa: boolean;
}

export type EstadoArticulo = 'Activo' | 'Descontinuado';

export type TipoCodigoAlterno =
  'Oem' | 'Proveedor' | 'Equivalencia' | 'Interno';

export interface CodigoAlterno {
  readonly codigo: string;
  readonly tipo: TipoCodigoAlterno;
}

/** Fila de la lista de artículos. */
export interface ArticuloLista {
  readonly id: string;
  readonly sku: string;
  readonly nombre: string;
  readonly marca: string | null;
  readonly categoriaNombre: string;
  readonly unidadCodigo: string;
  readonly precioVenta: number;
  readonly estado: EstadoArticulo;
  readonly tieneImagen: boolean;
}

/** Ficha completa del artículo. */
export interface Articulo {
  readonly id: string;
  readonly sku: string;
  readonly codigoBarras: string | null;
  readonly nombre: string;
  readonly descripcion: string | null;
  readonly marca: string | null;
  readonly numeroParteOem: string | null;
  readonly categoriaId: string;
  readonly categoriaNombre: string;
  readonly unidadMedidaId: string;
  readonly unidadCodigo: string;
  readonly costo: number;
  readonly precioVenta: number;
  readonly ivaPorcentaje: number;
  readonly estado: EstadoArticulo;
  readonly tieneImagen: boolean;
  readonly codigosAlternos: readonly CodigoAlterno[];
  readonly creadoEn: string;
}

export interface GuardarArticulo {
  readonly sku?: string | null;
  readonly codigoBarras?: string | null;
  readonly nombre: string;
  readonly descripcion?: string | null;
  readonly marca?: string | null;
  readonly numeroParteOem?: string | null;
  readonly categoriaId?: string | null;
  readonly unidadMedidaId: string;
  readonly costo: number;
  readonly precioVenta: number;
  readonly ivaPorcentaje?: number | null;
  readonly codigosAlternos?: readonly CodigoAlterno[];
}

export interface FiltroArticulos {
  readonly texto?: string;
  readonly categoriaId?: string;
  readonly estado?: EstadoArticulo;
  readonly pagina?: number;
  readonly tamano?: number;
}

// ---- Existencias (Hito 3) ----

export interface Ubicacion {
  readonly id: string;
  readonly sucursalId: string;
  readonly codigo: string;
  readonly descripcion: string | null;
  readonly activa: boolean;
}

export interface CrearUbicacion {
  readonly sucursalId: string;
  readonly codigo: string;
  readonly descripcion?: string | null;
}

export interface ActualizarUbicacion {
  readonly codigo: string;
  readonly descripcion?: string | null;
  readonly activa: boolean;
}

export interface ExistenciaLista {
  readonly articuloId: string;
  readonly sku: string;
  readonly articuloNombre: string;
  readonly sucursalId: string;
  readonly sucursalNombre: string;
  readonly cantidad: number;
  readonly unidadCodigo: string;
  readonly costoPromedio: number;
  readonly valor: number;
  readonly minimo: number;
  readonly bajoMinimo: boolean;
}

export interface ExistenciaUbicacion {
  readonly ubicacionId: string;
  readonly ubicacionCodigo: string;
  readonly cantidad: number;
}

export interface Existencia {
  readonly articuloId: string;
  readonly sku: string;
  readonly articuloNombre: string;
  readonly sucursalId: string;
  readonly sucursalNombre: string;
  readonly cantidad: number;
  readonly costoPromedio: number;
  readonly valor: number;
  readonly minimo: number;
  readonly maximo: number;
  readonly puntoReorden: number;
  readonly bajoMinimo: boolean;
  readonly porUbicacion: readonly ExistenciaUbicacion[];
}

export interface ValorizacionSucursal {
  readonly sucursalId: string;
  readonly sucursalNombre: string;
  readonly articulos: number;
  readonly unidades: number;
  readonly valor: number;
}

export interface Valorizacion {
  readonly valorTotal: number;
  readonly unidadesTotal: number;
  readonly articulosConExistencia: number;
  readonly porSucursal: readonly ValorizacionSucursal[];
}

export interface ResultadoPaginado<T> {
  readonly items: readonly T[];
  readonly total: number;
  readonly pagina: number;
  readonly tamano: number;
}
