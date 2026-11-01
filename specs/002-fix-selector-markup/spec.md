# Feature Specification: Resilient Interactive Task Selection

**Feature Branch**: `002-fix-selector-markup`

**Created**: 2026-11-01

**Status**: Draft

**Input**: User description: "Corrigir exceção de markup malformado no InteractiveTaskSelector ao renderizar a opção de saída. Contexto do Problema: Ao executar o comando nxtl sem argumentos em ambiente interativo, o terminal renderiza a tabela de tarefas com sucesso, mas aborta com System.InvalidOperationException: Encountered malformed markup tag at position 8 originada em Spectre.Console.SelectionPrompt chamado por InteractiveTaskSelector.SelectTask. Causa Raiz: No arquivo src/NetTaskList/Presentation/InteractiveTaskSelector.cs (linha 26), a escolha para cancelamento foi adicionada como texto literal '[Sair]'. Requisitos: Tratamento de Markup na Opção de Saída, Resiliência a Caracteres Especiais, Cobertura de Testes."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Graceful Interactive Menu Launch and Exit (Priority: P1)

As a developer running `nxtl` without arguments in an interactive terminal, I want the interactive task selection menu to display reliably and include a safe exit option so that I can choose a task to run or exit without unexpected application crashes.

**Why this priority**: Crashing upon launching the default interactive mode is a critical blocker that prevents users from using the CLI tool interactively.

**Independent Test**: Can be fully tested by launching `nxtl` in an interactive console, verifying that the interactive choice list appears with all configured tasks and an exit option, choosing the exit option, and confirming that the application exits cleanly with code 0.

**Acceptance Scenarios**:

1. **Given** a valid configuration with one or more tasks, **When** the user runs `nxtl` in an interactive terminal session, **Then** the interactive selection prompt appears without any unhandled exceptions, displaying all tasks along with an option to exit.
2. **Given** the interactive selection prompt is displayed, **When** the user selects the exit option, **Then** the application prints a Portuguese cancellation message ("Operação cancelada pelo usuário.") and terminates with exit code 0.

---

### User Story 2 - Resilient Display of Tasks with Special Characters (Priority: P2)

As a developer defining tasks with bracketed tags, version numbers, or formatted labels (e.g., `[docker]`, `[v1.0]`, `[test]`), I want the interactive menu to safely display all text verbatim without interpreting bracketed terms as terminal style directives.

**Why this priority**: Users frequently use brackets in commands, labels, and descriptions. Terminal prompts must treat these as literal text to avoid runtime syntax parse errors.

**Independent Test**: Can be tested by configuring tasks whose names, commands, or descriptions contain single and nested square brackets, then verifying that the interactive prompt renders and selects them without throwing formatting errors.

**Acceptance Scenarios**:

1. **Given** a task with square brackets in its name or description (e.g., `[build:prod]`), **When** the interactive prompt renders, **Then** the text is displayed with literal brackets intact and no formatting errors occur.
2. **Given** a task containing characters that resemble formatting tags, **When** the user selects that task, **Then** the application correctly resolves the chosen task and proceeds without error.

---

### Edge Cases

- **Task names or descriptions containing nested or unclosed brackets**: When a task has inputs like `build [nested [brackets]`, the interactive prompt must treat them strictly as literal text.
- **Tasks containing color keywords in brackets**: When a task or description contains text like `[red]` or `[green]` meant literally, the text must not unintentionally trigger color changes or style tokenization errors.
- **Empty task list**: If no tasks exist, the CLI already displays an informative warning and exits before entering the interactive selector.
- **Non-interactive / redirected streams**: If standard input is redirected, the CLI informs the user that interactive mode is unavailable and guides them to use direct command flags without invoking the interactive selector.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST render all options in the interactive selection prompt—including task entries, type indicators, descriptions, and the exit/cancel action—without triggering terminal markup parsing errors.
- **FR-002**: The system MUST provide an explicit, properly formatted exit choice in the interactive selection menu that allows users to cancel without selecting a task.
- **FR-003**: The system MUST treat all user-defined strings (task names, descriptions, commands) and fixed option labels as literal text in interactive prompts, ensuring special characters are escaped from terminal formatting engines.
- **FR-004**: When the user selects the exit option, the system MUST display a user-friendly cancellation notice in Brazilian Portuguese (`pt-BR`) and return exit code 0.
- **FR-005**: The interactive task selection logic MUST be verifiable through automated tests using console abstractions without causing terminal markup exceptions.

### Key Entities *(include if feature involves data)*

- **Interactive Choice**: Represents an selectable item in the interactive menu, encapsulating either a reference to an executable script task or a null reference denoting the cancellation/exit action, along with its display label.
- **Script Task**: The configured task containing a name, task type (tool or csharp script), command string, and an optional description.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of interactive menu invocations launch and render without unhandled formatting exceptions across all valid task configurations.
- **SC-002**: 100% of choices containing brackets or markup characters (including `[Sair]`, `[tool]`, and custom user brackets) render safely as literal text.
- **SC-003**: Users can launch the interactive selection, view options, and exit cleanly in under 2 seconds.
- **SC-004**: Automated test coverage for the interactive selection presentation layer passes with 0 failures.

## Assumptions

- The terminal environment supports ANSI cursor navigation and selection when input is not redirected.
- User-facing terminal messages remain strictly in Brazilian Portuguese (`pt-BR`) in compliance with project governance.
- The default option for exiting the menu is labeled in Brazilian Portuguese as `[Sair]` or a styled equivalent that safely escapes the brackets.
