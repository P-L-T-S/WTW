using System.Text.Json;
using Wtw.Api.Contracts;
using Wtw.Application.Comum;
using Wtw.Domain.Results;

namespace Wtw.Api.Extensions;

internal static class ResultExtensions
{
    public static IResult ParaOk<T>(this Result<T> resultado) =>
        resultado.Success ? TypedResults.Ok(resultado.Data) : resultado.Erro!.ParaProblema();

    public static IResult ParaSemConteudo(this Result resultado) =>
        resultado.Success ? TypedResults.NoContent() : resultado.Erro!.ParaProblema();

    public static IResult ParaCriado(this Result<Guid> resultado, string rotaDeConsulta) =>
        resultado.Success
            ? TypedResults.CreatedAtRoute(new CriadoResponse(resultado.Data), rotaDeConsulta, new { id = resultado.Data })
            : resultado.Erro!.ParaProblema();

    private static IResult ParaProblema(this Erro erro)
    {
        if (erro is ErroValidacao validacao)
        {
            return TypedResults.ValidationProblem(
                validacao.Campos.ToDictionary(campo => ParaCamelCase(campo.Key), campo => campo.Value),
                title: "Dados inválidos");
        }

        var (status, titulo) = erro.Tipo switch
        {
            TipoErro.Validacao => (StatusCodes.Status400BadRequest, "Dados inválidos"),
            TipoErro.NaoEncontrado => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            TipoErro.Conflito => (StatusCodes.Status409Conflict, "Conflito de concorrência"),
            _ => (StatusCodes.Status422UnprocessableEntity, "Regra de negócio violada")
        };

        return TypedResults.Problem(
            detail: erro.Mensagem,
            statusCode: status,
            title: titulo,
            extensions: new Dictionary<string, object?> { ["codigo"] = erro.Metodo });
    }

    private static string ParaCamelCase(string caminho) =>
        string.Join('.', caminho.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
}
