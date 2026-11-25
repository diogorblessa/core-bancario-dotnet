namespace CoreBanking.Ledger.Domain;

public readonly record struct Money : IComparable<Money>
{
    public static Money Zero { get; } = new(0m);

    public decimal Amount { get; }

    private Money(decimal amount) => Amount = amount;

    public static Money Of(decimal amount)
    {
        if (decimal.Round(amount, 2) != amount)
        {
            throw new DomainException("money.invalid_scale", "Amount must have at most 2 decimal places.");
        }

        if (amount < 0m)
        {
            throw new DomainException("money.negative", "Amount cannot be negative.");
        }

        return new Money(amount);
    }

    public bool IsZero => Amount == 0m;

    public bool IsPositive => Amount > 0m;

    public static Money operator +(Money left, Money right) => Of(left.Amount + right.Amount);

    public static Money operator -(Money left, Money right) => Of(left.Amount - right.Amount);

    public int CompareTo(Money other) => Amount.CompareTo(other.Amount);

    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;

    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;
}
