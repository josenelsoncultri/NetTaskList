# Tasks: Atalhos de Comandos de Tarefas e Prevenção de Recursão (Task Command Shortcuts)

**Input**: Design documents from `specs/009-task-command-shortcuts/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

## Format: `- [ ] [TaskID] [P?] [Story?] Description with file path`

- **[P]**: Pode ser executado em paralelo (arquivos distintos, sem dependência direta de tarefas incompletas)
- **[Story]**: Mapeia para a história de usuário correspondente (`[US1]`, `[US2]`, `[US3]`, `[US4]`)
- Todos os caminhos de arquivo são especificados explicitamente

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Atualização do schema declarativo de configuração para suporte a atalhos de tarefas

- [x] T001 Atualizar a propriedade `command` para suportar `taskShortcut` (`{ "task": string }`) e arrays mistos em `src/NetTaskList/schemas/nxtl-v1.schema.json`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Modelagem de dados, DTOs, serialização JSON e validações estruturais que bloqueiam todas as histórias de usuário

**⚠️ CRITICAL**: Nenhuma história de usuário pode ser iniciada antes da conclusão desta fase

- [x] T002 [P] Criar o DTO intermediário `TaskCommandDto` (`RawCommand`, `TargetTask`, `IsShortcut`) em `src/NetTaskList/Infrastructure/Dtos/TaskCommandDto.cs`
- [x] T003 [P] Criar a entidade de domínio `TaskCommandStep` (`CommandLine`, `TargetTaskName`, `IsShortcut`, fábricas estáticas) em `src/NetTaskList/Domain/Models/TaskCommandStep.cs`
- [x] T004 [P] Criar testes unitários para a entidade `TaskCommandStep` em `tests/NetTaskList.Tests/Domain/TaskCommandStepTests.cs`
- [x] T005 Atualizar `ScriptTask` para suportar `CommandSteps` (`IReadOnlyList<TaskCommandStep>`) mantendo compatibilidade de `Commands` em `src/NetTaskList/Domain/Models/ScriptTask.cs`
- [x] T006 Atualizar testes unitários de `ScriptTaskTests` para cobrir inicialização com `TaskCommandStep` em `tests/NetTaskList.Tests/Domain/ScriptTaskTests.cs`
- [x] T007 Atualizar `CommandPropertyJsonConverter` para deserializar comandos em string única, objeto `{ "task": string }` e array misto em `src/NetTaskList/Infrastructure/Serialization/CommandPropertyJsonConverter.cs`
- [x] T008 Registrar `TaskCommandDto` no contexto de serialização Native AOT em `src/NetTaskList/Infrastructure/Serialization/TaskJsonContext.cs`
- [x] T009 Atualizar `ScriptTaskDto` para utilizar `List<TaskCommandDto>? Commands` em `src/NetTaskList/Infrastructure/Dtos/ConfigurationDto.cs`
- [x] T010 Atualizar `CommandPropertyJsonConverterTests` com testes para deserialização de atalhos e arrays polimórficos em `tests/NetTaskList.Tests/Infrastructure/CommandPropertyJsonConverterTests.cs`
- [x] T011 Atualizar `ConfigurationValidator` com regras VR-005 (proibir atalhos em tarefas C#) e VR-006 (proibir atalhos com nome vazio) em `src/NetTaskList/Domain/Validation/ConfigurationValidator.cs`
- [x] T012 Atualizar `ConfigurationValidatorTests` com testes para as regras VR-005 e VR-006 em `tests/NetTaskList.Tests/Domain/ConfigurationValidatorTests.cs`
- [x] T013 Atualizar `JsonConfigurationReader` para mapear `TaskCommandDto` em `TaskCommandStep` na construção de `ScriptTask` em `src/NetTaskList/Infrastructure/JsonConfigurationReader.cs`
- [x] T014 Atualizar `JsonConfigurationReaderTests` com testes de leitura de `nxtl.json` contendo atalhos em `tests/NetTaskList.Tests/Infrastructure/JsonConfigurationReaderTests.cs`

**Checkpoint**: Camada de dados e serialização pronta — histórias de usuário podem ser implementadas

---

## Phase 3: User Story 1 - Executing a Task Shortcut (Priority: P1) 🎯 MVP

**Goal**: Permitir que tarefas de ferramentas referenciem outras tarefas de ferramentas como atalhos diretos, executando seus comandos e exibindo a linha de status

**Independent Test**: Configurar uma tarefa `B` com comando `{ "task": "A" }`, executá-la via `nxtl --run B` e verificar que a mensagem `Executando atalho: A` é impressa e o comando de `A` é executado com sucesso

### Tests for User Story 1

- [x] T015 [P] [US1] Criar testes unitários para resolução e execução de atalho único e impressão da linha de status em `tests/NetTaskList.Tests/Presentation/TaskExecutionCoordinatorTests.cs`
- [x] T016 [P] [US1] Criar testes unitários para exibição do rótulo `atalho: <nome>` no seletor interativo em `tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorTests.cs`

### Implementation for User Story 1

- [x] T017 [US1] Implementar resolução de atalho de tarefa, execução e linha de status `Executando atalho: <nome>` em `src/NetTaskList/Presentation/TaskExecutionCoordinator.cs`
- [x] T018 [US1] Atualizar `InteractiveTaskSelector` para exibir `atalho: <target>` para tarefas de atalho sem descrição em `src/NetTaskList/Presentation/InteractiveTaskSelector.cs`

**Checkpoint**: User Story 1 funcional de forma isolada (MVP)

---

## Phase 4: User Story 2 - Detecting and Preventing Circular Reference Loops (Priority: P1)

**Goal**: Detectar ciclos e recursões infinitas na árvore de atalhos antes de executar qualquer processo, exibindo a cadeia de tarefas e mensagem de erro em pt-BR

**Independent Test**: Configurar tarefas com referência circular (ex: `clean -> build -> clean`), executá-las e verificar que a execução aborta imediatamente com exit code 1, nenhum comando shell roda e o caminho do loop é impresso

### Tests for User Story 2

- [x] T019 [P] [US2] Criar testes unitários para auto-referência (`A -> A`), ciclos mútuos (`A -> B -> A`) e recursão profunda em árvore em `tests/NetTaskList.Tests/Presentation/RecursionDetectionTests.cs`
- [x] T020 [P] [US2] Criar testes unitários para grafos diamante (DAGs) garantindo execução limpa sem falsos positivos de ciclo em `tests/NetTaskList.Tests/Presentation/DiamondDependencyTests.cs`

### Implementation for User Story 2

- [x] T021 [US2] Implementar algoritmo de validação prévia de ciclos (DFS com rastreamento de pilha de caminho) em `src/NetTaskList/Presentation/TaskExecutionCoordinator.cs`
- [x] T022 [US2] Implementar pilha ativa de runtime (`activeTaskStack`) como guarda de segurança durante a execução em `src/NetTaskList/Presentation/TaskExecutionCoordinator.cs`

**Checkpoint**: Prevenção de loops e recursão em árvore totalmente validada

---

## Phase 5: User Story 3 - Sequential Execution Combining Commands and Shortcuts (Priority: P2)

**Goal**: Permitir sequências ordenadas que misturam comandos shell comuns e atalhos de tarefas em arrays, garantindo execução ordenada e interrupção imediata em caso de falha

**Independent Test**: Configurar uma tarefa com `["echo inicio", { "task": "subtask" }, "echo fim"]` e verificar execução na ordem exata; configurar falha em `subtask` e verificar interrupção imediata dos comandos subsequentes

### Tests for User Story 3

- [x] T023 [P] [US3] Criar testes unitários para sequências mistas de comandos e atalhos e teste de parada imediata em falha em `tests/NetTaskList.Tests/Presentation/TaskExecutionCoordinatorTests.cs`

### Implementation for User Story 3

- [x] T024 [US3] Implementar iteração e execução de sequências mistas com interrupção e propagação de exit code em `src/NetTaskList/Presentation/TaskExecutionCoordinator.cs`

**Checkpoint**: Sequências mistas e propagação de erros funcionando de forma integrada

---

## Phase 6: User Story 4 - Handling Invalid or Incompatible References (Priority: P3)

**Goal**: Rejeitar de forma clara referências a tarefas inexistentes no `nxtl.json` e tentativas de referenciar tarefas de script C# (`type: "csharp"`)

**Independent Test**: Executar uma tarefa que referencia `{ "task": "nao-existe" }` ou `{ "task": "script-csharp" }` e verificar mensagem descritiva de erro em pt-BR com exit code 1 sem executar processos

### Tests for User Story 4

- [x] T025 [P] [US4] Criar testes unitários para rejeição de tarefa inexistente e rejeição de atalho para script C# em `tests/NetTaskList.Tests/Presentation/TaskExecutionCoordinatorTests.cs`

### Implementation for User Story 4

- [x] T026 [US4] Implementar validações de existência e verificação de tipo `Tool` para atalhos com mensagens de erro em pt-BR em `src/NetTaskList/Presentation/TaskExecutionCoordinator.cs`

**Checkpoint**: Todas as histórias de usuário implementadas e protegidas contra entradas inválidas

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Verificação ponta a ponta, conformidade constitucional e gestão de versão

- [x] T027 [P] Criar testes automatizados para os 6 cenários do `quickstart.md` em `tests/NetTaskList.Tests/Presentation/QuickstartValidationTests.cs`
- [x] T028 Executar suite completa de testes e verificar 100% de aprovação
- [x] T029 Verificar compilação com zero avisos (`0 Warning(s)`) em `src/NetTaskList/NetTaskList.csproj`
- [x] T030 Atualizar o elemento `<Version>` em `src/NetTaskList/NetTaskList.csproj` para a nova versão Minor conforme Princípio Constitucional VI

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências — pode iniciar imediatamente.
- **Foundational (Phase 2)**: Depende da Fase 1 — **BLOQUEIA** todas as histórias de usuário.
- **User Story 1 (Phase 3)**: Depende da Fase 2 (Foundational) — MVP da funcionalidade.
- **User Story 2 (Phase 4)**: Depende da Fase 2 e integra com US1 — Prevenção crítica contra loops infinitos.
- **User Story 3 (Phase 5)**: Depende de US1 e US2 — Execução mista sequencial com proteção de ciclo.
- **User Story 4 (Phase 6)**: Depende de US1 e US2 — Validação de integridade referencial.
- **Polish (Phase 7)**: Depende da conclusão de todas as histórias de usuário desejadas.

---

## Parallel Opportunities

- **Fase 2**: T002 (`TaskCommandDto`), T003 (`TaskCommandStep`) e T004 (`TaskCommandStepTests`) podem ser criados em paralelo.
- **Fase 3**: T015 e T016 (testes de US1) podem ser desenvolvidos em paralelo.
- **Fase 4**: T019 (testes de ciclo) e T020 (testes de grafo diamante) podem ser implementados em paralelo.
- **Fase 7**: T027 (`QuickstartValidationTests`) pode ser preparado em paralelo com revisões de código.

---

## Implementation Strategy

### MVP First (User Story 1)

1. Concluir Fase 1 (Setup) e Fase 2 (Foundational).
2. Concluir Fase 3 (User Story 1).
3. **Validar MVP**: Executar um atalho simples de ferramenta e verificar funcionamento.

### Entrega Incremental e Segura

1. Adicionar User Story 2 (detecção estática prévia de loops com DFS e guarda em tempo de execução).
2. Adicionar User Story 3 (sequências mistas de comandos e atalhos em array).
3. Adicionar User Story 4 (validação de inexistência e rejeição de C#).
4. Executar bateria de testes do `quickstart.md` e finalizar com incremento de versão Minor.
