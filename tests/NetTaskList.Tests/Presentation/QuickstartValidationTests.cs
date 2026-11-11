using System.Text.RegularExpressions;
using NetTaskList.Tests.Mocks;
using Xunit;

namespace NetTaskList.Tests.Presentation;

public class QuickstartValidationTests
{
    private readonly MockFileSystem _fileSystem = new();
    private readonly MockProcessExecutor _executor = new();
    private readonly MockAnsiConsole _console = new();

    private static string NormalizeConsoleOutput(string input)
    {
        var withoutAnsi = Regex.Replace(input, @"\x1B\[[^@-~]*[@-~]", "");
        return Regex.Replace(withoutAnsi, @"\s+", " ");
    }

    [Fact]
    public void Scenario1_ProjectWithOnlyPackageJson_RunsTestTask()
    {
        var packageJson = """
        {
          "name": "sample-node-project",
          "scripts": {
            "start": "echo Starting Application",
            "test": "echo Running Tests"
          }
        }
        """;
        _fileSystem.AddFile("package.json", packageJson);

        var exitCode = Program.Run(["--run", "test"], _fileSystem, _executor, _console);

        Assert.Equal(0, exitCode);
        Assert.Single(_executor.ExecutedCommands);
        Assert.Equal("echo Running Tests", _executor.ExecutedCommands[0]);
    }

    [Fact]
    public void Scenario2_ProjectWithBothNxtlJsonAndPackageJson_NxtlJsonPrevails()
    {
        var packageJson = """
        {
          "name": "sample-node-project",
          "scripts": {
            "test": "echo Running from package.json"
          }
        }
        """;
        var nxtlJson = """
        {
          "scripts": [
            {
              "name": "test",
              "command": "echo Running from nxtl.json"
            }
          ]
        }
        """;
        _fileSystem.AddFile("package.json", packageJson);
        _fileSystem.AddFile("nxtl.json", nxtlJson);

        var exitCode = Program.Run(["--run", "test"], _fileSystem, _executor, _console);

        Assert.Equal(0, exitCode);
        Assert.Single(_executor.ExecutedCommands);
        Assert.Equal("echo Running from nxtl.json", _executor.ExecutedCommands[0]);
    }

    [Fact]
    public void Scenario3_NpmLifecycleHooks_RunsPreMainPostInOrder()
    {
        var packageJson = """
        {
          "scripts": {
            "prebuild": "echo [PRE-BUILD]",
            "build": "echo [MAIN-BUILD]",
            "postbuild": "echo [POST-BUILD]"
          }
        }
        """;
        _fileSystem.AddFile("package.json", packageJson);

        var exitCode = Program.Run(["--run", "build"], _fileSystem, _executor, _console);

        Assert.Equal(0, exitCode);
        Assert.Equal(3, _executor.ExecutedCommands.Count);
        Assert.Equal("echo [PRE-BUILD]", _executor.ExecutedCommands[0]);
        Assert.Equal("echo [MAIN-BUILD]", _executor.ExecutedCommands[1]);
        Assert.Equal("echo [POST-BUILD]", _executor.ExecutedCommands[2]);
    }

    [Fact]
    public void Scenario4_FailingPreHook_AbortsAndReturnsExitCode()
    {
        var packageJson = """
        {
          "scripts": {
            "prebuild": "exit 1",
            "build": "echo [MAIN-BUILD]",
            "postbuild": "echo [POST-BUILD]"
          }
        }
        """;
        _fileSystem.AddFile("package.json", packageJson);
        _executor.EnqueueExitCode(1);

        var exitCode = Program.Run(["--run", "build"], _fileSystem, _executor, _console);

        Assert.Equal(1, exitCode);
        Assert.Single(_executor.ExecutedCommands);
        Assert.Equal("exit 1", _executor.ExecutedCommands[0]);
    }

    [Fact]
    public void Scenario5_NeitherConfigFileFound_ReturnsExitCode1WithPortugueseDiagnostics()
    {
        var exitCode = Program.Run([], _fileSystem, _executor, _console);

        Assert.Equal(1, exitCode);
        Assert.Contains("Nenhum arquivo de configuração", _console.Output);
        Assert.Contains("nxtl.json", _console.Output);
        Assert.Contains("package.json", _console.Output);
        Assert.Contains("nxtl --init", _console.Output);
    }

    [Fact]
    public void ShortcutScenario1_SingleTaskShortcut_ExecutesSuccessfully()
    {
        var nxtlJson = """
        {
          "scripts": [
            { "name": "say-hello", "command": "echo Olá Mundo", "type": "tool" },
            { "name": "cumprimentar", "command": { "task": "say-hello" }, "type": "tool" }
          ]
        }
        """;
        _fileSystem.AddFile("nxtl.json", nxtlJson);

        var exitCode = Program.Run(["--run", "cumprimentar"], _fileSystem, _executor, _console);

        Assert.Equal(0, exitCode);
        Assert.Single(_executor.ExecutedCommands);
        Assert.Equal("echo Olá Mundo", _executor.ExecutedCommands[0]);
        Assert.Contains("Executando atalho:", _console.Output);
        Assert.Contains("say-hello", _console.Output);
    }

