import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_CLIENT_CONFIG } from '@inventario/shared-api-client';
import type {
  ActualizarCategoria,
  CategoriaNodo,
  CrearCategoria,
} from '@inventario/shared-domain';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class CategoriasService {
  private readonly http = inject(HttpClient);
  private readonly url = `${inject(API_CLIENT_CONFIG).baseUrl}/api/categorias`;

  arbol(incluirInactivas = false): Observable<CategoriaNodo[]> {
    return this.http.get<CategoriaNodo[]>(this.url, {
      params: { incluirInactivas },
    });
  }

  crear(datos: CrearCategoria): Observable<CategoriaNodo> {
    return this.http.post<CategoriaNodo>(this.url, datos);
  }

  actualizar(
    id: string,
    datos: ActualizarCategoria,
  ): Observable<CategoriaNodo> {
    return this.http.put<CategoriaNodo>(`${this.url}/${id}`, datos);
  }

  cambiarActivacion(id: string, activa: boolean): Observable<void> {
    return this.http.patch<void>(`${this.url}/${id}/activacion`, { activa });
  }
}
