import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_CLIENT_CONFIG } from '@inventario/shared-api-client';
import type {
  ActualizarUsuario,
  CrearUsuario,
  Usuario,
} from '@inventario/shared-domain';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class UsuariosService {
  private readonly http = inject(HttpClient);
  private readonly url = `${inject(API_CLIENT_CONFIG).baseUrl}/api/usuarios`;

  listar(): Observable<Usuario[]> {
    return this.http.get<Usuario[]>(this.url);
  }

  crear(datos: CrearUsuario): Observable<Usuario> {
    return this.http.post<Usuario>(this.url, datos);
  }

  actualizar(id: string, datos: ActualizarUsuario): Observable<Usuario> {
    return this.http.put<Usuario>(`${this.url}/${id}`, datos);
  }

  cambiarActivacion(id: string, activo: boolean): Observable<void> {
    return this.http.patch<void>(`${this.url}/${id}/activacion`, { activo });
  }

  resetContrasena(id: string, contrasena: string): Observable<void> {
    return this.http.post<void>(`${this.url}/${id}/contrasena`, { contrasena });
  }
}
