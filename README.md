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
  Wtw.Infrastructure   EF Core, repositórios, UnitOfWork (publica os Domain Events), migrations e seed
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

Suba o SQL Server 2022 local com Docker:

```bash
docker compose up -d
```

O container `wtw-sqlserver` fica disponível em `localhost,1433` (usuário `sa`, senha `Wtw@Dev2026!`, apenas para desenvolvimento). Aguarde o status `healthy` em `docker ps` antes de continuar.

Em seguida, crie o banco usando uma das opções da seção [Banco de dados: criação e carga inicial](#banco-de-dados-criação-e-carga-inicial).

### 2. Back-end

_(a definir)_

### 3. Front-end

_(a definir)_

## Banco de dados: criação e carga inicial

O banco `WtwPedidos` é criado pelas migrations do EF Core, que já incluem os dados iniciais. Há três opções:

**a) Migrations via CLI (recomendado)**

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update --project src/Wtw.Infrastructure
```

Para usar outra instância de SQL Server, defina a variável `WTW_CONNECTION_STRING` antes de rodar o comando.

**b) Script SQL**

Execute [`scripts/create-database.sql`](scripts/create-database.sql) (idempotente, gerado a partir das migrations) em um banco `WtwPedidos` vazio, pelo SSMS, Azure Data Studio ou `sqlcmd`.

**c) Automático ao iniciar a API** _(disponível após a etapa da API)_

Em ambiente `Development`, a API aplica as migrations pendentes no startup.

**Dados iniciais**

| Clientes | Produtos (estoque) |
|----------|--------------------|
| Ana Souza, Bruno Lima, Carla Mendes | Teclado mecânico (15), Mouse sem fio (40), Monitor 24" (8), Headset USB (20), Webcam HD (3) |

A Webcam HD tem estoque baixo de propósito, para facilitar o teste da regra de estoque insuficiente.

Para recriar o banco do zero:

```bash
dotnet ef database drop --force --project src/Wtw.Infrastructure
dotnet ef database update --project src/Wtw.Infrastructure
```

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
- **Domain Events na mesma transação:** o `UnitOfWork` publica os eventos pendentes dos agregados e só então chama um único `SaveChanges`. Como os handlers apenas alteram entidades rastreadas, tudo é gravado atomicamente, sem transação explícita. Se o estoque estiver insuficiente no momento da confirmação, nada é persistido e o pedido continua `Criado`.
- **Value Objects sem acoplamento ao EF:** a conversão de cada VO fica na configuração da entidade (`HasConversion` usando `Valor` e a fábrica `Criar`). O domínio não expõe construtores públicos nem atributos de persistência.
- **`RowVersion` como shadow property:** a coluna de concorrência existe só no modelo do EF, não nas entidades, porque é mecanismo de persistência e não regra de negócio. Já `ItemPedido.PedidoId` é propriedade do domínio, pois o vínculo do item com seu pedido faz parte do modelo.
- **Estoque validado duas vezes:** ao adicionar o item (feedback imediato) e ao confirmar (o estoque pode ter mudado nesse intervalo).
- **Concorrência otimista:** `RowVersion` em produtos e pedidos evita que duas operações simultâneas sobrescrevam uma à outra (ex.: duas confirmações baixando o mesmo estoque). Conflitos retornam HTTP 409.
- **MediatR apenas para Domain Events:** versão 12.5 (Apache 2.0). O domínio depende só de `MediatR.Contracts`. Os casos de uso ficam em services simples, sem um handler por comando.
- **Result pattern em vez de exceções:** regras de negócio violadas retornam `Result` com um `Erro` tipado (validação, não encontrado, regra de negócio, conflito), e a API converte esse tipo no status HTTP correspondente.
- **Erros padronizados em ProblemDetails (RFC 7807):** 400 para validação, 404 para recurso inexistente, 422 para regra de negócio, 409 para conflito de concorrência.
- **Transições de status como ações** (`POST /pedidos/{id}/confirmar`), por serem comandos de negócio com efeitos colaterais.

## Melhorias futuras

_(a definir)_
