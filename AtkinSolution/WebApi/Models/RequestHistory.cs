namespace WebApi.Models;

public class RequestHistory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string Endpoint { get; set; }
    public required string Method { get; set; }
    public DateTime Timestamp { get; set; }
    public virtual User? User { get; set; }
} 