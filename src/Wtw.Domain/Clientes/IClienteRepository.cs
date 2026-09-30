namespace Wtw.Domain.Clientes;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Cliente>> ObterPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken cancellationToken = default);
}
