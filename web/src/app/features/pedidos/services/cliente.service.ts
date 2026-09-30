import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Cliente } from '../../../shared/models/cliente.model';

@Injectable({ providedIn: 'root' })
export class ClienteService {
  private readonly http = inject(HttpClient);

  listar(): Observable<Cliente[]> {
    return this.http.get<Cliente[]>(`${environment.apiUrl}/clientes`);
  }
}
