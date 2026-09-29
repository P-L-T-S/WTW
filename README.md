# Wtw — Gestão de Pedidos

Sistema de gestão de pedidos para uma pequena loja, desenvolvido como teste técnico para a vaga de **Desenvolvedor(a) Pleno Full-Stack (C# + Angular)** da WTW.

- **Back-end:** .NET 10, arquitetura em camadas com DDD, EF Core, SQL Server
- **Front-end:** Angular, Bootstrap

> 🚧 Em desenvolvimento. As seções marcadas com _(a definir)_ serão completadas conforme as etapas forem implementadas.

## Funcionalidades

- Cadastro e listagem de produtos (nome, descrição, preço, estoque)
- Criação de pedidos com seleção de cliente, múltiplos itens e total calculado automaticamente
- Listagem de pedidos com ações de confirmar, cancelar e finalizar
- Controle de estoque via **Domain Events**: baixa ao confirmar, reposição ao cancelar um pedido confirmado

## Arquitetura

```
src/
  Wtw.Domain           Entidades, Value Objects, Agregados, Domain Events, interfaces de repositório
  Wtw.Application      Casos de uso (services), DTOs, validações, handlers de Domain Events
  Wtw.Infrastructure   EF Core, repositórios, UnitOfWork, dispatcher de eventos, migrations
  Wtw.Api              Controllers, injeção de dependência, tratamento global de erros
tests/
  Wtw.Domain.Tests
  Wtw.Application.Tests
web/                   Aplicação Angular
docs/specs/            Especificações detalhadas de cada camada
```

A documentação detalhada de cada camada está em [`docs/specs/`](docs/specs/).

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (LTS) e npm
- [Docker](https://www.docker.com/) (para o SQL Server) — ou uma instância própria de SQL Server
- (opcional) `dotnet-ef`: `dotnet tool install --global dotnet-ef`

## Como executar

### 1. Banco de dados

_(a definir)_

### 2. Back-end

_(a definir)_

### 3. Front-end

_(a definir)_

## Banco de dados: criação e carga inicial

_(a definir — migrations, script SQL e dados de seed)_

## Testes

_(a definir)_

## Regras de negócio

| # | Regra |
|---|-------|
| RN01 | Um pedido não pode ser criado sem ao menos um item. |
| RN02 | Não é possível adicionar item com quantidade maior que o estoque disponível. |
| RN03 | Ao confirmar um pedido, o estoque dos produtos é reduzido. |
| RN04 | Ao cancelar um pedido confirmado, o estoque é restaurado. |
| RN05 | Um pedido finalizado não pode mais ser alterado ou cancelado. |

As regras complementares definidas durante o desenvolvimento estão em [`docs/specs/00-visao-geral.md`](docs/specs/00-visao-geral.md).

## Decisões técnicas

- **Cliente com seed:** o enunciado pede seleção de cliente, mas não define um cadastro. Foi criado um agregado `Cliente` simples, populado via seed, com endpoint apenas de listagem.
- **Referência entre agregados por Id:** `ItemPedido` guarda `ProdutoId` e uma cópia do nome e do preço unitário no momento da inclusão, para que mudanças de preço não alterem pedidos existentes.
- **Domain Events na mesma transação:** eventos são despachados pelo `UnitOfWork` antes do commit. Se o estoque estiver insuficiente no momento da confirmação, toda a operação é revertida.
- **Estoque validado duas vezes:** ao adicionar o item (feedback imediato) e ao confirmar (o estoque pode ter mudado nesse intervalo).
- **Concorrência otimista:** `RowVersion` em produtos e pedidos; conflitos retornam HTTP 409.
- **MediatR apenas para Domain Events:** versão 12.5 (Apache 2.0). O domínio depende só de `MediatR.Contracts`. Os casos de uso ficam em services simples, sem um handler por comando.
- **Result pattern em vez de exceções:** regras de negócio violadas retornam `Result` com um `Erro` tipado (validação, não encontrado, regra de negócio, conflito), e a API converte esse tipo no status HTTP correspondente.
- **Erros padronizados em ProblemDetails (RFC 7807):** 400 para validação, 404 para recurso inexistente, 422 para regra de negócio, 409 para conflito de concorrência.
- **Transições de status como ações** (`POST /pedidos/{id}/confirmar`), por serem comandos de negócio com efeitos colaterais.

## Melhorias futuras

_(a definir)_
