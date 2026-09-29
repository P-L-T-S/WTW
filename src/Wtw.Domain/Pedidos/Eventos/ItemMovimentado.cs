namespace Wtw.Domain.Pedidos.Eventos;

public sealed record ItemMovimentado(Guid ProdutoId, int Quantidade);
