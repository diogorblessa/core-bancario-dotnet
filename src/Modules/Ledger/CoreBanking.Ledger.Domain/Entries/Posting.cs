using CoreBanking.Ledger.Domain.Accounts;

namespace CoreBanking.Ledger.Domain.Entries;

public sealed class Posting
{
    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }

    public PostingSide Side { get; private set; }

    public Money Amount { get; private set; }

    // Used by EF Core to materialize postings from the database.
    private Posting()
    {
    }

    internal Posting(Guid accountId, PostingSide side, Money amount)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        Side = side;
        Amount = amount;
    }
}
