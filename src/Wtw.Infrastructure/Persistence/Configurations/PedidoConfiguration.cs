using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wtw.Domain.Clientes;
using Wtw.Domain.Pedidos;

namespace Wtw.Infrastructure.Persistence.Configurations;

internal sealed class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");

        builder.HasKey(pedido => pedido.Id);
        builder.Property(pedido => pedido.Id).ValueGeneratedNever();

        builder.Property(pedido => pedido.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(pedido => pedido.CriadoEmUtc)
            .HasConversion(data => data, data => DateTime.SpecifyKind(data, DateTimeKind.Utc))
            .IsRequired();

        builder.Property(pedido => pedido.AtualizadoEmUtc)
            .HasConversion(data => data, data => data.HasValue ? DateTime.SpecifyKind(data.Value, DateTimeKind.Utc) : data);

        builder.HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(pedido => pedido.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(pedido => pedido.Itens)
            .WithOne()
            .HasForeignKey(item => item.PedidoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(pedido => pedido.Itens)
            .HasField("_itens")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Shadow property: controle de concorrência otimista do SQLServer.
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(pedido => pedido.Status);
        builder.HasIndex(pedido => pedido.CriadoEmUtc);

        builder.Ignore(pedido => pedido.ValorTotal);
        builder.Ignore(pedido => pedido.Eventos);
    }
}
