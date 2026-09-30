using MediatR;
using Wtw.Api.Extensions;
using Wtw.Application.Clientes.Dtos;
using Wtw.Application.Clientes.ListarClientes;

namespace Wtw.Api.Endpoints;

internal static class ClientesEndpoints
{
    public static IEndpointRouteBuilder MapClientesEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/clientes").WithTags("Clientes");

        grupo.MapGet("", ListarAsync)
            .WithSummary("Lista os clientes ordenados por nome")
            .Produces<IReadOnlyList<ClienteDto>>();

        return app;
    }

    private static async Task<IResult> ListarAsync(ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ListarClientesQuery(), cancellationToken)).ParaOk();
}
