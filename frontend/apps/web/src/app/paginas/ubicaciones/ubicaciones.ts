import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  SucursalesService,
  UbicacionesService,
} from '@inventario/inventario-data-access';
import { AuthService } from '@inventario/shared-auth';
import type { Sucursal, Ubicacion } from '@inventario/shared-domain';

@Component({
  selector: 'inv-ubicaciones',
  imports: [ReactiveFormsModule],
  templateUrl: './ubicaciones.html',
  styleUrl: './ubicaciones.scss',
})
export class UbicacionesPage implements OnInit {
  private readonly api = inject(UbicacionesService);
  private readonly sucursalesApi = inject(SucursalesService);
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);

  protected readonly sucursales = signal<Sucursal[]>([]);
  protected readonly ubicaciones = signal<Ubicacion[]>([]);
  protected readonly sucursalId = signal('');
  protected readonly cargando = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editandoId = signal<string | null>(null);
  protected readonly incluirInactivas = signal(false);

  protected readonly esAdmin = computed(
    () => this.auth.usuario()?.rol === 'Administrador',
  );

  protected readonly form = this.fb.nonNullable.group({
    codigo: ['', [Validators.required]],
    descripcion: [''],
    activa: [true],
  });

  ngOnInit(): void {
    this.sucursalesApi.listar().subscribe({
      next: (s) => {
        this.sucursales.set(s);
        if (s.length > 0) {
          this.sucursalId.set(s[0].id);
          this.recargar();
        }
      },
    });
  }

  protected cambiarSucursal(id: string): void {
    this.sucursalId.set(id);
    this.cancelar();
    this.recargar();
  }

  protected recargar(): void {
    if (!this.sucursalId()) {
      return;
    }
    this.cargando.set(true);
    this.error.set(null);
    this.api.listar(this.sucursalId(), this.incluirInactivas()).subscribe({
      next: (u) => {
        this.ubicaciones.set(u);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('No se pudieron cargar las ubicaciones.');
        this.cargando.set(false);
      },
    });
  }

  protected alternarInactivas(): void {
    this.incluirInactivas.update((v) => !v);
    this.recargar();
  }

  protected nuevo(): void {
    this.editandoId.set(null);
    this.form.reset({ codigo: '', descripcion: '', activa: true });
  }

  protected editar(u: Ubicacion): void {
    this.editandoId.set(u.id);
    this.form.reset({
      codigo: u.codigo,
      descripcion: u.descripcion ?? '',
      activa: u.activa,
    });
  }

  protected cancelar(): void {
    this.editandoId.set(null);
    this.form.reset({ codigo: '', descripcion: '', activa: true });
  }

  protected guardar(): void {
    if (this.form.invalid) {
      return;
    }
    const v = this.form.getRawValue();
    const id = this.editandoId();
    const peticion = id
      ? this.api.actualizar(id, {
          codigo: v.codigo,
          descripcion: v.descripcion || null,
          activa: v.activa,
        })
      : this.api.crear({
          sucursalId: this.sucursalId(),
          codigo: v.codigo,
          descripcion: v.descripcion || null,
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
}
