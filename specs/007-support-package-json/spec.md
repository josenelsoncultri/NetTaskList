# Feature Specification: Suporte ao Arquivo package.json (package.json Support)

**Feature Branch**: `007-support-package-json`

**Created**: 2026-11-03

**Status**: Draft

**Input**: User description: "Preparar a ferramenta para ler também o arquivo package.json no projeto. Caso exista tanto o nxtl.json e o package.json, o nxtl.json deve prevalecer e o package.json será ignorado"

## Clarifications

### Session 2026-11-03

- Q: Como o `nxtl` deve se comportar em relação a scripts de ciclo de vida do npm (como `pre<tarefa>` e `post<tarefa>`) presentes na seção `"scripts"` do `package.json`? → A: Executar automaticamente os scripts `pre<tarefa>` antes e `post<tarefa>` depois, se existirem no `package.json`, funcionando como um substituto fiel do `ntl` no ecossistema Node.js.
- Q: Como as tarefas de ciclo de vida (`pre*` e `post*`) originadas do `package.json` devem ser apresentadas no menu interativo de seleção de tarefas? → A: Ocultar automaticamente scripts com prefixo `pre` ou `post` do menu interativo quando existir o comando principal correspondente (ex: ocultar `pretest` e `posttest` se `test` existir), exibindo apenas os comandos principais conforme o padrão do `ntl`.
- Q: Como o comando `nxtl --init` deve se comportar quando executado em um projeto que já possui um arquivo `package.json`? → A: Gerar o `nxtl.json` com o template padrão de exemplo, sem alterar ou ler o `package.json`, mantendo a inicialização simples e desacoplada.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Carregar e Executar Tarefas a Partir de package.json com Ciclo de Vida npm (Priority: P1)

Como uma pessoa desenvolvedora trabalhando em um projeto que já possui um arquivo `package.json` com scripts configurados (e nenhum `nxtl.json`), quero executar o comando `nxtl` no diretório e ter todas as automações de `scripts` listadas e prontas para execução (tanto de forma interativa quanto direta), executando automaticamente os scripts de ciclo de vida (`pre` e `post`) caso definidos e mantendo o menu interativo limpo (ocultando hooks que pertençam a comandos principais), para que eu possa utilizar o `nxtl` diretamente em substituição à ferramenta `ntl` do ecossistema Node.js.

**Why this priority**: Esta é a capacidade central da nova funcionalidade. Permitir que projetos existentes que utilizam `package.json` sejam compatíveis imediatamente, respeitem o ciclo de vida de scripts e mantenham o menu interativo limpo entrega paridade com a ferramenta `ntl`.

**Independent Test**: Pode ser testado de forma isolada em um diretório contendo apenas `package.json` com scripts `"prebuild": "echo pre"`, `"build": "echo main"` e `"postbuild": "echo post"`. Ao abrir o menu interativo do `nxtl`, apenas `"build"` deve ser exibido; ao selecioná-lo (ou executar `nxtl --run build`), a ferramenta deve executar ordenadamente `"prebuild"`, `"build"` e `"postbuild"`.

**Acceptance Scenarios**:

1. **Given** um diretório de projeto contendo apenas um arquivo `package.json` válido com tarefas definidas na seção `"scripts"` (ex: `"start": "node index.js"`), **When** o usuário executa `nxtl` sem argumentos adicionais, **Then** a ferramenta detecta o arquivo `package.json`, carrega as tarefas e apresenta a lista interativa de comandos no terminal.
2. **Given** um diretório de projeto contendo apenas `package.json` com tarefas `"build"` e `"test"`, **When** o usuário executa `nxtl --run build`, **Then** a ferramenta localiza e executa diretamente o comando associado a `"build"` no terminal sem exibir seleção interativa.
3. **Given** um arquivo `package.json` contendo scripts com caracteres especiais ou argumentos (ex: `"test:watch": "vitest --watch"`), **When** a ferramenta carrega as tarefas, **Then** o nome completo da tarefa e sua linha de comando são preservados fielmente para execução.
4. **Given** um arquivo `package.json` contendo scripts `"prebuild"`, `"build"` e `"postbuild"`, **When** o usuário dispara a execução da tarefa `"build"`, **Then** a ferramenta executa sequencialmente `"prebuild"`, depois `"build"` e finalmente `"postbuild"`. Caso `"prebuild"` falhe (código de retorno diferente de zero), a execução é abortada imediatamente sem executar `"build"` nem `"postbuild"`.
5. **Given** um arquivo `package.json` contendo `"prebuild"`, `"build"`, `"postbuild"` e `"prepare"`, **When** o usuário executa `nxtl` para seleção interativa, **Then** o menu exibe `"build"` e `"prepare"`, ocultando `"prebuild"` e `"postbuild"` por serem hooks vinculados ao comando principal `"build"`.

