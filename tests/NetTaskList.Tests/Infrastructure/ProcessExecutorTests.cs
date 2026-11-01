using System.Diagnostics;
using NetTaskList.Infrastructure;
using Xunit;

namespace NetTaskList.Tests.Infrastructure;

public class ProcessExecutorTests
{
    [Fact]
    public void GetShellCommand_WhenWindows_ReturnsCmdExeWithSlashC()
    {
        // Arrange
        var command = "echo \"Hello World\"";

        // Act
        var (fileName, arguments) = ProcessExecutor.GetShellCommand(command, isWindows: true);

        // Assert
        Assert.True(fileName.EndsWith("cmd.exe", StringComparison.OrdinalIgnoreCase) || fileName.Equals("cmd.exe", StringComparison.OrdinalIgnoreCase));
        Assert.Equal($"/c {command}", arguments);
    }

    [Fact]
    public void GetShellCommand_WhenUnix_ReturnsBinShWithEscapedDashC()
    {
        // Arrange
        var command = "echo \"Hello World\"";

        // Act
        var (fileName, arguments) = ProcessExecutor.GetShellCommand(command, isWindows: false);

        // Assert
        Assert.Equal("/bin/sh", fileName);
        Assert.Equal("-c \"echo \\\"Hello World\\\"\"", arguments);
    }

    [Fact]
    public void CreateStartInfo_ConfiguresDirectConsoleHandlesAndZeroBuffering()
    {
        // Arrange
        var fileName = "cmd.exe";
        var arguments = "/c echo test";
        var workingDir = "D:\\Projetos\\Test";

        // Act
        var startInfo = ProcessExecutor.CreateStartInfo(fileName, arguments, workingDir);

        // Assert
        Assert.Equal(fileName, startInfo.FileName);
        Assert.Equal(arguments, startInfo.Arguments);
        Assert.Equal(workingDir, startInfo.WorkingDirectory);
        Assert.False(startInfo.UseShellExecute);
        Assert.False(startInfo.RedirectStandardInput);
        Assert.False(startInfo.RedirectStandardOutput);
        Assert.False(startInfo.RedirectStandardError);
        Assert.False(startInfo.CreateNoWindow);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Execute_WhenCommandLineIsNullOrWhiteSpace_ReturnsFailure(string? command)
    {
        // Arrange
        var executor = new ProcessExecutor();

        // Act
        var result = executor.Execute(command!);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(1, result.ExitCode);
        Assert.Contains("Nenhum comando", result.ErrorMessage);
    }
}
