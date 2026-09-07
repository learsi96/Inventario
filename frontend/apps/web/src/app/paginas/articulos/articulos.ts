import { DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import {
  ArticulosService,
  CategoriasService,
  UnidadesMedidaService,
} from '@inventario/inventario-data-access';
import { AuthService } from '@inventario/shared-auth';
import type {
  ArticuloLista,
  CategoriaNodo,
  EstadoArticulo,
  ResultadoPaginado,
  TipoCodigoAlterno,
  UnidadMedida,
} from '@inventario/shared-domain';

interface OpcionCategoria {
  readonly id: string;
  readonly etiqueta: string;
}

const TIPOS_CODIGO: readonly TipoCodigoAlterno[] = [
  'Oem',
  'Proveedor',
  'Equivalencia',
  'Interno',
];

@Component({
  selector: 'inv-articulos',
  imports: [ReactiveFormsModule, DecimalPipe],
  templateUrl: './articulos.html',
  styleUrl: './articulos.scss',
})
export class ArticulosPage implements OnInit {
  private readonly api = inject(ArticulosService);
  private readonly categoriasApi = inject(CategoriasService);
  private readonly unidadesApi = inject(UnidadesMedidaService);
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);

  protected readonly tiposCodigo = TIPOS_CODIGO;

  protected readonly resultado = signal<ResultadoPaginado<ArticuloLista>>({
    items: [],
    total: 0,
    pagina: 1,
    tamano: 20,
  });
  protected readonly cargando = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly categorias = signal<OpcionCategoria[]>([]);
  protected readonly unidades = signal<UnidadMedida[]>([]);
  protected readonly editandoId = signal<string | null>(null);
  protected readonly mostrarForm = signal(false);
  protected readonly imagenUrl = signal<string | null>(null);
  protected readonly subiendoImagen = signal(false);

  protected readonly filtroTexto = signal('');
  protected readonly filtroCategoria = signal('');
  protected readonly filtroEstado = signal<'' | EstadoArticulo>('');
  protected readonly pagina = signal(1);

  protected readonly esAdmin = computed(
    () => this.auth.usuario()?.rol === 'Administrador',
  );

  protected readonly totalPaginas = computed(() => {
    const r = this.resultado();
    return Math.max(1, Math.ceil(r.total / r.tamano));
  });

  protected readonly codigosAlternos = this.fb.array<FormGroup>([]);

  protected readonly form = this.fb.nonNullable.group({
    sku: [''],
    codigoBarras: [''],
    nombre: ['', [Validators.required]],
    descripcion: [''],
    marca: [''],
    numeroParteOem: [''],
    categoriaId: [''],
    unidadMedidaId: ['', [Validators.required]],
    costo: [0, [Validators.required, Validators.min(0)]],
    precioVenta: [0, [Validators.required, Validators.min(0)]],
    ivaPorcentaje: [16, [Validators.min(0), Validators.max(100)]],
  });

  ngOnInit(): void {
    this.cargarCatalogos();
    this.buscar();
  }

  protected cargarCatalogos(): void {
    this.categoriasApi.arbol().subscribe({
      next: (arbol) => this.categorias.set(this.aplanar(arbol, 0)),
    });
    this.unidadesApi.listar().subscribe({ next: (u) => this.unidades.set(u) });
  }

  protected buscar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.api
      .listar({
        texto: this.filtroTexto() || undefined,
        categoriaId: this.filtroCategoria() || undefined,
        estado: this.filtroEstado() || undefined,
        pagina: this.pagina(),
        tamano: 20,
      })
      .subscribe({
        next: (r) => {
          this.resultado.set(r);
          this.cargando.set(false);
        },
        error: () => {
          this.error.set('No se pudieron cargar los artículos.');
          this.cargando.set(false);
        },
      });
  }

  protected filtrar(): void {
    this.pagina.set(1);
    this.buscar();
  }

  protected irAPagina(p: number): void {
    this.pagina.set(p);
    this.buscar();
  }

  protected nuevo(): void {
    this.editandoId.set(null);
    this.limpiarImagen();
    this.codigosAlternos.clear();
    this.form.reset({
      sku: '',
      codigoBarras: '',
      nombre: '',
      descripcion: '',
      marca: '',
      numeroParteOem: '',
      categoriaId: '',
      unidadMedidaId: '',
      costo: 0,
      precioVenta: 0,
      ivaPorcentaje: 16,
    });
    this.mostrarForm.set(true);
  }

  protected editar(articulo: ArticuloLista): void {
    this.limpiarImagen();
    this.api.obtener(articulo.id).subscribe({
      next: (a) => {
        this.editandoId.set(a.id);
        if (a.tieneImagen) {
          this.cargarImagen(a.id);
        }
        this.codigosAlternos.clear();
        for (const c of a.codigosAlternos) {
          this.codigosAlternos.push(this.grupoCodigo(c.codigo, c.tipo));
        }
        this.form.reset({
          sku: a.sku,
          codigoBarras: a.codigoBarras ?? '',
          nombre: a.nombre,
          descripcion: a.descripcion ?? '',
          marca: a.marca ?? '',
          numeroParteOem: a.numeroParteOem ?? '',
          categoriaId: a.categoriaId,
          unidadMedidaId: a.unidadMedidaId,
          costo: a.costo,
          precioVenta: a.precioVenta,
          ivaPorcentaje: a.ivaPorcentaje,
        });
        this.mostrarForm.set(true);
      },
      error: () => this.error.set('No se pudo cargar el artículo.'),
    });
  }

  protected agregarCodigo(): void {
    this.codigosAlternos.push(this.grupoCodigo());
  }

  protected quitarCodigo(indice: number): void {
    this.codigosAlternos.removeAt(indice);
  }

  protected cancelar(): void {
    this.mostrarForm.set(false);
    this.editandoId.set(null);
    this.limpiarImagen();
  }

  protected subirImagen(evento: Event): void {
    const input = evento.target as HTMLInputElement;
    const archivo = input.files?.[0];
    const id = this.editandoId();
    if (!archivo || !id) {
      return;
    }
    this.subiendoImagen.set(true);
    this.api.subirImagen(id, archivo).subscribe({
      next: () => {
        this.subiendoImagen.set(false);
        input.value = '';
        this.cargarImagen(id);
      },
      error: (r: { error?: { title?: string } }) => {
        this.subiendoImagen.set(false);
        this.error.set(r.error?.title ?? 'No se pudo subir la imagen.');
      },
    });
  }

  protected eliminarImagen(): void {
    const id = this.editandoId();
    if (!id) {
      return;
    }
    this.api.eliminarImagen(id).subscribe({
      next: () => this.limpiarImagen(),
      error: () => this.error.set('No se pudo eliminar la imagen.'),
    });
  }

  private cargarImagen(id: string): void {
    this.api.obtenerImagen(id).subscribe({
      next: (blob) => {
        this.limpiarImagen();
        this.imagenUrl.set(URL.createObjectURL(blob));
      },
    });
  }

  private limpiarImagen(): void {
    const actual = this.imagenUrl();
    if (actual) {
      URL.revokeObjectURL(actual);
    }
    this.imagenUrl.set(null);
  }

  protected guardar(): void {
    if (this.form.invalid) {
      return;
    }
    const v = this.form.getRawValue();
    const codigos = this.codigosAlternos.getRawValue() as {
      codigo: string;
      tipo: TipoCodigoAlterno;
    }[];
    const datos = {
      sku: v.sku || null,
      codigoBarras: v.codigoBarras || null,
      nombre: v.nombre,
      descripcion: v.descripcion || null,
      marca: v.marca || null,
      numeroParteOem: v.numeroParteOem || null,
      categoriaId: v.categoriaId || null,
      unidadMedidaId: v.unidadMedidaId,
      costo: v.costo,
      precioVenta: v.precioVenta,
      ivaPorcentaje: v.ivaPorcentaje,
      codigosAlternos: codigos.filter((c) => c.codigo?.trim()),
    };
    const id = this.editandoId();
    const peticion = id
      ? this.api.actualizar(id, datos)
      : this.api.crear(datos);
    peticion.subscribe({
      next: () => {
        this.cancelar();
        this.buscar();
      },
      error: (r: { error?: { detail?: string } }) =>
        this.error.set(r.error?.detail ?? 'No se pudo guardar.'),
    });
  }

  protected alternarEstado(articulo: ArticuloLista): void {
    const nuevo: EstadoArticulo =
      articulo.estado === 'Activo' ? 'Descontinuado' : 'Activo';
    this.api.cambiarEstado(articulo.id, nuevo).subscribe({
      next: () => this.buscar(),
      error: () => this.error.set('No se pudo cambiar el estado.'),
    });
  }

  private grupoCodigo(
    codigo = '',
    tipo: TipoCodigoAlterno = 'Equivalencia',
  ): FormGroup {
    return this.fb.nonNullable.group({ codigo: [codigo], tipo: [tipo] });
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
