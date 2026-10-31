# Tasks: Processador de Arquivos de Configuração JSON (nxtl)

**Input**: Design documents from `/specs/001-json-config-processor/`  
**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [data-model.md](data-model.md), [contracts/cli-contract.md](contracts/cli-contract.md), [research.md](research.md), [quickstart.md](quickstart.md), [.specify/memory/constitution.md](../../.specify/memory/constitution.md)  
**Tests**: Testes automatizados unitários são obrigatórios conforme o Princípio III da Constituição do projeto (`NetTaskList`).  
**Organization**: As tarefas estão organizadas por jornadas do usuário (User Stories) para permitir implementação e validação independentes de cada incremento.  

## Format: `- [ ] [TaskID] [P?] [Story?] Description with file path`

- **[P]**: Tarefa paralelizada (arquivos distintos, sem dependência de tarefas incompletas).
- **[Story]**: Identificador da história de usuário ([US1], [US2], [US3], [US4]).
- Todos os caminhos de arquivos são explícitos e baseados na raiz do repositório (`src/` e `tests/`).

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Inicialização da solução, configuração dos projetos .NET 10, dependências e esquemas.

- [ ] T001 Initialize solution and create project folders for `src/NetTaskList/` and `tests/NetTaskList.Tests/` targeting net10.0 per plan.md
- [ ] T002 Configure `src/NetTaskList/NetTaskList.csproj` with Native AOT (`<PublishAot>true</PublishAot>`), tool packaging (`<PackAsTool>true</PackAsTool>`, `<ToolCommandName>nxtl</ToolCommandName>`), and dependency `Spectre.Console`
- [ ] T003 [P] Configure `tests/NetTaskList.Tests/NetTaskList.Tests.csproj` with xUnit, Microsoft.NET.Test.Sdk, and project reference to `src/NetTaskList/NetTaskList.csproj`
- [ ] T004 [P] Create formal JSON Schema specification file in `src/NetTaskList/schemas/nxtl-v1.schema.json` based on contracts/config-schema.json

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Infraestrutura básica de domínio, abstrações de I/O, DTOs e Source Generators do System.Text.Json para Native AOT.

**⚠️ CRITICAL**: Nenhuma história de usuário pode ser iniciada antes da conclusão desta fase.

- [ ] T005 [P] Create `TaskType` enum (`Tool`, `CSharp`) in `src/NetTaskList/Domain/Models/TaskType.cs`
- [ ] T006 [P] Create `ScriptTask` domain model (`Name`, `Command`, `Type`, `Description`, `ResolveExecutionCommand()`) in `src/NetTaskList/Domain/Models/ScriptTask.cs`
- [ ] T007 [P] Create `TaskConfiguration` domain aggregate in `src/NetTaskList/Domain/Models/TaskConfiguration.cs`
- [ ] T008 [P] Create `ValidationResult` and `ValidationError` models in `src/NetTaskList/Domain/Validation/ValidationResult.cs`
- [ ] T009 [P] Create `IFileSystem` interface in `src/NetTaskList/Infrastructure/Abstractions/IFileSystem.cs` and physical implementation in `src/NetTaskList/Infrastructure/FileSystem.cs`
- [ ] T010 [P] Create DTOs `ConfigurationDto` and `ScriptTaskDto` in `src/NetTaskList/Infrastructure/Dtos/ConfigurationDto.cs`
- [ ] T011 Create `TaskJsonContext` source generator in `src/NetTaskList/Infrastructure/Serialization/TaskJsonContext.cs` for Native AOT System.Text.Json compatibility

**Checkpoint**: Fundação pronta - a implementação das histórias de usuário pode prosseguir de forma independente.

---

## Phase 3: User Story 1 - Listar e Navegar Tarefas Configuradas Interativamente (Priority: P1) 🎯 MVP

**Goal**: Ler o arquivo `nxtl.json`, renderizar uma tabela estilizada com Spectre.Console, permitir navegação interativa por setas do teclado (Up/Down) e seleção direta via `--run <nome>`.

**Independent Test**: Executar `nxtl` ou `nxtl --run build` em um diretório com configuração válida e verificar a renderização da tabela, menu interativo e resolução da tarefa.

### Tests for User Story 1 ⚠️

- [ ] T012 [P] [US1] Unit test for command resolution and direct execution mapping in `tests/NetTaskList.Tests/Infrastructure/CommandResolutionTests.cs`
- [ ] T013 [P] [US1] Unit test for CLI option parsing (`--run` / `-r` and default invocation) in `tests/NetTaskList.Tests/Presentation/CommandLineParserTests.cs`

### Implementation for User Story 1

