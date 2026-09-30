import { CurrencyPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { ProdutoService } from '../../services/produto.service';

@Component({
  selector: 'app-produto-lista',
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './produto-lista.html',
})
export class ProdutoLista {
  private readonly produtoService = inject(ProdutoService);

  protected readonly produtos = rxResource({ stream: () => this.produtoService.listar() });
}
