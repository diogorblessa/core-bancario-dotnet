using CoreBanking.Ledger.Application.Abstractions;
using CoreBanking.Ledger.Domain;
using CoreBanking.Ledger.Domain.Accounts;
using CoreBanking.Ledger.Domain.Customers;

namespace CoreBanking.Ledger.Application.Customers;

public sealed class CreateCustomerHandler(
    ICustomerRepository customers,
    IAccountRepository accounts,
    IUnitOfWork unitOfWork)
{
    public async Task<CustomerCreated> HandleAsync(CreateCustomerCommand command, CancellationToken ct)
    {
        var customer = Customer.Register(command.Name, CpfNumber.Parse(command.Document));

        if (await customers.ExistsByDocumentAsync(customer.Document, ct))
        {
            throw new DomainException("customer.document_already_registered", "Document is already registered.");
        }

        var number = await customers.NextCustomerNumberAsync(ct);
        var code = $"{ChartOfAccounts.CustomerDepositsPrefix}.{number:D6}";
        var account = Account.OpenCustomerDeposit(code, customer.Name, customer.Id);

        customers.Add(customer);
        accounts.Add(account);
        await unitOfWork.SaveChangesAsync(ct);

        return new CustomerCreated(customer.Id, account.Id, account.Code);
    }
}
