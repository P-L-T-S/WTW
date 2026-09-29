using Wtw.Domain.Results;

namespace Wtw.Domain.Produtos;

public static class ProdutoErros
{
    public static Erro DescricaoMuitoLonga => new("Descricao.Criar",
        $"A descrição deve ter no máximo {Descricao.TamanhoMaximo} caracteres.")
    {
        Tipo = TipoErro.Validacao
    };

    public static Erro PrecoInvalido => new("Preco.Criar", "O preço deve ser maior que zero.")
    {
        Tipo = TipoErro.Validacao
    };

    public static Erro EstoqueNegativo => new("Estoque.Criar", "A quantidade em estoque não pode ser negativa.")
    {
        Tipo = TipoErro.Validacao
    };

    public static Erro QuantidadeMovimentacaoInvalida => new("Produto.MovimentarEstoque",
        "A quantidade movimentada deve ser maior que zero.");

    public static Erro EstoqueInsuficiente(Produto produto, int quantidadeSolicitada) => new("Produto.BaixarEstoque",
        $"Estoque insuficiente para o produto '{produto.Nome}'. Disponível: {produto.Estoque}, solicitado: {quantidadeSolicitada}.");
}
