using Finaco.Api.Domain;

namespace Finaco.Api.Application.Transactions;

/// <summary>Filters and pagination values for the transaction list.</summary>
public sealed record TransactionListQuery(
    int Page,
    int PageSize,
    DateOnly? From,
    DateOnly? To,
    TransactionType? Type,
    Guid? AccountId);

/// <summary>Represents a transaction returned by the API.</summary>
public sealed record TransactionListItemResponse(
    DateOnly Date,
    string Description,
    decimal Amount,
    string Currency,
    TransactionType Type,
    string AccountName,
    string? CategoryName);

/// <summary>Represents a paginated transaction list.</summary>
public sealed record TransactionListResponse(
    IReadOnlyList<TransactionListItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
