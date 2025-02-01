namespace WebApi.Models;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string? Token { get; set; }
    public virtual ICollection<RequestHistory>? RequestHistory { get; set; }
} 