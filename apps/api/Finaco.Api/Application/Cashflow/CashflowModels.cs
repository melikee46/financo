using Finaco.Api.Domain;

namespace Finaco.Api.Application.Cashflow;

public sealed record CashflowSettingsRequest
{
    public decimal? OpeningBalance { get; init; }
    public DateOnly? OpeningBalanceDate { get; init; }
    public DateOnly? NextIncomeDate { get; init; }
    public decimal? NextIncomeAmount { get; init; }
}

public sealed record CashflowScenarioRequest
{
    public decimal? UnexpectedExpenseAmount { get; init; }
    public DateOnly? UnexpectedExpenseDate { get; init; }
    public int DelayedIncomeDays { get; init; }
    public decimal? AdditionalIncomeAmount { get; init; }
    public DateOnly? AdditionalIncomeDate { get; init; }
}

public sealed record CashflowPointResponse(DateOnly Date, decimal Balance, bool IsProjected);
public sealed record CashflowEntryResponse(DateOnly Date, string Description, decimal Amount, string Currency, TransactionType Type);
public sealed record CashflowAccountResponse(Guid Id, string Name, string Currency, decimal? OpeningBalance, DateOnly? OpeningBalanceDate, DateOnly? NextIncomeDate, decimal? NextIncomeAmount);

public sealed record CashflowResponse(
    CashflowAccountResponse Account,
    DateOnly StartDate,
    DateOnly? IncomeDate,
    int? DaysUntilIncome,
    decimal? LowestProjectedBalance,
    DateOnly? LowestBalanceDate,
    IReadOnlyList<CashflowPointResponse> Points,
    IReadOnlyList<CashflowEntryResponse> IncludedEntries,
    IReadOnlyList<string> Warnings,
    string Method,
    string DataCutoffDate,
    bool IsLimited,
    CashflowScenarioSummary? Scenario);

public sealed record CashflowScenarioSummary(string Description, DateOnly IncomeDate, IReadOnlyList<CashflowPointResponse> Points, decimal? LowestProjectedBalance, DateOnly? LowestBalanceDate);

public static class CashflowValidation
{
    public static string? Settings(CashflowSettingsRequest request) => request.OpeningBalance is < 0 ? "Başlangıç bakiyesi negatif olamaz." : request.NextIncomeAmount is <= 0 ? "Beklenen gelir tutarı pozitif olmalı." : request.NextIncomeDate is not null && request.OpeningBalanceDate is not null && request.NextIncomeDate < request.OpeningBalanceDate ? "Gelir tarihi başlangıç tarihinden önce olamaz." : null;
    public static string? Scenario(CashflowScenarioRequest request) => request.DelayedIncomeDays < 0 ? "Gelir gecikmesi negatif olamaz." : request.UnexpectedExpenseAmount is <= 0 ? "Beklenmedik gider tutarı pozitif olmalı." : request.AdditionalIncomeAmount is <= 0 ? "Ek gelir tutarı pozitif olmalı." : request.UnexpectedExpenseAmount is not null && request.UnexpectedExpenseDate is null ? "Beklenmedik gider tarihi gerekli." : request.AdditionalIncomeAmount is not null && request.AdditionalIncomeDate is null ? "Ek gelir tarihi gerekli." : null;
}
