# Technical Research: Melhoria do Output de Execução e Navegação de Tarefas

**Feature**: `004-improve-execution-output` | **Date**: 2026-11-01

Este documento registra as decisões técnicas, alternativas consideradas e padrões adotados para atender aos requisitos de apresentação do terminal do `nxtl`.

---

## 1. Obtenção Dinâmica da Versão da Aplicação (.NET 10 & Native AOT)

### Contexto
O requisito FR-002 e FR-004 demandam que a ferramenta exiba `"nxtl versão <código da versão atual>"` de maneira dinâmica, evitando números de versão *hardcoded* espalhados pelo código e assegurando compatibilidade estrita com compilação Native AOT (`<PublishAot>true</PublishAot>`).

### Decisão
Criar uma classe utilitária de apresentação/domínio `AppVersion` em `src/NetTaskList/Presentation/AppVersion.cs` (ou propriedade estática utilitária) que resolve a versão através de `typeof(Program).Assembly.GetName().Version` ou `AssemblyInformationalVersionAttribute`.

```csharp
public static class AppVersion
{
    public static string Current { get; } = ResolveVersion();

    private static string ResolveVersion()
    {
        var assembly = typeof(Program).Assembly;
        var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(infoVersion))
        {
            // Remove metadados de commit (ex: 1.0.0+sha -> 1.0.0)
            return infoVersion.Split('+')[0];
        }

        var version = assembly.GetName().Version;
        return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";
    }
}
```

### Justificativa
- **Compatibilidade Native AOT**: `typeof(Program).Assembly.GetName().Version` e leitura de atributos de metadados de assembly são suportados sem avisos de reflexão no .NET 10.
- **Resolução Única e Centralizada**: Permite reutilização tanto no comando `nxtl --version` quanto no novo cabeçalho da listagem de execução, garantindo coerência sem divergência entre versões.
- **Tratamento de Build Metadata**: Ao descartar o sufixo `+<commit-hash>` gerado automaticamente pelo SDK do .NET, a exibição no terminal permanece limpa (ex: `1.0.0`).

### Alternativas Consideradas
- *Hardcoding da string "1.0.0"*: Rejeitado por violar o requisito FR-004 e exigir sincronização manual a cada novo release.
- *Leitura direta do nxtl.csproj em tempo de execução*: Inviável em produção, pois o `.csproj` não existe no ambiente do usuário final da ferramenta global.

---

## 2. Remoção da Tabela Estática de Tarefas

### Contexto
Anteriormente, o `Program.cs` invocava `TaskTableRenderer.Render(configuration.Tasks, ansiConsole)` logo antes de abrir o seletor interativo. Isso duplicava visualmente as tarefas na tela e empurrava o seletor para baixo da área visível do terminal.

### Decisão
Remover a chamada a `TaskTableRenderer.Render` em `Program.cs` no fluxo padrão de execução e remover a classe `TaskTableRenderer.cs` (ou descontinuá-la), pois não há utilidade ou comandos adicionais (`--list`) dependendo desse componente no momento, respeitando o princípio de desenvolvimento enxuto (Princípio IV - KISS / YAGNI).

### Justificativa
- A tabela estática exibia as mesmas tarefas que o `InteractiveTaskSelector` lista com interatividade, ícones e descrições.
- A exclusão do arquivo ou remoção da chamada elimina código morto e reduz o tamanho do binário compilado com Native AOT.

### Alternativas Consideradas
- *Manter a tabela condicionada a uma flag `--table`*: Rejeitado, pois não foi solicitado nos requisitos e adicionaria complexidade de CLI desnecessária neste momento.

---

## 3. Configuração de Viewport e Paginação no `SelectionPrompt` (Spectre.Console)

### Contexto
O `Spectre.Console` permite definir o número máximo de itens visíveis simultaneamente através do método `.PageSize(int)`. O valor anterior era 15, o que em telas menores ou com tarefas detalhadas ocupava altura excessiva.

### Decisão
Alterar a chamada em `InteractiveTaskSelector.cs`:
```csharp
var prompt = new SelectionPrompt<TaskChoice>()
    .Title("[cyan]Selecionar uma tarefa para executar[/]")
    .PageSize(8)
    .MoreChoicesText("[grey](Mova para cima e para baixo para ver mais opções)[/]")
    .UseConverter(choice => choice.DisplayName);
```

### Justificativa
- `.PageSize(8)` atende perfeitamente ao requisito "Na listagem de tarefas, deixar visível apenas 8 tarefas".
- O controle nativo de paginação do `Spectre.Console` gerencia automaticamente a rolagem, a visibilidade da opção `[Sair]` e a mensagem `MoreChoicesText` em `pt-BR`.

### Alternativas Consideradas
- *Paginação manual de lista*: Rejeitado, pois `SelectionPrompt.PageSize` nativo do `Spectre.Console` é robusto, testado e suporta interações de terminal com teclado.

---

## 4. Estilização do Título do Prompt em Ciano

### Contexto
O título anterior era `[bold yellow]Selecione uma tarefa para executar:[/]` com dois pontos e cor amarela.
O requisito solicita: Transformar o texto "Selecionar uma tarefa para executar" em cor ciano.

### Decisão
Definir o título como `[cyan]Selecionar uma tarefa para executar[/]`.

### Justificativa
- Conformidade estrita com o texto e cor solicitados pelo usuário.
- O ciano (`[cyan]`) oferece contraste agradável e harmoniza com as diretrizes do Spectre.Console já utilizadas na aplicação para ações e comandos.

---

## 5. Ordem de Renderização no Fluxo Principal (`Program.cs`)

### Contexto
Ordem dos elementos na saída do terminal para evitar quebra de fluxo ou inconsistência de apresentação.

### Decisão
No fluxo sem argumentos (após validação de tarefas existentes e checagem de `--run`):
1. Renderizar cabeçalho: `ansiConsole.MarkupLine($"[green]nxtl versão {Markup.Escape(AppVersion.Current)}[/]");`
2. Checar se a entrada está redirecionada (`Console.IsInputRedirected`). Se sim, exibir aviso e finalizar.
3. Chamar `InteractiveTaskSelector.SelectTask(configuration.Tasks, ansiConsole)`.

### Justificativa
O cabeçalho de versão antecede imediatamente o prompt interativo. Em ambientes não-interativos (redirecionamento de pipe/CI), o usuário ainda recebe a identificação da versão seguida da mensagem explicativa orientando o uso de `--run`.
