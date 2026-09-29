using Microsoft.EntityFrameworkCore;
using Wtw.Domain.Produtos;

namespace Wtw.Infrastructure.Persistence.Repositories;

internal sealed class ProdutoRepository(WtwDbContext context) : IProdutoRepository
{
    public Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Produtos.FirstOrDefaultAsync(produto => produto.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Produto>> ObterPorIdsAsync(IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var idsDistintos = ids.Distinct().ToList();

        return await context.Produtos
            .Where(produto => idsDistintos.Contains(produto.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken cancellationToken = default) =>
        await context.Produtos
            .AsNoTracking()
            .OrderBy(produto => produto.Nome)
            .ToListAsync(cancellationToken);

    public void Adicionar(Produto produto) => context.Produtos.Add(produto);
}
