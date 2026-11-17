using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.IdentityModel.Tokens;

namespace GrpcService.Interceptors
{
    public class JwtAuthInterceptor : Interceptor
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtAuthInterceptor> _logger;

        public JwtAuthInterceptor(IConfiguration configuration, ILogger<JwtAuthInterceptor> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            // O método de login não exige token JWT
            if (context.Method.Contains("Login"))
            {
                return await continuation(request, context);
            }

            var httpContext = context.GetHttpContext();
            var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("Tentativa de acesso sem token JWT no método: {Method}", context.Method);
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Token de autorização ausente ou inválido."));
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                var secret = _configuration["JwtSettings:Secret"];
                var key = Encoding.ASCII.GetBytes(secret);

                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JwtSettings:Audience"],
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return await continuation(request, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha na validação do token JWT.");
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Token inválido ou expirado."));
            }
        }
    }
}