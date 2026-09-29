# CLAUDE.md

Instruções para o Claude Code neste repositório.

## O que é este projeto

Teste técnico **WTW — Desenvolvedor(a) Pleno Full-Stack (C# + Angular)**: sistema de **Gestão de Pedidos** para uma pequena loja.
O foco da avaliação é **qualidade de arquitetura, DDD, SOLID, Clean Code e Domain Events** — não design visual.

Antes de implementar qualquer coisa, leia a spec correspondente em [`docs/specs/`](docs/specs/):

| Spec | Conteúdo |
|------|----------|
| [00-visao-geral.md](docs/specs/00-visao-geral.md) | Requisitos do teste, regras de negócio, decisões de escopo |
| [01-dominio.md](docs/specs/01-dominio.md) | Agregados, Value Objects, Domain Events, invariantes |
| [02-application.md](docs/specs/02-application.md) | Casos de uso, DTOs, validação, handlers de eventos |
| [03-infrastructure.md](docs/specs/03-infrastructure.md) | EF Core, SQL Server, mapeamentos, UnitOfWork, migrations, seed |
| [04-api.md](docs/specs/04-api.md) | Endpoints, contratos, tratamento de erros |
| [05-frontend.md](docs/specs/05-frontend.md) | Angular: telas, services, organização |
| [06-testes.md](docs/specs/06-testes.md) | Estratégia e casos de teste obrigatórios |

As specs são a fonte da verdade. Se uma implementação precisar divergir, **atualize a spec** e registre a decisão na seção "Decisões técnicas" do [README.md](README.md).

## Stack

- Back-end: **.NET 10** (C#), EF Core 10, SQL Server, MediatR 12.5 (apenas Domain Events), FluentValidation, xUnit
- Front-end: **Angular** (versão atual, standalone components), Bootstrap
- Banco local via **Docker Compose** (SQL Server)

## Estrutura alvo

```
Wtw.slnx
Directory.Build.props
src/
  Wtw.Domain/           # sem dependências externas
  Wtw.Application/      # depende de Domain
  Wtw.Infrastructure/   # depende de Application e Domain
  Wtw.Api/              # depende de Application e Infrastructure (apenas para DI)
tests/
  Wtw.Domain.Tests/
  Wtw.Application.Tests/
web/                    # aplicação Angular
docs/specs/
docker-compose.yml
README.md
```

Regra de dependência: **Domain não referencia outros projetos** (única dependência externa: `MediatR.Contracts`). Application não referencia Infrastructure nem Api. Controllers não acessam DbContext.

## Convenções de código

- **Idioma**: nomes de domínio, classes, métodos e mensagens em **português** (`Pedido`, `Confirmar()`, `BaixarEstoque()`). Termos técnicos consagrados podem ficar em inglês (`Repository`, `Handler`, `Dto`, `DbContext`).
- Namespaces seguem as pastas (`Wtw.Domain.Pedidos`, `Wtw.Domain.Produtos`), file-scoped.
- `TargetFramework`, `Nullable`, `ImplicitUsings` e `TreatWarningsAsErrors` definidos em `Directory.Build.props`.
- Entidades de domínio: **sem setters públicos**, construtores/fábricas que validam, comportamento em métodos com nome de negócio. Nada de modelo anêmico.
- Coleções expostas como `IReadOnlyCollection<T>`, com backing field privado.
- Violação de regra de negócio → retornar `Result.Fail(XxxErros.Algo)` (Result pattern, sem exceções). Mensagens em português, prontas para o usuário, centralizadas nas classes `XxxErros` de cada área. Exceções só para bugs e falhas de infraestrutura.
- Value Objects: `readonly record struct` com construtor privado e fábrica `Criar(...)` que retorna `Result<T>`.
- Domain Events herdam de `DomainEvent`, têm sufixo `DomainEvent` e ficam na pasta `Eventos/` do agregado. Handlers são `INotificationHandler<T>` do MediatR, na Application.
- Métodos curtos, uma responsabilidade por classe, injeção por construtor (primary constructors são bem-vindos em services/handlers).
- Sem comentários óbvios; comente apenas o "porquê" de decisões não evidentes.
- Métodos assíncronos com sufixo `Async` e `CancellationToken` propagado.

## Comandos

```bash
dotnet build Wtw.slnx
dotnet test Wtw.slnx
dotnet run --project src/Wtw.Api
docker compose up -d                                   # SQL Server local (localhost,1433)
dotnet ef migrations add <Nome> --project src/Wtw.Infrastructure --output-dir Persistence/Migrations
dotnet ef database update --project src/Wtw.Infrastructure
dotnet ef migrations script --idempotent --project src/Wtw.Infrastructure -o scripts/create-database.sql
cd web && npm install && npm start
```

## Fluxo de trabalho

1. Leia a spec da camada antes de codar.
2. Regras de negócio novas nascem **com teste** em `Wtw.Domain.Tests`.
3. Rode `dotnet build` e `dotnet test` antes de considerar uma tarefa concluída.
4. Ao finalizar uma etapa, atualize o README (como rodar, decisões técnicas).
5. Commits pequenos e descritivos, em português (ex.: `feat(domain): adiciona agregado Pedido com transições de status`).
