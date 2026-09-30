using MediatR;
using Wtw.Application.Comum;
using Wtw.Domain.Pedidos.Eventos;
using Wtw.Domain.Produtos;

namespace Wtw.Application.Pedidos.EventHandlers;

internal sealed class BaixarEstoqueAoConfirmarPedidoHandler(IProdutoRepository produtos)
    : INotificationHandler<PedidoConfirmadoDomainEvent>
{
    public async Task Handle(PedidoConfirmadoDomainEvent evento, CancellationToken cancellationToken)
    {
        var porId = await produtos.ObterPorIdsDoEventoAsync(evento.Itens, cancellationToken);

        foreach (var item in evento.Itens)
        {
            var resultado = porId[item.ProdutoId].BaixarEstoque(item.Quantidade);

            if (resultado.IsFailure)
            {
                throw new FalhaEventoDominioException(resultado.Erro!);
            }
        }
    }
}
