using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Wtw.Application.Comum;
using Wtw.Domain.Results;

namespace Wtw.Application.Abstractions.Behaviors;

internal sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requisicao = typeof(TRequest).Name;
        var inicio = Stopwatch.GetTimestamp();

        var resposta = await next();

        var duracaoMs = Stopwatch.GetElapsedTime(inicio).TotalMilliseconds;

        switch (resposta.Erro)
        {
            case null:
                logger.LogInformation("{Requisicao} concluída em {DuracaoMs:0.0} ms", requisicao, duracaoMs);
                break;

            case ErroValidacao validacao:
                logger.LogWarning("{Requisicao} rejeitada por validação em {DuracaoMs:0.0} ms. Campos: {CamposInvalidos}",
                    requisicao, duracaoMs, validacao.Campos.Keys);
                break;

            case var erro:
                logger.LogWarning("{Requisicao} falhou em {DuracaoMs:0.0} ms: {ErroTipo} {ErroCodigo} - {ErroMensagem}",
                    requisicao, duracaoMs, erro.Tipo, erro.Metodo, erro.Mensagem);
                break;
        }

        return resposta;
    }
}
