# Implementation Plan: Mecanismo de Execução de Tarefas e Scripts Configurados

**Branch**: `003-execute-configured-tasks` | **Date**: 2026-11-01 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/003-execute-configured-tasks/spec.md`

## Summary

Implementar o mecanismo central de execução de tarefas configuradas do NetTaskList (`nxtl`), viabilizando a execução de comandos nativos de terminal (`tool`) e scripts C# file-scoped (`csharp`). A solução conecta o processo filho diretamente aos fluxos padrão do terminal (stdin, stdout e stderr) sem buffering artificial ou perda de cores ANSI, aguarda a conclusão da tarefa e propaga o código de saída (exit code) original. Para atender aos princípios constitucionais de testabilidade e desacoplamento, a execução de processos é abstraída através da interface `IProcessExecutor` no namespace de infraestrutura, com implementação em produção via `System.Diagnostics.Process` e classe simulada `MockProcessExecutor` para testes unitários em `tests/NetTaskList.Tests/`. A execução é integrada tanto no fluxo direto via linha de comando (`nxtl --run <nome>`) quanto após a seleção interativa no console.

## Technical Context

**Language/Version**: C# 14 / .NET 10 (`net10.0`), compilado com Native AOT (`<PublishAot>true</PublishAot>`).

**Primary Dependencies**:
- `Spectre.Console` (v0.49+): Interface rica de terminal, exibição de mensagens de status e erros em `pt-BR`.
- `System.Diagnostics.Process` (nativo do .NET 10): Inicialização e sincronização de processos no sistema operacional.
- `System.Text.Json` (nativo do .NET 10 via Source Generators): Leitura das tarefas no `nxtl.json`.

**Storage**: Arquivos declarativos locais (`nxtl.json`).

**Testing**: xUnit (`net10.0`), com suíte abrangente em `tests/NetTaskList.Tests/` utilizando mocks para isolamento de I/O de disco (`MockFileSystem`) e execução de processos (`MockProcessExecutor`).

**Target Platform**: Multiplataforma (Windows, Linux, macOS) como binário nativo autônomo (*self-contained*) e ferramenta global .NET (`nxtl`).

**Project Type**: Executável CLI compilado como .NET Global Tool.

**Performance Goals**:
- Sobrecarga de invocação do processo inferior a 50 milissegundos.
- Latência de repasse de saída (stdout/stderr) zero (através de herança direta de descritores de console).

**Constraints**:
- Zero adição de novas bibliotecas de terceiros (Princípio IV - dependências mínimas).
- 100% compatível com Native AOT (zero reflexão dinâmica).
- Desacoplamento estrito entre lógica de negócio e invocação de processos via `IProcessExecutor` (Princípio III).
- Mensagens e diagnósticos apresentados estritamente em português brasileiro (`pt-BR`).
- Identificadores de código, interfaces, métodos e comentários de desenvolvimento em inglês.
- Estrutura estrita respeitando diretórios `src/` e `tests/`.

**Scale/Scope**: Execução pontual de comandos simples e encadeados, scripts de manutenção, tarefas interativas e pipelines locais de desenvolvimento.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio Constitucional | Status | Avaliação de Conformidade |
|--------------------------|--------|----------------------------|
| **I. Rich Terminal UX (Spectre.Console & Localization)** | **PASS** | Todas as mensagens de erro de execução, diagnóstico de scripts ausentes e confirmação de seleção são formatadas com `Spectre.Console` em português (`pt-BR`). |
| **II. Declarative Script Configuration (JSON / YAML)** | **PASS** | Executa comandos derivados das tarefas declaradas no `nxtl.json`, com resolução automática de scripts C# file-scoped e repasse seguro de streams e exit codes. |
| **III. Test-First & Mandatory Unit Test Coverage** | **PASS** | Criação da abstração `IProcessExecutor` desacoplada do runtime de SO, permitindo testes unitários com `MockProcessExecutor` cobrindo cenários de sucesso, erro de saída e falha de inicialização. |
| **IV. Minimal Dependencies & Explicit Justification** | **PASS** | Nenhuma dependência externa adicionada. Utiliza recursos nativos da BCL do .NET 10 (`System.Diagnostics.Process`). |
| **V. Tool Packaging & Distribution (.NET Global Tool - nxtl)** | **PASS** | Mantém compatibilidade com Native AOT e distribuição como .NET Global Tool. |
| **Estrutura de Diretórios (`src/` e `tests/`)** | **PASS** | Código de produção estritamente em `src/NetTaskList/` e testes unitários em `tests/NetTaskList.Tests/`. |

*Resultado do Gate*: **APROVADO (Zero violações)**.

## Project Structure

### Documentation (this feature)

```text
specs/003-execute-configured-tasks/
├── plan.md              # Este plano de implementação
├── research.md          # Decisões técnicas e justificativas de execução (Fase 0)
├── data-model.md        # Modelos de dados e ciclo de vida de execução (Fase 1)
├── quickstart.md        # Guia executável de validação ponta a ponta (Fase 1)
├── contracts/           # Contratos formais da interface de execução
│   └── process-execution-contract.md # Especificação do IProcessExecutor
└── checklists/
    └── requirements.md  # Checklist de qualidade dos requisitos
