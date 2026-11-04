# Feature Specification: Execução Sequencial de Múltiplos Comandos em Tarefas (Sequential Task Commands)

**Feature Branch**: `008-sequential-task-commands`

**Created**: 2026-11-04

**Status**: Draft

**Input**: User description: "Permitir que as tarefas no nxtl.json contenham um array de comandos a serem executados que, ao selecionar a tarefa, serão executados um por vez na sequência em que estiverem configurados no arquivo. Caso dê erro na execução de um deles, a execução dos comandos posteriores será interrompida"

## Clarifications

### Session 2026-11-04
- Q: Como o sistema deve se comportar se uma tarefa no `nxtl.json` contiver simultaneamente as propriedades `command` e `commands`? → A: A configuração de comandos múltiplos deve ser suportada exclusivamente através da propriedade `command` (aceitando texto puro ou array de textos), sem criar a propriedade `commands`, assegurando retrocompatibilidade total com arquivos `nxtl.json` já existentes.
- Q: Qual formato visual deve ser exibido no terminal antes de iniciar a execução de cada comando em tarefas configuradas com múltiplos comandos? → A: Exibir `Comando [1/N]: <linha de comando>` para tarefas com múltiplos comandos (onde N é o total de comandos na sequência) e preservar `Comando: <linha de comando>` quando a tarefa possuir um único comando.
- Q: No menu de seleção interativa (`nxtl`), como uma tarefa com múltiplos comandos deve ser exibida caso não possua uma `description` cadastrada? → A: Exibir o texto `<sem descrição>` no lugar da descrição ausente (ex: `nome [tool] - <sem descrição>`).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Configuração e Execução Sequencial de Múltiplos Comandos (Priority: P1)

Como uma pessoa usuária do CLI de tarefas, quero configurar uma lista ordenada de comandos em uma única tarefa no arquivo de configuração, para que ao acionar essa tarefa todos os comandos sejam executados automaticamente, um após o outro, na ordem em que foram declarados.

**Why this priority**: É a funcionalidade central solicitada. Permite compor fluxos de trabalho completos (como limpar, compilar e testar) sob um único nome de tarefa sem depender de scripts externos complexos ou concatenações manuais frágeis de shell.

**Independent Test**: Pode ser testado definindo uma tarefa com uma lista de comandos (por exemplo, dois comandos de impressão ou operações de sistema) e acionando-a via seleção interativa ou via comando direto de execução; o sistema deve executar o primeiro comando, aguardar seu término com sucesso e imediatamente executar o segundo comando.

**Acceptance Scenarios**:

1. **Given** uma tarefa configurada com uma lista de múltiplos comandos (ex: `["comando-1", "comando-2", "comando-3"]`), **When** o usuário aciona a execução dessa tarefa, **Then** o sistema executa `comando-1`, aguarda sua finalização com código de sucesso, em seguida executa `comando-2`, aguarda sua finalização e por fim executa `comando-3`.
2. **Given** a execução de uma tarefa com múltiplos comandos, **When** cada comando inicia, **Then** o sistema exibe no terminal a linha `Comando [X/N]: <linha de comando>` (onde X é a etapa corrente e N é o total de comandos), mantendo a saída padrão e de erro visíveis em tempo real. Para tarefas com comando único, o sistema mantém `Comando: <linha de comando>`.
3. **Given** uma tarefa configurada com apenas um comando em formato de texto simples (como nas versões anteriores), **When** o usuário a executa, **Then** o sistema executa o comando normalmente, mantendo total compatibilidade com configurações pré-existentes.

---

### User Story 2 - Interrupção Imediata da Cadeia de Execução em Caso de Erro (Priority: P2)

Como uma pessoa usuária do CLI de tarefas, quero que a execução dos comandos seguintes seja imediatamente interrompida se qualquer um dos comandos da sequência falhar, para que comandos dependentes não sejam executados em um estado inconsistente ou com erros anteriores não resolvidos.

**Why this priority**: Garante segurança operacional e previsibilidade. Se uma etapa de compilação ou validação falhar, etapas subsequentes (como empacotamento, publicação ou deploy) não devem rodar.

