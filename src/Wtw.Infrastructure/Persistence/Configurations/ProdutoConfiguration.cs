using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wtw.Domain.Comum;
using Wtw.Domain.Produtos;
using Wtw.Infrastructure.Persistence.Seed;

namespace Wtw.Infrastructure.Persistence.Configurations;

internal sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("Produtos");

        builder.HasKey(produto => produto.Id);
        builder.Property(produto => produto.Id).ValueGeneratedNever();

        builder.Property(produto => produto.Nome)
            .HasConversion(nome => nome.Valor, valor => Nome.Criar(valor).Data)
            .HasMaxLength(Nome.TamanhoMaximo)
            .IsRequired();

        builder.Property(produto => produto.Descricao)
            .HasConversion(descricao => descricao.Valor, valor => Descricao.Criar(valor).Data)
            .HasMaxLength(Descricao.TamanhoMaximo)
            .IsRequired();

        builder.Property(produto => produto.Preco)
            .HasConversion(preco => preco.Valor, valor => Preco.Criar(valor).Data)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(produto => produto.Estoque)
            .HasConversion(estoque => estoque.Valor, valor => Estoque.Criar(valor).Data)
            .IsRequired();

        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.Ignore(produto => produto.Eventos);

        builder.HasData(DadosIniciais.Produtos);
    }
}
