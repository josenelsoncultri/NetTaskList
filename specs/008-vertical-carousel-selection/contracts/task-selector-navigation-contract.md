# Interface Contract: Interactive Task Selector Navigation

**Feature**: `008-vertical-carousel-selection` | **Phase**: 1 | **Date**: 2026-11-03

## Presentation Layer Contract

### Public Method Signatures
```csharp
namespace NetTaskList.Presentation;

public static class InteractiveTaskSelector
{
    public static ScriptTask? SelectTask(
        IReadOnlyList<ScriptTask> tasks,
        IAnsiConsole? console = null);

    public static ScriptTask? SelectTask(
        IReadOnlyList<ScriptTask> tasks,
        ConfigurationSourceType sourceType,
        IAnsiConsole? console = null);
}
```

### Prompt Configuration Contract

The underlying `SelectionPrompt<TaskChoice>` instance constructed by `InteractiveTaskSelector.SelectTask` MUST satisfy:

| Property / Option | Required Value | Purpose |
| :--- | :--- | :--- |
| **Title** | `"[cyan]Selecionar uma tarefa para executar[/]"` | Portuguese prompt title |
| **PageSize** | `8` | Maximum choices visible per page |
| **MoreChoicesText** | `"[grey](Mova para cima e para baixo para ver mais opções)[/]"` | Portuguese scroll hint |
| **WrapAround** | `true` | **Enables vertical carousel navigation** |
| **UseConverter** | `choice => choice.DisplayName` | Renders escaped choice text |

---

## Keyboard Navigation & Boundary Behavior Contract

Given a choice list with $N$ items indexed $0, 1, \dots, N - 1$:

| Current Cursor Position | Key Pressed | Resulting Selected Choice | Behavior Description |
| :--- | :--- | :--- | :--- |
| **Index 0** (First Task) | `ConsoleKey.UpArrow` | **Index $N - 1$** (`[Sair]`) | **Backward Wrap**: Wraps from start of menu to end |
| **Index $N - 1$** (`[Sair]`) | `ConsoleKey.DownArrow` | **Index 0** (First Task) | **Forward Wrap**: Wraps from end of menu to start |
| **Index $i$** ($0 < i < N - 1$) | `ConsoleKey.UpArrow` | **Index $i - 1$** | Standard upward navigation |
| **Index $i$** ($0 \le i < N - 2$) | `ConsoleKey.DownArrow` | **Index $i + 1$** | Standard downward navigation |
| **Any Index $i$** | `ConsoleKey.Enter` | Confirm item $i$ | Returns `tasks[i]` if task choice; `null` if exit choice |

---

## Behavioral Test Contract (Mock Interface)

Unit tests validating this contract interact with `IAnsiConsole` through `MockAnsiConsole`:

1. **Backward Wrap Test**:
   - Initial state: index 0.
   - Keys: `PushKey(ConsoleKey.UpArrow)`, `PushKey(ConsoleKey.Enter)`.
   - Expected Output: `null` (since index $N-1$ is `[Sair]`).
2. **Forward Wrap Test**:
   - Initial state: index 0.
   - Keys: $N$ times `PushKey(ConsoleKey.DownArrow)`, `PushKey(ConsoleKey.Enter)`.
   - Expected Output: `tasks[0]` (wrapped around full circle back to first task).
3. **Continuous Multi-Cycle Test**:
   - Consecutive upward or downward key sequences exceeding list length continue wrapping cyclically without throwing exceptions or dead-ending.

