import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { StatusPedidoBadge } from '../../../../shared/pedidos/status-pedido-badge';
import { AcoesPedido } from '../../components/acoes-pedido/acoes-pedido';
import { PedidoService } from '../../services/pedido.service';

@Component({
  selector: 'app-pedido-detalhe',
  imports: [CurrencyPipe, DatePipe, RouterLink, StatusPedidoBadge, AcoesPedido],
  templateUrl: './pedido-detalhe.html',
})
export class PedidoDetalhe {
  private readonly pedidoService = inject(PedidoService);

  readonly id = input.required<string>();

  protected readonly pedido = rxResource({
    params: () => ({ id: this.id() }),
    stream: ({ params }) => this.pedidoService.obter(params.id),
  });
}
