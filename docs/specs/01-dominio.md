# 01 — Camada de Domínio (`Wtw.Domain`)

Toda regra de negócio de [00-visao-geral.md](00-visao-geral.md) que diz respeito ao estado de um agregado vive aqui. Única dependência externa: `MediatR.Contracts` (apenas a interface `INotification`, sem implementação).

## Estrutura

```
Wtw.Domain/
  Entity.cs                 # Id (Guid v7), igualdade por Id
  AggregateRoot.cs          # Eventos, Disparar(), LimparEventos()
  IDomainEvent.cs           # : INotification — Id, ProcessadoUtc
  DomainEvent.cs            # classe base dos eventos
  IUnitOfWork.cs
  Results/
    Result.cs               # Result e Result<T> (Ok / Fail)
    Erro.cs                 # Metodo, Mensagem, TipoExcecao, Detalhes, Tipo
    TipoErro.cs             # RegraNegocio, Validacao, NaoEncontrado, Conflito
  Comum/
    Nome.cs                 # VO compartilhado por Produto e Cliente
    ComumErros.cs
  Produtos/
    Produto.cs
    Descricao.cs, Preco.cs, Estoque.cs
    ProdutoErros.cs
    IProdutoRepository.cs
    Eventos/ProdutoCriadoDomainEvent.cs
  Clientes/
    Cliente.cs
    Email.cs
    ClienteErros.cs
    IClienteRepository.cs
  Pedidos/
    Pedido.cs
    ItemPedido.cs
    StatusPedido.cs
    PedidoErros.cs
    IPedidoRepository.cs
    Eventos/
      ItemMovimentado.cs
      PedidoConfirmadoDomainEvent.cs
      PedidoCanceladoDomainEvent.cs
```

## Tratamento de falhas: Result pattern

O domínio **não lança exceções para regras de negócio**. Operações que podem falhar retornam `Result` ou `Result<T>`:

```csharp
var resultado = pedido.Confirmar();
if (resultado.IsFailure) return resultado.Erro;   // Erro com Mensagem pronta para o usuário
```

- Cada área tem uma classe estática `XxxErros` com os erros possíveis (propriedades ou métodos quando a mensagem tem parâmetros). Nenhuma mensagem fica espalhada pelos métodos.
- `Erro.Tipo` classifica a falha e é usado pela API para escolher o status HTTP ([04-api.md](04-api.md)). O padrão é `RegraNegocio`. Falhas de formato (VOs) usam `Validacao`.
- `Erro.Metodo` identifica a origem (ex.: `"Pedido.Confirmar"`) e funciona como código do erro.

Exceções ficam reservadas para bugs e falhas de infraestrutura.

## Building blocks

- `Entity`: `Guid Id { get; protected init; }`, gerado no construtor com `Guid.CreateVersion7()` (ordenável, bom para índice clusterizado). `Equals` e `GetHashCode` por tipo + Id.
- `AggregateRoot : Entity`: guarda os eventos (`Eventos`, `Disparar`, `LimparEventos`). Apenas raízes de agregado disparam eventos; `ItemPedido` herda só de `Entity`.
- Entidades têm um construtor privado sem parâmetros, usado apenas pelo EF Core.

## Value Objects

`readonly record struct` com construtor privado e fábrica `Criar(...)` que valida e retorna `Result<T>`. Imutáveis; igualdade por valor.

| VO | Regras |
|----|--------|
| `Nome` | obrigatório, trim, 2–100 caracteres |
| `Descricao` | opcional (nulo vira vazio), trim, máx. 500 caracteres |
| `Preco` | `decimal` > 0, arredondado para 2 casas |
| `Estoque` | `int` ≥ 0; `PossuiDisponivel`, `Baixar`, `Repor` retornam novo `Estoque` |
| `Email` | obrigatório, formato válido, máx. 200, normalizado em minúsculas |

## Agregado `Produto`

```csharp
public static Produto Criar(Nome nome, Descricao descricao, Preco preco, Estoque estoque); // dispara ProdutoCriadoDomainEvent
public void Atualizar(Nome nome, Descricao descricao, Preco preco, Estoque estoque);
public bool PossuiEstoque(int quantidade);
public Result BaixarEstoque(int quantidade);   // falha se quantidade <= 0 ou > estoque
public Result ReporEstoque(int quantidade);    // falha se quantidade <= 0
```

