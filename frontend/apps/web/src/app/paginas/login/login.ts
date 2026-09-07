import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '@inventario/shared-auth';

@Component({
  selector: 'inv-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class LoginPage {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly cargando = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    email: ['admin@demo.com', [Validators.required, Validators.email]],
    contrasena: ['Demo1234!', [Validators.required]],
  });

  protected async enviar(): Promise<void> {
    if (this.form.invalid) {
      return;
    }
    this.cargando.set(true);
    this.error.set(null);
    const { email, contrasena } = this.form.getRawValue();
    try {
      await this.auth.login(email, contrasena);
      await this.router.navigate(['/']);
    } catch {
      this.error.set('Credenciales inválidas o servidor no disponible.');
    } finally {
      this.cargando.set(false);
    }
  }
}
