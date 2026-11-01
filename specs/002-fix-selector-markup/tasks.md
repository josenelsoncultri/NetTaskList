# Tasks: Correção de Exceção de Markup no InteractiveTaskSelector

**Input**: Design documents from `/specs/002-fix-selector-markup/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/task-selector-contract.md`, `quickstart.md`

## Format: `- [ ] [TaskID] [P?] [Story?] Description with file path`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., [US1], [US2])
- Every task includes an exact file path

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Provide console mocking infrastructure required to unit test interactive prompts without human keyboard interaction.

- [X] T001 Implement `MockAnsiConsole` and `MockAnsiConsoleInput` in `tests/NetTaskList.Tests/Mocks/MockAnsiConsole.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Foundational test scaffolding required before user story tests and implementations.

**⚠️ CRITICAL**: Foundational tasks must complete before user story implementation begins.

- [X] T002 Create test class scaffold for `InteractiveTaskSelector` in `tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorTests.cs`

**Checkpoint**: Foundation ready - console mocking and test class scaffold available.

---

## Phase 3: User Story 1 - Graceful Interactive Menu Launch and Exit (Priority: P1) 🎯 MVP

**Goal**: Ensure launching interactive task selection displays without markup parsing exceptions and selecting the exit choice returns `null` with code 0.

**Independent Test**: Execute `InteractiveTaskSelector.SelectTask` with mock input selecting `[Sair]`, asserting it returns `null` without throwing `System.InvalidOperationException`.

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they fail or demonstrate the issue before implementation**

- [X] T003 [US1] Add unit test for exit option selection returning null in `tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorTests.cs`
- [X] T004 [US1] Add unit test for valid task selection returning the chosen `ScriptTask` in `tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorTests.cs`

### Implementation for User Story 1

- [X] T005 [US1] Escape exit choice label with `Markup.Escape("[Sair]")` in `src/NetTaskList/Presentation/InteractiveTaskSelector.cs`

**Checkpoint**: User Story 1 is functional and verified independently. The prompt no longer crashes on launch.

---

## Phase 4: User Story 2 - Resilient Display of Tasks with Special Characters (Priority: P2)

**Goal**: Guarantee that task names, commands, and descriptions containing literal brackets (e.g., `[build:prod]`, `[docker] run`) render safely without syntax errors.

**Independent Test**: Pass tasks with single, nested, and unclosed brackets to `InteractiveTaskSelector.SelectTask`, asserting choices render verbatim and are selectable without `InvalidOperationException`.

### Tests for User Story 2

- [X] T006 [US2] Add unit tests for tasks containing brackets in names, commands, and descriptions in `tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorTests.cs`

### Implementation for User Story 2

- [X] T007 [US2] Ensure all task choice labels are escaped safely via `Markup.Escape` in `src/NetTaskList/Presentation/InteractiveTaskSelector.cs`

**Checkpoint**: User Stories 1 and 2 are functional and fully covered by unit tests.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Validation, regression checks, and verification against quickstart scenarios.

- [X] T008 [P] Run full test suite via `dotnet test` in `tests/NetTaskList.Tests/NetTaskList.Tests.csproj`
- [X] T009 Validate manual terminal run scenario per `specs/002-fix-selector-markup/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Can start immediately - creates `MockAnsiConsole.cs`.
- **Foundational (Phase 2)**: Depends on Phase 1 - creates `InteractiveTaskSelectorTests.cs` scaffold.
- **User Story 1 (Phase 3)**: Depends on Phase 2 - delivers MVP (fixes crash on launch and handles `[Sair]`).
- **User Story 2 (Phase 4)**: Depends on Phase 3 - verifies and hardens brackets in custom task definitions.
- **Polish (Phase 5)**: Depends on Phase 3 and Phase 4 completion.

### Within Each User Story

1. Write unit tests first in `tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorTests.cs`.
2. Apply the implementation fix in `src/NetTaskList/Presentation/InteractiveTaskSelector.cs`.
3. Verify tests pass before advancing to the next story.

---

## Parallel Opportunities

- **T008** can run in parallel with documentation/review checks.
- Once Phase 2 completes, test authoring for **US1** (`T003`, `T004`) and **US2** (`T006`) can be authored together in the test file before implementation fixes.

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (`MockAnsiConsole.cs`).
2. Complete Phase 2: Foundational (`InteractiveTaskSelectorTests.cs` scaffold).
3. Complete Phase 3: User Story 1 (`T003`, `T004`, `T005`).
4. **STOP and VALIDATE**: Run `dotnet test --filter InteractiveTaskSelectorTests` to confirm User Story 1 passes and crash is resolved.

### Incremental Delivery

1. Add User Story 2 (`T006`, `T007`) to harden bracket escaping in task definitions.
2. Complete Polish (`T008`, `T009`) running full regression test suite.
