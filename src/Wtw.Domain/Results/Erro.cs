namespace Wtw.Domain.Results;

public class Erro
{
    public Erro(string metodo, string mensagem, string? tipoExcecao = null, string? detalhes = null)
    {
        Metodo = metodo;
        Mensagem = mensagem;
        TipoExcecao = tipoExcecao;
        Detalhes = detalhes;
    }

    public string Metodo { get; init; }
    public string Mensagem { get; init; }
    public string? TipoExcecao { get; init; }
    public string? Detalhes { get; init; }
    public TipoErro Tipo { get; init; } = TipoErro.RegraNegocio;
}
