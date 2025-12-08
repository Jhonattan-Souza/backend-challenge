using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class StoreOwnerConfiguration : IEntityTypeConfiguration<StoreOwner>
{
    public void Configure(EntityTypeBuilder<StoreOwner> builder)
    {
        builder.ToTable("StoreOwners");
        
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();
        
        builder.Property(o => o.Name).HasMaxLength(100).IsRequired();
        
        builder.Property(o => o.Cpf).HasMaxLength(11).IsRequired();
        
        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.UpdatedAt).IsRequired();
        
        builder.HasIndex(o => o.Cpf)
            .IsUnique()
            .HasDatabaseName("IX_StoreOwners_Cpf");
    }
}