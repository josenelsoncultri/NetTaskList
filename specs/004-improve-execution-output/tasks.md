# Tasks: Melhoria do Output de Execução e Navegação de Tarefas

**Input**: Design documents from `/specs/004-improve-execution-output/`  
**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [data-model.md](data-model.md), [contracts/terminal-presentation-contract.md](contracts/terminal-presentation-contract.md), [research.md](research.md), [quickstart.md](quickstart.md), [.specify/memory/constitution.md](../../.specify/memory/constitution.md)  
**Tests**: Testes automatizados unitários são obrigatórios conforme o Princípio III da Constituição do projeto (`NetTaskList`).  
**Organization**: As tarefas estão organizadas por jornadas do usuário (User Stories) para permitir implementação e validação independentes de cada incremento funcional.

## Format: `- [ ] [TaskID] [P?] [Story?] Description with file path`

- **[P]**: Tarefa paralelizada (arquivos distintos, sem dependência de tarefas incompletas).
- **[Story]**: Identificador da história de usuário ([US1], [US2]).
- Todos os caminhos de arquivos são explícitos e referenciam a raiz do repositório (`src/` e `tests/`).

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Criação do componente utilitário de resolução dinâmica de versão da aplicação e seus testes unitários.

- [X] T001 Create `AppVersion` utility class in `src/NetTaskList/Presentation/AppVersion.cs` to dynamically resolve semantic version via `AssemblyInformationalVersionAttribute` and `Assembly.GetName().Version` with commit hash stripping
- [X] T002 [P] Create unit tests in `tests/NetTaskList.Tests/Presentation/AppVersionTests.cs` verifying dynamic version resolution format `Major.Minor.Patch` (e.g., `"1.0.0"`)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Integração da versão dinâmica centralizada na apresentação da aplicação.

**⚠️ CRITICAL**: Nenhuma história de usuário de apresentação visual deve prosseguir sem a conclusão desta fase.

- [X] T003 Update `ShowVersion` in `src/NetTaskList/Program.cs` to consume `AppVersion.Current` replacing hardcoded version string
- [X] T004 [P] Add unit test in `tests/NetTaskList.Tests/Presentation/ProgramExecutionTests.cs` verifying that `nxtl --version` outputs the resolved dynamic version from `AppVersion.Current`

**Checkpoint**: Fundação pronta - a implementação e os testes das histórias de usuário podem prosseguir de forma independente.

---

## Phase 3: User Story 1 - Visualização Compacta e Interativa da Listagem de Tarefas (Priority: P1) 🎯 MVP

**Goal**: Eliminar a tabela estática de tarefas disponíveis (`"Tarefas Disponíveis (nxtl)"`), configurar a janela de visualização do seletor interativo para 8 tarefas simultâneas (`PageSize(8)`) e formatar a instrução de seleção `"Selecionar uma tarefa para executar"` na cor ciano.

**Independent Test**: Executar `nxtl` sem argumentos em ambiente com tarefas configuradas e verificar que o texto `"Tarefas Disponíveis (nxtl)"` e a tabela estática não são exibidos, o título do seletor é renderizado em ciano (`[cyan]Selecionar uma tarefa para executar[/]`) e apenas 8 opções aparecem simultaneamente no viewport.

### Tests for User Story 1 ⚠️

> **NOTA: Escrever os testes PRIMEIRO e garantir que falhem antes da implementação das alterações de apresentação**

- [X] T005 [P] [US1] Add unit tests in `tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorTests.cs` asserting that `InteractiveTaskSelector` sets `.Title("[cyan]Selecionar uma tarefa para executar[/]")` and `.PageSize(8)`
- [X] T006 [P] [US1] Add unit tests in `tests/NetTaskList.Tests/Presentation/ProgramExecutionTests.cs` verifying that `Program.Run` without arguments does not render `"Tarefas Disponíveis (nxtl)"` or static table border characters

### Implementation for User Story 1

- [X] T007 [US1] Update `src/NetTaskList/Presentation/InteractiveTaskSelector.cs` to set `.Title("[cyan]Selecionar uma tarefa para executar[/]")` and `.PageSize(8)`
- [X] T008 [US1] Update `src/NetTaskList/Program.cs` to remove `TaskTableRenderer.Render(configuration.Tasks, ansiConsole)` from the execution flow
- [X] T009 [US1] Delete obsolete file `src/NetTaskList/Presentation/TaskTableRenderer.cs` to eliminate unused dead code

