import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_CLIENT_CONFIG } from '@inventario/shared-api-client';
import type {
  KardexRenglon,
  Movimiento,
  MovimientoLista,
  ResultadoPaginado,
  TipoMovimiento,
} from '@inventario/shared-domain';
import { Observable } from 'rxjs';

export interface FiltroMovimientos {
  readonly sucursalId?: string;
  readonly tipo?: TipoMovimiento;
  readonly texto?: string;
  readonly pagina?: number;
}

export interface RenglonEntrada {
  readonly articuloId: string;
  readonly cantidad: number;
  readonly costoUnitario: number;
}

export interface RenglonSalida {
  readonly articuloId: string;
  readonly cantidad: number;
}

@Injectable({ providedIn: 'root' })
export class MovimientosService {
  private readonly http = inject(HttpClient);
  private readonly url = `${inject(API_CLIENT_CONFIG).baseUrl}/api/movimientos`;

  listar(
    filtro: FiltroMovimientos,
  ): Observable<ResultadoPaginado<MovimientoLista>> {
    let params = new HttpParams();
    if (filtro.sucursalId) params = params.set('sucursalId', filtro.sucursalId);
    if (filtro.tipo) params = params.set('tipo', filtro.tipo);
    if (filtro.texto) params = params.set('texto', filtro.texto);
    if (filtro.pagina) params = params.set('pagina', filtro.pagina);
    return this.http.get<ResultadoPaginado<MovimientoLista>>(this.url, {
      params,
    });
  }

  obtener(id: string): Observable<Movimiento> {
    return this.http.get<Movimiento>(`${this.url}/${id}`);
  }

  kardex(articuloId: string, sucursalId?: string): Observable<KardexRenglon[]> {
    let params = new HttpParams();
    if (sucursalId) params = params.set('sucursalId', sucursalId);
    return this.http.get<KardexRenglon[]>(`${this.url}/kardex/${articuloId}`, {
      params,
    });
  }

  registrarEntrada(datos: {
    sucursalId: string;
    referencia?: string | null;
    motivo?: string | null;
    renglones: RenglonEntrada[];
  }): Observable<Movimiento> {
    return this.http.post<Movimiento>(`${this.url}/entrada`, datos);
  }

  registrarSalida(datos: {
    sucursalId: string;
    tipo: 'Salida' | 'Merma';
    motivo: string;
    renglones: RenglonSalida[];
  }): Observable<Movimiento> {
    return this.http.post<Movimiento>(`${this.url}/salida`, datos);
  }
}
