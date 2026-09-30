# 02 — Camada de Aplicação (`Wtw.Application`)

Implementa os casos de uso com **CQRS** sobre o MediatR: cada operação de escrita é um **command** e cada leitura é uma **query**, cada uma com seu handler. A camada **não contém regra de negócio**: coordena repositórios, domínio e `IUnitOfWork`, valida a entrada e mapeia para DTOs.

Dependências: `Wtw.Domain`, `MediatR` **12.5.0** (última versão Apache 2.0, compatível com o `MediatR.Contracts` 2.0.1 do domínio), `FluentValidation` e `FluentValidation.DependencyInjectionExtensions` 12.1.

## Abstrações (`Abstractions/Messaging`)

```csharp
public interface ICommand : IRequest<Result>;
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
```

- **Commands alteram estado e não devolvem dados de leitura.** Criação devolve só o `Guid` gerado (`ICommand<Guid>`); as demais devolvem `Result` (`ICommand`).
- **Queries não alteram estado** e devolvem DTOs.
- Todos os handlers retornam `Result`/`Result<T>`: nenhum lança exceção para validação, recurso inexistente ou regra de negócio.
- Quem chama (a API) usa apenas `ISender.Send(...)`. Não há services nem dependência direta dos handlers.

## Estrutura (um diretório por caso de uso)

```
Wtw.Application/
  DependencyInjection.cs                          # AddApplication()
  Abstractions/Messaging/                         # ICommand, ICommandHandler, IQuery, IQueryHandler
  Comum/
    ApplicationErros.cs                           # NaoEncontrado(entidade, id)
    ErroValidacao.cs                              # Erro com falhas agrupadas por campo
    ValidatorExtensions.cs                        # IValidator<T>.ValidarAsync → Result
    FalhaEventoDominioException.cs
  Produtos/
    Comum/
      IDadosProduto.cs                            # campos comuns a Criar e Atualizar
      DadosProdutoValidator.cs                    # regras compartilhadas (Include)
      ValoresProduto.cs                           # cria os VOs a partir de IDadosProduto
    CriarProduto/      CriarProdutoCommand, ...Handler, ...Validator
    AtualizarProduto/  AtualizarProdutoCommand, ...Handler, ...Validator
    ListarProdutos/    ListarProdutosQuery, ...Handler
    ObterProduto/      ObterProdutoQuery, ...Handler
    Dtos/ProdutoDto.cs                            # + ProdutoMapeamento.ToDto()
  Clientes/
    ListarClientes/    ListarClientesQuery, ...Handler
    Dtos/ClienteDto.cs
  Pedidos/
    Comum/AlterarStatusPedidoCommandHandler.cs    # IAlterarStatusPedidoCommand + handler base
    CriarPedido/       CriarPedidoCommand (+ ItemPedidoCommand), ...Handler, ...Validator
    ConfirmarPedido/   ConfirmarPedidoCommand, ...Handler
    CancelarPedido/    CancelarPedidoCommand, ...Handler
    FinalizarPedido/   FinalizarPedidoCommand, ...Handler
    ListarPedidos/     ListarPedidosQuery, ...Handler
    ObterPedido/       ObterPedidoQuery, ...Handler
    Dtos/PedidoDto.cs                             # PedidoDto, ItemPedidoDto, PedidoResumoDto + mapeamento
    EventHandlers/                                # handlers dos Domain Events (INotificationHandler)
```

Commands, queries e DTOs são `public`, porque a API os usa. Handlers, validators e helpers são `internal`.

## Commands

| Command | Retorno | Comportamento |
|---------|---------|---------------|
| `CriarProdutoCommand(Nome, Descricao, Preco, QuantidadeEstoque)` | `Guid` | valida, cria os VOs, `Produto.Criar`, salva |
| `AtualizarProdutoCommand(Id, Nome, Descricao, Preco, QuantidadeEstoque)` | — | valida, carrega, `Atualizar`, salva |
| `CriarPedidoCommand(ClienteId, Itens[ProdutoId, Quantidade])` | `Guid` | valida; carrega o cliente; carrega **todos os produtos de uma vez**; inexistente → `NaoEncontrado`; `Pedido.Criar`; salva |
| `ConfirmarPedidoCommand(PedidoId)` | — | `pedido.Confirmar()`. O handler do evento baixa o estoque na mesma transação. |
| `CancelarPedidoCommand(PedidoId)` | — | `pedido.Cancelar()`. Se estava confirmado, o handler do evento repõe o estoque. |
| `FinalizarPedidoCommand(PedidoId)` | — | `pedido.Finalizar()` |

**Transições de status**: os três commands implementam `IAlterarStatusPedidoCommand` (expõe `PedidoId`), e seus handlers herdam de `AlterarStatusPedidoCommandHandler<TCommand>`. A base faz *carregar → aplicar → salvar* e cada handler concreto implementa só `AlterarStatus(Pedido)` (ex.: `=> pedido.Confirmar()`). O MediatR ignora a classe base, por ser abstrata.

