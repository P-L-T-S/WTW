using Wtw.Domain.Pedidos.Eventos;
using Wtw.Domain.Produtos;
using Wtw.Domain.Results;

namespace Wtw.Domain.Pedidos;

public sealed class Pedido : AggregateRoot
{
    private readonly List<ItemPedido> _itens = [];

    private Pedido()
    {
    }

    private Pedido(Guid clienteId)
    {
        ClienteId = clienteId;
        Status = StatusPedido.Criado;
        CriadoEmUtc = DateTime.UtcNow;
    }

    public Guid ClienteId { get; private set; }
    public StatusPedido Status { get; private set; }
    public DateTime CriadoEmUtc { get; private set; }
    public DateTime? AtualizadoEmUtc { get; private set; }
    public IReadOnlyCollection<ItemPedido> Itens => _itens.AsReadOnly();
    public decimal ValorTotal => _itens.Sum(item => item.Subtotal);

    public static Result<Pedido> Criar(Guid clienteId, IReadOnlyCollection<(Produto Produto, int Quantidade)>? itens)
    {
        if (clienteId == Guid.Empty)
        {
            return Result<Pedido>.Fail(PedidoErros.ClienteObrigatorio);
        }

        if (itens is null || itens.Count == 0)
        {
            return Result<Pedido>.Fail(PedidoErros.SemItens);
        }

        var pedido = new Pedido(clienteId);

        foreach (var (produto, quantidade) in itens)
        {
            var resultado = pedido.IncluirItem(produto, quantidade);

            if (resultado.IsFailure)
            {
                return Result<Pedido>.Fail(resultado.Erro!);
            }
        }

        return Result<Pedido>.Ok(pedido);
    }

    public Result AdicionarItem(Produto produto, int quantidade)
    {
        if (Status != StatusPedido.Criado)
        {
            return Result.Fail(PedidoErros.NaoEditavel(Status));
        }

        return RegistrarAlteracao(IncluirItem(produto, quantidade));
    }

    public Result AlterarQuantidadeItem(Produto produto, int novaQuantidade)
    {
        if (Status != StatusPedido.Criado)
        {
            return Result.Fail(PedidoErros.NaoEditavel(Status));
        }

        var item = ObterItem(produto.Id);

        if (item is null)
        {
            return Result.Fail(PedidoErros.ItemNaoEncontrado);
        }

        var validacao = ValidarQuantidade(produto, novaQuantidade);

        if (validacao.IsFailure)
        {
            return validacao;
        }

        item.AlterarQuantidade(novaQuantidade);

        return RegistrarAlteracao(Result.Ok());
    }

    public Result RemoverItem(Guid produtoId)
    {
        if (Status != StatusPedido.Criado)
        {
            return Result.Fail(PedidoErros.NaoEditavel(Status));
        }

        var item = ObterItem(produtoId);

        if (item is null)
        {
            return Result.Fail(PedidoErros.ItemNaoEncontrado);
        }

        if (_itens.Count == 1)
        {
            return Result.Fail(PedidoErros.UltimoItem);
        }

        _itens.Remove(item);

        return RegistrarAlteracao(Result.Ok());
    }

    public Result Confirmar()
    {
        if (Status != StatusPedido.Criado)
        {
            return Result.Fail(PedidoErros.ConfirmacaoInvalida(Status));
        }

        Status = StatusPedido.Confirmado;
        Disparar(new PedidoConfirmadoDomainEvent(Id, ItensMovimentados()));

        return RegistrarAlteracao(Result.Ok());
    }

    public Result Cancelar()
    {
        if (Status == StatusPedido.Finalizado)
        {
            return Result.Fail(PedidoErros.FinalizadoNaoPodeSerCancelado);
        }

        if (Status == StatusPedido.Cancelado)
        {
            return Result.Fail(PedidoErros.JaCancelado);
        }

        if (Status == StatusPedido.Confirmado)
        {
            Disparar(new PedidoCanceladoDomainEvent(Id, ItensMovimentados()));
        }

        Status = StatusPedido.Cancelado;

        return RegistrarAlteracao(Result.Ok());
    }

    public Result Finalizar()
    {
        if (Status != StatusPedido.Confirmado)
        {
            return Result.Fail(PedidoErros.FinalizacaoInvalida(Status));
        }

        Status = StatusPedido.Finalizado;

        return RegistrarAlteracao(Result.Ok());
    }

    private Result IncluirItem(Produto produto, int quantidade)
    {
        if (quantidade <= 0)
        {
            return Result.Fail(PedidoErros.QuantidadeInvalida);
        }

        var itemExistente = ObterItem(produto.Id);
        var quantidadeTotal = (itemExistente?.Quantidade ?? 0) + quantidade;

        var validacao = ValidarQuantidade(produto, quantidadeTotal);

        if (validacao.IsFailure)
        {
            return validacao;
        }

        if (itemExistente is null)
        {
            _itens.Add(new ItemPedido(Id, produto, quantidade));
        }
        else
        {
            itemExistente.AlterarQuantidade(quantidadeTotal);
        }

        return Result.Ok();
    }

    private static Result ValidarQuantidade(Produto produto, int quantidade)
    {
        if (quantidade <= 0)
        {
            return Result.Fail(PedidoErros.QuantidadeInvalida);
        }

        if (!produto.PossuiEstoque(quantidade))
        {
            return Result.Fail(PedidoErros.EstoqueInsuficiente(produto, quantidade));
        }

        return Result.Ok();
    }

    private ItemPedido? ObterItem(Guid produtoId) => _itens.FirstOrDefault(item => item.ProdutoId == produtoId);

    private List<ItemMovimentado> ItensMovimentados() =>
        _itens.Select(item => new ItemMovimentado(item.ProdutoId, item.Quantidade)).ToList();

    private Result RegistrarAlteracao(Result resultado)
    {
        if (resultado.Success)
        {
            AtualizadoEmUtc = DateTime.UtcNow;
        }

        return resultado;
    }
}
