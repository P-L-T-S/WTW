using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using Serilog;
using Wtw.Api.Endpoints;
using Wtw.Api.Extensions;
using Wtw.Api.Middlewares;
using Wtw.Application;
using Wtw.Infrastructure;

const string politicaCorsFrontend = "Frontend";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((servicos, configuracao) => configuracao
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(servicos));

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration);

    builder.Services.ConfigureHttpJsonOptions(jsonOptions =>
        jsonOptions.SerializerOptions.Converters.Add(new JsonStringEnumConverter())
    );

    builder.Services.Configure<RouteHandlerOptions>(opcoes => opcoes.ThrowOnBadRequest = false);

    builder.Services.AddProblemDetailsEmPortugues();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddOpenApi();

    builder.Services.AddCors(opcoes => opcoes.AddPolicy(politicaCorsFrontend, politica => politica
            .WithOrigins(builder.Configuration.GetSection("Cors:Origens").Get<string[]>() ?? [])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("Location")
        )
    );

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    if (app.Configuration.GetValue<bool>("Database:AplicarMigrationsNoStartup"))
    {
        await app.Services.AplicarMigrationsAsync();
    }

    app.UseCors(politicaCorsFrontend);

    app.MapProdutosEndpoints()
        .MapClientesEndpoints()
        .MapPedidosEndpoints();

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "A API foi encerrada por uma falha no startup");
}
finally
{
    await Log.CloseAndFlushAsync();
}