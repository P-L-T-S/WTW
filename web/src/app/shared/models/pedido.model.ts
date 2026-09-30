export const STATUS_PEDIDO = ['Criado', 'Confirmado', 'Cancelado', 'Finalizado'] as const;

export type StatusPedido = (typeof STATUS_PEDIDO)[number];

export interface PedidoResumo {
  id: string;
  nomeCliente: string;
  status: StatusPedido;
  criadoEmUtc: string;
  quantidadeItens: number;
  valorTotal: number;
}

export interface ItemPedido {
  produtoId: string;
  nomeProduto: string;
  precoUnitario: number;
  quantidade: number;
  subtotal: number;
}

export interface Pedido {
  id: string;
  clienteId: string;
  nomeCliente: string;
  status: StatusPedido;
  criadoEmUtc: string;
  atualizadoEmUtc: string | null;
  valorTotal: number;
  itens: ItemPedido[];
}

export interface CriarPedido {
  clienteId: string;
  itens: { produtoId: string; quantidade: number }[];
}
