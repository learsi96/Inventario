import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  SucursalesService,
  UsuariosService,
} from '@inventario/inventario-data-access';
import { ROLES } from '@inventario/shared-domain';
import type { RolUsuario, Sucursal, Usuario } from '@inventario/shared-domain';

@Component({
  selector: 'inv-usuarios',
  imports: [ReactiveFormsModule],
  templateUrl: './usuarios.html',
  styleUrl: './usuarios.scss',
})
export class UsuariosPage implements OnInit {
  private readonly api = inject(UsuariosService);
  private readonly sucursalesApi = inject(SucursalesService);
  private readonly fb = inject(FormBuilder);

  protected readonly roles = ROLES;

  protected readonly usuarios = signal<Usuario[]>([]);
  protected readonly sucursales = signal<Sucursal[]>([]);
  protected readonly cargando = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editandoId = signal<string | null>(null);
  protected readonly mostrarForm = signal(false);
  protected readonly sucursalesSel = signal<Set<string>>(new Set());

  protected readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    nombreCompleto: ['', [Validators.required]],
    rol: ['Consulta' as RolUsuario, [Validators.required]],
    contrasena: [''],
    activo: [true],
  });

  ngOnInit(): void {
    this.recargar();
    this.sucursalesApi
      .listar(true)
      .subscribe({ next: (s) => this.sucursales.set(s) });
  }

  protected recargar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.api.listar().subscribe({
      next: (u) => {
        this.usuarios.set(u);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('No se pudieron cargar los usuarios.');
        this.cargando.set(false);
      },
    });
  }

  protected nuevo(): void {
    this.editandoId.set(null);
    this.sucursalesSel.set(new Set());
    this.form.reset({
      email: '',
      nombreCompleto: '',
      rol: 'Consulta',
      contrasena: '',
      activo: true,
    });
    this.form.controls.email.enable();
    this.form.controls.contrasena.addValidators(Validators.minLength(8));
    this.form.controls.contrasena.updateValueAndValidity();
    this.mostrarForm.set(true);
  }

  protected editar(usuario: Usuario): void {
    this.editandoId.set(usuario.id);
    this.sucursalesSel.set(new Set(usuario.sucursalIds));
    this.form.reset({
      email: usuario.email,
      nombreCompleto: usuario.nombreCompleto,
      rol: usuario.rol,
      contrasena: '',
      activo: usuario.activo,
    });
    this.form.controls.email.disable();
    this.form.controls.contrasena.clearValidators();
    this.form.controls.contrasena.updateValueAndValidity();
    this.mostrarForm.set(true);
  }

  protected cancelar(): void {
    this.mostrarForm.set(false);
    this.editandoId.set(null);
  }

  protected alternarSucursal(id: string): void {
    this.sucursalesSel.update((set) => {
      const copia = new Set(set);
      if (copia.has(id)) {
        copia.delete(id);
      } else {
        copia.add(id);
      }
      return copia;
    });
  }

  protected guardar(): void {
    if (this.form.invalid) {
      return;
    }
    const v = this.form.getRawValue();
    const sucursalIds = [...this.sucursalesSel()];
    const id = this.editandoId();

    const peticion = id
      ? this.api.actualizar(id, {
          nombreCompleto: v.nombreCompleto,
          rol: v.rol,
          activo: v.activo,
          sucursalIds,
        })
      : this.api.crear({
          email: v.email,
          nombreCompleto: v.nombreCompleto,
          rol: v.rol,
          contrasena: v.contrasena,
          sucursalIds,
        });

    peticion.subscribe({
      next: () => {
        this.cancelar();
        this.recargar();
      },
      error: (r: { error?: { detail?: string } }) =>
        this.error.set(r.error?.detail ?? 'No se pudo guardar.'),
    });
  }

  protected alternarActivacion(usuario: Usuario): void {
    this.api.cambiarActivacion(usuario.id, !usuario.activo).subscribe({
      next: () => this.recargar(),
      error: (r: { error?: { detail?: string } }) =>
        this.error.set(r.error?.detail ?? 'No se pudo cambiar el estado.'),
    });
  }

  protected resetContrasena(usuario: Usuario): void {
    const clave = window.prompt(
      `Nueva contraseña para ${usuario.nombreCompleto} (mín. 8 caracteres):`,
    );
    if (!clave) {
      return;
    }
    this.api.resetContrasena(usuario.id, clave).subscribe({
      next: () => this.error.set(null),
      error: (r: { error?: { detail?: string } }) =>
        this.error.set(
          r.error?.detail ?? 'No se pudo restablecer la contraseña.',
        ),
    });
  }
}
