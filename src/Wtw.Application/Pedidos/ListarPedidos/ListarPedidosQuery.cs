using Wtw.Application.Abstractions.Messaging;
using Wtw.Application.Pedidos.Dtos;
using Wtw.Domain.Pedidos;

namespace Wtw.Application.Pedidos.ListarPedidos;

public sealed record ListarPedidosQuery(StatusPedido? Status = null) : IQuery<IReadOnlyList<PedidoResumoDto>>;
