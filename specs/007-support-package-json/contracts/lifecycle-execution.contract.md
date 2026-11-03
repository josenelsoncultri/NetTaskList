# Contract: Orquestração de Ciclo de Vida npm

## Escopo
Este contrato define a execução sequencial de ganchos de ciclo de vida (`pre*` e `post*`) para tarefas originadas de arquivos `package.json`.

---

## Regras de Execução

### 1. Desencadeamento Automático
Quando uma tarefa `<tarefa>` de uma configuração `package.json` for selecionada (seja via menu interativo ou via `nxtl --run <tarefa>`):
1. **Passo Pré-Execução**:
   - Verificar se existe uma tarefa com nome exatamente igual a `pre<tarefa>` (comparação insensível a maiúsculas/minúsculas).
   - Se existir, executar `pre<tarefa>`.
   - Se o código de retorno de `pre<tarefa>` for diferente de 0, abortar a execução imediatamente. O código de retorno de `pre<tarefa>` torna-se o código de saída do `nxtl`.
2. **Passo Execução Principal**:
   - Executar a tarefa principal `<tarefa>`.
   - Se o código de retorno de `<tarefa>` for diferente de 0, abortar o passo pós-execução. O código de retorno de `<tarefa>` torna-se o código de saída do `nxtl`.
3. **Passo Pós-Execução**:
   - Verificar se existe uma tarefa com nome exatamente igual a `post<tarefa>` (comparação insensível a maiúsculas/minúsculas).
   - Se existir, executar `post<tarefa>`.
   - O código de retorno de `post<tarefa>` torna-se o código de saída final do `nxtl`.

---

### 2. Execução Direta e Isolada de Hooks
- Se o usuário executar explicitamente um hook pelo nome completo (ex: `nxtl --run prebuild`), o sistema DEVE executar apenas o comando de `prebuild` como tarefa avulsa, sem engatilhar recursivamente outros scripts.

---

### 3. Filtragem de Apresentação no Menu Interativo
- Tarefas originadas de `package.json` que comecem com `pre` ou `post` DEVEM ser omitidas da listagem interativa SE o comando principal correspondente estiver presente na lista de tarefas.
- **Exemplo**:
  - Scripts: `prebuild`, `build`, `postbuild`, `prepare`.
  - Exibidos no menu: `build`, `prepare`.
  - Ocultados do menu: `prebuild` (pois `build` existe), `postbuild` (pois `build` existe).

