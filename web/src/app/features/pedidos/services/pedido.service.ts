import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AcaoPedido } from '../../../shared/pedidos/acoes-pedido';
import { Criado } from '../../../shared/models/criado.model';
import { CriarPedido, Pedido, PedidoResumo, StatusPedido } from '../../../shared/models/pedido.model';

@Injectable({ providedIn: 'root' })
export class PedidoService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/pedidos`;

  listar(status?: StatusPedido): Observable<PedidoResumo[]> {
    const params = status ? new HttpParams().set('status', status) : undefined;
    return this.http.get<PedidoResumo[]>(this.url, { params });
  }

  obter(id: string): Observable<Pedido> {
    return this.http.get<Pedido>(`${this.url}/${id}`);
  }

  criar(pedido: CriarPedido): Observable<Criado> {
    return this.http.post<Criado>(this.url, pedido);
  }

  executar(id: string, acao: AcaoPedido): Observable<void> {
    return this.http.post<void>(`${this.url}/${id}/${acao}`, null);
  }
}
