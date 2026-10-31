# Implementation Plan: Processador de Arquivos de Configuração JSON

**Branch**: `001-json-config-processor` | **Date**: 2026-10-30 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-json-config-processor/spec.md`

## Summary

Implementar o módulo central do NetTaskList (`nxtl`) responsável pelo carregamento, validação e exibição interativa de tarefas declaradas em arquivos JSON (`nxtl.json`). A solução utiliza .NET 10 (`net10.0`) com C# 14, compilada como **binário nativo independente (*self-contained* via Native AOT)** sem dependência de runtime do .NET instalado no ambiente de destino. Utiliza `System.Text.Json` com Source Generators (`TaskJsonContext`) para parsing com zero alocação de reflexão (100% trim/AOT-safe), e `Spectre.Console` para fornecer uma interface de terminal rica com listagem tabular colorida e prompt de seleção interativo navegável via setas do teclado (Up/Down). O projeto é configurado com suporte à compilação AOT standalone e empacotamento NuGet como .NET Global Tool, estruturado estritamente nas pastas `src/` e `tests/`.

## Technical Context

**Language/Version**: C# 14 / .NET 10 (`net10.0`) compilado com Native AOT (`<PublishAot>true</PublishAot>`).

**Primary Dependencies**:
- `Spectre.Console` (v0.49+): Interface rica de terminal, tabelas estilizadas, badges e prompt de seleção interativa.
- `System.Text.Json` (nativo .NET 10): Deserialização estática via Source Generators (`JsonSerializerContext`), garantindo zero reflexão e compatibilidade nativa com AOT.

**Storage**: Arquivo declarativo local no formato JSON (`nxtl.json` por padrão, ou arquivo customizado indicado via flag `--config <caminho>`).

**Testing**: xUnit (`net10.0`), FluentAssertions / asserções nativas, suíte completa de testes unitários em `tests/NetTaskList.Tests/` desacoplada de I/O de console.

**Target Platform**: Multiplataforma (Windows, Linux, macOS) como binário nativo autônomo (*self-contained*) e ferramenta global .NET (`nxtl`).

**Project Type**: Executável nativo compilado com Native AOT e empacotável para distribuição no NuGet (`<PublishAot>true</PublishAot>`, `<PackAsTool>true</PackAsTool>`, `<ToolCommandName>nxtl</ToolCommandName>`).

**Performance Goals**: Tempo de inicialização sub-15ms via Native AOT (sem JIT warmup); leitura e renderização da listagem/prompt em menos de 100ms para até 100 tarefas.

**Constraints**:
- Compilação obrigatória como binário independente e self-contained via Native AOT.
- Uso mandatório de Source Generators para serialização JSON para evitar exceções de trimming em tempo de execução.
- Código-fonte principal alocado exclusivamente em `src/NetTaskList/`.
- Testes unitários e de integração alocados exclusivamente em `tests/NetTaskList.Tests/`.
- Mensagens e interface com o usuário estritamente em português brasileiro (`pt-BR`).
- Símbolos de código (classes, métodos, variáveis e testes) em inglês.
- Dependência externa mínima (apenas `Spectre.Console` explicitamente justificado).

**Scale/Scope**: Suporte a arquivos contendo de 1 a dezenas de tarefas com validação instantânea de unicidade (case-insensitive) e integridade de esquema.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio Constitucional | Status | Avaliação de Conformidade |
|--------------------------|--------|----------------------------|
| **I. Rich Terminal UX (Spectre.Console & Localization)** | **PASS** | Usa `Spectre.Console` para tabelas e seleção interativa; todas as mensagens e prompts em `pt-BR`. |
| **II. Declarative Script Configuration (JSON / YAML)** | **PASS** | Processa `nxtl.json`, valida nomes duplicados (case-insensitive), detecta sintaxe inválida e diferencia `tool` de `csharp`. |
| **III. Test-First & Mandatory Unit Test Coverage** | **PASS** | Camadas de domínio e parsing desacopladas de I/O com suíte abrangente de testes xUnit em `tests/NetTaskList.Tests/`. |
| **IV. Minimal Dependencies & Explicit Justification** | **PASS** | Apenas `Spectre.Console` adicionado; `System.Text.Json` é nativo do .NET 10 com Source Generation. |
| **V. Tool Packaging & Distribution (.NET Global Tool - nxtl)** | **PASS** | Compilável como binário independente Native AOT e configurado como .NET Tool (`<PackAsTool>true</PackAsTool>`, `<ToolCommandName>nxtl</ToolCommandName>`). |
| **Estrutura de Diretórios (`src/` e `tests/`)** | **PASS** | Código de produção em `src/NetTaskList/`, testes em `tests/NetTaskList.Tests/`; raiz preservada sem arquivos de código. |

*Resultado do Gate*: **APROVADO (Zero violações)**.

## Project Structure

### Documentation (this feature)

```text
specs/001-json-config-processor/
├── plan.md              # Este plano de implementação
├── research.md          # Decisões técnicas e justificativas (Fase 0)
├── data-model.md        # Modelos de dados e validações (Fase 1)
├── quickstart.md        # Cenários de validação executáveis (Fase 1)
├── contracts/           # Contratos formais da interface
│   ├── cli-contract.md  # Contrato de linha de comando e opções
│   └── config-schema.json # JSON Schema de validação do nxtl.json
└── checklists/
    └── requirements.md  # Checklist de qualidade dos requisitos
