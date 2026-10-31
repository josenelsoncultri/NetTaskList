using NetTaskList.Infrastructure.Abstractions;

namespace NetTaskList.Infrastructure;

/// <summary>
/// Default filesystem implementation backed by System.IO.
/// </summary>
public sealed class FileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);

    public string ReadAllText(string path) => File.ReadAllText(path);

    public void WriteAllText(string path, string content) => File.WriteAllText(path, content);

    public string GetFullPath(string path) => Path.GetFullPath(path);

    public string GetCurrentDirectory() => Directory.GetCurrentDirectory();
}
