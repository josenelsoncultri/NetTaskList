# Implementation Plan: Adição do Comando de Atualização da Ferramenta ao nxtl.json Padrão

**Branch**: `006-add-update-tool-task` | **Date**: 2026-11-01 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/006-add-update-tool-task/spec.md`

## Summary

Acrescentar a tarefa de atualização global da ferramenta (`update-nettasklist`) com comando `dotnet tool update -g nettasklist`, tipo `tool` e descrição `"Atualiza a ferramenta nxtl de modo global"` ao template de configuração padrão gerado em `TemplateConfigurationGenerator.GenerateDefaultJson()`. Além disso, atualizar o arquivo de configuração `nxtl.json` da raiz do repositório, os exemplos da documentação (`README.md`) e os testes unitários em `TemplateConfigurationGeneratorTests.cs`.

## Technical Context

**Language/Version**: C# 14 / .NET 10 (`net10.0`), Native AOT (`<PublishAot>true</PublishAot>`).

**Primary Dependencies**:
- `System.Text.Json` (Source Generator AOT).
- `Spectre.Console` (v0.57.2).

**Storage**: Arquivo `nxtl.json`.

**Testing**: xUnit (`net10.0`), suíte `tests/NetTaskList.Tests/`.

**Target Platform**: .NET Global Tool multiplataforma (`nxtl`).

**Project Type**: CLI.

**Performance Goals**: Tempo de execução e deserialização sub-5ms.

**Constraints**:
- Conformidade total com Native AOT.
- Zero dependências de terceiros adicionais (Princípio IV).
- Textos da interface e descrição de tarefas em português brasileiro (`pt-BR`).
- Símbolos de código e testes em inglês.
- Estrutura estrita em `src/` e `tests/`.

**Scale/Scope**: Inclusão pontual de tarefa padrão no gerador de template, documentação e testes.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio Constitucional | Status | Avaliação de Conformidade |
|---|---|---|
| **I. Rich Terminal UX (Spectre.Console & Localization)** | **PASS** | Descrição da tarefa em `pt-BR` ("Atualiza a ferramenta nxtl de modo global"). Código em inglês. |
| **II. Declarative Script Configuration (JSON / YAML)** | **PASS** | Tarefa declarada seguindo o contrato padrão de scripts (`name`, `command`, `type`, `description`). |
| **III. Test-First & Mandatory Unit Test Coverage** | **PASS** | Testes automatizados em `TemplateConfigurationGeneratorTests.cs` cobrindo a presença e integridade da nova tarefa. |
| **IV. Minimal Dependencies & Explicit Justification** | **PASS** | Nenhuma dependência externa adicionada. |
| **V. Tool Packaging & Distribution (.NET Global Tool - nxtl)** | **PASS** | Mantém integridade de empacotamento da ferramenta. |
| **VI. Versioning Policy & Spec Implementation Lifecycle** | **PASS** | Modificação mínima no template e configuração. Bump estimado como PATCH ou avaliado no encerramento. |
| **Estrutura de Diretórios (`src/` e `tests/`)** | **PASS** | Modificações restritas a `src/NetTaskList/`, `tests/NetTaskList.Tests/` e arquivos de documentação/configuração raiz. |

*Resultado do Gate*: **APROVADO (Zero violações)**.

## Project Structure

### Documentation (this feature)

```text
specs/006-add-update-tool-task/
├── plan.md              # Este plano de implementação
├── research.md          # Pesquisa técnica e definição do identificador (Fase 0)
├── data-model.md        # Modelagem do template com a nova tarefa (Fase 1)
├── quickstart.md        # Guia de validação prática (Fase 1)
├── contracts/           # Contratos formais da tarefa e template (Fase 1)
│   └── default-template-contract.md
└── checklists/
    └── requirements.md  # Checklist de qualidade dos requisitos
```

### Source Code (repository root)

```text
nxtl.json                                       # ATUALIZADO: Inclusão da tarefa update-nettasklist
README.md                                       # ATUALIZADO: Inclusão da tarefa de atualização no exemplo

src/NetTaskList/
└── Infrastructure/
    └── TemplateConfigurationGenerator.cs      # ATUALIZADO: Inclusão da tarefa update-nettasklist no template padrão

tests/NetTaskList.Tests/
└── Infrastructure/
    └── TemplateConfigurationGeneratorTests.cs # ATUALIZADO: Asserção de 4 tarefas e validação da tarefa de update
```

**Structure Decision**: Utilização dos projetos existentes em `src/NetTaskList` e `tests/NetTaskList.Tests` sem novas estruturas.

## Complexity Tracking

> Nenhuma violação constitucional identificada. Tabela de justificativas dispensada.
