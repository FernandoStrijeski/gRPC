using GrpcService.Extensions;
using GrpcService.Interceptors;
using GrpcService.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurando o Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Política global ou nomeada para limitar requisições por cliente
    options.AddFixedWindowLimiter("FixedPolicy", opt =>
    {
        opt.PermitLimit = 10;                // Máximo de 10 requisições
        opt.Window = TimeSpan.FromSeconds(10); // Dentro de uma janela de 10 segundos
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;                  // Máximo de 2 requisições na fila de espera
    });

    // Comportamento customizado ao rejeitar por excesso de requisições
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
        {
            await context.HttpContext.Response.WriteAsync(
                $"Muitas requisições. Tente novamente em {retryAfter.TotalSeconds} segundos.", cancellationToken);
        }
        else
        {
            await context.HttpContext.Response.WriteAsync(
                "Muitas requisições. Limite excedido. Tente novamente mais tarde.", cancellationToken);
        }
    };
});

// Adicionando gRPC ao container
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<JwtAuthInterceptor>();
    options.Interceptors.Add<ExceptionInterceptor>();
});

// Adicionando os serviços da aplicação de forma organizada
builder.Services.AddApplicationServices();

// Opcional para Empresas: Adicionando Health Checks (essencial para Kubernetes/Docker)
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configuração do Pipeline HTTP e Middlewares de Segurança
app.MapGrpcService<GreeterService>();

// Endpoint raiz informativo
app.MapGet("/", () => "A comunicação com endpoints gRPC deve ser realizada por meio de um cliente gRPC.");

// Opcional corporativo: Mapear Health Checks para monitoramento de infraestrutura
app.MapHealthChecks("/health");

app.Run();