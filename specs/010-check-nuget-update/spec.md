# Feature Specification: Verificação de Nova Versão no NuGet com Alerta no Console

**Feature Branch**: `010-check-nuget-update`

**Created**: 2026-11-11

**Status**: Draft

**Input**: User description: "Acrescentar uma verificação para checar a existência de uma nova versão da ferramenta no Nuget, e demonstrar no topo do console (antes de qualquer outra informação) um alerta indicando uma nova versão, solicitando ao usuário para que a atualização seja feita"

## Clarifications

### Session 2026-11-11

- Q: Como o alerta de nova versão deve ser visualmente apresentado no topo do terminal? (FR-003) → A: Painel (`Panel`) do Spectre.Console com borda arredondada amarela, indicando a versão atual, a nova versão disponível e o comando para atualizar (`dotnet tool update -g nettasklist`).
- Q: Como deve ser obtida a versão local da ferramenta para comparação com o NuGet? → A: Diretamente a partir do próprio binário executável da ferramenta, utilizando a mesma versão reportada pela opção `--version`.
- Q: A verificação deve manter um cache local ou consultar o NuGet diretamente a cada execução? → A: Consultar o NuGet diretamente a cada execução com timeout rápido (1 a 4 segundos), sem criar ou persistir arquivos de cache em disco.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Exibição de Alerta no Topo do Console Quando Existir Nova Versão (Priority: P1)

Como um desenvolvedor que utiliza o `nxtl` diariamente no terminal, desejo ser alertado logo no topo do console quando houver uma nova versão da ferramenta disponível no NuGet através de um painel destacado com borda arredondada, recebendo a instrução exata de como efetuar a atualização, para que eu mantenha meu ambiente sempre atualizado com as últimas correções e funcionalidades sem precisar pesquisar manualmente.

**Why this priority**: É a entrega central e mais valiosa da funcionalidade solicitada pelo usuário — manter o desenvolvedor ciente de novas versões e facilitar o comando de atualização.

**Independent Test**: Pode ser testado de forma isolada simulando a resposta do registro de pacotes com uma versão superior à versão local instalada (obtida do binário da aplicação) e verificando se o alerta em formato de painel é impresso no topo do console antes de qualquer outra informação (cabeçalho, lista de tarefas ou execução direta).

**Acceptance Scenarios**:

1. **Given** que a versão local do binário da ferramenta (exibida por `--version`) é `1.6.1` e existe a versão `1.7.0` publicada no NuGet, **When** o usuário executa o comando `nxtl` (modo interativo), **Then** um alerta visual em formato de painel (`Panel`) com borda arredondada amarela é exibido no topo do console antes de qualquer outra saída, informando que uma nova versão está disponível (atual: 1.6.1, disponível: 1.7.0) e solicitando a atualização com a instrução `dotnet tool update -g nettasklist`.
2. **Given** que existe uma versão mais recente disponível no NuGet, **When** o usuário executa uma tarefa diretamente via `nxtl --run <nome>`, **Then** o painel de alerta de atualização é exibido no topo do console antes do início da execução e da saída da tarefa.

---

### User Story 2 - Comportamento Silencioso Quando a Ferramenta Estiver Atualizada (Priority: P2)

Como um desenvolvedor com a ferramenta atualizada na versão mais recente, desejo que nenhuma mensagem ou alerta intrusivo sobre atualizações seja exibido no topo do console, garantindo uma experiência limpa e focada exclusivamente nas minhas tarefas.

**Why this priority**: Evita poluição visual desnecessária e garante que alertas só apareçam quando houver ação útil a ser tomada pelo usuário.

**Independent Test**: Pode ser testado executando o utilitário em um cenário onde a versão instalada do binário é igual ou superior à versão disponível no registro de pacotes, confirmando que nenhum alerta de atualização é emitido.

**Acceptance Scenarios**:

