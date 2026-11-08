namespace HomeFinance.Domain.Entities;

public class Household
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string Currency { get; set; } = "ARS";
    public string TimeZone { get; set; } = "America/Argentina/Cordoba";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
