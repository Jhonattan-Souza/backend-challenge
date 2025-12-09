using System.Collections.Generic;
using Domain.Validators;
using FluentResults;

namespace Domain.Entities;

public class StoreOwner : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Cpf { get; private set; } = null!;
    
    public virtual ICollection<Store> Stores { get; private set; } = new List<Store>();

    private StoreOwner() { }

    public StoreOwner(string name, string cpf)
    {
        Name = name;
        Cpf = cpf;
    }

    public static Result<StoreOwner> Create(string name, string cpf)
    {
        var storeOwner = new StoreOwner(name, cpf);
        return Validate(storeOwner, new StoreOwnerValidator());
    }
}
