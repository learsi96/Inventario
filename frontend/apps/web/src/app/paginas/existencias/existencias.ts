import { DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  ArticulosService,
  CategoriasService,
  ExistenciasService,
  SucursalesService,
  UbicacionesService,
} from '@inventario/inventario-data-access';
import { AuthService } from '@inventario/shared-auth';
import type {
  ArticuloLista,
  CategoriaNodo,
  Existencia,
  ExistenciaLista,
  ResultadoPaginado,
  Sucursal,
  Ubicacion,
  Valorizacion,
} from '@inventario/shared-domain';

interface OpcionCategoria {
  readonly id: string;
  readonly etiqueta: string;
}

@Component({
  selector: 'inv-existencias',
  imports: [ReactiveFormsModule, DecimalPipe],
  templateUrl: './existencias.html',
  styleUrl: './existencias.scss',
})
export class ExistenciasPage implements OnInit {
  private readonly api = inject(ExistenciasService);
  private readonly sucursalesApi = inject(SucursalesService);
  private readonly categoriasApi = inject(CategoriasService);
  private readonly articulosApi = inject(ArticulosService);
  private readonly ubicacionesApi = inject(UbicacionesService);
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);

  protected readonly sucursales = signal<Sucursal[]>([]);
  protected readonly categorias = signal<OpcionCategoria[]>([]);
  protected readonly sucursalId = signal('');
  protected readonly resultado = signal<ResultadoPaginado<ExistenciaLista>>({
    items: [],
    total: 0,
    pagina: 1,
    tamano: 20,
  });
  protected readonly valorizacion = signal<Valorizacion | null>(null);
  protected readonly cargando = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly filtroTexto = signal('');
  protected readonly filtroCategoria = signal('');
  protected readonly soloBajoMinimo = signal(false);
  protected readonly pagina = signal(1);

  protected readonly detalle = signal<Existencia | null>(null);
  protected readonly ubicaciones = signal<Ubicacion[]>([]);
  protected readonly mostrarNuevoAjuste = signal(false);
  protected readonly resultadosBusqueda = signal<ArticuloLista[]>([]);
  protected readonly articuloSeleccionado = signal<ArticuloLista | null>(null);

  protected readonly esOperador = computed(() => {
    const rol = this.auth.usuario()?.rol;
    return rol === 'Administrador' || rol === 'EncargadoAlmacen';
  });
  protected readonly esAdmin = computed(
    () => this.auth.usuario()?.rol === 'Administrador',
  );
  protected readonly totalPaginas = computed(() =>
    Math.max(1, Math.ceil(this.resultado().total / this.resultado().tamano)),
  );

  protected readonly ajusteForm = this.fb.nonNullable.group({
    cantidad: [0, [Validators.required, Validators.min(0)]],
    costoPromedio: [0, [Validators.required, Validators.min(0)]],
    motivo: ['', [Validators.required]],
  });

  protected readonly parametrosForm = this.fb.nonNullable.group({
    minimo: [0, [Validators.min(0)]],
    maximo: [0, [Validators.min(0)]],
    puntoReorden: [0, [Validators.min(0)]],
  });

  protected readonly nuevoAjusteForm = this.fb.nonNullable.group({
    busqueda: [''],
    cantidad: [0, [Validators.required, Validators.min(0)]],
    costoPromedio: [0, [Validators.required, Validators.min(0)]],
    motivo: ['Carga inicial', [Validators.required]],
  });

  ngOnInit(): void {
    this.sucursalesApi.listar().subscribe({
      next: (s) => {
        this.sucursales.set(s);
        if (s.length > 0) {
          this.sucursalId.set(s[0].id);
          this.buscar();
        }
      },
    });
    this.categoriasApi.arbol().subscribe({
      next: (arbol) => this.categorias.set(this.aplanar(arbol, 0)),
    });
  }

  protected cambiarSucursal(id: string): void {
    this.sucursalId.set(id);
    this.cerrarDetalle();
    this.pagina.set(1);
    this.buscar();
  }

  protected buscar(): void {
    if (!this.sucursalId()) {
      return;
    }
    this.cargando.set(true);
    this.error.set(null);
    this.api
      .listar({
        sucursalId: this.sucursalId(),
        categoriaId: this.filtroCategoria() || undefined,
        texto: this.filtroTexto() || undefined,
        soloBajoMinimo: this.soloBajoMinimo(),
        pagina: this.pagina(),
      })
      .subscribe({
        next: (r) => {
          this.resultado.set(r);
          this.cargando.set(false);
        },
        error: () => {
          this.error.set('No se pudieron cargar las existencias.');
          this.cargando.set(false);
        },
      });
    this.api
      .valorizacion(this.sucursalId())
      .subscribe({ next: (v) => this.valorizacion.set(v) });
  }

  protected filtrar(): void {
    this.pagina.set(1);
    this.buscar();
  }

  protected irAPagina(p: number): void {
    this.pagina.set(p);
    this.buscar();
  }

  protected abrirDetalle(fila: ExistenciaLista): void {
    this.cargarDetalle(fila.articuloId);
  }

  private cargarDetalle(articuloId: string): void {
    this.api.obtener(articuloId, this.sucursalId()).subscribe({
      next: (e) => {
        this.detalle.set(e);
        this.ajusteForm.reset({
          cantidad: e.cantidad,
          costoPromedio: e.costoPromedio,
          motivo: '',
        });
        this.parametrosForm.reset({
          minimo: e.minimo,
          maximo: e.maximo,
          puntoReorden: e.puntoReorden,
        });
        this.ubicacionesApi
          .listar(this.sucursalId())
          .subscribe({ next: (u) => this.ubicaciones.set(u) });
      },
      error: () => this.error.set('No se pudo cargar el detalle.'),
    });
  }

  protected cerrarDetalle(): void {
    this.detalle.set(null);
  }

  protected guardarAjuste(): void {
    const e = this.detalle();
    if (!e || this.ajusteForm.invalid) {
      return;
    }
    const v = this.ajusteForm.getRawValue();
    this.api
      .ajustar({
        articuloId: e.articuloId,
        sucursalId: this.sucursalId(),
        cantidad: v.cantidad,
        costoPromedio: v.costoPromedio,
        motivo: v.motivo,
      })
      .subscribe({
        next: (act) => {
          this.detalle.set(act);
          this.buscar();
        },
        error: (r: { error?: { detail?: string } }) =>
          this.error.set(r.error?.detail ?? 'No se pudo ajustar.'),
      });
  }

  protected guardarParametros(): void {
    const e = this.detalle();
    if (!e || this.parametrosForm.invalid) {
      return;
    }
    const v = this.parametrosForm.getRawValue();
    this.api
      .guardarParametros({
        articuloId: e.articuloId,
        sucursalId: this.sucursalId(),
        minimo: v.minimo,
        maximo: v.maximo,
        puntoReorden: v.puntoReorden,
      })
      .subscribe({
        next: (act) => {
          this.detalle.set(act);
          this.buscar();
        },
        error: (r: { error?: { detail?: string } }) =>
          this.error.set(
            r.error?.detail ?? 'No se pudieron guardar los parámetros.',
          ),
      });
  }

  protected abrirNuevoAjuste(): void {
    this.mostrarNuevoAjuste.set(true);
    this.articuloSeleccionado.set(null);
    this.resultadosBusqueda.set([]);
    this.nuevoAjusteForm.reset({
      busqueda: '',
      cantidad: 0,
      costoPromedio: 0,
      motivo: 'Carga inicial',
    });
  }

  protected buscarArticulo(): void {
    const texto = this.nuevoAjusteForm.controls.busqueda.value.trim();
    if (texto.length < 2) {
      this.resultadosBusqueda.set([]);
      return;
    }
    this.articulosApi
      .listar({ texto, estado: 'Activo', tamano: 8 })
      .subscribe({ next: (r) => this.resultadosBusqueda.set([...r.items]) });
  }

  protected elegirArticulo(a: ArticuloLista): void {
    this.articuloSeleccionado.set(a);
    this.resultadosBusqueda.set([]);
    this.nuevoAjusteForm.controls.busqueda.setValue(`${a.sku} — ${a.nombre}`);
  }

  protected guardarNuevoAjuste(): void {
    const a = this.articuloSeleccionado();
    if (!a || this.nuevoAjusteForm.invalid) {
      return;
    }
    const v = this.nuevoAjusteForm.getRawValue();
    this.api
      .ajustar({
        articuloId: a.id,
        sucursalId: this.sucursalId(),
        cantidad: v.cantidad,
        costoPromedio: v.costoPromedio,
        motivo: v.motivo,
      })
      .subscribe({
        next: () => {
          this.mostrarNuevoAjuste.set(false);
          this.buscar();
        },
        error: (r: { error?: { detail?: string } }) =>
          this.error.set(r.error?.detail ?? 'No se pudo registrar.'),
      });
  }

  private aplanar(
    nodos: readonly CategoriaNodo[],
    nivel: number,
  ): OpcionCategoria[] {
    const acc: OpcionCategoria[] = [];
    for (const n of nodos) {
      acc.push({ id: n.id, etiqueta: `${'— '.repeat(nivel)}${n.nombre}` });
      acc.push(...this.aplanar(n.subcategorias, nivel + 1));
    }
    return acc;
  }
}
