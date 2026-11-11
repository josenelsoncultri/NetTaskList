# Data Model & Validation Rules: Task Command Shortcuts

**Feature**: Task Command Shortcuts and Recursion Detection in `nxtl.json`  
**Branch**: `009-task-command-shortcuts`  
**Date**: 2026-11-10

---

## 1. Domain Model Entities

### `TaskCommandStep` (`NetTaskList.Domain.Models`)
Represents an individual executable step within a task's sequence, which can either be a native shell command line or a shortcut reference to another task.

| Property | Type | Description |
|---|---|---|
| `CommandLine` | `string?` | Raw shell command to execute when `IsShortcut == false`. |
| `TargetTaskName` | `string?` | Name of the referenced task to invoke when `IsShortcut == true`. |
| `IsShortcut` | `bool` | `true` if this step references another task; `false` if it is a shell command. |

**Invariants & Factory Methods**:
- `TaskCommandStep.FromCommandLine(string commandLine)`: Requires non-null, non-empty command line. Sets `TargetTaskName = null`.
- `TaskCommandStep.FromShortcut(string targetTaskName)`: Requires non-null, non-empty task name. Sets `CommandLine = null`.
- Exactly one of `CommandLine` or `TargetTaskName` is non-null.

---

### `ScriptTask` (`NetTaskList.Domain.Models`)
Represents a configured task with its execution steps, metadata, and category.

| Property | Type | Description |
|---|---|---|
| `Name` | `string` | Unique task identifier. |
| `CommandSteps` | `IReadOnlyList<TaskCommandStep>` | Ordered list of execution steps (shell commands and/or task shortcuts). |
| `Commands` | `IReadOnlyList<string>` | Backwards-compatible representation of steps as strings. |
| `Command` | `string` | First step's string representation or empty string if no steps. |
| `Type` | `TaskType` | Task category: `TaskType.Tool` or `TaskType.CSharp`. |
| `Description` | `string?` | Optional human-readable description for terminal tables and selection prompts. |

---

### `TaskConfiguration` (`NetTaskList.Domain.Models`)
Aggregates all parsed tasks from the loaded configuration file.

| Property | Type | Description |
|---|---|---|
| `FilePath` | `string` | Absolute path to the configuration file (`nxtl.json` or `package.json`). |
| `Tasks` | `IReadOnlyList<ScriptTask>` | All configured tasks. |
| `SourceType` | `ConfigurationSourceType` | `ConfigurationSourceType.NxtlJson` or `ConfigurationSourceType.PackageJson`. |

---

## 2. DTOs & Serialization Layer

### `TaskCommandDto` (`NetTaskList.Infrastructure.Dtos`)
Intermediate DTO representing a raw deserialized command step.

| Property | Type | Description |
|---|---|---|
| `RawCommand` | `string?` | Deserialized string command. |
| `TargetTask` | `string?` | Deserialized `"task"` property value from `{ "task": "<name>" }`. |
| `IsShortcut` | `bool` | Computed: `!string.IsNullOrWhiteSpace(TargetTask)`. |

---

### `ScriptTaskDto` (`NetTaskList.Infrastructure.Dtos`)
DTO mapped from `nxtl.json` tasks or scripts array.

| Property | JSON Field | Type | Converter / Attributes |
|---|---|---|---|
| `Name` | `name` | `string?` | `[JsonPropertyName("name")]` |
| `Commands` | `command` | `List<TaskCommandDto>?` | `[JsonConverter(typeof(CommandPropertyJsonConverter))]` |
| `Type` | `type` | `string?` | `[JsonPropertyName("type")]` |
| `Description` | `description` | `string?` | `[JsonPropertyName("description")]` |

---

## 3. Validation Rules

| Rule ID | Level | Target | Condition / Failure Message |
|---|---|---|---|
| **VR-001** | Parsing/Config | `Name` | Duplicate task name detected across the configuration. Message: *"O script com o nome '{name}' está duplicado..."* |
| **VR-002** | Parsing/Config | `Name` | Missing or blank task name. Message: *"Foi encontrada uma tarefa sem nome válido..."* |
| **VR-003** | Parsing/Config | `Commands` | Missing or empty command definition. Message: *"A tarefa '{name}' não define um comando a ser executado."* |
| **VR-004** | Parsing/Config | `Type` | Unrecognized type (must be `'tool'` or `'csharp'`). Message: *"A tarefa '{name}' possui o tipo '{type}' não reconhecido..."* |
| **VR-005** | Parsing/Config | `Commands` (Shortcut) | Task shortcut configured inside a `type: "csharp"` task. Message: *"A tarefa '{name}' é do tipo 'csharp' e não pode conter atalhos para outras tarefas."* |
| **VR-006** | Parsing/Config | `Commands` (Shortcut) | Task shortcut with empty or whitespace target name (`{ "task": "" }`). Message: *"A tarefa '{name}' possui um atalho com nome de tarefa vazio."* |
| **VR-007** | Pre-Execution | Reference Tree | Referenced task does not exist in `nxtl.json`. Message: *"A tarefa referenciada '{targetName}' não foi encontrada no arquivo de configuração."* |
| **VR-008** | Pre-Execution | Reference Tree | Referenced task has `type: "csharp"`. Message: *"A tarefa '{name}' tenta referenciar '{targetName}', que é um script C#. Tarefas C# não podem ser referenciadas como atalhos."* |
| **VR-009** | Pre-Execution & Runtime | Reference Tree | Circular reference loop detected (`A -> A`, `A -> B -> A`, or `A -> ... -> A`). Message: *"Referência circular detectada entre tarefas: {chain}. A execução causaria um loop infinito no nxtl."* |

---

## 4. State Transitions & Call Graph Flow

```text
[CLI Invocado: nxtl --run <task>]
            │
            ▼
[Validação Estática da Árvore de Atalhos (DFS)]
  ├── Ciclo detectado? ────────────────────────► [Exibe Erro de Recursão em pt-BR & Aborta (Exit 1)]
  ├── Tarefa referenciada não existe? ────────► [Exibe Erro de Tarefa Inexistente em pt-BR & Aborta (Exit 1)]
  └── Referencia tarefa C#? ──────────────────► [Exibe Erro de Tarefa C# em pt-BR & Aborta (Exit 1)]
            │ (Validação aprovada)
            ▼
[Execução Sequencial dos Passos]
  ├── Passo: Shell Command
  │     ├── Exibe: "Comando: <linha>"
  │     └── Executa via IProcessExecutor
  │           └── Falhou (Exit != 0)? ────────► [Exibe Interrupção & Propaga Exit Code]
  │
  └── Passo: Task Shortcut { task: <target> }
        ├── Exibe: "Executando atalho: <target>"
        ├── Empilha no ActiveTaskStack (Runtime Guard)
        ├── Executa passos da tarefa alvo recursivamente
        └── Desempilha do ActiveTaskStack
```
