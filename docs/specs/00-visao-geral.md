# 00 — Visão geral

## Objetivo

Sistema de **Gestão de Pedidos** para uma pequena loja, com back-end .NET em DDD e front-end Angular.

## Funcionalidades

- **Produtos**: cadastro, edição e listagem (Nome, Descrição, Preço, Quantidade em estoque).
- **Clientes**: listagem para seleção na criação do pedido (ver decisão D1).
- **Pedidos**:
  - criação com cliente e um ou mais itens (produto + quantidade);
  - status: `Criado`, `Confirmado`, `Cancelado`, `Finalizado`;
  - valor total calculado automaticamente;
  - listagem com ações de confirmar, cancelar e finalizar.

## Regras de negócio (definidas pela WTW)

| # | Regra |
|---|-------|
| RN01 | Um pedido não pode ser criado sem ao menos um item. |
| RN02 | Não é possível adicionar item com quantidade maior que o estoque disponível do produto. |
| RN03 | Ao confirmar um pedido, o estoque dos produtos envolvidos é reduzido. |
| RN04 | Ao cancelar um pedido já confirmado, o estoque é restaurado. |
| RN05 | Um pedido finalizado não pode mais ser alterado ou cancelado. |

## Regras complementares (decisões técnicas)

| # | Regra |
|---|-------|
| RC01 | Transições válidas: `Criado → Confirmado`, `Criado → Cancelado`, `Confirmado → Cancelado`, `Confirmado → Finalizado`. Qualquer outra é rejeitada. |
| RC02 | Itens só podem ser adicionados/removidos/alterados enquanto o pedido está `Criado`. |
| RC03 | `Cancelado` é estado final (não pode ser confirmado nem finalizado). |
| RC04 | Adicionar um produto que já está no pedido **soma a quantidade** ao item existente; a quantidade total continua sujeita à RN02. |
| RC05 | O item guarda o **preço unitário no momento da inclusão** (snapshot). Alterar o preço do produto não altera pedidos existentes. |
| RC06 | O estoque é validado ao adicionar o item (RN02) **e novamente ao confirmar**, pois pode ter sido consumido por outro pedido nesse intervalo. Se insuficiente, a confirmação falha inteira (transação). |
| RC07 | Quantidade de item deve ser > 0. Preço de produto deve ser > 0. Estoque não pode ser negativo. |
| RC08 | Um pedido não pode ficar sem itens: remover o último item é rejeitado (consequência de RN01). |
| RC09 | Produtos não são excluídos (evita quebrar histórico de pedidos). Fora de escopo: exclusão/inativação. |

## Decisões de escopo

- **D1 — Cliente**: o enunciado pede "seleção de cliente" mas não define cadastro. Será um agregado simples `Cliente` (Nome, Email) populado via **seed**, com endpoint apenas de listagem. Sem tela de cadastro.
- **D2 — Autenticação**: fora de escopo.
- **D3 — Paginação**: listagens retornam todos os registros (volume pequeno). Pode ser adicionada depois sem quebrar contrato se usar query params opcionais.
- **D4 — Domain Events** são despachados **na mesma transação** do agregado que os gerou (consistência imediata do estoque), e não via mensageria/outbox — suficiente para um monólito.
- **D5 — MediatR para Domain Events**: `MediatR` **12.5.0** (última versão Apache 2.0; da v13 em diante a licença é comercial) é usado apenas para publicar Domain Events. O domínio depende só de `MediatR.Contracts` (`INotification`). Casos de uso ficam em services, não em handlers de comando.
- **D6 — Result pattern**: regras de negócio violadas retornam `Result`/`Erro` em vez de lançar exceções. O `Erro.Tipo` define o status HTTP. A única exceção de negócio é a falha dentro de um handler de evento, que precisa abortar a transação (ver [02-application.md](02-application.md)).
