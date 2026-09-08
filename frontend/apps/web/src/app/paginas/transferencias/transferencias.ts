import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  ArticulosService,
  SucursalesService,
  TransferenciasService,
} from '@inventario/inventario-data-access';
import { AuthService } from '@inventario/shared-auth';
import type {
  ArticuloLista,
  EstadoTransferencia,
  ResultadoPaginado,
  Sucursal,
  Transferencia,
  TransferenciaLista,
} from '@inventario/shared-domain';

interface RenglonEdit {
  articuloId: string;
  sku: string;
  nombre: string;
  cantidad: number;
}

@Component({
  selector: 'inv-transferencias',
  imports: [FormsModule, DecimalPipe, DatePipe],
  templateUrl: './transferencias.html',
  styleUrl: './transferencias.scss',
})
export class TransferenciasPage implements OnInit {
  private readonly api = inject(TransferenciasService);
  private readonly sucursalesApi = inject(SucursalesService);
  private readonly articulosApi = inject(ArticulosService);
  private readonly auth = inject(AuthService);

  protected readonly estados: EstadoTransferencia[] = [
    'Solicitada',
    'EnTransito',
    'Recibida',
    'Cancelada',
  ];

  protected readonly sucursales = signal<Sucursal[]>([]);
  protected readonly resultado = signal<ResultadoPaginado<TransferenciaLista>>({
    items: [],
    total: 0,
    pagina: 1,
    tamano: 20,
  });
  protected readonly cargando = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly detalle = signal<Transferencia | null>(null);
  protected readonly recepcion = signal<Record<string, number>>({});

  protected readonly filtroEstado = signal<'' | EstadoTransferencia>('');
  protected readonly pagina = signal(1);

  protected readonly mostrarForm = signal(false);
  protected readonly formOrigen = signal('');
  protected readonly formDestino = signal('');
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
          this.formOrigen.set(s[0].id);
          this.formDestino.set(s.length > 1 ? s[1].id : s[0].id);
        }
      },
    });
    this.buscar();
  }

  protected buscar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.api
      .listar({
        estado: this.filtroEstado() || undefined,
        pagina: this.pagina(),
      })
      .subscribe({
        next: (r) => {
          this.resultado.set(r);
          this.cargando.set(false);
        },
        error: () => {
          this.error.set('No se pudieron cargar las transferencias.');
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

  protected verDetalle(t: TransferenciaLista): void {
    this.api.obtener(t.id).subscribe({
      next: (d) => {
        this.detalle.set(d);
        const rec: Record<string, number> = {};
        for (const r of d.renglones) {
          rec[r.articuloId] = r.cantidadRecibida ?? r.cantidadEnviada;
        }
        this.recepcion.set(rec);
      },
    });
  }

  protected abrirForm(): void {
    this.mostrarForm.set(true);
    this.renglones.set([]);
    this.busqueda.set('');
    this.resultadosBusqueda.set([]);
    this.formMotivo.set('');
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

  protected agregar(a: ArticuloLista): void {
    if (this.renglones().some((r) => r.articuloId === a.id)) {
      return;
    }
    this.renglones.update((rs) => [
      ...rs,
      { articuloId: a.id, sku: a.sku, nombre: a.nombre, cantidad: 1 },
    ]);
    this.busqueda.set('');
    this.resultadosBusqueda.set([]);
  }

  protected quitar(i: number): void {
    this.renglones.update((rs) => rs.filter((_, idx) => idx !== i));
  }

  protected solicitar(): void {
    if (this.renglones().length === 0) {
      return;
    }
    if (this.formOrigen() === this.formDestino()) {
      this.error.set('El origen y el destino deben ser distintos.');
      return;
    }
    this.api
      .solicitar({
        sucursalOrigenId: this.formOrigen(),
        sucursalDestinoId: this.formDestino(),
        motivo: this.formMotivo() || null,
        renglones: this.renglones().map((r) => ({
          articuloId: r.articuloId,
          cantidad: r.cantidad,
        })),
      })
      .subscribe({
        next: () => {
          this.mostrarForm.set(false);
          this.buscar();
        },
        error: (e) => this.mostrarError(e),
      });
  }

  protected enviar(id: string): void {
    this.api.enviar(id).subscribe({
      next: (d) => {
        this.detalle.set(d);
        this.buscar();
      },
      error: (e) => this.mostrarError(e),
    });
  }

  protected recibir(): void {
    const d = this.detalle();
    if (!d) {
      return;
    }
    const rec = this.recepcion();
    this.api
      .recibir(
        d.id,
        d.renglones.map((r) => ({
          articuloId: r.articuloId,
          cantidadRecibida: rec[r.articuloId] ?? 0,
        })),
      )
      .subscribe({
        next: (act) => {
          this.detalle.set(act);
          this.buscar();
        },
        error: (e) => this.mostrarError(e),
      });
  }

  protected cancelar(id: string): void {
    const motivo = window.prompt('Motivo de la cancelación:');
    if (motivo === null) {
      return;
    }
    this.api.cancelar(id, motivo).subscribe({
      next: (d) => {
        this.detalle.set(d);
        this.buscar();
      },
      error: (e) => this.mostrarError(e),
    });
  }

  protected fijarRecibida(articuloId: string, valor: string): void {
    this.recepcion.update((r) => ({ ...r, [articuloId]: Number(valor) || 0 }));
  }

  private mostrarError(r: {
    error?: { detail?: string; title?: string };
  }): void {
    this.error.set(
      r.error?.detail ?? r.error?.title ?? 'Operación no permitida.',
    );
  }
}
