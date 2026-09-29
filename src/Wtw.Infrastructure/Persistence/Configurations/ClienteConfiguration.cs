using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wtw.Domain.Clientes;
using Wtw.Domain.Comum;
using Wtw.Infrastructure.Persistence.Seed;

namespace Wtw.Infrastructure.Persistence.Configurations;

internal sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(cliente => cliente.Id);
        builder.Property(cliente => cliente.Id).ValueGeneratedNever();

        builder.Property(cliente => cliente.Nome)
            .HasConversion(nome => nome.Valor, valor => Nome.Criar(valor).Data)
            .HasMaxLength(Nome.TamanhoMaximo)
            .IsRequired();

        builder.Property(cliente => cliente.Email)
            .HasConversion(email => email.Valor, valor => Email.Criar(valor).Data)
            .HasMaxLength(Email.TamanhoMaximo)
            .IsRequired();

        builder.HasIndex(cliente => cliente.Email).IsUnique();

        builder.Ignore(cliente => cliente.Eventos);

        builder.HasData(DadosIniciais.Clientes);
    }
}
