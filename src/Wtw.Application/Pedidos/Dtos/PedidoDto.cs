using Wtw.Domain.Pedidos;

namespace Wtw.Application.Pedidos.Dtos;

public sealed record PedidoDto(
    Guid Id,
    Guid ClienteId,
    string NomeCliente,
    StatusPedido Status,
    DateTime CriadoEmUtc,
    DateTime? AtualizadoEmUtc,
    decimal ValorTotal,
    IReadOnlyList<ItemPedidoDto> Itens);

public sealed record ItemPedidoDto(Guid ProdutoId, string NomeProduto, decimal PrecoUnitario, int Quantidade, decimal Subtotal);

public sealed record PedidoResumoDto(
    Guid Id,
    string NomeCliente,
    StatusPedido Status,
    DateTime CriadoEmUtc,
    int QuantidadeItens,
    decimal ValorTotal);

internal static class PedidoMapeamento
{
    public static PedidoDto ToDto(this Pedido pedido, string nomeCliente) =>
        new(pedido.Id,
            pedido.ClienteId,
            nomeCliente,
            pedido.Status,
            pedido.CriadoEmUtc,
            pedido.AtualizadoEmUtc,
            pedido.ValorTotal,
            pedido.Itens.Select(item => item.ToDto()).ToList());

    public static PedidoResumoDto ToResumoDto(this Pedido pedido, string nomeCliente) =>
        new(pedido.Id,
            nomeCliente,
            pedido.Status,
            pedido.CriadoEmUtc,
            pedido.Itens.Count,
            pedido.ValorTotal);

    private static ItemPedidoDto ToDto(this ItemPedido item) =>
        new(item.ProdutoId, item.NomeProduto, item.PrecoUnitario, item.Quantidade, item.Subtotal);
}
