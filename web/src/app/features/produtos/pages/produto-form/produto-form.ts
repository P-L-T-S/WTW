import { Component, OnInit, inject, input, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { ErroApi } from '../../../../core/http/erro-api';
import { NotificacaoService } from '../../../../core/notificacoes/notificacao.service';
import { aplicarErrosDoServidor, mensagemDeErro } from '../../../../shared/forms/erros-formulario';
import { SalvarProduto } from '../../../../shared/models/produto.model';
import { ProdutoService } from '../../services/produto.service';

@Component({
  selector: 'app-produto-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './produto-form.html',
})
export class ProdutoForm implements OnInit {
  private readonly produtoService = inject(ProdutoService);
  private readonly notificacoes = inject(NotificacaoService);
  private readonly router = inject(Router);

  readonly id = input<string>();

  protected readonly salvando = signal(false);
  protected readonly errosGerais = signal<string[]>([]);
  protected readonly mensagemDeErro = mensagemDeErro;

  protected readonly form = inject(NonNullableFormBuilder).group({
    nome: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    descricao: ['', [Validators.maxLength(500)]],
    preco: [0, [Validators.required, Validators.min(0.01)]],
    quantidadeEstoque: [0, [Validators.required, Validators.min(0)]],
  });

  ngOnInit(): void {
    const id = this.id();

    if (id) {
      this.produtoService.obter(id).subscribe((produto) => this.form.setValue({
        nome: produto.nome,
        descricao: produto.descricao,
        preco: produto.preco,
        quantidadeEstoque: produto.quantidadeEstoque,
      }));
    }
  }

  protected salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const id = this.id();
    const produto: SalvarProduto = this.form.getRawValue();
    const requisicao: Observable<unknown> = id
      ? this.produtoService.atualizar(id, produto)
      : this.produtoService.criar(produto);

    this.salvando.set(true);
    this.errosGerais.set([]);

    requisicao.subscribe({
      next: () => {
        this.notificacoes.sucesso(id ? 'Produto atualizado.' : 'Produto cadastrado.');
        this.router.navigate(['/produtos']);
      },
      error: (erro: unknown) => {
        this.salvando.set(false);

        if (erro instanceof ErroApi && erro.ehValidacao) {
          this.errosGerais.set(aplicarErrosDoServidor(this.form, erro.campos));
        }
      },
    });
  }
}
