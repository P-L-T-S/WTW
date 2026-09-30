using MediatR;
using Wtw.Application.Comum;
using Wtw.Domain.Pedidos.Eventos;
using Wtw.Domain.Produtos;

namespace Wtw.Application.Pedidos.EventHandlers;

internal sealed class ReporEstoqueAoCancelarPedidoHandler(IProdutoRepository produtos)
    : INotificationHandler<PedidoCanceladoDomainEvent>
{
    public async Task Handle(PedidoCanceladoDomainEvent evento, CancellationToken cancellationToken)
    {
        var porId = await produtos.ObterPorIdsDoEventoAsync(evento.Itens, cancellationToken);

        foreach (var item in evento.Itens)
        {
            var resultado = porId[item.ProdutoId].ReporEstoque(item.Quantidade);

            if (resultado.IsFailure)
            {
                throw new FalhaEventoDominioException(resultado.Erro!);
            }
        }
    }
}
