using CoreBanking.Ledger.Domain.Accounts;

namespace CoreBanking.Ledger.Domain.Tests.Accounts;

public class AccountTests
{
    private static Account NewCustomerAccount() =>
        Account.OpenCustomerDeposit("4.1.1.000001", "Cliente Fictício 1", Guid.NewGuid());

    private static Account NewSettlementAccount() =>
        Account.CreateInternal(ChartOfAccounts.SettlementAccount, "Conta de liquidação", AccountNature.Asset);

    [Fact]
    public void OpenCustomerDeposit_StartsActiveLiabilityWithZeroBalance()
    {
        var customerId = Guid.NewGuid();

        var account = Account.OpenCustomerDeposit("4.1.1.000002", "Cliente Fictício 2", customerId);

        Assert.Equal(AccountNature.Liability, account.Nature);
        Assert.Equal(AccountType.CustomerDeposit, account.Type);
        Assert.Equal(AccountStatus.Active, account.Status);
        Assert.Equal(customerId, account.CustomerId);
        Assert.True(account.CurrentBalance.IsZero);
    }

    [Fact]
    public void Apply_CreditOnCustomerAccount_IncreasesBalance()
    {
        var account = NewCustomerAccount();

        account.Apply(PostingSide.Credit, Money.Of(100m));

        Assert.Equal(Money.Of(100m), account.CurrentBalance);
    }

    [Fact]
    public void Apply_DebitWithinBalance_DecreasesBalance()
    {
        var account = NewCustomerAccount();
        account.Apply(PostingSide.Credit, Money.Of(100m));

        account.Apply(PostingSide.Debit, Money.Of(30m));

        Assert.Equal(Money.Of(70m), account.CurrentBalance);
    }

    [Fact]
    public void Apply_DebitBeyondBalance_Throws()
    {
        var account = NewCustomerAccount();
        account.Apply(PostingSide.Credit, Money.Of(50m));

        var ex = Assert.Throws<DomainException>(() => account.Apply(PostingSide.Debit, Money.Of(50.01m)));

        Assert.Equal("account.insufficient_balance", ex.Code);
        Assert.Equal(Money.Of(50m), account.CurrentBalance);
    }

    [Fact]
    public void Apply_OnClosedAccount_Throws()
    {
        var account = NewCustomerAccount();
        account.Close();

        var ex = Assert.Throws<DomainException>(() => account.Apply(PostingSide.Credit, Money.Of(10m)));

        Assert.Equal("account.closed", ex.Code);
    }

    [Fact]
    public void Apply_OnInternalAccount_DoesNotStoreBalance()
    {
        var account = NewSettlementAccount();

        account.Apply(PostingSide.Debit, Money.Of(100m));

        Assert.True(account.CurrentBalance.IsZero);
    }

    [Fact]
    public void Close_WithBalance_Throws()
    {
        var account = NewCustomerAccount();
        account.Apply(PostingSide.Credit, Money.Of(10m));

        var ex = Assert.Throws<DomainException>(account.Close);

        Assert.Equal("account.balance_not_zero", ex.Code);
    }

    [Fact]
    public void Close_InternalAccount_Throws()
    {
        var account = NewSettlementAccount();

        var ex = Assert.Throws<DomainException>(account.Close);

        Assert.Equal("account.cannot_close_internal", ex.Code);
    }

    [Fact]
    public void Close_Twice_Throws()
    {
        var account = NewCustomerAccount();
        account.Close();

        var ex = Assert.Throws<DomainException>(account.Close);

        Assert.Equal("account.already_closed", ex.Code);
    }

    [Fact]
    public void BalanceFrom_Asset_IsDebitsMinusCredits()
    {
        var balance = Account.BalanceFrom(AccountNature.Asset, Money.Of(100m), Money.Of(30m));

        Assert.Equal(70m, balance);
    }

    [Fact]
    public void BalanceFrom_Liability_IsCreditsMinusDebits()
    {
        var balance = Account.BalanceFrom(AccountNature.Liability, Money.Of(100m), Money.Of(30m));

        Assert.Equal(-70m, balance);
    }
}
