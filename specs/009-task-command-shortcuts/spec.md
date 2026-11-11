# Feature Specification: Task Command Shortcuts and Recursion Detection in nxtl.json

**Feature Branch**: `009-task-command-shortcuts`

**Created**: 2026-11-10

**Status**: Draft

**Input**: User description:
"- Permitir que os comandos de ferramentas do nxtl.json sejam atalhos de comandos do próprio nxtl.json
- Quando for um atalho de uma tarefa do nxtl.json, especificar um objeto (no comando único ou em array) com a seguinte descrição: { \"task\": \"<nome da tarefa>\" }
- Caso seja referenciada uma tarefa em um script, validar ao rodar o script se a mesma entraria em loop, ou seja, se eu referenciar uma task chamada \"build\" dentro de uma chamada \"clean\", não posso referenciar dentro da \"clean\" uma chamada \"build\". Validar aqui também caso seja uma árvore, ou seja, task dentro de task, se nessa situação entrar em loop o comando nxtl deve ser interrompido e indicar a recursão, dando o nome das tasks e apontando que a chamada de uma faria o nxtl entrar em loop"

## Clarifications

### Session 2026-11-10

- Q: Quando deve ser realizada a detecção de recursão e ciclos entre as tarefas referenciadas? → A: Validação em dois níveis: validação prévia completa (upfront) de toda a árvore antes de iniciar qualquer comando e manutenção de pilha de segurança em tempo de execução.
- Q: Quais tipos de tarefas do nxtl.json podem ser referenciadas por um atalho `{ "task": "<nome>" }`? → A: Apenas tarefas do tipo `tool`. Tarefas `csharp` só podem iniciar diretamente o arquivo em questão e não podem ser referenciadas como atalhos nem conter atalhos.
- Q: Como o terminal deve exibir visualmente a execução de um atalho de tarefa? → A: Linha de status explícita informando o acionamento do atalho (ex: `Executando atalho: <nome>`) antes de executar os comandos da tarefa referenciada.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Executing a Task Shortcut (Priority: P1)

A developer configuring `nxtl.json` wants to reuse existing `tool` tasks as shortcuts without duplicating command strings. They specify `{ "task": "<target-task>" }` as the command of a `tool` task. When running this task with `nxtl`, the tool detects the shortcut, prints an explicit status line indicating the shortcut activation, resolves the target `tool` task, and executes its commands.

**Why this priority**: Core value of the feature — allows task reuse and composition, avoiding command duplication across tasks.

**Independent Test**: Can be tested by creating two `tool` tasks in `nxtl.json` where task B's command is `{ "task": "A" }`, running `nxtl` for task B, and verifying that the shortcut status line is printed and task A's command executes successfully.

**Acceptance Scenarios**:

1. **Given** a `tool` task `A` with command `echo "hello from A"` and a `tool` task `B` with command `{ "task": "A" }`, **When** the user executes task `B`, **Then** the system outputs a status line indicating execution of shortcut `A`, executes task `A`'s command, and outputs `hello from A`.
2. **Given** a `tool` task `A` with a sequential command list and a `tool` task `B` referencing `A`, **When** the user executes task `B`, **Then** the system announces shortcut `A` and all sequential commands of `A` are executed in order.

---

### User Story 2 - Detecting and Preventing Circular Reference Loops (Priority: P1)

A developer accidentally configures task references that form an infinite loop (e.g., direct self-reference, mutual reference between two tasks, or multi-step circular dependencies across an execution tree). When running `nxtl`, the system validates the reference chain upfront and maintains a runtime stack guard, halts immediately before executing any infinite loop or cyclic command, and displays a clear error in Brazilian Portuguese (`pt-BR`) specifying the recursion path and indicating that the call would cause an infinite loop.

**Why this priority**: Critical safety mechanism — prevents terminal lockups, stack overflows, or uncontrolled runaway processes when circular references exist.

**Independent Test**: Can be tested by configuring cyclical tasks (e.g., `clean` referencing `build` and `build` referencing `clean`), running either task, and verifying that execution halts with the expected recursion error message displaying the cycle path.

**Acceptance Scenarios**:

1. **Given** task `build` referencing task `clean` and task `clean` referencing task `build`, **When** the user runs `nxtl` for either task, **Then** upfront validation halts execution immediately, zero commands are executed, and a message in Portuguese explains that recursion was detected with the task chain (e.g., `clean -> build -> clean`).
2. **Given** task `A` referencing `A` directly (`{ "task": "A" }`), **When** the user runs task `A`, **Then** execution is halted immediately indicating self-referential loop before running any commands.
3. **Given** a multi-level tree `A` -> `B` -> `C` -> `A`, **When** the user runs task `A`, **Then** execution is halted before entering the loop and the full chain `A -> B -> C -> A` is displayed to the user.

---

### User Story 3 - Sequential Execution Combining Commands and Shortcuts (Priority: P2)

A developer wants to define a composite workflow task where some steps are raw shell commands and other steps are shortcuts to other `tool` tasks (e.g., `["echo Starting", { "task": "clean" }, { "task": "build" }, "echo Done"]`). When running the composite task, `nxtl` executes each step sequentially, resolving each task shortcut in place.

**Why this priority**: Extends task composition to multi-step pipelines, enabling powerful task orchestration.

**Independent Test**: Can be tested by defining an array of mixed string commands and task reference objects, running the task, and verifying that each command and shortcut executes in sequence.

**Acceptance Scenarios**:

1. **Given** a `tool` task with an array command containing `["echo step 1", { "task": "subtask" }, "echo step 3"]`, **When** the user runs the task, **Then** "step 1" executes, then the shortcut announcement and `subtask` execute, and then "step 3" executes in order.
2. **Given** a composite task where a referenced task fails with a non-zero exit code, **When** the failure occurs, **Then** subsequent steps in the composite task are cancelled and the failure is reported.

---

### User Story 4 - Handling Invalid or Incompatible References (Priority: P3)

A developer mistypes a task name in a shortcut object (e.g., `{ "task": "non-existent" }`) or attempts to reference a `type: "csharp"` script task as a shortcut. When running the task, `nxtl` validates the reference upfront, informs the user in Portuguese that the target task was not found or is not a `tool` task, and stops execution with a non-zero exit code.

**Why this priority**: Provides clear diagnostic feedback for configuration errors, preventing silent failures or obscure crashes.

**Independent Test**: Can be tested by configuring a task referencing a non-existent task name or a C# script task, and verifying that the CLI outputs a descriptive error message in Portuguese.

**Acceptance Scenarios**:

1. **Given** a task referencing `{ "task": "missing-task" }` which does not exist in `nxtl.json`, **When** the user runs the task, **Then** execution halts immediately with a clear diagnostic message in Portuguese indicating that task "missing-task" was not found.
2. **Given** a task referencing `{ "task": "csharp-script" }` where `csharp-script` is `type: "csharp"`, **When** the user runs the task, **Then** execution halts immediately with a diagnostic message in Portuguese stating that C# script tasks cannot be referenced as shortcuts.

---

### Edge Cases

- **Direct Self-Reference**: A task whose command references itself directly (`{ "task": "<self>" }`). Must be detected during upfront validation before any command is executed.
- **Deep / Branching Cycle (Tree recursion)**: Task `A` calls `B` and `C`; `C` calls `D`; `D` calls `B`. The cycle `B -> C -> D -> B` must be identified with the exact sub-chain displayed.
- **Diamond Dependencies (Non-Cyclic DAG)**: Task `A` calls both `B` and `C`, and both `B` and `C` reference task `D`. Because `D` does not reference any active ancestor in the call chain, this is a valid Directed Acyclic Graph (DAG) and MUST NOT be flagged as a recursion loop.
- **Attempting to Reference a C# Script Task**: A shortcut `{ "task": "<csharp-task>" }` targeting a `type: "csharp"` task must be rejected during upfront validation, as C# script tasks only execute files directly.
- **Attempting to Declare Shortcuts in a C# Script Task**: A `type: "csharp"` task whose command is or contains `{ "task": "..." }` must be rejected during configuration validation.
- **Empty / Whitespace Task Name**: A shortcut specifying `{ "task": "" }` or `{ "task": "   " }` must be caught during validation as an invalid task reference with an informative error message.
- **Cascading Step Failure**: If task `B` (referenced by task `A`) fails during one of its commands, the entire sequence must stop immediately and propagate the failure code up to task `A`.
- **Single vs. Array Shortcut Format**: A task shortcut can be declared as a single object (`"command": { "task": "target" }`) or as an item in an array (`"command": [{ "task": "target" }]`). Both representations must behave identically.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow `tool` tasks (`type: "tool"`) in `nxtl.json` to have command steps declared as task shortcut objects of the form `{ "task": "<target-task-name>" }`.
- **FR-002**: System MUST allow task shortcut objects to be placed within sequential command arrays in `nxtl.json`, alongside standard string commands or other task shortcut objects.
- **FR-003**: System MUST resolve and execute the commands of the referenced `tool` task in place when a task shortcut is encountered during task execution.
- **FR-004**: System MUST perform two-phase cycle detection on task shortcut references: a full upfront tree validation before launching any command of the requested task, plus an active call stack guard during runtime execution to ensure no circular references (`A -> A`, `A -> B -> A`, or `A -> ... -> A`) can execute.
- **FR-005**: If a recursion cycle is detected during either the upfront validation or the runtime execution guard, the system MUST abort execution immediately before executing any command of the cyclic path.
- **FR-006**: Upon detecting a cycle, the system MUST display an explicit error message in Brazilian Portuguese (`pt-BR`) specifying the recursive task chain (e.g., `clean -> build -> clean`) and stating that executing the task would result in an infinite loop.
- **FR-007**: If a task shortcut references a task name that is not defined in the configuration, the system MUST halt execution and display an error message in Portuguese indicating that the referenced task was not found.
- **FR-008**: System MUST reject task shortcuts that reference tasks of type `csharp` (`type: "csharp"`), as C# tasks only execute script files directly.
- **FR-009**: System MUST disallow declaring task shortcut objects inside `type: "csharp"` tasks.
- **FR-010**: System MUST support non-cyclic shared task references (such as diamond dependencies where two sibling tasks reference the same dependency task) without triggering false recursion errors.
- **FR-011**: If an individual command within a referenced task fails (non-zero exit code), the system MUST halt execution of the remaining commands and propagate the failure.
- **FR-012**: System MUST validate the configuration against a schema that accepts both string commands and task shortcut objects (`{ "task": "string" }`) as single commands or array items for `tool` tasks.
- **FR-013**: System MUST output an explicit status line in the terminal (e.g., `Executando atalho: <nome>`) prior to executing the commands of a referenced task shortcut.

### Key Entities *(include if feature involves data)*

- **Task**: An individually named executable configuration entry containing a name, command(s), type, and optional description.
- **Task Shortcut**: A command item representing an invocation alias to another configured `tool` task, specified as an object with a `task` property containing the target task name.
- **Call Chain / Execution Stack**: The ordered sequence of active tasks currently being resolved or executed, used to detect circular paths and prevent infinite loops.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of valid single and sequential task shortcuts execute their target `tool` task commands successfully.
- **SC-002**: 100% of circular task references (direct, mutual, or tree cycles) are detected and halted with zero infinite loops, process hangs, or stack overflow crashes, aborting prior to the execution of any cyclic command.
- **SC-003**: In 100% of detected recursion events, the error output displays the exact chain of tasks involved and a descriptive explanation in Portuguese.
- **SC-004**: 100% of diamond/shared dependency structures execute without false positive recursion errors.
- **SC-005**: Missing task references and invalid references to `csharp` tasks are rejected with an explanatory message in Portuguese in under 1 second without attempting process execution.
- **SC-006**: Terminal output clearly indicates every task shortcut activation before executing its underlying commands.

## Assumptions

- Task shortcuts are supported specifically in `nxtl.json` configurations (the declarative format of `nxtl`).
- Task shortcuts can only reference and be declared within `tool` tasks (`type: "tool"`). C# tasks (`type: "csharp"`) only execute script files directly.
- Matching of task names in shortcuts follows the same case sensitivity rules applied to task lookups across `nxtl`.
- Diamond dependency patterns (e.g., task A executes B and C, where both B and C execute D) are valid and allowed because D finishes before the next branch runs; recursion detection tracks tasks currently active on the execution stack.
- All terminal messages, errors, warnings, and prompts adhere to the project constitution's requirement for Brazilian Portuguese (`pt-BR`).
