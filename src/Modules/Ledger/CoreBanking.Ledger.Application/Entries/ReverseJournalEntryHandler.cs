using CoreBanking.Ledger.Application.Abstractions;
using CoreBanking.Ledger.Application.Postings;
using CoreBanking.Ledger.Domain;

namespace CoreBanking.Ledger.Application.Entries;

public sealed class ReverseJournalEntryHandler(
    IJournalEntryRepository entries,
    PostingWorkflow workflow,
    TimeProvider time)
{
    public async Task<PostingResult> HandleAsync(ReverseJournalEntryCommand command, CancellationToken ct)
    {
        var original = await entries.GetByIdAsync(command.JournalEntryId, ct)
            ?? throw new NotFoundException("JournalEntry", command.JournalEntryId);

        if (await entries.HasReversalAsync(original.Id, ct))
        {
            throw new DomainException("entry.already_reversed", "Entry has already been reversed.");
        }

        // The reversal is always booked today, even when the original is in a closed period.
        var reversal = original.CreateReversal(LedgerClock.Today(time), time.GetUtcNow());

        return await workflow.PostAsync(reversal, ct);
    }
}
