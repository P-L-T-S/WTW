# Wtw — Gestão de Pedidos

Sistema de gestão de pedidos para uma pequena loja, desenvolvido como teste técnico para a vaga de **Desenvolvedor(a) Pleno Full-Stack (C# + Angular)** da WTW.

- **Back-end:** .NET 10, arquitetura em camadas com DDD, EF Core, SQL Server
- **Front-end:** Angular 22 (standalone, zoneless, signals), Bootstrap 5

## Funcionalidades

- Cadastro e listagem de produtos (nome, descrição, preço, estoque)
- Criação de pedidos com seleção de cliente, múltiplos itens e total calculado automaticamente
- Listagem de pedidos com ações de confirmar, cancelar e finalizar
- Controle de estoque via **Domain Events**: baixa ao confirmar, reposição ao cancelar um pedido confirmado

## Arquitetura

```
src/
  Wtw.Domain           Entidades, Value Objects, Agregados, Domain Events, interfaces de repositório
  Wtw.Application      CQRS: commands, queries e seus handlers (MediatR), DTOs, validações, handlers de Domain Events
  Wtw.Infrastructure   EF Core, repositórios, UnitOfWork (publica os Domain Events), migrations e seed
  Wtw.Api              Minimal APIs (endpoints por recurso), injeção de dependência, tratamento global de erros, logs
tests/
  Wtw.Domain.Tests
  Wtw.Application.Tests
web/                   Aplicação Angular
docs/specs/            Especificações detalhadas de cada camada
```

A documentação detalhada de cada camada está em [`docs/specs/`](docs/specs/).

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) LTS (22.22+ ou 24.15+) e npm
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

Com o SQL Server rodando:

```bash
dotnet run --project src/Wtw.Api --launch-profile http
```

- API: `http://localhost:5000/api`
- Documentação interativa (Scalar): `http://localhost:5000/scalar/v1`
- Documento OpenAPI: `http://localhost:5000/openapi/v1.json`
- Logs: console, e arquivo JSON em `src/Wtw.Api/logs/wtw-api-AAAAMMDD.json`

Em `Development`, a API aplica as migrations pendentes ao iniciar, então basta o container do banco estar de pé; o banco `WtwPedidos` e os dados iniciais são criados automaticamente. Para desligar, use `Database:AplicarMigrationsNoStartup = false`.

Exemplo de fluxo completo:

```bash
curl -X POST http://localhost:5000/api/pedidos -H "Content-Type: application/json" -d '{"clienteId":"0199a000-0000-7000-8000-000000000001","itens":[{"produtoId":"0199a000-0000-7000-8000-000000000105","quantidade":2}]}'
```

```bash
curl -X POST http://localhost:5000/api/pedidos/{id}/confirmar
```

### 3. Front-end

Requer Node.js 22.22+ ou 24.15+ (LTS). Com a API rodando em `http://localhost:5000`:

```bash
cd web
npm install
npm start
```

Acesse `http://localhost:4200`. O endereço da API fica em `web/src/environments/environment.ts`.

Roteiro rápido para ver as regras de negócio pela interface:

1. **Pedidos → Novo pedido**: escolha um cliente, adicione 2 × Webcam HD (estoque 3) e tente adicionar mais 2. O aviso local de estoque aparece.
2. Crie o pedido e clique em **Confirmar**: o estoque da webcam cai para 1.
3. Crie outro pedido com 1 webcam, depois um terceiro com 1 webcam, e confirme os dois. O segundo é recusado pela API com a mensagem do domínio ("Estoque insuficiente…") e uma referência para localizar a requisição no log.
4. **Cancele** o pedido confirmado: o estoque é restaurado. **Finalize** outro: ele deixa de ter ações.

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

**c) Automático ao iniciar a API**

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

```bash
dotnet test
```

Na raiz do repositório, roda os **97 testes** (xUnit v3 + Shouldly) em poucos segundos, sem banco nem Docker:

| Projeto | Testes | O que cobre |
|---------|--------|-------------|
| `tests/Wtw.Domain.Tests` | 72 | Value Objects; estoque do `Produto`; todas as regras do `Pedido` (RN01–RN05 e RC01–RC08), incluindo a matriz completa de transições de status e os Domain Events disparados |
| `tests/Wtw.Application.Tests` | 25 | Handlers de command e query com repositórios em memória; handlers dos Domain Events de estoque; validators com as mensagens do domínio |

