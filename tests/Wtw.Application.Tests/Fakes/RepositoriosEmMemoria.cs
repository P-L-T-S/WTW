using Wtw.Domain.Clientes;
using Wtw.Domain.Pedidos;
using Wtw.Domain.Produtos;

namespace Wtw.Application.Tests.Fakes;

internal sealed class ProdutoRepositoryEmMemoria(params Produto[] produtos) : IProdutoRepository
{
    public List<Produto> Produtos { get; } = [.. produtos];

    public Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Produtos.FirstOrDefault(produto => produto.Id == id));

    public Task<IReadOnlyList<Produto>> ObterPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Produto>>(Produtos.Where(produto => ids.Contains(produto.Id)).ToList());

    public Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Produto>>(Produtos);

    public void Adicionar(Produto produto) => Produtos.Add(produto);
}

internal sealed class ClienteRepositoryEmMemoria(params Cliente[] clientes) : IClienteRepository
{
    public Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(clientes.FirstOrDefault(cliente => cliente.Id == id));

    public Task<IReadOnlyList<Cliente>> ObterPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Cliente>>(clientes.Where(cliente => ids.Contains(cliente.Id)).ToList());

    public Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Cliente>>(clientes);
}

internal sealed class PedidoRepositoryEmMemoria(params Pedido[] pedidos) : IPedidoRepository
{
    public List<Pedido> Pedidos { get; } = [.. pedidos];

    public Task<Pedido?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Pedidos.FirstOrDefault(pedido => pedido.Id == id));

    public Task<IReadOnlyList<Pedido>> ListarAsync(StatusPedido? status = null, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Pedido>>(Pedidos.Where(pedido => status is null || pedido.Status == status).ToList());

    public void Adicionar(Pedido pedido) => Pedidos.Add(pedido);
}