---

### User Story 2 - Prevalência de nxtl.json sobre package.json (Priority: P2)

Como uma pessoa desenvolvedora ou mantenedora de um projeto híbrido que possui tanto `nxtl.json` quanto `package.json` no mesmo diretório, quero que o processador utilize exclusivamente as definições contidas em `nxtl.json` e ignore totalmente o `package.json`, para que configurações ricas e personalizadas do `nxtl` tenham precedência absoluta sem causar ambiguidades ou conflitos de tarefas.

**Why this priority**: Esta é a regra de desempate e precedência mandatória especificada pelo usuário. Sem ela, projetos com ambos os arquivos poderiam mesclar ou sobrescrever comandos de forma inesperada.

**Independent Test**: Pode ser testado criando um diretório contendo simultaneamente `nxtl.json` (com a tarefa `"build": "dotnet build"`) e `package.json` (com a tarefa `"build": "npm run build"` e `"extra": "echo ok"`). Ao executar `nxtl`, a saída deve exibir apenas as tarefas de `nxtl.json`, comprovando que `package.json` foi completamente ignorado.

**Acceptance Scenarios**:

1. **Given** um diretório contendo simultaneamente um arquivo `nxtl.json` e um arquivo `package.json`, **When** o comando `nxtl` é executado, **Then** o sistema carrega exclusivamente as tarefas definidas em `nxtl.json` e desconsidera integralmente o conteúdo de `package.json`.
2. **Given** um diretório com ambos os arquivos onde `nxtl.json` possui um erro de sintaxe ou de validação, **When** o comando `nxtl` é executado, **Then** o sistema reporta o erro presente em `nxtl.json` e não tenta realizar fallback silencioso para `package.json`.

---

### User Story 3 - Resolução de Arquivo Explícito e Ausência de Configuração (Priority: P3)

Como uma pessoa desenvolvedora, quero receber orientações claras em português caso nenhum arquivo de configuração (`nxtl.json` ou `package.json`) seja localizado, ou poder apontar manualmente um arquivo de configuração específico via argumento de linha de comando, para que eu tenha clareza diagnóstica e controle total sobre a origem dos comandos.

**Why this priority**: Essencial para a experiência do usuário (UX) em novos diretórios ou cenários de automação em que o arquivo de configuração está em caminho não padrão.

**Independent Test**:
- Teste A: Executar `nxtl` em um diretório vazio e validar que a mensagem de erro informa a ausência tanto de `nxtl.json` quanto de `package.json` com orientações em português (`pt-BR`).
- Teste B: Executar `nxtl --config ./custom-scripts.json` em um diretório que possua `package.json` e validar que o arquivo explícito foi o único carregado.

**Acceptance Scenarios**:

1. **Given** um diretório sem nenhum arquivo `nxtl.json` e sem `package.json`, **When** o usuário executa `nxtl`, **Then** o sistema exibe mensagem de erro clara em português indicando que nenhum arquivo de configuração (`nxtl.json` ou `package.json`) foi encontrado e orienta sobre o comando `--init`.
2. **Given** um diretório contendo `package.json`, **When** o usuário especifica explicitamente outro arquivo via `--config <caminho>`, **Then** o sistema lê diretamente o arquivo especificado sem realizar a descoberta automática de `package.json`.

---

### User Story 4 - Resiliência e Validação de Formato do package.json (Priority: P4)

Como uma pessoa desenvolvedora executando `nxtl` em um repositório onde `package.json` não contém scripts ou possui sintaxe corrompida, quero receber mensagens amigáveis em português sem que a ferramenta encerre abruptamente, para que eu possa diagnosticar o problema de forma rápida.

**Why this priority**: Garante robustez contra arquivos inválidos, JSON corrompido ou projetos onde `package.json` é utilizado apenas para metadados/dependências sem a seção `scripts`.

**Independent Test**: Pode ser testado fornecendo um `package.json` sem a seção `"scripts"` (ou com `"scripts": {}`), e um `package.json` com JSON corrompido, verificando as mensagens informativas em português.

**Acceptance Scenarios**:

1. **Given** um projeto com apenas `package.json` que não possui a seção `"scripts"` ou onde `"scripts"` está vazio, **When** o usuário executa `nxtl`, **Then** o sistema avisa amigavelmente que nenhuma tarefa foi encontrada no arquivo de configuração.
2. **Given** um projeto com apenas `package.json` cujo conteúdo JSON é inválido, **When** o usuário executa `nxtl`, **Then** o sistema exibe mensagem de erro diagnóstica clara em português indicando a falha de formato no arquivo `package.json`.
3. **Given** um `package.json` contendo entradas na seção `"scripts"` que não são comandos válidos (ex: valores nulos ou vazios), **When** a ferramenta valida o arquivo, **Then** o sistema rejeita as entradas malformadas com aviso compreensível em português.

---

### Edge Cases

- **package.json sem seção "scripts"**: Se o arquivo existir mas não contiver o campo `"scripts"`, a ferramenta deve tratar como uma configuração sem tarefas cadastradas, exibindo mensagem amigável ao usuário sem gerar erro fatal.
- **Falha em script de ciclo de vida (`pre<tarefa>`)**: Se um script `pre<tarefa>` retornar código de encerramento diferente de zero, o `nxtl` deve interromper a cadeia imediatamente, propagando o erro e não executando a tarefa principal nem o `post<tarefa>`.
- **Scripts com prefixo pre/post sem comando principal associado**: Scripts como `"prepare"` ou `"precommit"` que não possuem um comando principal correspondente (ex: não existe `"commit"`) permanecem visíveis e selecionáveis normalmente no menu interativo.
- **Execução direta de hooks ocultos via linha de comando**: Caso o usuário execute diretamente `nxtl --run prebuild`, a ferramenta deve permitir a execução do comando específico de forma avulsa.
- **Execução do nxtl --init com package.json existente**: Se o comando `nxtl --init` for executado em um diretório que já contenha um `package.json`, o arquivo nativo `nxtl.json` será criado com o modelo padrão de exemplo sem alterar ou ler o `package.json`. A partir desse momento, como ambos os arquivos passam a existir, o novo `nxtl.json` prevalecerá.
- **package.json com sintaxe JSON corrompida**: Se `nxtl.json` não existir e `package.json` tiver erro de sintaxe, o sistema deve reportar erro de formatação específico referenciando o arquivo `package.json`.
- **Precedência e erros no nxtl.json**: Quando ambos os arquivos existirem e o `nxtl.json` contiver erros, o erro de `nxtl.json` deve ser exibido. Não deve haver fallback para `package.json` para evitar execuções de comandos indesejados.
- **Diferenças de maiúsculas e minúsculas no nome dos arquivos**: Em sistemas operacionais com sistemas de arquivos sensíveis a maiúsculas (ex: Linux), os nomes padrão procurados devem ser exatamente `nxtl.json` e `package.json`.
- **Scripts em package.json com valores não textuais**: Caso uma propriedade de `"scripts"` tenha um valor que não seja uma cadeia de caracteres de comando (ex: número, objeto ou booleano), o validador deve emitir mensagem de erro descritiva em português.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE verificar a existência de `nxtl.json` no diretório atual como arquivo de configuração padrão primário.
- **FR-002**: O sistema DEVE verificar a existência de `package.json` no diretório atual como arquivo de configuração secundário quando `nxtl.json` não for encontrado.
- **FR-003**: Caso ambos os arquivos (`nxtl.json` e `package.json`) existam no mesmo diretório, o sistema DEVE priorizar `nxtl.json`, ignorando integralmente o `package.json`.
- **FR-004**: O sistema DEVE extrair as tarefas a partir da seção `"scripts"` do arquivo `package.json`, mapeando o nome da propriedade para o nome da tarefa e seu valor para a linha de comando a ser executada.
- **FR-005**: As tarefas originadas do `package.json` DEVEM ser classificadas com o tipo padrão de execução de ferramentas do sistema.
- **FR-006**: O sistema DEVE permitir a execução das tarefas lidas do `package.json` tanto pelo menu de seleção interativo quanto diretamente pelo argumento `--run` (ou `-r`).
- **FR-007**: Quando o usuário especificar explicitamente o arquivo de configuração por meio do argumento `--config` (ou `-c`), o sistema DEVE carregar o arquivo informado e não aplicar a ordem de busca automática padrão.
- **FR-008**: Se nem `nxtl.json` nem `package.json` forem encontrados no diretório atual (e nenhum caminho explícito tiver sido fornecido), o sistema DEVE apresentar uma mensagem de erro em português (`pt-BR`) informando que nenhum dos arquivos foi localizado e sugerindo a inicialização.
- **FR-009**: O sistema DEVE validar a sintaxe e a consistência das tarefas contidas em `package.json`, reportando quaisquer erros de formato através de mensagens claras em português (`pt-BR`).
- **FR-010**: Se o arquivo `package.json` for selecionado mas não contiver a seção `"scripts"` ou estiver com a lista vazia, o sistema DEVE notificar o usuário amigavelmente de que nenhuma tarefa foi encontrada.
- **FR-011**: Ao executar uma tarefa selecionada a partir de `package.json`, o sistema DEVE identificar e executar automaticamente o script `pre<nome>` (antes da tarefa principal) e o script `post<nome>` (após a tarefa principal com sucesso), se tais scripts existirem no `package.json`. Caso algum script da cadeia falhe (código de retorno diferente de zero), o sistema DEVE abortar os passos subsequentes imediatamente.
- **FR-012**: No menu interativo de seleção de tarefas originadas do `package.json`, o sistema DEVE ocultar os scripts que comecem com `pre` ou `post` quando existir o comando principal correspondente (ex: ocultar `pretest` se `test` existir), exibindo apenas os scripts principais e aqueles que não possuírem comando correspondente (ex: `prepare`).
- **FR-013**: O comando `nxtl --init` DEVE gerar o arquivo inicial `nxtl.json` com o modelo padrão do sistema, sem ler nem migrar dados de um `package.json` eventualmente existente no mesmo diretório.

