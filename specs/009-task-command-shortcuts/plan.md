# Implementation Plan: Atalhos de Comandos de Tarefas e Prevenção de Recursão (Task Command Shortcuts)

**Branch**: `009-task-command-shortcuts` | **Date**: 2026-11-10 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `specs/009-task-command-shortcuts/spec.md`

## Summary

O objetivo desta funcionalidade é permitir que as tarefas do tipo `tool` no arquivo `nxtl.json` definam comandos que sejam atalhos diretos para outras tarefas configuradas, utilizando a sintaxe de objeto `{ "task": "<nome da tarefa>" }`. Esses atalhos podem ser declarados tanto como comando único quanto dentro de arrays de comandos sequenciais. Ao executar uma tarefa que referencia atalhos, o `nxtl` deve validar previamente a árvore de dependências para detectar e impedir qualquer tipo de recursão ou ciclo infinito (auto-referência, ciclos mútuos diretos ou loops profundos em árvore), interrompendo a execução imediatamente com uma mensagem em português e informando a cadeia de tarefas que geraria o loop. Quando válida, a execução exibe uma linha de status indicando o acionamento de cada atalho (`Executando atalho: <nome>`) e executa seus comandos sequencialmente, abortando passos posteriores em caso de falha.

A abordagem técnica expande o conversor polimórfico `CommandPropertyJsonConverter` e introduz o DTO `TaskCommandDto` e a entidade de domínio `TaskCommandStep`, mantendo compatibilidade Native AOT trim-safe via `System.Text.Json` e zero dependências externas adicionais.

---

## Technical Context

**Language/Version**: C# / .NET 10 (`net10.0`)  
**Primary Dependencies**: `Spectre.Console`, `System.Text.Json` (código-fonte gerado via `JsonSerializerContext` / Native AOT)  
**Storage**: Arquivo de configuração local `nxtl.json`  
**Testing**: `xUnit`, `FluentAssertions` executados via `dotnet test`  
**Target Platform**: Multiplataforma (Windows, Linux, macOS)  
**Project Type**: CLI tool (.NET Global Tool `nxtl`)  
**Performance Goals**: Validação da árvore de atalhos e detecção de ciclos em menos de 10ms para grafos típicos (<50 tarefas); inicialização rápida sem sobrecarga perceptível  
**Constraints**: Zero pacotes NuGet adicionais; Native AOT trim-safe; mensagens de terminal 100% em português brasileiro (`pt-BR`); código e testes estritamente em inglês  
**Scale/Scope**: Suporte a atalhos únicos, compostos em array e árvores de chamada arbitrárias com validação de grafos acíclicos dirigidos (DAGs)  

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Principle I (Rich Terminal UX & pt-BR Localization)**: Todas as mensagens do terminal (`Executando atalho:`, `Referência circular detectada entre tarefas:`, `A execução causaria um loop infinito no nxtl`) formuladas estritamente em `pt-BR`. O código, símbolos, classes e testes são nomeados em inglês.
- [x] **Principle II (Declarative Script Configuration)**: Suporte declarativo à sintaxe `{ "task": "<nome>" }` no `nxtl.json`, validação prévia de integridade referencial e atualização do esquema `nxtl-v1.schema.json`.
- [x] **Principle III (Test-First & Mandatory Unit Test Coverage)**: Desacoplamento via abstrações (`IProcessExecutor`, `IFileSystem`, `IAnsiConsole`), com testes unitários cobrindo atalhos únicos, atalhos em array, detecção de loops em auto-referência, ciclos mútuos, recursão em árvore, grafos diamante válidos, tarefa inexistente e rejeição de scripts C#.
- [x] **Principle IV (Minimal Dependencies & Explicit Justification)**: Nenhuma dependência externa adicionada. A serialização faz uso exclusivo do `System.Text.Json` do runtime .NET 10.
- [x] **Principle V (Tool Packaging & Distribution)**: Empacotamento mantido como .NET global tool (`nxtl`) no projeto `src/NetTaskList/NetTaskList.csproj` direcionado ao .NET 10.
- [x] **Principle VI (Versioning Policy & Spec Implementation Lifecycle)**: Como a especificação adiciona uma nova capacidade de configuração e sintaxe de comandos, a versão do projeto sofrerá incremento Minor (`X.(Y+1).0`) ao término da implementação.

---

## Project Structure

### Documentation (this feature)

```text
specs/009-task-command-shortcuts/
├── plan.md              # Este plano de implementação (/speckit-plan)
├── research.md          # Pesquisa técnica e decisões de arquitetura (/speckit-plan)
├── data-model.md        # Modelos de domínio, DTOs e regras de validação (/speckit-plan)
├── quickstart.md        # Cenários executáveis para validação prática (/speckit-plan)
├── contracts/
│   ├── nxtl-configuration.contract.md  # Contrato do schema JSON para atalhos
│   └── task-execution.contract.md      # Contrato de formatação, loops e parada imediata
└── checklists/
    └── requirements.md  # Validação de qualidade da especificação
```

### Source Code (repository layout)

```text
src/NetTaskList/
├── Domain/
│   ├── Models/
│   │   ├── TaskCommandStep.cs        # Representa um passo: comando shell ou atalho de tarefa
│   │   ├── ScriptTask.cs             # Suporte a CommandSteps (IReadOnlyList<TaskCommandStep>)
│   │   ├── TaskConfiguration.cs
│   │   └── TaskType.cs
│   └── Validation/
│       ├── ConfigurationValidator.cs # Valida atalhos vazios e proíbe atalhos em tarefas csharp
│       └── ValidationResult.cs
├── Infrastructure/
│   ├── Dtos/
│   │   ├── TaskCommandDto.cs         # DTO intermediário para comando bruto ou targetTask
│   │   └── ScriptTaskDto.cs          # Propriedade Commands com CommandPropertyJsonConverter
│   ├── Serialization/
│   │   ├── CommandPropertyJsonConverter.cs # Conversor polimórfico (string, object, array misto)
│   │   └── TaskJsonContext.cs        # Registro de TaskCommandDto no JsonSerializerContext
│   ├── JsonConfigurationReader.cs   # Mapeia TaskCommandDto para TaskCommandStep
│   └── schemas/
│       └── nxtl-v1.schema.json       # Atualizado com taskShortcut e oneOf
├── Presentation/
│   ├── InteractiveTaskSelector.cs   # Exibe "atalho: <nome>" quando não houver descrição
│   └── TaskExecutionCoordinator.cs  # Validação prévia de ciclos (DFS) + execução com status de atalho
└── Program.cs

tests/NetTaskList.Tests/
├── Domain/
│   ├── TaskCommandStepTests.cs       # Testes unitários da entidade de passos de comando
│   ├── ScriptTaskTests.cs            # Testes de inicialização com atalhos e comandos
│   └── ConfigurationValidatorTests.cs # Testes de validação de atalhos inválidos e restrição a C#
├── Infrastructure/
│   ├── CommandPropertyJsonConverterTests.cs # Testes de deserialização de string, object e array misto
│   └── JsonConfigurationReaderTests.cs      # Testes de leitura de nxtl.json com atalhos
└── Presentation/
    ├── TaskExecutionCoordinatorTests.cs     # Testes de execução de atalhos, mensagens de status e parada
    ├── RecursionDetectionTests.cs           # Testes específicos de auto-loop, ciclo mútuo e árvore
    └── DiamondDependencyTests.cs            # Testes de grafo diamante válido sem falso positivo
```

---

## Complexity Tracking

> Nenhuma violação constitucional detectada.
