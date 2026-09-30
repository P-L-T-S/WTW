import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { STATUS_PEDIDO, StatusPedido } from '../../../../shared/models/pedido.model';
import { StatusPedidoBadge } from '../../../../shared/pedidos/status-pedido-badge';
import { AcoesPedido } from '../../components/acoes-pedido/acoes-pedido';
import { PedidoService } from '../../services/pedido.service';

@Component({
  selector: 'app-pedido-lista',
  imports: [CurrencyPipe, DatePipe, RouterLink, StatusPedidoBadge, AcoesPedido],
  templateUrl: './pedido-lista.html',
})
export class PedidoLista {
  private readonly pedidoService = inject(PedidoService);

  protected readonly statusDisponiveis = STATUS_PEDIDO;
  protected readonly filtroStatus = signal<StatusPedido | undefined>(undefined);

  protected readonly pedidos = rxResource({
    params: () => ({ status: this.filtroStatus() }),
    stream: ({ params }) => this.pedidoService.listar(params.status),
  });

  protected filtrar(valor: string): void {
    this.filtroStatus.set(valor ? (valor as StatusPedido) : undefined);
  }
}
