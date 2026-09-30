# 03 — Camada de Infraestrutura (`Wtw.Infrastructure`)

Implementa a persistência (EF Core + SQL Server), os repositórios e o UnitOfWork, que publica os Domain Events via MediatR.

Pacotes: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design` (10.0.x). O MediatR chega por referência transitiva da Application.

## Estrutura

```
Wtw.Infrastructure/
  DependencyInjection.cs                 # AddInfrastructure(IConfiguration) e AplicarMigrationsAsync()
  Persistence/
    WtwDbContext.cs                      # DbSets + ApplyConfigurationsFromAssembly
    WtwDbContextDesignTimeFactory.cs     # usado só pelo `dotnet ef`
    UnitOfWork.cs
    PersistenciaErros.cs
    Configurations/                      # IEntityTypeConfiguration<T> por entidade
    Repositories/
    Seed/DadosIniciais.cs
    Migrations/
scripts/
  create-database.sql                    # gerado via `dotnet ef migrations script --idempotent`
docker-compose.yml                       # SQL Server 2022 para desenvolvimento
```

## Mapeamento

| Tabela | Detalhes |
|--------|----------|
| `Produtos` | `Nome` nvarchar(100), `Descricao` nvarchar(500), `Preco` decimal(18,2), `Estoque` int, `RowVersion` rowversion |
| `Clientes` | `Nome` nvarchar(100), `Email` nvarchar(200) único |
| `Pedidos` | `ClienteId` FK (restrict), `Status` nvarchar(20) (enum como string), `CriadoEmUtc`, `AtualizadoEmUtc`, `RowVersion`; índices em `Status` e `CriadoEmUtc` |
| `ItensPedido` | `PedidoId` FK obrigatória (cascade), `ProdutoId` FK (restrict), `NomeProduto` nvarchar(100), `PrecoUnitario` decimal(18,2), `Quantidade`; índice único `(PedidoId, ProdutoId)` |

- **Value Objects**: convertidos em cada `IEntityTypeConfiguration` com `HasConversion(vo => vo.Valor, valor => Vo.Criar(valor).Data)`, junto com tamanho máximo e precisão da coluna. Tudo que diz respeito a uma tabela fica em um único arquivo. A leitura passa pela fábrica `Criar`, que é idempotente para dados já validados (trim, arredondamento, minúsculas). Se o banco tiver um valor inválido gravado por fora da aplicação, o VO fica com o valor padrão.
- **Datas UTC**: `CriadoEmUtc` e `AtualizadoEmUtc` têm conversão de leitura com `DateTime.SpecifyKind(..., Utc)`. O SQL Server `datetime2` não guarda o fuso, e sem isso o EF devolveria `Kind = Unspecified`, serializado sem `Z` e interpretado pelo navegador como horário local.
- **Ids** com `ValueGeneratedNever()`: o domínio gera o Guid. Sem essa configuração, o EF trataria um item novo adicionado a um pedido já existente como registro existente e tentaria um `UPDATE`.
- `Pedido.Itens` usa o backing field `_itens` (`PropertyAccessMode.Field`). A FK é a propriedade de domínio `ItemPedido.PedidoId`, preenchida pelo `Pedido` ao criar o item, o que permite configurar FK e índice com lambdas tipadas.
- `RowVersion` é **shadow property** em `Produto` e `Pedido` (concorrência otimista). Nenhuma regra de negócio lê ou decide com base nela: é mecanismo do banco e não entra no domínio. O SQL Server incrementa a coluna a cada `UPDATE`, e o EF inclui `WHERE RowVersion = <valor lido>` no `UPDATE`. Se outra operação alterou a linha nesse meio-tempo, nenhuma linha é afetada, o EF lança `DbUpdateConcurrencyException` e o UnitOfWork devolve `Erro` do tipo `Conflito` (HTTP 409). Sem isso, duas confirmações simultâneas poderiam ler o mesmo estoque e uma baixa sobrescreveria a outra (*lost update*).
- Ignorados no mapeamento: `ValorTotal`, `ItemPedido.Subtotal` e `AggregateRoot.Eventos`.

## Repositórios

- `ObterPorIdAsync` / `ObterPorIdsAsync` retornam entidades **rastreadas**, para serem alteradas e salvas pelo UnitOfWork.
- `ListarAsync` usa `AsNoTracking()`, porque é só leitura.
- `PedidoRepository` sempre inclui os itens (`Include`), já que o agregado é carregado inteiro.

## UnitOfWork e publicação de eventos

```text
SaveChangesAsync(ct) → Result:
  loop:
    agregados = AggregateRoot rastreados com Eventos pendentes
    se vazio → sai
    LimparEventos() nos agregados
    para cada evento: publisher.Publish(evento)   # handlers alteram outros agregados rastreados
  dbContext.SaveChangesAsync()                    # uma transação implícita grava tudo
  → Result.Ok()

  FalhaEventoDominioException  → ChangeTracker.Clear(), Result.Fail(ex.Erro)
  DbUpdateConcurrencyException → ChangeTracker.Clear(), Result.Fail(PersistenciaErros.ConflitoDeConcorrencia)
```

- **Sem transação explícita.** Os handlers apenas alteram entidades rastreadas no mesmo `DbContext`, e um único `SaveChanges` grava tudo atomicamente. Se um handler falhar, nada chega ao banco (decisão D4).
- O loop permite que handlers alterem outros agregados que disparem novos eventos.
- O `Publish` do MediatR executa os handlers em sequência (`ForeachAwaitPublisher`, padrão), então a primeira falha interrompe o fluxo.
- Falhas esperadas voltam como `Result`: a Application recebe `Result` do `SaveChangesAsync` e não precisa tratar exceção.

Fluxo validado contra SQL Server real: confirmação baixa o estoque; falta de estoque na confirmação reverte o pedido (continua `Criado`); cancelamento repõe o estoque; duas baixas concorrentes no mesmo produto retornam `Conflito`.

## Migrations e seed

- Migration inicial `InicialCriacao`, em `Persistence/Migrations`.
- Seed (`HasData`) com 3 clientes e 5 produtos de Ids fixos, definidos em `Seed/DadosIniciais.cs`. Os valores passam pelas fábricas dos VOs. Um produto (Webcam HD) tem estoque baixo (3), para facilitar o teste da regra de estoque.
- `IServiceProvider.AplicarMigrationsAsync()` aplica as migrations. A API chama esse método no startup em `Development`.
- Script SQL idempotente em `scripts/create-database.sql` (inclui o seed).

Comandos (a factory de design-time dispensa `--startup-project`):

```bash
dotnet ef migrations add <Nome> --project src/Wtw.Infrastructure --output-dir Persistence/Migrations
dotnet ef database update --project src/Wtw.Infrastructure
dotnet ef migrations script --idempotent --project src/Wtw.Infrastructure -o scripts/create-database.sql
```

## Configuração

- Runtime: connection string `ConnectionStrings:Wtw` (em `src/Wtw.Api/appsettings.Development.json`), com `EnableRetryOnFailure()` para falhas transitórias do SQL Server. Isso é compatível porque não há transação explícita (o `UnitOfWork` usa um único `SaveChanges`).
- Design-time: variável `WTW_CONNECTION_STRING`, com fallback para a instância do `docker-compose.yml`:

```
Server=localhost,1433;Database=WtwPedidos;User Id=sa;Password=Wtw@Dev2026!;TrustServerCertificate=True
```

A senha é exclusiva do ambiente local de desenvolvimento.
