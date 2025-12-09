using System.Collections.Generic;

namespace Domain.Entities;

public class StoreOwner : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Cpf { get; private set; } = null!;
    
    public virtual ICollection<Store> Stores { get; private set; } = new List<Store>();

    private StoreOwner() { }

    public static StoreOwner Create(string name, string cpf)
    {
        return new StoreOwner
        {
            Name = name,
            Cpf = cpf
        };
    }
}
