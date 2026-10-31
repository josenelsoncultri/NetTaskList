# Feature Specification: Processador de Arquivos de Configuração JSON (JSON Configuration Processor)

**Feature Branch**: `001-json-config-processor`

**Created**: 2026-10-30

**Status**: Draft

**Input**: User description: "Um processador de arquivos de configuração em formato json, que será uma lista de comandos com um nome e o comando em si para executar, além de um tipo para identificar possível execução de ferramentas ou projetos C# file-scoped. Comportamento esperado: - O processador deverá ler esse arquivo de configuração e listar as tarefas, detalhando nome e o comando a ser executado - A configuração deve ser de fácil acesso para edição via editor - Não se pode ter mais de um script com o mesmo nome - Pode-se configurar vários scripts ao mesmo tempo"

## Clarifications

### Session 2026-10-30

- Q: Como o campo command deve ser especificado para tarefas do tipo C# file-scoped (`csharp`) no arquivo de configuração? → A: Híbrido/Flexível: aceita tanto apenas o caminho do arquivo `.cs` (inferindo a execução padrão) quanto a linha de comando completa.
- Q: Como o usuário deve acionar a listagem detalhada de tarefas através do terminal? → A: Invocação direta via comando `nxtl`, listando as tarefas lidas do arquivo JSON e permitindo a navegação e seleção interativa com as teclas de seta (Up/Down).
- Q: Como a ferramenta deve se comportar quando o comando nxtl for executado em um ambiente de terminal não interativo? → A: A ferramenta deve permitir a especificação direta da tarefa através de uma flag (ex: `nxtl --run <nome>` ou `-r <nome>`), contornando a navegação interativa e viabilizando a automação e uso em CI/CD.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Listar e Navegar Tarefas Configuras Interativamente (Priority: P1)

Como uma pessoa desenvolvedora executando o comando `nxtl` no diretório do projeto, quero que o processador leia o arquivo de configuração JSON e exiba uma lista interativa de todas as tarefas cadastradas com navegação via setas do teclado (Up/Down), detalhando seus respectivos nomes, comandos a serem executados e os tipos de execução, para que eu possa explorar e selecionar facilmente as automações disponíveis.

**Why this priority**: Esta é a jornada central (MVP). Sem a capacidade de carregar o arquivo e apresentar as tarefas com navegação interativa e detalhes de comandos e tipos, a ferramenta não entrega seu valor básico de inspeção e catálogo de scripts.

**Independent Test**: Pode ser testado de forma independente criando um arquivo de configuração JSON com duas tarefas válidas no diretório atual e executando o comando `nxtl` diretamente; a saída deve apresentar a lista interativa com suporte à navegação por setas (Up/Down) exibindo nomes, comandos e tipos correspondentes de forma legível.

**Acceptance Scenarios**:

1. **Given** um arquivo de configuração JSON válido contendo duas tarefas ("build" com comando `dotnet build` e tipo `tool`, e "script" com comando `dotnet run Script.cs` e tipo `csharp`), **When** o usuário executa o comando `nxtl` no terminal, **Then** o sistema exibe as tarefas em modo interativo navegável por setas (Up/Down), detalhando claramente seus nomes, comandos e tipos.
2. **Given** um arquivo de configuração JSON válido com múltiplas tarefas, **When** o comando `nxtl` é executado no terminal, **Then** as informações são apresentadas organizadas em formato tabular interativo e navegável.
3. **Given** um arquivo de configuração JSON válido com a tarefa "build", **When** o usuário executa `nxtl --run build`, **Then** o sistema seleciona a tarefa "build" diretamente para execução sem solicitar navegação interativa por setas.

---

### User Story 2 - Validação de Nomes Duplicados e Erros de Formato (Priority: P2)

Como uma pessoa mantenedora de scripts, quero que o processador valide o arquivo de configuração ao carregá-lo e rejeite tarefas que compartilhem o mesmo nome, além de acusar arquivos JSON mal formatados, exibindo mensagens de erro claras em português, para que inconsistências sejam corrigidas antes de qualquer execução ambígua.

