# 05 — Front-end (`web/`)

Angular (versão atual), **standalone components**, rotas com lazy loading por feature, Bootstrap para estilo. Design simples; foco em navegação funcional e organização.

## Estrutura

```
web/src/app/
  core/
    services/api-error.service.ts     # converte ProblemDetails em mensagens
    interceptors/error.interceptor.ts # captura HttpErrorResponse e exibe notificação
    components/navbar/
    components/notificacao/           # toasts de sucesso/erro
  shared/
    models/                           # interfaces espelhando os DTOs da API
    pipes/status-pedido.pipe.ts
  features/
    produtos/
      produtos.routes.ts
      services/produto.service.ts
      pages/produto-lista/
      pages/produto-form/             # criar e editar
    pedidos/
      pedidos.routes.ts
      services/pedido.service.ts
      services/cliente.service.ts
      pages/pedido-lista/
      pages/pedido-form/              # criação
      pages/pedido-detalhe/
  app.routes.ts
  app.config.ts
environments/environment.ts           # apiUrl: 'http://localhost:5000/api'
```

> O enunciado fala em "módulos": a organização por **feature folders com rotas lazy** é o equivalente moderno aos NgModules de feature, recomendado pelo Angular atual.

## Rotas

| Rota | Tela |
|------|------|
| `/` | redireciona para `/pedidos` |
| `/produtos` | listagem de produtos |
| `/produtos/novo`, `/produtos/:id/editar` | formulário de produto |
| `/pedidos` | listagem de pedidos |
| `/pedidos/novo` | criação de pedido |
| `/pedidos/:id` | detalhe do pedido |

## Telas

### Listagem de produtos
Tabela: nome, descrição, preço (pipe `currency` BRL), estoque. Botões "Novo produto" e "Editar".

### Formulário de produto
Reactive Forms com validação espelhando a API (nome obrigatório 2–100, preço > 0, estoque ≥ 0). Erros 400 da API são mapeados para os campos do form.

### Criação de pedido
1. Seleção de cliente (select).
2. Adição de itens: select de produto (mostrando estoque disponível) + quantidade + botão "Adicionar". Produto repetido soma quantidade.
3. Tabela de itens com preço unitário, subtotal e remover.
4. **Total** calculado em tela (somente exibição — o valor oficial vem da API).
5. "Criar pedido" desabilitado sem cliente ou sem itens. Validação local de quantidade ≤ estoque para feedback imediato; a API é quem garante a regra.

### Listagem de pedidos
Tabela: data, cliente, qtd itens, total, status (badge colorido). Filtro por status. Ações conforme status:
- `Criado`: Confirmar, Cancelar
- `Confirmado`: Finalizar, Cancelar
- `Cancelado` / `Finalizado`: nenhuma

Confirmação (`confirm`/modal) antes de cancelar. Após a ação, recarrega a lista e exibe toast.

## Regras de implementação

- **Toda comunicação HTTP em services** (`inject(HttpClient)`); componentes não usam `HttpClient` diretamente.
- **Sem regra de negócio nos componentes**: a decisão de quais ações exibir por status fica em uma função utilitária/pipe em `shared`, não espalhada em templates. O cálculo do total em tela é apresentação.
- Estado local com **signals**; `computed` para total e flags.
- Erros da API (ProblemDetails) tratados no interceptor: `detail` exibido em toast; `errors` de validação repassados ao formulário.
- Modelos tipados (`interface Produto`, `type StatusPedido = 'Criado' | ...`), sem `any`.
- Nova sintaxe de control flow (`@if`, `@for`).
