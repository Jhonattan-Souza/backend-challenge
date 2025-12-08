using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Type).IsRequired();

        builder.Property(t => t.Date).IsRequired();

        builder.Property(t => t.Amount).HasPrecision(18, 2).IsRequired();

        builder.Property(t => t.Cpf).HasMaxLength(11).IsRequired();

        builder.Property(t => t.CardNumber).HasMaxLength(12).IsRequired();

        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();
        
        builder.HasOne(t => t.Store).WithMany(s => s.Transactions);
    }
}
