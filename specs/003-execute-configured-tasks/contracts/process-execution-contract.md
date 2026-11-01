# Process Execution Contract: `IProcessExecutor`

**Feature**: `003-execute-configured-tasks` | **Date**: 2026-11-01

Este contrato define a interface pública, as garantias de comportamento e os detalhes de invocação do executor de processos do NetTaskList (`nxtl`).

---

## 1. Interface C# (`IProcessExecutor`)

```csharp
namespace NetTaskList.Infrastructure.Abstractions;

/// <summary>
/// Abstraction for operating system process execution to enable testability and decoupling.
/// </summary>
public interface IProcessExecutor
{
    /// <summary>
    /// Executes a command line in the host environment, attaching terminal streams and waiting for completion.
    /// </summary>
    /// <param name="commandLine">The command string to execute.</param>
    /// <param name="workingDirectory">Optional working directory; defaults to current directory if null.</param>
    /// <returns>A result containing the exit code and potential error diagnostics.</returns>
    ProcessExecutionResult Execute(string commandLine, string? workingDirectory = null);
}
```

---

## 2. Modelo de Resultado (`ProcessExecutionResult`)

```csharp
namespace NetTaskList.Domain.Models;

/// <summary>
/// Represents the outcome of an operating system process execution.
/// </summary>
public sealed class ProcessExecutionResult
{
    public ProcessExecutionResult(int exitCode, string? errorMessage = null)
    {
        ExitCode = exitCode;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// The numeric exit code returned by the executed process.
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    /// True if the process completed with exit code 0 and no launch error occurred.
    /// </summary>
    public bool Success => ExitCode == 0 && ErrorMessage == null;

    /// <summary>
    /// Diagnostic message if the process failed to start or threw an OS exception.
    /// </summary>
    public string? ErrorMessage { get; }

    public static ProcessExecutionResult FromExitCode(int exitCode) => new(exitCode);

    public static ProcessExecutionResult Failure(string errorMessage, int exitCode = 1) =>
        new(exitCode, errorMessage);
}
```

---

## 3. Comportamento Operacional da Implementação (`ProcessExecutor`)

### 3.1 Resolução do Interpretador de Comandos (Shell Resolution)

Para garantir que comandos normais funcionem exatamente como se tivessem sido digitados no terminal (suportando comandos encadeados com `&&`, pipes `|`, variáveis de ambiente e arquivos executáveis sem extensão explícita):

| Plataforma | Executável Primário | Argumentos Formatados |
|------------|---------------------|-----------------------|
| **Windows** | `Environment.GetEnvironmentVariable("COMSPEC") ?? "cmd.exe"` | `/c "{commandLine}"` |
| **Linux / macOS** | `/bin/sh` | `-c "{commandLine}"` |

### 3.2 Configuração de ProcessStartInfo

```csharp
var startInfo = new ProcessStartInfo
{
    FileName = shellExecutable,
    Arguments = shellArguments,
    WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
    UseShellExecute = false,
    RedirectStandardInput = false,
    RedirectStandardOutput = false,
    RedirectStandardError = false,
    CreateNoWindow = false
};
```

### 3.3 Garantias de Fluxo e Sincronização

1. **Herança Direta de Terminal**:
   - `UseShellExecute = false` combinado com a ausência de redirecionamento (`Redirect* = false`) conecta os descritores de console padrão diretamente ao processo filho.
   - Preserva total fidelidade para:
     - Cores ANSI e formatação de texto em tempo real.
     - Entrada de teclado para prompts interativos (ex: respostas Y/N, senhas, seleções).
     - Atualizações de linha (caracteres `\r`, spinners e barras de progresso).
2. **Sincronismo**:
   - O método `Execute` bloqueia sincronamente através de `process.WaitForExit()` até que o processo filho tenha encerrado completamente.
3. **Tratamento de Exceções**:
   - Se `Process.Start()` falhar (ex: executável de shell não encontrado, privilégios insuficientes), captura a exceção (`Win32Exception` ou `Exception`) e retorna `ProcessExecutionResult.Failure(ex.Message, exitCode: 1)`.
