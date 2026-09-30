using FluentValidation;
using Wtw.Domain.Results;

namespace Wtw.Application.Comum;

internal static class ValidatorExtensions
{
    public static async Task<Result> ValidarAsync<T>(this IValidator<T> validator, T request,
        CancellationToken cancellationToken)
    {
        var resultado = await validator.ValidateAsync(request, cancellationToken);

        if (resultado.IsValid)
        {
            return Result.Ok();
        }

        var campos = resultado.Errors
            .GroupBy(falha => falha.PropertyName)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Select(falha => falha.ErrorMessage).Distinct().ToArray());

        return Result.Fail(new ErroValidacao(campos));
    }
}
