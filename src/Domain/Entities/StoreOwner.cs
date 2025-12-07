using System.Collections.Generic;

namespace Domain;

public class StoreOwner : BaseEntity
{
    public string Name { get; private set; }
    public string Cpf { get; private set; }
    
    public virtual ICollection<Store> Stores { get; private set; }
}
