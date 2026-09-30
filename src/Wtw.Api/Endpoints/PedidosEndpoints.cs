using MediatR;
using Wtw.Api.Contracts;
using Wtw.Api.Extensions;
using Wtw.Application.Pedidos.CancelarPedido;
using Wtw.Application.Pedidos.ConfirmarPedido;
using Wtw.Application.Pedidos.CriarPedido;
using Wtw.Application.Pedidos.Dtos;
using Wtw.Application.Pedidos.FinalizarPedido;
using Wtw.Application.Pedidos.ListarPedidos;
using Wtw.Application.Pedidos.ObterPedido;
using Wtw.Domain.Pedidos;

namespace Wtw.Api.Endpoints;

internal static class PedidosEndpoints
{
    private const string RotaObterPedido = "ObterPedido";

    public static IEndpointRouteBuilder MapPedidosEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/pedidos").WithTags("Pedidos");

        grupo.MapGet("", ListarAsync)
            .WithSummary("Lista os pedidos, opcionalmente filtrando por status")
            .Produces<IReadOnlyList<PedidoResumoDto>>();

        grupo.MapGet("{id:guid}", ObterAsync)
            .WithName(RotaObterPedido)
            .WithSummary("Obtém um pedido com seus itens")
            .Produces<PedidoDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        grupo.MapPost("", CriarAsync)
            .WithSummary("Cria um pedido")
            .Produces<CriadoResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        grupo.MapPost("{id:guid}/confirmar", ConfirmarAsync)
            .WithSummary("Confirma o pedido e baixa o estoque")
            .ProducesTransicao()
            .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapPost("{id:guid}/cancelar", CancelarAsync)
            .WithSummary("Cancela o pedido e repõe o estoque se ele estava confirmado")
            .ProducesTransicao()
            .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapPost("{id:guid}/finalizar", FinalizarAsync)
            .WithSummary("Finaliza um pedido confirmado")
            .ProducesTransicao();

        return app;
    }

    private static RouteHandlerBuilder ProducesTransicao(this RouteHandlerBuilder rota) => rota
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

    private static async Task<IResult> ListarAsync(StatusPedido? status, ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new ListarPedidosQuery(status), cancellationToken)).ParaOk();

    private static async Task<IResult> ObterAsync(Guid id, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ObterPedidoQuery(id), cancellationToken)).ParaOk();

    private static async Task<IResult> CriarAsync(CriarPedidoCommand command, ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(command, cancellationToken)).ParaCriado(RotaObterPedido);

    private static async Task<IResult> ConfirmarAsync(Guid id, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ConfirmarPedidoCommand(id), cancellationToken)).ParaSemConteudo();

    private static async Task<IResult> CancelarAsync(Guid id, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new CancelarPedidoCommand(id), cancellationToken)).ParaSemConteudo();

    private static async Task<IResult> FinalizarAsync(Guid id, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new FinalizarPedidoCommand(id), cancellationToken)).ParaSemConteudo();
}
