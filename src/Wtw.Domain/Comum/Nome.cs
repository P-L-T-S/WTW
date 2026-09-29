using Wtw.Domain.Results;

namespace Wtw.Domain.Comum;

public readonly record struct Nome
{
    public const int TamanhoMinimo = 2;
    public const int TamanhoMaximo = 100;

    private Nome(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; }

    public static Result<Nome> Criar(string? valor)
    {
        var nome = valor?.Trim();

        if (string.IsNullOrEmpty(nome) || nome.Length < TamanhoMinimo || nome.Length > TamanhoMaximo)
        {
            return Result<Nome>.Fail(ComumErros.NomeInvalido);
        }

        return Result<Nome>.Ok(new Nome(nome));
    }

    public override string ToString() => Valor;
}
