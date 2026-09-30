import { Injectable, signal } from '@angular/core';

export type TipoNotificacao = 'sucesso' | 'erro';

export interface Notificacao {
  id: number;
  tipo: TipoNotificacao;
  titulo: string;
  mensagem: string;
  referencia?: string;
}

const DURACAO_MS = 6000;

@Injectable({ providedIn: 'root' })
export class NotificacaoService {
  private proximoId = 1;
  private readonly _notificacoes = signal<Notificacao[]>([]);

  readonly notificacoes = this._notificacoes.asReadonly();

  sucesso(mensagem: string): void {
    this.adicionar({ tipo: 'sucesso', titulo: 'Sucesso', mensagem });
  }

  erro(titulo: string, mensagem: string, referencia?: string): void {
    this.adicionar({ tipo: 'erro', titulo, mensagem, referencia });
  }

  remover(id: number): void {
    this._notificacoes.update((lista) => lista.filter((notificacao) => notificacao.id !== id));
  }

  private adicionar(notificacao: Omit<Notificacao, 'id'>): void {
    const id = this.proximoId++;
    this._notificacoes.update((lista) => [...lista, { ...notificacao, id }]);
    setTimeout(() => this.remover(id), DURACAO_MS);
  }
}
