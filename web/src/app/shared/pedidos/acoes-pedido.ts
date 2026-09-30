import { StatusPedido } from '../models/pedido.model';

export type AcaoPedido = 'confirmar' | 'cancelar' | 'finalizar';

export interface DefinicaoAcao {
  acao: AcaoPedido;
  rotulo: string;
  classe: string;
  confirmacao?: string;
  sucesso: string;
}

const DEFINICOES: Record<AcaoPedido, DefinicaoAcao> = {
  confirmar: {
    acao: 'confirmar',
    rotulo: 'Confirmar',
    classe: 'btn-success',
    sucesso: 'Pedido confirmado. O estoque foi baixado.',
  },
  finalizar: {
    acao: 'finalizar',
    rotulo: 'Finalizar',
    classe: 'btn-primary',
    confirmacao: 'Finalizar o pedido? Depois disso ele não poderá mais ser alterado ou cancelado.',
    sucesso: 'Pedido finalizado.',
  },
  cancelar: {
    acao: 'cancelar',
    rotulo: 'Cancelar',
    classe: 'btn-outline-danger',
    confirmacao: 'Cancelar o pedido? Se ele estiver confirmado, o estoque será restaurado.',
    sucesso: 'Pedido cancelado.',
  },
};

const ACOES_POR_STATUS: Record<StatusPedido, AcaoPedido[]> = {
  Criado: ['confirmar', 'cancelar'],
  Confirmado: ['finalizar', 'cancelar'],
  Cancelado: [],
  Finalizado: [],
};

export function acoesDisponiveis(status: StatusPedido): DefinicaoAcao[] {
  return ACOES_POR_STATUS[status].map((acao) => DEFINICOES[acao]);
}