**Checkpoint**: Neste estágio, a User Story 1 (MVP) está completamente funcional e testável de forma independente.

---

## Phase 4: User Story 2 - Identificação Imediata da Versão da Ferramenta no Cabeçalho (Priority: P2)

**Goal**: Exibir `"nxtl versão <código da versão atual>"` na cor verde no topo da listagem de tarefas antes do seletor interativo ou da mensagem de console redirecionado.

**Independent Test**: Executar `nxtl` sem argumentos e verificar se a primeira linha impressa no console contém `"nxtl versão <versão>"` com estilização em verde.

### Tests for User Story 2 ⚠️

> **NOTA: Escrever os testes PRIMEIRO e garantir que falhem antes da implementação da renderização do cabeçalho**

- [X] T010 [P] [US2] Add unit tests in `tests/NetTaskList.Tests/Presentation/ProgramExecutionTests.cs` verifying that `Program.Run` renders `[green]nxtl versão {version}[/]` at the top of the task listing in both interactive and redirected console environments

### Implementation for User Story 2

- [X] T011 [US2] Update `src/NetTaskList/Program.cs` to render `[green]nxtl versão {Markup.Escape(AppVersion.Current)}[/]` at the top of the task listing prior to console redirection checks and interactive prompt activation

**Checkpoint**: Neste estágio, tanto a User Story 1 quanto a User Story 2 estão completamente funcionais e testáveis de forma independente.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Verificação de qualidade global, testes ponta a ponta e garantia de zero regressões.

- [X] T012 [P] Validate all end-to-end scenarios manually using the guide in `specs/004-improve-execution-output/quickstart.md`
- [X] T013 Run full test suite via `dotnet test` ensuring 100% pass rate with zero warnings and full compliance with `specs/004-improve-execution-output/contracts/terminal-presentation-contract.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências externas — inicia imediatamente com `AppVersion`.
- **Foundational (Phase 2)**: Depende da Phase 1 — centraliza a versão dinâmica no `Program.cs`.
- **User Story 1 (Phase 3 - P1 MVP)**: Depende da Phase 2 — remove a tabela estática e ajusta o seletor.
- **User Story 2 (Phase 4 - P2)**: Depende da Phase 2 — adiciona o cabeçalho de versão no topo da listagem. Pode rodar em paralelo ou sequencialmente após a US1.
- **Polish (Phase 5)**: Depende da conclusão de todas as histórias de usuário desejadas (US1 e US2).

### User Story Dependencies

- **User Story 1 (P1)**: Independente da US2. Focada no seletor e remoção da tabela.
- **User Story 2 (P2)**: Independente da US1. Focada no cabeçalho em verde emitido pelo `Program.cs`.

### Parallel Opportunities

- **Phase 1**: `T001` e `T002` podem ser desenvolvidos em paralelo (TDD).
- **Phase 2**: `T003` e `T004` podem ser desenvolvidos em paralelo.
- **Phase 3**: `T005` e `T006` podem ser escritos em paralelo antes da implementação de `T007` e `T008`.
- **Phase 4**: `T010` pode ser escrito em paralelo com as atividades da US1.
- **US1 & US2**: Uma vez concluída a Phase 2, as User Stories 1 e 2 podem ser executadas por desenvolvedores distintos em paralelo.

---

## Parallel Example: User Story 1

```bash
# Executar testes preliminares de User Story 1 (TDD):
Task: "Add unit tests in tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorTests.cs"
Task: "Add unit tests in tests/NetTaskList.Tests/Presentation/ProgramExecutionTests.cs"

# Implementar componentes em paralelo:
Task: "Update src/NetTaskList/Presentation/InteractiveTaskSelector.cs"
Task: "Update src/NetTaskList/Program.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Concluir Phase 1: Setup (`AppVersion.cs` e testes).
2. Concluir Phase 2: Foundational (Version integration em `Program.cs`).
3. Concluir Phase 3: User Story 1 (remoção de tabela estática + `PageSize(8)` + título ciano).
4. **VALIDAR MVP**: Rodar `dotnet test` e checar funcionamento independente da User Story 1.

### Entrega Incremental

1. Com o MVP validado, implementar a Phase 4 (User Story 2: cabeçalho `"nxtl versão <versão>"` em verde).
2. Executar a validação final (Phase 5: Polish & `quickstart.md`).
3. Deploy e distribuição do release.
