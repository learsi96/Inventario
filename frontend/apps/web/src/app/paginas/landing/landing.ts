import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

interface ValorProposito {
  readonly titulo: string;
  readonly descripcion: string;
}

interface Caracteristica {
  readonly icono: string;
  readonly titulo: string;
  readonly descripcion: string;
}

interface Modulo {
  readonly numero: string;
  readonly titulo: string;
  readonly descripcion: string;
}

@Component({
  selector: 'inv-landing',
  imports: [RouterLink],
  templateUrl: './landing.html',
  styleUrl: './landing.scss',
})
export class LandingPage {
  protected readonly anio = new Date().getFullYear();

  protected readonly valores: readonly ValorProposito[] = [
    {
      titulo: 'Existencias en tiempo real',
      descripcion:
        'Visibilidad de existencias por sucursal y ubicación, sin hojas de cálculo paralelas.',
    },
    {
      titulo: 'Costeo automático',
      descripcion:
        'Promedio ponderado calculado en cada movimiento, sin ajustes manuales.',
    },
    {
      titulo: 'Trazabilidad completa',
      descripcion:
        'Kardex por artículo: qué se movió, cuándo y a qué costo resultó.',
    },
    {
      titulo: 'Control por rol',
      descripcion:
        'Cada usuario ve y hace solo lo que su rol y sucursal le permiten.',
    },
  ];

  protected readonly caracteristicas: readonly Caracteristica[] = [
    {
      icono: 'layers',
      titulo: 'Multi-partner',
      descripcion:
        'Cada refaccionaria opera con sus propios datos, aislados desde el diseño del sistema.',
    },
    {
      icono: 'chart',
      titulo: 'Costeo promedio ponderado',
      descripcion:
        'El costo de cada artículo se recalcula automáticamente con cada entrada.',
    },
    {
      icono: 'swap',
      titulo: 'Kardex de movimientos',
      descripcion:
        'Entradas, salidas y mermas con folio consecutivo y costo resultante.',
    },
    {
      icono: 'truck',
      titulo: 'Transferencias entre sucursales',
      descripcion:
        'Solicitud, envío y recepción con diferencias visibles si algo no cuadra.',
    },
    {
      icono: 'check',
      titulo: 'Conteos físicos',
      descripcion:
        'Captura incremental por sucursal y conciliación que ajusta el sistema.',
    },
    {
      icono: 'box',
      titulo: 'Reportes exportables',
      descripcion:
        'Existencias, valorización, kardex y diferencias de conteo en Excel y PDF.',
    },
  ];

  protected readonly modulos: readonly Modulo[] = [
    {
      numero: '01',
      titulo: 'Catálogo',
      descripcion:
        'Artículos, categorías y unidades de medida por partner, con código de barras y SKU.',
    },
    {
      numero: '02',
      titulo: 'Existencias',
      descripcion:
        'Existencia por artículo y sucursal, con desglose por ubicación y alertas de mínimo.',
    },
    {
      numero: '03',
      titulo: 'Movimientos',
      descripcion:
        'Entradas, salidas y mermas que actualizan existencia, costo y kardex al instante.',
    },
    {
      numero: '04',
      titulo: 'Transferencias',
      descripcion:
        'Traslados entre sucursales con seguimiento de estado y recepción parcial.',
    },
    {
      numero: '05',
      titulo: 'Conteos',
      descripcion:
        'Conteo físico por sucursal y categoría, con conciliación contra el sistema.',
    },
    {
      numero: '06',
      titulo: 'Reportes',
      descripcion:
        'Exportación a Excel y PDF para análisis fuera del sistema.',
    },
  ];
}
