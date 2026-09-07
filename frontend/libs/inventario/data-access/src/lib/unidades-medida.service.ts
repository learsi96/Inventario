import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_CLIENT_CONFIG } from '@inventario/shared-api-client';
import type { UnidadMedida } from '@inventario/shared-domain';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class UnidadesMedidaService {
  private readonly http = inject(HttpClient);
  private readonly url = `${inject(API_CLIENT_CONFIG).baseUrl}/api/unidades-medida`;

  listar(): Observable<UnidadMedida[]> {
    return this.http.get<UnidadMedida[]>(this.url);
  }
}
