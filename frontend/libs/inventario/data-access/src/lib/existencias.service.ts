import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_CLIENT_CONFIG } from '@inventario/shared-api-client';
import type {
  ActualizarUbicacion,
  CrearUbicacion,
  Existencia,
  ExistenciaLista,
  ResultadoPaginado,
  Ubicacion,
  Valorizacion,
} from '@inventario/shared-domain';
import { Observable } from 'rxjs';

export interface FiltroExistencias {
  readonly sucursalId?: string;
  readonly categoriaId?: string;
  readonly texto?: string;
  readonly soloBajoMinimo?: boolean;
  readonly pagina?: number;
}

@Injectable({ providedIn: 'root' })
export class UbicacionesService {
  private readonly http = inject(HttpClient);
  private readonly url = `${inject(API_CLIENT_CONFIG).baseUrl}/api/ubicaciones`;

  listar(
    sucursalId?: string,
    incluirInactivas = false,
  ): Observable<Ubicacion[]> {
    let params = new HttpParams().set('incluirInactivas', incluirInactivas);
    if (sucursalId) params = params.set('sucursalId', sucursalId);
    return this.http.get<Ubicacion[]>(this.url, { params });
  }

  crear(datos: CrearUbicacion): Observable<Ubicacion> {
    return this.http.post<Ubicacion>(this.url, datos);
  }

  actualizar(id: string, datos: ActualizarUbicacion): Observable<Ubicacion> {
    return this.http.put<Ubicacion>(`${this.url}/${id}`, datos);
  }
}

@Injectable({ providedIn: 'root' })
export class ExistenciasService {
  private readonly http = inject(HttpClient);
  private readonly url = `${inject(API_CLIENT_CONFIG).baseUrl}/api/existencias`;

  listar(
    filtro: FiltroExistencias,
  ): Observable<ResultadoPaginado<ExistenciaLista>> {
    let params = new HttpParams();
    if (filtro.sucursalId) params = params.set('sucursalId', filtro.sucursalId);
    if (filtro.categoriaId)
      params = params.set('categoriaId', filtro.categoriaId);
    if (filtro.texto) params = params.set('texto', filtro.texto);
    if (filtro.soloBajoMinimo) params = params.set('soloBajoMinimo', true);
    if (filtro.pagina) params = params.set('pagina', filtro.pagina);
    return this.http.get<ResultadoPaginado<ExistenciaLista>>(this.url, {
      params,
    });
  }

  obtener(articuloId: string, sucursalId: string): Observable<Existencia> {
    return this.http.get<Existencia>(`${this.url}/${articuloId}/${sucursalId}`);
  }

  valorizacion(sucursalId?: string): Observable<Valorizacion> {
    let params = new HttpParams();
    if (sucursalId) params = params.set('sucursalId', sucursalId);
    return this.http.get<Valorizacion>(`${this.url}/valorizacion`, { params });
  }

  ajustar(datos: {
    articuloId: string;
    sucursalId: string;
    cantidad: number;
    costoPromedio: number;
    motivo: string;
  }): Observable<Existencia> {
    return this.http.post<Existencia>(`${this.url}/ajuste`, datos);
  }

  guardarParametros(datos: {
    articuloId: string;
    sucursalId: string;
    minimo: number;
    maximo: number;
    puntoReorden: number;
  }): Observable<Existencia> {
    return this.http.put<Existencia>(`${this.url}/parametros`, datos);
  }

  asignarUbicaciones(datos: {
    articuloId: string;
    sucursalId: string;
    asignaciones: { ubicacionId: string; cantidad: number }[];
  }): Observable<Existencia> {
    return this.http.put<Existencia>(`${this.url}/ubicaciones`, datos);
  }
}
