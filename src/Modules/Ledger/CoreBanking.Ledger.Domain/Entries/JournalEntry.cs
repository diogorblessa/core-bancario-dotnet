using CoreBanking.Ledger.Domain.Accounts;

namespace CoreBanking.Ledger.Domain.Entries;

public sealed class JournalEntry
{
    private readonly List<Posting> _postings = [];

    public Guid Id { get; private set; }

    public DateOnly BookingDate { get; private set; }

    public DateTimeOffset RecordedAt { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public string? ExternalReference { get; private set; }

    public Guid? ReversalOf { get; private set; }

    public IReadOnlyList<Posting> Postings => _postings;

    public IReadOnlyCollection<Guid> AccountIds => _postings.Select(p => p.AccountId).Distinct().ToArray();

    // Used by EF Core to materialize entries from the database.
    private JournalEntry()
    {
    }

    public static JournalEntry Create(
        DateOnly bookingDate,
        DateTimeOffset recordedAt,
        string description,
        string? externalReference,
        IReadOnlyList<PostingLine> lines) =>
        Build(bookingDate, recordedAt, description, externalReference, reversalOf: null, lines);

    public JournalEntry CreateReversal(DateOnly bookingDate, DateTimeOffset recordedAt)
    {
        if (ReversalOf is not null)
        {
            throw new DomainException("entry.cannot_reverse_reversal", "A reversal cannot be reversed.");
        }

        var lines = _postings
            .Select(p => new PostingLine(p.AccountId, Opposite(p.Side), p.Amount))
            .ToList();

        return Build(bookingDate, recordedAt, $"Estorno de {Id}", externalReference: null, reversalOf: Id, lines);
    }

    private static JournalEntry Build(
        DateOnly bookingDate,
        DateTimeOffset recordedAt,
        string description,
        string? externalReference,
        Guid? reversalOf,
        IReadOnlyList<PostingLine> lines)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("entry.description_required", "Description is required.");
        }

        if (lines.Count < 2)
        {
            throw new DomainException("entry.too_few_postings", "An entry needs at least two postings.");
        }

        if (lines.Any(l => !Enum.IsDefined(l.Side)))
        {
            throw new DomainException("posting.invalid_side", "Posting side must be debit or credit.");
        }

        if (lines.Any(l => !l.Amount.IsPositive))
        {
            throw new DomainException("posting.non_positive_amount", "Posting amounts must be positive.");
        }

        var debits = lines.Where(l => l.Side == PostingSide.Debit).Sum(l => l.Amount.Amount);
        var credits = lines.Where(l => l.Side == PostingSide.Credit).Sum(l => l.Amount.Amount);

        if (debits != credits)
        {
            throw new DomainException("entry.unbalanced", "Total debits must equal total credits.");
        }

        var entry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            BookingDate = bookingDate,
            RecordedAt = recordedAt,
            Description = description,
            ExternalReference = externalReference,
            ReversalOf = reversalOf,
        };

        entry._postings.AddRange(lines.Select(l => new Posting(l.AccountId, l.Side, l.Amount)));

        return entry;
    }

    private static PostingSide Opposite(PostingSide side) =>
        side == PostingSide.Debit ? PostingSide.Credit : PostingSide.Debit;
}
