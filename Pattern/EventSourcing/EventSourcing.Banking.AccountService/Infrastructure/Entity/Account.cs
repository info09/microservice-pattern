using EventSourcing.Banking.AccountService.DomainEvents;
using EventSourcing.Banking.SeedWork;

namespace EventSourcing.Banking.AccountService.Infrastructure.Entity;

public class Account : Aggregate
{
    public string AccountNumber { get; set; } = default!;
    public string Currency { get; set; } = default!;
    public decimal Balance { get; set; }
    public decimal CurrentCredit { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsClosed { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime BalanceChangedAtUtc { get; set; }
    public List<Transaction> Transactions { get; set; } = [];

    public Account()
    {

    }

    public static Account Create(Guid id, string accountNumber, string currency, decimal balance, decimal creditLimit)
    {
        var account = new Account
        {
            Id = id,
            AccountNumber = accountNumber,
            Currency = currency,
            Balance = balance,
            CreditLimit = creditLimit,
            CreatedAtUtc = DateTime.UtcNow,
            BalanceChangedAtUtc = DateTime.UtcNow,
            IsClosed = false,
            Version = 0
        };

        var evt = new AccountOpenedEvent(account.Id, account.AccountNumber, account.Currency, account.Balance, account.CreditLimit);
        account.Apply(evt);
        account._changes.Add(evt);

        return account;
    }

    public void Deposit(decimal amount)
    {
        if (IsClosed)
        {
            throw new InvalidOperationException("Account is closed");
        }
        var evt = new MoneyDepositedEvent(Id, amount);
        Apply(evt);
        _changes.Add(evt);
    }

    public void Withdraw(decimal amount)
    {
        if (IsClosed)
        {
            throw new InvalidOperationException("Account is closed");
        }
        if (Balance < amount)
        {
            throw new InvalidOperationException("Insufficient balance");
        }
        var evt = new MoneyWithdrawnEvent(Id, amount);
        Apply(evt);
        _changes.Add(evt);
    }

    public void Close()
    {
        if (IsClosed)
        {
            throw new InvalidOperationException("Account is already closed");
        }
        var evt = new AccountClosedEvent(Id);
        Apply(evt);
        _changes.Add(evt);
    }

    public void AssignCreditLimit(decimal creditLimit)
    {
        if (IsClosed)
        {
            throw new InvalidOperationException("Account is closed");
        }
        var evt = new CreditLimitAssignedEvent(Id, creditLimit);
        Apply(evt);
        _changes.Add(evt);
    }

    #region Apply Method
    public void Apply(AccountOpenedEvent @event)
    {
        AccountNumber = @event.AccountNumber;
        Currency = @event.Currency;
        Balance = @event.InitialBalance;
        CreditLimit = @event.CreditLimit;
        IsClosed = false;
        CreatedAtUtc = @event.TimeStamp;
        BalanceChangedAtUtc = @event.TimeStamp;
    }

    public void Apply(MoneyDepositedEvent @event)
    {
        if (@event.AccountId != Id)
        {
            throw new InvalidOperationException("Invalid account id");
        }

        Transactions.Add(new Entity.Transaction
        {
            Id = @event.EventId,
            AccountId = Id,
            Amount = @event.Amount,
            TimeStamp = @event.TimeStamp
        });
        Balance += @event.Amount;
        BalanceChangedAtUtc = @event.TimeStamp;
    }

    public void Apply(MoneyWithdrawnEvent @event)
    {
        if (@event.AccountId != Id)
        {
            throw new InvalidOperationException("Invalid account id");
        }

        if (IsClosed)
        {
            throw new InvalidOperationException("Account is closed");
        }

        if (Balance + (CreditLimit - CurrentCredit) < @event.Amount)
        {
            throw new InvalidOperationException("Insufficient funds");
        }

        Transactions.Add(new Entity.Transaction
        {
            Id = @event.EventId,
            AccountId = Id,
            Amount = -@event.Amount,
            TimeStamp = @event.TimeStamp
        });
        Balance -= @event.Amount;
        BalanceChangedAtUtc = @event.TimeStamp;
    }

    public void Apply(AccountClosedEvent @event)
    {
        if (@event.AccountId != Id)
        {
            throw new InvalidOperationException("Invalid account id");
        }

        if (IsClosed)
        {
            throw new InvalidOperationException("Account is already closed");
        }

        IsClosed = true;
    }

    public void Apply(CreditLimitAssignedEvent @event)
    {
        if (@event.AccountId != Id)
        {
            throw new InvalidOperationException("Invalid account id");
        }

        if (IsClosed)
        {
            throw new InvalidOperationException("Account is closed");
        }

        CreditLimit = @event.CreditLimit;
    }

    #endregion
}