`Criar` e `Atualizar` recebem VOs já válidos, então não falham.

## Agregado `Cliente`

`Nome`, `Email`, fábrica `Criar`. Sem comportamento adicional (decisão D1).

## Agregado `Pedido`

```csharp
public Guid ClienteId { get; }
public StatusPedido Status { get; }
public DateTime CriadoEmUtc { get; }
public DateTime? AtualizadoEmUtc { get; }
public IReadOnlyCollection<ItemPedido> Itens { get; }   // backing field _itens
public decimal ValorTotal { get; }                     // soma dos subtotais, não persistido

public static Result<Pedido> Criar(Guid clienteId, IReadOnlyCollection<(Produto Produto, int Quantidade)>? itens);
public Result AdicionarItem(Produto produto, int quantidade);
public Result AlterarQuantidadeItem(Produto produto, int novaQuantidade);
public Result RemoverItem(Guid produtoId);
public Result Confirmar();
public Result Cancelar();
public Result Finalizar();
```

Outros agregados são referenciados **apenas por Id** (`ClienteId`, `ItemPedido.ProdutoId`). O `Produto` é recebido como parâmetro só para leitura (snapshot de nome e preço e validação de estoque, RN02); o `Pedido` nunca altera o `Produto`.

### Comportamentos e invariantes

| Método | Regras |
|--------|--------|
| `Criar` | `clienteId` não vazio; ≥ 1 item (RN01); cada item passa pela mesma validação de `AdicionarItem`; status `Criado`. |
| `AdicionarItem` | só em `Criado` (RC02); quantidade > 0 (RC07); quantidade total do produto no pedido ≤ estoque (RN02, RC04); guarda snapshot de nome e preço (RC05). |
| `AlterarQuantidadeItem` | só em `Criado`; item precisa existir; > 0; ≤ estoque. |
| `RemoverItem` | só em `Criado`; item precisa existir; não remove o último item (RC08). |
| `Confirmar` | só a partir de `Criado`; muda para `Confirmado`; dispara `PedidoConfirmadoDomainEvent`. **Não** mexe no estoque: a baixa (RN03) é feita pelo handler do evento. |
| `Cancelar` | falha se `Finalizado` (RN05) ou já `Cancelado`; se estava `Confirmado`, dispara `PedidoCanceladoDomainEvent` (RN04); muda para `Cancelado`. |
| `Finalizar` | só a partir de `Confirmado` (RC01). |

Toda operação bem-sucedida que altera o pedido atualiza `AtualizadoEmUtc`.

### `ItemPedido` (entidade filha)

`PedidoId`, `ProdutoId`, `NomeProduto`, `PrecoUnitario`, `Quantidade`, `Subtotal`. O construtor e `AlterarQuantidade` são `internal`: só o agregado `Pedido` modifica seus itens.

### `StatusPedido`

`enum { Criado = 1, Confirmado = 2, Cancelado = 3, Finalizado = 4 }`

## Domain Events

Classes que herdam de `DomainEvent` (que implementa `IDomainEvent : INotification`). Carregam os dados de que o handler precisa para agir sem reler o pedido:

```csharp
public sealed class PedidoConfirmadoDomainEvent(Guid pedidoId, IReadOnlyCollection<ItemMovimentado> itens) : DomainEvent;
public sealed class PedidoCanceladoDomainEvent(Guid pedidoId, IReadOnlyCollection<ItemMovimentado> itens) : DomainEvent;
public sealed record ItemMovimentado(Guid ProdutoId, int Quantidade);
```

Os handlers ficam na camada Application ([02-application.md](02-application.md)).

## Interfaces de repositório

```csharp
public interface IProdutoRepository
{
    Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Produto>> ObterPorIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken ct = default);
    void Adicionar(Produto produto);
}

public interface IClienteRepository
{
    Task<bool> ExisteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Cliente>> ObterPorIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken ct = default);
}

public interface IPedidoRepository
{
    Task<Pedido?> ObterPorIdAsync(Guid id, CancellationToken ct = default);   // inclui itens
    Task<IReadOnlyList<Pedido>> ListarAsync(StatusPedido? status = null, CancellationToken ct = default);
    void Adicionar(Pedido pedido);
}

public interface IUnitOfWork
{
    Task<Result> SaveChangesAsync(CancellationToken ct = default);   // publica eventos e persiste atomicamente; falha em handler ou concorrência → Result.Fail
}
```
