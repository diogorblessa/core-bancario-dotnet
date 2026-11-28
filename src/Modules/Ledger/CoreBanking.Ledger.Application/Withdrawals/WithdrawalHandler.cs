using CoreBanking.Ledger.Application.Abstractions;
using CoreBanking.Ledger.Application.Postings;
using CoreBanking.Ledger.Domain;
using CoreBanking.Ledger.Domain.Accounts;
using CoreBanking.Ledger.Domain.Entries;

namespace CoreBanking.Ledger.Application.Withdrawals;

public sealed class WithdrawalHandler(IAccountRepository accounts, PostingWorkflow workflow, TimeProvider time)
{
    public async Task<PostingResult> HandleAsync(WithdrawalCommand command, CancellationToken ct)
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
            command.Description ?? "Saque",
            command.ExternalReference,
            [
                new PostingLine(customer.Id, PostingSide.Debit, amount),
                new PostingLine(settlement.Id, PostingSide.Credit, amount),
            ]);

        return await workflow.PostAsync(entry, ct);
    }
}
