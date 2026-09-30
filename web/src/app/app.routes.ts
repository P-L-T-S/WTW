import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'pedidos' },
  {
    path: 'produtos',
    loadChildren: () => import('./features/produtos/produtos.routes').then((m) => m.PRODUTOS_ROUTES),
  },
  {
    path: 'pedidos',
    loadChildren: () => import('./features/pedidos/pedidos.routes').then((m) => m.PEDIDOS_ROUTES),
  },
  { path: '**', redirectTo: 'pedidos' },
];