1. **Given** que a versão do binário instalada é `1.6.1` e a versão mais recente no NuGet é `1.6.1`, **When** o usuário executa o `nxtl`, **Then** nenhum alerta de versão é exibido e a tela usual do aplicativo é apresentada diretamente.
2. **Given** que a versão do binário instalada é uma versão de desenvolvimento ou prévia superior à versão estável do NuGet (ex: `1.7.0-dev` vs `1.6.1`), **When** o usuário executa o `nxtl`, **Then** nenhum alerta de atualização é exibido.

---

### User Story 3 - Resiliência e Tolerância a Falhas Sem Bloqueio do Usuário (Priority: P2)

Como um desenvolvedor trabalhando offline, em redes lentas ou com instabilidade no registro de pacotes, desejo que a ferramenta continue funcionando perfeitamente sem travar, demorar ou emitir mensagens de erro técnico de conexão, para que minha produtividade local nunca seja interrompida.

**Why this priority**: Garante estabilidade operacional e confiabilidade da ferramenta CLI, impedindo que falhas de rede impeçam o uso das tarefas locais.

**Independent Test**: Pode ser testado simulando ausência de internet, respostas HTTP com erro ou atrasos de resposta acima do tempo limite tolerado (1 a 2 segundos), confirmando que a execução das tarefas ocorre normalmente e sem lentidão perceptível.

**Acceptance Scenarios**:

1. **Given** um ambiente sem conexão com a internet (offline ou modo avião), **When** o usuário executa o `nxtl`, **Then** o sistema ignora a verificação silenciosamente sem falhar e prossegue com a execução normal do aplicativo.
2. **Given** uma conexão lenta em que a consulta ao registro ultrapassa o limite máximo de tolerância (1 a 2 segundos), **When** o usuário executa o `nxtl`, **Then** a verificação é interrompida silenciosamente e o aplicativo segue imediatamente para as tarefas do usuário sem travar o console.
3. **Given** que o registro de pacotes retorne uma resposta inesperada ou código de erro, **When** o usuário executa o `nxtl`, **Then** o erro é tratado silenciosamente sem exibir rastros de exceção ou interromper o comando.

---

### User Story 4 - Operação Leve e Sem Persistência em Disco (Priority: P3)

Como um desenvolvedor preocupado com arquivos residuais no ambiente, desejo que a verificação de versão seja totalmente sem estado (*stateless*), sem criar arquivos temporários ou registros em disco, mantendo o ambiente limpo.

**Why this priority**: Assegura simplicidade operacional, evitando problemas de permissão em diretórios locais ou arquivos residuais no sistema de arquivos do usuário.

**Independent Test**: Pode ser testado executando a ferramenta e verificando que nenhum arquivo de cache é criado no disco.

**Acceptance Scenarios**:

1. **Given** a execução do comando `nxtl`, **When** a checagem remota de versão for concluída (com sucesso ou falha), **Then** nenhum arquivo temporário ou de cache de versão é gravado no disco do usuário.

---

### Edge Cases

