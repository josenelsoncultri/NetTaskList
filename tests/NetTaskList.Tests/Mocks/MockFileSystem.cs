using NetTaskList.Infrastructure.Abstractions;

namespace NetTaskList.Tests.Mocks;

public sealed class MockFileSystem : IFileSystem
{
    private readonly Dictionary<string, string> _files = new(StringComparer.OrdinalIgnoreCase);

    public void AddFile(string path, string content)
    {
        _files[path] = content;
    }

    public bool FileExists(string path) => _files.ContainsKey(path);

    public string ReadAllText(string path)
    {
        if (_files.TryGetValue(path, out var content))
            return content;

        throw new FileNotFoundException("File not found", path);
    }

    public void WriteAllText(string path, string content)
    {
        _files[path] = content;
    }

    public string GetFullPath(string path) => path;

    public string GetCurrentDirectory() => "D:/Projetos/Test";
}
