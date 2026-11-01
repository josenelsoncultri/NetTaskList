# Technical Research: Mecanismo de Execução de Tarefas e Scripts Configurados

**Feature**: `003-execute-configured-tasks` | **Date**: 2026-11-01

Este documento registra as decisões de arquitetura e tecnologia para o mecanismo de execução de processos e scripts configurados no NetTaskList (`nxtl`), atendendo a todos os princípios da Constituição do projeto e aos requisitos da especificação (`spec.md`).

---

## Decisão 1: Estratégia de Execução de Processos no Sistema Operacional

### Contexto
Tarefas configuradas no `nxtl.json` podem ser de dois tipos:
1. `tool`: Comandos de terminal arbitrários (ex: `dotnet build`, `npm test`, `git status`, comandos encadeados com `&&` ou pipelines).
2. `csharp`: Scripts C# file-scoped (ex: `scripts/seed.cs` ou `dotnet run scripts/seed.cs`).

### Opções Avaliadas
- **Opção A (Execução Direta de Binário sem Shell)**: Invocar `Process.Start` passando o primeiro token como binário e os demais como argumentos.
  - *Prós*: Não depende de shell intermediário.
  - *Contras*: Não suporta nativamente scripts batch (`.cmd`/`.bat`) sem extensão explícita no Windows, comandos compostos (`&&`, `||`), redirecionamentos de shell, nem comandos internos do shell (`dir`, `echo`, etc.).
- **Opção B (Execução via Shell do Sistema Operacional)**: Executar comandos através do interpretador de comandos nativo da plataforma (`cmd.exe /c` no Windows, `sh -c` no Linux/macOS).
  - *Prós*: Compatibilidade total com a forma como desenvolvedores rodam tarefas no terminal (idêntico a `npm run`, `make`, `cargo-make`); suporte a comandos compostos, variáveis de ambiente e arquivos executáveis no `PATH`.
  - *Contras*: Cria um processo de shell intermediário.
- **Opção C (Estratégia Híbrida / Otimizada)**: Para comandos `csharp` resolvidos (que sempre iniciam com `dotnet run ...`), invocar diretamente o executável `dotnet` com a lista de argumentos para evitar overhead desnecessário de shell; para comandos `tool`, invocar via shell nativo (`cmd.exe /c` no Windows, `sh -c` no Unix/macOS) garantindo fidelidade de terminal.

### Decisão
**Opção C (Híbrida / Otimizada)**:
- Para tarefas `tool`, a execução é despachada através do shell do sistema (`cmd.exe /c` no Windows, `/bin/sh -c` no Linux/macOS), cumprindo o requisito de *"rodar apenas o texto do comando normalmente, como se estivesse rodando diretamente pelo terminal sem o uso da ferramenta"*.
- Para tarefas `csharp`, quando o comando é resolvido para `dotnet run <arquivo.cs> [args]`, o sistema pode despachar diretamente o executável `dotnet` ou delegar ao executor de linha de comando com herança direta de console. Para garantir uniformidade de resolução e suporte a argumentos customizados, o `IProcessExecutor` oferece método que recebe a linha de comando completa e lida com o despacho correto.

---

## Decisão 2: Conexão e Fidelidade dos Fluxos de Terminal (stdin, stdout, stderr)

### Contexto
O requisito FR-004 e a Constituição exigem que os fluxos de entrada e saída sejam repassados em tempo real, sem perdas de cores ANSI e preservando a interatividade do console.

### Opções Avaliadas
- **Opção A (Redirecionamento com Buffering e Leitura Assíncrona)**: Configurar `RedirectStandardOutput = true`, ler via streams e reescrever no `Console.Out`.
  - *Contras*: Introduz latência de buffer, perde códigos de escape de terminal interativo (como requisições de tamanho de janela, modo RAW), quebra prompts interativos do processo filho no `stdin`, e consome threads adicionais.