**Independent Test**: Pode ser testado configurando uma tarefa com três comandos em sequência, onde o segundo comando falha intencionalmente (código de saída diferente de zero ou falha de inicialização); o primeiro comando deve rodar, o segundo deve falhar e o terceiro comando NUNCA deve ser executado.

**Acceptance Scenarios**:

1. **Given** uma tarefa com uma sequência de três comandos, **When** o segundo comando encerra com código de erro (diferente de zero), **Then** o sistema interrompe a execução imediatamente, não inicia o terceiro comando e exibe uma mensagem explicativa em português informando que a execução foi interrompida devido à falha.
2. **Given** uma tarefa cuja execução foi interrompida por erro em um de seus comandos, **When** o processo do CLI finaliza, **Then** o código de saída retornado pelo CLI é exatamente o código de erro retornado pelo comando que falhou.
3. **Given** uma tarefa cujo comando na sequência não existe ou falha ao iniciar, **When** o sistema tenta executá-lo, **Then** a execução é interrompida imediatamente, os comandos seguintes não são executados e uma mensagem clara de erro é apresentada ao usuário.

---

### User Story 3 - Validação e Flexibilidade Declarativa no Arquivo de Configuração (Priority: P3)

Como uma pessoa usuária configurando tarefas no arquivo de configuração, quero que o sistema aceite tanto comandos únicos quanto listas de comandos de forma intuitiva, validando a estrutura e alertando claramente caso existam listas vazias ou itens em branco.

**Why this priority**: Previne falhas silenciosas ou comportamentos inesperados causados por arquivos de configuração malformados ou incompletos, fornecendo diagnósticos rápidos e fáceis de corrigir.

**Independent Test**: Pode ser testado fornecendo um arquivo de configuração com uma tarefa contendo lista vazia de comandos ou contendo comandos com textos vazios; o sistema deve recusar a inicialização da tarefa e exibir mensagens de erro amigáveis em português detalhando a inconsistência encontrada.

**Acceptance Scenarios**:

1. **Given** uma tarefa configurada com a propriedade de comando contendo uma lista vazia (`[]`), **When** o arquivo é carregado pelo CLI, **Then** o sistema rejeita a configuração e apresenta uma mensagem de validação em português indicando que a lista de comandos não pode ser vazia.
2. **Given** uma tarefa contendo uma lista de comandos onde um dos itens é vazio ou composto apenas por espaços em branco, **When** o arquivo é lido, **Then** o sistema rejeita a configuração com mensagem de erro clara indicando o item inválido.
3. **Given** uma tarefa configurada com a propriedade `command` contendo um array de textos (ex: `["dotnet build", "dotnet test"]`), **When** o arquivo é processado, **Then** o sistema interpreta adequadamente a lista ordenada de instruções a serem executadas, mantendo a propriedade `command` como o campo unificado tanto para comando único quanto para múltiplos comandos.
4. **Given** a exibição de tarefas no menu de seleção interativa para uma tarefa com múltiplos comandos que não possua o campo `description` configurado, **When** as opções são listadas para o usuário, **Then** o sistema exibe o texto `<sem descrição>` no lugar da descrição ausente (ex: `nome-tarefa [tool] - <sem descrição>`).

---

### Edge Cases

