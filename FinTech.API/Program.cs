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

// Intentar leer DATABASE_URL de múltiples fuentes
Console.WriteLine("\n🔍 Buscando DATABASE_URL...");
Console.WriteLine(new string('-', 60));

string? databaseUrl = null;

// 1. Intentar desde variables de entorno
databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    Console.WriteLine("✓ DATABASE_URL encontrada en Environment Variables");
}

// 2. Intentar desde configuración (appsettings o Railway)
if (string.IsNullOrEmpty(databaseUrl))
{
    databaseUrl = builder.Configuration["DATABASE_URL"];
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        Console.WriteLine("✓ DATABASE_URL encontrada en Configuration");
    }
}

// 3. Intentar desde ConnectionStrings en configuración
if (string.IsNullOrEmpty(databaseUrl))
{
    databaseUrl = builder.Configuration.GetConnectionString("DATABASE_URL");
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        Console.WriteLine("✓ DATABASE_URL encontrada en ConnectionStrings");
    }
}

// Debugging: Mostrar todas las variables de entorno disponibles
Console.WriteLine("\n🔑 Variables de entorno disponibles:");
var envVars = Environment.GetEnvironmentVariables();
var railwayVars = new List<string>();
foreach (var key in envVars.Keys)
{
    var keyStr = key.ToString()!;
    if (keyStr.Contains("DATABASE", StringComparison.OrdinalIgnoreCase) ||
        keyStr.Contains("RAILWAY", StringComparison.OrdinalIgnoreCase) ||
        keyStr.Contains("POSTGRES", StringComparison.OrdinalIgnoreCase))
    {
        railwayVars.Add(keyStr);
        var value = envVars[key]?.ToString();
        // Ocultar password en logs
        if (value != null && value.Contains("://"))
        {
            Console.WriteLine($"   {keyStr}: [DETECTED - postgresql://***]");
        }
        else
        {
            Console.WriteLine($"   {keyStr}: {value}");
        }
    }
}

if (railwayVars.Count == 0)
{
    Console.WriteLine("   ⚠️  No se encontraron variables relacionadas con Railway/Database");
}

Console.WriteLine(new string('-', 60));


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
