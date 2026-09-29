namespace Wtw.Domain.Produtos.Eventos;

public class ProdutoCriadoDomainEvent(Guid produtoId) : DomainEvent
{
    public Guid ProdutoId { get; init; } = produtoId;
}