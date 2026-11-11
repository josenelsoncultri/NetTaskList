# Contract: Task Execution & Recursion Prevention Behavior

**Branch**: `009-task-command-shortcuts` | **Spec**: [spec.md](../spec.md)

Este contrato estabelece o comportamento padronizado do terminal (`Spectre.Console`), mensagens de status, detecção de loops de recursão e propagação de códigos de saída ao executar tarefas contendo atalhos no `nxtl`.

---

## 1. Validação Prévia (Upfront) e Mensagens de Erro

Antes de iniciar a execução do primeiro comando da tarefa selecionada, o `TaskExecutionCoordinator` valida recursivamente a árvore de atalhos. Caso seja identificada qualquer inconsistência, a execução é interrompida com código de saída `1` e nenhuma instrução do sistema operacional é disparada.

### A. Referência Circular / Loop Infinito
Quando uma tarefa referenciar a si mesma ou houver ciclos diretos ou indiretos na árvore:

```text
[bold red]Erro:[/] Referência circular detectada entre tarefas: [yellow]<cadeia-de-recursão>[/]. A execução causaria um loop infinito no nxtl.
```

**Exemplos de Cadeias Formatadas**:
- Auto-referência: `build -> build`
- Ciclo direto de duas tarefas: `clean -> build -> clean`
- Ciclo indireto em árvore: `all -> pipeline -> build -> clean -> build`

### B. Tarefa Referenciada Não Encontrada
Quando o nome especificado em `{ "task": "<nome>" }` não existe no arquivo de configuração:

```text
[bold red]Erro:[/] A tarefa referenciada '[yellow]<nome-inexistente>[/]' não foi encontrada no arquivo de configuração.
```

### C. Tentativa de Referenciar Tarefa C#
Quando o atalho apontar para uma tarefa com `type: "csharp"`:

```text
[bold red]Erro:[/] A tarefa '[yellow]<tarefa-origem>[/]' tenta referenciar '[yellow]<tarefa-destino>[/]', que é um script C#. Tarefas C# não podem ser referenciadas como atalhos.
```

---

## 2. Apresentação Visual no Terminal Durante a Execução

Quando a árvore de tarefas é válida, a execução segue de forma determinística e sequencial.

### Indicador de Ativação de Atalho
Ao atingir uma etapa que é um atalho para outra tarefa, o `nxtl` imprime:

```text
[bold cyan]Executando atalho:[/] [bold white]<nome-do-atalho>[/]
```

### Exemplo Completo de Saída no Terminal (Sucesso)

Dado o comando `nxtl --run rebuild` onde `rebuild` chama o atalho `clean` e depois executa `dotnet build`:

```text
Tarefa selecionada: rebuild [[tool]]
Executando atalho: clean
Comando: dotnet clean
Comando [2/2]: dotnet build
```

---

## 3. Propagação de Falhas e Interrupção Imediata

Se qualquer comando dentro de uma tarefa referenciada (ou na tarefa principal) falhar com código de saída diferente de zero:
1. O erro de processo é impresso imediatamente:
   ```text
   [bold red]Erro ao executar tarefa:[/] <mensagem-de-erro>
   ```
2. A execução é abortada, prevenindo a execução de passos posteriores tanto da subtarefa quanto da tarefa chamadora:
   ```text
   [bold red]Execução interrompida:[/] O comando falhou com código de saída <exitCode>. Comandos posteriores não serão executados.
   ```
3. O código de saída exato do processo com falha é retornado como exit code do processo `nxtl`.
