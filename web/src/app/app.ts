import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from './core/layout/navbar';
import { Notificacoes } from './core/notificacoes/notificacoes';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar, Notificacoes],
  template: `
    <app-navbar />
    <main class="container pb-5">
      <router-outlet />
    </main>
    <app-notificacoes />
  `,
})
export class App {}
