using System;
using Domain.Enums;

namespace Domain.Entities;

public class Transaction : BaseEntity
{
    public TransactionType Type { get; private set; }
    public DateTimeOffset Date { get; private set; }
    public decimal Amount { get; private set; }
    public string Cpf { get; private set; } = null!;
    public string CardNumber { get; private set; } = null!;
    
    public Guid StoreId { get; private set; }
    public virtual Store Store { get; private set; } = null!;

    private Transaction() { }

    public static Transaction Create(
        TransactionType type,
        DateTimeOffset date,
        decimal amount,
        string cpf,
        string cardNumber,
        Store store)
    {
        return new Transaction
        {
            Type = type,
            Date = date,
            Amount = amount,
            Cpf = cpf,
            CardNumber = cardNumber,
            Store = store,
            StoreId = store.Id
        };
    }
}