    [Fact]
    public void ShortcutScenario2_MixedSequentialWithShortcuts_ExecutesInOrder()
    {
        var nxtlJson = """
        {
          "scripts": [
            { "name": "step-clean", "command": "echo Limpando artefatos", "type": "tool" },
            { "name": "step-build", "command": "echo Compilando binarios", "type": "tool" },
            {
              "name": "pipeline",
              "command": [
                "echo [Inicio]",
                { "task": "step-clean" },
                { "task": "step-build" },
                "echo [Fim]"
              ],
              "type": "tool"
            }
          ]
        }
        """;
        _fileSystem.AddFile("nxtl.json", nxtlJson);

        var exitCode = Program.Run(["--run", "pipeline"], _fileSystem, _executor, _console);

        Assert.Equal(0, exitCode);
        Assert.Equal(4, _executor.ExecutedCommands.Count);
        Assert.Equal("echo [Inicio]", _executor.ExecutedCommands[0]);
        Assert.Equal("echo Limpando artefatos", _executor.ExecutedCommands[1]);
        Assert.Equal("echo Compilando binarios", _executor.ExecutedCommands[2]);
        Assert.Equal("echo [Fim]", _executor.ExecutedCommands[3]);
    }

    [Fact]
    public void ShortcutScenario3_DirectCycleTwoTasks_HaltsBeforeExecutionWithLoopMessage()
    {
        var nxtlJson = """
        {
          "scripts": [
            { "name": "build", "command": { "task": "clean" }, "type": "tool" },
            { "name": "clean", "command": { "task": "build" }, "type": "tool" }
          ]
        }
        """;
        _fileSystem.AddFile("nxtl.json", nxtlJson);

        var exitCode = Program.Run(["--run", "build"], _fileSystem, _executor, _console);

        Assert.Equal(1, exitCode);
        Assert.Empty(_executor.ExecutedCommands);
        Assert.Contains("Referência circular detectada entre tarefas:", _console.Output);
        Assert.Contains("build -> clean -> build", _console.Output);
        Assert.Contains("loop infinito no nxtl", _console.Output);
    }

    [Fact]
    public void ShortcutScenario4_DeepTreeCycle_HaltsBeforeExecutionWithLoopMessage()
    {
        var nxtlJson = """
        {
          "scripts": [
            { "name": "task-a", "command": { "task": "task-b" }, "type": "tool" },
            { "name": "task-b", "command": { "task": "task-c" }, "type": "tool" },
            { "name": "task-c", "command": { "task": "task-a" }, "type": "tool" }
          ]
        }
        """;
        _fileSystem.AddFile("nxtl.json", nxtlJson);

        var exitCode = Program.Run(["--run", "task-a"], _fileSystem, _executor, _console);

        Assert.Equal(1, exitCode);
        Assert.Empty(_executor.ExecutedCommands);
        var normalized = NormalizeConsoleOutput(_console.Output);
        Assert.Contains("Referência circular detectada entre tarefas:", normalized);
        Assert.Contains("task-a -> task-b -> task-c -> task-a", normalized);
        Assert.Contains("loop infinito no nxtl", normalized);
    }

    [Fact]
    public void ShortcutScenario5_DiamondDAG_ExecutesWithoutFalseCycle()
    {
        var nxtlJson = """
        {
          "scripts": [
            { "name": "shared-seed", "command": "echo preparando banco", "type": "tool" },
            {
              "name": "unit-tests",
              "command": [
                { "task": "shared-seed" },
                "echo rodando testes unitarios"
              ],
              "type": "tool"
            },
            {
              "name": "integration-tests",
              "command": [
                { "task": "shared-seed" },
                "echo rodando testes integrados"
              ],
              "type": "tool"
            },
            {
              "name": "test-all",
              "command": [
                { "task": "unit-tests" },
                { "task": "integration-tests" }
              ],
              "type": "tool"
            }
          ]
        }
        """;
        _fileSystem.AddFile("nxtl.json", nxtlJson);

        var exitCode = Program.Run(["--run", "test-all"], _fileSystem, _executor, _console);

        Assert.Equal(0, exitCode);
        Assert.Equal(4, _executor.ExecutedCommands.Count);
        Assert.Equal("echo preparando banco", _executor.ExecutedCommands[0]);
        Assert.Equal("echo rodando testes unitarios", _executor.ExecutedCommands[1]);
        Assert.Equal("echo preparando banco", _executor.ExecutedCommands[2]);
        Assert.Equal("echo rodando testes integrados", _executor.ExecutedCommands[3]);
    }

    [Fact]
    public void ShortcutScenario6_MissingTargetAndCSharpTarget_HaltsWithExitCode1()
    {
        var nxtlJson = """
        {
          "scripts": [
            { "name": "tarefa-invalida", "command": { "task": "fantasma" }, "type": "tool" },
            { "name": "meu-script-cs", "command": "scripts/clean.cs", "type": "csharp" },
            { "name": "chamar-csharp", "command": { "task": "meu-script-cs" }, "type": "tool" }
          ]
        }
        """;
        _fileSystem.AddFile("nxtl.json", nxtlJson);

        var exitCode1 = Program.Run(["--run", "tarefa-invalida"], _fileSystem, _executor, _console);
        Assert.Equal(1, exitCode1);
        Assert.Contains("A tarefa referenciada", _console.Output);
        Assert.Contains("fantasma", _console.Output);
        Assert.Contains("não foi encontrada", _console.Output);

        var console2 = new MockAnsiConsole();
        var exitCode2 = Program.Run(["--run", "chamar-csharp"], _fileSystem, _executor, console2);
        Assert.Equal(1, exitCode2);
        var normalized2 = console2.Output.Replace("\r", "").Replace("\n", "");
        Assert.Contains("tenta referenciar", normalized2);
        Assert.Contains("meu-script-cs", normalized2);
        Assert.Contains("que é um script C#", normalized2);
    }
}
