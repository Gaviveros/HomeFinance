using HomeFinance.Application.Services;
using HomeFinance.Domain.Entities;
using HomeFinance.Domain.Repositories;
using HomeFinance.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;

namespace HomeFinance.IntegrationTests;

public class AuthAndHouseholdTests
{
    [Fact]
    public async Task RegisterUser_WhenEmailIsUnique_ShouldCreateUser()
    {
        var repo = new InMemoryUserRepository();
        var authService = new AuthService(repo, new PasswordHasher<User>());

        var result = await authService.RegisterAsync("test@example.com", "Password123!");

        Assert.True(result.Success);
        Assert.Equal("test@example.com", result.User!.Email);
        Assert.NotEqual("Password123!", result.User.PasswordHash);
    }

    [Fact]
    public async Task RegisterUser_WhenEmailAlreadyExists_ShouldFail()
    {
        var repo = new InMemoryUserRepository();
        var authService = new AuthService(repo, new PasswordHasher<User>());

        await authService.RegisterAsync("test@example.com", "Password123!");
        var result = await authService.RegisterAsync("test@example.com", "Password456!");

        Assert.False(result.Success);
        Assert.Contains("ya existe", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ShouldReturnUser()
    {
        var repo = new InMemoryUserRepository();
        var authService = new AuthService(repo, new PasswordHasher<User>());
        await authService.RegisterAsync("login@example.com", "Password123!");

        var result = await authService.LoginAsync("login@example.com", "Password123!");

        Assert.True(result.Success);
        Assert.Equal("login@example.com", result.User!.Email);
    }

    [Fact]
    public async Task CreateHousehold_WhenOwnerIsMissing_ShouldFail()
    {
        var householdService = new HouseholdService(new InMemoryHouseholdRepository());

        var result = await householdService.CreateHouseholdAsync(null, "Mi hogar");

        Assert.False(result.Success);
        Assert.Contains("titular", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateHousehold_WhenNameIsValid_ShouldUseDefaultCurrencyAndTimezone()
    {
        var householdRepo = new InMemoryHouseholdRepository();
        var householdService = new HouseholdService(householdRepo);
        var owner = new User { Id = Guid.NewGuid(), Email = "owner@example.com", PasswordHash = "hash" };

        var result = await householdService.CreateHouseholdAsync(owner, "Hogar principal");

        Assert.True(result.Success);
        Assert.Equal("ARS", result.Household!.Currency);
        Assert.Equal("America/Argentina/Cordoba", result.Household.TimeZone);
        Assert.Equal("Hogar principal", result.Household.Name);
    }
}
