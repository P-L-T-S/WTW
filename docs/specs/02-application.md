# 02 — Camada de Aplicação (`Wtw.Application`)

Orquestra os casos de uso: carrega agregados pelos repositórios, chama o comportamento do domínio e persiste via `IUnitOfWork`. **Não contém regra de negócio**, apenas coordenação, validação de entrada e mapeamento para DTOs.

Dependências: `Wtw.Domain`, `MediatR` **12.5.0** (última versão Apache 2.0, compatível com o `MediatR.Contracts` 2.0.1 do domínio), `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`.

## Estrutura

```
Wtw.Application/
  Comum/
    ApplicationErros.cs           # NaoEncontrado(entidade, id), Validacao(...)
    FalhaEventoDominioException.cs
  Produtos/
    IProdutoService.cs / ProdutoService.cs
    Dtos/ (ProdutoDto, SalvarProdutoRequest)
    Validators/
  Clientes/
    IClienteService.cs / ClienteService.cs
    Dtos/ClienteDto
  Pedidos/
    IPedidoService.cs / PedidoService.cs
    Dtos/ (PedidoDto, PedidoResumoDto, ItemPedidoDto, CriarPedidoRequest, ItemPedidoRequest)
    Validators/
    EventHandlers/
      BaixarEstoqueAoConfirmarPedidoHandler.cs
      ReporEstoqueAoCancelarPedidoHandler.cs
  DependencyInjection.cs          # AddApplication(this IServiceCollection)
```

> Um service por agregado (em vez de um handler MediatR por comando) mantém o número de arquivos proporcional ao tamanho do teste. O MediatR é usado **apenas para publicar Domain Events**. Os services dependem de interfaces e têm um método pequeno por caso de uso.

## Contrato dos services

Todos os métodos retornam `Result` / `Result<TDto>` e nunca lançam exceções para regras de negócio ou recurso inexistente. A API converte o `Erro` em HTTP ([04-api.md](04-api.md)).

- Recurso inexistente → `Erro` com `Tipo = NaoEncontrado` (em `ApplicationErros`).
- Request inválido → `Erro` com `Tipo = Validacao` e as falhas por campo em `Detalhes` (ou em uma estrutura própria, se for preciso mais de um campo; ver a seção de validação).

## Casos de uso

### Produtos
| Método | Comportamento |
|--------|---------------|
| `ListarAsync()` | `IReadOnlyList<ProdutoDto>` ordenado por nome |
| `ObterAsync(id)` | `ProdutoDto` ou `NaoEncontrado` |
| `CriarAsync(SalvarProdutoRequest)` | valida o request, cria os VOs (combinando as falhas), `Produto.Criar`, salva, retorna `ProdutoDto` |
| `AtualizarAsync(id, SalvarProdutoRequest)` | carrega, cria os VOs, `Atualizar`, salva |

### Clientes
- `ListarAsync()` → `IReadOnlyList<ClienteDto>` ordenado por nome

### Pedidos
| Método | Comportamento |
|--------|---------------|
| `ListarAsync(StatusPedido? status)` | `PedidoResumoDto` (id, nome do cliente, status, total, qtd de itens, data), mais recentes primeiro. Os nomes dos clientes vêm de uma consulta `IClienteRepository.ObterPorIdsAsync`. |
| `ObterAsync(id)` | `PedidoDto` com itens |
| `CriarAsync(CriarPedidoRequest)` | valida o request; verifica se o cliente existe; carrega **todos os produtos de uma vez** (`ObterPorIdsAsync`); produto inexistente → `NaoEncontrado`; `Pedido.Criar`; salva |
| `ConfirmarAsync(id)` | carrega, `Confirmar()`, salva. O handler baixa o estoque na mesma transação. |
| `CancelarAsync(id)` | carrega, `Cancelar()`, salva |
| `FinalizarAsync(id)` | carrega, `Finalizar()`, salva |

Padrão de cada comando:

```csharp
var pedido = await pedidos.ObterPorIdAsync(id, ct);
if (pedido is null) return Result<PedidoDto>.Fail(ApplicationErros.NaoEncontrado("Pedido", id));

var resultado = pedido.Confirmar();
if (resultado.IsFailure) return Result<PedidoDto>.Fail(resultado.Erro!);

var persistencia = await unitOfWork.SaveChangesAsync(ct);   // falha de handler de evento ou concorrência
if (persistencia.IsFailure) return Result<PedidoDto>.Fail(persistencia.Erro!);

return Result<PedidoDto>.Ok(pedido.ToDto());
```

## Validação de entrada (FluentValidation)

Valida o **formato** do request: campos obrigatórios, limites, lista de itens não vazia, quantidade > 0 e produtos duplicados na mesma request. Regras que dependem de estado continuam no domínio. A duplicação de checagens simples é intencional: dá feedback rápido ao front, e o domínio continua sempre protegido.

Os services chamam `IValidator<T>.ValidateAsync` e, havendo falhas, retornam um `Erro` do tipo `Validacao` com os erros agrupados por campo, que a API expõe como `ValidationProblemDetails`.

## Domain Event handlers

Handlers MediatR (`INotificationHandler<TEvento>`):

```csharp
internal sealed class BaixarEstoqueAoConfirmarPedidoHandler(IProdutoRepository produtos)
    : INotificationHandler<PedidoConfirmadoDomainEvent>
{
    // Carrega os produtos dos itens de uma vez e chama produto.BaixarEstoque(qtd) para cada um.
    // Se algum Result falhar (estoque insuficiente, RC06), lança FalhaEventoDominioException(erro).
}

internal sealed class ReporEstoqueAoCancelarPedidoHandler(IProdutoRepository produtos)
    : INotificationHandler<PedidoCanceladoDomainEvent>
{
    // produto.ReporEstoque(qtd) para cada item.
}
```

- Handlers **não chamam `SaveChanges`**: alteram agregados rastreados, e o `UnitOfWork` persiste tudo junto.
- Como `INotificationHandler` retorna `Task`, a única forma de abortar a operação é lançar exceção. A `FalhaEventoDominioException` (já criada em `Comum/`) carrega o `Erro` original. O `UnitOfWork` a captura, descarta as alterações e devolve `Result.Fail(erro)`, então **a exceção não sai da camada de infraestrutura**.

Registro em `AddApplication()`: `AddMediatR(cfg => cfg.RegisterServicesFromAssembly(...))`, `AddValidatorsFromAssembly(...)` e os services.

## DTOs

`record`s imutáveis. Mapeamento manual via métodos de extensão (`produto.ToDto()`), sem AutoMapper, porque os mapeamentos são poucos e explícitos.
