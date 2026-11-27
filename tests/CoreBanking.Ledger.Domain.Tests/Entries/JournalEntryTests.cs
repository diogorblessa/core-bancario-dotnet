using CoreBanking.Ledger.Domain.Accounts;
using CoreBanking.Ledger.Domain.Entries;

namespace CoreBanking.Ledger.Domain.Tests.Entries;

public class JournalEntryTests
{
    private static readonly Guid SettlementAccountId = Guid.NewGuid();
    private static readonly Guid CustomerAccountId = Guid.NewGuid();
    private static readonly DateOnly BookingDate = new(2026, 11, 26);
    private static readonly DateTimeOffset RecordedAt = new(2026, 11, 26, 15, 0, 0, TimeSpan.Zero);

    private static JournalEntry CreateDeposit(decimal amount) =>
        JournalEntry.Create(
            BookingDate,
            RecordedAt,
            "Depósito do Cliente Fictício 1",
            "EXT-0001",
            [
                new PostingLine(SettlementAccountId, PostingSide.Debit, Money.Of(amount)),
                new PostingLine(CustomerAccountId, PostingSide.Credit, Money.Of(amount)),
            ]);

    [Fact]
    public void Create_Balanced_Succeeds()
    {
        var entry = CreateDeposit(100m);

        Assert.Equal(2, entry.Postings.Count);
        Assert.Equal(BookingDate, entry.BookingDate);
        Assert.Equal("Depósito do Cliente Fictício 1", entry.Description);
        Assert.Equal("EXT-0001", entry.ExternalReference);
        Assert.Null(entry.ReversalOf);
    }

    [Fact]
    public void Create_SinglePosting_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => JournalEntry.Create(
            BookingDate,
            RecordedAt,
            "Lançamento incompleto",
            null,
            [new PostingLine(SettlementAccountId, PostingSide.Debit, Money.Of(100m))]));

        Assert.Equal("entry.too_few_postings", ex.Code);
    }

    [Fact]
    public void Create_ZeroAmount_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => CreateDeposit(0m));

        Assert.Equal("posting.non_positive_amount", ex.Code);
    }

    [Fact]
    public void Create_Unbalanced_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => JournalEntry.Create(
            BookingDate,
            RecordedAt,
            "Lançamento desbalanceado",
            null,
            [
                new PostingLine(SettlementAccountId, PostingSide.Debit, Money.Of(100.00m)),
                new PostingLine(CustomerAccountId, PostingSide.Credit, Money.Of(99.99m)),
            ]));

        Assert.Equal("entry.unbalanced", ex.Code);
    }

    [Fact]
    public void Create_UndefinedSide_Throws()
    {
        var undefinedSide = (PostingSide)99;

        var ex = Assert.Throws<DomainException>(() => JournalEntry.Create(
            BookingDate,
            RecordedAt,
            "Lançamento com lado inválido",
            null,
            [
                new PostingLine(SettlementAccountId, undefinedSide, Money.Of(10m)),
                new PostingLine(CustomerAccountId, undefinedSide, Money.Of(10m)),
            ]));

        Assert.Equal("posting.invalid_side", ex.Code);
    }

    [Fact]
    public void Create_EmptyDescription_Throws()
    {
        var ex = Assert.Throws<DomainException>(() => JournalEntry.Create(
            BookingDate,
            RecordedAt,
            "   ",
            null,
            [
                new PostingLine(SettlementAccountId, PostingSide.Debit, Money.Of(10m)),
                new PostingLine(CustomerAccountId, PostingSide.Credit, Money.Of(10m)),
            ]));

        Assert.Equal("entry.description_required", ex.Code);
    }

    [Fact]
    public void Create_MultipleLines_BalancedAcrossLines_Succeeds()
    {
        var otherCustomerAccountId = Guid.NewGuid();

        var entry = JournalEntry.Create(
            BookingDate,
            RecordedAt,
            "Depósito dividido entre duas contas",
            null,
            [
                new PostingLine(SettlementAccountId, PostingSide.Debit, Money.Of(100m)),
                new PostingLine(CustomerAccountId, PostingSide.Credit, Money.Of(60m)),
                new PostingLine(otherCustomerAccountId, PostingSide.Credit, Money.Of(40m)),
            ]);

        Assert.Equal(3, entry.Postings.Count);
    }

    [Fact]
    public void AccountIds_ReturnsEachAccountOnce()
    {
        var entry = JournalEntry.Create(
            BookingDate,
            RecordedAt,
            "Dois créditos na mesma conta",
            null,
            [
                new PostingLine(SettlementAccountId, PostingSide.Debit, Money.Of(100m)),
                new PostingLine(CustomerAccountId, PostingSide.Credit, Money.Of(60m)),
                new PostingLine(CustomerAccountId, PostingSide.Credit, Money.Of(40m)),
            ]);

        Assert.Equal(2, entry.AccountIds.Count);
        Assert.Contains(SettlementAccountId, entry.AccountIds);
        Assert.Contains(CustomerAccountId, entry.AccountIds);
    }

    [Fact]
    public void CreateReversal_FlipsSidesAndLinksOriginal()
    {
        var original = CreateDeposit(100m);
        var reversalDate = new DateOnly(2026, 11, 27);

        var reversal = original.CreateReversal(reversalDate, RecordedAt.AddDays(1));

        Assert.Equal(original.Id, reversal.ReversalOf);
        Assert.Equal(reversalDate, reversal.BookingDate);
        Assert.Equal($"Estorno de {original.Id}", reversal.Description);

        var settlement = Assert.Single(reversal.Postings, p => p.AccountId == SettlementAccountId);
        Assert.Equal(PostingSide.Credit, settlement.Side);
        Assert.Equal(Money.Of(100m), settlement.Amount);

        var customer = Assert.Single(reversal.Postings, p => p.AccountId == CustomerAccountId);
        Assert.Equal(PostingSide.Debit, customer.Side);
        Assert.Equal(Money.Of(100m), customer.Amount);
    }

    [Fact]
    public void CreateReversal_OfReversal_Throws()
    {
        var reversal = CreateDeposit(100m).CreateReversal(BookingDate, RecordedAt);

        var ex = Assert.Throws<DomainException>(() => reversal.CreateReversal(BookingDate, RecordedAt));

        Assert.Equal("entry.cannot_reverse_reversal", ex.Code);
    }
}
