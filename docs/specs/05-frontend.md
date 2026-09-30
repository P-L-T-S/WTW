# 05 — Front-end (`web/`)

**Angular 22** com standalone components, **zoneless** (sem `zone.js`; a detecção de mudanças é guiada por signals), TypeScript 6 em modo estrito e Bootstrap 5. Rotas com lazy loading por feature. Design simples; o foco é navegação funcional e organização.

## Estrutura

```
web/src/
  environments/environment.ts          # apiUrl: 'http://localhost:5000/api'
  app/
    app.ts / app.config.ts / app.routes.ts
    core/                              # singletons usados pela aplicação inteira
      http/erro-api.ts                 # ProblemDetails → ErroApi (título, detalhe, campos, código, traceId)
      http/erro.interceptor.ts         # converte erros HTTP e exibe notificação
      notificacoes/                    # NotificacaoService (signal) + componente de toasts
      layout/navbar.ts
    shared/                            # reutilizável e sem estado
      models/                          # interfaces espelhando os DTOs da API
      forms/erros-formulario.ts        # aplica erros 400 da API nos controles; mensagens de validação
      pedidos/acoes-pedido.ts          # ações disponíveis por status (função pura)
      pedidos/status-pedido-badge.ts
    features/
      produtos/
        produtos.routes.ts
        services/produto.service.ts
        pages/produto-lista/  pages/produto-form/        # criar e editar
      pedidos/
        pedidos.routes.ts
        services/pedido.service.ts  services/cliente.service.ts
        components/acoes-pedido/                          # botões de transição (lista e detalhe)
        pages/pedido-lista/  pages/pedido-form/  pages/pedido-detalhe/
```

> O enunciado fala em "módulos": a organização por **feature folders com rotas lazy** (`loadChildren` / `loadComponent`) é o equivalente moderno aos NgModules de feature, recomendado pelo Angular atual. Cada página vira um bundle próprio carregado sob demanda.

Nomes seguem o guia de estilo atual do Angular: arquivos sem sufixo `.component` (`produto-lista.ts`, classe `ProdutoLista`).

## Configuração (`app.config.ts`)

- `provideRouter(routes, withComponentInputBinding())`: parâmetros de rota chegam como `input()` (ex.: `id` em `PedidoDetalhe`).
- `provideHttpClient(withFetch(), withInterceptors([erroInterceptor]))`.
- `LOCALE_ID = 'pt-BR'` e `DEFAULT_CURRENCY_CODE = 'BRL'`: moeda (`R$ 1.234,50`) e datas no formato brasileiro. As datas UTC da API (com `Z`) são exibidas no fuso local do navegador.

## Rotas

| Rota | Tela |
|------|------|
| `/` | redireciona para `/pedidos` |
| `/produtos` | listagem de produtos |
| `/produtos/novo`, `/produtos/:id/editar` | formulário de produto |
| `/pedidos` | listagem de pedidos |
| `/pedidos/novo` | criação de pedido |
| `/pedidos/:id` | detalhe do pedido |
| qualquer outra | redireciona para `/pedidos` |

## Telas

### Listagem de produtos
Tabela: nome, descrição, preço em reais, estoque (em vermelho quando zerado). Botões "Novo produto" e "Editar".

### Formulário de produto
Reactive Forms tipados (`NonNullableFormBuilder`) com validação espelhando a API (nome 2–100, descrição até 500, preço > 0, estoque ≥ 0). Erros 400 da API são aplicados **no próprio campo** (`aplicarErrosDoServidor`). Erros sem campo correspondente aparecem como alerta no topo. Após salvar, volta para a listagem com notificação.

### Criação de pedido
1. Seleção de cliente.
2. Adição de itens: produto (com preço e estoque disponível; produtos sem estoque ficam desabilitados) e quantidade. Produto repetido **soma a quantidade** (RC04).
3. Tabela de itens com preço unitário, subtotal e remover.
4. **Total** calculado em tela com `computed` (somente exibição; o valor oficial vem da API).
5. Validação local de quantidade ≤ estoque para feedback imediato. A API continua sendo quem garante a regra, e um 422 dela aparece como notificação.
6. Após criar, navega para o detalhe do pedido.

