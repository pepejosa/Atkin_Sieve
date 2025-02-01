namespace WebApi.Models.DTOs;

public class HistoryDto
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Endpoint { get; set; }
    public required string Method { get; set; }
    public DateTime Timestamp { get; set; }
} 