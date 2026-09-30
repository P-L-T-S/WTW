using Wtw.Domain.Clientes;
using Wtw.Domain.Comum;
using Wtw.Domain.Pedidos;
using Wtw.Domain.Produtos;

namespace Wtw.Application.Tests.Fakes;

internal static class Dados
{
    public static Produto Produto(string nome = "Webcam HD", decimal preco = 159m, int estoque = 3) =>
        Domain.Produtos.Produto.Criar(
            Nome.Criar(nome).Data,
            Descricao.Criar(null).Data,
            Preco.Criar(preco).Data,
            Estoque.Criar(estoque).Data);

    public static Cliente Cliente(string nome = "Ana Souza", string email = "ana@exemplo.com") =>
        Domain.Clientes.Cliente.Criar(Nome.Criar(nome).Data, Email.Criar(email).Data);

    public static Pedido Pedido(Cliente cliente, params (Produto Produto, int Quantidade)[] itens) =>
        Domain.Pedidos.Pedido.Criar(cliente.Id, itens).Data!;
}
