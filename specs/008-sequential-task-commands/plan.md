# Implementation Plan: Suporte a Múltiplos Comandos Sequenciais em Tarefas (Sequential Task Commands)

**Branch**: `008-sequential-task-commands` | **Date**: 2026-11-04 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/008-sequential-task-commands/spec.md`

## Summary

O objetivo desta funcionalidade é permitir que as tarefas declaradas no arquivo `nxtl.json` configurem um array ordenado de comandos na propriedade `command`. Ao acionar uma tarefa com múltiplos comandos, o `nxtl` executará cada instrução de forma estritamente sequencial no terminal, exibindo o indicador de progresso `Comando [X/N]: <linha>` antes de cada etapa. Caso qualquer um dos comandos falhe (código de saída diferente de zero ou erro de lançamento de processo), a execução da sequência é interrompida imediatamente, os comandos posteriores são ignorados e o código de erro é propagado fielmente para o chamador do CLI.

A abordagem técnica utiliza um conversor customizado do `System.Text.Json` compatível com Native AOT (`CommandPropertyJsonConverter`) para permitir que a propriedade `command` aceite tanto `string` única quanto `List<string>`, mantendo 100% de retrocompatibilidade com configurações existentes e zero dependências externas.

---

## Technical Context

**Language/Version**: C# / .NET 10 (`net10.0`)  
**Primary Dependencies**: `Spectre.Console`, `System.Text.Json` (código-fonte gerado via `JsonSerializerContext` / Native AOT)  
**Storage**: Arquivos do sistema local (`nxtl.json`, `package.json`)  
**Testing**: `xUnit`, `FluentAssertions` executados via `dotnet test`  
**Target Platform**: Multiplataforma (Windows, Linux, macOS)  
**Project Type**: CLI tool (.NET Global Tool `nxtl`)  
**Performance Goals**: Tempo de inicialização e transição entre comandos inferior a 50ms (excluindo tempo intrínseco do processo filho)  
**Constraints**: Zero pacotes NuGet adicionais; trim-safe (Native AOT); mensagens de interface 100% em português brasileiro (`pt-BR`); código e testes em inglês  
**Scale/Scope**: Execução de sequências arbitrárias de comandos em tarefas configuradas localmente  

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Principle I (Rich Terminal UX & pt-BR Localization)**: Todas as mensagens interativas (`Comando [X/N]:`, `Execução interrompida:`, `<sem descrição>`) formuladas em `pt-BR`. O código e os testes são nomeados estritamente em inglês.
- [x] **Principle II (Declarative Script Configuration)**: Validação explícita de comandos no `ConfigurationValidator` e atualização de `nxtl-v1.schema.json` para suportar `string | string[]`.
- [x] **Principle III (Test-First & Mandatory Unit Test Coverage)**: Desacoplamento via abstrações (`IProcessExecutor`, `IFileSystem`, `IAnsiConsole`), garantindo cobertura completa de testes unitários para execução sequencial, parada em erro, deserialização polimórfica e exibição no menu interativo.
- [x] **Principle IV (Minimal Dependencies & Explicit Justification)**: Nenhuma dependência externa adicionada. A deserialização faz uso exclusivo dos conversores nativos do .NET 10 com `JsonSerializerContext`.
- [x] **Principle V (Tool Packaging & Distribution)**: Mantém o empacotamento padrão como global tool em `src/NetTaskList/NetTaskList.csproj` direcionado ao .NET 10.
- [x] **Principle VI (Versioning Policy & Spec Implementation Lifecycle)**: Por adicionar uma nova capacidade de configuração e formato de execução de comandos, a versão sofrerá incremento Minor (`X.(Y+1).0`) ao término da implementação.

---

## Project Structure

### Documentation (this feature)

```text
specs/008-sequential-task-commands/
├── plan.md              # Este plano de implementação (/speckit-plan)
├── research.md          # Pesquisa técnica e decisões de arquitetura (/speckit-plan)
├── data-model.md        # Modelos de domínio, DTOs e regras de validação (/speckit-plan)
├── quickstart.md        # Cenários executáveis para validação prática (/speckit-plan)
├── contracts/
│   ├── nxtl-configuration.contract.md  # Contrato do schema JSON para comandos
│   └── task-execution.contract.md      # Contrato de formatação, parada e códigos de saída
└── checklists/
    └── requirements.md  # Validação de qualidade da especificação
```

### Source Code (repository layout)

```text
src/NetTaskList/
├── Domain/
│   ├── Models/
│   │   ├── ScriptTask.cs             # Suporte a Commands (IReadOnlyList<string>) e ResolveExecutionCommands()
│   │   ├── TaskConfiguration.cs
│   │   └── TaskType.cs
│   └── Validation/
│       ├── ConfigurationValidator.cs # Valida listas vazias ou itens em branco em Commands
│       └── ValidationResult.cs
├── Infrastructure/
│   ├── Dtos/
│   │   └── ScriptTaskDto.cs          # Propriedade Command com CommandPropertyJsonConverter
│   ├── Serialization/
│   │   ├── CommandPropertyJsonConverter.cs # Conversor polimórfico de string ou string[]
│   │   └── TaskJsonContext.cs        # Registro de conversores no JsonSerializerContext
│   ├── JsonConfigurationReader.cs   # Repassa lista de comandos para ScriptTask
│   └── schemas/
│       └── nxtl-v1.schema.json       # Atualizado com oneOf: string | array de strings
├── Presentation/
│   ├── InteractiveTaskSelector.cs   # Renderiza <sem descrição> para tarefas multi-comando sem descrição
│   └── TaskExecutionCoordinator.cs  # Execução sequencial com parada imediata em erro e indicador [X/N]
└── Program.cs

tests/NetTaskList.Tests/
├── Domain/
│   ├── ScriptTaskTests.cs           # Testes de inicialização com string única ou array e resolução de comandos
│   └── ConfigurationValidatorTests.cs # Testes de validação para array vazio ou itens em branco
├── Infrastructure/
│   ├── JsonConfigurationReaderTests.cs # Testes de deserialização de nxtl.json com comandos em string ou array
│   └── CommandPropertyJsonConverterTests.cs # Testes unitários do conversor polimórfico
└── Presentation/
    ├── InteractiveTaskSelectorTests.cs # Testes do rótulo <sem descrição> para tarefas com múltiplos comandos
    └── TaskExecutionCoordinatorTests.cs # Testes de execução sequencial ordenada, parada em erro e exit codes
```

---

## Complexity Tracking

> Nenhuma violação constitucional detectada.
