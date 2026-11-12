namespace Finaco.Api.Domain;

public enum AccountType { Bank, Cash, CreditCard }
public enum CategoryKind { Income, Expense }
public enum TransactionType { Income, Expense, Transfer, Refund, CardPayment }
public enum ImportStatus { Pending, Confirmed, Failed }