**Why this priority**: Evitar ambiguidade é um requisito essencial do usuário ("Não se pode ter mais de um script com o mesmo nome"). Sem essa validação precoce, execuções subsequentes poderiam invocar o script incorreto.

**Independent Test**: Pode ser testado fornecendo um arquivo JSON contendo tarefas com o mesmo nome (ou sintaxe JSON inválida) e verificando que a ferramenta interrompe a operação com uma mensagem diagnóstica precisa em português indicando o nome duplicado ou o erro de sintaxe.

**Acceptance Scenarios**:

1. **Given** um arquivo de configuração JSON contendo duas tarefas com o mesmo nome (ex: "teste"), **When** o processador tenta ler o arquivo, **Then** o sistema recusa a configuração e exibe uma mensagem de erro em português indicando que o nome "teste" já está em uso e não são permitidos nomes duplicados.
2. **Given** um arquivo de configuração JSON contendo duplicidade com variações de caixa (ex: "Build" e "build"), **When** o processador faz a leitura, **Then** a duplicidade é identificada independentemente da caixa (case-insensitive) e a operação é abortada com mensagem explicativa.
3. **Given** um arquivo de configuração com sintaxe JSON corrompida (ex: vírgula sobrando ou chaves abertas), **When** o processador tenta realizar a leitura, **Then** o sistema apresenta um aviso diagnóstico compreensível orientando o usuário sobre o problema no arquivo.

---

### User Story 3 - Suporte a Múltiplos Scripts e Tipificação (Tool vs C# File-Scoped) (Priority: P3)

