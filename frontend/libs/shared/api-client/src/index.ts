// Configuración compartida del cliente HTTP contra la API .NET.

import { InjectionToken, Provider } from '@angular/core';

export interface ApiClientConfig {
  /** URL base de la API, p. ej. http://localhost:5027 (sin barra final). */
  readonly baseUrl: string;
}

export const API_CLIENT_CONFIG = new InjectionToken<ApiClientConfig>(
  'API_CLIENT_CONFIG',
);

export function provideApiClient(config: ApiClientConfig): Provider {
  return { provide: API_CLIENT_CONFIG, useValue: config };
}
