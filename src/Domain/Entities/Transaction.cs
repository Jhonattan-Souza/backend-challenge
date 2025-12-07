using System;
using Domain.Enums;

namespace Domain.Entities;

public class Transaction : BaseEntity
{
    public TransactionType Type { get; private set; }
    public DateTimeOffset Date { get; private set; }
    public decimal Amount { get; private set; }
    public string Cpf { get; private set; }
    public string CardNumber { get; private set; }
    
    public virtual Store Store { get; private set; }
}