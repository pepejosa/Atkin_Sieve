namespace SieveClient.Models;

public class HistoryItem
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public string Method { get; set; } = "";
    public DateTime Timestamp { get; set; }
} 