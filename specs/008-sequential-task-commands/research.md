# Research: Suporte a Múltiplos Comandos Sequenciais em Tarefas (Sequential Task Commands)

**Branch**: `008-sequential-task-commands` | **Spec**: [spec.md](./spec.md)

## Decisions and Architecture Analysis

### 1. Deserialização de Valor Polimórfico (`string` ou `array de strings`) com System.Text.Json e Native AOT

- **Contexto**: A propriedade `command` de uma tarefa no `nxtl.json` precisa suportar tanto um texto único (ex: `"dotnet build"`) quanto um array de textos (ex: `["dotnet clean", "dotnet build"]`). O projeto utiliza compilação nativa (Native AOT) com geradores de código do `System.Text.Json` (`JsonSerializerContext`), exigindo trim-safety estrita e ausência de reflexão dinâmica em tempo de execução.
- **Decisão**: Criar um `JsonConverter<List<string>>` especializado (ex: `CommandPropertyJsonConverter` ou `StringOrStringListConverter`) aplicado à propriedade `Command` ou `Commands` no `ScriptTaskDto`.
  - Quando o token for `JsonTokenType.String`: lê `reader.GetString()`, retornando uma lista com um único elemento (`new List<string> { value }`).
  - Quando o token for `JsonTokenType.StartArray`: itera pelos itens consumindo cada string até `JsonTokenType.EndArray`. Se um elemento não for string, lança `JsonException`.
  - Quando o token for `JsonTokenType.Null`: retorna uma lista vazia ou nula para que o `ConfigurationValidator` trate como erro amigável em `pt-BR`.
- **Racional**:
  - Totalmente compatível com `JsonSerializerContext` e Native AOT no .NET 10.
  - Elimina necessidade de DTOs intermediários ou objetos dinâmicos.
  - Preserva performance máxima e zero alocações desnecessárias.
- **Alternativas consideradas**:
  - *Usar `JsonElement` puro no DTO e fazer parsing manual no reader*: Funcionaria, mas transfere a responsabilidade de desserialização para a lógica de negócio e impede validações estruturais consistentes no nível do serializer.
  - *Criar uma classe wrapper `CommandDefinition`*: Adicionaria camadas desnecessárias para representar apenas uma lista de strings.

---

### 2. Representação de Comandos no Modelo de Domínio (`ScriptTask`)

- **Contexto**: `ScriptTask` atualmente possui a propriedade `public string Command { get; }` e o método `public string ResolveExecutionCommand()`.
- **Decisão**:
  - Adicionar `public IReadOnlyList<string> Commands { get; }` ao `ScriptTask`.
  - Preservar a propriedade `public string Command => Commands.Count > 0 ? Commands[0] : string.Empty;` para retrocompatibilidade em código existente.
  - Fornecer construtores sobrecarregados:
    - `ScriptTask(string name, IReadOnlyList<string> commands, TaskType type = TaskType.Tool, string? description = null)`
    - `ScriptTask(string name, string command, TaskType type = TaskType.Tool, string? description = null)` (que delega encapsulando o comando em lista de 1 item).
  - Adicionar o método `public IReadOnlyList<string> ResolveExecutionCommands()` que resolve cada item de `Commands` (aplicando `dotnet run` para scripts `.cs` quando o tipo for `TaskType.CSharp`).
  - Manter `public string ResolveExecutionCommand()` retornando a resolução do primeiro comando para compatibilidade.
- **Racional**:
  - Mantém o princípio de isolamento e robustez de domínio.
  - Zero impacto ou quebras em chamadas legadas ou testes unitários prévios de `ScriptTask`.
- **Alternativas consideradas**:
  - *Remover a propriedade `Command`*: Causaria quebra desnecessária em diversas partes do código e testes pré-existentes.

---

### 3. Regras de Execução e Interrupção Imediata no `TaskExecutionCoordinator`

- **Contexto**: Quando uma tarefa possui múltiplos comandos, o usuário precisa ver o progresso passo a passo (`Comando [X/N]: <cmd>`) e a execução deve ser estritamente sequencial. Se qualquer comando falhar (código de saída diferente de 0 ou falha de inicialização de processo/arquivo C# ausente), os comandos posteriores devem ser abortados imediatamente.
- **Decisão**:
  - No método `ExecuteSingleTask`, iterar sobre a lista de comandos resolvidos (`task.ResolveExecutionCommands()`).
  - Imprimir o cabeçalho `Tarefa selecionada: <nome> [<tipo>]` uma única vez no início da tarefa.
  - Para cada comando da lista:
    - Validar existência do arquivo de script se o comando for um script C#.
    - Exibir `Comando [X/N]: <cmd>` se $N > 1$, ou `Comando: <cmd>` se $N == 1$.
    - Invocar o `IProcessExecutor`.
    - Se o resultado possuir código de saída diferente de 0 ou mensagem de erro:
      - Se $N > 1$, exibir `[bold red]Execução interrompida:[/] O comando falhou com código de saída {ExitCode}. Comandos posteriores não serão executados.`
      - Encerrar imediatamente retornando o código de saída do comando que falhou.
  - Se todos os comandos executarem com sucesso, retornar 0.
- **Racional**:
  - Atende 100% aos requisitos de segurança operacional e propagação de exit codes definidos na constituição e na especificação.
  - Mensagens limpas, claras e estilizadas via `Spectre.Console` em `pt-BR`.
- **Alternativas consideradas**:
  - *Encadear comandos no shell nativo (ex: `cmd1 && cmd2`)*: Rejeitado porque dependeria do shell do SO subjacente (cmd, powershell, sh), impediria a validação individual de scripts C# file-scoped pelo .NET e impediria a formatação individualizada do Spectre.Console para cada passo.

---

### 4. Apresentação no Menu Interativo (`InteractiveTaskSelector`)

- **Contexto**: A clarificação definiu que, caso a tarefa possua múltiplos comandos e não possua `description` definida, o rótulo deve exibir `<sem descrição>`.
- **Decisão**:
  - Se `task.Description` estiver preenchido: usar `task.Description`.
  - Se `task.Description` for nulo ou vazio:
    - Se `task.Commands.Count > 1`: usar `"<sem descrição>"`.
    - Se `task.Commands.Count <= 1`: manter `task.Command`.
- **Racional**:
  - Respeita diretamente a decisão tomada com o usuário na etapa de clarificação.
  - Evita poluição visual de múltiplos comandos concatenados na tela de escolha.
- **Alternativas consideradas**:
  - *Concatenar comandos com ` && `*: Rejeitado pelo usuário durante a clarificação.

