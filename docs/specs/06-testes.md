# 06 — Testes

xUnit + FluentAssertions (ou `Shouldly`). Padrão **Arrange / Act / Assert**, nomes descritivos em português: `Confirmar_PedidoFinalizado_DeveRetornarFalha`.

## `Wtw.Domain.Tests` (obrigatório — prioridade máxima)

Testes puros, sem mocks nem banco. Cobrir **todas** as regras de [00-visao-geral.md](00-visao-geral.md):

### Value Objects
- Nome vazio / curto / longo → `Result` com falha; nome válido é normalizado (trim).
- Preço ≤ 0 → `Result` com falha.
- Email inválido → `Result` com falha; válido é normalizado em minúsculas.

### Produto
- `BaixarEstoque` reduz; quantidade > estoque → `Result` com falha; quantidade ≤ 0 → `Result` com falha.
- `ReporEstoque` aumenta.

### Pedido
- RN01: criar sem itens → `Result` com falha.
- RN02: item com quantidade > estoque → `Result` com falha.
- RC04: mesmo produto duas vezes soma quantidade; soma > estoque → `Result` com falha.
- RC05: alterar preço do produto após adicionar não muda o subtotal do item.
- `ValorTotal` = soma dos subtotais.
- RC08: remover último item → `Result` com falha.
- RC02: adicionar item em pedido `Confirmado` → `Result` com falha.
- RN03: `Confirmar` muda status e gera `PedidoConfirmadoDomainEvent` com os itens corretos; **não altera estoque do produto**.
- RN04: `Cancelar` pedido confirmado gera `PedidoCanceladoDomainEvent`; cancelar pedido `Criado` **não** gera evento.
- RN05: `Cancelar` / `AdicionarItem` em `Finalizado` → `Result` com falha.
- RC01: matriz de transições (`[Theory]`) — válidas retornam sucesso, inválidas retornam falha.

## `Wtw.Application.Tests`

Foco nos handlers de evento e orquestração, com repositórios fake em memória (sem framework de mock obrigatório):

- `BaixarEstoqueAoConfirmarPedidoHandler` baixa o estoque de cada produto.
- Handler com estoque insuficiente lança `FalhaEventoDominioException` com o `Erro` do domínio.
- `ReporEstoqueAoCancelarPedidoHandler` repõe estoque.
- `PedidoService.CriarAsync` com produto inexistente → falha com `Erro.Tipo = NaoEncontrado`.

## Integração (desejável, se houver tempo)

`WebApplicationFactory` + Testcontainers (SQL Server) cobrindo o fluxo completo: criar produto → criar pedido → confirmar (estoque baixa) → cancelar (estoque volta), e o contrato de erro 422.

## Front-end (opcional)

Testes unitários dos services Angular com `HttpTestingController`.