Como uma pessoa desenvolvedora trabalhando em projetos .NET, quero poder declarar vários scripts simultaneamente no mesmo arquivo e especificar o tipo de cada um (identificando se é uma ferramenta/comando comum ou a execução de um script C# file-scoped), para que tarefas de natureza diferente sejam reconhecidas e categorizadas corretamente pelo sistema.

**Why this priority**: Atende diretamente à necessidade de diferenciar comandos de ferramentas externas de arquivos/projetos C# executáveis em arquivo único (file-scoped), permitindo um ecossistema misto de automações no mesmo repositório.

**Independent Test**: Pode ser testado configurando um arquivo com 5 ou mais scripts de diferentes categorias (comandos gerais e arquivos C#), garantindo que todos sejam carregados na mesma sessão e seus tipos sejam distinguidos.

**Acceptance Scenarios**:

1. **Given** um arquivo de configuração com múltiplos scripts declarados ao mesmo tempo, **When** o processador carrega a lista, **Then** todos os scripts são carregados preservando a ordem ou identificação definida pelo usuário.
2. **Given** um script cujo tipo é especificado como projeto ou arquivo C# file-scoped, **When** a lista é exibida, **Then** o tipo é categorizado e indicado como execução de script C#.
3. **Given** um script onde o campo de tipo foi omitido, **When** o processador carrega a tarefa, **Then** o sistema adota um tipo padrão seguro (comando de ferramenta padrão) sem falhar.

---

### User Story 4 - Facilidade de Edição Manual e Localização do Arquivo (Priority: P4)

Como uma pessoa desenvolvedora, quero que a estrutura do arquivo de configuração JSON seja intuitiva, limpa e padronizada, permitindo fácil edição em qualquer editor de texto ou IDE, além de permitir apontar para um arquivo de configuração em caminho customizado quando necessário.

**Why this priority**: Garante excelente experiência de desenvolvimento (DX) e facilidade de manutenção sem exigir ferramentas externas complexas para alterar os scripts do projeto.

**Independent Test**: Pode ser testado abrindo e editando o arquivo JSON manualmente em um editor convencional, e invocando a ferramenta com opção de caminho customizado (`--config <caminho>`).

**Acceptance Scenarios**:

1. **Given** um desenvolvedor editando o arquivo de configuração com um editor de texto convencional, **When** ele adiciona ou altera um bloco de script no formato JSON padronizado, **Then** a ferramenta reconhece imediatamente as alterações na próxima execução.
2. **Given** um arquivo de configuração localizado em um caminho alternativo ou subdiretório, **When** o usuário passa o argumento de configuração personalizada na CLI, **Then** o processador lê as tarefas daquele caminho específico com sucesso.

---

### Edge Cases

- **Arquivo ausente no diretório**: O que acontece se o usuário rodar o comando e não existir nenhum arquivo `nxtl.json`? O sistema deve exibir uma mensagem amigável em português informando que nenhum arquivo de configuração foi localizado e instruindo como criá-lo.
- **Arquivo vazio ou com tamanho zero**: O que acontece se o arquivo existir mas estiver completamente em branco? O sistema deve reportar que o arquivo está vazio de forma clara.
- **Lista de tarefas vazia**: O que acontece se o arquivo for um JSON válido `{ "scripts": [] }` ou `[]`? O sistema deve informar ao usuário que nenhuma tarefa foi cadastrada no arquivo.
- **Tarefa sem nome ou com nome em branco**: Se um item possuir nome vazio (`""`) ou contendo apenas espaços, a validação deve rejeitar o item informando que toda tarefa requer um nome válido.
- **Tarefa sem comando**: Se um item possuir nome mas não definir o comando correspondente, a validação deve rejeitar o arquivo apontando a pendência na respectiva tarefa.
- **Tipo não reconhecido**: Se for fornecido um valor desconhecido no campo de tipo, o sistema deve alertar o usuário sobre o tipo não suportado e listar as opções aceitas.
- **Execução com flag `--run` para tarefa inexistente**: Se o usuário fornecer `--run <nome>` e esse nome não existir no arquivo de configuração, o sistema deve abortar com mensagem de erro em português indicando que a tarefa não foi encontrada no arquivo.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE procurar e carregar por padrão o arquivo de configuração denominado `nxtl.json` no diretório de trabalho atual.
- **FR-002**: O sistema DEVE permitir a especificação de um caminho customizado para o arquivo de configuração através de parâmetro de linha de comando (ex: `--config <caminho>`).
- **FR-003**: O sistema DEVE aceitar arquivos de configuração no formato JSON estruturados com uma lista declarativa de scripts.
- **FR-004**: Cada definição de script DEVE conter obrigatoriamente um identificador textual único (`name`) e uma instrução de execução não vazia (`command`).
- **FR-005**: Cada definição de script DEVE permitir a especificação de um atributo de tipo (`type`) para diferenciar a natureza da execução (ex: comandos de ferramentas gerais versus execução de scripts/projetos C# file-scoped). Para tarefas do tipo `csharp`, o campo `command` DEVE aceitar tanto diretamente o caminho do arquivo `.cs` (onde a execução padrão é inferida) quanto uma instrução de comando completa.
- **FR-006**: Caso o atributo `type` seja omitido em uma tarefa, o sistema DEVE assumir um valor padrão representativo de comando de ferramenta padrão (`tool`).
- **FR-007**: O sistema DEVE permitir atributos descritivos opcionais (`description`) para enriquecer a exibição da finalidade de cada tarefa na listagem.
- **FR-008**: O sistema DEVE rejeitar a configuração caso dois ou mais scripts possuam o mesmo nome (comparação sem diferenciação de maiúsculas/minúsculas - case-insensitive), emitindo mensagem de erro diagnóstica em português brasileiro (`pt-BR`).
- **FR-009**: O sistema DEVE validar a sintaxe e a integridade estrutural do JSON, exibindo mensagens de erro descritivas em português brasileiro caso o arquivo esteja mal formatado, corrompido ou com propriedades obrigatórias ausentes.
- **FR-010**: O sistema DEVE, ao ser executado diretamente pelo comando `nxtl` sem argumentos no diretório de trabalho, carregar o arquivo de configuração e disponibilizar a listagem interativa das tarefas com navegação via setas do teclado (Up/Down), apresentando organizadamente:
  - Nome da tarefa
  - Tipo de execução (ferramenta vs script C#)
  - Comando que será executado
  - Descrição da tarefa (se informada)
- **FR-011**: O sistema DEVE permitir a configuração simultânea de múltiplos scripts (suportando desde arquivos com apenas 1 tarefa até dezenas de tarefas no mesmo documento).
- **FR-012**: Todas as mensagens direcionadas ao usuário no terminal (listagens, avisos, cabeçalhos, erros de validação e ajuda) DEVEM ser emitidas em português brasileiro (`pt-BR`).
- **FR-013**: O sistema DEVE disponibilizar uma opção via parâmetro de linha de comando (ex: `--run <nome>` ou `-r <nome>`) para selecionar diretamente uma tarefa específica cadastrada no arquivo de configuração, contornando a seleção interativa por setas do teclado e permitindo execução direta e automatizada em ambientes de script ou CI/CD.

### Key Entities *(include if feature involves data)*

- **ScriptTask**: Representa uma tarefa ou script configurado individualmente.
  - *Name* (string, obrigatório): Identificador único e amigável da tarefa (ex: "build", "teste", "gerar-docs").
  - *Command* (string, obrigatório): Linha de comando ou instrução exata a ser executada. Para tarefas do tipo "csharp", aceita tanto o caminho direto do arquivo `.cs` quanto o comando completo.
  - *Type* (string, opcional, padrão: "tool"): Categoria da execução (ex: "tool" para comandos do sistema/CLI e "csharp" para programas C# em arquivo único / file-scoped).
  - *Description* (string, opcional): Resumo textual do propósito da tarefa para exibição na listagem.
- **TaskConfiguration**: Representa o documento de configuração consolidado.
  - *SourcePath* (string): Caminho de origem do arquivo JSON lido.
  - *Tasks* (coleção ordenada de ScriptTask): Conjunto de todas as tarefas cadastradas e validadas.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: O processador carrega o arquivo de configuração e exibe a listagem completa das tarefas no terminal em menos de 300 milissegundos para arquivos típicos (até 50 tarefas).
- **SC-002**: 100% dos casos de duplicidade de nomes de tarefas são interceptados e impedem a continuação, informando explicitamente qual nome causou a colisão.
- **SC-003**: 100% dos erros de sintaxe ou de campos ausentes no JSON produzem mensagens diagnósticas legíveis em português, sem interrupção abrupta por falha não tratada.
- **SC-004**: O usuário consegue editar, adicionar ou remover tarefas diretamente em qualquer editor de texto convencional (como VS Code, Notepad, vim) em conformidade com a especificação JSON padrão, sem necessidade de ferramentas proprietárias.
- **SC-005**: A saída em terminal da listagem de tarefas exibe com 100% de clareza a separação entre nome, tipo e comando em layout estruturado.

## Assumptions

- **Localização padrão do arquivo**: O arquivo de configuração principal é procurado no diretório atual de execução com o nome `nxtl.json` (alinhado ao nome do utilitário `nxtl`), com suporte adicional a caminho explícito via argumento `--config`.
- **Formato flexível de entrada JSON**: O leitor suportará tanto a forma de objeto com propriedade raiz contendo a coleção (`{ "scripts": [ ... ] }` ou `{ "tasks": [ ... ] }`) quanto um array direto na raiz (`[ ... ]`), proporcionando flexibilidade para o desenvolvedor.
- **Tipos de tarefas reconhecidos inicialmente**:
  - `tool`: Representa ferramentas de linha de comando gerais ou comandos de shell (padrão).
  - `csharp`: Representa arquivos C# executáveis em modo file-scoped / script simples (ex: `dotnet run arquivo.cs`).
- **Escopo desta funcionalidade**: O foco deste módulo é o carregamento, validação e apresentação/listagem das tarefas configuradas. A orquestração da execução física dos processos (com controle de streams, códigos de saída e cancelamento) será acoplada nas camadas de execução correspondentes.
- **Idioma das mensagens de interface**: Em estrita conformidade com a Constituição do projeto (`NetTaskList`), todas as comunicações com o usuário no terminal utilizam português brasileiro (`pt-BR`).
