using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Store");
        
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();
        
        builder.Property(o => o.Name).HasMaxLength(100).IsRequired();
        
        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.UpdatedAt).IsRequired();

        builder.HasOne(x => x.Owner)
            .WithMany(o => o.Stores)
            .HasForeignKey(s => s.OwnerId);
        
        builder.HasIndex(o => o.Name)
            .IsUnique()
            .HasDatabaseName("IX_StoreName");
    }
}