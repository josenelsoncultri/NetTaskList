# Interface Contract: Interactive Task Selector

**Feature**: `002-fix-selector-markup` | **Phase**: 1 | **Date**: 2026-11-01

## API Contract

### Method Signature
```csharp
namespace NetTaskList.Presentation;

public static class InteractiveTaskSelector
{
    public static ScriptTask? SelectTask(
        IReadOnlyList<ScriptTask> tasks, 
        IAnsiConsole? console = null);
}
```

### Preconditions
1. `tasks` MUST NOT be null or empty (empty checks are performed upstream in `Program.cs`).
2. `console` may be provided for dependency injection / unit testing; if `null`, defaults to `AnsiConsole.Console`.

### Postconditions
1. Returns a valid `ScriptTask` instance if the user selects a task choice.
2. Returns `null` if the user selects the `[Sair]` choice or cancels the prompt.
3. Does NOT throw `System.InvalidOperationException` due to markup parsing errors under any conditions.

---

## Visual Presentation Contract

| Element | Format / Text (pt-BR) | Markup Escaping Policy |
| :--- | :--- | :--- |
| **Prompt Title** | `"[bold yellow]Selecione uma tarefa para executar:[/]" | Hardcoded Spectre markup |
| **Page Size** | `15` | Numeric limit |
| **More Choices Text** | `"[grey](Mova para cima e para baixo para ver mais opções)[/]" | Hardcoded Spectre markup |
| **Task Choice** | `{Name} [{typeTag}] - {Description}` | Fully escaped via `Markup.Escape` |
| **Exit Choice** | `[Sair]` or `[grey][[Sair]][/]` | Fully escaped (`Markup.Escape("[Sair]")` or `[[Sair]]`) |

---

## Behavioral Test Contract (Mocking Interface)

To enable automated testing without human console interaction, the mock console contract requires:
1. `IAnsiConsoleInput.ReadKey(bool intercept)`: Returns queued `ConsoleKeyInfo` instances (e.g., `ConsoleKey.Enter`, `ConsoleKey.DownArrow`).
2. `IAnsiConsole.Profile`: Returns a valid profile (width, height, color system).
3. `IAnsiConsole.Cursor`: Returns a no-op / mock cursor.
4. `IAnsiConsole.Write(IRenderable renderable)`: Writes rendered segments to a `StringWriter` buffer for verification.
