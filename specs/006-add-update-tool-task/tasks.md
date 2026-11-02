---
description: "Task list for feature implementation: Adição do Comando de Atualização da Ferramenta ao nxtl.json Padrão"
---

# Tasks: Adição do Comando de Atualização da Ferramenta ao nxtl.json Padrão

**Input**: Design documents from `/specs/006-add-update-tool-task/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Tarefas executáveis em paralelo (arquivos distintos, sem dependência mútua)
- **[Story]**: Identificador da história de usuário (`[US1]`, `[US2]`)
- Caminhos exatos para todos os arquivos afetados

---

## Phase 1: Setup (Ambiente e Pré-condições)

**Purpose**: Verificação do ambiente de compilação e linha de base dos testes automatizados

- [X] T001 Verify existing test suite baseline in tests/NetTaskList.Tests/ via dotnet test --no-restore

---

## Phase 2: Foundational (Pré-requisitos Compartilhados)

**Purpose**: Confirmar que o validador e o enum de tipos suportam a nova tarefa sem restrições

- [X] T002 Verify TaskType.Tool mapping and configuration validator in src/NetTaskList/Domain/Validation/ConfigurationValidator.cs

**Checkpoint**: Pré-requisitos estruturais validados. O desenvolvimento das histórias de usuário pode prosseguir.

---

## Phase 3: User Story 1 - Geração de Arquivo Modelo com Tarefa de Atualização da Ferramenta (Priority: P1) 🎯 MVP

**Goal**: Incluir no gerador de template `TemplateConfigurationGenerator.GenerateDefaultJson()` a tarefa `update-nettasklist` com comando `dotnet tool update -g nettasklist`, tipo `tool` e descrição `"Atualiza a ferramenta nxtl de modo global"`.

**Independent Test**: Executar os testes em `TemplateConfigurationGeneratorTests.cs` comprovando que o template gerado possui 4 tarefas, inclui `update-nettasklist` com seus parâmetros e é lido com êxito pelo parser.

### Tests for User Story 1 ⚠️

- [X] T003 [P] [US1] Update unit tests in tests/NetTaskList.Tests/Infrastructure/TemplateConfigurationGeneratorTests.cs to assert 4 tasks including update-nettasklist with command dotnet tool update -g nettasklist

### Implementation for User Story 1

- [X] T004 [US1] Add update-nettasklist task definition to TemplateConfigurationGenerator.GenerateDefaultJson() in src/NetTaskList/Infrastructure/TemplateConfigurationGenerator.cs
- [X] T005 [US1] Run unit tests in tests/NetTaskList.Tests/Infrastructure/TemplateConfigurationGeneratorTests.cs to confirm template generator passes

**Checkpoint**: User Story 1 concluída e testada de forma independente. O comando `nxtl --init` agora gera a tarefa de atualização global.

---

## Phase 4: User Story 2 - Padronização nos Exemplos e Configurações de Referência (Priority: P2)

**Goal**: Atualizar o arquivo de configuração `nxtl.json` na raiz do repositório e os exemplos documentados em `README.md` com a tarefa `update-nettasklist`.

**Independent Test**: Inspecionar `nxtl.json` e `README.md` para garantir a inclusão da nova tarefa e executar a suíte completa de testes (`dotnet test`).

### Implementation for User Story 2

- [X] T006 [P] [US2] Add update-nettasklist task to root configuration file in nxtl.json
- [X] T007 [P] [US2] Update nxtl.json sample structure in README.md to include update-nettasklist task
- [X] T008 [US2] Execute full test suite via dotnet test --no-restore in tests/NetTaskList.Tests/ to verify zero regressions

**Checkpoint**: User Story 2 concluída e validada. A documentação e o repositório estão sincronizados com o template padrão.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Validação final de ponta a ponta e governança da versão segundo a Constituição

- [X] T009 Execute quickstart validation scenarios in specs/006-add-update-tool-task/quickstart.md to verify end-to-end functionality
- [X] T010 Evaluate version bump policy in src/NetTaskList/NetTaskList.csproj per Constitution Principle VI

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências — execução imediata.
- **Foundational (Phase 2)**: Depende da Phase 1.
- **User Story 1 (Phase 3)**: Depende da Phase 2. Entrega o MVP da funcionalidade.
- **User Story 2 (Phase 4)**: Depende da Phase 3.
- **Polish (Phase 5)**: Depende da conclusão de todas as histórias.

### Within Each User Story

- Teste unitário (T003) atualizado antes da implementação de código (T004).
- Alteração em `TemplateConfigurationGenerator.cs` (T004) validada por T005.
- Sincronização em `nxtl.json` (T006) e `README.md` (T007) executadas em paralelo.

### Parallel Opportunities

- **US1**: T003 prepara asserções de teste antecipadamente.
- **US2**: T006 (`nxtl.json`) e T007 (`README.md`) afetam arquivos completamente distintos e podem ser executadas em paralelo (`[P]`).

---

## Parallel Example: User Story 2

```bash
# Execução paralela de tarefas de documentação e configuração:
Task: "Add update-nettasklist task to root configuration file in nxtl.json"
Task: "Update nxtl.json sample structure in README.md to include update-nettasklist task"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Concluir Phase 1 (Baseline) e Phase 2 (Foundational).
2. Executar Phase 3 (US1): atualizar testes de template e adicionar a tarefa ao gerador padrão.
3. Validar US1 independentemente com `dotnet test`.
4. *Resultado*: Novos arquivos `nxtl.json` gerados via `nxtl --init` passam a conter o comando de atualização.

### Incremental Delivery (User Story 2 & Polish)

1. Adicionar a tarefa no `nxtl.json` do próprio repositório.
2. Atualizar os snippets de exemplo no `README.md`.
3. Executar validação de testes e os cenários do `quickstart.md`.
4. Avaliar bump de versão no `.csproj` conforme a Constituição.
