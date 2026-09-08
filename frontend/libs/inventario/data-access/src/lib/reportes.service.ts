import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_CLIENT_CONFIG } from '@inventario/shared-api-client';
import { firstValueFrom } from 'rxjs';

export type FormatoReporte = 'xlsx' | 'pdf';

@Injectable({ providedIn: 'root' })
export class ReportesService {
  private readonly http = inject(HttpClient);
  private readonly base = `${inject(API_CLIENT_CONFIG).baseUrl}/api/reportes`;

  async existencias(
    formato: FormatoReporte,
    filtro: {
      sucursalId?: string;
      categoriaId?: string;
      soloBajoMinimo?: boolean;
    },
  ): Promise<void> {
    let params = new HttpParams().set('formato', formato);
    if (filtro.sucursalId) params = params.set('sucursalId', filtro.sucursalId);
    if (filtro.categoriaId)
      params = params.set('categoriaId', filtro.categoriaId);
    if (filtro.soloBajoMinimo) params = params.set('soloBajoMinimo', true);
    await this.descargar(`${this.base}/existencias`, params);
  }

  async valorizacion(
    formato: FormatoReporte,
    sucursalId?: string,
  ): Promise<void> {
    let params = new HttpParams().set('formato', formato);
    if (sucursalId) params = params.set('sucursalId', sucursalId);
    await this.descargar(`${this.base}/valorizacion`, params);
  }

  async kardex(
    formato: FormatoReporte,
    articuloId: string,
    sucursalId?: string,
  ): Promise<void> {
    let params = new HttpParams().set('formato', formato);
    if (sucursalId) params = params.set('sucursalId', sucursalId);
    await this.descargar(`${this.base}/kardex/${articuloId}`, params);
  }

  async diferenciasConteo(
    formato: FormatoReporte,
    conteoId: string,
  ): Promise<void> {
    const params = new HttpParams().set('formato', formato);
    await this.descargar(
      `${this.base}/conteos/${conteoId}/diferencias`,
      params,
    );
  }

  private async descargar(url: string, params: HttpParams): Promise<void> {
    const respuesta = await firstValueFrom(
      this.http.get(url, { params, observe: 'response', responseType: 'blob' }),
    );
    const blob = respuesta.body;
    if (!blob) {
      return;
    }
    const dispo = respuesta.headers.get('content-disposition') ?? '';
    const nombre =
      /filename\*?=(?:UTF-8''|")?([^";]+)/i.exec(dispo)?.[1] ?? 'reporte';
    const objectUrl = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = objectUrl;
    a.download = decodeURIComponent(nombre);
    a.click();
    URL.revokeObjectURL(objectUrl);
  }
}
