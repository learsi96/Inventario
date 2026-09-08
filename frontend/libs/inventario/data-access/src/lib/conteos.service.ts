import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_CLIENT_CONFIG } from '@inventario/shared-api-client';
import type {
  Conteo,
  ConteoLista,
  EstadoConteo,
} from '@inventario/shared-domain';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ConteosService {
  private readonly http = inject(HttpClient);
  private readonly url = `${inject(API_CLIENT_CONFIG).baseUrl}/api/conteos`;

  listar(
    sucursalId?: string,
    estado?: EstadoConteo,
  ): Observable<ConteoLista[]> {
    let params = new HttpParams();
    if (sucursalId) params = params.set('sucursalId', sucursalId);
    if (estado) params = params.set('estado', estado);
    return this.http.get<ConteoLista[]>(this.url, { params });
  }

  obtener(id: string): Observable<Conteo> {
    return this.http.get<Conteo>(`${this.url}/${id}`);
  }

  iniciar(sucursalId: string, categoriaId?: string | null): Observable<Conteo> {
    return this.http.post<Conteo>(this.url, {
      sucursalId,
      categoriaId: categoriaId ?? null,
    });
  }

  capturar(
    id: string,
    renglones: { articuloId: string; cantidadContada: number }[],
  ): Observable<Conteo> {
    return this.http.put<Conteo>(`${this.url}/${id}/captura`, { renglones });
  }

  conciliar(id: string): Observable<Conteo> {
    return this.http.post<Conteo>(`${this.url}/${id}/conciliar`, {});
  }

  cancelar(id: string): Observable<Conteo> {
    return this.http.post<Conteo>(`${this.url}/${id}/cancelar`, {});
  }
}
