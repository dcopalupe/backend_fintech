using FinTech.API.Data;
using FinTech.API.Services;
using FinTech.API.Repositories.Interfaces;
using FinTech.API.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using FinTech.API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

string connectionString;
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

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
    }
    catch (Exception)
    {
        throw;
    }
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://frontendfintech.vercel.app",
                "https://frontendfintech-git-main-dcopa-bcpbols-projects.vercel.app"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

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

var runMigrations = Environment.GetEnvironmentVariable("RUN_MIGRATIONS");
if (string.Equals(runMigrations, "true", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "FinTech API v1");
    options.RoutePrefix = "swagger";
});

app.UseCors("AllowFrontend");

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
