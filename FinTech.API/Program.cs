using FinTech.API.Data;
using FinTech.API.Services;
using FinTech.API.Repositories.Interfaces;
using FinTech.API.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using FinTech.API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

string connectionString;
// Intentar múltiples fuentes para la conexión
Console.WriteLine("\n🔍 BUSCANDO CONFIGURACIÓN DE BASE DE DATOS...\n");

// 1. Intentar Environment.GetEnvironmentVariable("DATABASE_URL")
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
Console.WriteLine($"1️⃣  Environment.GetEnvironmentVariable(\"DATABASE_URL\"): {(!string.IsNullOrEmpty(databaseUrl) ? "✓ ENCONTRADA" : "✗ Vacía")}");

// 2. Intentar builder.Configuration["DATABASE_URL"]
if (string.IsNullOrEmpty(databaseUrl))
{
    databaseUrl = builder.Configuration["DATABASE_URL"];
    Console.WriteLine($"2️⃣  builder.Configuration[\"DATABASE_URL\"]: {(!string.IsNullOrEmpty(databaseUrl) ? "✓ ENCONTRADA" : "✗ Vacía")}");
}

// 3. Intentar builder.Configuration.GetConnectionString("DATABASE_URL")
if (string.IsNullOrEmpty(databaseUrl))
{
    databaseUrl = builder.Configuration.GetConnectionString("DATABASE_URL");
    Console.WriteLine($"3️⃣  ConnectionStrings:DATABASE_URL: {(!string.IsNullOrEmpty(databaseUrl) ? "✓ ENCONTRADA" : "✗ Vacía")}");
}

// 4. Intentar DefaultConnection
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"4️⃣  DefaultConnection: {(!string.IsNullOrEmpty(defaultConnection) ? "✓ ENCONTRADA" : "✗ Vacía")}");

Console.WriteLine(new string('-', 70));

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
