using HomeFinance.Domain.Entities;

namespace HomeFinance.Application.Services;

public class HouseholdResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public Household? Household { get; set; }
}