- [ ] T014 [US1] Implement `CommandLineOptions` and `CommandLineParser` in `src/NetTaskList/Presentation/CommandLineParser.cs` supporting `--run` / `-r` and non-interactive stream detection
- [ ] T015 [US1] Implement `TaskTableRenderer` in `src/NetTaskList/Presentation/TaskTableRenderer.cs` rendering Spectre.Console tables with badges (tool vs csharp)
- [ ] T016 [US1] Implement `InteractiveTaskSelector` in `src/NetTaskList/Presentation/InteractiveTaskSelector.cs` using Spectre.Console SelectionPrompt with Arrow keys (Up/Down)
- [ ] T017 [US1] Implement core reader `JsonConfigurationReader` in `src/NetTaskList/Infrastructure/JsonConfigurationReader.cs` using `TaskJsonContext` and `IFileSystem`
- [ ] T018 [US1] Implement application orchestrator and entry point in `src/NetTaskList/Program.cs` tying CLI parsing, reading, table rendering, and selection prompt together

**Checkpoint**: Neste ponto, a User Story 1 é um MVP funcional e testável de forma autônoma.

---

## Phase 4: User Story 2 - Validação de Nomes Duplicados e Erros de Formato (Priority: P2)

**Goal**: Validar a configuração na leitura, rejeitando nomes duplicados (case-insensitive), campos obrigatórios ausentes e sintaxe JSON corrompida com mensagens diagnósticas em português (`pt-BR`).

**Independent Test**: Fornecer um arquivo JSON contendo tarefas com o mesmo nome (ex: "build" e "Build") ou sintaxe quebrada e verificar que o processo é abortado com mensagem diagnóstica em pt-BR e código de saída 1.

### Tests for User Story 2 ⚠️

- [ ] T019 [P] [US2] Unit test for duplicate name detection (case-insensitive) and missing required fields in `tests/NetTaskList.Tests/Domain/ConfigurationValidatorTests.cs`
- [ ] T020 [P] [US2] Unit test for malformed JSON syntax error diagnostics in `tests/NetTaskList.Tests/Infrastructure/JsonConfigurationReaderTests.cs`

### Implementation for User Story 2

- [ ] T021 [US2] Implement `IConfigurationValidator` and `ConfigurationValidator` in `src/NetTaskList/Domain/Validation/ConfigurationValidator.cs` enforcing VR-001 (case-insensitive unique names), VR-002 (name required), VR-003 (command required), and VR-004 (type validation)
- [ ] T022 [US2] Integrate `ConfigurationValidator` into `src/NetTaskList/Infrastructure/JsonConfigurationReader.cs` to return detailed pt-BR validation errors prior to task processing
- [ ] T023 [US2] Update `src/NetTaskList/Program.cs` to format and render validation error messages using Spectre.Console markup and exit with code 1

**Checkpoint**: Histórias 1 e 2 funcionam de forma integrada e independente, impedindo ambiguidades de execução.

---

## Phase 5: User Story 3 - Suporte a Múltiplos Scripts e Tipificação (Tool vs C# File-Scoped) (Priority: P3)

**Goal**: Suportar a declaração simultânea de dezenas de tarefas mistas (`tool` e `csharp`), inferindo `dotnet run <script.cs>` para arquivos C# file-scoped e mantendo comandos completos inalterados.

**Independent Test**: Carregar um arquivo com múltiplos scripts contendo ferramentas CLI e scripts C# (ex: `scripts/seed.cs` e `dotnet run custom.cs`), verificando a correta identificação dos tipos, badges e comandos inferidos.

### Tests for User Story 3 ⚠️

- [ ] T024 [P] [US3] Unit test for multi-script loading and type-specific execution command resolution in `tests/NetTaskList.Tests/Infrastructure/CommandResolutionTests.cs`

### Implementation for User Story 3

- [ ] T025 [US3] Implement C# file-scoped command resolution logic in `src/NetTaskList/Domain/Models/ScriptTask.cs` (inferring `dotnet run {command}` when command ends with `.cs` without prefix)
- [ ] T026 [US3] Enhance `src/NetTaskList/Infrastructure/JsonConfigurationReader.cs` to support both root collection formats: object `{ "scripts": [...] }` / `{ "tasks": [...] }` and direct array `[...]`
- [ ] T027 [US3] Update `src/NetTaskList/Presentation/TaskTableRenderer.cs` to support clean visual rendering and color badges for large collections of tasks

**Checkpoint**: Todas as tipificações e múltiplos scripts são suportados com alto desempenho.

---

## Phase 6: User Story 4 - Facilidade de Edição Manual, Inicialização (`--init`) e Localização (`--config`) (Priority: P4)

**Goal**: Permitir fácil criação e manutenção manual de `nxtl.json` com comando de inicialização `--init`, suporte a caminho customizado via `--config <caminho>`, e mensagens amigáveis de arquivo ausente.

**Independent Test**: Executar `nxtl --init` para gerar `nxtl.json` inicial e executar `nxtl --config configs/custom.json` para carregar tarefas de local alternativo.

### Tests for User Story 4 ⚠️

- [ ] T028 [P] [US4] Unit test for `--init` template creation and `--config` custom path resolution in `tests/NetTaskList.Tests/Presentation/CommandLineParserTests.cs`

### Implementation for User Story 4

