namespace CoreBanking.Ledger.Domain.Accounts;

public sealed class Account
{
    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public AccountNature Nature { get; private set; }

    public AccountType Type { get; private set; }

    public AccountStatus Status { get; private set; }

    public Guid? CustomerId { get; private set; }

    public Money CurrentBalance { get; private set; }

    // Used by EF Core to materialize accounts from the database.
    private Account()
    {
    }

    public static Account CreateInternal(string code, string name, AccountNature nature) => new()
    {
        Id = Guid.NewGuid(),
        Code = code,
        Name = name,
        Nature = nature,
        Type = AccountType.Internal,
        Status = AccountStatus.Active,
        CurrentBalance = Money.Zero,
    };

    public static Account OpenCustomerDeposit(string code, string name, Guid customerId) => new()
    {
        Id = Guid.NewGuid(),
        Code = code,
        Name = name,
        Nature = AccountNature.Liability,
        Type = AccountType.CustomerDeposit,
        Status = AccountStatus.Active,
        CustomerId = customerId,
        CurrentBalance = Money.Zero,
    };

    public void Apply(PostingSide side, Money amount)
    {
        if (Status == AccountStatus.Closed)
        {
            throw new DomainException("account.closed", "Cannot post to a closed account.");
        }

        if (Type == AccountType.Internal)
        {
            return;
        }

        var debits = side == PostingSide.Debit ? amount : Money.Zero;
        var credits = side == PostingSide.Credit ? amount : Money.Zero;
        var newBalance = CurrentBalance.Amount + BalanceFrom(Nature, debits, credits);

        if (newBalance < 0m)
        {
            throw new DomainException("account.insufficient_balance", "Insufficient balance.");
        }

        CurrentBalance = Money.Of(newBalance);
    }

    public void Close()
    {
        if (Type == AccountType.Internal)
        {
            throw new DomainException("account.cannot_close_internal", "Internal accounts cannot be closed.");
        }

        if (Status == AccountStatus.Closed)
        {
            throw new DomainException("account.already_closed", "Account is already closed.");
        }

        if (!CurrentBalance.IsZero)
        {
            throw new DomainException("account.balance_not_zero", "Account balance must be zero to close.");
        }

        Status = AccountStatus.Closed;
    }

    public static decimal BalanceFrom(AccountNature nature, Money debits, Money credits) => nature switch
    {
        AccountNature.Asset or AccountNature.Expense => debits.Amount - credits.Amount,
        AccountNature.Liability or AccountNature.Equity or AccountNature.Revenue => credits.Amount - debits.Amount,
        _ => throw new ArgumentOutOfRangeException(nameof(nature), nature, "Unknown account nature."),
    };
}
