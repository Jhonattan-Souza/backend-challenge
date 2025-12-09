using System;
using System.Linq;
using Domain.Enums;
using Domain.Validators;
using FluentResults;

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

    public Transaction(
        TransactionType type,
        DateTimeOffset date,
        decimal amount,
        string cpf,
        string cardNumber,
        Store store)
    {
        Type = type;
        Date = date;
        Amount = amount;
        Cpf = cpf;
        CardNumber = cardNumber;
        Store = store;
        StoreId = store.Id;
    }

    public static Result<Transaction> Create(
        TransactionType type,
        DateTimeOffset date,
        decimal amount,
        string cpf,
        string cardNumber,
        Store store)
    {
        var transaction = new Transaction(type, date, amount, cpf, cardNumber, store);
        return Validate(transaction, new TransactionValidator());
    }
}