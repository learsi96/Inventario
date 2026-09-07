// Cliente HTTP tipado contra la API .NET. Se poblará a partir del Hito 1 con los
// servicios por recurso (SucursalesApi, ArticulosApi, ...) y el interceptor que
// añade el token y el tenant.

import { InjectionToken } from '@angular/core';

export interface ApiClientConfig {
  /** URL base de la API, p. ej. https://localhost:5001/api */
  readonly baseUrl: string;
}

export const API_CLIENT_CONFIG = new InjectionToken<ApiClientConfig>('API_CLIENT_CONFIG');
