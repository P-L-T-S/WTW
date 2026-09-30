# 04 — Camada de API (`Wtw.Api`)

ASP.NET Core com **Minimal APIs**. Responsável por HTTP, DI, middlewares, CORS e documentação OpenAPI. Endpoints são finos: montam o command ou a query, enviam via `ISender` (MediatR) e convertem o `Result` em `IResult`. Nenhum endpoint conhece handlers, repositórios ou `DbContext`.

Pacotes: `Microsoft.AspNetCore.OpenApi` (documento OpenAPI nativo), `Scalar.AspNetCore` (UI de documentação) e `Serilog.AspNetCore` (logs estruturados).

## Estrutura

```
Wtw.Api/
  Contracts/
    SalvarProdutoRequest.cs      # corpo do POST/PUT de produto (o Id do PUT vem da rota)
    CriadoResponse.cs            # { id } devolvido nos 201
  Endpoints/
    ProdutosEndpoints.cs         # MapProdutosEndpoints(): grupo /api/produtos
    ClientesEndpoints.cs         # MapClientesEndpoints(): grupo /api/clientes
    PedidosEndpoints.cs          # MapPedidosEndpoints(): grupo /api/pedidos
  Extensions/
    ResultExtensions.cs          # Result → IResult (ParaOk, ParaSemConteudo, ParaCriado)
    ProblemDetailsExtensions.cs  # AddProblemDetailsEmPortugues(): títulos/detalhes padrão em português
  Middlewares/
    GlobalExceptionHandler.cs    # IExceptionHandler → 500 em ProblemDetails
  Program.cs
  appsettings.json / appsettings.Development.json
```

Cada arquivo de `Endpoints/` é uma classe estática com um método de extensão `MapXxxEndpoints(this IEndpointRouteBuilder)`. Ele cria um `MapGroup` (prefixo e tag do OpenAPI) e registra as rotas apontando para **métodos estáticos privados** (um por rota), em vez de lambdas inline. Assim o mapeamento fica legível, e cada handler HTTP é uma linha:

```csharp
private static async Task<IResult> ObterAsync(Guid id, ISender sender, CancellationToken cancellationToken) =>
    (await sender.Send(new ObterPedidoQuery(id), cancellationToken)).ParaOk();
```

Os parâmetros são inferidos pelo ASP.NET: rota (`id`), query (`status`), corpo (tipos complexos, como `SalvarProdutoRequest` e `CriarPedidoCommand`) e serviços (`ISender`). O `Program.cs` só encadeia `app.MapProdutosEndpoints().MapClientesEndpoints().MapPedidosEndpoints()`.

Extensões de `ResultExtensions`:

| Extensão | Sucesso | Falha |
|----------|---------|-------|
| `ParaOk()` em `Result<T>` | 200 com o valor | `ProblemDetails` conforme `Erro.Tipo` |
| `ParaSemConteudo()` em `Result` | 204 | idem |
| `ParaCriado(nomeDaRota)` em `Result<Guid>` | 201 + `Location` (via `CreatedAtRoute`, usando a rota nomeada `ObterProduto`/`ObterPedido`) + `{ id }` | idem |

## Endpoints

Prefixo `/api`. JSON camelCase. Enum `status` serializado como string (`ConfigureHttpJsonOptions` + `JsonStringEnumConverter`). O filtro `?status=` aceita os nomes exatos do enum (`Criado`, `Confirmado`, `Cancelado`, `Finalizado`), com diferença entre maiúsculas e minúsculas.

Cada rota declara seus metadados de OpenAPI: `WithSummary(...)`, `Produces<T>(...)`, `ProducesProblem(...)` e `ProducesValidationProblem()`. As transições de pedido compartilham os metadados comuns via `ProducesTransicao()`.

### Produtos
| Método | Rota | Corpo | Sucesso |
|--------|------|-------|---------|
| GET | `/api/produtos` | — | 200 `ProdutoDto[]` |
| GET | `/api/produtos/{id}` | — | 200 `ProdutoDto` |
| POST | `/api/produtos` | `{ nome, descricao, preco, quantidadeEstoque }` → `CriarProdutoCommand` | 201 + `Location` + `{ id }` |
| PUT | `/api/produtos/{id}` | idem → `AtualizarProdutoCommand(id, ...)` | 204 |

### Clientes
- `GET /api/clientes` → 200 `ClienteDto[]`

### Pedidos
| Método | Rota | Corpo | Sucesso |
|--------|------|-------|---------|
| GET | `/api/pedidos?status=Criado` | — | 200 `PedidoResumoDto[]` |
| GET | `/api/pedidos/{id}` | — | 200 `PedidoDto` |
| POST | `/api/pedidos` | `{ clienteId, itens: [{ produtoId, quantidade }] }` (o próprio `CriarPedidoCommand`) | 201 + `Location` + `{ id }` |
| POST | `/api/pedidos/{id}/confirmar` | — | 204 |
| POST | `/api/pedidos/{id}/cancelar` | — | 204 |
| POST | `/api/pedidos/{id}/finalizar` | — | 204 |

