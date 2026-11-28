using CoreBanking.Ledger.Application.Abstractions;
using CoreBanking.Ledger.Domain;
using CoreBanking.Ledger.Domain.Entries;

namespace CoreBanking.Ledger.Application.Postings;

public sealed class PostingWorkflow(
    IAccountRepository accounts,
    IJournalEntryRepository entries,
    IAccountingCalendarRepository calendars,
    IUnitOfWork unitOfWork,
    TimeProvider time)
{
    public async Task<PostingResult> PostAsync(JournalEntry entry, CancellationToken ct)
    {
        var accountsById = (await accounts.GetByIdsAsync(entry.AccountIds, ct)).ToDictionary(a => a.Id);

        foreach (var accountId in entry.AccountIds)
        {
            if (!accountsById.ContainsKey(accountId))
            {
                throw new NotFoundException("Account", accountId);
            }
        }

        var calendar = await calendars.GetAsync(ct);
        calendar.EnsureCanBook(entry.BookingDate, LedgerClock.Today(time));

        foreach (var posting in entry.Postings)
        {
            accountsById[posting.AccountId].Apply(posting.Side, posting.Amount);
        }

        entries.Add(entry);
        await unitOfWork.SaveChangesAsync(ct);

        return new PostingResult(entry.Id);
    }
}
