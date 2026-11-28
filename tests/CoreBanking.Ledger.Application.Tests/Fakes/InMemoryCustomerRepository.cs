using CoreBanking.Ledger.Application.Abstractions;
using CoreBanking.Ledger.Domain.Customers;

namespace CoreBanking.Ledger.Application.Tests.Fakes;

public sealed class InMemoryCustomerRepository : ICustomerRepository
{
    private int _lastNumber;

    public List<Customer> Customers { get; } = [];

    public void Add(Customer customer) => Customers.Add(customer);

    public Task<bool> ExistsByDocumentAsync(CpfNumber document, CancellationToken ct) =>
        Task.FromResult(Customers.Any(c => c.Document == document));

    public Task<int> NextCustomerNumberAsync(CancellationToken ct) => Task.FromResult(++_lastNumber);
}
