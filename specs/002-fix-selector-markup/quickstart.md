# Quickstart: Validating Resilient Interactive Task Selection

**Feature**: `002-fix-selector-markup` | **Phase**: 1 | **Date**: 2026-11-01

## Prerequisites
- .NET 10 SDK installed.
- Repository root directory: `d:\Projetos\C#\NetTaskList`.

---

## Validation Scenario 1: Automated Unit Tests

Run all unit tests verifying the interactive selector with special characters and the exit choice:

```powershell
dotnet test --filter "FullyQualifiedName~InteractiveTaskSelectorTests" --no-restore /p:NuGetAudit=false
```

### Expected Result
- All tests in `InteractiveTaskSelectorTests` pass with 0 errors.
- Test cases cover:
  1. Selecting the exit option (`[Sair]`) returns `null`.
  2. Selecting a task option returns the corresponding `ScriptTask`.
  3. Tasks containing literal brackets in name, description, and command (e.g., `[build:prod]`, `[docker] run`) render without `InvalidOperationException`.

---

## Validation Scenario 2: Manual Terminal Execution

### Setup
Ensure a sample `nxtl.json` exists with tasks containing brackets:
```json
{
  "tasks": [
    {
      "name": "build",
      "type": "tool",
      "command": "dotnet build",
      "description": "Compila a solução do projeto"
    },
    {
      "name": "test [fast]",
      "type": "tool",
      "command": "dotnet test --filter Category=Unit",
      "description": "Executa testes unitários [rápidos]"
    }
  ]
}
```

### Execution
Run the CLI tool without flags in an interactive console:
```powershell
dotnet run --project src/NetTaskList/NetTaskList.csproj --no-restore /p:NuGetAudit=false
```

### Expected Behavior
1. The tasks table renders cleanly.
2. The selection prompt appears with:
   - `build [tool] - Compila a solução do projeto`
   - `test [fast] [tool] - Executa testes unitários [rápidos]`
   - `[Sair]`
3. Pressing `DownArrow` to highlight `[Sair]` and `Enter` cancels gracefully with message:
   `Operação cancelada pelo usuário.`
   Exit code is `0`.
