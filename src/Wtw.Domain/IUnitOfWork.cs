namespace Wtw.Domain;

public interface IUnitOfWork
{
    /// <summary>
    /// Despacha os eventos de domínio pendentes e persiste todas as alterações em uma única transação.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
