import { CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ErroApi } from '../../../../core/http/erro-api';
import { NotificacaoService } from '../../../../core/notificacoes/notificacao.service';
import { ProdutoService } from '../../../produtos/services/produto.service';
import { ClienteService } from '../../services/cliente.service';
import { PedidoService } from '../../services/pedido.service';
import { RascunhoPedido } from './rascunho-pedido';

@Component({
  selector: 'app-pedido-form',
  imports: [CurrencyPipe, ReactiveFormsModule, RouterLink],
  templateUrl: './pedido-form.html',
})
export class PedidoForm {
  private readonly pedidoService = inject(PedidoService);
  private readonly produtoService = inject(ProdutoService);
  private readonly clienteService = inject(ClienteService);
  private readonly notificacoes = inject(NotificacaoService);
  private readonly router = inject(Router);
  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly clientes = rxResource({ stream: () => this.clienteService.listar() });
  protected readonly produtos = rxResource({ stream: () => this.produtoService.listar() });

  protected readonly rascunho = new RascunhoPedido();
  protected readonly erroItem = signal<string | null>(null);
  protected readonly errosGerais = signal<string[]>([]);
  protected readonly salvando = signal(false);

  protected readonly clienteId = this.fb.control('', Validators.required);
  protected readonly novoItem = this.fb.group({
    produtoId: ['', Validators.required],
    quantidade: [1, [Validators.required, Validators.min(1)]],
  });

  protected adicionarItem(): void {
    const { produtoId, quantidade } = this.novoItem.getRawValue();
    const produto = this.produtos.value()?.find((p) => p.id === produtoId);

    if (!produto || this.novoItem.invalid) {
      this.erroItem.set('Selecione um produto e informe uma quantidade maior que zero.');
      return;
    }

    const erro = this.rascunho.adicionar(produto, quantidade);
    this.erroItem.set(erro);

    if (!erro) {
      this.novoItem.reset();
    }
  }

  protected criar(): void {
    if (this.clienteId.invalid || this.rascunho.vazio()) {
      this.clienteId.markAsTouched();
      return;
    }

    this.salvando.set(true);
    this.errosGerais.set([]);

    const pedido = {
      clienteId: this.clienteId.value,
      itens: this.rascunho.itens().map((item) => ({ produtoId: item.produto.id, quantidade: item.quantidade })),
    };

    this.pedidoService.criar(pedido).subscribe({
      next: ({ id }) => {
        this.notificacoes.sucesso('Pedido criado.');
        this.router.navigate(['/pedidos', id]);
      },
      error: (erro: unknown) => {
        this.salvando.set(false);
        this.produtos.reload();

        if (erro instanceof ErroApi && erro.ehValidacao) {
          this.errosGerais.set(Object.values(erro.campos).flat());
        }
      },
    });
  }
}