```

### Source Code (repository root)

```text
src/
└── NetTaskList/
    ├── NetTaskList.csproj                 # Configuração do projeto .NET 10 (PublishAot=true, PackAsTool=true)
    ├── Program.cs                         # Ponto de entrada do CLI e despacho de fluxo
    ├── Domain/
    │   ├── Models/
    │   │   ├── ScriptTask.cs              # Entidade da tarefa (Name, Command, Type, Description)
    │   │   ├── TaskType.cs                # Enum: Tool vs CSharp
    │   │   └── TaskConfiguration.cs       # Coleção consolidada de tarefas validadas
    │   └── Validation/
    │       ├── IConfigurationValidator.cs # Contrato de validação semântica
    │       ├── ConfigurationValidator.cs  # Regras de unicidade de nome e campos obrigatórios
    │       └── ValidationResult.cs        # Resultado e coleção de erros diagnósticos em pt-BR
    ├── Infrastructure/
    │   ├── Abstractions/
    │   │   └── IFileSystem.cs             # Abstração de acesso a arquivos para mockability
    │   ├── Dtos/
    │   │   ├── ConfigurationDto.cs        # DTO para deserialização flexível do JSON
    │   │   └── ScriptTaskDto.cs           # DTO da tarefa serializada
    │   ├── Serialization/
    │   │   └── TaskJsonContext.cs         # JsonSerializerContext com Source Generation para Native AOT
    │   └── JsonConfigurationReader.cs     # Implementação do leitor System.Text.Json sem reflexão
    └── Presentation/
        ├── CommandLineOptions.cs          # Estrutura de flags (--config, --run, --init, etc.)
        ├── CommandLineParser.cs           # Parser de argumentos CLI minimalista
        ├── InteractiveTaskSelector.cs     # Prompt de seleção interativo (Up/Down) com Spectre.Console
        └── TaskTableRenderer.cs           # Renderização da tabela estilizada com badges de tipo

tests/
└── NetTaskList.Tests/
    ├── NetTaskList.Tests.csproj           # Projeto de testes xUnit net10.0
    ├── Domain/
    │   └── ConfigurationValidatorTests.cs # Testes de unicidade de nome (case-insensitive) e campos
    ├── Infrastructure/
    │   ├── JsonConfigurationReaderTests.cs# Testes de parsing JSON, formatos flexíveis e sintaxe
    │   └── CommandResolutionTests.cs      # Testes de inferência para scripts C# file-scoped
    └── Presentation/
        └── CommandLineParserTests.cs      # Testes de parsing das flags CLI (--config, --run, --init)
```

**Structure Decision**:
A solução é dividida em dois projetos bem delineados: `src/NetTaskList/` (ferramenta principal) e `tests/NetTaskList.Tests/` (testes automatizados). A arquitetura interna adota separação estrita de responsabilidades:
1. **Domain**: Contém as entidades e regras de validação puras, totalmente isoladas de infraestrutura e console.
2. **Infrastructure**: Responsável pela leitura de arquivos e deserialização JSON com abstrações testáveis via mocks.
3. **Presentation**: Responsável por receber argumentos de linha de comando e interagir com o usuário via `Spectre.Console`.

## Complexity Tracking

*Nenhuma violação identificada. A arquitetura segue rigorosamente as diretrizes da Constituição e atende a todos os requisitos funcionais sem sobrecarga desnecessária.*
