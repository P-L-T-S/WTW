using MediatR;
using Wtw.Api.Contracts;
using Wtw.Api.Extensions;
using Wtw.Application.Produtos.AtualizarProduto;
using Wtw.Application.Produtos.CriarProduto;
using Wtw.Application.Produtos.Dtos;
using Wtw.Application.Produtos.ListarProdutos;
using Wtw.Application.Produtos.ObterProduto;

namespace Wtw.Api.Endpoints;

internal static class ProdutosEndpoints
{
    private const string RotaObterProduto = "ObterProduto";

    public static IEndpointRouteBuilder MapProdutosEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/produtos").WithTags("Produtos");

        grupo.MapGet("", ListarAsync)
            .WithSummary("Lista os produtos ordenados por nome")
            .Produces<IReadOnlyList<ProdutoDto>>();

        grupo.MapGet("{id:guid}", ObterAsync)
            .WithName(RotaObterProduto)
            .WithSummary("Obtém um produto")
            .Produces<ProdutoDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        grupo.MapPost("", CriarAsync)
            .WithSummary("Cadastra um produto")
            .Produces<CriadoResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        grupo.MapPut("{id:guid}", AtualizarAsync)
            .WithSummary("Atualiza um produto")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<IResult> ListarAsync(ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ListarProdutosQuery(), cancellationToken)).ParaOk();

    private static async Task<IResult> ObterAsync(Guid id, ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ObterProdutoQuery(id), cancellationToken)).ParaOk();

    private static async Task<IResult> CriarAsync(SalvarProdutoRequest request, ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CriarProdutoCommand(request.Nome, request.Descricao, request.Preco, request.QuantidadeEstoque);

        return (await sender.Send(command, cancellationToken)).ParaCriado(RotaObterProduto);
    }

    private static async Task<IResult> AtualizarAsync(Guid id, SalvarProdutoRequest request, ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new AtualizarProdutoCommand(id, request.Nome, request.Descricao, request.Preco, request.QuantidadeEstoque);

        return (await sender.Send(command, cancellationToken)).ParaSemConteudo();
    }
}
