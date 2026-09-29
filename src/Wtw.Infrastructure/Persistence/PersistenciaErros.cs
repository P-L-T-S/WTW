using Wtw.Domain.Results;

namespace Wtw.Infrastructure.Persistence;

internal static class PersistenciaErros
{
    public static Erro ConflitoDeConcorrencia => new("UnitOfWork.SaveChanges",
        "O registro foi alterado por outra operação. Atualize os dados e tente novamente.")
    {
        Tipo = TipoErro.Conflito
    };
}
