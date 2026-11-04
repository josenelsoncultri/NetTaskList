# Tasks: Suporte a Múltiplos Comandos Sequenciais em Tarefas (Sequential Task Commands)

**Input**: Design documents from `specs/008-sequential-task-commands/` (`spec.md`, `plan.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`)  
**Prerequisites**: `plan.md`, `spec.md`, `data-model.md`, `contracts/`  
**Constitution Compliance**: Principle I (pt-BR UX), Principle II (Declarative JSON), Principle III (Test-First), Principle IV (Minimal Deps), Principle V (.NET 10 Tool), Principle VI (Version bump)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verificação do ambiente e baseline da solução

- [X] T001 Executar `dotnet test` na raiz da solução para confirmar baseline limpo de testes e compilação sem avisos

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Infraestrutura de serialização polimórfica e modelo de dados de domínio que bloqueia todas as histórias

**⚠️ CRITICAL**: Nenhuma história de usuário pode ser finalizada antes da conclusão desta fase

- [X] T002 [P] Implementar o conversor polimórfico `CommandPropertyJsonConverter` em `src/NetTaskList/Infrastructure/Serialization/CommandPropertyJsonConverter.cs` para ler `command` como `string` única ou `List<string>`
- [X] T003 [P] Atualizar a propriedade `Command`/`Commands` com `[JsonConverter(typeof(CommandPropertyJsonConverter))]` em `src/NetTaskList/Infrastructure/Dtos/ConfigurationDto.cs`
- [X] T004 Registrar o conversor e tipos no contexto de geração AOT em `src/NetTaskList/Infrastructure/Serialization/TaskJsonContext.cs`
- [X] T005 [P] Atualizar o modelo de domínio `ScriptTask` em `src/NetTaskList/Domain/Models/ScriptTask.cs` para suportar `IReadOnlyList<string> Commands`, `ResolveExecutionCommands()` e manter `Command` e `ResolveExecutionCommand()` retrocompatíveis
- [X] T006 Atualizar a instanciação de tarefas em `src/NetTaskList/Infrastructure/JsonConfigurationReader.cs` para repassar a lista de comandos para `ScriptTask`
- [X] T007 [P] Criar testes unitários para o conversor e modelo de domínio em `tests/NetTaskList.Tests/Infrastructure/CommandPropertyJsonConverterTests.cs` e `tests/NetTaskList.Tests/Domain/ScriptTaskTests.cs`

**Checkpoint**: Fundação pronta - a infraestrutura de dados suporta tanto comandos únicos quanto arrays de comandos.

---

## Phase 3: User Story 1 - Configuração e Execução Sequencial de Múltiplos Comandos (Priority: P1) 🎯 MVP

**Goal**: Permitir que tarefas configuradas com múltiplos comandos no `nxtl.json` executem um por um na sequência configurada, com indicação de progresso `Comando [X/N]: <linha>` no terminal

**Independent Test**: Executar uma tarefa configurada com múltiplos comandos via CLI e verificar a execução ordenada de cada comando com os rótulos de progresso corretos

### Tests for User Story 1 ⚠️

- [X] T008 [P] [US1] Criar testes unitários em `tests/NetTaskList.Tests/Presentation/TaskExecutionCoordinatorTests.cs` validando a execução ordenada sequencial e os rótulos `Comando [X/N]: <cmd>` e `Comando: <cmd>`

### Implementation for User Story 1

- [X] T009 [US1] Atualizar `TaskExecutionCoordinator` em `src/NetTaskList/Presentation/TaskExecutionCoordinator.cs` para iterar sobre `task.ResolveExecutionCommands()` e exibir `Comando [X/N]: [bold yellow]{cmd}[/]` quando $N > 1$ e `Comando: [bold yellow]{cmd}[/]` quando $N == 1$
- [X] T010 [US1] Validar e assegurar a compatibilidade reversa para tarefas com comando único e tarefas originadas de `package.json` em `tests/NetTaskList.Tests/Presentation/TaskExecutionCoordinatorTests.cs`

**Checkpoint**: MVP concluído - o `nxtl` executa tarefas com múltiplos comandos sequenciais no terminal com sucesso.

---

## Phase 4: User Story 2 - Interrupção Imediata da Cadeia de Execução em Caso de Erro (Priority: P2)

**Goal**: Interromper imediatamente a execução ao encontrar erro em qualquer comando intermediário, impedindo comandos posteriores e propagando o código de erro

**Independent Test**: Executar uma tarefa com 3 comandos onde o segundo falha; verificar que o primeiro executa, o segundo falha com mensagem em pt-BR, o terceiro nunca executa e o CLI retorna o código de saída do comando falho

### Tests for User Story 2 ⚠️

- [X] T011 [P] [US2] Criar testes unitários em `tests/NetTaskList.Tests/Presentation/TaskExecutionCoordinatorTests.cs` para interrupção imediata em erro (`ExitCode != 0` ou exceção de processo) e propagação do código de saída

### Implementation for User Story 2

