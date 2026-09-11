import { Component, computed, inject, signal } from '@angular/core';
import {
  Router,
  RouterLink,
  RouterLinkActive,
  RouterOutlet,
} from '@angular/router';
import { AuthService } from '@inventario/shared-auth';

interface EnlaceNav {
  readonly ruta: string;
  readonly texto: string;
  readonly icono: string;
  readonly soloAdmin?: boolean;
}

interface GrupoNav {
  readonly titulo: string;
  readonly enlaces: readonly EnlaceNav[];
}

@Component({
  selector: 'inv-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly usuario = this.auth.usuario;
  protected readonly menuAbierto = signal(false);

  /** Paths de iconos (viewBox 0 0 24 24, trazo). */
  protected readonly iconos: Record<string, string> = {
    box: 'M21 8l-9-5-9 5 9 5 9-5zM3 8v8l9 5 9-5V8M12 13v8',
    tag: 'M20.6 13.4L13.4 20.6a2 2 0 01-2.8 0l-7.2-7.2A2 2 0 013 12V5a2 2 0 012-2h7a2 2 0 011.4.6l7.2 7.2a2 2 0 010 2.6zM7.5 7.5h.01',
    layers: 'M12 2l9 5-9 5-9-5 9-5zM3 12l9 5 9-5M3 17l9 5 9-5',
    swap: 'M7 10l-4 4 4 4M3 14h13M17 14l4-4-4-4M21 10H8',
    truck:
      'M1 3h15v13H1zM16 8h4l3 3v5h-7M5.5 20a2.5 2.5 0 100-5 2.5 2.5 0 000 5zM18.5 20a2.5 2.5 0 100-5 2.5 2.5 0 000 5z',
    check:
      'M9 11l3 3L22 4M21 12v7a2 2 0 01-2 2H5a2 2 0 01-2-2V5a2 2 0 012-2h11',
    chart: 'M3 3v18h18M7 14v4M12 9v9M17 5v13',
    store: 'M3 9l1.5-5h15L21 9M4 9v11h16V9M4 9h16M9 20v-6h6v6',
    pin: 'M12 21s-7-6.3-7-11a7 7 0 1114 0c0 4.7-7 11-7 11zM12 12a3 3 0 100-6 3 3 0 000 6z',
    users:
      'M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2M9 11a4 4 0 100-8 4 4 0 000 8zM23 21v-2a4 4 0 00-3-3.87M16 3.13a4 4 0 010 7.75',
  };

  protected readonly esAdmin = computed(
    () => this.auth.usuario()?.rol === 'Administrador',
  );

  protected readonly grupos: readonly GrupoNav[] = [
    {
      titulo: 'Catálogo',
      enlaces: [
        { ruta: 'articulos', texto: 'Artículos', icono: 'box' },
        { ruta: 'categorias', texto: 'Categorías', icono: 'tag' },
      ],
    },
    {
      titulo: 'Inventario',
      enlaces: [
        { ruta: 'existencias', texto: 'Existencias', icono: 'layers' },
        { ruta: 'movimientos', texto: 'Movimientos', icono: 'swap' },
        { ruta: 'transferencias', texto: 'Transferencias', icono: 'truck' },
        { ruta: 'conteos', texto: 'Conteos', icono: 'check' },
      ],
    },
    {
      titulo: 'Análisis',
      enlaces: [{ ruta: 'reportes', texto: 'Reportes', icono: 'chart' }],
    },
    {
      titulo: 'Configuración',
      enlaces: [
        { ruta: 'sucursales', texto: 'Sucursales', icono: 'store' },
        { ruta: 'ubicaciones', texto: 'Ubicaciones', icono: 'pin' },
        {
          ruta: 'usuarios',
          texto: 'Usuarios',
          icono: 'users',
          soloAdmin: true,
        },
      ],
    },
  ];

  protected gruposVisibles(): readonly GrupoNav[] {
    if (this.esAdmin()) {
      return this.grupos;
    }
    return this.grupos.map((g) => ({
      ...g,
      enlaces: g.enlaces.filter((e) => !e.soloAdmin),
    }));
  }

  protected alternarMenu(): void {
    this.menuAbierto.update((v) => !v);
  }

  protected salir(): void {
    this.auth.logout();
    void this.router.navigate(['/login']);
  }
}
