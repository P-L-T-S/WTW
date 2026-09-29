using Wtw.Domain.Clientes;
using Wtw.Domain.Comum;
using Wtw.Domain.Produtos;
using Wtw.Domain.Results;

namespace Wtw.Infrastructure.Persistence.Seed;

/// <summary>
/// Dados de exemplo incluídos na migration. Os Ids são fixos para que o seed seja idempotente;
/// por isso são usados objetos anônimos em vez das fábricas das entidades, que geram Ids novos.
/// Os valores ainda passam pelas fábricas dos Value Objects, garantindo que o seed respeite as regras do domínio.
/// </summary>
internal static class DadosIniciais
{
    public static readonly object[] Clientes =
    [
        Cliente("0199a000-0000-7000-8000-000000000001", "Ana Souza", "ana.souza@exemplo.com"),
        Cliente("0199a000-0000-7000-8000-000000000002", "Bruno Lima", "bruno.lima@exemplo.com"),
        Cliente("0199a000-0000-7000-8000-000000000003", "Carla Mendes", "carla.mendes@exemplo.com")
    ];

    public static readonly object[] Produtos =
    [
        Produto("0199a000-0000-7000-8000-000000000101", "Teclado mecânico", "Teclado ABNT2 com switches marrons.", 349.90m, 15),
        Produto("0199a000-0000-7000-8000-000000000102", "Mouse sem fio", "Mouse óptico 1600 DPI com receptor USB.", 89.90m, 40),
        Produto("0199a000-0000-7000-8000-000000000103", "Monitor 24\"", "Monitor Full HD IPS de 24 polegadas.", 899.00m, 8),
        Produto("0199a000-0000-7000-8000-000000000104", "Headset USB", "Headset com microfone e cancelamento de ruído.", 199.50m, 20),
        Produto("0199a000-0000-7000-8000-000000000105", "Webcam HD", "Webcam 1080p com microfone embutido.", 159.00m, 3)
    ];

    private static object Cliente(string id, string nome, string email) => new
    {
        Id = Guid.Parse(id),
        Nome = Valor(Nome.Criar(nome)),
        Email = Valor(Email.Criar(email))
    };

    private static object Produto(string id, string nome, string descricao, decimal preco, int estoque) => new
    {
        Id = Guid.Parse(id),
        Nome = Valor(Nome.Criar(nome)),
        Descricao = Valor(Descricao.Criar(descricao)),
        Preco = Valor(Preco.Criar(preco)),
        Estoque = Valor(Estoque.Criar(estoque))
    };

    private static T Valor<T>(Result<T> resultado) where T : struct =>
        resultado.Success
            ? resultado.Data
            : throw new InvalidOperationException($"Seed inválido: {resultado.Erro!.Mensagem}");
}
