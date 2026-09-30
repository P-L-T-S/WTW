using Wtw.Domain.Results;

namespace Wtw.Application.Comum;

public static class ApplicationErros
{
    public static Erro NaoEncontrado(string entidade, Guid id) =>
        new($"{entidade}.NaoEncontrado", $"{entidade} não encontrado.", detalhes: $"Id: {id}")
        {
            Tipo = TipoErro.NaoEncontrado
        };
}
