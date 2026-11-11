# Tasks: Verificação de Nova Versão no NuGet com Alerta no Console

**Input**: Design documents from `specs/010-check-nuget-update/` (`spec.md`, `plan.md`, `data-model.md`, `contracts/`, `research.md`, `quickstart.md`)  
**Prerequisites**: `spec.md`, `plan.md`, `data-model.md`, `contracts/`, `research.md`  
**Tests**: Obrigatórios conforme o Princípio III da Constituição (Test-First & Mandatory Unit Test Coverage).  
**Organization**: As tarefas são agrupadas por user story para viabilizar implementação e testes independentes de cada fatia.

## Format: `- [ ] [ID] [P?] [Story?] Description with file path`

- **[P]**: Tarefas paralelizáveis (arquivos distintos, sem dependência mútua)
- **[Story]**: Identificador da história de usuário (`[US1]`, `[US2]`, `[US3]`, `[US4]`)
- Caminhos completos dos arquivos incluídos em todas as descrições

---

## Phase 1: Setup (Shared Infrastructure & DTOs)

**Purpose**: Criação de DTOs e configuração de serialização Native AOT para comunicação com a API do NuGet.

- [X] T001 [P] Criar o DTO `NugetPackageVersionsDto` com propriedade `Versions` (`List<string>?`, mapeada para o campo JSON `"versions"`) em `src/NetTaskList/Infrastructure/Dtos/NugetPackageVersionsDto.cs`
- [X] T002 Registrar `NugetPackageVersionsDto` no contexto de geração de código JSON `TaskJsonContext` (`[JsonSerializable(typeof(NugetPackageVersionsDto))]`) em `src/NetTaskList/Infrastructure/Serialization/TaskJsonContext.cs`
- [X] T003 [P] Criar testes unitários para validação da serialização e deserialização AOT de `NugetPackageVersionsDto` em `tests/NetTaskList.Tests/Infrastructure/NugetPackageVersionsDtoTests.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Modelos de domínio básicos (`SemanticVersion`, `VersionCheckResult`) e abstração `INugetVersionChecker` que bloqueiam todas as histórias de usuário.

**⚠️ CRITICAL**: Nenhuma tarefa das histórias de usuário pode começar antes da conclusão desta fase.

- [X] T004 [P] Criar testes unitários para parse e comparação de versões semânticas SemVer 2.0 (`Major`, `Minor`, `Patch`, `PreRelease`, `IsPreRelease`, `CompareTo`, `IsNewerThan`) em `tests/NetTaskList.Tests/Domain/SemanticVersionTests.cs`
- [X] T005 Implementar a entidade de domínio `SemanticVersion` em `src/NetTaskList/Domain/Models/SemanticVersion.cs` atendendo aos testes de T004
- [X] T006 [P] Criar testes unitários para a entidade e métodos de fábrica de `VersionCheckResult` (`NoUpdate`, `UpdateAvailable`, `Failed`) em `tests/NetTaskList.Tests/Domain/VersionCheckResultTests.cs`
- [X] T007 Implementar o modelo de domínio `VersionCheckResult` com propriedades `HasUpdate` (bool), `CurrentVersion` (string), `LatestVersion` (string?) e `IsSuccess` (bool) em `src/NetTaskList/Domain/Models/VersionCheckResult.cs`
- [X] T008 [P] Definir a interface `INugetVersionChecker` com métodos `CheckForUpdate(string currentVersion, TimeSpan timeout)` e `CheckForUpdateAsync(string currentVersion, CancellationToken cancellationToken = default)` em `src/NetTaskList/Infrastructure/Abstractions/INugetVersionChecker.cs`
- [X] T009 Criar a classe mock `MockNugetVersionChecker` implementando `INugetVersionChecker` para testes controlados da aplicação em `tests/NetTaskList.Tests/Mocks/MockNugetVersionChecker.cs`

**Checkpoint**: Infraestrutura fundamental e modelos de domínio prontos — implementação das user stories pode iniciar.

---

## Phase 3: User Story 1 - Exibição de Alerta no Topo do Console Quando Existir Nova Versão (Priority: P1) 🎯 MVP

**Goal**: Consultar o NuGet e exibir o painel de alerta (`Spectre.Console.Panel`) com borda arredondada amarela no topo do console (antes de qualquer outra informação) em modo interativo e direto (`--run`), informando versão atual, versão mais recente e comando `dotnet tool update -g nettasklist`.

**Independent Test**: Simular resposta remota com versão superior à versão local instalada e verificar se o painel de alerta é renderizado no topo da saída do console.

### Tests for User Story 1 ⚠️

- [X] T010 [P] [US1] Criar testes unitários para a classe `NugetVersionChecker` simulando resposta `200 OK` do endpoint NuGet Flat Container com versão mais recente em `tests/NetTaskList.Tests/Infrastructure/NugetVersionCheckerTests.cs`
- [X] T011 [P] [US1] Criar testes unitários para o componente `UpdateNotificationPresenter` validando o painel gerado (borda arredondada amarela, mensagens em `pt-BR`, comando de atualização) em `tests/NetTaskList.Tests/Presentation/UpdateNotificationPresenterTests.cs`

### Implementation for User Story 1

- [X] T012 [US1] Implementar a classe `NugetVersionChecker` em `src/NetTaskList/Infrastructure/NugetVersionChecker.cs` executando requisições HTTP para `https://api.nuget.org/v3-flatcontainer/nettasklist/index.json`
- [X] T013 [US1] Implementar `UpdateNotificationPresenter` em `src/NetTaskList/Presentation/UpdateNotificationPresenter.cs` renderizando o painel com `BoxBorder.Rounded`, cor amarela e textos em `pt-BR`
- [X] T014 [US1] Atualizar `Program.Run` em `src/NetTaskList/Program.cs` para injetar `INugetVersionChecker`, disparar a checagem e renderizar o alerta no topo absoluto da saída antes de listar tarefas ou executar via `--run`
- [X] T015 [US1] Adicionar testes em `tests/NetTaskList.Tests/Presentation/ProgramExecutionTests.cs` verificando a presença do painel de alerta no topo do console em modo interativo e em modo direto `--run`

