---
description: "Task list for feature implementation: Remoção da Linha de Schema do nxtl.json de Configuração"
---

# Tasks: Remoção da Linha de Schema do nxtl.json de Configuração

**Input**: Design documents from `/specs/005-remove-schema-line/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Tarefas executáveis em paralelo (arquivos distintos, sem dependência mútua)
- **[Story]**: Identificador da história de usuário (`[US1]`, `[US2]`)
- Caminhos absolutos/relativos exatos especificados para cada tarefa

---

## Phase 1: Setup (Ambiente e Pré-condições)

**Purpose**: Verificação do ambiente de compilação e linha de base dos testes automatizados

- [X] T001 Verify existing test suite baseline in tests/NetTaskList.Tests/ via dotnet test --no-restore

---

## Phase 2: Foundational (Pré-requisitos Compartilhados)

**Purpose**: Garantir que as estruturas compartilhadas suportem a ausência e a presença do schema sem conflitos

- [X] T002 Verify ConfigurationDto in src/NetTaskList/Infrastructure/Dtos/ConfigurationDto.cs preserves optional Schema property for backward compatibility

**Checkpoint**: Pré-requisitos estruturais validados. O desenvolvimento das histórias de usuário pode prosseguir.

---

## Phase 3: User Story 1 - Geração de Arquivo de Configuração Limpo sem Linha de Schema (Priority: P1) 🎯 MVP

**Goal**: Fazer com que o gerador de template `TemplateConfigurationGenerator.GenerateDefaultJson()` produza o arquivo `nxtl.json` padrão sem a linha indicativa de `"$schema"`.

**Independent Test**: Executar o teste unitário de geração de template para comprovar que `"$schema"` não está presente e que o JSON gerado é perfeitamente interpretável e válido.

### Tests for User Story 1 ⚠️

- [X] T003 [P] [US1] Update unit test in tests/NetTaskList.Tests/Infrastructure/TemplateConfigurationGeneratorTests.cs to assert that generated template does not contain $schema or nxtl-v1.schema.json

### Implementation for User Story 1

- [X] T004 [US1] Remove $schema property line from TemplateConfigurationGenerator.GenerateDefaultJson() in src/NetTaskList/Infrastructure/TemplateConfigurationGenerator.cs
- [X] T005 [US1] Run unit tests in tests/NetTaskList.Tests/Infrastructure/TemplateConfigurationGeneratorTests.cs to confirm the new template generation passes

**Checkpoint**: User Story 1 concluída e testada de forma independente. O comando `nxtl --init` agora gera arquivos limpos.

---

## Phase 4: User Story 2 - Atualização da Configuração do Projeto e Compatibilidade Retroativa (Priority: P2)

**Goal**: Limpar o arquivo `nxtl.json` do próprio repositório, atualizar a documentação no `README.md` e testar explicitamente a compatibilidade retroativa para arquivos com `"$schema"`.

**Independent Test**: O arquivo `nxtl.json` na raiz não contém mais `"$schema"`, testes unitários de `JsonConfigurationReader` validam arquivos legados com `"$schema"` com sucesso e a suíte completa passa.

### Tests for User Story 2 ⚠️

- [X] T006 [P] [US2] Add unit test in tests/NetTaskList.Tests/Infrastructure/JsonConfigurationReaderTests.cs asserting successful parsing of legacy configurations containing $schema

### Implementation for User Story 2

- [X] T007 [P] [US2] Remove $schema property line from root configuration file in nxtl.json
- [X] T008 [P] [US2] Update nxtl.json code snippets and schema description in README.md to remove $schema line
- [X] T009 [US2] Execute full test suite via dotnet test --no-restore in tests/NetTaskList.Tests/ to verify zero regressions

**Checkpoint**: User Story 2 concluída e validada. A configuração do repositório, documentação e retrocompatibilidade estão consistentes.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Validação final de ponta a ponta e governança da versão segundo a Constituição

- [X] T010 Execute quickstart validation scenarios in specs/005-remove-schema-line/quickstart.md to verify end-to-end functionality
- [X] T011 Evaluate version bump policy in src/NetTaskList/NetTaskList.csproj per Constitution Principle VI

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências externas — inicia imediatamente.
- **Foundational (Phase 2)**: Depende da verificação da linha de base de testes (Phase 1).
- **User Story 1 (Phase 3)**: Depende da Phase 2. Entrega o MVP da funcionalidade.
- **User Story 2 (Phase 4)**: Depende da Phase 3 (pode ser refinada em paralelo na documentação e retrocompatibilidade).
- **Polish (Phase 5)**: Depende da conclusão de todas as histórias de usuário.

### Within Each User Story

- Testes unitários (T003, T006) são ajustados/adicionados antes ou em conjunto com a alteração do código de produção.
- Alteração no gerador de template (T004) precede a verificação de aprovação (T005).
- Atualização do `nxtl.json` raiz (T007) e `README.md` (T008) executadas após validação dos modelos.

### Parallel Opportunities

- **Fase 3 (US1)**: T003 pode ser preparado em paralelo com a análise de T004.
- **Fase 4 (US2)**: T006 (`JsonConfigurationReaderTests.cs`), T007 (`nxtl.json`) e T008 (`README.md`) operam em arquivos totalmente independentes e podem ser executadas em paralelo (`[P]`).

---

## Parallel Example: User Story 2

```bash
# Execução paralela de tarefas de arquivos independentes:
Task: "Add unit test in tests/NetTaskList.Tests/Infrastructure/JsonConfigurationReaderTests.cs asserting successful parsing of legacy configurations containing $schema"
Task: "Remove $schema property line from root configuration file in nxtl.json"
Task: "Update nxtl.json code snippets and schema description in README.md to remove $schema line"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Executar Phase 1 (Baseline) e Phase 2 (Foundational).
2. Executar Phase 3 (User Story 1): atualizar teste de template e remover a linha de schema de `TemplateConfigurationGenerator.cs`.
3. Validar US1 independentemente através dos testes unitários de infraestrutura.
4. *Resultado*: Novos arquivos `nxtl.json` são gerados sem a linha de schema.

### Incremental Delivery (User Story 2 & Polish)

1. Adicionar o teste de retrocompatibilidade em `JsonConfigurationReaderTests.cs`.
2. Remover a linha de schema do `nxtl.json` na raiz do projeto.
3. Atualizar os exemplos do `README.md`.
4. Rodar a suíte completa de testes (`dotnet test`).
5. Executar os cenários de `quickstart.md` e avaliar o bump de versão no `.csproj` conforme a Constituição.
