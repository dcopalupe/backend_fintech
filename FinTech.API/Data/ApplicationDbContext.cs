using Microsoft.EntityFrameworkCore;
using FinTech.API.Models;

namespace FinTech.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for FinTech Loan System
    public DbSet<Loan> Loans { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<PaymentSchedule> PaymentSchedules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Loan entity
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.InterestRate).HasPrecision(5, 2);
            entity.Property(e => e.MonthlyPayment).HasPrecision(18, 2);

            entity.Property(e => e.LoanType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
        });

        // Configure Transaction entity
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);

            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();

            entity.HasIndex(e => e.IdempotencyKey).IsUnique();
            entity.HasIndex(e => e.LoanId);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.Loan)
                .WithMany(l => l.Transactions)
                .HasForeignKey(e => e.LoanId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure PaymentSchedule entity
        modelBuilder.Entity<PaymentSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalPayment).HasPrecision(18, 2);
            entity.Property(e => e.Principal).HasPrecision(18, 2);
            entity.Property(e => e.Interest).HasPrecision(18, 2);
            entity.Property(e => e.RemainingBalance).HasPrecision(18, 2);

            entity.Property(e => e.Status).HasConversion<string>();

            entity.HasOne(e => e.Loan)
                .WithMany(l => l.PaymentSchedules)
                .HasForeignKey(e => e.LoanId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.LoanId);
            entity.HasIndex(e => e.DueDate);
            entity.HasIndex(e => e.Status);
        });
    }
}
