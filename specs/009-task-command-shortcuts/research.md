# Technical Research & Architecture Decisions: Task Command Shortcuts

**Feature**: Task Command Shortcuts and Recursion Detection in `nxtl.json`  
**Branch**: `009-task-command-shortcuts`  
**Date**: 2026-11-10

---

## 1. DTO & Polymorphic JSON Deserialization

### Context
In `nxtl.json`, the `command` property of a `tool` task must support three shapes:
1. Single command string: `"dotnet build"`
2. Single shortcut object: `{ "task": "clean" }`
3. Sequential array of commands and/or shortcut objects: `["dotnet clean", { "task": "build" }, "dotnet test"]`

### Decision
Extend `CommandPropertyJsonConverter` (implementing `JsonConverter<List<TaskCommandDto>>`) and define `TaskCommandDto` in `NetTaskList.Infrastructure.Dtos`:
```csharp
public sealed class TaskCommandDto
{
    public string? RawCommand { get; set; }
    public string? TargetTask { get; set; }
    public bool IsShortcut => !string.IsNullOrWhiteSpace(TargetTask);
}
```

The converter handles:
- `JsonTokenType.String`: creates a `TaskCommandDto` with `RawCommand = reader.GetString()`.
- `JsonTokenType.StartObject`: inspects properties; when `"task"` is encountered, reads its string value into `TargetTask`. Throws a descriptive `JsonException` if the object is empty or contains unrecognized properties.
- `JsonTokenType.StartArray`: iterates elements, allowing each item to be either `JsonTokenType.String` or `JsonTokenType.StartObject` (processed as above).
- `JsonTokenType.Null`: returns `null` or empty list.

Writing (`Write` method):
- If list has 1 item and `RawCommand != null`: writes JSON string.
- If list has 1 item and `TargetTask != null`: writes JSON object `{"task": "..."}`.
- Otherwise: writes JSON array with string or object items.

### Rationale
- Native AOT Trim-Safe: Completely source-generator friendly, requires zero runtime reflection or dynamic code generation.
- Full backwards compatibility with existing string and array-of-string configurations.

### Alternatives Considered
- *Prefixing strings (e.g. `"task:build"`)*: Rejected because the user specification strictly requires an object with `{ "task": "<nome da tarefa>" }`.
- *Separate top-level properties (e.g. `"shortcut": "clean"`)*: Rejected because shortcuts must be composable within sequential command arrays.

---

## 2. Domain Modeling of Task Commands and Steps

### Context
`ScriptTask` in `NetTaskList.Domain.Models` currently maintains `Commands: IReadOnlyList<string>`. We need a domain model that cleanly represents both raw shell commands and task shortcuts without introducing heavy abstractions.

### Decision
Introduce `TaskCommandStep` in `NetTaskList.Domain.Models`:
```csharp
public sealed class TaskCommandStep
{
    public string? CommandLine { get; }
    public string? TargetTaskName { get; }
    public bool IsShortcut => !string.IsNullOrWhiteSpace(TargetTaskName);

    public TaskCommandStep(string commandLine)
    {
        CommandLine = commandLine ?? throw new ArgumentNullException(nameof(commandLine));
        TargetTaskName = null;
    }

    public TaskCommandStep(string targetTaskName, bool isShortcut)
    {
        if (!isShortcut) throw new ArgumentException("isShortcut must be true for task shortcuts.", nameof(isShortcut));
        CommandLine = null;
        TargetTaskName = targetTaskName ?? throw new ArgumentNullException(nameof(targetTaskName));
    }

    public static TaskCommandStep FromCommandLine(string commandLine) => new(commandLine);
    public static TaskCommandStep FromShortcut(string targetTaskName) => new(targetTaskName, true);
}
```

Update `ScriptTask`:
- Add `public IReadOnlyList<TaskCommandStep> CommandSteps { get; }`.
- Maintain `Commands` property: returns the command line string for shell commands, or `$"task: {TargetTaskName}"` for shortcuts, preserving backwards compatibility for string-based consumers (such as tests or selector summaries).
- Add constructor overload accepting `IReadOnlyList<TaskCommandStep>`.

### Rationale
- Minimalist, cohesive, and follows Principle IV (YAGNI & KISS). Avoids class explosion while providing strong typing and immutability.

---

## 3. Cycle and Recursion Detection Strategy

