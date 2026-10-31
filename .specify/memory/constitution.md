<!--
Sync Impact Report:
- Version change: 1.0.0 → 1.1.0
- List of modified principles:
  - PRINCIPLE_5: V. Tool Packaging & Distribution (.NET Global Tool - nxtl) (explicit src/ placement constraint)
- Added sections:
  - Project Structure & Directory Layout (under Architecture & Technology Constraints)
- Removed sections: None
- Follow-up TODOs: None
-->

# NetTaskList (nxtl) Constitution

## Core Principles

### I. Rich Terminal UX (Spectre.Console & Localization)
The CLI tool MUST utilize `Spectre.Console` to deliver an interactive, rich terminal user experience (including interactive task selection prompts, status spinners, formatted tables, and colored status badges).
- **Localization (pt-BR)**: All user-facing terminal outputs, prompts, interactive menus, help messages, warnings, and error descriptions MUST be written in Brazilian Portuguese (`pt-BR`).
- **Codebase Language (English)**: All code symbols, including class names, interfaces, methods, variables, unit tests, and internal developer comments, MUST be named and authored strictly in English.

### II. Declarative Script Configuration (JSON / YAML)
The tool MUST parse and execute custom scripts defined declaratively in structured files (supporting JSON and/or YAML formats, analogous to npm `package.json` "scripts").
- Script configurations MUST define task names, descriptions (optional), and the underlying shell commands to execute.
- Configuration schemas MUST be explicitly validated, returning clear, user-friendly diagnostic messages in Portuguese upon parsing or syntax errors.
- Script execution MUST handle process launching, argument forwarding, stream piping, and exit code propagation safely and reliably.

### III. Test-First & Mandatory Unit Test Coverage
Quality and correctness MUST be enforced through automated tests.
- Every business rule, configuration reader, parser, argument binder, and execution coordinator MUST have comprehensive unit tests.
- Business logic MUST remain decoupled from console I/O (`Spectre.Console`) and direct OS process invocations via appropriate abstractions (interfaces/adapters) to ensure full mockability and testability.
- All unit tests MUST pass cleanly via `dotnet test` prior to any code integration.

### IV. Minimal Dependencies & Explicit Justification
The project maintains a lean dependency tree.
- No third-party NuGet package MAY be added to the project without explicit prior justification demonstrating why native .NET 10 runtime capabilities or existing packages are insufficient.
- Unnecessary abstractions, heavy frameworks, and redundant utilities are strictly prohibited (favor YAGNI and KISS).

### V. Tool Packaging & Distribution (.NET Global Tool - nxtl)
The project MUST be packaged and distributed as a modern .NET CLI tool.
- The project MUST target .NET 10 (`net10.0`), utilizing the latest installed .NET version.
- The production tool project MUST reside within the `src/` directory at the repository root (e.g., `src/NetTaskList/NetTaskList.csproj`).
- The project `.csproj` MUST be configured as a packable tool (`<PackAsTool>true</PackAsTool>`).
- The executable command name MUST be configured as `nxtl` (`<ToolCommandName>nxtl</ToolCommandName>`), enabling global invocation directly from the terminal.

## Architecture & Technology Constraints

- **Platform & Runtime**: .NET 10 (`net10.0`) C#.
- **Project Structure & Directory Layout**:
  - All production application source code and `.csproj` files MUST reside within the `src/` directory at the repository root (e.g., `src/NetTaskList/`).
  - Unit and integration test projects MUST reside within the `tests/` directory at the repository root (e.g., `tests/NetTaskList.Tests/`).
  - No application code, project files, or source files may be placed directly at the repository root level.
- **Terminal UI**: `Spectre.Console` as the sole interactive terminal UI framework.
- **Serialization**: Native `System.Text.Json` or a lightweight, justified YAML parser (e.g., `YamlDotNet` if YAML support is implemented).
- **Architectural Separation**: Strict separation between:
  1. CLI presentation layer (commands, interactive prompts, Spectre rendering).
  2. Domain and business rules (script models, configuration validation, task selection logic).
  3. Execution infrastructure (process execution bridge, filesystem access).

## Development Workflow & Quality Gates

- **Conventional Commits**: All commit messages MUST strictly adhere to the Conventional Commits specification (e.g., `feat:`, `fix:`, `docs:`, `test:`, `refactor:`, `chore:`).
- **Continuous Verification**: Changes must compile with zero warnings and pass 100% of unit tests before committing.
- **Review Compliance**: Every Pull Request or code contribution must be reviewed against these constitutional principles, specifically verifying bilingual consistency (English code / Portuguese UI), repository layout compliance (`src/` and `tests/`), and dependency justification.

## Governance

- This Constitution serves as the single source of truth for architectural standards, development rules, and non-negotiable quality gates for the `nxtl` project.
- Any amendment to these principles requires updating this document with an increment to the semantic version:
  - **MAJOR**: Incompatible changes, removal of core principles, or major architectural shifts.
  - **MINOR**: Addition of new principles or material expansions of technical guidance.
  - **PATCH**: Clarifications, wording adjustments, or non-semantic corrections.
- All feature specifications (`spec.md`), architectural plans (`plan.md`), and task lists (`tasks.md`) produced across Spec Kit workflows MUST maintain strict compliance with this Constitution.

**Version**: 1.1.0 | **Ratified**: 2026-10-30 | **Last Amended**: 2026-10-30