O estado do pedido em montagem fica em `RascunhoPedido` (classe com signals: `itens`, `total`, `vazio`, `adicionar`, `remover`), fora do componente, que só orquestra a tela.

### Listagem de pedidos
Tabela: data, cliente, qtd de itens, total, status (badge colorido) e ações. Filtro por status (`?status=` com o nome exato do enum). A tabela continua visível durante o recarregamento, sem piscar.

### Detalhe do pedido
Cliente, datas de criação e atualização, status, itens com preço unitário e subtotal, total e as mesmas ações da listagem.

### Ações por status
| Status | Ações |
|--------|-------|
| `Criado` | Confirmar, Cancelar |
| `Confirmado` | Finalizar, Cancelar |
| `Cancelado` / `Finalizado` | nenhuma |

Cancelar e Finalizar pedem confirmação (`confirm`). Após cada ação: notificação de sucesso e recarga dos dados (`resource.reload()`), porque os commands da API respondem 204 sem corpo (CQRS). A regra "quais ações por status" fica em `shared/pedidos/acoes-pedido.ts`, uma função pura, e o componente `AcoesPedido` é reutilizado na listagem e no detalhe.

## Tratamento de erros

`erroInterceptor` converte todo `HttpErrorResponse` em `ErroApi` e relança:

| Situação | Comportamento |
|----------|---------------|
| 400 com `errors` (validação) | **sem** notificação; o formulário aplica os erros nos campos |
| Demais erros (404, 409, 422, 500, 400 sem campos) | notificação com `title` e `detail` da API + **referência** (trace id) |
| API fora do ar (status 0) | notificação "API indisponível" |

A referência exibida é o trace id da requisição, o mesmo valor do campo `@tr` no log JSON da API. Com ela, o suporte encontra no log todos os eventos daquela requisição.

## Regras de implementação

- **Toda comunicação HTTP em services** (`inject(HttpClient)`). Componentes não usam `HttpClient`.
- **Sem regra de negócio nos componentes**: ações por status em `shared/pedidos`; estado do pedido em montagem em `RascunhoPedido`. O cálculo do total em tela é apresentação.
- **Leituras com `rxResource`** (`@angular/core/rxjs-interop`): `isLoading()`, `hasValue()`, `error()` e `reload()` sem subscribe manual. Filtros e parâmetros de rota entram como `params`, e o resource recarrega sozinho quando mudam.
- **Mutações** (salvar, criar, ações) com `subscribe` e signals de estado (`salvando`, `executando`).
- Estado local com **signals**; `computed` para total e flags; `input()` / `output()` nos componentes.
- Modelos tipados (`StatusPedido` derivado de `STATUS_PEDIDO as const`), sem `any`.
- Control flow nativo (`@if`, `@for`, `@let`).

## Execução

```bash
cd web
npm install
npm start          # http://localhost:4200 (a API deve estar em http://localhost:5000)
npm run build      # saída em web/dist/web
```

O npm 11 bloqueia por padrão os scripts de instalação de dependências nativas do tooling (`esbuild`, `lmdb`, `@parcel/watcher`, `msgpackr-extract`). O build funciona sem eles, porque os binários vêm pré-compilados.

## Verificação

Testado no navegador contra a API real: listagem de produtos em reais; erro 400 da API aplicado no campo Nome; criação de pedido com soma de item repetido, aviso local de estoque e total calculado; navegação para o detalhe; confirmação com baixa de estoque; confirmação sem estoque exibindo o 422 do domínio com a referência; cancelamento com confirmação e estoque restaurado; filtro por status; pedido inexistente com notificação 404; referência da notificação encontrada no log da API.