- [ ] T029 [US4] Implement template generator in `src/NetTaskList/Infrastructure/TemplateConfigurationGenerator.cs` generating clean `nxtl.json` with schema reference and sample tasks
- [ ] T030 [US4] Implement `--init` and `--config` CLI handlers in `src/NetTaskList/Presentation/CommandLineParser.cs` and wire into `src/NetTaskList/Program.cs`
- [ ] T031 [US4] Implement friendly pt-BR diagnostic message when `nxtl.json` is missing in `src/NetTaskList/Program.cs`, including guidance to run `nxtl --init` or `--config`

**Checkpoint**: Experiência do desenvolvedor completa para criação, edição e descoberta de arquivos de configuração.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Documentação, empacotamento Native AOT, publicação NuGet e validação global.

- [ ] T032 [P] Create repository documentation and usage guide in `README.md` detailing installation, `nxtl.json` configuration, and CLI commands
- [ ] T033 Configure Native AOT publishing profile and parameters in `src/NetTaskList/NetTaskList.csproj` ensuring trim-safety and zero reflection warnings
- [ ] T034 Run end-to-end quickstart validation scenarios from `specs/001-json-config-processor/quickstart.md` validating `dotnet test`, `dotnet pack`, and Native AOT `dotnet publish`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências - inicia imediatamente.
- **Foundational (Phase 2)**: Depende da conclusão da Fase 1 - BLOQUEIA todas as histórias de usuário.
- **User Stories (Phase 3+)**: Todas dependem da conclusão da Fase 2 (Foundational).
  - Devem ser executadas na ordem de prioridade (P1 → P2 → P3 → P4) para entrega incremental contínua.
- **Polish (Phase 7)**: Depende da conclusão de todas as histórias de usuário desejadas.

### User Story Dependencies

- **User Story 1 (P1)**: Depende apenas da Fase 2. Não possui dependências de outras histórias (entrega o MVP).
- **User Story 2 (P2)**: Depende da Fase 2. Integra validações semânticas ao leitor construído na US1.
- **User Story 3 (P3)**: Depende da Fase 2 e enriquece a resolução de comandos e formatos suportados na US1.
- **User Story 4 (P4)**: Depende da Fase 2 e adiciona os utilitários de CLI (`--init`, `--config`).

### Within Each User Story

- Testes automatizados DEVEM ser escritos antes da implementação e falhar inicialmente (TDD).
- Modelos e contratos antes de serviços.
- Serviços antes da integração com o console/CLI.
- Cada história deve estar 100% testada e funcional antes do avanço para a próxima.

---

## Parallel Opportunities

- **Fase 1 (Setup)**: T003 e T004 podem rodar em paralelo após T001/T002.
- **Fase 2 (Foundational)**: T005, T006, T007, T008, T009 e T010 podem ser implementados em paralelo por estarem em arquivos e camadas desacopladas.
- **Fase 3 (US1)**: Testes T012 e T013 podem ser escritos em paralelo; componentes visuais T015 e T016 podem ser desenvolvidos em paralelo antes de T018.
- **Fase 4 (US2)**: Testes T019 e T020 podem ser escritos em paralelo.
- **Fase 5 (US3)**: Teste T024 e lógica de inferência T025 podem rodar em paralelo com melhorias de DTOs T026.
- **Fase 6 (US4)**: Teste T028 e gerador de templates T029 podem rodar em paralelo.

---

## Parallel Example: User Story 1

```bash
# Executar testes unitários da User Story 1 em paralelo:
Task: "Unit test for command resolution and direct execution mapping in tests/NetTaskList.Tests/Infrastructure/CommandResolutionTests.cs"
Task: "Unit test for CLI option parsing (--run / -r and default invocation) in tests/NetTaskList.Tests/Presentation/CommandLineParserTests.cs"

# Desenvolver componentes de apresentação em paralelo:
Task: "Implement TaskTableRenderer in src/NetTaskList/Presentation/TaskTableRenderer.cs"
Task: "Implement InteractiveTaskSelector in src/NetTaskList/Presentation/InteractiveTaskSelector.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Concluir **Fase 1: Setup** (projetos `src/` e `tests/`, net10.0, Native AOT, Spectre.Console).
2. Concluir **Fase 2: Foundational** (modelos de domínio, DTOs, `TaskJsonContext`).
3. Concluir **Fase 3: User Story 1** (leitura básica, exibição tabular e navegação por setas).
4. **VALIDAÇÃO MVP**: Executar `dotnet test` e rodar a CLI para validar a listagem e seleção de tarefas.

### Incremental Delivery

1. Setup + Foundational concluídos → Fundação sólida pronta.
2. User Story 1 adicionada → Listagem interativa e `--run` funcionais (MVP entregue!).
3. User Story 2 adicionada → Validação estrita de unicidade e mensagens diagnósticas em pt-BR.
4. User Story 3 adicionada → Resolução flexível para scripts C# file-scoped e formatos mistos.
5. User Story 4 adicionada → Comandos de inicialização (`--init`) e customização (`--config`).
6. Polish → Compilação Native AOT validada e empacotamento NuGet.
