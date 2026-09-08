import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  CategoriasService,
  ConteosService,
  SucursalesService,
} from '@inventario/inventario-data-access';
import { AuthService } from '@inventario/shared-auth';
import type {
  CategoriaNodo,
  Conteo,
  ConteoLista,
  EstadoConteo,
  Sucursal,
} from '@inventario/shared-domain';

interface OpcionCategoria {
  readonly id: string;
  readonly etiqueta: string;
}

@Component({
  selector: 'inv-conteos',
  imports: [FormsModule, DecimalPipe, DatePipe],
  templateUrl: './conteos.html',
  styleUrl: './conteos.scss',
})
export class ConteosPage implements OnInit {
  private readonly api = inject(ConteosService);
  private readonly sucursalesApi = inject(SucursalesService);
  private readonly categoriasApi = inject(CategoriasService);
  private readonly auth = inject(AuthService);

  protected readonly estados: EstadoConteo[] = [
    'EnProgreso',
    'Conciliado',
    'Cancelado',
  ];

  protected readonly sucursales = signal<Sucursal[]>([]);
  protected readonly categorias = signal<OpcionCategoria[]>([]);
  protected readonly lista = signal<ConteoLista[]>([]);
  protected readonly detalle = signal<Conteo | null>(null);
  protected readonly capturas = signal<Record<string, number>>({});
  protected readonly cargando = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly filtroEstado = signal<'' | EstadoConteo>('');
  protected readonly nuevaSucursal = signal('');
  protected readonly nuevaCategoria = signal('');

  protected readonly esOperador = computed(() => {
    const rol = this.auth.usuario()?.rol;
    return rol === 'Administrador' || rol === 'EncargadoAlmacen';
  });

  ngOnInit(): void {
    this.sucursalesApi.listar().subscribe({
      next: (s) => {
        this.sucursales.set(s);
        if (s.length > 0) {
          this.nuevaSucursal.set(s[0].id);
        }
      },
    });
    this.categoriasApi.arbol().subscribe({
      next: (arbol) => this.categorias.set(this.aplanar(arbol, 0)),
    });
    this.recargar();
  }

  protected recargar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.api.listar(undefined, this.filtroEstado() || undefined).subscribe({
      next: (l) => {
        this.lista.set(l);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('No se pudieron cargar los conteos.');
        this.cargando.set(false);
      },
    });
  }

  protected iniciar(): void {
    if (!this.nuevaSucursal()) {
      return;
    }
    this.api
      .iniciar(this.nuevaSucursal(), this.nuevaCategoria() || null)
      .subscribe({
        next: (c) => {
          this.abrirDetalle(c);
          this.recargar();
        },
        error: (e) => this.mostrarError(e),
      });
  }

  protected verDetalle(c: ConteoLista): void {
    this.api.obtener(c.id).subscribe({ next: (d) => this.abrirDetalle(d) });
  }

  private abrirDetalle(c: Conteo): void {
    this.detalle.set(c);
    const caps: Record<string, number> = {};
    for (const d of c.detalles) {
      if (d.cantidadContada !== null) {
        caps[d.articuloId] = d.cantidadContada;
      }
    }
    this.capturas.set(caps);
  }

  protected fijarCaptura(articuloId: string, valor: string): void {
    this.capturas.update((c) => ({ ...c, [articuloId]: Number(valor) }));
  }

  protected guardarCaptura(): void {
    const c = this.detalle();
    if (!c) {
      return;
    }
    const caps = this.capturas();
    const renglones = Object.entries(caps)
      .filter(([, v]) => !Number.isNaN(v))
      .map(([articuloId, cantidadContada]) => ({
        articuloId,
        cantidadContada,
      }));
    if (renglones.length === 0) {
      return;
    }
    this.api.capturar(c.id, renglones).subscribe({
      next: (act) => this.abrirDetalle(act),
      error: (e) => this.mostrarError(e),
    });
  }

  protected conciliar(): void {
    const c = this.detalle();
    if (!c) {
      return;
    }
    this.api.conciliar(c.id).subscribe({
      next: (act) => {
        this.abrirDetalle(act);
        this.recargar();
      },
      error: (e) => this.mostrarError(e),
    });
  }

  protected cancelar(): void {
    const c = this.detalle();
    if (!c || !window.confirm('¿Cancelar este conteo?')) {
      return;
    }
    this.api.cancelar(c.id).subscribe({
      next: (act) => {
        this.abrirDetalle(act);
        this.recargar();
      },
      error: (e) => this.mostrarError(e),
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

  private mostrarError(r: {
    error?: { detail?: string; title?: string };
  }): void {
    this.error.set(
      r.error?.detail ?? r.error?.title ?? 'Operación no permitida.',
    );
  }
}
