# Research & Technical Decisions: Vertical Carousel Terminal Selection

**Feature**: [spec.md](spec.md) | **Date**: 2026-11-03

## Research Topics & Decisions

### 1. Native Wrap-Around Navigation in Spectre.Console SelectionPrompt

- **Context**: In interactive terminal navigation (`nxtl` executed without arguments), users want circular navigation (carousel mode): pressing Down on the last option highlights the first option, and pressing Up on the first option highlights the last option.
- **Decision**: Configure `WrapAround = true` (via fluent extension `.WrapAround(true)`) on `SelectionPrompt<TaskChoice>` within `InteractiveTaskSelector.SelectTask`.
- **Rationale**:
  - `Spectre.Console` provides first-class native support for circular menu navigation via the `WrapAround` property/extension on `SelectionPrompt<T>`.
  - When enabled, Spectre.Console automatically handles cursor wrapping across arbitrary list sizes and paginated displays without requiring custom key event loops or manual cursor math.
  - Zero performance overhead, zero additional allocations, and 100% Native AOT compatibility.
- **Alternatives Considered**:
  - **Custom Keyboard Interception Loop**: Implementing a custom `while (true)` reading `Console.ReadKey()` and re-rendering Spectre tables or widgets. *Rejected*: Highly error-prone, duplicates Spectre.Console's terminal capability negotiation, pagination logic, cursor repositioning, and violates Constitution Principle IV (favoring KISS and YAGNI).
  - **Custom SelectionPrompt Subclass**: Creating a derived prompt type. *Rejected*: Unnecessary because `WrapAround` is already an existing, publicly exposed configuration on `SelectionPrompt<T>`.

### 2. Automated Testing of Carousel Navigation with MockAnsiConsole

- **Context**: Constitution Principle III requires mandatory automated unit test coverage decoupled from console I/O and OS process invocations.
- **Decision**: Extend `tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorTests.cs` using the existing `MockAnsiConsole` to assert boundary wrapping.
- **Rationale**:
  - `MockAnsiConsole` provides `PushKey(ConsoleKey key)` to queue keyboard events into `IAnsiConsoleInput`.
  - For backward wrap (Story 2): Initial cursor is at index 0. Enqueueing `ConsoleKey.UpArrow` followed by `ConsoleKey.Enter` immediately selects the last choice (the `[Sair]` exit choice, returning `null`).
  - For forward wrap (Story 1): In a 3-task list + 1 exit choice (total 4 choices at indices 0, 1, 2, 3), enqueuing 4 `ConsoleKey.DownArrow` keypresses moves 0 -> 1 -> 2 -> 3 -> 0 (wrapped back to first task). Enqueueing `ConsoleKey.Enter` asserts that the first task is selected.
  - For multi-cycle navigation (Story 3): Enqueueing repeated Down and Up arrow sequences asserts that cursor position wraps consistently across multiple continuous cycles.
- **Alternatives Considered**:
  - **Integration testing with pseudo-terminals (pty/tmux)**: *Rejected*: Slow, fragile across different CI environments, and unnecessary when `IAnsiConsole` mock abstractions already provide 100% test coverage and sub-millisecond execution.

### 3. Scope and Backward Compatibility Verification

- **Context**: Verifying whether enabling `WrapAround` affects other interactive behaviors or introduces regressions in existing tests.
- **Decision**: Validate that all existing 111 unit tests continue passing and that `FilterDisplayTasks`, `PageSize`, Portuguese labels, and markup escaping remain intact.
- **Rationale**:
  - Enabling `WrapAround` changes only cursor transition behavior when attempting to navigate past boundaries (Up on index 0, or Down on index N-1).
  - Normal sequential navigation (Down on index 0 to reach index 1, or Up on index 1 to reach index 0) remains identical to previous behavior.
- **Alternatives Considered**:
  - None required; wrap-around is strictly non-breaking and purely additive to navigation ergonomics.

