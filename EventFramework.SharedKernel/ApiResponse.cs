namespace EventFramework.SharedKernel;

public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? StackTrace { get; set; }
}