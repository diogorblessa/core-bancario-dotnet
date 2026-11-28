using CoreBanking.Ledger.Application.Abstractions;
using CoreBanking.Ledger.Application.Postings;
using CoreBanking.Ledger.Domain;
using CoreBanking.Ledger.Domain.Accounts;
using CoreBanking.Ledger.Domain.Entries;

namespace CoreBanking.Ledger.Application.Transfers;

public sealed class TransferHandler(IAccountRepository accounts, PostingWorkflow workflow, TimeProvider time)
{
    public async Task<PostingResult> HandleAsync(TransferCommand command, CancellationToken ct)
    {
        if (command.FromAccountId == command.ToAccountId)
        {
            throw new DomainException("transfer.same_account", "Cannot transfer to the same account.");
        }

        var from = await accounts.GetCustomerAccountAsync(command.FromAccountId, ct);
        var to = await accounts.GetCustomerAccountAsync(command.ToAccountId, ct);
        var amount = Money.Of(command.Amount);

        var entry = JournalEntry.Create(
            command.BookingDate ?? LedgerClock.Today(time),
            time.GetUtcNow(),
            command.Description ?? "Transferência",
            externalReference: null,
            [
                new PostingLine(from.Id, PostingSide.Debit, amount),
                new PostingLine(to.Id, PostingSide.Credit, amount),
            ]);

        return await workflow.PostAsync(entry, ct);
    }
}
