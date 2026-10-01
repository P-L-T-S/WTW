using System.Diagnostics.CodeAnalysis;

namespace Wtw.Domain.Results;

public class Result
{
    protected Result(bool success, Erro? erro = null)
    {
        Success = success;
        Erro = erro;
    }
    [MemberNotNullWhen(false, nameof(Erro))]
    public bool Success { get; }
    public Erro? Erro { get; }
    
    [MemberNotNullWhen(true, nameof(Erro))]
    public bool IsFailure => !Success;

    public static Result Ok() => new Result(true);
    public static Result Fail(Erro erro) => new Result(false, erro);

    public static Result Fail(string metodo, string mensagem, string? tipoExcecao, string? detalhes) =>
        new Result(false, new Erro(metodo, mensagem, tipoExcecao, detalhes));
}

public class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool success, Erro? erro = null, T? data = default) : base(success, erro)
    {
        Data = data;
    }

    public static Result<T> Ok(T value) => new Result<T>(true, data: value);

    public new static Result<T> Fail(Erro erro) => new Result<T>(false, erro);

    public new static Result<T> Fail(string metodo, string mensagem, string? tipoExcecao, string? detalhes) =>
        new Result<T>(false, new Erro(metodo, mensagem, tipoExcecao, detalhes));
}