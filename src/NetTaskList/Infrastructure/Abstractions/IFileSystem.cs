namespace NetTaskList.Infrastructure.Abstractions;

/// <summary>
/// Abstraction for filesystem operations to enable testing without actual disk I/O.
/// </summary>
public interface IFileSystem
{
    bool FileExists(string path);
    string ReadAllText(string path);
    void WriteAllText(string path, string content);
    string GetFullPath(string path);
    string GetCurrentDirectory();
}
