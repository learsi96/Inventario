import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_CLIENT_CONFIG } from '@inventario/shared-api-client';
import type {
  ActualizarSucursal,
  CrearSucursal,
  Sucursal,
} from '@inventario/shared-domain';
import { Observable } from 'rxjs';

/** Acceso a los endpoints de sucursales de la API. */
@Injectable({ providedIn: 'root' })
export class SucursalesService {
  private readonly http = inject(HttpClient);
  private readonly url = `${inject(API_CLIENT_CONFIG).baseUrl}/api/sucursales`;

  listar(incluirInactivas = false): Observable<Sucursal[]> {
    return this.http.get<Sucursal[]>(this.url, {
      params: { incluirInactivas },
    });
  }

  obtener(id: string): Observable<Sucursal> {
    return this.http.get<Sucursal>(`${this.url}/${id}`);
  }

  crear(datos: CrearSucursal): Observable<Sucursal> {
    return this.http.post<Sucursal>(this.url, datos);
  }

  actualizar(id: string, datos: ActualizarSucursal): Observable<Sucursal> {
    return this.http.put<Sucursal>(`${this.url}/${id}`, datos);
  }

  cambiarActivacion(id: string, activa: boolean): Observable<Sucursal> {
    return this.http.patch<Sucursal>(`${this.url}/${id}/activacion`, {
      activa,
    });
  }
}
