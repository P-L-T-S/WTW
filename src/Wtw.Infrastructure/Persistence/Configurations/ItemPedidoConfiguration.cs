using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wtw.Domain.Comum;
using Wtw.Domain.Pedidos;
using Wtw.Domain.Produtos;

namespace Wtw.Infrastructure.Persistence.Configurations;

internal sealed class ItemPedidoConfiguration : IEntityTypeConfiguration<ItemPedido>
{
    public void Configure(EntityTypeBuilder<ItemPedido> builder)
    {
        builder.ToTable("ItensPedido");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id).ValueGeneratedNever();

        builder.Property(item => item.NomeProduto)
            .HasMaxLength(Nome.TamanhoMaximo)
            .IsRequired();

        builder.Property(item => item.PrecoUnitario)
            .HasPrecision(18, 2)
            .IsRequired();
        builder.Property(item => item.Quantidade).IsRequired();

        builder.HasOne<Produto>()
            .WithMany()
            .HasForeignKey(item => item.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(item => new { item.PedidoId, item.ProdutoId }).IsUnique();

        builder.Ignore(item => item.Subtotal);
    }
}