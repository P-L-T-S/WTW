using Wtw.Domain.Results;

namespace Wtw.Domain;

public interface IUnitOfWork
{
    /// <summary>
    /// Publica os eventos de domínio pendentes e persiste todas as alterações em uma única transação.
    /// Retorna falha, sem persistir nada, se um handler de evento rejeitar a operação ou houver conflito de concorrência.
    /// </summary>
    Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default);
}
