// Generated-style snapshot kept with the initial migration.
using Finaco.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace Finaco.Api.Migrations;

[DbContext(typeof(FinacoDbContext))]
partial class FinacoDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.11");
        modelBuilder.Entity("Finaco.Api.Domain.Account", b => { b.Property<Guid>("Id"); b.Property<string>("CurrencyCode").HasMaxLength(3).HasDefaultValue("TRY"); b.Property<DateTimeOffset>("CreatedAt"); b.Property<string>("Name").HasMaxLength(160); b.Property<decimal?>("OpeningBalance").HasPrecision(18, 2); b.Property<DateOnly?>("OpeningBalanceDate"); b.Property<int>("Type"); b.HasKey("Id"); b.HasIndex("Name"); b.ToTable("accounts"); });
        modelBuilder.Entity("Finaco.Api.Domain.Category", b => { b.Property<Guid>("Id"); b.Property<DateTimeOffset>("CreatedAt"); b.Property<int>("Kind"); b.Property<bool>("IsArchived"); b.Property<string>("Name").HasMaxLength(120); b.HasKey("Id"); b.HasIndex("Name", "Kind").IsUnique(); b.ToTable("categories"); });
        modelBuilder.Entity("Finaco.Api.Domain.ImportJob", b => { b.Property<Guid>("Id"); b.Property<DateTimeOffset>("CreatedAt"); b.Property<string>("FileName").HasMaxLength(255); b.Property<int>("Status"); b.HasKey("Id"); b.HasIndex("FileName", "CreatedAt"); b.ToTable("import_jobs"); });
        modelBuilder.Entity("Finaco.Api.Domain.ImportRow", b => { b.Property<Guid>("Id"); b.Property<Guid>("ImportJobId"); b.Property<string>("ParsedValuesJson").HasColumnType("jsonb"); b.Property<int>("RowNumber"); b.Property<string>("ValidationErrorsJson").HasColumnType("jsonb"); b.HasKey("Id"); b.HasIndex("ImportJobId", "RowNumber").IsUnique(); b.ToTable("import_rows"); });
        modelBuilder.Entity("Finaco.Api.Domain.Transaction", b => { b.Property<Guid>("Id"); b.Property<decimal>("Amount").HasPrecision(18, 2); b.Property<Guid>("AccountId"); b.Property<Guid?>("CategoryId"); b.Property<string>("CurrencyCode").HasMaxLength(3).HasDefaultValue("TRY"); b.Property<DateTimeOffset>("CreatedAt"); b.Property<string>("DescriptionNormalized").HasMaxLength(500); b.Property<string>("DescriptionOriginal").HasMaxLength(500); b.Property<DateOnly>("TransactionDate"); b.Property<int>("Type"); b.Property<DateTimeOffset>("UpdatedAt"); b.HasKey("Id"); b.HasIndex("AccountId", "TransactionDate"); b.HasIndex("CategoryId"); b.HasIndex("Type"); b.ToTable("transactions", t => t.HasCheckConstraint("ck_transactions_amount_positive", "amount > 0")); });
        modelBuilder.Entity("Finaco.Api.Domain.ImportRow", b => b.HasOne("Finaco.Api.Domain.ImportJob", "ImportJob").WithMany("Rows").HasForeignKey("ImportJobId").OnDelete(DeleteBehavior.Cascade).IsRequired());
        modelBuilder.Entity("Finaco.Api.Domain.Transaction", b => b.HasOne("Finaco.Api.Domain.Account", "Account").WithMany("Transactions").HasForeignKey("AccountId").OnDelete(DeleteBehavior.Cascade).IsRequired());
        modelBuilder.Entity("Finaco.Api.Domain.Transaction", b => b.HasOne("Finaco.Api.Domain.Category", "Category").WithMany("Transactions").HasForeignKey("CategoryId").OnDelete(DeleteBehavior.SetNull));
        modelBuilder.Entity("Finaco.Api.Domain.Account", b => b.Navigation("Transactions")); modelBuilder.Entity("Finaco.Api.Domain.Category", b => b.Navigation("Transactions")); modelBuilder.Entity("Finaco.Api.Domain.ImportJob", b => b.Navigation("Rows")); modelBuilder.Entity("Finaco.Api.Domain.ImportRow", b => b.Navigation("ImportJob")); modelBuilder.Entity("Finaco.Api.Domain.Transaction", b => { b.Navigation("Account"); b.Navigation("Category"); });
    }
}

