# Tasks: Vertical Carousel Terminal Selection

**Input**: Design documents from `/specs/008-vertical-carousel-selection/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/task-selector-navigation-contract.md`, `quickstart.md`

## Format: `- [ ] [TaskID] [P?] [Story?] Description with file path`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., [US1], [US2], [US3])
- Every task includes an exact file path

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify existing console mocking infrastructure and test scaffolding.

- [X] T001 Verify `MockAnsiConsole` input queuing capabilities in `tests/NetTaskList.Tests/Mocks/MockAnsiConsole.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Ensure baseline test suite passes before implementing navigation modifications.

**⚠️ CRITICAL**: Foundational verification must pass before user story implementation begins.

- [X] T002 Verify baseline `InteractiveTaskSelector` tests pass via `XDG_RUNTIME_DIR=/tmp dotnet test --filter "FullyQualifiedName~InteractiveTaskSelectorTests" --no-restore` in `tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorTests.cs`

**Checkpoint**: Foundation verified - mock console and baseline test assertions ready.

---

## Phase 3: User Story 1 - Forward Wrap-Around from End to Beginning (Priority: P1) 🎯 MVP

**Goal**: When navigating Down from the last choice (`[Sair]`), the cursor wraps around to the first task.

**Independent Test**: Pass sample tasks to `InteractiveTaskSelector.SelectTask`, enqueue Down arrow keypresses equal to the total item count ($N$), enqueue Enter, and assert that the first task is selected and returned.

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they fail or demonstrate missing wrap-around before implementation**

- [X] T003 [US1] Add unit test asserting that pressing Down from the exit option wraps around to the first task in `tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorTests.cs`

### Implementation for User Story 1

- [X] T004 [US1] Enable `.WrapAround(true)` on `SelectionPrompt<TaskChoice>` in `src/NetTaskList/Presentation/InteractiveTaskSelector.cs`

**Checkpoint**: User Story 1 is functional and verified independently. Forward wrap-around operates smoothly.

---

## Phase 4: User Story 2 - Backward Wrap-Around from Beginning to End (Priority: P1)

**Goal**: When navigating Up from the initial option (first task), the cursor wraps around to the last choice (`[Sair]`).

**Independent Test**: Launch `InteractiveTaskSelector.SelectTask` (cursor starts on item 0), enqueue a single Up arrow keypress, enqueue Enter, and assert that the return value is `null` (exit option selected).

### Tests for User Story 2

- [X] T005 [US2] Add unit test asserting that pressing Up from the initial first task wraps around to the exit choice (`[Sair]`) returning null in `tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorTests.cs`

### Implementation for User Story 2

- [X] T006 [US2] Verify backward wrap-around selection resolution and exit code handling in `src/NetTaskList/Presentation/InteractiveTaskSelector.cs`

**Checkpoint**: User Stories 1 and 2 are functional and verified independently. Bidirectional wrap-around is active.

---

## Phase 5: User Story 3 - Continuous Cyclical Navigation Across Long Lists (Priority: P2)

**Goal**: Ensure continuous wrap-around navigation in both vertical directions works across multiple cycles and paginated lists.

**Independent Test**: Pass 10 tasks (exceeding `PageSize` 8) with an exit choice (11 total items), enqueue continuous sequences of Up and Down arrows across multiple wrap cycles, and assert correct choice resolution upon Enter.

### Tests for User Story 3

- [X] T007 [US3] Add unit test asserting continuous multi-cycle navigation and wrap-around across paginated lists (10+ tasks) in `tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorTests.cs`

### Implementation for User Story 3

- [X] T008 [US3] Validate viewport stability and Portuguese scroll text hints under pagination wrapping in `src/NetTaskList/Presentation/InteractiveTaskSelector.cs`

**Checkpoint**: All three user stories are verified independently and work cohesively.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Regression testing, quickstart validation, and constitution quality compliance.

- [X] T009 [P] Run full test suite via `XDG_RUNTIME_DIR=/tmp dotnet test --no-restore` in `tests/NetTaskList.Tests/NetTaskList.Tests.csproj`
- [X] T010 Validate manual scenarios and boundary edge cases per `specs/008-vertical-carousel-selection/quickstart.md`
- [X] T011 Evaluate version bump in `src/NetTaskList/NetTaskList.csproj` per Constitution Principle VI (Versioning Policy & Spec Implementation Lifecycle)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Can start immediately.
- **Foundational (Phase 2)**: Depends on Phase 1 - verifies baseline tests before making changes.
- **User Story 1 (Phase 3)**: Depends on Phase 2 - delivers core MVP (forward wrap-around).
- **User Story 2 (Phase 4)**: Depends on Phase 3 - delivers backward wrap-around.
- **User Story 3 (Phase 5)**: Depends on Phase 4 - validates multi-cycle and paginated lists.
- **Polish (Phase 6)**: Depends on completion of all user story phases.

### User Story Dependencies

- **User Story 1 (P1)**: Independent after Phase 2 foundation.
- **User Story 2 (P1)**: Shares `.WrapAround(true)` with US1; verified independently via backward key test.
- **User Story 3 (P2)**: Tests paginated scenario; independently verifiable once US1 and US2 are active.

### Within Each User Story

1. Write test first in `tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorTests.cs`.
2. Apply or verify implementation in `src/NetTaskList/Presentation/InteractiveTaskSelector.cs`.
3. Verify test passes via `dotnet test`.

---

## Parallel Opportunities

- **T001** and **T002** can be inspected in parallel.
- Test cases for **US1** (`T003`), **US2** (`T005`), and **US3** (`T007`) can be authored together in `InteractiveTaskSelectorTests.cs`.
- **T009** (full test run) and **T010** (quickstart validation) can run in parallel during Phase 6.

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (`T001`).
2. Complete Phase 2: Foundational baseline test run (`T002`).
3. Complete Phase 3: User Story 1 (`T003`, `T004`).
4. **STOP and VALIDATE**: Confirm forward wrap-around test passes.

### Incremental Delivery

1. Add User Story 2 (`T005`, `T006`) → Test backward wrap-around independently.
2. Add User Story 3 (`T007`, `T008`) → Test continuous multi-cycle and paginated wrap-around.
3. Complete Polish (`T009`, `T010`, `T011`) → Full test suite, quickstart check, and version evaluation.

