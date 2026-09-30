import { Routes } from '@angular/router';

export const PRODUTOS_ROUTES: Routes = [
  {
    path: '',
    title: 'Produtos',
    loadComponent: () => import('./pages/produto-lista/produto-lista').then((m) => m.ProdutoLista),
  },
  {
    path: 'novo',
    title: 'Novo produto',
    loadComponent: () => import('./pages/produto-form/produto-form').then((m) => m.ProdutoForm),
  },
  {
    path: ':id/editar',
    title: 'Editar produto',
    loadComponent: () => import('./pages/produto-form/produto-form').then((m) => m.ProdutoForm),
  },
];
