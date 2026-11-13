namespace Finaco.Api.Application.Imports;

/// <summary>Represents the parsed result of one uploaded CSV row.</summary>
public sealed record ImportRowPreviewResponse(
    int RowNumber,
    bool IsValid,
    string? TransactionDate,
    string? Description,
    decimal? Amount,
    string CurrencyCode,
    IReadOnlyList<string> ValidationErrors);

/// <summary>Represents the validation preview for an uploaded CSV file.</summary>
public sealed record ImportPreviewResponse(
    Guid ImportJobId,
    string FileName,
    int TotalRows,
    int ValidRows,
    int InvalidRows,
    IReadOnlyList<ImportRowPreviewResponse> Rows);
