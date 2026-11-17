using Grpc.Core;
using Grpc.Core.Interceptors;

namespace GrpcService.Interceptors
{
    public class ExceptionInterceptor : Interceptor
    {
        private readonly ILogger<ExceptionInterceptor> _logger;

        public ExceptionInterceptor(ILogger<ExceptionInterceptor> logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                // Continua a execução normal do método gRPC
                return await continuation(request, context);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erro de validação (InvalidArgument) capturado pelo interceptor.");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Recurso não encontrado (NotFound) capturado pelo interceptor.");
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                // Loga o erro crítico completo internamente no servidor, mas oculta os detalhes do sistema para o cliente final
                _logger.LogError(ex, "Erro interno crítico não tratado capturado pelo interceptor.");
                throw new RpcException(new Status(StatusCode.Internal, "Ocorreu um erro interno no servidor ao processar a requisição."));
            }
        }
    }
}