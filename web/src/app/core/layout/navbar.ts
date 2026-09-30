import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive],
  template: `
    <nav class="navbar navbar-expand navbar-dark bg-dark mb-4">
      <div class="container">
        <a class="navbar-brand" routerLink="/">Gestão de Pedidos</a>
        <ul class="navbar-nav">
          <li class="nav-item">
            <a class="nav-link" routerLink="/pedidos" routerLinkActive="active">Pedidos</a>
          </li>
          <li class="nav-item">
            <a class="nav-link" routerLink="/produtos" routerLinkActive="active">Produtos</a>
          </li>
        </ul>
      </div>
    </nav>
  `,
})
export class Navbar {}
