import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CategoriasService } from '@inventario/inventario-data-access';
import { AuthService } from '@inventario/shared-auth';
import type { CategoriaNodo } from '@inventario/shared-domain';

interface FilaCategoria {
  readonly nodo: CategoriaNodo;
  readonly nivel: number;
}

@Component({
  selector: 'inv-categorias',
  imports: [ReactiveFormsModule],
  templateUrl: './categorias.html',
  styleUrl: './categorias.scss',
})
export class CategoriasPage implements OnInit {
  private readonly api = inject(CategoriasService);
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);

  protected readonly arbol = signal<CategoriaNodo[]>([]);
  protected readonly cargando = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly editandoId = signal<string | null>(null);
  protected readonly incluirInactivas = signal(false);

  protected readonly esAdmin = computed(
    () => this.auth.usuario()?.rol === 'Administrador',
  );

  /** Árbol aplanado para pintar la tabla con sangría por nivel. */
  protected readonly filas = computed<FilaCategoria[]>(() => {
    const acc: FilaCategoria[] = [];
    const recorrer = (nodos: readonly CategoriaNodo[], nivel: number) => {
      for (const nodo of nodos) {
        acc.push({ nodo, nivel });
        recorrer(nodo.subcategorias, nivel + 1);
      }
    };
    recorrer(this.arbol(), 0);
    return acc;
  });

  protected readonly form = this.fb.nonNullable.group({
    nombre: ['', [Validators.required]],
    categoriaPadreId: [''],
  });

  ngOnInit(): void {
    this.recargar();
  }

  protected recargar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.api.arbol(this.incluirInactivas()).subscribe({
      next: (arbol) => {
        this.arbol.set(arbol);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('No se pudieron cargar las categorías.');
        this.cargando.set(false);
      },
    });
  }

  protected alternarInactivas(): void {
    this.incluirInactivas.update((v) => !v);
    this.recargar();
  }

  protected nuevaRaiz(): void {
    this.editandoId.set(null);
    this.form.reset({ nombre: '', categoriaPadreId: '' });
  }

  protected nuevaHija(padre: CategoriaNodo): void {
    this.editandoId.set(null);
    this.form.reset({ nombre: '', categoriaPadreId: padre.id });
  }

  protected editar(nodo: CategoriaNodo): void {
    this.editandoId.set(nodo.id);
    this.form.reset({
      nombre: nodo.nombre,
      categoriaPadreId: nodo.categoriaPadreId ?? '',
    });
  }

  protected cancelar(): void {
    this.editandoId.set(null);
    this.form.reset({ nombre: '', categoriaPadreId: '' });
  }

  protected guardar(): void {
    if (this.form.invalid) {
      return;
    }
    const { nombre, categoriaPadreId } = this.form.getRawValue();
    const datos = { nombre, categoriaPadreId: categoriaPadreId || null };
    const id = this.editandoId();
    const peticion = id
      ? this.api.actualizar(id, datos)
      : this.api.crear(datos);

    peticion.subscribe({
      next: () => {
        this.cancelar();
        this.recargar();
      },
      error: (r: { error?: { detail?: string } }) => {
        this.error.set(r.error?.detail ?? 'No se pudo guardar.');
      },
    });
  }

  protected alternarActivacion(nodo: CategoriaNodo): void {
    this.api.cambiarActivacion(nodo.id, !nodo.activa).subscribe({
      next: () => this.recargar(),
      error: (r: { error?: { detail?: string } }) => {
        this.error.set(r.error?.detail ?? 'No se pudo cambiar la activación.');
      },
    });
  }
}
