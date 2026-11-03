# Tasks: Suporte ao Arquivo package.json (package.json Support)

**Input**: Design documents from `specs/007-support-package-json/` (`spec.md`, `plan.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`)  
**Prerequisites**: `plan.md` (completed), `spec.md` (completed), `constitution.md` (ratified v2.1.0)  
**Tests**: Obrigatórios por princípio constitucional (Princípio III - Test-First & Mandatory Unit Test Coverage).  
**Organization**: As tarefas estão estruturadas por história de usuário para viabilizar implementação e testes independentes de cada entrega.

---

## Format: `- [ ] [ID] [P?] [Story?] Description with file path`

- **[P]**: Tarefa paralelizada (arquivos distintos, sem dependência de tarefas incompletas)
- **[Story]**: Identificador da história de usuário (ex: `[US1]`, `[US2]`, `[US3]`, `[US4]`)
- Todos os caminhos de arquivo são especificados explicitamente

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Estruturas de dados básicas e modelos compartilhados necessários para suportar o formato `package.json`.

- [x] T001 [P] Create `ConfigurationSourceType` enum (`NxtlJson`, `PackageJson`, `Custom`) in `src/NetTaskList/Domain/Models/ConfigurationSourceType.cs`
- [x] T002 [P] Create `PackageJsonDto` mapping `name` and `scripts` (`Dictionary<string, JsonElement>?`) in `src/NetTaskList/Infrastructure/Dtos/PackageJsonDto.cs`
- [x] T003 Register `PackageJsonDto` in `src/NetTaskList/Infrastructure/Serialization/TaskJsonContext.cs` for Native AOT trim-safe source generation

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Infraestrutura de resolução e abstração que bloqueia a implementação das histórias de usuário.

**⚠️ CRITICAL**: Nenhuma história de usuário pode ser iniciada antes da conclusão desta fase.

- [x] T004 Update `TaskConfiguration` in `src/NetTaskList/Domain/Models/TaskConfiguration.cs` to include `ConfigurationSourceType SourceType` property
- [x] T005 [P] Create `ConfigurationFileResolver` in `src/NetTaskList/Infrastructure/ConfigurationFileResolver.cs` implementing discovery logic via `IFileSystem`
- [x] T006 [P] Unit tests for `ConfigurationFileResolver` in `tests/NetTaskList.Tests/Infrastructure/ConfigurationFileResolverTests.cs`

**Checkpoint**: Base compartilhada pronta. A implementação das histórias de usuário pode iniciar de forma independente.

---

## Phase 3: User Story 1 - Carregar e Executar Tarefas a Partir de package.json com Ciclo de Vida npm (Priority: P1) 🎯 MVP

**Goal**: Permitir que projetos contendo apenas `package.json` tenham suas tarefas descobertas, listadas e executadas pelo `nxtl`, orquestrando automaticamente scripts de ciclo de vida (`pre<tarefa>` e `post<tarefa>`) e ocultando esses ganchos do menu interativo quando o comando principal existir.

**Independent Test**: Criar um diretório de teste contendo apenas `package.json` com scripts `"prebuild"`, `"build"`, `"postbuild"` e `"prepare"`. Ao executar `nxtl`, o menu interativo exibe apenas `"build"` e `"prepare"`. Ao executar `nxtl --run build`, os comandos `"prebuild"`, `"build"` e `"postbuild"` são executados ordenadamente em sequência.

### Tests for User Story 1 ⚠️

- [x] T007 [P] [US1] Unit tests for reading and extracting tasks from `package.json` in `tests/NetTaskList.Tests/Infrastructure/PackageJsonReaderTests.cs`
- [x] T008 [P] [US1] Unit tests for interactive menu filtering of `pre*`/`post*` hooks in `tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorFilteringTests.cs`
- [x] T009 [P] [US1] Unit tests for npm lifecycle hooks execution coordinator in `tests/NetTaskList.Tests/Presentation/TaskExecutionCoordinatorTests.cs`

### Implementation for User Story 1