- **Opção B (Herança Direta de Descritores de Console - Direct Console Attachment)**: Configurar `UseShellExecute = false`, `RedirectStandardInput = false`, `RedirectStandardOutput = false`, `RedirectStandardError = false`.
  - *Prós*: O processo filho herda diretamente os descritores de console nativos do processo pai (`conin$`, `conout$`, `conerr$` no Windows; descritores 0, 1, 2 no POSIX):
    - Zero latência e zero alocação de memória intermediária.
    - 100% de compatibilidade com cores ANSI, tabelas, spinners e prompts interativos de qualquer ferramenta (ex: `dotnet watch`, `npm`, `git rebase -i`, prompts interativos).
    - Teclas como setas, backspace e Ctrl+C funcionam nativamente no processo filho.
  - *Contras*: A ferramenta `nxtl` não intercepta o texto do processo filho (o que é exatamente o comportamento desejado).

### Decisão
**Opção B (Herança Direta de Descritores)**: `RedirectStandardInput = false`, `RedirectStandardOutput = false`, `RedirectStandardError = false`, permitindo que o processo filho converse diretamente com o console do usuário.

---

## Decisão 3: Desacoplamento e Testabilidade (`IProcessExecutor`)

### Contexto
O Princípio III da Constituição estabelece:
> *"Business logic MUST remain decoupled from console I/O (`Spectre.Console`) and direct OS process invocations via appropriate abstractions (interfaces/adapters) to ensure full mockability and testability."*

### Decisão
Criar a interface `IProcessExecutor` no namespace `NetTaskList.Infrastructure.Abstractions` e sua implementação padrão `ProcessExecutor` no namespace `NetTaskList.Infrastructure`:

```csharp
public interface IProcessExecutor
{
    ProcessExecutionResult Execute(string commandLine, string? workingDirectory = null);
}

public sealed class ProcessExecutionResult
{
    public int ExitCode { get; }
    public bool Success => ExitCode == 0;
    public string? ErrorMessage { get; }
    
    public static ProcessExecutionResult FromExitCode(int exitCode) => new(exitCode, null);
    public static ProcessExecutionResult Failure(string errorMessage, int exitCode = 1) => new(exitCode, errorMessage);
}
```

Para os testes unitários, será criada a classe `MockProcessExecutor` no projeto de testes (`NetTaskList.Tests.Mocks`), permitindo simular execuções com sucesso, códigos de saída de erro específicos (ex: 1, 42, 127) e falhas simuladas de inicialização de processo.

---

## Decisão 4: Validação Prévia de Scripts C#

### Contexto
Quando uma tarefa é configurada como `csharp` com caminho de arquivo (ex: `scripts/seed.cs`), o usuário pode apontar para um arquivo inexistente ou digitado incorretamente.

### Decisão
Antes de disparar o processo `dotnet run`, o executor (ou serviço de coordenação) verifica via `IFileSystem.FileExists` se o alvo do script existe quando o comando é um caminho de arquivo `.cs`. Caso não exista, o `nxtl` exibe imediatamente uma mensagem amigável em português:
`[bold red]Erro:[/] O arquivo de script C# '[yellow]{caminho}[/]' não foi encontrado.`
Isso evita o tempo de inicialização do compilador .NET para falhar com um erro genérico e oferece diagnóstico imediato.

---

## Decisão 5: Propagação de Sinal de Interrupção (Ctrl+C / Cancelamento)

### Contexto
O usuário pode pressionar Ctrl+C enquanto um script de longa duração estiver em execução.

### Decisão
Com herança direta de console, o sistema operacional transmite o sinal de cancelamento (SIGINT / `CTRL_C_EVENT`) diretamente para o grupo de processos do console. O processo pai (`nxtl`) aguarda o término do processo filho através de `process.WaitForExit()`, capturando seu código de saída resultante (tipicamente código não-zero de aborto no Windows/Linux) e encerrando de forma limpa sem deixar processos órfãos.
