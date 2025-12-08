using System.Collections.Generic;

namespace Domain.Entities;

public class Store : BaseEntity
{
    public string Name { get; private set; }
    
    public virtual StoreOwner Owner { get; private set; }
    public virtual ICollection<Transaction> Transactions { get; private set; }
}