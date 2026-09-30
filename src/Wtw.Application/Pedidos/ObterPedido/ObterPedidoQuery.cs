using Wtw.Application.Abstractions.Messaging;
using Wtw.Application.Pedidos.Dtos;

namespace Wtw.Application.Pedidos.ObterPedido;

public sealed record ObterPedidoQuery(Guid Id) : IQuery<PedidoDto>;
