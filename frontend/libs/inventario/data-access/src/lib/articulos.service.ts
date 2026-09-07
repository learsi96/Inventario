import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_CLIENT_CONFIG } from '@inventario/shared-api-client';
import type {
  Articulo,
  ArticuloLista,
  EstadoArticulo,
  FiltroArticulos,
  GuardarArticulo,
  ResultadoPaginado,
} from '@inventario/shared-domain';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ArticulosService {
  private readonly http = inject(HttpClient);
  private readonly url = `${inject(API_CLIENT_CONFIG).baseUrl}/api/articulos`;

  listar(
    filtro: FiltroArticulos,
  ): Observable<ResultadoPaginado<ArticuloLista>> {
    let params = new HttpParams();
    if (filtro.texto) params = params.set('texto', filtro.texto);
    if (filtro.categoriaId)
      params = params.set('categoriaId', filtro.categoriaId);
    if (filtro.estado) params = params.set('estado', filtro.estado);
    if (filtro.pagina) params = params.set('pagina', filtro.pagina);
    if (filtro.tamano) params = params.set('tamano', filtro.tamano);
    return this.http.get<ResultadoPaginado<ArticuloLista>>(this.url, {
      params,
    });
  }

  obtener(id: string): Observable<Articulo> {
    return this.http.get<Articulo>(`${this.url}/${id}`);
  }

  crear(datos: GuardarArticulo): Observable<Articulo> {
    return this.http.post<Articulo>(this.url, datos);
  }

  actualizar(id: string, datos: GuardarArticulo): Observable<Articulo> {
    return this.http.put<Articulo>(`${this.url}/${id}`, datos);
  }

  cambiarEstado(id: string, estado: EstadoArticulo): Observable<Articulo> {
    return this.http.patch<Articulo>(`${this.url}/${id}/estado`, { estado });
  }

  subirImagen(id: string, archivo: File): Observable<Articulo> {
    const cuerpo = new FormData();
    cuerpo.append('archivo', archivo);
    return this.http.post<Articulo>(`${this.url}/${id}/imagen`, cuerpo);
  }

  eliminarImagen(id: string): Observable<Articulo> {
    return this.http.delete<Articulo>(`${this.url}/${id}/imagen`);
  }

  /** Descarga la imagen como blob (con el token del interceptor) para mostrarla. */
  obtenerImagen(id: string): Observable<Blob> {
    return this.http.get(`${this.url}/${id}/imagen`, { responseType: 'blob' });
  }
}
