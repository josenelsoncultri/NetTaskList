# Tasks: Mecanismo de Execução de Tarefas e Scripts Configurados

**Input**: Design documents from `/specs/003-execute-configured-tasks/`  
**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [data-model.md](data-model.md), [contracts/process-execution-contract.md](contracts/process-execution-contract.md), [research.md](research.md), [quickstart.md](quickstart.md), [.specify/memory/constitution.md](../../.specify/memory/constitution.md)  
**Tests**: Testes automatizados unitários são obrigatórios conforme o Princípio III da Constituição do projeto (`NetTaskList`).  
**Organization**: As tarefas estão organizadas por jornadas do usuário (User Stories) para permitir implementação e validação independentes de cada incremento funcional.

## Format: `- [ ] [TaskID] [P?] [Story?] Description with file path`

- **[P]**: Tarefa paralelizada (arquivos distintos, sem dependência de tarefas incompletas).
- **[Story]**: Identificador da história de usuário ([US1], [US2], [US3]).
- Todos os caminhos de arquivos são explícitos e referenciam a raiz do repositório (`src/` e `tests/`).

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Criação das entidades básicas de resultado de execução, interfaces e mocks para desacoplamento e testabilidade.

- [X] T001 [P] Create `ProcessExecutionResult` model with `ExitCode`, `Success`, `ErrorMessage`, and factory methods in `src/NetTaskList/Domain/Models/ProcessExecutionResult.cs`
- [X] T002 [P] Create `IProcessExecutor` interface defining `Execute(string commandLine, string? workingDirectory = null)` in `src/NetTaskList/Infrastructure/Abstractions/IProcessExecutor.cs`
- [X] T003 [P] Create `MockProcessExecutor` mock class implementing `IProcessExecutor` for simulating exit codes and recording executions in `tests/NetTaskList.Tests/Mocks/MockProcessExecutor.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Implementação da infraestrutura de execução de processos do sistema operacional via shell com conexão direta aos descritores de console nativos.

**⚠️ CRITICAL**: Nenhuma história de usuário pode ser finalizada antes da conclusão desta fase.

- [X] T004 Implement `ProcessExecutor` in `src/NetTaskList/Infrastructure/ProcessExecutor.cs` using `Process.Start` with OS shell resolution (`cmd.exe /c` on Windows, `/bin/sh -c` on Unix/macOS) and direct console stream attachment (`Redirect* = false`)
- [X] T005 [P] Implement unit tests for `ProcessExecutor` verifying shell command formatting and start info properties in `tests/NetTaskList.Tests/Infrastructure/ProcessExecutorTests.cs`

**Checkpoint**: Fundação pronta - a implementação e testes das histórias de usuário podem prosseguir de forma independente.

---

## Phase 3: User Story 1 - Executar Tarefas de Comandos Normais (Priority: P1) 🎯 MVP

**Goal**: Permitir que tarefas do tipo comando normal (`tool`) sejam executadas diretamente no terminal, tanto via parâmetro direto `--run <nome>` quanto após seleção interativa no menu, preservando em tempo real os fluxos de entrada e saída do console.

**Independent Test**: Executar uma tarefa configurada com comando normal (ex: `echo "Hello"`) via `nxtl --run <nome>` e verificar a saída no console e o retorno com código de sucesso 0.

### Tests for User Story 1 ⚠️

- [X] T006 [P] [US1] Create unit tests in `tests/NetTaskList.Tests/Infrastructure/TaskExecutionTests.cs` verifying that tool tasks dispatch their resolved command line through `IProcessExecutor`
- [X] T007 [P] [US1] Create unit tests in `tests/NetTaskList.Tests/Presentation/ProgramExecutionTests.cs` verifying that both direct execution (`--run`) and interactive task selection trigger `IProcessExecutor.Execute` and return the process exit code

### Implementation for User Story 1

- [X] T008 [US1] Integrate `IProcessExecutor` into direct execution mode (`--run` / `-r`) in `src/NetTaskList/Program.cs` to launch the resolved command and return its exit code
- [X] T009 [US1] Integrate `IProcessExecutor` into interactive task selection mode in `src/NetTaskList/Program.cs` to launch the selected task and return its exit code

**Checkpoint**: Neste estágio, a User Story 1 (MVP) está completamente funcional e testável de forma independente.

---

## Phase 4: User Story 2 - Executar Scripts C# File-Scoped (Priority: P2)

**Goal**: Reconhecer tarefas do tipo `csharp`, verificar previamente a existência do arquivo `.cs` no disco (com mensagem de diagnóstico amigável em `pt-BR` se ausente) e executar o script via .NET CLI (`dotnet run`).

**Independent Test**: Criar um arquivo `.cs` de teste e acioná-lo via `nxtl --run <csharp-task>`, conferindo que `dotnet run` é invocado; em seguida, apontar para um arquivo inexistente e conferir a mensagem `[bold red]Erro:[/] O arquivo de script C# '[yellow]...[/]' não foi encontrado.` com código de saída 1.

### Tests for User Story 2 ⚠️

- [X] T010 [P] [US2] Create unit tests in `tests/NetTaskList.Tests/Infrastructure/CSharpTaskExecutionTests.cs` verifying automatic `dotnet run` resolution, script file existence validation, and friendly error reporting for missing `.cs` files

