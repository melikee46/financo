using Finaco.Api.Domain;

namespace Finaco.Api.Application.Cashflow;

public static class CashflowCalculator
{
    public static IReadOnlyList<CashflowPointResponse> Project(
        DateOnly startDate, decimal openingBalance, DateOnly incomeDate, decimal incomeAmount,
        IReadOnlyList<Transaction> transactions, CashflowScenarioRequest? scenario = null, DateOnly? dataCutoffDate = null)
    {
        var delayedIncomeDate = incomeDate.AddDays(Math.Max(0, scenario?.DelayedIncomeDays ?? 0));
        var endDate = delayedIncomeDate;
        if (scenario?.UnexpectedExpenseDate is { } expenseDate && expenseDate > endDate) endDate = expenseDate;
        if (scenario?.AdditionalIncomeDate is { } additionalDate && additionalDate > endDate) endDate = additionalDate;
        var points = new List<CashflowPointResponse>();
        var balance = openingBalance;
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            foreach (var transaction in transactions.Where(x => x.TransactionDate == date)) balance += SignedAmount(transaction);
            if (date == delayedIncomeDate) balance += incomeAmount;
            if (scenario?.UnexpectedExpenseAmount is > 0 && scenario.UnexpectedExpenseDate == date) balance -= scenario.UnexpectedExpenseAmount.Value;
            if (scenario?.AdditionalIncomeAmount is > 0 && scenario.AdditionalIncomeDate == date) balance += scenario.AdditionalIncomeAmount.Value;
            points.Add(new CashflowPointResponse(date, balance, dataCutoffDate is null || date > dataCutoffDate.Value));
        }
        return points;
    }

    public static decimal SignedAmount(Transaction transaction) => transaction.Type switch
    {
        TransactionType.Income or TransactionType.Refund => transaction.Amount,
        TransactionType.Expense => -transaction.Amount,
        _ => 0m
    };
}
