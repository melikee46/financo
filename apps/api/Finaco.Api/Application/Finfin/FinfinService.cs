using Finaco.Api.Application.Cashflow;

namespace Finaco.Api.Application.Finfin;

public sealed class FinfinService(CashflowService cashflowService)
{
    public async Task<FinfinChatResponse> ChatAsync(FinfinChatRequest request, CancellationToken cancellationToken)
    {
        if (request.AccountId is null)
            return FinfinDemoResponder.Create("Bir hesap seçilmesi gerekiyor.", null, [], ["hesap"]);

        var (projection, error) = await cashflowService.GetAsync(request.AccountId.Value, DateOnly.FromDateTime(DateTime.UtcNow), null, cancellationToken);
        if (error is not null)
        {
            var missing = error.Contains(":", StringComparison.Ordinal) ? error[(error.IndexOf(':') + 1)..].TrimEnd('.') .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) : [error];
            return FinfinDemoResponder.Create(error, projection?.Method, projection?.Warnings ?? [], missing);
        }
        return FinfinDemoResponder.Create(null, projection?.Method, projection?.Warnings ?? [], []);
    }
}
