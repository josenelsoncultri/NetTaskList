# Contract: Task Execution & Interruption Protocol

**Branch**: `008-sequential-task-commands` | **Spec**: [spec.md](../spec.md)

Este contrato formaliza a coordenação de execução sequencial, formatação no terminal com `Spectre.Console`, interrupção e propagação de códigos de saída (`exit code`).

---

## 1. Protocolo de Formatação Visual no Terminal

### 1.1 Abertura da Tarefa
O cabeçalho de seleção da tarefa é emitido uma única vez no início da execução:
```text
Tarefa selecionada: [bold white]{task.Name}[/] [[[{badgeColor}]{badgeText}[/]]]
```
Onde:
- Se `task.Type == TaskType.CSharp`: `badgeColor = "green"`, `badgeText = "csharp"`
- Se `task.Type == TaskType.Tool`: `badgeColor = "cyan"`, `badgeText = "tool"`

### 1.2 Etapas de Execução do Comando
Para cada comando da lista:
- **Caso $N == 1$ (Comando único)**:
  ```text
  Comando: [bold yellow]{commandLine}[/]
  ```
- **Caso $N > 1$ (Múltiplos comandos)**:
  ```text
  Comando [{X}/{N}]: [bold yellow]{commandLine}[/]
  ```
  *(Onde $X$ varia de 1 a $N$, correspondendo ao índice 1-based da etapa).*

---

## 2. Protocolo de Erros e Interrupção

### 2.1 Falha de Execução de Processo
Se a execução do comando resultar em `ExitCode != 0` ou gerar `ErrorMessage`:
1. Exibir o erro do processo:
   ```text
   [bold red]Erro ao executar tarefa:[/] {ErrorMessage}
   ```
2. Se $N > 1$, exibir imediatamente a mensagem de interrupção:
   ```text
   [bold red]Execução interrompida:[/] O comando falhou com código de saída {ExitCode}. Comandos posteriores não serão executados.
   ```
3. Não executar nenhum dos comandos subsequentes.
4. Encerrar imediatamente a execução retornando o código de saída do processo com falha (`ExitCode`).

### 2.2 Falha de Arquivo C# Inexistente
Se uma tarefa do tipo `csharp` fizer referência a um arquivo `.cs` inexistente:
1. Exibir a mensagem diagnóstica:
   ```text
   [bold red]Erro:[/] O arquivo de script C# '[yellow]{rawCommand}[/]' não foi encontrado.
   ```
2. Se $N > 1$, exibir a mensagem de interrupção:
   ```text
   [bold red]Execução interrompida:[/] Comandos posteriores não serão executados.
   ```
3. Interromper imediatamente e retornar código de saída `1`.

---

## 3. Protocolo de Apresentação no Menu Interativo

Para tarefas exibidas no menu de seleção interativa:
- Se `task.Description` não for vazia:
  `{task.Name} [{typeTag}] - {task.Description}`
- Se `task.Description` for vazia/nula:
  - Se $N == 1$: `{task.Name} [{typeTag}] - {task.Command}`
  - Se $N > 1$: `{task.Name} [{typeTag}] - <sem descrição>`