Produto repetido no mesmo `CriarPedidoCommand` **é aceito**: o domínio soma as quantidades (RC04) e valida o total contra o estoque.

Padrão dos handlers de command:

```csharp
var validacao = await validator.ValidarAsync(command, cancellationToken);
if (validacao.IsFailure) return validacao;

var produto = await produtos.ObterPorIdAsync(command.Id, cancellationToken);
if (produto is null) return Result.Fail(ApplicationErros.NaoEncontrado(nameof(Produto), command.Id));

produto.Atualizar(...);

return await unitOfWork.SaveChangesAsync(cancellationToken);   // falha de handler de evento ou concorrência vira Result
```

## Queries

| Query | Retorno |
|-------|---------|
| `ListarProdutosQuery` | `IReadOnlyList<ProdutoDto>` ordenado por nome |
| `ObterProdutoQuery(Id)` | `ProdutoDto` ou `NaoEncontrado` |
| `ListarClientesQuery` | `IReadOnlyList<ClienteDto>` ordenado por nome |
| `ListarPedidosQuery(Status?)` | `IReadOnlyList<PedidoResumoDto>`, mais recentes primeiro; nomes dos clientes em uma única consulta |
| `ObterPedidoQuery(Id)` | `PedidoDto` com itens, ou `NaoEncontrado` |

As queries usam os mesmos repositórios dos commands, cujas listagens usam `AsNoTracking`. Um modelo de leitura separado (projeções diretas no `DbContext` ou Dapper) não se justifica neste volume e fica como melhoria futura.

## Tipos de falha

Convertidos em HTTP pela API ([04-api.md](04-api.md)):

| `Erro.Tipo` | Origem |
|-------------|--------|
| `Validacao` | `ErroValidacao`, com `Campos: { "Nome": [...], "Itens[0].Quantidade": [...] }` |
| `NaoEncontrado` | `ApplicationErros.NaoEncontrado("Pedido", id)` |
| `RegraNegocio` | domínio, inclusive o que foi rejeitado por um handler de evento |
| `Conflito` | concorrência, vindo do `UnitOfWork` |

## Validação de entrada (FluentValidation)

- Um validator por command que recebe dados (`CriarProduto`, `AtualizarProduto`, `CriarPedido`). As regras comuns de produto ficam em `DadosProdutoValidator`, incluído via `Include(...)`.
- Valida o **formato** (obrigatórios, limites, lista não vazia, quantidade > 0). As regras que dependem de estado (estoque, status) continuam no domínio.
- Os validators **reusam os limites e as mensagens do domínio** (`.WithMessage(ProdutoErros.PrecoInvalido.Mensagem)`), sem duplicar textos.
- **A validação é chamada explicitamente no handler** (`validator.ValidarAsync`), e não num `IPipelineBehavior`. Um behavior genérico precisaria construir `Result<T>.Fail` para qualquer `T` via reflexão, ou exigiria alterar o `Result` do domínio. Com poucos commands validados, a chamada explícita é mais simples e deixa visível, em cada handler, que há validação.

## Domain Event handlers

Handlers MediatR (`INotificationHandler<TEvento>`), publicados pelo `UnitOfWork`:

| Handler | Evento | Ação |
|---------|--------|------|
| `BaixarEstoqueAoConfirmarPedidoHandler` | `PedidoConfirmadoDomainEvent` | `produto.BaixarEstoque(qtd)` para cada item |
| `ReporEstoqueAoCancelarPedidoHandler` | `PedidoCanceladoDomainEvent` | `produto.ReporEstoque(qtd)` para cada item |

- Os produtos do evento são carregados em uma única consulta. Um produto inexistente é bug (a FK impede), então lança `InvalidOperationException`.
- Handlers **não chamam `SaveChanges`**: alteram agregados rastreados, e o `UnitOfWork` persiste tudo junto.
- Como `INotificationHandler` retorna `Task`, a única forma de abortar a operação é lançar exceção. A `FalhaEventoDominioException` carrega o `Erro` original. O `UnitOfWork` a captura, descarta as alterações e devolve `Result.Fail(erro)`, então **a exceção não sai da camada de infraestrutura**.

## Registro

`AddApplication()`: `AddMediatR(RegisterServicesFromAssembly)` registra os handlers de command, query e evento (inclusive internos), e `AddValidatorsFromAssembly(includeInternalTypes: true)` registra os validators.

## DTOs

`record`s imutáveis. Mapeamento manual via métodos de extensão internos (`produto.ToDto()`, `pedido.ToDto(nomeCliente)`), sem AutoMapper, porque os mapeamentos são poucos e explícitos. `StatusPedido` é exposto como enum, e a API o serializa como string.

## Verificação

Fluxo validado contra SQL Server real, enviando commands e queries pelo `ISender`: validação por campo, `NaoEncontrado` para cliente/produto/pedido, estoque insuficiente na criação e na confirmação (pedido continua `Criado`, estoque intacto), baixa na confirmação, reposição ao cancelar confirmado, bloqueio de cancelamento de `Finalizado` e filtro por status.
