using Spectre.Console;
using Spectre.Console.Rendering;

namespace NetTaskList.Tests.Mocks;

public sealed class MockAnsiConsole : IAnsiConsole
{
    private readonly IAnsiConsole _inner;
    private readonly MockAnsiConsoleInput _input = new();
    private readonly StringWriter _writer;

    public MockAnsiConsole(StringWriter? writer = null)
    {
        _writer = writer ?? new StringWriter();
        _inner = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(_writer),
            Interactive = InteractionSupport.Yes,
            Ansi = AnsiSupport.Yes,
        });

        _inner.Profile.Width = 80;
        _inner.Profile.Height = 24;
    }

    public Profile Profile => _inner.Profile;
    public IAnsiConsoleCursor Cursor => _inner.Cursor;
    public IAnsiConsoleInput Input => _input;
    public IExclusivityMode ExclusivityMode => _inner.ExclusivityMode;
    public RenderPipeline Pipeline => _inner.Pipeline;

    public string Output => _writer.ToString();

    public void Clear(bool home) => _inner.Clear(home);
    public void Write(IRenderable renderable) => _inner.Write(renderable);
    public void WriteAnsi(Action<AnsiWriter> action) => _inner.WriteAnsi(action);

    public void PushKey(ConsoleKey key, char keyChar = '\0', bool shift = false, bool alt = false, bool control = false)
    {
        _input.PushKey(key, keyChar, shift, alt, control);
    }
}

public sealed class MockAnsiConsoleInput : IAnsiConsoleInput
{
    private readonly Queue<ConsoleKeyInfo> _keys = new();

    public void PushKey(ConsoleKey key, char keyChar = '\0', bool shift = false, bool alt = false, bool control = false)
    {
        _keys.Enqueue(new ConsoleKeyInfo(keyChar, key, shift, alt, control));
    }

    public bool IsKeyAvailable() => _keys.Count > 0;

    public ConsoleKeyInfo? ReadKey(bool intercept)
    {
        return _keys.Count > 0 ? _keys.Dequeue() : null;
    }

    public Task<ConsoleKeyInfo?> ReadKeyAsync(bool intercept, CancellationToken cancellationToken)
    {
        return Task.FromResult(ReadKey(intercept));
    }
}
