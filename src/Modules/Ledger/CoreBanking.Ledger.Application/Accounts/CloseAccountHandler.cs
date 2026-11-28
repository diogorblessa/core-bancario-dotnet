using CoreBanking.Ledger.Application.Abstractions;

namespace CoreBanking.Ledger.Application.Accounts;

public sealed class CloseAccountHandler(IAccountRepository accounts, IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(CloseAccountCommand command, CancellationToken ct)
    {
        var account = await accounts.GetByIdAsync(command.AccountId, ct)
            ?? throw new NotFoundException("Account", command.AccountId);

        account.Close();
        await unitOfWork.SaveChangesAsync(ct);
    }
}
