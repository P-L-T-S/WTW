import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Criado } from '../../../shared/models/criado.model';
import { Produto, SalvarProduto } from '../../../shared/models/produto.model';

@Injectable({ providedIn: 'root' })
export class ProdutoService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/produtos`;

  listar(): Observable<Produto[]> {
    return this.http.get<Produto[]>(this.url);
  }

  obter(id: string): Observable<Produto> {
    return this.http.get<Produto>(`${this.url}/${id}`);
  }

  criar(produto: SalvarProduto): Observable<Criado> {
    return this.http.post<Criado>(this.url, produto);
  }

  atualizar(id: string, produto: SalvarProduto): Observable<void> {
    return this.http.put<void>(`${this.url}/${id}`, produto);
  }
}
