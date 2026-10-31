AnsiConsole.MarkupLine("[bold blue]Welcome[/] to [green].NET Task List[/]!");

AnsiConsole
    .Status()
    .Start("Lendo tarefas...", 
    ctx => {
        Thread.Sleep(2500);
    }
);