Transições são modeladas como **ações** (`POST .../confirmar`) e não como `PATCH status`, porque representam comandos de negócio com efeitos colaterais. Seguindo o CQRS, commands respondem sem corpo (`204`) ou só com o `id` criado (`201`). Quem precisa do estado atualizado faz uma query (`GET`).

Datas (`criadoEmUtc`, `atualizadoEmUtc`) são UTC e serializadas com `Z` (ver [03-infrastructure.md](03-infrastructure.md)).

## Tratamento de erros

Todas as respostas de erro seguem **RFC 7807 (`ProblemDetails`)**, com `traceId`, para o front tratar de forma uniforme.

| Origem | Status | Corpo |
|--------|--------|-------|
| `ErroValidacao` (FluentValidation) | 400 | `HttpValidationProblemDetails`, `title: "Dados inválidos"`, `errors` por campo com chaves em camelCase (`Itens[0].Quantidade` → `itens[0].quantidade`) |
| Requisição ilegível (JSON malformado, enum inválido na query) | 400 | `title: "Requisição inválida"`, `detail` genérico em português |
| Rota inexistente / método não suportado | 404 / 405 | títulos e detalhes em português |
| `Erro.Tipo = Validacao` sem campos (VO) | 400 | `title: "Dados inválidos"`, `detail` com a mensagem |
| `Erro.Tipo = NaoEncontrado` | 404 | `title: "Recurso não encontrado"` |
| `Erro.Tipo = Conflito` | 409 | `title: "Conflito de concorrência"` |
| `Erro.Tipo = RegraNegocio` | 422 | `title: "Regra de negócio violada"` |
| Exceção não tratada | 500 | `title: "Erro interno"`; `detail` com a mensagem da exceção só em `Development` |

- Exceto na validação, o `ProblemDetails` inclui `codigo = Erro.Metodo` (ex.: `"Pedido.Cancelar"`), para o front identificar o erro sem depender do texto.
- As respostas usam `TypedResults.Problem(...)` / `TypedResults.ValidationProblem(...)`, escritas pelo `IProblemDetailsService`, que adiciona `type` e `traceId` e aplica o `CustomizeProblemDetails`.
- **Textos padrão em português num único ponto** (`AddProblemDetailsEmPortugues`): título "Dados inválidos" para toda resposta de validação, e título e detalhe para 400/404/405 que chegam sem `detail`.
- **Requisições ilegíveis não viram exceção.** Em `Development`, minimal APIs lançam `BadHttpRequestException` quando não conseguem ler a requisição (`ThrowOnBadRequest`), e o log de requisições a registraria como `Error`, com stack trace, para algo que é erro do cliente. Com `ThrowOnBadRequest = false`, o framework responde 400 sem corpo, e o `UseStatusCodePages()` preenche o `ProblemDetails`. O mesmo mecanismo cobre 404 de rota inexistente e 405.
- Minimal APIs não aplicam `[Required]` implícito nem validação por atributos. Toda validação de conteúdo fica nos validators do FluentValidation, com as mensagens do domínio.
- Falhas de handler de evento (estoque insuficiente na confirmação) e conflitos de concorrência chegam como `Result` vindo do `IUnitOfWork`. O `GlobalExceptionHandler` só cuida do inesperado e apenas monta a resposta 500. O registro da exceção fica com o log de requisições do Serilog (ver "Logs"), para não duplicar o stack trace.

Exemplo (422):

```json
{
  "type": "https://tools.ietf.org/html/rfc4918#section-11.2",
  "title": "Regra de negócio violada",
  "status": 422,
  "detail": "Estoque insuficiente para o produto 'Webcam HD'. Disponível: 1, solicitado: 2.",
  "traceId": "00-...",
  "codigo": "Produto.BaixarEstoque"
}
```

