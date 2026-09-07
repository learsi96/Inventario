import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { SucursalesService } from '@inventario/inventario-data-access';
import { AuthService } from '@inventario/shared-auth';
import type { Sucursal } from '@inventario/shared-domain';

@Component({
  selector: 'inv-sucursales',
  imports: [ReactiveFormsModule],
  templateUrl: './sucursales.html',
  styleUrl: './sucursales.scss',
})
export class SucursalesPage implements OnInit {
  private readonly api = inject(SucursalesService);
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);

  protected readonly sucursales = signal<Sucursal[]>([]);
  protected readonly cargando = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editandoId = signal<string | null>(null);
  protected readonly incluirInactivas = signal(false);

  protected readonly esAdmin = computed(
    () => this.auth.usuario()?.rol === 'Administrador',
  );

  protected readonly form = this.fb.nonNullable.group({
    nombre: ['', [Validators.required]],
    codigo: ['', [Validators.required]],
    direccion: [''],
  });

  ngOnInit(): void {
    this.recargar();
  }

  protected recargar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.api.listar(this.incluirInactivas()).subscribe({
      next: (lista) => {
        this.sucursales.set(lista);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar la lista de sucursales.');
        this.cargando.set(false);
      },
    });
  }

  protected alternarInactivas(): void {
    this.incluirInactivas.update((v) => !v);
    this.recargar();
  }

  protected editar(sucursal: Sucursal): void {
    this.editandoId.set(sucursal.id);
    this.form.setValue({
      nombre: sucursal.nombre,
      codigo: sucursal.codigo,
      direccion: sucursal.direccion ?? '',
    });
  }

  protected cancelar(): void {
    this.editandoId.set(null);
    this.form.reset();
  }

  protected guardar(): void {
    if (this.form.invalid) {
      return;
    }
    const datos = this.form.getRawValue();
    const id = this.editandoId();
    const peticion = id
      ? this.api.actualizar(id, datos)
      : this.api.crear(datos);

    peticion.subscribe({
      next: () => {
        this.cancelar();
        this.recargar();
      },
      error: (respuesta: { error?: { detail?: string } }) => {
        this.error.set(respuesta.error?.detail ?? 'No se pudo guardar.');
      },
    });
  }

  protected alternarActivacion(sucursal: Sucursal): void {
    this.api.cambiarActivacion(sucursal.id, !sucursal.activa).subscribe({
      next: () => this.recargar(),
      error: () => this.error.set('No se pudo cambiar la activación.'),
    });
  }
}
