using CoreBanking.Ledger.Application.Abstractions;
using CoreBanking.Ledger.Application.Postings;
using CoreBanking.Ledger.Domain;
using CoreBanking.Ledger.Domain.Accounts;
using CoreBanking.Ledger.Domain.Entries;

namespace CoreBanking.Ledger.Application.Deposits;

public sealed class DepositHandler(IAccountRepository accounts, PostingWorkflow workflow, TimeProvider time)
{
    public async Task<PostingResult> HandleAsync(DepositCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.ExternalReference))
        {
            throw new DomainException("entry.external_reference_required", "External reference is required.");
        }

        var customer = await accounts.GetCustomerAccountAsync(command.AccountId, ct);
        var settlement = await accounts.GetByCodeAsync(ChartOfAccounts.SettlementAccount, ct);
        var amount = Money.Of(command.Amount);

        var entry = JournalEntry.Create(
            command.BookingDate ?? LedgerClock.Today(time),
            time.GetUtcNow(),
            command.Description ?? "Depósito",
            command.ExternalReference,
            [
                new PostingLine(settlement.Id, PostingSide.Debit, amount),
                new PostingLine(customer.Id, PostingSide.Credit, amount),
            ]);

        return await workflow.PostAsync(entry, ct);
    }
}
