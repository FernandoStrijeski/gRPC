using FluentValidation;
using Grpc.Core;
using GrpcService.Models;
using GrpcService.Repositories;

namespace GrpcService.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        private readonly IClienteRepository _clienteRepository;
        private readonly TokenService _tokenService;
        private readonly IValidator<CadastroRequest> _cadastroValidator;
        private readonly IValidator<ReajusteRequest> _reajusteValidator;

        public GreeterService(
             ILogger<GreeterService> logger,
             IClienteRepository clienteRepository,
             IValidator<CadastroRequest> cadastroValidator,
             IValidator<ReajusteRequest> reajusteValidator)
        {
            _logger = logger;
            _clienteRepository = clienteRepository;
            _cadastroValidator = cadastroValidator;
            _reajusteValidator = reajusteValidator;
        }

        public override Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Acionado o método Login");
            _logger.LogInformation("Tentativa de autenticação para o usuário: {Usuario}", request.Usuario);

            // Validação simulada
            if (request.Usuario == "admin" && request.Senha == "123456")
            {
                var token = _tokenService.GerarToken(request.Usuario);
                _logger.LogInformation("Autenticação bem-sucedida para o usuário: {Usuario}", request.Usuario);

                return Task.FromResult(new LoginReply
                {
                    Sucesso = true,
                    Token = token,
                    Mensagem = "Login realizado com sucesso."
                });
            }

            _logger.LogWarning("Falha na autenticação para o usuário: {Usuario}", request.Usuario);
            return Task.FromResult(new LoginReply
            {
                Sucesso = false,
                Token = string.Empty,
                Mensagem = "Usuário ou senha inválidos."
            });
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Acionado o método SayHello");
            _logger.LogInformation("Processando saudação para o usuário: {Name}", request.Name);

            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public override async Task<ReajusteReply> ReajustarSalario(ReajusteRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Acionado o método ReajustarSalario");
            _logger.LogInformation("Iniciando reajuste salarial para o cliente ID: {Id} com percentual de {Percentual}%", request.Identificador, request.PercentualReajuste);

            var validationResult = await _reajusteValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var erros = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new RpcException(new Status(StatusCode.InvalidArgument, erros));
            }

            var cliente = _clienteRepository.ObterPorId(request.Identificador);
            if (cliente == null)
            {
                throw new KeyNotFoundException($"Cliente com o identificador {request.Identificador} não foi encontrado.");
            }

            double novoSalario = cliente.Salario + (cliente.Salario * (request.PercentualReajuste / 100.0));
            cliente.Salario = novoSalario;

            _logger.LogInformation("Salário atualizado com sucesso para o ID {Id}. Novo valor: R$ {NovoSalario}", request.Identificador, novoSalario);

            return new ReajusteReply
            {
                SalarioAtualizado = novoSalario,
                Mensagem = "Salário reajustado com sucesso."
            };
        }

        public override async Task<CadastroReply> CadastrarCliente(CadastroRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Acionado o método CadastrarCliente");
            _logger.LogInformation("Processando os dados de cadastro para o cliente: {Nome}", request.NomeCompleto);

            // Valida os dados usando FluentValidation
            var validationResult = await _cadastroValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var erros = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Falha de validação no cadastro: {Erros}", erros);
                throw new RpcException(new Status(StatusCode.InvalidArgument, erros));
            }

            var clienteModel = new ClienteModel
            {
                NomeCompleto = request.NomeCompleto,
                DataNascimento = request.DataNascimento,
                Sexo = request.Sexo,
                Endereco = request.Endereco,
                Situacao = request.Situacao,
                Salario = request.Salario
            };

            int identificador = _clienteRepository.Adicionar(clienteModel);

            _logger.LogInformation("Cliente cadastrado e persistido com sucesso na posição/ID: {Id}", identificador);

            return new CadastroReply
            {
                Mensagem = $"Cliente cadastrado com sucesso para o identificador {identificador}.",
                Identificador = identificador
            };
        }

        public override Task<BuscaClienteReply> BuscarCliente(BuscaClienteRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Acionado o método BuscarCliente");
            _logger.LogInformation("Consultando base de dados em memória para o ID: {Id}", request.Identificador);

            var cliente = _clienteRepository.ObterPorId(request.Identificador);

            if (cliente == null)
            {
                // O interceptor vai capturar isso e retornar StatusCode.NotFound automaticamente para o cliente!
                throw new KeyNotFoundException($"Cliente com o identificador {request.Identificador} não foi encontrado.");
            }

            _logger.LogInformation("Cliente ID {Id} encontrado com sucesso.", request.Identificador);

            return Task.FromResult(new BuscaClienteReply
            {
                Encontrado = true,
                Mensagem = "Cliente encontrado com sucesso.",
                NomeCompleto = cliente.NomeCompleto,
                DataNascimento = cliente.DataNascimento,
                Sexo = cliente.Sexo,
                Endereco = cliente.Endereco,
                Situacao = cliente.Situacao,
                Salario = cliente.Salario
            });
        }
    }
}