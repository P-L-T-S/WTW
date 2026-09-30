import { Component, computed, inject, input, output, signal } from '@angular/core';
import { NotificacaoService } from '../../../../core/notificacoes/notificacao.service';
import { StatusPedido } from '../../../../shared/models/pedido.model';
import { DefinicaoAcao, acoesDisponiveis } from '../../../../shared/pedidos/acoes-pedido';
import { PedidoService } from '../../services/pedido.service';

@Component({
  selector: 'app-acoes-pedido',
  template: `
    <div class="d-flex gap-2 justify-content-end">
      @for (acao of acoes(); track acao.acao) {
        <button
          type="button"
          class="btn btn-sm {{ acao.classe }}"
          [disabled]="executando()"
          (click)="executar(acao)"
        >
          {{ acao.rotulo }}
        </button>
      } @empty {
        <span class="text-muted small">Sem ações</span>
      }
    </div>
  `,
})
export class AcoesPedido {
  private readonly pedidoService = inject(PedidoService);
  private readonly notificacoes = inject(NotificacaoService);

  readonly pedidoId = input.required<string>();
  readonly status = input.required<StatusPedido>();
  readonly alterado = output<void>();

  protected readonly executando = signal(false);
  protected readonly acoes = computed(() => acoesDisponiveis(this.status()));

  protected executar(definicao: DefinicaoAcao): void {
    if (definicao.confirmacao && !confirm(definicao.confirmacao)) {
      return;
    }

    this.executando.set(true);

    this.pedidoService.executar(this.pedidoId(), definicao.acao).subscribe({
      next: () => {
        this.notificacoes.sucesso(definicao.sucesso);
        this.executando.set(false);
        this.alterado.emit();
      },
      error: () => {
        this.executando.set(false);
        this.alterado.emit();
      },
    });
  }
}
