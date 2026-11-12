using System.ComponentModel.DataAnnotations;

namespace Finaco.Api.Domain;

public sealed class Account
{
    public Guid Id { get; set; }
    [MaxLength(160)] public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    [MaxLength(3)] public string CurrencyCode { get; set; } = "TRY";
    public decimal? OpeningBalance { get; set; }
    public DateOnly? OpeningBalanceDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

public sealed class Category
{
    public Guid Id { get; set; }
    [MaxLength(120)] public string Name { get; set; } = string.Empty;
    public CategoryKind Kind { get; set; }
    public bool IsArchived { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

public sealed class Transaction
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public DateOnly TransactionDate { get; set; }
    [MaxLength(500)] public string DescriptionOriginal { get; set; } = string.Empty;
    [MaxLength(500)] public string? DescriptionNormalized { get; set; }
    public decimal Amount { get; set; }
    [MaxLength(3)] public string CurrencyCode { get; set; } = "TRY";
    public TransactionType Type { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Account Account { get; set; } = null!;
    public Category? Category { get; set; }
}

public sealed class ImportJob
{
    public Guid Id { get; set; }
    [MaxLength(255)] public string FileName { get; set; } = string.Empty;
    public ImportStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ICollection<ImportRow> Rows { get; set; } = new List<ImportRow>();
}

public sealed class ImportRow
{
    public Guid Id { get; set; }
    public Guid ImportJobId { get; set; }
    public int RowNumber { get; set; }
    public string ParsedValuesJson { get; set; } = "{}";
    public string ValidationErrorsJson { get; set; } = "[]";
    public ImportJob ImportJob { get; set; } = null!;
}

