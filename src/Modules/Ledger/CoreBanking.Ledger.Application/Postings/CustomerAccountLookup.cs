using CoreBanking.Ledger.Application.Abstractions;
using CoreBanking.Ledger.Domain;
using CoreBanking.Ledger.Domain.Accounts;

namespace CoreBanking.Ledger.Application.Postings;

internal static class CustomerAccountLookup
{
    public static async Task<Account> GetCustomerAccountAsync(
        this IAccountRepository accounts,
        Guid accountId,
        CancellationToken ct)
    {
        var account = await accounts.GetByIdAsync(accountId, ct) ?? throw new NotFoundException("Account", accountId);

        if (account.Type != AccountType.CustomerDeposit)
        {
            throw new DomainException("account.not_customer_account", "Operation requires a customer account.");
        }

        return account;
    }
}
