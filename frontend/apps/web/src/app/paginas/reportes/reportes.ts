import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  ArticulosService,
  CategoriasService,
  ConteosService,
  ReportesService,
  SucursalesService,
} from '@inventario/inventario-data-access';
import type {
  ArticuloLista,
  CategoriaNodo,
  ConteoLista,
  Sucursal,
} from '@inventario/shared-domain';

interface OpcionCategoria {
  readonly id: string;
  readonly etiqueta: string;
}

@Component({
  selector: 'inv-reportes',
  imports: [FormsModule],
  templateUrl: './reportes.html',
  styleUrl: './reportes.scss',
})
export class ReportesPage implements OnInit {
  private readonly reportes = inject(ReportesService);
  private readonly sucursalesApi = inject(SucursalesService);
  private readonly categoriasApi = inject(CategoriasService);
  private readonly articulosApi = inject(ArticulosService);
  private readonly conteosApi = inject(ConteosService);

  protected readonly sucursales = signal<Sucursal[]>([]);
  protected readonly categorias = signal<OpcionCategoria[]>([]);
  protected readonly conteos = signal<ConteoLista[]>([]);
  protected readonly error = signal<string | null>(null);

  // Existencias / valorización
  protected readonly sucursal = signal('');
  protected readonly categoria = signal('');
  protected readonly soloBajoMinimo = signal(false);

  // Kardex
  protected readonly busqueda = signal('');
  protected readonly resultadosBusqueda = signal<ArticuloLista[]>([]);
  protected readonly articulo = signal<ArticuloLista | null>(null);
  protected readonly kardexSucursal = signal('');

  // Diferencias de conteo
  protected readonly conteoId = signal('');

  ngOnInit(): void {
    this.sucursalesApi
      .listar()
      .subscribe({ next: (s) => this.sucursales.set(s) });
    this.categoriasApi.arbol().subscribe({
      next: (a) => this.categorias.set(this.aplanar(a, 0)),
    });
    this.conteosApi.listar(undefined, 'Conciliado').subscribe({
      next: (c) => this.conteos.set(c),
    });
  }

  protected buscarArticulo(): void {
    const t = this.busqueda().trim();
    if (t.length < 2) {
      this.resultadosBusqueda.set([]);
      return;
    }
    this.articulosApi
      .listar({ texto: t, tamano: 8 })
      .subscribe({ next: (r) => this.resultadosBusqueda.set([...r.items]) });
  }

  protected elegirArticulo(a: ArticuloLista): void {
    this.articulo.set(a);
    this.resultadosBusqueda.set([]);
    this.busqueda.set(`${a.sku} — ${a.nombre}`);
  }

  protected async existencias(formato: 'xlsx' | 'pdf'): Promise<void> {
    await this.ejecutar(() =>
      this.reportes.existencias(formato, {
        sucursalId: this.sucursal() || undefined,
        categoriaId: this.categoria() || undefined,
        soloBajoMinimo: this.soloBajoMinimo(),
      }),
    );
  }

  protected async valorizacion(formato: 'xlsx' | 'pdf'): Promise<void> {
    await this.ejecutar(() =>
      this.reportes.valorizacion(formato, this.sucursal() || undefined),
    );
  }

  protected async kardex(formato: 'xlsx' | 'pdf'): Promise<void> {
    const a = this.articulo();
    if (!a) {
      return;
    }
    await this.ejecutar(() =>
      this.reportes.kardex(formato, a.id, this.kardexSucursal() || undefined),
    );
  }

  protected async diferencias(formato: 'xlsx' | 'pdf'): Promise<void> {
    if (!this.conteoId()) {
      return;
    }
    await this.ejecutar(() =>
      this.reportes.diferenciasConteo(formato, this.conteoId()),
    );
  }

  private async ejecutar(accion: () => Promise<void>): Promise<void> {
    this.error.set(null);
    try {
      await accion();
    } catch {
      this.error.set('No se pudo generar el reporte.');
    }
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
