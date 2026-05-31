using FinTech.API.Data;
using FinTech.API.Services;
using FinTech.API.Repositories.Interfaces;
using FinTech.API.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using FinTech.API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("\n" + new string('=', 60));
Console.WriteLine("🚀 FINTECH API - INICIALIZANDO");
Console.WriteLine(new string('=', 60));

// MOSTRAR TODAS LAS VARIABLES DE ENTORNO
Console.WriteLine("\n🔍 TODAS LAS VARIABLES DE ENTORNO:");
Console.WriteLine(new string('-', 70));

var envVars = Environment.GetEnvironmentVariables();
var sortedKeys = new List<string>();

foreach (var key in envVars.Keys)
{
    sortedKeys.Add(key.ToString()!);
}

sortedKeys.Sort();

int count = 0;
foreach (var key in sortedKeys)
{
    var value = envVars[key]?.ToString() ?? "";

    // Truncar valores muy largos
    if (value.Length > 80)
    {
        value = value.Substring(0, 77) + "...";
    }

    // Ocultar passwords/secrets parcialmente
    if (key.Contains("PASSWORD", StringComparison.OrdinalIgnoreCase) ||
        key.Contains("SECRET", StringComparison.OrdinalIgnoreCase) ||
        key.Contains("TOKEN", StringComparison.OrdinalIgnoreCase))
    {
        if (value.Length > 8)
        {
            value = value.Substring(0, 4) + "***" + value.Substring(value.Length - 4);
        }
        else if (value.Length > 0)
        {
            value = "***";
        }
    }

    Console.WriteLine($"{count + 1,3}. {key,-40} = {value}");
    count++;
}

Console.WriteLine($"\n📊 Total de variables encontradas: {count}");
Console.WriteLine(new string('-', 70));

string databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL") ?? "";
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    try
    {
        var databaseUri = new Uri(databaseUrl);
        var userInfo = databaseUri.UserInfo.Split(':');

        connectionString = string.Format(
            "Host={0};Port={1};Database={2};Username={3};Password={4};SSL Mode=Prefer;Trust Server Certificate=true",
            databaseUri.Host,
            databaseUri.Port,
            databaseUri.AbsolutePath.TrimStart('/'),
            userInfo[0],
            userInfo[1]
        );

        Console.WriteLine($"Host:     {databaseUri.Host}");
        Console.WriteLine($"Puerto:   {databaseUri.Port}");
        Console.WriteLine($"Database: {databaseUri.AbsolutePath.TrimStart('/')}");
        Console.WriteLine($"Usuario:  {userInfo[0]}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR parseando DATABASE_URL: {ex.Message}");
        throw;
    }
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
    Console.WriteLine($"Local Connection String: {connectionString}");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options
        .UseNpgsql(connectionString, npgsql =>
        {
            npgsql.EnableRetryOnFailure();
            npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        }));

builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IPaymentScheduleRepository, PaymentScheduleRepository>();

builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FinTech API",
        Version = "v1",
        Description = "API for managing loans, transactions, and payment schedules",
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "FinTech API v1");
    options.RoutePrefix = "swagger";
});

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
