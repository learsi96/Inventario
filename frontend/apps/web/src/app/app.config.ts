import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideApiClient } from '@inventario/shared-api-client';
import { AuthService, authInterceptor } from '@inventario/shared-auth';
import { appRoutes } from './app.routes';
import { API_BASE_URL } from './config';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(appRoutes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideApiClient({ baseUrl: API_BASE_URL }),
    provideAppInitializer(() => inject(AuthService).cargarUsuario()),
  ],
};
