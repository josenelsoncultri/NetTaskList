# Quickstart: Validating Vertical Carousel Terminal Selection

**Feature**: `008-vertical-carousel-selection` | **Phase**: 1 | **Date**: 2026-11-03

## Prerequisites

- .NET 10 SDK installed (`net10.0`).
- Working directory: repository root (`NetTaskList`).

---

## Validation Scenario 1: Automated Unit Tests

Run the interactive task selector unit tests:

```bash
XDG_RUNTIME_DIR=/tmp dotnet test --filter "FullyQualifiedName~InteractiveTaskSelectorTests" --no-restore
```

### Expected Result
- 100% of tests pass cleanly.
- Test suite validates:
  1. **Backward Wrap (Up from Index 0)**: Pressing Up arrow immediately wraps around from the first task to the last option (`[Sair]`), and pressing Enter returns `null`.
  2. **Forward Wrap (Down past End)**: Pressing Down arrow through all choices (N times) wraps around from `[Sair]` back to the first task, and pressing Enter returns that first task.
  3. **Multi-Cycle Carousel**: Repeated Up or Down arrows continue cycling continuously without blocking or crashing.

---

## Validation Scenario 2: Manual Interactive Terminal Execution

### Setup
Create or verify a test `nxtl.json` file in the current directory:

```json
{
  "tasks": [
    {
      "name": "build",
      "type": "tool",
      "command": "dotnet build",
      "description": "Compila o projeto"
    },
    {
      "name": "test",
      "type": "tool",
      "command": "dotnet test",
      "description": "Executa testes"
    }
  ]
}
```

### Execution
Run the CLI tool in interactive mode:

```bash
XDG_RUNTIME_DIR=/tmp dotnet run --project src/NetTaskList/NetTaskList.csproj --no-restore
```

### Verification Steps
1. **Backward Wrap Verification**:
   - The selection prompt appears with `build` highlighted.
   - Press the **Up Arrow** key once.
   - **Verification**: The cursor jumps directly to the last option `[Sair]`.
   - Press **Enter**.
   - **Verification**: The application cancels cleanly: `"Operação cancelada pelo usuário."` with exit code `0`.
2. **Forward Wrap Verification**:
   - Run the command again.
   - Press the **Down Arrow** key 3 times (`build` -> `test` -> `[Sair]` -> `build`).
   - **Verification**: The cursor wraps around from `[Sair]` back to `build`.
   - Press **Enter**.
   - **Verification**: The `build` task is selected and executed.

