# 03 — Camada de Infraestrutura (`Wtw.Infrastructure`)

Implementa a persistência (EF Core + SQL Server), os repositórios e o UnitOfWork, que publica os Domain Events via MediatR.

Pacotes: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`.

## Estrutura

```
Wtw.Infrastructure/
  Persistence/
    WtwDbContext.cs
    Configurations/          # IEntityTypeConfiguration<T> por agregado
    Converters/              # ValueConverters dos Value Objects
    Repositories/
    UnitOfWork.cs
    Seed/DadosIniciais.cs    # HasData para clientes e produtos de exemplo
    Migrations/
  DependencyInjection.cs     # AddInfrastructure(this IServiceCollection, IConfiguration)
scripts/
  create-database.sql        # gerado via `dotnet ef migrations script --idempotent`
```

## Mapeamento

| Tabela | Detalhes |
|--------|----------|
| `Produtos` | `Nome` nvarchar(100), `Descricao` nvarchar(500), `Preco` decimal(18,2), `Estoque` int, `RowVersion` rowversion (concorrência otimista) |
| `Clientes` | `Nome` nvarchar(100), `Email` nvarchar(200) único |
| `Pedidos` | `ClienteId` FK, `Status` nvarchar(20) (enum como string), `CriadoEmUtc`, `AtualizadoEmUtc`, `RowVersion` |
| `ItensPedido` | `PedidoId` FK (cascade), `ProdutoId` FK (restrict), `NomeProduto` nvarchar(100), `PrecoUnitario` decimal(18,2), `Quantidade` |

- Value Objects (`readonly record struct` com construtor privado) são mapeados como colunas simples via `HasConversion`. A leitura do banco **não passa pela fábrica `Criar`**, porque o dado já foi validado ao ser gravado. Ela usa um conversor em `Persistence/Converters/` que instancia o VO pelo construtor privado. Se o `ComplexProperty` do EF Core ligar o construtor diretamente, pode ser usado no lugar. Escolher na implementação e registrar no README.
- `Pedido.Itens` via backing field `_itens` (`PropertyAccessMode.Field`). `ItemPedido` é entidade dependente de `Pedido`, com FK `PedidoId` como **shadow property** (o domínio não precisa dela).
- `ValorTotal` e `ItemPedido.Subtotal` **não são persistidos** (calculados) e ficam ignorados no mapeamento.
- `AggregateRoot.Eventos` ignorado no mapeamento.
- `RowVersion` como shadow property em `Produto` e `Pedido`: é detalhe de persistência e não entra no domínio.
- Construtores privados sem parâmetros nas entidades são usados apenas pelo EF.

## UnitOfWork e publicação de eventos

```text
SaveChangesAsync(ct):
  inicia transação
  loop:
    eventos = AggregateRoot rastreados com Eventos pendentes
    se vazio → sai
    LimparEventos() nos agregados
    para cada evento: publisher.Publish(evento)   # IPublisher do MediatR; handlers alteram outros agregados rastreados
  dbContext.SaveChangesAsync()
  commit
  (qualquer exceção, inclusive FalhaEventoDominioException → rollback e relança)
```

O loop permite que handlers gerem novos eventos. Tudo acontece em **uma transação** (decisão D4).

O `Publish` do MediatR executa os handlers em sequência (estratégia padrão `ForeachAwaitPublisher`), o que garante a ordem e que a primeira falha interrompa o fluxo.

`DbUpdateConcurrencyException` (dois pedidos confirmando o mesmo produto ao mesmo tempo) é propagada, e a API responde 409.

## Migrations e seed

- Migration inicial `InicialCriacao`.
- Seed (`HasData`) com 3 clientes e ~5 produtos de exemplo, com Ids fixos.
- Em ambiente `Development`, a API aplica as migrations no startup (`Database.MigrateAsync()`), configurável por `Database:AplicarMigrationsNoStartup`.
- Script SQL idempotente versionado em `scripts/create-database.sql`, para quem preferir criar o banco manualmente.

## Configuração

Connection string `ConnectionStrings:Wtw` em `appsettings.Development.json`, apontando para o SQL Server do `docker-compose.yml`:

```
Server=localhost,1433;Database=WtwPedidos;User Id=sa;Password=<definida no compose>;TrustServerCertificate=True
```
