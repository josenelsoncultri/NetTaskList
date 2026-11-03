# Implementation Plan: Vertical Carousel Terminal Selection

**Branch**: `008-vertical-carousel-selection` | **Date**: 2026-11-03 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/008-vertical-carousel-selection/spec.md`

## Summary

Implement circular (carousel) vertical navigation in the interactive task selection menu (`InteractiveTaskSelector`). When the user is at the last option and presses the Down arrow, the cursor wraps around to the first option; when at the first option and pressing the Up arrow, the cursor wraps around to the last option (`[Sair]`). This capability is natively supported in `Spectre.Console` via `SelectionPrompt<T>.WrapAround = true` (or `.WrapAround(true)`). To comply with Constitution Principle III (Test-First & Mandatory Unit Test Coverage), new automated unit tests will be added to `tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorTests.cs` using the existing `MockAnsiConsole` to assert forward, backward, and multi-cycle wrap-around navigation with 100% test automation.

## Technical Context

**Language/Version**: C# 14 / .NET 10 (`net10.0`).

**Primary Dependencies**:
- `Spectre.Console` (v0.57.2+): Rich terminal UI framework and selection prompt (`SelectionPrompt<T>`).
- Zero new external dependencies.

**Storage**: N/A (pure in-memory presentation layer).

**Testing**: xUnit (`net10.0`), console input mocking via `MockAnsiConsole` in `tests/NetTaskList.Tests/Mocks/MockAnsiConsole.cs`, unit test assertions in `tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorTests.cs`.

**Target Platform**: Cross-platform CLI (.NET Global Tool on Linux, macOS, Windows).

**Project Type**: CLI Tool (`nxtl`).

**Performance Goals**: Prompt creation and navigation transitions complete in sub-millisecond execution (< 1ms); zero additional string allocations.

**Constraints**:
- All production code strictly in `src/NetTaskList/`.
- All unit test code strictly in `tests/NetTaskList.Tests/`.
- User-facing terminal text in Brazilian Portuguese (`pt-BR`).
- All code identifiers, comments, and test names in English.
- Native AOT compatibility preserved (`<PublishAot>true</PublishAot>`).

**Scale/Scope**: Arbitrary task counts, single-page and paginated task selection lists.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio Constitucional | Status | Avaliação de Conformidade |
|:---|:---:|:---|
| **I. Rich Terminal UX (Spectre.Console & Localization)** | **PASS** | Utiliza a capacidade nativa `WrapAround` do `Spectre.Console`; mantém todos os textos, títulos e mensagens em `pt-BR`. |
| **II. Declarative Script Configuration (JSON / YAML)** | **PASS** | Não altera a estrutura nem o parsing de arquivos de configuração (`nxtl.json`, `package.json`). |
| **III. Test-First & Mandatory Unit Test Coverage** | **PASS** | Adiciona testes unitários dedicados em `InteractiveTaskSelectorTests.cs` simulando teclas com `MockAnsiConsole` para validar wrap-around bidirecional. |
| **IV. Minimal Dependencies & Explicit Justification** | **PASS** | Zero novas dependências de pacotes NuGet adicionadas; aproveita funcionalidade nativa já presente no `Spectre.Console`. |
| **V. Tool Packaging & Distribution (.NET Global Tool - nxtl)** | **PASS** | Mantém compatibilidade total com empacotamento global tool e compilação Native AOT. |
| **VI. Versioning Policy & Spec Implementation Lifecycle** | **PASS** | Modificação de usabilidade e comportamento do menu interativo elegível como ajuste/melhoria de UX (Patch ou Minor conforme política de versão). |
| **Estrutura de Diretórios (`src/` e `tests/`)** | **PASS** | Produção em `src/NetTaskList/Presentation/InteractiveTaskSelector.cs`, testes em `tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorTests.cs`. |

*Resultado do Gate*: **APROVADO (Zero violações)**.

## Project Structure

### Documentation (this feature)

```text
specs/008-vertical-carousel-selection/
├── spec.md              # Requisitos funcionais e cenários de aceite
├── plan.md              # Este plano de implementação (/speckit-plan)
├── research.md          # Decisões técnicas e pesquisa de API do Spectre.Console (Fase 0)
├── data-model.md        # Modelo conceitual de cursores e transições de estado (Fase 1)
├── quickstart.md        # Guia de validação automatizada e manual (Fase 1)
├── contracts/           # Contratos de interface e comportamento (Fase 1)
│   └── task-selector-navigation-contract.md
└── checklists/
    └── requirements.md  # Checklist de qualidade da especificação
```

### Source Code (repository root)

```text
src/
└── NetTaskList/
    └── Presentation/
        └── InteractiveTaskSelector.cs       # Habilitação de WrapAround no SelectionPrompt

tests/
└── NetTaskList.Tests/
    ├── Mocks/
    │   └── MockAnsiConsole.cs               # Mock de IAnsiConsole / IAnsiConsoleInput (existente)
    └── Presentation/
        └── InteractiveTaskSelectorTests.cs   # Novos testes de wrap-around (Up do início, Down do fim, ciclo contínuo)
```

**Structure Decision**: Apenas modificações cirúrgicas em arquivos existentes:
1. `src/NetTaskList/Presentation/InteractiveTaskSelector.cs`: Adicionar `.WrapAround(true)` na construção do prompt.
2. `tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorTests.cs`: Adicionar casos de teste cobrindo a navegação carrossel vertical.

## Complexity Tracking

> **Zero violações constitucionais**. Nenhuma complexidade adicional introduzida.