**Checkpoint**: User Story 1 (MVP) funcional e testável de ponta a ponta.

---

## Phase 4: User Story 2 - Comportamento Silencioso Quando a Ferramenta Estiver Atualizada (Priority: P2)

**Goal**: Garantir que nenhum alerta seja exibido se a ferramenta já estiver na versão mais recente publicada ou em versão prévia/desenvolvimento superior.

**Independent Test**: Executar a ferramenta com versão local igual ou superior à retornada pelo NuGet e verificar que nenhum painel de alerta é emitido.

### Tests for User Story 2 ⚠️

- [X] T016 [P] [US2] Adicionar testes unitários em `tests/NetTaskList.Tests/Infrastructure/NugetVersionCheckerTests.cs` verificando que versões estáveis iguais à atual e versões prévias não disparam `HasUpdate = true`
- [X] T017 [P] [US2] Adicionar testes de integração em `tests/NetTaskList.Tests/Presentation/ProgramExecutionTests.cs` garantindo que o painel de alerta não é renderizado quando a versão instalada é a mais recente

### Implementation for User Story 2

- [X] T018 [US2] Ajustar a lógica de seleção de versão em `src/NetTaskList/Infrastructure/NugetVersionChecker.cs` para filtrar apenas versões estáveis e validar estritamente `latestVersion.IsNewerThan(currentVersion)`

**Checkpoint**: Comportamento silencioso garantido quando a ferramenta estiver atualizada.

---

## Phase 5: User Story 3 - Resiliência e Tolerância a Falhas Sem Bloqueio do Usuário (Priority: P2)

**Goal**: Assegurar resiliência total a falhas de rede, timeout de 2 segundos, modo offline e exceções de conexão, além de garantir que `--version` e `--help` não executem a checagem.

**Independent Test**: Simular queda de conexão, timeout superior a 2 segundos e chamadas informativas (`--version`, `--help`), verificando saída limpa sem travamentos ou exceções.

### Tests for User Story 3 ⚠️

- [X] T019 [P] [US3] Criar testes unitários em `tests/NetTaskList.Tests/Infrastructure/NugetVersionCheckerTests.cs` para falhas de rede (`HttpRequestException`, `TaskCanceledException`, status HTTP 404/500 e corpo JSON inválido) confirmando retorno de `VersionCheckResult.Failed`
- [X] T020 [P] [US3] Adicionar testes em `tests/NetTaskList.Tests/Presentation/ProgramExecutionTests.cs` validando que falhas de rede ou timeout não geram mensagens de erro para o usuário e prosseguem normalmente com as tarefas
- [X] T021 [P] [US3] Adicionar testes em `tests/NetTaskList.Tests/Presentation/ProgramExecutionTests.cs` garantindo que `nxtl --version` e `nxtl --help` nunca disparam a verificação do `INugetVersionChecker`

### Implementation for User Story 3

- [X] T022 [US3] Implementar tratamento de timeout rígido (máximo de 2 segundos) e captura silenciosa de todas as exceções de rede em `src/NetTaskList/Infrastructure/NugetVersionChecker.cs`
- [X] T023 [US3] Garantir em `src/NetTaskList/Program.cs` que as flags `--version` e `--help` executam de imediato sem disparar o `INugetVersionChecker`