- [X] T012 [US2] Implementar a lógica de parada imediata e emissão da mensagem `Execução interrompida: O comando falhou com código de saída {ExitCode}. Comandos posteriores não serão executados.` em `src/NetTaskList/Presentation/TaskExecutionCoordinator.cs`
- [X] T013 [US2] Implementar interrupção imediata com código `1` caso um script C# (`.cs`) na sequência não seja encontrado em `src/NetTaskList/Presentation/TaskExecutionCoordinator.cs`

**Checkpoint**: Histórias 1 e 2 funcionais - comandos sequenciais executam com sucesso ou interrompem imediatamente em caso de falha.

---

## Phase 5: User Story 3 - Validação e Flexibilidade Declarativa no Arquivo de Configuração (Priority: P3)

**Goal**: Validar arrays de comandos vazios ou com itens em branco no `ConfigurationValidator`, atualizar o JSON Schema e exibir `<sem descrição>` no menu interativo para tarefas sem descrição

**Independent Test**: Carregar arquivo com array vazio ou itens inválidos e validar mensagens de erro em pt-BR; abrir menu interativo com tarefa multi-comando sem descrição e verificar o texto `<sem descrição>`

### Tests for User Story 3 ⚠️

- [X] T014 [P] [US3] Criar testes unitários em `tests/NetTaskList.Tests/Domain/ConfigurationValidatorTests.cs` validando rejeição de arrays vazios (`[]`) e comandos compostos por espaços em branco
- [X] T016 [P] [US3] Criar testes unitários em `tests/NetTaskList.Tests/Presentation/InteractiveTaskSelectorTests.cs` verificando o rótulo `<sem descrição>` para tarefas com múltiplos comandos sem descrição

### Implementation for User Story 3

- [X] T015 [US3] Implementar regras VR-003 e VR-004 para validação de listas de comandos no `ConfigurationValidator` em `src/NetTaskList/Domain/Validation/ConfigurationValidator.cs`
- [X] T017 [US3] Atualizar `InteractiveTaskSelector` em `src/NetTaskList/Presentation/InteractiveTaskSelector.cs` para exibir `"{task.Name} [{typeTag}] - <sem descrição>"` quando `task.Description` for nula/vazia e `task.Commands.Count > 1`
- [X] T018 [P] [US3] Atualizar o JSON Schema em `src/NetTaskList/schemas/nxtl-v1.schema.json` para permitir `command` como `oneOf` (`string` com `minLength: 1` ou `array` com `minItems: 1` e itens com `minLength: 1`)

**Checkpoint**: Todas as histórias de usuário implementadas e validadas com testes automatizados.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Verificação final, validação prática ponta a ponta e governança constitucional de versão

- [X] T019 [P] Atualizar `src/NetTaskList/Infrastructure/TemplateConfigurationGenerator.cs` e `nxtl.json` da raiz com exemplo de tarefa contendo array de comandos
- [X] T020 Executar a suite completa `dotnet test` garantindo 100% de testes aprovados e compilação sem avisos
- [X] T021 Executar os 4 cenários de validação ponta a ponta definidos em `specs/008-sequential-task-commands/quickstart.md`
- [X] T022 Atualizar o elemento `<Version>` em `src/NetTaskList/NetTaskList.csproj` para a próxima versão Minor (`X.(Y+1).0`) em conformidade com o Princípio VI da Constituição

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: Sem dependências - execução imediata.
- **Phase 2 (Foundational)**: Depende da Phase 1 - BLOQUEIA todas as histórias de usuário.
- **Phase 3 (User Story 1 - P1 🎯 MVP)**: Depende da Phase 2.
- **Phase 4 (User Story 2 - P2)**: Depende da Phase 3 (estende o coordenador de execução com a parada em falha).
- **Phase 5 (User Story 3 - P3)**: Depende da Phase 2 para validação e seleção interativa.
- **Phase 6 (Polish)**: Depende da conclusão de todas as histórias (Phases 3, 4 e 5).

### Parallel Opportunities

- **Na Phase 2**: T002 e T005 podem ser implementados em paralelo (serialização vs modelo de domínio).
- **Na Phase 3**: T008 (testes) pode ser escrito antes da implementação de T009.
- **Na Phase 4**: T011 (testes) pode ser escrito antes de T012 e T013.
- **Na Phase 5**: T014 (testes validator), T016 (testes selector) e T018 (schema JSON) podem ser executados em paralelo.

---

## Implementation Strategy

### MVP First (User Story 1 Only)
1. Concluir Phase 1 (Setup baseline).
2. Concluir Phase 2 (Foundational: serialização polimórfica e `ScriptTask`).
3. Concluir Phase 3 (User Story 1: execução sequencial).
4. **Validar MVP**: Executar uma tarefa com múltiplos comandos e verificar execução ordenada.

### Incremental Delivery
1. Baseline + Fundação concluídos.
2. User Story 1 (MVP) → Comandos sequenciais funcionais.
3. User Story 2 → Parada imediata em caso de falha adicionada com segurança operacional.
4. User Story 3 → Validações estritas de schema e melhorias de UX no menu interativo.
5. Polish → Testes globais, cenários do quickstart e bump de versão.

