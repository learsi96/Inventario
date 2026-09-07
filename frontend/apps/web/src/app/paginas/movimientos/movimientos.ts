import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  ArticulosService,
  MovimientosService,
  SucursalesService,
} from '@inventario/inventario-data-access';
import { AuthService } from '@inventario/shared-auth';
import type {
  ArticuloLista,
  Movimiento,
  MovimientoLista,
  ResultadoPaginado,
  Sucursal,
  TipoMovimiento,
} from '@inventario/shared-domain';

interface RenglonEdit {
  articuloId: string;
  sku: string;
  nombre: string;
  cantidad: number;
  costoUnitario: number;
}

type ModoForm = 'ninguno' | 'entrada' | 'salida';

@Component({
  selector: 'inv-movimientos',
  imports: [FormsModule, DecimalPipe, DatePipe],
  templateUrl: './movimientos.html',
  styleUrl: './movimientos.scss',
})
export class MovimientosPage implements OnInit {
  private readonly api = inject(MovimientosService);
  private readonly sucursalesApi = inject(SucursalesService);
  private readonly articulosApi = inject(ArticulosService);
  private readonly auth = inject(AuthService);

  protected readonly tipos: TipoMovimiento[] = [
    'Entrada',
    'Salida',
    'Merma',
    'AjusteInventario',
  ];

  protected readonly sucursales = signal<Sucursal[]>([]);
  protected readonly resultado = signal<ResultadoPaginado<MovimientoLista>>({
    items: [],
    total: 0,
    pagina: 1,
    tamano: 20,
  });
  protected readonly cargando = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly detalle = signal<Movimiento | null>(null);

  protected readonly filtroSucursal = signal('');
  protected readonly filtroTipo = signal<'' | TipoMovimiento>('');
  protected readonly filtroTexto = signal('');
  protected readonly pagina = signal(1);

  protected readonly modo = signal<ModoForm>('ninguno');
  protected readonly formSucursal = signal('');
  protected readonly formTipoSalida = signal<'Salida' | 'Merma'>('Salida');
  protected readonly formReferencia = signal('');
  protected readonly formMotivo = signal('');
  protected readonly renglones = signal<RenglonEdit[]>([]);
  protected readonly busqueda = signal('');
  protected readonly resultadosBusqueda = signal<ArticuloLista[]>([]);

  protected readonly esOperador = computed(() => {
    const rol = this.auth.usuario()?.rol;
    return rol === 'Administrador' || rol === 'EncargadoAlmacen';
  });
  protected readonly totalPaginas = computed(() =>
    Math.max(1, Math.ceil(this.resultado().total / this.resultado().tamano)),
  );

  ngOnInit(): void {
    this.sucursalesApi.listar().subscribe({
      next: (s) => {
        this.sucursales.set(s);
        if (s.length > 0) {
          this.filtroSucursal.set(s[0].id);
          this.formSucursal.set(s[0].id);
        }
        this.buscar();
      },
    });
  }

  protected buscar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.api
      .listar({
        sucursalId: this.filtroSucursal() || undefined,
        tipo: this.filtroTipo() || undefined,
        texto: this.filtroTexto() || undefined,
        pagina: this.pagina(),
      })
      .subscribe({
        next: (r) => {
          this.resultado.set(r);
          this.cargando.set(false);
        },
        error: () => {
          this.error.set('No se pudieron cargar los movimientos.');
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

  protected verDetalle(m: MovimientoLista): void {
    this.api.obtener(m.id).subscribe({ next: (d) => this.detalle.set(d) });
  }

  protected abrir(modo: ModoForm): void {
    this.modo.set(modo);
    this.renglones.set([]);
    this.busqueda.set('');
    this.resultadosBusqueda.set([]);
    this.formReferencia.set('');
    this.formMotivo.set('');
  }

  protected cerrarForm(): void {
    this.modo.set('ninguno');
  }

  protected buscarArticulo(): void {
    const t = this.busqueda().trim();
    if (t.length < 2) {
      this.resultadosBusqueda.set([]);
      return;
    }
    this.articulosApi
      .listar({ texto: t, estado: 'Activo', tamano: 8 })
      .subscribe({ next: (r) => this.resultadosBusqueda.set([...r.items]) });
  }

  protected agregarRenglon(a: ArticuloLista): void {
    if (this.renglones().some((r) => r.articuloId === a.id)) {
      return;
    }
    this.renglones.update((rs) => [
      ...rs,
      {
        articuloId: a.id,
        sku: a.sku,
        nombre: a.nombre,
        cantidad: 1,
        costoUnitario: 0,
      },
    ]);
    this.busqueda.set('');
    this.resultadosBusqueda.set([]);
  }

  protected quitarRenglon(i: number): void {
    this.renglones.update((rs) => rs.filter((_, idx) => idx !== i));
  }

  protected guardar(): void {
    if (this.renglones().length === 0 || !this.formSucursal()) {
      return;
    }
    const modo = this.modo();
    if (modo === 'entrada') {
      this.api
        .registrarEntrada({
          sucursalId: this.formSucursal(),
          referencia: this.formReferencia() || null,
          motivo: this.formMotivo() || null,
          renglones: this.renglones().map((r) => ({
            articuloId: r.articuloId,
            cantidad: r.cantidad,
            costoUnitario: r.costoUnitario,
          })),
        })
        .subscribe({
          next: () => this.finalizar(),
          error: (e) => this.mostrarError(e),
        });
    } else if (modo === 'salida') {
      if (!this.formMotivo().trim()) {
        this.error.set('El motivo es obligatorio.');
        return;
      }
      this.api
        .registrarSalida({
          sucursalId: this.formSucursal(),
          tipo: this.formTipoSalida(),
          motivo: this.formMotivo(),
          renglones: this.renglones().map((r) => ({
            articuloId: r.articuloId,
            cantidad: r.cantidad,
          })),
        })
        .subscribe({
          next: () => this.finalizar(),
          error: (e) => this.mostrarError(e),
        });
    }
  }

  private finalizar(): void {
    this.modo.set('ninguno');
    this.buscar();
  }

  private mostrarError(r: {
    error?: { detail?: string; title?: string };
  }): void {
    this.error.set(
      r.error?.detail ??
        r.error?.title ??
        'No se pudo registrar el movimiento.',
    );
  }
}
