using Finaco.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Finaco.Api.Application.Transactions;

public interface ITransactionListService
{
    Task<TransactionListResponse> ListAsync(TransactionListQuery query, CancellationToken cancellationToken);
}

public sealed class TransactionListService(FinacoDbContext db) : ITransactionListService
{
    public async Task<TransactionListResponse> ListAsync(
        TransactionListQuery query,
        CancellationToken cancellationToken)
    {
        var transactions = db.Transactions
            .AsNoTracking()
            .AsQueryable();

        if (query.From is not null)
            transactions = transactions.Where(transaction => transaction.TransactionDate >= query.From.Value);

        if (query.To is not null)
            transactions = transactions.Where(transaction => transaction.TransactionDate <= query.To.Value);

        if (query.Type is not null)
            transactions = transactions.Where(transaction => transaction.Type == query.Type.Value);

        if (query.AccountId is not null)
            transactions = transactions.Where(transaction => transaction.AccountId == query.AccountId.Value);

        var totalCount = await transactions.CountAsync(cancellationToken);
        var items = await transactions
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.CreatedAt)
            .ThenByDescending(transaction => transaction.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(transaction => new TransactionListItemResponse(
                transaction.TransactionDate,
                transaction.DescriptionOriginal,
                transaction.Amount,
                transaction.CurrencyCode,
                transaction.Type,
                transaction.Account.Name,
                transaction.Category == null ? null : transaction.Category.Name))
            .ToListAsync(cancellationToken);

        return new TransactionListResponse(
            items,
            query.Page,
            query.PageSize,
            totalCount,
            (int)Math.Ceiling(totalCount / (double)query.PageSize));
    }
}