**Checkpoint**: Operação 100% resiliente a falhas de conexão e flags rápidas preservadas.

---

## Phase 6: User Story 4 - Operação Leve e Sem Persistência em Disco (Priority: P3)

**Goal**: Garantir que a verificação de versão seja totalmente stateless e não crie nenhum arquivo temporário ou de cache em disco.

**Independent Test**: Verificar após a execução que nenhum arquivo de cache de versão foi gravado no sistema de arquivos.

### Tests for User Story 4 ⚠️

- [X] T024 [P] [US4] Criar teste em `tests/NetTaskList.Tests/Infrastructure/NugetVersionCheckerTests.cs` verificando ausência total de interações ou chamadas de gravação em disco durante a checagem

### Implementation for User Story 4

- [X] T025 [US4] Revisar `src/NetTaskList/Infrastructure/NugetVersionChecker.cs` e `src/NetTaskList/Program.cs` para assegurar arquitetura estritamente em memória e stateless

**Checkpoint**: Operação stateless validada sem efeitos colaterais no disco.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Verificação ponta a ponta dos cenários do quickstart, conformidade com a Constituição e atualização da versão no `.csproj`.

- [X] T026 Executar a validação manual dos cenários ponta a ponta documentados em `specs/010-check-nuget-update/quickstart.md`
- [X] T027 Atualizar a versão no arquivo `src/NetTaskList/NetTaskList.csproj` de `1.6.1` para `1.7.0` (incremento Minor por adição de nova funcionalidade) conforme o Princípio VI da Constituição
- [X] T028 Executar a suíte completa de testes com `dotnet test` e compilação limpa (`dotnet build`), garantindo 0 avisos, 0 falhas e 100% de testes aprovados

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências — pode iniciar de imediato.
- **Foundational (Phase 2)**: Depende da conclusão de Setup (Phase 1). **BLOQUEIA** todas as histórias de usuário.
- **User Story 1 (Phase 3)**: Depende de Foundational (Phase 2). Entrega o MVP da funcionalidade.
- **User Story 2 (Phase 4)**: Depende de US1 (Phase 3). Refina a lógica de filtragem para versões atualizadas.
- **User Story 3 (Phase 5)**: Depende de US1 (Phase 3). Adiciona proteção de timeout e resiliência offline.
- **User Story 4 (Phase 6)**: Depende de US1 e US3. Garante natureza stateless.
- **Polish (Phase 7)**: Depende da conclusão de todas as histórias de usuário.

### Within Each User Story

- Testes unitários DEVEM ser escritos e falhar antes da implementação concreta correspondente (TDD / Princípio III).
- Modelos e contratos antes dos serviços de infraestrutura.
- Serviços de infraestrutura antes da integração na camada de apresentação (`Program.cs`).

### Parallel Opportunities

- Em Phase 1: `T001` e `T003` podem ser desenvolvidos em paralelo.
- Em Phase 2: `T004`, `T006` e `T008` podem ser desenvolvidos em paralelo.
- Em Phase 3: `T010` e `T011` podem ser desenvolvidos em paralelo.
- Em Phase 5: `T019`, `T020` e `T021` podem ser desenvolvidos em paralelo.

---

## Parallel Example: User Story 1

```bash
# Execução paralela de testes de US1:
Task: "T010 [P] [US1] Criar testes unitários para a classe NugetVersionChecker em tests/NetTaskList.Tests/Infrastructure/NugetVersionCheckerTests.cs"
Task: "T011 [P] [US1] Criar testes unitários para o componente UpdateNotificationPresenter em tests/NetTaskList.Tests/Presentation/UpdateNotificationPresenterTests.cs"

# Execução paralela de modelos/componentes de US1:
Task: "T012 [US1] Implementar a classe NugetVersionChecker em src/NetTaskList/Infrastructure/NugetVersionChecker.cs"
Task: "T013 [US1] Implementar UpdateNotificationPresenter em src/NetTaskList/Presentation/UpdateNotificationPresenter.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Concluir Phase 1 (Setup) e Phase 2 (Foundational).
2. Implementar Phase 3 (User Story 1).
3. **VALIDAR**: Testar a exibição do painel de alerta no topo do console.
4. O MVP já entrega valor imediato alertando sobre novas versões.

### Incremental Delivery

1. Setup + Foundational → Base tipada e testável pronta.
2. US1 → Alerta funcional no topo do console (MVP!).
3. US2 → Supressão de alertas quando atualizado ou em versão de desenvolvimento.
4. US3 → Tolerância a falhas offline, timeout de 2s e bypass em `--version`/`--help`.
5. US4 → Confirmação da operação stateless sem lixo em disco.
6. Polish → Testes 100% verdes e atualização da versão no `.csproj` para `1.7.0`.
