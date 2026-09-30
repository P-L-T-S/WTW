# 04 — Camada de API (`Wtw.Api`)

ASP.NET Core com **Controllers**. Responsável por HTTP, DI, middlewares, CORS e documentação OpenAPI. Controllers são finos: montam o command ou a query, enviam via `ISender` (MediatR) e convertem o `Result` em resposta HTTP. Nenhum controller conhece handlers, repositórios ou `DbContext`.

## Estrutura

```
Wtw.Api/
  Contracts/
    SalvarProdutoRequest.cs     # corpo do POST/PUT de produto (o Id do PUT vem da rota)
  Controllers/
    ProdutosController.cs
    ClientesController.cs
    PedidosController.cs
  Extensions/
    ResultExtensions.cs         # Result → IActionResult / ProblemDetails
  Middlewares/
    GlobalExceptionHandler.cs   # IExceptionHandler → ProblemDetails
  Program.cs
  appsettings.json / appsettings.Development.json
```

## Endpoints

Prefixo `/api`. JSON camelCase. Enum `status` serializado como string.

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

## Tratamento de erros

Todas as respostas de erro seguem **RFC 7807 (`ProblemDetails`)**, para o front tratar de forma uniforme.

Os handlers retornam `Result`. Os controllers convertem a falha em resposta HTTP por um único método de extensão (`ToActionResult()` / `ToProblem()` em `Extensions/ResultExtensions.cs`), com base em `Erro.Tipo`:

| `Erro.Tipo` | Status | Corpo |
|-------------|--------|-------|
| `Validacao` | 400 | `ValidationProblemDetails` com `errors` a partir de `ErroValidacao.Campos`, com chaves convertidas para camelCase (`Itens[0].Quantidade` → `itens[0].quantidade`) para casar com o JSON do front. Um `Erro` de validação sem campos (vindo de um VO) vai em `detail`. |
| `NaoEncontrado` | 404 | `detail` com a mensagem |
| `RegraNegocio` | 422 | `title: "Regra de negócio violada"`, `detail` com `Erro.Mensagem` |
| `Conflito` | 409 | `detail` com a mensagem |

O `ProblemDetails` inclui `extensions.codigo = Erro.Metodo`, para o front identificar o erro sem depender do texto.

Falhas de handler de evento (ex.: estoque insuficiente na confirmação) e conflitos de concorrência já chegam como `Result` vindo do `IUnitOfWork` (ver [03-infrastructure.md](03-infrastructure.md)). O `GlobalExceptionHandler` só cuida de exceções inesperadas: responde 500 com mensagem genérica e deixa os detalhes apenas no log (e na resposta, em `Development`).

Exemplo:

```json
{
  "type": "https://tools.ietf.org/html/rfc4918#section-11.2",
  "title": "Regra de negócio violada",
  "status": 422,
  "detail": "Estoque insuficiente para o produto 'Teclado'. Disponível: 3, solicitado: 5.",
  "traceId": "..."
}
```

## Configuração (`Program.cs`)

- `AddApplication()`, `AddInfrastructure(configuration)`
- `AddControllers()` com `JsonStringEnumConverter`
- `AddProblemDetails()` + `AddExceptionHandler<GlobalExceptionHandler>()`
- OpenAPI nativo (`AddOpenApi`) + UI (Scalar ou Swagger UI) em `Development`
- CORS liberado para `http://localhost:4200`
- Aplicar migrations no startup em `Development` (ver [03-infrastructure.md](03-infrastructure.md))
- Porta padrão: `http://localhost:5000`
