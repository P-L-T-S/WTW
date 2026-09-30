# 06 — Testes

**xUnit v3** + **Shouldly**, rodando na **Microsoft.Testing.Platform** (`global.json` → `"test": { "runner": "Microsoft.Testing.Platform" }`), que é o modo exigido pelo `dotnet test` do .NET 10 para projetos xUnit v3. Os projetos de teste são executáveis (`OutputType=Exe`) e só referenciam `xunit.v3` e `Shouldly`, sem `Microsoft.NET.Test.Sdk` nem `xunit.runner.visualstudio` (pacotes do VSTest).

Por que **Shouldly** e não FluentAssertions: a partir da v8, o FluentAssertions exige licença comercial para uso profissional; o Shouldly é MIT.

Padrão **Arrange / Act / Assert**, nomes descritivos em português, **com o código da regra coberta** quando houver: `Cancelar_PedidoCriado_NaoDeveDispararEvento_RN04`.

```bash
dotnet test                                     # raiz: roda a solution inteira
dotnet test --project tests/Wtw.Domain.Tests    # um projeto
```

## `Wtw.Domain.Tests` (72 testes)

Testes puros, sem mocks nem banco. Builders em `Comum/`: `ProdutoTeste.Criar(nome, preco, estoque)` e `PedidoTeste.NoStatus(status)`, que coloca um pedido em qualquer status pelos próprios métodos do agregado.

| Arquivo | Cobertura |
|---------|-----------|
| `ValueObjects/ValueObjectsTests.cs` | `Nome` (vazio, curto, longo, trim), `Descricao` (nula → vazia, limite), `Preco` (≤ 0, arredondamento), `Estoque` (negativo, zero), `Email` (inválidos, normalização), igualdade por valor |
| `Produtos/ProdutoTests.cs` | `ProdutoCriadoDomainEvent`; `BaixarEstoque` (reduz, zera, acima do disponível sem alterar, quantidade ≤ 0); `ReporEstoque` |
| `Pedidos/PedidoItensTests.cs` | RN01 (sem itens, lista nula), cliente obrigatório, RN02 (acima do estoque com mensagem exata; estoque exato permitido), RC04 (soma de repetidos, soma acima do estoque, repetidos na criação), RC05 (snapshot de nome e preço), referência por Id, `ValorTotal`, `AlterarQuantidadeItem`, RC08 (último item), RC02/RN05 (itens imutáveis fora de `Criado`), `AtualizadoEmUtc` |
| `Pedidos/PedidoStatusTests.cs` | **RC01: matriz completa 4 status × 3 transições** (`[Theory]` com 12 casos, verificando sucesso/falha e status final); RN03 (evento com os itens corretos e sem mexer no estoque); RN04 (evento só ao cancelar confirmado); RN05 (mensagem específica); transição rejeitada não dispara evento |

## `Wtw.Application.Tests` (25 testes)

Sem framework de mock: fakes em memória em `Fakes/` (`ProdutoRepositoryEmMemoria`, `ClienteRepositoryEmMemoria`, `PedidoRepositoryEmMemoria` e `UnitOfWorkFalso`, que conta as chamadas e pode devolver uma falha configurada). A Application expõe seus tipos internos ao projeto de teste via `InternalsVisibleTo`.

| Arquivo | Cobertura |
|---------|-----------|
| `Pedidos/EventHandlersTests.cs` | baixa de estoque de cada item; estoque insuficiente lança `FalhaEventoDominioException` com o `Erro` do domínio; produto inexistente é bug (`InvalidOperationException`); reposição |
| `Pedidos/CriarPedidoCommandHandlerTests.cs` | sucesso (pedido salvo, total); validação por campo; cliente e produto inexistentes (`NaoEncontrado`); estoque insuficiente (`RegraNegocio`); nenhum desses salva; erro do `UnitOfWork` propagado |
| `Pedidos/AlterarStatusPedidoCommandHandlerTests.cs` | confirmar (status, evento, salva); inexistente e já confirmado não salvam; falha de handler de evento devolvida pelo `UnitOfWork` é propagada; cancelar confirmado dispara reposição; finalizar pedido `Criado` é rejeitado |
| `Produtos/ProdutoCommandHandlersTests.cs` | criar com normalização dos VOs; todos os campos inválidos reportados com as mensagens do domínio; atualizar; atualizar inexistente |
| `Pedidos/PedidoQueriesTests.cs` | listagem com nome do cliente e filtro por status; detalhe com snapshot de preço; detalhe inexistente |
| `Pedidos/CriarPedidoCommandValidatorTests.cs` | erro apontando o campo do item (`Itens[1].Quantidade`); produto repetido aceito (RC04) |

## Verificação de efetividade

Foi feita uma checagem de mutação manual: trocar em `Pedido.Cancelar` a condição do evento de reposição (`Status == Confirmado` → `Status != Cancelado`) faz o teste `Cancelar_PedidoCriado_NaoDeveDispararEvento_RN04` falhar. O código foi restaurado em seguida.

## Integração (não implementado, melhoria futura)

`WebApplicationFactory` + Testcontainers (SQL Server) cobrindo o fluxo HTTP completo: criar produto → criar pedido → confirmar (estoque baixa) → cancelar (estoque volta), além do contrato de erro 422 e do `UnitOfWork` real (publicação de eventos e rollback). Hoje esse fluxo foi validado manualmente contra SQL Server real (ver seções "Verificação" das specs 03 e 04).

## Front-end (não implementado, opcional)

Testes unitários dos services Angular com `HttpTestingController` e de `RascunhoPedido` / `acoesDisponiveis`, que são classes e funções puras.
