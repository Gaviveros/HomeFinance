using HomeFinance.Domain.Entities;

namespace HomeFinance.Application.Services;

public class AuthResult
{
    public bool Success { get; set; }
    public bool DuplicateEmail { get; set; }
    public string? Error { get; set; }
    public User? User { get; set; }
}
