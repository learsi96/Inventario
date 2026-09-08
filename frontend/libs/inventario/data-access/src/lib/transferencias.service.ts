import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_CLIENT_CONFIG } from '@inventario/shared-api-client';
import type {
  EstadoTransferencia,
  ResultadoPaginado,
  Transferencia,
  TransferenciaLista,
} from '@inventario/shared-domain';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class TransferenciasService {
  private readonly http = inject(HttpClient);
  private readonly url = `${inject(API_CLIENT_CONFIG).baseUrl}/api/transferencias`;

  listar(filtro: {
    sucursalId?: string;
    estado?: EstadoTransferencia;
    pagina?: number;
  }): Observable<ResultadoPaginado<TransferenciaLista>> {
    let params = new HttpParams();
    if (filtro.sucursalId) params = params.set('sucursalId', filtro.sucursalId);
    if (filtro.estado) params = params.set('estado', filtro.estado);
    if (filtro.pagina) params = params.set('pagina', filtro.pagina);
    return this.http.get<ResultadoPaginado<TransferenciaLista>>(this.url, {
      params,
    });
  }

  obtener(id: string): Observable<Transferencia> {
    return this.http.get<Transferencia>(`${this.url}/${id}`);
  }

  solicitar(datos: {
    sucursalOrigenId: string;
    sucursalDestinoId: string;
    motivo?: string | null;
    renglones: { articuloId: string; cantidad: number }[];
  }): Observable<Transferencia> {
    return this.http.post<Transferencia>(this.url, datos);
  }

  enviar(id: string): Observable<Transferencia> {
    return this.http.post<Transferencia>(`${this.url}/${id}/enviar`, {});
  }

  recibir(
    id: string,
    renglones: { articuloId: string; cantidadRecibida: number }[],
  ): Observable<Transferencia> {
    return this.http.post<Transferencia>(`${this.url}/${id}/recibir`, {
      renglones,
    });
  }

  cancelar(id: string, motivo: string): Observable<Transferencia> {
    return this.http.post<Transferencia>(`${this.url}/${id}/cancelar`, {
      motivo,
    });
  }
}
