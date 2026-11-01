# Feature Specification: Mecanismo de Execução de Tarefas e Scripts Configurados (Task Execution Engine)

**Feature Branch**: `003-execute-configured-tasks`

**Created**: 2026-11-01

**Status**: Draft

**Input**: User description: "Implementar mecanismo de execuçaõ dos comandos detalhados na ferramenta. - Se for um tipo de comando csharp, irá iniciar o arquivo como um file-scoped app, ou seja, rodar o dotnet em cima do arquivo - Se for um comando normal, irá rodar apenas o texto do comando normalmente, como se tivesse rodando o comando diretamente pelo terminal sem o uso da ferramenta"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Executar Tarefas de Comandos Normais (Priority: P1)

Como uma pessoa desenvolvedora utilizando a ferramenta no terminal, quero selecionar ou especificar uma tarefa do tipo comando normal (ferramenta/shell) para que ela seja executada diretamente no ambiente do sistema operacional, exibindo sua saída em tempo real e preservando a interatividade do terminal exatamente como se eu tivesse digitado o comando manualmente.

**Why this priority**: Esta é a capacidade central (MVP) do executor. Sem conseguir disparar e conectar o comando ao terminal do usuário, a ferramenta apenas exibe metadados de tarefas sem efetivar nenhuma automação.

**Independent Test**: Pode ser testado configurando uma tarefa comum (ex: `echo "Olá Mundo"` ou `git status`) e acionando-a via `nxtl --run <nome>` ou via menu interativo; a saída deve aparecer imediatamente no console e o processo deve finalizar com código de sucesso.

**Acceptance Scenarios**:

1. **Given** uma tarefa configurada com o comando `echo "Executando teste"` do tipo normal (`tool`), **When** o usuário a executa via `nxtl --run <nome>` ou selecionando-a na lista interativa, **Then** o sistema executa o comando diretamente no terminal, exibindo a mensagem "Executando teste" em tempo real para o usuário.
2. **Given** um comando normal interativo que solicita confirmação do usuário (ex: prompt de confirmação de ferramenta CLI), **When** a tarefa é executada pelo `nxtl`, **Then** a entrada padrão (stdin) permanece conectada, permitindo ao usuário responder à interação diretamente pelo console.
3. **Given** uma tarefa de comando normal que imprime tanto saídas padrão (stdout) quanto saídas de erro (stderr), **When** o comando é executado, **Then** ambos os fluxos de saída são refletidos imediatamente no terminal sem buffering artificial ou perda de formatação e cores.

---

### User Story 2 - Executar Scripts C# File-Scoped (Priority: P2)

Como uma pessoa desenvolvedora que mantém automações em C# em arquivos únicos (file-scoped scripts), quero que a ferramenta reconheça tarefas do tipo `csharp` e as inicie utilizando o runtime do .NET (`dotnet`), permitindo tanto a indicação direta do arquivo `.cs` quanto comandos customizados, para que scripts C# sejam executados sem esforço manual de compilação ou configuração de projetos separados.

**Why this priority**: O diferencial arquitetural da ferramenta é a execução transparente de scripts C# file-scoped. Essa história viabiliza rodar scripts `.cs` de forma fluida e automática.

**Independent Test**: Pode ser testado criando um arquivo `script.cs` válido (com saída simples no console) e uma tarefa do tipo `csharp` configurada apenas com o nome do arquivo; ao acionar a tarefa, a ferramenta deve acionar `dotnet run script.cs` e exibir o resultado no terminal.

**Acceptance Scenarios**:

1. **Given** uma tarefa do tipo `csharp` configurada com o caminho de um arquivo C# (ex: `scripts/demo.cs`), **When** o usuário aciona a execução da tarefa, **Then** o sistema inicia a execução via .NET (`dotnet run scripts/demo.cs`), exibindo a saída do script no console.
2. **Given** uma tarefa do tipo `csharp` cujo comando já inclui explicitamente o prefixo de execução do .NET (ex: `dotnet run scripts/demo.cs`), **When** o usuário aciona a execução, **Then** o sistema executa a linha de comando sem duplicar o comando `dotnet run`.
3. **Given** uma tarefa do tipo `csharp` apontando para um arquivo `.cs` inexistente ou inacessível, **When** a execução for solicitada, **Then** o sistema deve informar claramente que o arquivo de script não foi encontrado antes ou durante a execução, retornando código de erro apropriado.

---

### User Story 3 - Propagação de Códigos de Saída e Tratamento de Erros (Priority: P3)

Como uma pessoa desenvolvedora ou mantenedora de pipelines de automação/CI, quero que a ferramenta propague fielmente o código de saída (exit code) do processo executado e trate falhas de inicialização ou interrupções de processo (como Ctrl+C), para que scripts externos e operadores humanos saibam com precisão se a tarefa teve sucesso ou falhou.

**Why this priority**: Garantir integridade do fluxo de controle e sinalização de erros permite que o `nxtl` seja integrado de forma confiável tanto no dia a dia do desenvolvedor quanto em scripts de automação.

**Independent Test**: Pode ser testado executando uma tarefa configurada com um comando que falha intencionalmente (ex: retornando exit code 42 ou executando um comando inexistente); o `nxtl` deve encerrar com o mesmo código de saída ou com erro diagnóstico claro.

