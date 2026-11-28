using CoreBanking.Ledger.Domain.Customers;

namespace CoreBanking.Ledger.Application.Abstractions;

public interface ICustomerRepository
{
    void Add(Customer customer);

    Task<bool> ExistsByDocumentAsync(CpfNumber document, CancellationToken ct);

    Task<int> NextCustomerNumberAsync(CancellationToken ct);
}
