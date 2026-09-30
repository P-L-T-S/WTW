namespace Wtw.Api.Extensions;

internal static class ProblemDetailsExtensions
{
    private static readonly Dictionary<int, (string Titulo, string Detalhe)> RespostasPadrao = new()
    {
        [StatusCodes.Status400BadRequest] = ("Requisição inválida",
            "Não foi possível ler a requisição. Verifique o formato dos dados enviados."),
        [StatusCodes.Status404NotFound] = ("Recurso não encontrado", "O endereço solicitado não existe."),
        [StatusCodes.Status405MethodNotAllowed] =
            ("Método não permitido", "O método HTTP não é suportado neste endereço.")
    };

    public static IServiceCollection AddProblemDetailsEmPortugues(this IServiceCollection services) =>
        services.AddProblemDetails(detailsOptions => detailsOptions.CustomizeProblemDetails = detailsContext =>
        {
            var problema = detailsContext.ProblemDetails;

            if (problema is HttpValidationProblemDetails)
            {
                problema.Title = "Dados inválidos";
                return;
            }

            if (
                problema.Detail is null
                && problema.Status is { } status
                && RespostasPadrao.TryGetValue(status, out var padrao)
            )
            {
                problema.Title = padrao.Titulo;
                problema.Detail = padrao.Detalhe;
            }
        });
}