Exemplo (400):

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Dados inválidos",
  "status": 400,
  "errors": { "itens[0].quantidade": ["A quantidade do item deve ser maior que zero."] },
  "traceId": "00-..."
}
```

## Logs (Serilog)

Logs estruturados com **Serilog**, configurados na seção `Serilog` do `appsettings.json` (`ReadFrom.Configuration`):

| Sink | Formato | Uso |
|------|---------|-----|
| Console | texto: `[HH:mm:ss NÍV] Origem: mensagem` | leitura humana durante o desenvolvimento |
| Arquivo `logs/wtw-api-AAAAMMDD.json` | **JSON compacto** (`CompactJsonFormatter`), um evento por linha, rotação diária, 7 dias retidos | log estruturado para consulta e ferramentas (Seq, Elastic, Loki) |

- O arquivo fica em `src/Wtw.Api/logs/`, porque o `dotnet run` usa a pasta do projeto como diretório de trabalho. A pasta está no `.gitignore`.
- Cada evento carrega as propriedades como campos próprios (`Requisicao`, `DuracaoMs`, `ErroCodigo`, `StatusCode`, `RequestPath`, ...), além de `Aplicacao = "Wtw.Api"`, `SourceContext` e o contexto da requisição (`RequestId`, `ActionName`).
- **Correlação**: todo evento traz `@tr` (trace id da requisição), o mesmo valor que aparece no `traceId` do `ProblemDetails`. Com o `traceId` de um erro mostrado no front, dá para achar no log todos os eventos daquela requisição.
- Níveis: `Information` por padrão (`Debug` em `Development`); `Microsoft.AspNetCore`, `Microsoft.EntityFrameworkCore` e `System.Net.Http` só a partir de `Warning`, para reduzir ruído.

Fontes de eventos:

| Evento | Origem |
|--------|--------|
| Uma linha por requisição HTTP (método, rota, status, duração; exceção anexada em 5xx) | `UseSerilogRequestLogging()` |
| Uma linha por command/query (nome, duração, tipo e código do erro) | `LoggingBehavior` da Application ([02-application.md](02-application.md)) |
| Falha de startup (ex.: banco indisponível ao aplicar migrations) | logger de *bootstrap* + `try/catch` no `Program.cs`, registrado como `Fatal`, com `Log.CloseAndFlushAsync()` no `finally` |

Exceções não tratadas são registradas **uma vez**, pelo log de requisições. O `GlobalExceptionHandler` não loga, para não duplicar o stack trace.

Exemplo de evento no arquivo:

```json
{"@t":"2026-09-30T12:16:19.42Z","@mt":"{Requisicao} falhou em {DuracaoMs:0.0} ms: {ErroTipo} {ErroCodigo} - {ErroMensagem}","@l":"Warning","@tr":"65e1c4f0d401c2d1e132b32f95e3eb4d","Requisicao":"ConfirmarPedidoCommand","DuracaoMs":5.84,"ErroTipo":"RegraNegocio","ErroCodigo":"Pedido.Confirmar","ErroMensagem":"Apenas pedidos com status Criado podem ser confirmados. Status atual: Confirmado.","RequestPath":"/api/pedidos/.../confirmar","Aplicacao":"Wtw.Api"}
```

## Configuração (`Program.cs`)

- Serilog: logger de bootstrap, `AddSerilog(ReadFrom.Configuration + ReadFrom.Services)` e `UseSerilogRequestLogging()`.
- `AddApplication()` e `AddInfrastructure(configuration)`.
- `ConfigureHttpJsonOptions` com `JsonStringEnumConverter` e `RouteHandlerOptions.ThrowOnBadRequest = false`.
- `AddProblemDetailsEmPortugues()` + `AddExceptionHandler<GlobalExceptionHandler>()`.
- Pipeline: `UseExceptionHandler()` → `UseStatusCodePages()` → `UseSerilogRequestLogging()` → OpenAPI/Scalar → `UseCors` → `MapXxxEndpoints()`.
- OpenAPI nativo em `/openapi/v1.json` e UI **Scalar** em `/scalar/v1`, apenas em `Development`.
- CORS: política `Frontend` com origens em `Cors:Origens` (padrão `http://localhost:4200`), expondo o header `Location`.
- Migrations aplicadas no startup quando `Database:AplicarMigrationsNoStartup = true` (ligado em `appsettings.Development.json`, desligado no `appsettings.json`).
- Sem `UseHttpsRedirection` nem `UseAuthorization`: a API roda em HTTP local e não há autenticação (D2).
- Porta padrão: `http://localhost:5000` (perfil `http` do `launchSettings.json`).

## Verificação

Testado via HTTP contra SQL Server real:
- banco criado pelas migrations no startup;
- 201 com `Location` e `{ id }`; 204 nas ações;
- 400 com erros por campo em camelCase e título em português; 400 "Requisição inválida" para JSON malformado e enum inválido, registrado no log só como `Information`; 404 de rota inexistente e 405 em português;
- 404 para produto, cliente e pedido;
- 422 para estoque insuficiente (criação e confirmação) e para cancelar pedido finalizado;
- datas com `Z`;
- preflight CORS de `http://localhost:4200`;
- documento OpenAPI com todas as rotas e UI Scalar respondendo;
- logs: console e arquivo JSON gerados; `@tr` do log igual ao `traceId` da resposta; validação registra só os nomes dos campos; com o banco parado, o 500 é registrado com a exceção e o mesmo trace id.