### Implementation for User Story 2

- [X] T011 [US2] Implement C# script file existence validation in `src/NetTaskList/Program.cs` using `IFileSystem.FileExists` prior to executing tasks of type `CSharp`
- [X] T012 [US2] Implement C# file-scoped application execution dispatch in `src/NetTaskList/Program.cs` using the resolved `dotnet run` command line

**Checkpoint**: Neste estágio, tanto comandos normais (`tool`) quanto scripts C# file-scoped (`csharp`) operam de ponta a ponta com validação integrada.

---

## Phase 5: User Story 3 - Propagação de Códigos de Saída e Tratamento de Erros (Priority: P3)

**Goal**: Assegurar que o código de saída (exit code) do processo executado seja propagado fielmente como o código de retorno da ferramenta `nxtl`, tratando erros de inicialização de processos com mensagens claras em português.

**Independent Test**: Executar uma tarefa configurada com comando que retorna código de saída diferente de zero (ex: `exit 42`) e verificar que o `nxtl` finaliza com o mesmo código; simular falha de execução de processo do SO e verificar a mensagem de erro amigável.

### Tests for User Story 3 ⚠️

- [X] T013 [P] [US3] Create unit tests in `tests/NetTaskList.Tests/Infrastructure/ExitCodePropagationTests.cs` verifying faithful propagation of non-zero exit codes (e.g., 1, 42, 127) and handling of process launch failures

### Implementation for User Story 3

- [X] T014 [US3] Implement exit code propagation and friendly error formatting (`[bold red]Erro ao executar tarefa:[/] ...`) in `src/NetTaskList/Program.cs` for process launch failures

**Checkpoint**: Todas as histórias de usuário estão integradas, testadas e prontas para uso em ambientes locais e pipelines de automação.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Verificação completa da suíte de testes, integridade de compilação sem avisos e validação dos cenários do quickstart.

- [X] T015 [P] Run full test suite via `dotnet test` ensuring 100% test pass rate across all unit tests
- [X] T016 Verify zero compiler warnings and Native AOT compatibility (`<PublishAot>true</PublishAot>`) in `src/NetTaskList/NetTaskList.csproj`
- [X] T017 Execute end-to-end validation scenarios defined in `specs/003-execute-configured-tasks/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências externas — pode iniciar imediatamente.
- **Foundational (Phase 2)**: Depende da conclusão da Fase 1 (T001, T002, T003) — BLOQUEIA a implementação das histórias de usuário.
- **User Story 1 (Phase 3)**: Depende da conclusão da Fundação (Fase 2). Não depende de outras histórias.
- **User Story 2 (Phase 4)**: Depende da conclusão da Fase 2 e integra-se com a coordenação de execução do US1.
- **User Story 3 (Phase 5)**: Depende da conclusão da Fase 2 e integra-se com o retorno de saída do US1/US2.
- **Polish (Phase 6)**: Depende da conclusão de todas as histórias de usuário implementadas.

### Within Each User Story

1. Testes unitários correspondentes à história devem ser especificados primeiro.
2. Implementação das regras e integrações no `Program.cs` / serviços de infraestrutura.
3. Validação do incremento funcional antes de avançar para a próxima história.

### Parallel Opportunities

- **Fase 1 (Setup)**: `T001`, `T002` e `T003` operam em arquivos completamente distintos e podem ser criados em paralelo.
- **Fase 2 (Foundational)**: `T005` pode ser preparado em paralelo com `T004`.
- **Fase 3 (User Story 1)**: `T006` e `T007` (testes de infraestrutura e apresentação) podem ser desenvolvidos em paralelo.
- **Fase 4 (User Story 2)**: `T010` (testes C#) pode ser escrito em paralelo com a preparação dos cenários de teste.
- **Fase 5 (User Story 3)**: `T013` (testes de exit code) pode ser escrito em paralelo.

---

## Parallel Example: User Story 1

```bash
# Executar testes unitários do US1 em paralelo:
Task: "Create unit tests in tests/NetTaskList.Tests/Infrastructure/TaskExecutionTests.cs"
Task: "Create unit tests in tests/NetTaskList.Tests/Presentation/ProgramExecutionTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Concluir Fase 1 (Setup: `ProcessExecutionResult`, `IProcessExecutor`, `MockProcessExecutor`).
2. Concluir Fase 2 (Foundational: `ProcessExecutor` e testes).
3. Concluir Fase 3 (User Story 1: Execução de comandos normais `tool` via `--run` e menu interativo).
4. **VALIDAR MVP**: Testar execução de comandos como `echo` e `git status`.
5. Entregar incremento funcional.

### Incremental Delivery

1. Setup + Foundational concluídos → Base sólida e testável.
2. User Story 1 implementada → Execução de comandos de terminal funcionando (MVP).
3. User Story 2 implementada → Scripts C# file-scoped com `dotnet run` e validação de arquivos.
4. User Story 3 implementada → Códigos de saída de erro propagados e tratamento de exceções de SO.
5. Polish concluído → Validação final de ponta a ponta e compilação Native AOT.
