using CoreBanking.Ledger.Application.Abstractions;
using CoreBanking.Ledger.Domain;
using CoreBanking.Ledger.Domain.Accounts;

namespace CoreBanking.Ledger.Application.Queries;

public sealed class GetBalanceHandler(IAccountRepository accounts, TimeProvider time)
{
    public async Task<BalanceView> HandleAsync(GetBalanceQuery query, CancellationToken ct)
    {
        var today = LedgerClock.Today(time);
        var asOf = query.AsOf ?? today;

        if (asOf > today)
        {
            throw new DomainException("balance.as_of_in_future", "Balance date cannot be in the future.");
        }

        var account = await accounts.GetByIdAsync(query.AccountId, ct)
            ?? throw new NotFoundException("Account", query.AccountId);

        // Always computed from postings, so a past date ignores later entries.
        var totals = await accounts.GetPostingTotalsAsync(account.Id, asOf, ct);
        var balance = Account.BalanceFrom(account.Nature, totals.Debits, totals.Credits);

        return new BalanceView(account.Id, account.Code, asOf, balance);
    }
}
