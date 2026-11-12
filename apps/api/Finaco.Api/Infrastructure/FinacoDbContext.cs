using Finaco.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Finaco.Api.Infrastructure;

public sealed class FinacoDbContext(DbContextOptions<FinacoDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();
    public DbSet<ImportRow> ImportRows => Set<ImportRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts"); entity.HasKey(x => x.Id); entity.Property(x => x.CurrencyCode).HasDefaultValue("TRY"); entity.Property(x => x.OpeningBalance).HasPrecision(18, 2); entity.HasIndex(x => x.Name);
        });
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories"); entity.HasKey(x => x.Id); entity.HasIndex(x => new { x.Name, x.Kind }).IsUnique();
        });
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("transactions"); entity.HasKey(x => x.Id); entity.Property(x => x.Amount).HasPrecision(18, 2); entity.Property(x => x.CurrencyCode).HasDefaultValue("TRY"); entity.HasIndex(x => new { x.AccountId, x.TransactionDate }); entity.HasIndex(x => x.Type); entity.HasIndex(x => x.CategoryId); entity.HasCheckConstraint("ck_transactions_amount_positive", "amount > 0"); entity.HasOne(x => x.Account).WithMany(x => x.Transactions).HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade); entity.HasOne(x => x.Category).WithMany(x => x.Transactions).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<ImportJob>(entity =>
        {
            entity.ToTable("import_jobs"); entity.HasKey(x => x.Id); entity.HasIndex(x => new { x.FileName, x.CreatedAt });
        });
        modelBuilder.Entity<ImportRow>(entity =>
        {
            entity.ToTable("import_rows"); entity.HasKey(x => x.Id); entity.Property(x => x.ParsedValuesJson).HasColumnType("jsonb"); entity.Property(x => x.ValidationErrorsJson).HasColumnType("jsonb"); entity.HasIndex(x => new { x.ImportJobId, x.RowNumber }).IsUnique(); entity.HasOne(x => x.ImportJob).WithMany(x => x.Rows).HasForeignKey(x => x.ImportJobId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}

