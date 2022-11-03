namespace EventFramework.SharedKernel;

public class Checkpoint
{
    public string? Id { get; set; }    
    public string? Topic { get; set; }
    public int? Partition { get; set; }
    public long? Offset { get; set; }
}