using HomeFinance.Application.Services;
using HomeFinance.Domain.Repositories;
using HomeFinance.Infrastructure.Repositories;

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

builder.Services.AddSingleton<InMemoryUserRepository>();
builder.Services.AddSingleton<IUserRepository>(sp => sp.GetRequiredService<InMemoryUserRepository>());
builder.Services.AddSingleton<InMemoryHouseholdRepository>();
builder.Services.AddSingleton<IHouseholdRepository>(sp => sp.GetRequiredService<InMemoryHouseholdRepository>());
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<HouseholdService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
