using System.Text;
using HomeFinance.Api.Authentication;
using HomeFinance.Api.Middleware;
using HomeFinance.Application.Services;
using HomeFinance.Domain.Repositories;
using HomeFinance.Domain.Entities;
using HomeFinance.Infrastructure;
using HomeFinance.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var connectionString = builder.Configuration.GetConnectionString("HomeFinance");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Debe configurar ConnectionStrings:HomeFinance.");
}

builder.Services.AddDbContext<PersistenceDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<IUserRepository, PostgresUserRepository>();
builder.Services.AddScoped<IHouseholdRepository, PostgresHouseholdRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<HouseholdService>();
builder.Services.AddScoped<AccessTokenService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddOptions<AuthenticationOptions>()
    .Bind(builder.Configuration.GetSection(AuthenticationOptions.SectionName))
    .Validate(options => Encoding.UTF8.GetByteCount(options.SigningKey) >= 32,
        "Authentication:SigningKey debe tener al menos 32 bytes.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Authentication:Issuer es obligatorio.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Authentication:Audience es obligatorio.")
    .Validate(options => options.TokenLifetimeMinutes is > 0 and <= 1440,
        "Authentication:TokenLifetimeMinutes debe estar entre 1 y 1440.")
    .ValidateOnStart();

var authenticationOptions = builder.Configuration.GetSection(AuthenticationOptions.SectionName).Get<AuthenticationOptions>()
    ?? new AuthenticationOptions();
if (Encoding.UTF8.GetByteCount(authenticationOptions.SigningKey) < 32)
{
    throw new InvalidOperationException("Authentication:SigningKey debe tener al menos 32 bytes.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authenticationOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = authenticationOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");
app.UseHttpsRedirection();
app.UseMiddleware<ApiExceptionHandler>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
