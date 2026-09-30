using Wtw.Domain.Pedidos.Eventos;
using Wtw.Domain.Produtos;

namespace Wtw.Application.Pedidos.EventHandlers;

internal static class ProdutoRepositoryExtensions
{
    public static async Task<IReadOnlyDictionary<Guid, Produto>> ObterPorIdsDoEventoAsync(
        this IProdutoRepository produtos, IReadOnlyCollection<ItemMovimentado> itens,
        CancellationToken cancellationToken)
    {
        var encontrados = await produtos.ObterPorIdsAsync(itens.Select(item => item.ProdutoId), cancellationToken);
        var porId = encontrados.ToDictionary(produto => produto.Id);

        var inexistente = itens.FirstOrDefault(item => !porId.ContainsKey(item.ProdutoId));

        if (inexistente is not null)
        {
            throw new InvalidOperationException(
                $"Produto {inexistente.ProdutoId} do evento não existe; itens de pedido referenciam produtos por FK.");
        }

        return porId;
    }
}