- **Ambiente sem conexão com a internet**: Falha de rede tratada de modo transparente; nenhum erro técnico ou rastreamento de pilha (stack trace) é exibido no console.
- **Timeout de conexão ou latência excessiva**: Limite estrito de no máximo 4 segundos (1 a 4s) para a consulta de rede. Caso o tempo expire, o fluxo prossegue imediatamente sem bloquear a inicialização.
- **Execução com parâmetros informativos simples (`--version`, `--help`)**: Comandos que solicitam apenas metadados rápidos (como `nxtl -v` ou `nxtl -h`) não devem acionar a verificação de versão nem exibir o alerta no topo, mantendo a resposta limpa e rápida para scripts e automações externas.
- **Versão instalada superior ou pré-lançamento**: Caso a versão do binário em uso seja igual ou mais recente que a versão publicada no NuGet, a verificação considera o sistema atualizado e não exibe alerta.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE consultar a versão mais recente publicada do pacote `NetTaskList` no registro oficial NuGet.
- **FR-002**: O sistema DEVE obter a versão instalada diretamente a partir do próprio binário executável da aplicação (a mesma versão reportada pela opção `--version`) e compará-la semanticamente com a versão estável mais recente obtida no NuGet.
- **FR-003**: Caso a versão do NuGet seja superior à versão local, o sistema DEVE renderizar no topo do console (antes de qualquer cabeçalho, menu interativo ou resultado de execução) um alerta visual formatado como um painel (`Panel`) do Spectre.Console com borda arredondada amarela.
- **FR-004**: O painel de alerta exibido DEVE conter a versão atualmente instalada, a nova versão disponível e a instrução clara de atualização com o comando `dotnet tool update -g nettasklist`.
- **FR-005**: Todas as mensagens do alerta DEVEM ser apresentadas no idioma Português do Brasil (`pt-BR`).
- **FR-006**: Caso a versão instalada seja igual ou superior à versão no NuGet, o sistema NÃO DEVE exibir nenhum alerta de versão.
- **FR-007**: A verificação de versão DEVE possuir um tempo limite rígido de no máximo 2 segundos de espera para a consulta remota, garantindo que o usuário nunca sofra bloqueios ou atrasos excessivos.
- **FR-008**: Qualquer falha na consulta (como ausência de internet, indisponibilidade do serviço remoto, resposta inesperada ou timeout) DEVE ser tratada de forma silenciosa e segura, sem exibir exceções ao usuário e sem impedir a execução regular das tarefas.
- **FR-009**: A verificação de versão NÃO DEVE criar nem persistir arquivos de cache em disco, realizando a checagem diretamente a cada execução.
- **FR-010**: A exibição do alerta de atualização DEVE ocorrer tanto na inicialização do menu interativo (`nxtl`) quanto na execução direta de tarefas (`nxtl --run <nome>`).
- **FR-011**: As opções informativas rápidas de versão (`--version`, `-v`) e ajuda (`--help`, `-h`) NÃO DEVEM disparar a verificação de rede nem exibir o alerta de atualização, preservando a saída limpa e instantânea para chamadas automatizadas.

### Key Entities *(include if feature involves data)*

- **Resultado da Verificação de Versão (VersionCheckResult)**: Contém o status da checagem, a versão instalada obtida do binário da ferramenta, a versão mais recente encontrada no registro remoto e se uma atualização é recomendada.
- **Alerta de Atualização (UpdateNotification)**: Componente visual de apresentação no console formatado como um painel estilizado (`Panel`) com borda arredondada amarela contendo a versão atual, a nova versão e o comando de atualização.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em 100% dos testes com versão remota mais recente, o painel de alerta de atualização é apresentado no topo absoluto do console antes de qualquer outra informação do aplicativo.
- **SC-002**: Em 100% dos cenários de falha de rede (offline, DNS inexistente, resposta HTTP inválida ou timeout), o aplicativo executa normalmente sem travar e sem apresentar erros ou exceções na saída.
- **SC-003**: O tempo máximo adicionado à inicialização da ferramenta pela verificação remota não ultrapassa 2 segundos sob nenhuma condição de rede.
- **SC-004**: A operação é totalmente stateless: 0 arquivos temporários ou de cache são criados ou persistidos no disco do usuário.
- **SC-005**: 100% dos textos e instruções exibidos no terminal estão no idioma Português do Brasil (`pt-BR`).
- **SC-006**: 100% da suíte de testes automatizados é executada com sucesso sem regressões.

## Assumptions

- O identificador do pacote no NuGet é `NetTaskList` e a ferramenta global é instalada e atualizada pelo comando `dotnet tool update -g nettasklist`.
- A versão instalada para comparação é resolvida dinamicamente a partir dos atributos de assembly do executável (idêntica ao valor exposto pela flag `--version`).
- A comparação de versões segue o padrão de versionamento semântico (SemVer), onde apenas versões estáveis lançadas oficialmente no NuGet são comparadas com a versão local.
- A consulta direta ao NuGet é executada a cada início do comando com cancelamento automático via timeout máximo de 4 segundos para manter o CLI ágil.
- Comandos dedicados exclusivamente à inspeção como `nxtl --version` e `nxtl --help` continuam emitindo exclusivamente a informação solicitada pelo usuário sem realizar chamadas de rede.
