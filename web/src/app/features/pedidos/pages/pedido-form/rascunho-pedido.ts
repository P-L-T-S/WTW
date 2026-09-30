import { computed, signal } from '@angular/core';
import { Produto } from '../../../../shared/models/produto.model';

export interface ItemRascunho {
  produto: Produto;
  quantidade: number;
}

export class RascunhoPedido {
  private readonly _itens = signal<ItemRascunho[]>([]);

  readonly itens = this._itens.asReadonly();
  readonly vazio = computed(() => this._itens().length === 0);
  readonly total = computed(() =>
    this._itens().reduce((soma, item) => soma + item.produto.preco * item.quantidade, 0),
  );

  adicionar(produto: Produto, quantidade: number): string | null {
    const existente = this._itens().find((item) => item.produto.id === produto.id);
    const quantidadeTotal = (existente?.quantidade ?? 0) + quantidade;

    if (quantidadeTotal > produto.quantidadeEstoque) {
      return `Estoque insuficiente para '${produto.nome}'. Disponível: ${produto.quantidadeEstoque}, no pedido: ${quantidadeTotal}.`;
    }

    this._itens.update((itens) =>
      existente
        ? itens.map((item) => (item === existente ? { ...item, quantidade: quantidadeTotal } : item))
        : [...itens, { produto, quantidade }],
    );

    return null;
  }

  remover(produtoId: string): void {
    this._itens.update((itens) => itens.filter((item) => item.produto.id !== produtoId));
  }
}