- [x] T010 [US1] Implement `package.json` parsing in `src/NetTaskList/Infrastructure/JsonConfigurationReader.cs` converting script entries into `ScriptTask` with `TaskType.Tool`
- [x] T011 [US1] Implement lifecycle hook detection and sequential execution (`pre<name>` -> `<name>` -> `post<name>`) with abort semantics in `src/NetTaskList/Presentation/TaskExecutionCoordinator.cs`
- [x] T012 [US1] Update `src/NetTaskList/Presentation/InteractiveTaskSelector.cs` to filter out `pre*` and `post*` tasks when origin is `PackageJson` and corresponding main task exists
- [x] T013 [US1] Integrate `ConfigurationFileResolver`, `JsonConfigurationReader`, and `TaskExecutionCoordinator` into `src/NetTaskList/Program.cs` for User Story 1 execution

**Checkpoint**: User Story 1 (MVP) completamente funcional e testável de forma independente.

---

## Phase 4: User Story 2 - Prevalência de nxtl.json sobre package.json (Priority: P2)

**Goal**: Garantir que quando `nxtl.json` e `package.json` coexistirem no mesmo diretório, o `nxtl.json` prevaleça com exclusividade, ignorando totalmente o `package.json` e sem fallback silencioso em caso de erro no `nxtl.json`.

**Independent Test**: Criar um diretório contendo simultaneamente `nxtl.json` e `package.json` com tarefas distintas. Ao executar `nxtl`, verificar que exclusivamente as tarefas de `nxtl.json` são carregadas e listadas. Em caso de `nxtl.json` com sintaxe corrompida, o erro do `nxtl.json` deve ser exibido sem ler o `package.json`.

### Tests for User Story 2 ⚠️

- [x] T014 [P] [US2] Unit tests verifying `nxtl.json` precedence over `package.json` and error reporting without fallback in `tests/NetTaskList.Tests/Infrastructure/ConfigurationFileResolverPrecedenceTests.cs`

### Implementation for User Story 2

- [x] T015 [US2] Update `ConfigurationFileResolver` in `src/NetTaskList/Infrastructure/ConfigurationFileResolver.cs` and `src/NetTaskList/Program.cs` to guarantee absolute precedence and error propagation of `nxtl.json`

**Checkpoint**: User Story 2 completamente funcional e testável de forma independente.

---

## Phase 5: User Story 3 - Resolução de Arquivo Explícito e Ausência de Configuração (Priority: P3)

**Goal**: Assegurar que o parâmetro `--config <caminho>` aponte diretamente para o arquivo indicado ignorando a auto-descoberta, que mensagens diagnósticas em `pt-BR` sejam exibidas quando nenhum arquivo for encontrado, e que `nxtl --init` gere o modelo padrão sem alterar `package.json`.

**Independent Test**: Executar `nxtl` em diretório vazio e validar mensagem de erro em `pt-BR` com código de saída 1. Executar `nxtl --config custom.json` em diretório com `package.json` e validar que o arquivo explícito foi o único carregado. Executar `nxtl --init` e verificar a criação do `nxtl.json` padrão.

### Tests for User Story 3 ⚠️

- [x] T016 [P] [US3] Unit tests for missing configuration file diagnostics in `tests/NetTaskList.Tests/Presentation/MissingConfigurationDiagnosticsTests.cs`
- [x] T017 [P] [US3] Unit tests for explicit `--config` path resolution in `tests/NetTaskList.Tests/Infrastructure/ExplicitConfigResolutionTests.cs`

### Implementation for User Story 3

- [x] T018 [US3] Update diagnostic error message in `src/NetTaskList/Program.cs` in `pt-BR` when neither `nxtl.json` nor `package.json` is found ("Nenhum arquivo de configuração ('nxtl.json' ou 'package.json') foi encontrado")
- [x] T019 [US3] Ensure `nxtl --init` in `src/NetTaskList/Program.cs` preserves default template creation without reading or modifying `package.json`

**Checkpoint**: User Story 3 completamente funcional e testável de forma independente.

---

