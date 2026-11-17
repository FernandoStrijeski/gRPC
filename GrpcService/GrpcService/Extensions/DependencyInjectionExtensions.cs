using FluentValidation;
using GrpcService.Repositories;
using GrpcService.Services;
using GrpcService.Validators;

namespace GrpcService.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Registro de Repositórios e Serviços de Domínio
            services.AddSingleton<IClienteRepository, InMemoryClienteRepository>();

            // Registra os validadores automaticamente
            services.AddValidatorsFromAssemblyContaining<CadastroRequestValidator>();

            services.AddSingleton<TokenService>();

            return services;
        }
    }
}