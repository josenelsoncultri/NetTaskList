<!--
Sync Impact Report:
- Version change: 2.0.0 → 2.1.0
- List of modified principles: None
- Added principles / sections:
  - Added Principle VI. Versioning Policy & Spec Implementation Lifecycle (under Core Principles)
  - Added Spec Completion & Version Management gate (under Development Workflow & Quality Gates)
- Removed sections: None
- Modified sections:
  - Core Principles: Added VI. Versioning Policy & Spec Implementation Lifecycle
  - Development Workflow & Quality Gates: Added version estimation and update rule for spec completion
  - Review & Quality Compliance: Added verification of version management in src/NetTaskList/NetTaskList.csproj
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

### VI. Versioning Policy & Spec Implementation Lifecycle
Project versioning MUST follow semantic versioning rules directly tied to the scope and nature of code modifications, governed in the primary component project file `src/NetTaskList/NetTaskList.csproj` (`<Version>` element).
- **Build / Patch Version (`X.Y.Z` → `X.Y.(Z+1)`)**: Small code modifications, targeted bug fixes, and minimal adjustments that do not alter the fundamental essence or public behavior of the software MUST increment only the build/patch version.
- **Minor / Feature Version (`X.Y.Z` → `X.(Y+1).0`)**: Changes that add new features, commands, configuration capabilities, or enhancements MUST increment the minor version number.
- **Major Version (`X.Y.Z` → `(X+1).0.0`)**: Major architectural revisions, breaking changes, or substantial new functional subsystems MUST increment the major version.
- **Spec Implementation Gate & User Confirmation**: Upon completing the implementation of any specification, the code changes MUST be analyzed to estimate the appropriate version bump, and the `<Version>` element in `src/NetTaskList/NetTaskList.csproj` updated. If the correct version cannot be estimated with confidence, the implementer MUST NOT modify the file automatically; they MUST prompt the user for the correct target version, await explicit confirmation, and only then apply the modification.

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

- **Workflow & Branch Flexibility**: Branch isolation is not mandatory. Developers MAY commit and push directly to `main` or use branches according to project needs without requiring dedicated feature branches or mandatory Pull Requests.
- **Automated Deployment via GitHub Actions**: Package building, validation, and deployment (publishing to NuGet) are governed and executed via the official GitHub Actions workflow (`.github/workflows/publish.yml`) using NuGet Trusted Publishing (OIDC). Publishing is triggered by published GitHub Releases or manual execution via `workflow_dispatch`.
- **Conventional Commits**: All commit messages MUST strictly adhere to the Conventional Commits specification (e.g., `feat:`, `fix:`, `docs:`, `test:`, `refactor:`, `chore:`).
- **Continuous Verification**: Changes must compile with zero warnings and pass 100% of unit tests (`dotnet test`) before deployment.
- **Spec Completion & Version Management**: At the conclusion of any feature or spec implementation, the codebase MUST be evaluated against the versioning policy. The `<Version>` element in `src/NetTaskList/NetTaskList.csproj` MUST be updated according to the change magnitude. When confidence is lacking, the user MUST be prompted for explicit version guidance before any modification is applied.
- **Review & Quality Compliance**: All contributions must adhere to these constitutional principles, specifically verifying bilingual consistency (English code / Portuguese UI), repository layout compliance (`src/` and `tests/`), dependency justification, version management in `src/NetTaskList/NetTaskList.csproj`, and automated workflow deployment.

## Governance

- This Constitution serves as the single source of truth for architectural standards, development rules, and non-negotiable quality gates for the `nxtl` project.
- Any amendment to these principles requires updating this document with an increment to the semantic version:
  - **MAJOR**: Incompatible changes, removal of core principles, or major architectural shifts.
  - **MINOR**: Addition of new principles or material expansions of technical guidance.
  - **PATCH**: Clarifications, wording adjustments, or non-semantic corrections.
- All feature specifications (`spec.md`), architectural plans (`plan.md`), and task lists (`tasks.md`) produced across Spec Kit workflows MUST maintain strict compliance with this Constitution.

**Version**: 2.1.0 | **Ratified**: 2026-10-30 | **Last Amended**: 2026-11-01