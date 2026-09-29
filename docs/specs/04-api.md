# 04 — Camada de API (`Wtw.Api`)

ASP.NET Core com **Controllers**. Responsável por HTTP, DI, middlewares, CORS e documentação OpenAPI. Controllers são finos: recebem o request, chamam o service e retornam o resultado.

## Estrutura

```
Wtw.Api/
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
| POST | `/api/produtos` | `{ nome, descricao, preco, quantidadeEstoque }` | 201 + `Location` |
| PUT | `/api/produtos/{id}` | idem | 200 `ProdutoDto` |

### Clientes
- `GET /api/clientes` → 200 `ClienteDto[]`

### Pedidos
| Método | Rota | Corpo | Sucesso |
|--------|------|-------|---------|
| GET | `/api/pedidos?status=Criado` | — | 200 `PedidoResumoDto[]` |
| GET | `/api/pedidos/{id}` | — | 200 `PedidoDto` |
| POST | `/api/pedidos` | `{ clienteId, itens: [{ produtoId, quantidade }] }` | 201 + `Location` |
| POST | `/api/pedidos/{id}/confirmar` | — | 200 `PedidoDto` |
| POST | `/api/pedidos/{id}/cancelar` | — | 200 `PedidoDto` |
| POST | `/api/pedidos/{id}/finalizar` | — | 200 `PedidoDto` |

Transições são modeladas como **ações** (`POST .../confirmar`) e não como `PATCH status`, porque representam comandos de negócio com efeitos colaterais.

## Tratamento de erros

Todas as respostas de erro seguem **RFC 7807 (`ProblemDetails`)**, para o front tratar de forma uniforme.

Os services retornam `Result`. Os controllers convertem a falha em resposta HTTP por um único método de extensão (`ToActionResult()` / `ToProblem()` em `Extensions/ResultExtensions.cs`), com base em `Erro.Tipo`:

| `Erro.Tipo` | Status | Corpo |
|-------------|--------|-------|
| `Validacao` | 400 | `ValidationProblemDetails` com `errors: { campo: [mensagens] }` |
| `NaoEncontrado` | 404 | `detail` com a mensagem |
| `RegraNegocio` | 422 | `title: "Regra de negócio violada"`, `detail` com `Erro.Mensagem` |
| `Conflito` | 409 | `detail` com a mensagem |

O `ProblemDetails` inclui `extensions.codigo = Erro.Metodo`, para o front identificar o erro sem depender do texto.

Exceções são tratadas pelo `GlobalExceptionHandler`:

| Exceção | Status |
|---------|--------|
| `FalhaEventoDominioException` | conforme o `Erro.Tipo` que ela carrega (normalmente 422) |
| `DbUpdateConcurrencyException` | 409, `detail: "O registro foi alterado por outra operação. Tente novamente."` |
| Outras | 500, mensagem genérica; detalhes apenas em log (e em `Development`) |

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
