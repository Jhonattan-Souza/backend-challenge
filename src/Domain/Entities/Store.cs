using System;
using System.Collections.Generic;

namespace Domain.Entities;

public class Store : BaseEntity
{
    public string Name { get; private set; } = null!;
    
    public Guid OwnerId { get; private set; }
    public virtual StoreOwner Owner { get; private set; } = null!;
    public virtual ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();

    private Store() { }

    public static Store Create(string name, StoreOwner owner)
    {
        return new Store
        {
            Name = name,
            Owner = owner,
            OwnerId = owner.Id
        };
    }
}