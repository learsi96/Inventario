import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { API_CLIENT_CONFIG } from '@inventario/shared-api-client';
import type { UsuarioActual } from '@inventario/shared-domain';
import { firstValueFrom } from 'rxjs';

const CLAVE_TOKEN = 'inventario.token';

interface TokenResponse {
  readonly accessToken: string;
  readonly expiraEn: string;
}

/** Estado de autenticación del cliente: token, usuario y sesión. */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_CLIENT_CONFIG).baseUrl;

  private readonly _token = signal<string | null>(this.leerToken());
  private readonly _usuario = signal<UsuarioActual | null>(null);

  readonly token = this._token.asReadonly();
  readonly usuario = this._usuario.asReadonly();
  readonly estaAutenticado = computed(() => this._token() !== null);

  async login(email: string, contrasena: string): Promise<void> {
    const respuesta = await firstValueFrom(
      this.http.post<TokenResponse>(`${this.baseUrl}/api/auth/login`, {
        email,
        contrasena,
      }),
    );
    this.guardarToken(respuesta.accessToken);
    this._token.set(respuesta.accessToken);
    await this.cargarUsuario();
  }

  async cargarUsuario(): Promise<void> {
    if (!this._token()) {
      return;
    }
    try {
      const usuario = await firstValueFrom(
        this.http.get<UsuarioActual>(`${this.baseUrl}/api/auth/me`),
      );
      this._usuario.set(usuario);
    } catch {
      this.logout();
    }
  }

  logout(): void {
    try {
      localStorage.removeItem(CLAVE_TOKEN);
    } catch {
      /* almacenamiento no disponible */
    }
    this._token.set(null);
    this._usuario.set(null);
  }

  private leerToken(): string | null {
    try {
      return localStorage.getItem(CLAVE_TOKEN);
    } catch {
      return null;
    }
  }

  private guardarToken(token: string): void {
    try {
      localStorage.setItem(CLAVE_TOKEN, token);
    } catch {
      /* almacenamiento no disponible */
    }
  }
}
