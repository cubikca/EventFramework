namespace EventFramework.EventSourcing;

public class Checkpoint
{
    public string? Id { get; set; }
    public ulong? Position { get; set; }
}