using Finaco.Api.Application.Cashflow;
using Finaco.Api.Domain;
using Xunit;

namespace Finaco.Api.Tests;

public sealed class CashflowCalculatorTests
{
    [Fact]
    public void Projects_daily_balance_with_income_and_expense()
    {
        var start = new DateOnly(2026, 11, 1);
        var transactions = new[] { new Transaction { TransactionDate = start.AddDays(1), Amount = 25m, Type = TransactionType.Expense } };
        var points = CashflowCalculator.Project(start, 100m, start.AddDays(2), 50m, transactions);
        Assert.Equal(new[] { 100m, 75m, 125m }, points.Select(x => x.Balance));
        Assert.All(points, x => Assert.True(x.IsProjected));
    }

    [Fact]
    public void Scenario_expense_starts_on_its_date_and_delayed_income_moves()
    {
        var start = new DateOnly(2026, 11, 1);
        var scenario = new CashflowScenarioRequest { UnexpectedExpenseAmount = 20m, UnexpectedExpenseDate = start.AddDays(1), DelayedIncomeDays = 1 };
        var points = CashflowCalculator.Project(start, 100m, start.AddDays(2), 50m, Array.Empty<Transaction>(), scenario);
        Assert.Equal(new[] { 100m, 80m, 80m, 130m }, points.Select(x => x.Balance));
    }

    [Fact]
    public void Transfer_and_card_payment_do_not_change_balance()
    {
        var start = new DateOnly(2026, 11, 1);
        var transactions = new[] { new Transaction { TransactionDate = start, Amount = 20m, Type = TransactionType.Transfer }, new Transaction { TransactionDate = start, Amount = 30m, Type = TransactionType.CardPayment } };
        var points = CashflowCalculator.Project(start, 100m, start, 0m, transactions);
        Assert.Equal(100m, points[0].Balance);
    }

    [Fact]
    public void Negative_opening_balance_is_rejected_by_cashflow_validation()
    {
        var error = CashflowValidation.Settings(new CashflowSettingsRequest { OpeningBalance = -1m, NextIncomeAmount = 10m });
        Assert.NotNull(error);
        Assert.NotNull(CashflowValidation.Scenario(new CashflowScenarioRequest { UnexpectedExpenseAmount = -1m }));
    }
}
