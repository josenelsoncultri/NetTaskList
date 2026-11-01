# Research: Resilient Interactive Task Selection Markup

**Feature**: `002-fix-selector-markup` | **Phase**: 0 | **Date**: 2026-11-01

## Problem Investigation

### Crash Root Cause
In `NetTaskList.Presentation.InteractiveTaskSelector`:
```csharp
prompt.AddChoice(new TaskChoice(null, "[Sair]"));
```
When `Spectre.Console` renders items in a `SelectionPrompt<T>`, it invokes the configured converter (`choice => choice.DisplayName`) and feeds the resulting string directly into `new Markup(text, ...)`.
When rendering non-selected choices, Spectre indents the string with 2 spaces:
```text
"  [Sair]"
```
Spectre's internal `MarkupTokenizer` treats square brackets (`[...]`) as styling and color tags. Because `"Sair"` is not a recognised color or style modifier, `MarkupTokenizer.ReadMarkup()` fails and invokes `ThrowMalformed(pos)`:
```text
System.InvalidOperationException: Encountered malformed markup tag at position 8.
   at Spectre.Console.AnsiMarkup.Parse(String markup, Nullable`1 style)
   at Spectre.Console.SelectionPrompt`1.Render(...)
```

---

## Technical Decisions

### Decision 1: Markup Escaping Strategy for Selection Choices
- **Decision**: Systematically pass all text rendered inside `SelectionPrompt` through `Markup.Escape(...)`. Specifically, define the exit choice display string using `Markup.Escape("[Sair]")` (or literal double-bracket format `[[Sair]]`).
- **Rationale**: 
  - `Markup.Escape(string text)` escapes `[` to `[[` and `]` to `]]`, which Spectre's markup parser resolves as literal printable brackets rather than markup tokens.
  - Ensures absolute resilience against user input containing brackets (e.g. task names like `[build:prod]` or descriptions like `Executa script [v1.0]`).
- **Alternatives Considered**:
  - *Remove brackets entirely (`"Sair"`)*: Rejected because the bracketed notation `[Sair]` provides clear visual affordance distinguishing the command control from user tasks.
  - *Use styled markup (e.g. `"[grey][[Sair]][/]"`)*: Viable, but still requires the brackets of "Sair" to be escaped as `[[Sair]]`. We can combine a subtle color style with escaped brackets (e.g. `"[grey]" + Markup.Escape("[Sair]") + "[/]"` or `Markup.Escape("[Sair]")`).

### Decision 2: Console Mocking and Automated Unit Testing
- **Decision**: Create a lightweight `MockAnsiConsole` and `MockAnsiConsoleInput` inside `tests/NetTaskList.Tests/Mocks/` implementing `IAnsiConsole` and `IAnsiConsoleInput`.
- **Rationale**:
  - `InteractiveTaskSelector.SelectTask` already accepts `IAnsiConsole? console = null` as a parameter.
  - `IAnsiConsoleInput` requires only 3 simple methods: `IsKeyAvailable()`, `ReadKey(bool)`, and `ReadKeyAsync(bool, CancellationToken)`.
  - By queuing simulated keystrokes (such as `ConsoleKey.Enter` or `ConsoleKey.DownArrow`), we can deterministically drive `SelectionPrompt` in automated xUnit tests.
  - Adheres strictly to Constitution Principle IV (Minimal Dependencies & Explicit Justification), avoiding pulling in unnecessary external NuGet packages.
- **Alternatives Considered**:
  - *Add `Spectre.Console.Testing` NuGet package*: Rejected because it is not present in the local cache, introduces unnecessary external dependencies, and a simple custom mock takes less than 50 lines of code.
  - *Skip unit tests for interactive UI*: Strictly forbidden by Constitution Principle III (Test-First & Mandatory Unit Test Coverage).
