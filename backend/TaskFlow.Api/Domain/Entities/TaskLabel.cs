namespace TaskFlow.Api.Domain.Entities;

public class TaskLabel
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366f1";

    public TaskItem Task { get; set; } = null!;
}