- **Lista contendo exatamente um único comando**: Deve ser tratada com o mesmo fluxo consistente da lista sequencial, executando o comando individual e finalizando com seu status.
- **Cancelamento pelo usuário durante a execução (Ctrl+C)**: Se o usuário abortar o processo durante a execução de qualquer comando intermediário, o comando corrente deve ser finalizado e nenhum dos comandos subsequentes da sequência deve ser iniciado.
- **Espaços e quebras de linha em comandos**: Cada comando na lista de comandos deve ter seus espaços nas extremidades devidamente tratados, preservando argumentos internos e aspas.
- **Comandos de scripts C# file-scoped em sequência**: Caso a tarefa seja categorizada como script C#, cada comando ou caminho de script configurado na lista deve ser devidamente inferido e executado conforme as regras de execução da categoria.
- **Comando subsequente dependente de arquivos gerados pelo comando anterior**: O sistema deve garantir que o processo anterior finalizou completamente e liberou seus recursos antes de disparar o comando posterior.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE permitir que a propriedade `command` das tarefas no arquivo `nxtl.json` receba tanto um texto puro (string) quanto um array de textos (array de strings) com múltiplos comandos a serem executados sequencialmente.
- **FR-002**: O sistema DEVE manter total compatibilidade com tarefas configuradas com um único comando em formato de texto (string).
- **FR-003**: Ao acionar a execução de uma tarefa com múltiplos comandos (seja via menu interativo ou via execução direta por linha de comando), o sistema DEVE executar os comandos um por vez, respeitando rigorosamente a sequência em que estão declarados no arquivo.
- **FR-004**: O sistema DEVE aguardar o término da execução de cada comando antes de iniciar o comando seguinte da sequência.
- **FR-005**: Se a execução de qualquer comando da sequência resultar em erro (código de saída diferente de zero ou falha de inicialização), o sistema DEVE interromper imediatamente o fluxo e NÃO DEVE executar nenhum dos comandos posteriores.
- **FR-006**: Ao interromper a execução devido a erro em um comando, o sistema DEVE exibir uma mensagem explicativa no terminal no idioma Português do Brasil (`pt-BR`) informando a interrupção da sequência.
- **FR-007**: O sistema DEVE retornar como código de saída final da ferramenta o mesmo código de saída gerado pelo comando que falhou na sequência. Caso todos os comandos sejam executados com sucesso, o sistema DEVE retornar código de saída zero.
- **FR-008**: O sistema DEVE validar o arquivo de configuração, rejeitando tarefas cuja lista de comandos esteja vazia ou contenha itens vazios ou compostos exclusivamente por espaços em branco, emitindo mensagens de diagnóstico claras em português (`pt-BR`).
- **FR-009**: Durante a execução de uma tarefa com múltiplos comandos, o sistema DEVE exibir no terminal a indicação de etapa no formato `Comando [X/N]: <linha de comando>` (onde X é o índice da etapa a partir de 1 e N é a quantidade total de comandos da sequência) antes de iniciar cada processo. Para tarefas com um único comando, o sistema DEVE exibir `Comando: <linha de comando>`.
- **FR-010**: O esquema de validação do arquivo de configuração (JSON Schema) DEVE ser atualizado para que a propriedade `command` aceite tanto o tipo `string` quanto o tipo `array` de `string` (com `minItems: 1` e cada item com `minLength: 1`).
- **FR-011**: No menu de seleção interativa de tarefas (`nxtl`), quando uma tarefa configurada com múltiplos comandos não possuir uma descrição (`description`) definida, o sistema DEVE exibir o texto `<sem descrição>` no rótulo da opção de escolha.

### Key Entities *(include if feature involves data)*

- **Tarefa de Script (Script Task)**: Unidade configurável de automação que contém um identificador único, uma lista ordenada de um ou mais comandos executáveis, categoria de execução e descrição opcional.
- **Sequência de Comandos (Command Sequence)**: Coleção ordenada de instruções de linha de comando pertencentes a uma tarefa, a serem despachadas sequencialmente com regra de parada obrigatória em caso de falha.
- **Resultado de Execução da Sequência (Execution Sequence Result)**: Registro do desfecho consolidado da tarefa, contendo a etapa na qual a execução foi concluída ou interrompida, o código de saída final e eventuais mensagens diagnósticas de erro.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% das tarefas configuradas com listas de múltiplos comandos executam na exata ordem em que estão declaradas no arquivo.
- **SC-002**: Em 100% dos cenários onde um comando intermediário falha, nenhum comando subsequente da lista é executado.
- **SC-003**: O código de saída do processo do CLI reflete fielmente o código de erro retornado pelo comando falho em 100% das ocorrências de falha.
- **SC-004**: 100% das tarefas existentes definidas com comando em formato de texto único continuam funcionando sem qualquer alteração comportamental perceptível.
- **SC-005**: Mensagens diagnósticas de erro relativas a listas de comandos vazias ou itens em branco são apresentadas em português (`pt-BR`) em 100% dos casos de validação de configuração inválida.

## Assumptions

- Cada comando configurado na lista executa no mesmo diretório de trabalho e herda o ambiente padrão do processo do CLI.
- Não há necessidade de paralelização ou execução concorrente de comandos na mesma tarefa; o comportamento desejado é estritamente sequencial.
- O formato de definição no arquivo `nxtl.json` continuará sendo JSON padronizado com suporte a esquemas de validação.
