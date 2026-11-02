# Implementation Plan: Melhoria do Output de Execução e Navegação de Tarefas

**Branch**: `004-improve-execution-output` | **Date**: 2026-11-01 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/004-improve-execution-output/spec.md`

## Summary

Aprimorar a apresentação visual e a experiência interativa do comando `nxtl` ao ser executado no terminal. A implementação elimina a tabela estática redundante de tarefas disponíveis (`"Tarefas Disponíveis (nxtl)"`), introduz um cabeçalho no topo da listagem com `"nxtl versão <código da versão atual>"` na cor verde obtido dinamicamente via metadados de assembly (`AppVersion`), ajusta o tamanho da janela de visualização do seletor interativo (`SelectionPrompt`) para exibir no máximo 8 tarefas visíveis simultaneamente com suporte a paginação/rolagem, e estiliza o título orientativo `"Selecionar uma tarefa para executar"` na cor ciano.

## Technical Context

**Language/Version**: C# 14 / .NET 10 (`net10.0`), compilado com suporte a Native AOT (`<PublishAot>true</PublishAot>`).

**Primary Dependencies**:
- `Spectre.Console` (v0.57.2): Renderização de interface de terminal, formatação com markups de cores ANSI (`[green]`, `[cyan]`, `[grey]`) e prompt de seleção interativa com paginação (`PageSize(8)`).
- `System.Reflection` (nativo do .NET 10): Resolução dinâmica de metadados de versão sem impacto em AOT.

**Storage**: Arquivos declarativos locais (`nxtl.json`).

**Testing**: xUnit (`net10.0`) com `tests/NetTaskList.Tests/` utilizando `MockAnsiConsole` para validar a saída textual e sequências de formatação do console.

**Target Platform**: Multiplataforma (Windows, Linux, macOS) como .NET Global Tool (`nxtl`) e binário nativo autônomo.

**Project Type**: CLI interativo para desenvolvedores.

**Performance Goals**:
- Sobrecarga de renderização imperceptível (< 10 ms).
- Viewport compacto e sem quebras visuais mesmo em terminais com altura reduzida.

**Constraints**:
- Zero adição de bibliotecas de terceiros (Princípio IV).
- Conformidade total com Native AOT (zero reflexão dinâmica de tipos).
- Textos voltados ao usuário estritamente em português brasileiro (`pt-BR`) (Princípio I).
- Código, classes e métodos em inglês (Princípio I).
- Preservação da estrutura de diretórios em `src/` e `tests/`.

**Scale/Scope**: Ajustes de apresentação visual no fluxo padrão de execução do `nxtl` e refinamento dos parâmetros do seletor interativo.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio Constitucional | Status | Avaliação de Conformidade |
|---|---|---|
| **I. Rich Terminal UX (Spectre.Console & Localization)** | **PASS** | Todas as alterações utilizam recursos nativos do `Spectre.Console` (`SelectionPrompt`, markups de cor verde/ciano/cinza) mantendo todas as mensagens em português do Brasil (`pt-BR`). |
| **II. Declarative Script Configuration (JSON / YAML)** | **PASS** | Não altera os esquemas ou a lógica de carregamento do `nxtl.json`. Mantém compatibilidade total com tarefas declaradas. |
| **III. Test-First & Mandatory Unit Test Coverage** | **PASS** | Validação automatizada através do `MockAnsiConsole`, cobrindo ausência da tabela estática, cabeçalho de versão, limite de 8 itens e título ciano. |
| **IV. Minimal Dependencies & Explicit Justification** | **PASS** | Nenhuma dependência externa adicionada; utiliza apenas os recursos já presentes de `Spectre.Console` e do runtime .NET 10. |
| **V. Tool Packaging & Distribution (.NET Global Tool - nxtl)** | **PASS** | Mantém a compilação como ferramenta global e compatibilidade com Native AOT. |
| **Estrutura de Diretórios (`src/` e `tests/`)** | **PASS** | Alterações confinadas exclusivamente aos diretórios `src/NetTaskList/` e `tests/NetTaskList.Tests/`. |

*Resultado do Gate*: **APROVADO (Zero violações)**.

## Project Structure

### Documentation (this feature)

```text
specs/004-improve-execution-output/
├── plan.md              # Este plano de implementação
├── research.md          # Pesquisa técnica e decisões de UX/AOT (Fase 0)
├── data-model.md        # Modelos de apresentação e fluxo de saída (Fase 1)
├── quickstart.md        # Guia executável de validação ponta a ponta (Fase 1)
├── contracts/           # Contratos formais da interface de terminal (Fase 1)
│   └── terminal-presentation-contract.md
└── checklists/
    └── requirements.md  # Checklist de qualidade dos requisitos
```

### Source Code (repository root)

```text
src/NetTaskList/
├── Presentation/
│   ├── AppVersion.cs                  # NOVO: Helper centralizado para resolução dinâmica de versão
│   ├── InteractiveTaskSelector.cs     # ATUALIZADO: PageSize(8) e título em ciano
│   ├── TaskTableRenderer.cs           # REMOVIDO: Tabela estática descontinuada
│   └── CommandLineParser.cs           # Inalterado
└── Program.cs                         # ATUALIZADO: Remoção da tabela estática e adição do cabeçalho de versão

tests/NetTaskList.Tests/
└── Presentation/
    ├── AppVersionTests.cs             # NOVO: Testes unitários para resolução de versão
    ├── InteractiveTaskSelectorTests.cs # ATUALIZADO: Validação do viewport de 8 itens e markup do título
    └── ProgramExecutionTests.cs       # ATUALIZADO: Validação de ausência da tabela e presença do cabeçalho
```

**Structure Decision**: Utilização direta do projeto único em `src/NetTaskList` e projeto de testes em `tests/NetTaskList.Tests`, conforme estabelecido na constituição do projeto.

## Complexity Tracking

> Nenhuma violação constitucional identificada. Tabela de justificativas dispensada.
