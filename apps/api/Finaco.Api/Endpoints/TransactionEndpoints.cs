using System.Globalization;
using Finaco.Api.Application.Transactions;
using Finaco.Api.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Finaco.Api.Endpoints;

public static class TransactionEndpoints
{
    public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/transactions", List)
            .WithName("ListTransactions")
            .WithSummary("List saved transactions")
            .WithDescription("Lists saved transactions with optional date, type, and account filters.")
            .Produces<TransactionListResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<Results<Ok<TransactionListResponse>, BadRequest<ProblemDetails>>> List(
        ITransactionListService service,
        string? from,
        string? to,
        string? type,
        string? accountId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequest("page", "page 1 veya daha büyük olmalıdır.");

        if (pageSize < 1 || pageSize > 100)
            return BadRequest("pageSize", "pageSize 1 ile 100 arasında olmalıdır.");

        if (!TryParseDate(from, "from", out var fromDate, out var dateError))
            return TypedResults.BadRequest(dateError!);

        if (!TryParseDate(to, "to", out var toDate, out dateError))
            return TypedResults.BadRequest(dateError!);

        if (fromDate is not null && toDate is not null && fromDate > toDate)
            return BadRequest("date", "from tarihi to tarihinden sonra olamaz.");

        if (!TryParseType(type, out var transactionType, out var typeError))
            return TypedResults.BadRequest(typeError!);

        if (!Guid.TryParse(accountId, out var parsedAccountId) && accountId is not null)
            return BadRequest("accountId", "accountId geçerli bir GUID olmalıdır.");

        var query = new TransactionListQuery(
            page,
            pageSize,
            fromDate,
            toDate,
            transactionType,
            accountId is null ? null : parsedAccountId);

        return TypedResults.Ok(await service.ListAsync(query, cancellationToken));
    }

    private static bool TryParseDate(
        string? value,
        string parameterName,
        out DateOnly? date,
        out ProblemDetails? error)
    {
        date = null;
        error = null;

        if (value is null)
            return true;

        if (DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            date = parsed;
            return true;
        }

        error = new ProblemDetails
        {
            Title = "Geçersiz tarih filtresi",
            Detail = $"{parameterName} YYYY-MM-DD biçiminde geçerli bir tarih olmalıdır."
        };
        return false;
    }

    private static bool TryParseType(
        string? value,
        out TransactionType? type,
        out ProblemDetails? error)
    {
        type = null;
        error = null;

        if (value is null)
            return true;

        if (Enum.TryParse<TransactionType>(value, ignoreCase: true, out var parsed) &&
            Enum.IsDefined(parsed))
        {
            type = parsed;
            return true;
        }

        error = new ProblemDetails
        {
            Title = "Geçersiz işlem türü",
            Detail = $"type şu değerlerden biri olmalıdır: {string.Join(", ", Enum.GetNames<TransactionType>())}."
        };
        return false;
    }

    private static BadRequest<ProblemDetails> BadRequest(string parameterName, string detail) =>
        TypedResults.BadRequest(new ProblemDetails
        {
            Title = $"Geçersiz {parameterName} filtresi",
            Detail = detail
        });
}
