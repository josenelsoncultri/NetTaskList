# Console Alert Contract: Update Notification Panel

**Component**: `NetTaskList.Presentation.UpdateNotificationPresenter`  
**Target UI**: `Spectre.Console.IAnsiConsole`  
**Language**: Português do Brasil (`pt-BR`)

---

## 1. Visual Presentation Specification

Quando `VersionCheckResult.HasUpdate == true`, o painel DEVE ser renderizado no topo absoluto da saída do console com as seguintes características:

- **Componente**: `Spectre.Console.Panel`
- **Borda**: `BoxBorder.Rounded` (cantos arredondados)
- **Cor da Borda**: `Color.Yellow` (estilo amarelo destacado)
- **Padding**: `Padding(1, 0, 1, 0)`

### Formato do Conteúdo (Spectre Markup):
```text
[bold yellow]Nova versão disponível:[/] [white]{currentVersion}[/] → [bold green]{latestVersion}[/]
Para atualizar a ferramenta globalmente, execute:
[bold cyan]dotnet tool update -g nettasklist[/]
```

### Exemplo Visual Renderizado:
```text
╭────────────────────────────────────────────────────────────╮
│ Nova versão disponível: 1.6.1 → 1.7.0                      │
│ Para atualizar a ferramenta globalmente, execute:          │
│ dotnet tool update -g nettasklist                          │
╰────────────────────────────────────────────────────────────╯
```

---

## 2. Order of Execution & Placement Contract

1. **Topo Absoluto**: O painel é renderizado antes de qualquer outra linha de saída do console (antes do cabeçalho de tarefas, mensagens de configuração ou execução de comandos).
2. **Modo Interativo (`nxtl`)**: Exibe o painel de alerta no topo, seguido de uma linha em branco, e então o cabeçalho `nxtl versão X.Y.Z` e o seletor interativo.
3. **Modo Execução Direta (`nxtl --run <task>`)**: Exibe o painel de alerta no topo, seguido de uma linha em branco, e então prossegue diretamente com o disparo dos comandos da tarefa.
4. **Exclusões Informativas**:
   - `nxtl --version` / `nxtl -v`: NÃO renderiza o painel nem faz verificação de rede.
   - `nxtl --help` / `nxtl -h`: NÃO renderiza o painel nem faz verificação de rede.
