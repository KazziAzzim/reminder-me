namespace ReminderMe.Models;

public sealed class ReminderItem
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Description { get; init; }
    public required DateTime DueAt { get; init; }
    public bool IsTriggered { get; set; }
}
