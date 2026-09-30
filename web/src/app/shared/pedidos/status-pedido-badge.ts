import { Component, computed, input } from '@angular/core';
import { StatusPedido } from '../models/pedido.model';

const CLASSES: Record<StatusPedido, string> = {
  Criado: 'text-bg-secondary',
  Confirmado: 'text-bg-info',
  Cancelado: 'text-bg-danger',
  Finalizado: 'text-bg-success',
};

@Component({
  selector: 'app-status-pedido-badge',
  template: `<span class="badge" [class]="classe()">{{ status() }}</span>`,
})
export class StatusPedidoBadge {
  readonly status = input.required<StatusPedido>();

  protected readonly classe = computed(() => `badge ${CLASSES[this.status()]}`);
}
