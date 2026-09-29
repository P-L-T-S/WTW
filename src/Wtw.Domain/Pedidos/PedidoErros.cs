using Wtw.Domain.Produtos;
using Wtw.Domain.Results;

namespace Wtw.Domain.Pedidos;

public static class PedidoErros
{
    public static Erro ClienteObrigatorio => new("Pedido.Criar", "O cliente do pedido é obrigatório.")
    {
        Tipo = TipoErro.Validacao
    };

    public static Erro SemItens => new("Pedido.Criar", "O pedido deve conter pelo menos um item.");

    public static Erro QuantidadeInvalida => new("Pedido.Item", "A quantidade do item deve ser maior que zero.")
    {
        Tipo = TipoErro.Validacao
    };

    public static Erro EstoqueInsuficiente(Produto produto, int quantidadeSolicitada) => new("Pedido.Item",
        $"Estoque insuficiente para o produto '{produto.Nome}'. Disponível: {produto.Estoque}, solicitado: {quantidadeSolicitada}.");

    public static Erro ItemNaoEncontrado => new("Pedido.Item", "O produto informado não faz parte do pedido.")
    {
        Tipo = TipoErro.NaoEncontrado
    };

    public static Erro UltimoItem => new("Pedido.RemoverItem",
        "Não é possível remover o último item. O pedido deve conter pelo menos um item.");

    public static Erro NaoEditavel(StatusPedido status) => new("Pedido.Item",
        $"Os itens só podem ser alterados em pedidos com status Criado. Status atual: {status}.");

    public static Erro ConfirmacaoInvalida(StatusPedido status) => new("Pedido.Confirmar",
        $"Apenas pedidos com status Criado podem ser confirmados. Status atual: {status}.");

    public static Erro FinalizacaoInvalida(StatusPedido status) => new("Pedido.Finalizar",
        $"Apenas pedidos com status Confirmado podem ser finalizados. Status atual: {status}.");

    public static Erro FinalizadoNaoPodeSerCancelado => new("Pedido.Cancelar",
        "Um pedido finalizado não pode ser cancelado.");

    public static Erro JaCancelado => new("Pedido.Cancelar", "O pedido já está cancelado.");
}
