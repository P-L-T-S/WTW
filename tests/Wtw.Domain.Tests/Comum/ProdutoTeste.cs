using Wtw.Domain.Comum;
using Wtw.Domain.Produtos;

namespace Wtw.Domain.Tests.Comum;

internal static class ProdutoTeste
{
    public static Produto Criar(string nome = "Webcam HD", decimal preco = 159m, int estoque = 3) =>
        Produto.Criar(
            Nome.Criar(nome).Data,
            Descricao.Criar(null).Data,
            Preco.Criar(preco).Data,
            Estoque.Criar(estoque).Data);
}