### Key Entities *(include if feature involves data)*

- **Origem de Configuração (Configuration Source)**: Representa a fonte do arquivo de onde as tarefas foram descobertas e carregadas (`nxtl.json`, `package.json` ou arquivo explícito via parâmetro), definindo a precedência adotada.
- **Tarefa de Script (Script Task)**: Representa uma unidade executável de automação composta por nome identificador, linha de comando do shell, tipo de execução (ferramenta padrão) e descrição opcional.
- **Coleção de Tarefas (Task Configuration)**: O conjunto consolidado de tarefas válidas carregadas da origem de configuração ativa, prontas para exibição e despacho de execução.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em 100% dos projetos que contenham apenas `package.json` com scripts válidos, o usuário consegue listar e executar os comandos sem necessidade de criar arquivo de configuração adicional.
- **SC-002**: Em 100% dos cenários onde coexistem `nxtl.json` e `package.json`, a ferramenta carrega apenas as tarefas de `nxtl.json`, garantindo zero vazamento ou sobreposição de comandos de `package.json`.
- **SC-003**: O tempo de detecção, carregamento e exibição das tarefas a partir de `package.json` ocorre em menos de 200 milissegundos em condições normais de uso em terminal.
- **SC-004**: 100% das mensagens de diagnóstico, erros de validação e avisos direcionados ao usuário no terminal são exibidos em português do Brasil (`pt-BR`).
- **SC-005**: Taxa de sucesso de 100% na execução de tarefas do `package.json` tanto via seleção interativa quanto via acionamento direto por linha de comando (`--run`).
- **SC-006**: Em 100% dos acionamentos de tarefas originadas de `package.json` que possuam scripts de ciclo de vida (`pre<tarefa>` e/ou `post<tarefa>`), os scripts são executados na ordem sequencial correta e abortados apropriadamente em caso de falha prévia.
- **SC-007**: No menu interativo de `package.json`, 100% dos scripts de ciclo de vida (`pre*`/`post*`) com comandos principais associados permanecem ocultos, apresentando uma lista limpa e concisa.

## Assumptions

- O formato adotado para o `package.json` segue a especificação padrão de pacotes do ecossistema Node.js/npm, no qual os scripts residem em um objeto JSON sob a chave raiz `"scripts"`.
- Como o `package.json` padrão não possui campo de descrição por tarefa, as tarefas extraídas dele terão o campo de descrição vazio e a interface de terminal apresentará o próprio comando como detalhe informativo.
- Caso o arquivo `nxtl.json` exista e apresente erros de sintaxe ou estrutura, o sistema priorizará a notificação desse erro em vez de tentar ler o `package.json` em segundo plano, evitando comportamento imprevisível ou execução de comandos não intencionais.
- O comando `nxtl --init` continuará gerando o arquivo nativo `nxtl.json` com template padrão, preservando a capacidade de configuração avançada com descrições e tipos de execução sem ler nem migrar o `package.json`.
- Para manter paridade com a ferramenta `ntl`, a descoberta e encadeamento de ganchos de ciclo de vida (`pre` e `post`), bem como a ocultação destes no menu interativo, aplicam-se às tarefas originadas do `package.json`.
