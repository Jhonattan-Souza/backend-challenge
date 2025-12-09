using System;
using System.Collections.Generic;
using Domain.Validators;
using FluentResults;

namespace Domain.Entities;

public class Store : BaseEntity
{
    public string Name { get; private set; } = null!;
    
    public Guid OwnerId { get; private set; }
    public virtual StoreOwner Owner { get; private set; } = null!;
    public virtual ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();

    private Store() { }

    public Store(string name, StoreOwner owner)
    {
        Name = name;
        Owner = owner;
        OwnerId = owner.Id;
    }

    public static Result<Store> Create(string name, StoreOwner owner)
    {
        var store = new Store(name, owner);
        return Validate(store, new StoreValidator());
    }
}