**Acceptance Scenarios**:

1. **Given** uma tarefa cujo comando encerra com código de saída 0 (sucesso), **When** a execução termina, **Then** o `nxtl` finaliza com código de saída 0.
2. **Given** uma tarefa cujo comando encerra com código de saída diferente de zero (ex: 1, 2 ou 127), **When** a execução termina, **Then** o `nxtl` propaga exatamente esse código de saída para o processo chamador.
3. **Given** uma tarefa cujo comando especificado não existe ou não pode ser iniciado pelo sistema operacional, **When** a ferramenta tenta iniciá-lo, **Then** uma mensagem de erro clara em português é exibida informando a falha de inicialização e a ferramenta encerra com status de erro.
4. **Given** uma tarefa de longa duração em execução, **When** o usuário pressiona Ctrl+C no terminal, **Then** o sinal de cancelamento é propagado para o processo filho e a ferramenta encerra de maneira limpa.

---

### Edge Cases

- **Comando com espaços, aspas ou argumentos múltiplos**: O sistema deve preservar a integridade de argumentos delimitados por aspas simples ou duplas ao repassar para o interpretador/shell de execução.
- **Arquivo de script C# inexistente**: Quando o comando referenciar um arquivo `.cs` que não existe no caminho indicado, o sistema deve fornecer diagnóstico imediato orientando sobre o caminho não encontrado.
- **Variáveis de ambiente locais**: Comandos que dependem de variáveis de ambiente do usuário ou do terminal devem herdar o ambiente do processo atual do `nxtl`.
- **Cancelamento interativo (Ctrl+C)**: Se o usuário abortar a execução com SIGINT (Ctrl+C), a ferramenta não deve travar nem deixar processos zumbis orquestrados em segundo plano.
- **Comandos vazios ou compostos apenas por espaços**: Devem ser rejeitados com aviso de erro em português antes de tentar invocar o sistema operacional.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE executar a tarefa selecionada pelo usuário após a confirmação no menu interativo ou quando fornecida diretamente através do parâmetro `--run <nome>`.
- **FR-002**: Para tarefas do tipo comando normal (`tool`), o sistema DEVE executar a cadeia de comando no ambiente do sistema operacional respeitando a sintaxe nativa de comandos do terminal.
- **FR-003**: Para tarefas do tipo C# (`csharp`), o sistema DEVE invocar a execução do arquivo como aplicação file-scoped através do comando `dotnet run`, inferindo automaticamente o comando caso apenas o caminho do arquivo `.cs` tenha sido fornecido.
- **FR-004**: O sistema DEVE conectar a entrada padrão (stdin), saída padrão (stdout) e saída de erro (stderr) diretamente ao terminal do usuário, garantindo exibição em tempo real e interatividade.
- **FR-005**: O sistema DEVE aguardar a conclusão do processo filho em execução antes de encerrar o `nxtl`.
- **FR-006**: O sistema DEVE propagar o código de saída (exit code) do processo executado como o código de retorno da ferramenta `nxtl`.
- **FR-007**: O sistema DEVE exibir mensagens de erro amigáveis em português (`pt-BR`) quando o comando ou arquivo associado à tarefa não for encontrado ou falhar ao iniciar.
- **FR-008**: O sistema DEVE preservar variáveis de ambiente e diretório de trabalho atual na inicialização do processo filho.

### Key Entities *(include if feature involves data)*

- **Tarefa Executável (ScriptTask)**: Representa uma unidade de automação configurada, contendo identificador único (`Name`), tipo de execução (`Type`: `tool` ou `csharp`), linha de comando original (`Command`) e a resolução da linha de execução final (`ResolveExecutionCommand`).
- **Resultado de Execução (ProcessExecutionResult)**: Representa o desfecho da execução de uma tarefa, contendo o código de saída do processo (`ExitCode`), indicativo de sucesso/falha e possíveis detalhes de erro caso a inicialização do processo tenha sido impedida.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: O tempo de inicialização da execução da tarefa após a seleção do usuário deve ser inferior a 100 milissegundos (excluindo o tempo de inicialização intrínseco do próprio comando a ser executado).
- **SC-002**: 100% dos códigos de saída de processos executados com sucesso (0) ou falha (não-zero) devem ser propagados fielmente ao ambiente do terminal.
- **SC-003**: 100% dos comandos de tarefas do tipo normal (`tool`) e scripts C# (`csharp`) válidos configurados executam e exibem suas mensagens na saída do terminal sem perdas de caracteres ou cores ANSI.
- **SC-004**: Mensagens diagnósticas de erro em caso de comando inexistente ou arquivo não encontrado devem ser exibidas em português (`pt-BR`) de forma compreensível em 100% das ocorrências de falha de lançamento de processo.

## Assumptions

- O usuário possui o SDK do .NET instalado e acessível no `PATH` do sistema para execução de tarefas do tipo `csharp`.
- A ferramenta herda as permissões de execução do usuário corrente no sistema operacional.
- Para comandos normais (`tool`), o sistema operacional disponibiliza um interpretador de comandos padrão (`cmd.exe`/`PowerShell` no Windows, `sh`/`bash` no Linux/macOS) para resolver comandos e utilitários da linha de comando.
