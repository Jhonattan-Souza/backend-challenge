using System;
using System.Collections.Generic;
using Domain.Entities;

namespace Domain;

public class Store : BaseEntity
{
    public string Name { get; private set; }
    
    public virtual ICollection<Transaction> Transactions { get; private set; }
}