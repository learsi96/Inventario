import { Route } from '@angular/router';
import { authGuard } from '@inventario/shared-auth';

export const appRoutes: Route[] = [
  {
    path: 'login',
    loadComponent: () =>
      import('./paginas/login/login').then((m) => m.LoginPage),
  },
  {
    path: '',
    loadComponent: () => import('./shell/shell').then((m) => m.Shell),
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'articulos' },
      {
        path: 'sucursales',
        loadComponent: () =>
          import('./paginas/sucursales/sucursales').then(
            (m) => m.SucursalesPage,
          ),
      },
      {
        path: 'categorias',
        loadComponent: () =>
          import('./paginas/categorias/categorias').then(
            (m) => m.CategoriasPage,
          ),
      },
      {
        path: 'articulos',
        loadComponent: () =>
          import('./paginas/articulos/articulos').then((m) => m.ArticulosPage),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
