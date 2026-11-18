using Finaco.Api.Domain;
using Finaco.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Finaco.Api.Application.Cashflow;

public sealed class CashflowService(FinacoDbContext db)
{
    public async Task<IReadOnlyList<CashflowAccountResponse>> ListAccountsAsync(CancellationToken cancellationToken) =>
        await db.Accounts.AsNoTracking().OrderBy(x => x.CreatedAt)
            .Select(x => new CashflowAccountResponse(x.Id, x.Name, x.CurrencyCode, x.OpeningBalance, x.OpeningBalanceDate, x.NextIncomeDate, x.NextIncomeAmount)).ToListAsync(cancellationToken);

    public async Task<CashflowAccountResponse?> SaveSettingsAsync(Guid accountId, CashflowSettingsRequest request, CancellationToken cancellationToken)
    {
        if (CashflowValidation.Settings(request) is not null || request.OpeningBalance is > 999999999999m || request.NextIncomeAmount is > 999999999999m) return null;
        var account = await db.Accounts.SingleOrDefaultAsync(x => x.Id == accountId, cancellationToken);
        if (account is null) return null;
        account.OpeningBalance = request.OpeningBalance;
        account.OpeningBalanceDate = request.OpeningBalanceDate;
        account.NextIncomeDate = request.NextIncomeDate;
        account.NextIncomeAmount = request.NextIncomeAmount;
        await db.SaveChangesAsync(cancellationToken);
        return new(account.Id, account.Name, account.CurrencyCode, account.OpeningBalance, account.OpeningBalanceDate, account.NextIncomeDate, account.NextIncomeAmount);
    }

    public async Task<(CashflowResponse? Response, string? Error)> GetAsync(Guid accountId, DateOnly asOfDate, CashflowScenarioRequest? scenario, CancellationToken cancellationToken)
    {
        var account = await db.Accounts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == accountId, cancellationToken);
        if (account is null) return (null, "Hesap bulunamadı.");
        var missing = new List<string>();
        if (account.OpeningBalance is null) missing.Add("başlangıç bakiyesi");
        if (account.OpeningBalanceDate is null) missing.Add("bakiyenin geçerli olduğu tarih");
        if (account.NextIncomeDate is null) missing.Add("beklenen gelir tarihi");
        if (account.NextIncomeAmount is null) missing.Add("beklenen gelir tutarı");
        if (missing.Count > 0) return (new CashflowResponse(ToAccount(account), asOfDate, null, null, null, null, [], [], [], "Başlangıç bakiyesi ve kayıtlı işlemler gün bazında toplanır; gelir beklenen tarihinde eklenir.", asOfDate.ToString("yyyy-MM-dd"), false, null), $"Tahmin için şu bilgiler eksik: {string.Join(", ", missing)}.");
        if (account.OpeningBalanceDate > account.NextIncomeDate) return (null, "Bakiyenin geçerli olduğu tarih beklenen gelir tarihinden sonra olamaz.");
        var start = account.OpeningBalanceDate!.Value;
        var incomeDate = account.NextIncomeDate!.Value;
        var scenarioEnd = incomeDate.AddDays(Math.Max(0, scenario?.DelayedIncomeDays ?? 0));
        if (scenario?.UnexpectedExpenseDate is { } unexpectedDate && unexpectedDate > scenarioEnd) scenarioEnd = unexpectedDate;
        if (scenario?.AdditionalIncomeDate is { } additionalDate && additionalDate > scenarioEnd) scenarioEnd = additionalDate;
        var transactions = await db.Transactions.AsNoTracking().Where(x => x.AccountId == accountId && x.TransactionDate >= start && x.TransactionDate <= scenarioEnd).ToListAsync(cancellationToken);
        var points = CashflowCalculator.Project(start, account.OpeningBalance!.Value, incomeDate, account.NextIncomeAmount!.Value, transactions, null, asOfDate);
        var warnings = new List<string> { "Bu bir tahmindir; henüz kaydedilmemiş gelir ve giderler sonucu değiştirebilir." };
        var limited = transactions.Any(x => x.Type is TransactionType.Transfer or TransactionType.CardPayment);
        if (limited) warnings.Add("Transfer ve kredi kartı ödemeleri güvenilir karşı hesap ilişkisi olmadığı için projeksiyonda harcama olarak sayılmadı; sonuç sınırlıdır.");
        var entries = transactions.Where(x => x.Type is not (TransactionType.Transfer or TransactionType.CardPayment)).OrderBy(x => x.TransactionDate).Select(x => new CashflowEntryResponse(x.TransactionDate, x.DescriptionOriginal, x.Amount, x.CurrencyCode, x.Type)).ToList();
        var scenarioSummary = scenario is null ? null : BuildScenario(start, account.OpeningBalance.Value, incomeDate, account.NextIncomeAmount.Value, transactions, scenario, asOfDate);
        var lowest = points.MinBy(x => x.Balance);
        return (new(ToAccount(account), start, incomeDate, incomeDate.DayNumber - asOfDate.DayNumber, lowest?.Balance, lowest?.Date, points, entries, warnings, "Başlangıç bakiyesi + yalnızca kayıtlı işlem türlerinin günlük net etkisi + beklenen gelir. Transfer/CardPayment hariç tutulur.", asOfDate.ToString("yyyy-MM-dd"), limited, scenarioSummary), null);
    }

    private static CashflowScenarioSummary BuildScenario(DateOnly start, decimal openingBalance, DateOnly incomeDate, decimal incomeAmount, IReadOnlyList<Transaction> transactions, CashflowScenarioRequest scenario, DateOnly dataCutoffDate)
    {
        var scenarioDate = incomeDate.AddDays(Math.Max(0, scenario.DelayedIncomeDays));
        var points = CashflowCalculator.Project(start, openingBalance, incomeDate, incomeAmount, transactions, scenario, dataCutoffDate);
        var lowest = points.MinBy(x => x.Balance);
        return new(scenario.DelayedIncomeDays > 0 ? $"Gelir {scenario.DelayedIncomeDays} gün gecikirse" : "Geçici senaryo", scenarioDate, points, lowest?.Balance, lowest?.Date);
    }

    private static CashflowAccountResponse ToAccount(Account x) => new(x.Id, x.Name, x.CurrencyCode, x.OpeningBalance, x.OpeningBalanceDate, x.NextIncomeDate, x.NextIncomeAmount);
}
