namespace NetTaskList.Presentation;

public sealed record CommandLineOptions
{
    public string? ConfigPath { get; init; }
    public string? RunTaskName { get; init; }
    public bool Init { get; init; }
    public bool ShowHelp { get; init; }
    public bool ShowVersion { get; init; }
    public bool IsValid { get; init; } = true;
    public string? ErrorMessage { get; init; }
}

public static class CommandLineParser
{
    public static CommandLineOptions Parse(string[] args)
    {
        string? configPath = null;
        string? runTaskName = null;
        bool init = false;
        bool showHelp = false;
        bool showVersion = false;

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg.Equals("--run", StringComparison.OrdinalIgnoreCase) ||
                arg.Equals("-r", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 >= args.Length || args[i + 1].StartsWith('-'))
                {
                    return new CommandLineOptions
                    {
                        IsValid = false,
                        ErrorMessage = "A opção '--run' requer o nome da tarefa a ser executada."
                    };
                }

                runTaskName = args[++i];
            }
            else if (arg.Equals("--config", StringComparison.OrdinalIgnoreCase) ||
                     arg.Equals("-c", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 >= args.Length || args[i + 1].StartsWith('-'))
                {
                    return new CommandLineOptions
                    {
                        IsValid = false,
                        ErrorMessage = "A opção '--config' requer o caminho do arquivo de configuração."
                    };
                }

                configPath = args[++i];
            }
            else if (arg.Equals("--init", StringComparison.OrdinalIgnoreCase) ||
                     arg.Equals("-i", StringComparison.OrdinalIgnoreCase))
            {
                init = true;
            }
            else if (arg.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
                     arg.Equals("-h", StringComparison.OrdinalIgnoreCase) ||
                     arg.Equals("-?", StringComparison.OrdinalIgnoreCase))
            {
                showHelp = true;
            }
            else if (arg.Equals("--version", StringComparison.OrdinalIgnoreCase) ||
                     arg.Equals("-v", StringComparison.OrdinalIgnoreCase))
            {
                showVersion = true;
            }
            else
            {
                return new CommandLineOptions
                {
                    IsValid = false,
                    ErrorMessage = $"Opção desconhecida: '{arg}'. Execute 'nxtl --help' para ver as opções disponíveis."
                };
            }
        }

        return new CommandLineOptions
        {
            ConfigPath = configPath,
            RunTaskName = runTaskName,
            Init = init,
            ShowHelp = showHelp,
            ShowVersion = showVersion,
            IsValid = true
        };
    }
}