### Context
The user specified that task recursion must be detected and halted when running a script, including deep tree references (`clean -> build -> clean`, `A -> B -> C -> A`, or self-references `A -> A`). Furthermore, diamond dependencies (where task A calls B and C, and both B and C call shared task D) must execute without false cycle errors.

### Decision
Two-phase cycle validation:

#### Phase 1: Upfront Call Tree Validation
Before executing any command of the requested task:
1. Traverse the shortcut reference graph using Depth-First Search (DFS).
2. Maintain an ordered list of tasks currently on the traversal path (`List<string> traversalPath`).
3. For each task step:
   - If it is a shortcut to `targetName`:
     - **Cycle Check**: Check if `targetName` already exists in `traversalPath`:
       - If yes: a cycle is found! Construct cycle chain: `string.Join(" -> ", [..traversalPath, targetName])`.
       - Format error: `[bold red]Erro:[/] Referência circular detectada entre tarefas: [yellow]{cycleChain}[/]. A execução causaria um loop infinito no nxtl.`
       - Abort execution immediately with exit code 1.
     - **Existence Check**: Look up `targetName` in configured tasks:
       - If not found: `[bold red]Erro:[/] A tarefa referenciada '[yellow]{targetName}[/]' não foi encontrada no arquivo de configuração.`
       - Abort execution immediately with exit code 1.
     - **Type Check**: Verify `targetTask.Type == TaskType.Tool`:
       - If `targetTask.Type == TaskType.CSharp`: `[bold red]Erro:[/] A tarefa '[yellow]{task.Name}[/]' tenta referenciar '[yellow]{targetName}[/]', que é um script C#. Tarefas C# não podem ser referenciadas como atalhos.`
       - Abort execution immediately with exit code 1.
     - Push `targetName` onto `traversalPath` and recurse.
     - Pop `targetName` from `traversalPath` upon returning (backtracking).

#### Diamond Dependency Handling
Because nodes are popped from `traversalPath` upon branch completion, a task D referenced by both B and C in `A -> B -> D` and `A -> C -> D` is only active while B's subtree is being examined. When C's subtree is examined, D is not in `traversalPath`. Therefore, DAG diamond dependencies validate cleanly without false cycle errors.

#### Phase 2: Runtime Execution Stack Guard
In `TaskExecutionCoordinator`, maintain an active task stack (`Stack<string> activeStack`):
- Push task name when entering task execution.
- If task name is already on `activeStack`, halt immediately with recursion error.
- Pop task name upon task completion.

---

## 4. Terminal Presentation & Localization (`pt-BR`)

### Context
Constitution Principle I requires all terminal output to be in Brazilian Portuguese (`pt-BR`).

### Decision
1. **Shortcut Activation Feedback**:
   Before executing commands of a referenced shortcut:
   ```csharp
   _console.MarkupLine($"[bold cyan]Executando atalho:[/] [bold white]{Markup.Escape(shortcutName)}[/]");
   ```
2. **Interactive Selector Summary**:
   In `InteractiveTaskSelector`, if a task has a single command step that is a shortcut, its description defaults to `atalho: {targetName}` (unless an explicit `description` is configured). If it has multiple steps, it displays `<sem descrição>`.

---

## 5. JSON Schema Update (`nxtl-v1.schema.json`)

### Context
`nxtl-v1.schema.json` defines the contract for editor intellisense and schema validation.

### Decision
Update `properties.command`:
```json
"command": {
  "description": "Comando do sistema operacional, atalho de tarefa, ou lista sequencial de comandos/atalhos.",
  "oneOf": [
    {
      "type": "string",
      "minLength": 1,
      "description": "Comando único de execução."
    },
    {
      "$ref": "#/$defs/taskShortcut",
      "description": "Atalho para outra tarefa de ferramenta configurada."
    },
    {
      "type": "array",
      "minItems": 1,
      "items": {
        "oneOf": [
          {
            "type": "string",
            "minLength": 1
          },
          {
            "$ref": "#/$defs/taskShortcut"
          }
        ]
      },
      "description": "Lista sequencial de comandos e/ou atalhos de tarefas executados um por vez."
    }
  ]
}
```
Define `$defs.taskShortcut`:
```json
"taskShortcut": {
  "type": "object",
  "title": "TaskShortcut",
  "description": "Referência de atalho para outra tarefa configurada no nxtl.json.",
  "properties": {
    "task": {
      "type": "string",
      "minLength": 1,
      "description": "Nome da tarefa de destino a ser executada."
    }
  },
  "required": ["task"],
  "additionalProperties": false
}
```
