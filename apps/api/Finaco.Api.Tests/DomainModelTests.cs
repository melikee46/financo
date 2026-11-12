using Finaco.Api.Domain;

namespace Finaco.Api.Tests;

public sealed class DomainModelTests
{
    [Fact]
    public void Account_defaults_to_try_and_allows_missing_opening_balance()
    {
        var account = new Account();
        Assert.Equal("TRY", account.CurrencyCode);
        Assert.Null(account.OpeningBalance);
        Assert.Null(account.OpeningBalanceDate);
    }

    [Fact]
    public void Transaction_uses_a_positive_decimal_amount_and_explicit_type()
    {
        var transaction = new Transaction { Amount = 42.50m, Type = TransactionType.Expense };
        Assert.True(transaction.Amount > 0);
        Assert.Equal(TransactionType.Expense, transaction.Type);
    }

    [Fact]
    public void Non_consumption_transaction_types_are_explicit()
    {
        Assert.Contains(TransactionType.Transfer, Enum.GetValues<TransactionType>());
        Assert.Contains(TransactionType.CardPayment, Enum.GetValues<TransactionType>());
        Assert.Contains(TransactionType.Refund, Enum.GetValues<TransactionType>());
    }
}

