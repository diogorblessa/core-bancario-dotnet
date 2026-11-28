namespace CoreBanking.Ledger.Domain.Customers;

public sealed class Customer
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public CpfNumber Document { get; private set; }

    // Used by EF Core to materialize customers from the database.
    private Customer()
    {
    }

    public static Customer Register(string name, CpfNumber document)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("customer.name_required", "Customer name is required.");
        }

        return new Customer
        {
            Id = Guid.NewGuid(),
            Name = name,
            Document = document,
        };
    }
}
