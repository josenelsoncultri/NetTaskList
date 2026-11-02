# Feature Specification: Melhoria do Output de Execução e Navegação de Tarefas

**Feature Branch**: `004-improve-execution-output`

**Created**: 2026-11-01

**Status**: Draft

**Input**: User description: "Melhorar output do comando nxtl na execução: - Remover a tabela de tarefas disponíveis e o texto "Tarefas Disponíveis (nxtl)" - Acrescentar o texto "nxtl versão <código da versão atual>" no topo da listagem, cor verde - Na listagem de tarefas, deixar visível apenas 8 tarefas - Transformar o texto "Selecionar uma tarefa para executar" em cor ciano"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Visualização Compacta e Interativa da Listagem de Tarefas (Priority: P1)

Como um desenvolvedor executando o comando `nxtl` no terminal, desejo visualizar um menu de seleção interativo direto e limpo, sem tabelas redundantes ocupando espaço da tela, com limite de 8 itens visíveis por vez e título em cor ciano, para que eu selecione e execute tarefas com agilidade e foco visual.

**Why this priority**: A usabilidade do fluxo padrão do CLI é o principal ponto de contato do usuário. A exibição anterior de uma tabela estática seguida imediatamente por um menu de seleção duplicava as informações e poluia a rolagem do terminal.

**Independent Test**: Pode ser testado executando `nxtl` em um terminal interativo com tarefas configuradas, verificando a ausência da tabela estática de tarefas, a presença do prompt interativo estilizado em ciano ("Selecionar uma tarefa para executar") e a exibição de no máximo 8 opções visíveis simultaneamente.

**Acceptance Scenarios**:

1. **Given** um arquivo de configuração com tarefas válidas, **When** o usuário executa `nxtl` sem argumentos no terminal interativo, **Then** o sistema NÃO exibe a tabela estática de tarefas nem o texto "Tarefas Disponíveis (nxtl)".
2. **Given** um arquivo de configuração com mais de 8 tarefas cadastradas, **When** o menu de seleção interativa é renderizado, **Then** apenas 8 tarefas permanecem visíveis simultaneamente na janela de rolagem, permitindo navegar pelas demais através das setas direcionais.
3. **Given** a inicialização do menu de seleção interativa, **When** o texto de orientação for apresentado ao usuário, **Then** a mensagem "Selecionar uma tarefa para executar" é exibida na cor ciano.

---

### User Story 2 - Identificação Imediata da Versão da Ferramenta no Cabeçalho (Priority: P2)

Como um desenvolvedor executando o `nxtl`, desejo ver no topo da listagem a versão atual da ferramenta destacada em verde, para que eu possa confirmar instantaneamente qual versão do CLI está em execução no meu ambiente de trabalho sem precisar rodar comandos extras.

**Why this priority**: Fornece visibilidade clara sobre o release em execução, reforçando a identidade da ferramenta e facilitando o diagnóstico de comportamento e suporte entre diferentes ambientes.

**Independent Test**: Pode ser testado executando `nxtl` com configuração válida e verificando se a primeira linha renderizada no topo da listagem exibe o formato "nxtl versão <versão>" na cor verde com o número correto de versão.

**Acceptance Scenarios**:

1. **Given** o utilitário `nxtl` instalado e configurado, **When** o comando de execução padrão for disparado, **Then** a primeira linha no topo da listagem exibe o texto "nxtl versão <código da versão atual>" formatado na cor verde.
2. **Given** a versão da ferramenta definida no projeto (ex: `1.0.0`), **When** o cabeçalho for renderizado, **Then** o código da versão exibido corresponde dinamicamente à versão do binário em execução.

---

### Edge Cases

- **Configuração com 8 ou menos tarefas**: Quando o total de tarefas for menor ou igual a 8, todas as opções são apresentadas diretamente no viewport sem a necessidade de rolagem, mantendo a experiência fluida.
- **Configuração com dezenas de tarefas**: O viewport permanece estritamente restrito a 8 itens visíveis por vez, com indicação de navegação superior/inferior para acesso às opções adicionais.
- **Redirecionamento de saída ou terminal não-interativo**: Quando o terminal não suportar interação ou a entrada for redirecionada, a tabela não deve ser renderizada e a mensagem de aviso adequada deve ser exibida, mantendo o cabeçalho de versão.
- **Versão contendo sufixos ou metadados de build**: O cabeçalho deve formatar adequadamente a versão sem corromper a estilização visual em verde.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema NÃO DEVE exibir a tabela estática de tarefas disponíveis nem o título "Tarefas Disponíveis (nxtl)" no fluxo de execução do CLI.
- **FR-002**: O sistema DEVE exibir o texto "nxtl versão <código da versão atual>" no topo da listagem de tarefas no terminal.
- **FR-003**: O texto de versão no topo da listagem DEVE ser estilizado na cor verde.
- **FR-004**: O código da versão exibido DEVE ser obtido dinamicamente a partir dos metadados de versão da aplicação em execução.
- **FR-005**: O seletor interativo de tarefas DEVE limitar o número de tarefas visíveis simultaneamente a no máximo 8 itens (tamanho de página/viewport igual a 8).
- **FR-006**: O texto orientativo de seleção ("Selecionar uma tarefa para executar") DEVE ser exibido na cor ciano.
- **FR-007**: O sistema DEVE manter o suporte à navegação interativa (setas direcionais para cima e para baixo) para tarefas excedentes aos 8 itens visíveis.
- **FR-008**: Todas as mensagens e rótulos de interação apresentados no terminal DEVEM permanecer no idioma Português do Brasil (`pt-BR`).

### Key Entities *(include if feature involves data)*

- **Cabeçalho da Listagem (Listing Header)**: Elemento visual de abertura da CLI que exibe a marca da ferramenta e o número da versão atual formatado em verde.
- **Viewport do Seletor Interativo (Task Selector Viewport)**: Componente de apresentação responsável pelo menu de escolhas, configurado com paginação de 8 itens visíveis e título em ciano.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: O terminal de saída durante a execução do comando padrão não apresenta nenhuma ocorrência do texto "Tarefas Disponíveis (nxtl)" nem bordas de tabela estática.
- **SC-002**: A linha inicial da saída exibe exatamente "nxtl versão <versão>" estilizada na cor verde.
- **SC-003**: Em listas contendo mais de 8 tarefas, exatamente 8 itens são exibidos simultaneamente no viewport interativo.
- **SC-004**: O título do seletor interativo exibe "Selecionar uma tarefa para executar" com formatação ciano aplicada.
- **SC-005**: 100% dos testes automatizados de apresentação e execução passam sem erros.

## Assumptions

- A tabela estática anterior era redundante em relação ao menu de seleção interativo e sua remoção melhora a ergonomia e clareza no terminal.
- O texto do prompt interativo é "Selecionar uma tarefa para executar" na cor ciano, mantendo a consistência do idioma `pt-BR`.
- O tamanho de página de 8 itens proporciona equilíbrio visual ideal para telas de terminal menores ou divididas.