## Phase 6: User Story 4 - Resiliência e Validação de Formato do package.json (Priority: P4)

**Goal**: Garantir tratamento elegante e amigável em `pt-BR` quando `package.json` for inválido, não contiver scripts, possuir scripts vazios ou valores não textuais na seção `"scripts"`.

**Independent Test**: Executar `nxtl` fornecendo `package.json` com `"scripts": {}`, JSON corrompido ou scripts com valores numéricos/objetos, verificando que mensagens de diagnóstico em português são apresentadas sem exceções não tratadas.

### Tests for User Story 4 ⚠️

- [x] T020 [P] [US4] Unit tests for empty scripts, malformed JSON, and non-string script values in `tests/NetTaskList.Tests/Infrastructure/PackageJsonValidationTests.cs`

### Implementation for User Story 4

- [x] T021 [US4] Implement validation in `src/NetTaskList/Infrastructure/JsonConfigurationReader.cs` for non-string, null, or empty values in `package.json` `"scripts"` with pt-BR error messages
- [x] T022 [US4] Handle empty or absent `"scripts"` in `src/NetTaskList/Program.cs` and `src/NetTaskList/Infrastructure/JsonConfigurationReader.cs` displaying polite notification without throwing unhandled exceptions

**Checkpoint**: Todas as histórias de usuário funcionais e validadas.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Verificação global de integridade, testes de ponta a ponta e conformidade com a Constituição.

- [x] T023 [P] Execute end-to-end quickstart validation scenarios defined in `specs/007-support-package-json/quickstart.md`
- [x] T024 [P] Update help message or documentation if applicable in `src/NetTaskList/Program.cs`
- [x] T025 Run full automated test suite via `dotnet test` to ensure 100% test pass rate with zero warnings

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências — pode ser iniciado imediatamente.
- **Foundational (Phase 2)**: Depende da conclusão do Setup — BLOQUEIA todas as histórias de usuário.
- **User Stories (Phase 3+)**: Todas dependem da conclusão da fase Foundational.
  - Podem ser executadas em sequência ordenada por prioridade (P1 → P2 → P3 → P4).
- **Polish (Phase 7)**: Depende da conclusão de todas as histórias de usuário desejadas.

### User Story Dependencies

- **User Story 1 (P1)**: Depende da fase Foundational (Phase 2). Não depende de outras histórias (MVP independente).
- **User Story 2 (P2)**: Depende da fase Foundational (Phase 2) e integra-se à resolução estabelecida em US1.
- **User Story 3 (P3)**: Depende da fase Foundational (Phase 2) e integra-se ao roteamento de Program.cs de US1/US2.
- **User Story 4 (P4)**: Depende do leitor estabelecido em US1.

---

## Parallel Execution Opportunities

- Tarefas T001 e T002 podem rodar em paralelo.
- Tarefas T005 e T006 podem ser elaboradas em paralelo.
- Na User Story 1: T007, T008 e T009 (testes unitários) podem rodar em paralelo antes da implementação dos componentes.
- Na User Story 3: T016 e T017 podem rodar em paralelo.
- Na User Story 4: T020 e T021 operam em testes e leitor.

---

## Implementation Strategy

### MVP First (User Story 1 Only)
1. Concluir Phase 1: Setup (T001 - T003).
2. Concluir Phase 2: Foundational (T004 - T006).
3. Concluir Phase 3: User Story 1 (T007 - T013).
4. **VALIDAÇÃO E TESTE**: Executar `dotnet test` e comprovar funcionamento em projeto com `package.json`.

### Incremental Delivery
1. Setup + Foundational concluídos.
2. User Story 1 implementada e testada (MVP entrega suporte a `package.json` e ciclo de vida).
3. User Story 2 implementada (garante prevalência absoluta de `nxtl.json`).
4. User Story 3 implementada (suporte a `--config` explícito e diagnóstico pt-BR).
5. User Story 4 implementada (resiliência total contra JSON corrompido e scripts inválidos).
6. Polish: Validação dos cenários de `quickstart.md` e suíte de testes completa.

