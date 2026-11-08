using HomeFinance.Application.Services;
using HomeFinance.Domain.Entities;
using HomeFinance.Domain.Repositories;
using HomeFinance.Infrastructure.Repositories;

namespace HomeFinance.IntegrationTests;

public class AuthAndHouseholdTests
{
    [Fact]
    public void RegisterUser_WhenEmailIsUnique_ShouldCreateUser()
    {
        var repo = new InMemoryUserRepository();
        var authService = new AuthService(repo);

        var result = authService.Register("test@example.com", "Password123!");

        Assert.True(result.Success);
        Assert.Equal("test@example.com", result.User.Email);
        Assert.NotEqual("Password123!", result.User.PasswordHash);
    }

    [Fact]
    public void RegisterUser_WhenEmailAlreadyExists_ShouldFail()
    {
        var repo = new InMemoryUserRepository();
        var authService = new AuthService(repo);

        authService.Register("test@example.com", "Password123!");
        var result = authService.Register("test@example.com", "Password456!");

        Assert.False(result.Success);
        Assert.Contains("ya existe", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Login_WhenCredentialsAreValid_ShouldReturnUser()
    {
        var repo = new InMemoryUserRepository();
        var authService = new AuthService(repo);
        authService.Register("login@example.com", "Password123!");

        var result = authService.Login("login@example.com", "Password123!");

        Assert.True(result.Success);
        Assert.Equal("login@example.com", result.User.Email);
    }

    [Fact]
    public void CreateHousehold_WhenOwnerIsMissing_ShouldFail()
    {
        var householdService = new HouseholdService(new InMemoryHouseholdRepository());

        var result = householdService.CreateHousehold(null, "Mi hogar");

        Assert.False(result.Success);
        Assert.Contains("titular", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CreateHousehold_WhenNameIsValid_ShouldUseDefaultCurrencyAndTimezone()
    {
        var householdRepo = new InMemoryHouseholdRepository();
        var householdService = new HouseholdService(householdRepo);
        var owner = new User { Id = Guid.NewGuid(), Email = "owner@example.com", PasswordHash = "hash" };

        var result = householdService.CreateHousehold(owner, "Hogar principal");

        Assert.True(result.Success);
        Assert.Equal("ARS", result.Household.Currency);
        Assert.Equal("America/Argentina/Cordoba", result.Household.TimeZone);
        Assert.Equal("Hogar principal", result.Household.Name);
    }
}
