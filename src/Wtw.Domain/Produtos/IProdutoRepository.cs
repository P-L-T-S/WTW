namespace Wtw.Domain.Produtos;

public interface IProdutoRepository
{
    Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Produto>> ObterPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken cancellationToken = default);
    void Adicionar(Produto produto);
}
