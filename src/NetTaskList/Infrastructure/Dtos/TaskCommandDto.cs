namespace NetTaskList.Infrastructure.Dtos;

public sealed class TaskCommandDto
{
    public string? RawCommand { get; set; }
    public string? TargetTask { get; set; }
    public bool IsShortcut => TargetTask != null;
}
