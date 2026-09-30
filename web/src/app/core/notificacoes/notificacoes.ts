import { Component, inject } from '@angular/core';
import { NotificacaoService } from './notificacao.service';

@Component({
  selector: 'app-notificacoes',
  template: `
    <div class="toast-container position-fixed top-0 end-0 p-3">
      @for (notificacao of servico.notificacoes(); track notificacao.id) {
        <div
          class="toast show border-0"
          [class.text-bg-success]="notificacao.tipo === 'sucesso'"
          [class.text-bg-danger]="notificacao.tipo === 'erro'"
          role="alert"
          aria-live="assertive"
        >
          <div class="d-flex">
            <div class="toast-body">
              <strong class="d-block">{{ notificacao.titulo }}</strong>
              {{ notificacao.mensagem }}
              @if (notificacao.referencia) {
                <small class="d-block opacity-75 mt-1">Referência: {{ notificacao.referencia }}</small>
              }
            </div>
            <button
              type="button"
              class="btn-close btn-close-white me-2 m-auto"
              aria-label="Fechar"
              (click)="servico.remover(notificacao.id)"
            ></button>
          </div>
        </div>
      }
    </div>
  `,
})
export class Notificacoes {
  protected readonly servico = inject(NotificacaoService);
}
