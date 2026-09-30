import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificacaoService } from '../notificacoes/notificacao.service';
import { ErroApi } from './erro-api';

export const erroInterceptor: HttpInterceptorFn = (requisicao, proximo) => {
  const notificacoes = inject(NotificacaoService);

  return proximo(requisicao).pipe(
    catchError((resposta: unknown) => {
      if (!(resposta instanceof HttpErrorResponse)) {
        return throwError(() => resposta);
      }

      const erro = ErroApi.de(resposta);

      if (!erro.ehValidacao) {
        notificacoes.erro(erro.titulo, erro.detalhe, erro.traceId);
      }

      return throwError(() => erro);
    }),
  );
};