```

### Source Code (repository root)

```text
src/NetTaskList/
├── Domain/
│   ├── Models/
│   │   ├── ProcessExecutionResult.cs      # NOVO: Modelo de resultado e exit code da execução
│   │   ├── ScriptTask.cs                 # Existente: Modelo da tarefa com resolução de comando
│   │   ├── TaskConfiguration.cs          # Existente: Agregado de tarefas
│   │   └── TaskType.cs                   # Existente: Enum (Tool, CSharp)
│   └── Validation/
│       ├── ConfigurationValidator.cs     # Existente: Validador de configurações
│       └── ValidationResult.cs           # Existente: Resultado de validação
├── Infrastructure/
│   ├── Abstractions/
│   │   ├── IFileSystem.cs                # Existente: Abstração de sistema de arquivos
│   │   └── IProcessExecutor.cs           # NOVO: Abstração de execução de processos de SO
│   ├── FileSystem.cs                     # Existente: Implementação de IFileSystem
│   ├── ProcessExecutor.cs                # NOVO: Implementação de IProcessExecutor via Process.Start
│   ├── JsonConfigurationReader.cs        # Existente: Leitor de arquivos JSON
│   ├── TemplateConfigurationGenerator.cs # Existente: Gerador de templates nxtl.json
│   └── Serialization/
│       └── TaskJsonContext.cs            # Existente: Contexto AOT Source Generator
├── Presentation/
│   ├── CommandLineParser.cs              # Existente: Parser de flags CLI
│   ├── InteractiveTaskSelector.cs        # Existente: Seleção interativa no terminal
│   └── TaskTableRenderer.cs              # Existente: Renderização tabular rica
└── Program.cs                            # Atualizado: Coordenação de execução e retorno de exit code

tests/NetTaskList.Tests/
├── Domain/
│   └── ConfigurationValidatorTests.cs    # Existente: Testes de regras de validação
├── Infrastructure/
│   ├── CommandResolutionTests.cs         # Existente: Testes de resolução de comando tool/csharp
│   ├── JsonConfigurationReaderTests.cs   # Existente: Testes de parsing
│   ├── ProcessExecutorTests.cs           # NOVO: Testes unitários do mecanismo de execução
│   └── TemplateConfigurationGeneratorTests.cs # Existente: Testes do template gerador
├── Mocks/
│   ├── MockFileSystem.cs                 # Existente: Mock de IFileSystem
│   ├── MockProcessExecutor.cs            # NOVO: Mock de IProcessExecutor para simulação de processos
│   └── MockAnsiConsole.cs                # Existente: Mock de IAnsiConsole
└── Presentation/
    ├── CommandLineParserTests.cs         # Existente: Testes de parser CLI
    └── InteractiveTaskSelectorTests.cs   # Existente: Testes de navegação interativa
```

**Structure Decision**: Padrão de projeto único em `src/NetTaskList/` com camadas de Domínio, Infraestrutura e Apresentação, espelhado no projeto de testes em `tests/NetTaskList.Tests/`. As novas estruturas de execução integram-se naturalmente às abstrações e modelos existentes sem introduzir novos projetos ou pacotes.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| *Nenhuma violação identificada* | N/A | N/A |
