import { HttpErrorResponse } from '@angular/common/http';

interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
  codigo?: string;
  traceId?: string;
}

export class ErroApi extends Error {
  private constructor(
    readonly status: number,
    readonly titulo: string,
    readonly detalhe: string,
    readonly campos: Record<string, string[]>,
    readonly codigo?: string,
    readonly traceId?: string,
  ) {
    super(detalhe);
  }

  get ehValidacao(): boolean {
    return Object.keys(this.campos).length > 0;
  }

  static de(resposta: HttpErrorResponse): ErroApi {
    if (resposta.status === 0) {
      return new ErroApi(
        0,
        'API indisponível',
        'Não foi possível conectar à API. Verifique se o back-end está em execução.',
        {},
      );
    }

    const problema: ProblemDetails = typeof resposta.error === 'object' && resposta.error ? resposta.error : {};

    return new ErroApi(
      resposta.status,
      problema.title ?? 'Erro',
      problema.detail ?? problema.title ?? 'Ocorreu um erro inesperado.',
      problema.errors ?? {},
      problema.codigo,
      ErroApi.extrairTraceId(problema.traceId),
    );
  }

  private static extrairTraceId(traceparent?: string): string | undefined {
    const partes = traceparent?.split('-');
    return partes?.length === 4 ? partes[1] : traceparent;
  }
}