Os nomes dos testes trazem a regra que cobrem (ex.: `Cancelar_PedidoCriado_NaoDeveDispararEvento_RN04`). O projeto usa a Microsoft.Testing.Platform, ativada no `global.json`, que é o runner padrão do `dotnet test` no .NET 10 com xUnit v3.

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
- **CQRS com MediatR:** cada caso de uso é um command (`ICommand`/`ICommandHandler`) ou query (`IQuery`/`IQueryHandler`) com seu próprio handler, organizado em uma pasta por caso de uso. Commands não devolvem dados de leitura (criação devolve só o `Id`), e a API só conhece o `ISender`. MediatR 12.5 (Apache 2.0); o domínio depende só de `MediatR.Contracts`.
- **Validação explícita no handler, não em pipeline behavior:** um behavior genérico precisaria construir `Result<T>.Fail` via reflexão. Com três commands validados, a chamada explícita é mais simples e visível.
- **Handler base para transições de status:** confirmar, cancelar e finalizar compartilham `AlterarStatusPedidoCommandHandler<T>`, e cada handler concreto só declara a transição (`=> pedido.Confirmar()`).
- **Result pattern em vez de exceções:** regras de negócio violadas retornam `Result` com um `Erro` tipado (validação, não encontrado, regra de negócio, conflito), e a API converte esse tipo no status HTTP correspondente.
- **Erros padronizados em ProblemDetails (RFC 7807):** 400 para validação (erros por campo em camelCase), 404 para recurso inexistente, 409 para conflito de concorrência, 422 para regra de negócio. Todas as respostas trazem `traceId`, e as de negócio trazem `codigo` (ex.: `Pedido.Cancelar`) para o front não depender do texto.
- **Minimal APIs:** cada recurso tem um arquivo em `Endpoints/` com `MapGroup` e métodos estáticos de uma linha que montam o command ou a query e chamam o `ISender`. A conversão de `Result` para `IResult` (`ParaOk`, `ParaSemConteudo`, `ParaCriado`) fica num único arquivo de extensões, e os metadados de OpenAPI são declarados em cada rota.
- **Validação toda no FluentValidation:** as mensagens de validação vêm dos validators, em português. Requisições ilegíveis (JSON malformado, enum inválido) não viram exceção: respondem 400 "Requisição inválida" via `UseStatusCodePages`, sem sujar o log de erros.
- **Datas sempre em UTC:** a leitura do EF marca as datas como UTC, e a API as serializa com `Z`.
- **Logs estruturados com Serilog:** console legível para desenvolvimento e arquivo em JSON compacto (`src/Wtw.Api/logs/`) com uma linha por requisição HTTP e uma por command/query (duração, tipo e código do erro). O `` de cada evento é o mesmo `traceId` devolvido no `ProblemDetails`, o que liga um erro visto no front ao log. A Application loga via `ILogger`, sem conhecer o Serilog.
- **Transições de status como ações** (`POST /pedidos/{id}/confirmar`), por serem comandos de negócio com efeitos colaterais.
- **Front-end zoneless com signals:** Angular 22 sem `zone.js`. Leituras via `rxResource` (estado de carregamento, erro e recarga prontos), estado local em signals e `computed`. Regras de apresentação, como as ações disponíveis por status, ficam em funções puras em `shared/`, e o pedido em montagem fica numa classe `RascunhoPedido`, fora do componente.
- **Erros tratados num só lugar no front:** um interceptor converte o `ProblemDetails` da API e exibe a notificação. Formulários só aplicam os erros 400 nos próprios campos. A notificação mostra o trace id da requisição, o mesmo do log da API.

## Melhorias futuras

- **Testes de integração:** `WebApplicationFactory` + Testcontainers (SQL Server) cobrindo o fluxo HTTP completo e o `UnitOfWork` real (publicação de eventos, rollback e concorrência). Hoje esse fluxo foi validado manualmente contra SQL Server real.
- **Testes do front-end:** services com `HttpTestingController`; `RascunhoPedido` e `acoesDisponiveis`, que são puros e fáceis de testar.
- **Edição de itens pela API e pela tela:** o domínio já tem `AdicionarItem`, `AlterarQuantidadeItem` e `RemoverItem` (com as regras RC02, RC04 e RC08); falta expor commands, endpoints e tela.
- **Concorrência entre requisições:** devolver o `RowVersion` como `ETag` e exigir `If-Match` nas alterações, para detectar também edições concorrentes feitas em telas abertas há mais tempo (hoje a proteção vale dentro de cada requisição).
- **Outbox para Domain Events:** se o estoque passar a ser outro serviço, trocar a publicação na mesma transação por um outbox com processamento assíncrono.
- **Modelo de leitura separado:** projeções diretas (`Select` no `DbContext` ou Dapper) nas queries, sem materializar agregados.
- **Paginação e ordenação** nas listagens (D3), autenticação (D2) e cadastro de clientes (D1).
- **Containers da API e do front** no `docker-compose.yml`, para subir tudo com um comando.
