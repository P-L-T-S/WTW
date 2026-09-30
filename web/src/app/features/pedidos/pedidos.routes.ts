import { Routes } from '@angular/router';

export const PEDIDOS_ROUTES: Routes = [
  {
    path: '',
    title: 'Pedidos',
    loadComponent: () => import('./pages/pedido-lista/pedido-lista').then((m) => m.PedidoLista),
  },
  {
    path: 'novo',
    title: 'Novo pedido',
    loadComponent: () => import('./pages/pedido-form/pedido-form').then((m) => m.PedidoForm),
  },
  {
    path: ':id',
    title: 'Detalhe do pedido',
    loadComponent: () => import('./pages/pedido-detalhe/pedido-detalhe').then((m) => m.PedidoDetalhe),
  },
];
