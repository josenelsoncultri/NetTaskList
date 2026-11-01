# Implementation Plan: Correção de Exceção de Markup no InteractiveTaskSelector

**Branch**: `002-fix-selector-markup` | **Date**: 2026-11-01 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/002-fix-selector-markup/spec.md`

## Summary

Corrigir a exceção de tempo de execução `System.InvalidOperationException: Encountered malformed markup tag at position 8` disparada pelo Spectre.Console ao renderizar a opção de saída `"[Sair]"` no menu interativo (`InteractiveTaskSelector`). A correção aplica o escape sistemático de marcação ANSI (`Markup.Escape`) a todas as opções dinâmicas e literais apresentadas no prompt interativo. Para garantir conformidade com a Constituição do projeto (cobertura obrigatória de testes sem dependências externas excessivas), será implementado um mock in-memory de `IAnsiConsole` e `IAnsiConsoleInput` em `tests/NetTaskList.Tests/Mocks/`, viabilizando uma suíte completa de testes xUnit para simular a navegação e seleção interativa com 100% de automação.

## Technical Context

**Language/Version**: C# 14 / .NET 10 (`net10.0`) compilado com Native AOT (`<PublishAot>true</PublishAot>`).

**Primary Dependencies**:
- `Spectre.Console` (v0.49+): Interface rica de terminal e prompt de seleção (`SelectionPrompt<T>`).
- Nenhuma dependência externa adicional adicionada.

**Storage**: N/A (componente puramente de apresentação em memória).

**Testing**: xUnit (`net10.0`), asserções nativas, suíte de testes unitários em `tests/NetTaskList.Tests/Presentation/` desacoplada de I/O de console via mock leve de `IAnsiConsole`.

**Target Platform**: Multiplataforma (Windows, Linux, macOS).

**Project Type**: CLI tool (.NET Global Tool).

**Performance Goals**: Tempo de renderização do prompt inferior a 10ms; zero alocação desnecessária de strings.

**Constraints**:
- Código de produção alocado estritamente em `src/NetTaskList/`.
- Testes e mocks alocados estritamente em `tests/NetTaskList.Tests/`.
- Mensagens para o usuário estritamente em português brasileiro (`pt-BR`).
- Símbolos de código, testes e comentários em inglês.
- Zero dependências de pacotes NuGet adicionais.

**Scale/Scope**: Suporte a qualquer número de tarefas com caracteres especiais, colchetes, parênteses e strings arbitrárias.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio Constitucional | Status | Avaliação de Conformidade |
|:---|:---:|:---|
| **I. Rich Terminal UX (Spectre.Console & Localization)** | **PASS** | Mantém `Spectre.Console` com escape adequado; mensagens e rótulos de controle (`[Sair]`) em `pt-BR`. |
| **II. Declarative Script Configuration (JSON / YAML)** | **PASS** | Não altera regras declarativas de configuração; tarefas continuam sendo representadas fielmente. |
| **III. Test-First & Mandatory Unit Test Coverage** | **PASS** | Adiciona testes unitários dedicados em `tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorTests.cs` cobrindo seleção de tarefas, cancelamento e resiliência a colchetes. |
| **IV. Minimal Dependencies & Explicit Justification** | **PASS** | Zero novos pacotes adicionados. O mock de console para testes é implementado nativamente com base nas interfaces públicas do Spectre.Console. |
| **V. Tool Packaging & Distribution (.NET Global Tool - nxtl)** | **PASS** | Mantém total compatibilidade com Native AOT e distribuição como .NET Global Tool. |
| **Estrutura de Diretórios (`src/` e `tests/`)** | **PASS** | `InteractiveTaskSelector.cs` permanece em `src/NetTaskList/Presentation/`, novo teste em `tests/NetTaskList.Tests/Presentation/`, mock em `tests/NetTaskList.Tests/Mocks/`. |

*Resultado do Gate*: **APROVADO (Zero violações)**.

## Project Structure

### Documentation (this feature)

```text
specs/002-fix-selector-markup/
├── plan.md              # Este plano de implementação (/speckit-plan)
├── research.md          # Decisões de escape de markup e mockabilidade (Fase 0)
├── data-model.md        # Modelo interno de escolha e ciclo de vida da seleção (Fase 1)
├── quickstart.md        # Cenários executáveis de teste e validação (Fase 1)
├── contracts/           # Contrato da API e de apresentação da interface (Fase 1)
│   └── task-selector-contract.md
├── checklists/
│   └── requirements.md  # Checklist de qualidade dos requisitos
└── spec.md              # Especificação de requisitos da feature
```

### Source Code (repository root)

```text
src/
└── NetTaskList/
    └── Presentation/
        └── InteractiveTaskSelector.cs     # Correção do escape de markup na opção de saída e escolhas

tests/
└── NetTaskList.Tests/
    ├── Mocks/
    │   └── MockAnsiConsole.cs             # Mock in-memory de IAnsiConsole e IAnsiConsoleInput para simular teclas
    └── Presentation/
        └── InteractiveTaskSelectorTests.cs # Testes unitários para InteractiveTaskSelector
```

**Structure Decision**: Utilizar o projeto existente em `src/NetTaskList/` e `tests/NetTaskList.Tests/`. Criar apenas `MockAnsiConsole.cs` em `tests/NetTaskList.Tests/Mocks/` e `InteractiveTaskSelectorTests.cs` em `tests/NetTaskList.Tests/Presentation/`.

## Complexity Tracking

> **Zero violações constitucionais**. Nenhuma complexidade adicional introduzida.
