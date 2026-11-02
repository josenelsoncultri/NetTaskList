# Feature Specification: Adição do Comando de Atualização da Ferramenta ao nxtl.json Padrão

**Feature Branch**: `006-add-update-tool-task`

**Created**: 2026-11-01

**Status**: Draft

**Input**: User description: "Acrescentar ao nxtl.json padrão o seguinte comando: dotnet tool update -g nexttasklist com o name update-nexttasklist, type tool e description 'Atualiza a ferramenta nxtl de modo global'"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Geração de Arquivo Modelo com Tarefa de Atualização da Ferramenta (Priority: P1)

Como um desenvolvedor inicializando o `nxtl` em um projeto (`nxtl --init` ou `nxtl -i`), desejo que o arquivo `nxtl.json` gerado por padrão já contenha uma tarefa pré-configurada para atualizar a ferramenta globalmente, para que eu e os membros da minha equipe possamos manter o CLI atualizado com facilidade diretamente pelo menu interativo de tarefas.

**Why this priority**: Torna o ciclo de atualização do próprio CLI conveniente e visível para qualquer projeto que adote o `nxtl`, reforçando a autonomia dos desenvolvedores.

**Independent Test**: Executar a geração de template padrão e inspecionar o JSON resultante, confirmando que a lista de `scripts` contém a nova tarefa com o comando de atualização, tipo `tool` e descrição especificada, mantendo a integridade de leitura e validação pelo `nxtl`.

**Acceptance Scenarios**:

1. **Given** a geração do arquivo padrão `nxtl.json` via comando `nxtl --init` (ou pelo gerador de template), **When** o arquivo gerado for analisado, **Then** ele contém uma tarefa com o comando de atualização global `dotnet tool update -g nettasklist`, nome `update-nettasklist`, tipo `tool` e descrição `"Atualiza a ferramenta nxtl de modo global"`.
2. **Given** o arquivo `nxtl.json` gerado com a nova tarefa, **When** o usuário executar o `nxtl` interativamente ou via `--run`, **Then** a tarefa de atualização é listada e selecionável normalmente como uma ferramenta de linha de comando.

---

### User Story 2 - Padronização nos Exemplos e Configurações de Referência (Priority: P2)

Como um usuário ou mantenedor do projeto, desejo que a tarefa de atualização da ferramenta esteja alinhada nos exemplos da documentação (`README.md`) e no arquivo de configuração `nxtl.json` do próprio repositório, mantendo consistência entre os artefatos de referência do projeto.

**Why this priority**: Assegura que novos usuários e desenvolvedores do repositório encontrem os mesmos comandos práticos no ambiente de desenvolvimento e nos exemplos do guia rápido.

**Independent Test**: Verificar se o `nxtl.json` da raiz e a documentação em `README.md` refletem a inclusão da tarefa de atualização da ferramenta.

**Acceptance Scenarios**:

1. **Given** o arquivo `nxtl.json` na raiz do projeto, **When** inspecionado, **Then** a tarefa de atualização global da ferramenta está disponível entre as tarefas configuradas.
2. **Given** a seção de estrutura do `nxtl.json` no `README.md`, **When** consultada pelo usuário, **Then** a tarefa de atualização está documentada como exemplo padrão.

---

### Edge Cases

- **Execução em ambiente sem .NET SDK ou com permissões restritas**: O comando `dotnet tool update -g` depende do ambiente do usuário; a ferramenta `nxtl` deve propagar a saída e o código de erro adequadamente caso a atualização falhe no sistema operacional.
- **Configuração já existente com tarefa de mesmo nome**: O gerador padrão só é invocado na inicialização de novos arquivos (`--init`), não sobrescrevendo arquivos preexistentes.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O gerador de configuração modelo (`nxtl --init`) DEVE incluir nos scripts padrões uma tarefa configurada com a descrição `"Atualiza a ferramenta nxtl de modo global"` e tipo `"tool"`.
- **FR-002**: O comando executado pela tarefa DEVE ser `dotnet tool update -g nettasklist`.
- **FR-003**: O nome identificador da tarefa DEVE ser `update-nettasklist`.
- **FR-004**: O arquivo `nxtl.json` gerado pelo template DEVE ser sintaticamente e semanticamente válido perante as regras do validador do `nxtl`.
- **FR-005**: A nova tarefa DEVE coexistir com as tarefas existentes de exemplo (`build`, `test`, `seed`) no template padrão.

### Key Entities *(include if feature involves data)*

- **Tarefa de Atualização (ScriptTask)**: Entidade de script com `name`, `command`, `type` igual a `"tool"` e `description` orientativa para atualização do utilitário no ambiente do usuário.
- **Modelo de Configuração Inicial**: Lista expandida de tarefas padrões que inclui operações de compilação, teste, script e agora manutenção/atualização da própria ferramenta.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% dos novos arquivos criados pelo comando `nxtl --init` possuem a tarefa de atualização global inclusa.
- **SC-002**: A tarefa de atualização é validada com sucesso pelo leitor de configuração sem qualquer erro semântico de nome duplicado ou formato inválido.
- **SC-003**: 100% dos testes automatizados continuam passando sem regressões (`dotnet test`).

## Assumptions

- O usuário possui o utilitário .NET CLI (`dotnet`) instalado e acessível no PATH do sistema operacional para que o comando possa ser executado quando selecionado.
- A ferramenta `nxtl` utiliza o executor de comandos padrão de ferramentas (`ToolTaskExecutor`) para despachar o comando para o terminal.
