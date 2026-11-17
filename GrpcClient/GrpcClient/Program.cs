using Grpc.Core;
using Grpc.Net.Client;
using GrpcService;

namespace GrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Cliente gRPC com Autenticação JWT ===");

            using var channel = GrpcChannel.ForAddress("https://localhost:7141");
            var client = new Greeter.GreeterClient(channel);

            string jwtToken = "";

            // Fluxo de Login
            while (string.IsNullOrEmpty(jwtToken))
            {
                Console.Write("Usuário (ex: admin): ");
                string user = Console.ReadLine() ?? "";

                Console.Write("Senha (ex: 123456): ");
                string pass = Console.ReadLine() ?? "";

                try
                {
                    var loginResponse = await client.LoginAsync(new LoginRequest { Usuario = user, Senha = pass });
                    if (loginResponse.Sucesso)
                    {
                        jwtToken = loginResponse.Token;
                        Console.WriteLine($"\n[Sucesso]: {loginResponse.Mensagem}\n");
                    }
                    else
                    {
                        Console.WriteLine($"\n[Erro]: {loginResponse.Mensagem}. Tente novamente.\n");
                    }
                }
                catch (RpcException ex)
                {
                    Console.WriteLine($"\n[Erro gRPC]: {ex.Status.Detail}\n");
                }
            }

            // Criando o Metadata contendo o token JWT Bearer para as próximas requisições
            var headers = new Metadata
            {
                { "Authorization", $"Bearer {jwtToken}" }
            };

            bool sair = false;
            while (!sair)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine(" Status: Autenticado via JWT [Admin]");
                Console.WriteLine("========================================");
                Console.WriteLine("Selecione uma opção do menu:");
                Console.WriteLine("  1. Cadastrar cliente");
                Console.WriteLine("  2. Consultar cliente por ID");
                Console.WriteLine("  3. Atualizar salário");
                Console.WriteLine("  0. Sair");
                Console.Write("\nEscolha uma opção: ");

                string opcao = Console.ReadLine() ?? "";

                switch (opcao)
                {
                    case "1":
                        await ExecutarCadastroAsync(client, headers);
                        break;
                    case "2":
                        await ExecutarBuscaAsync(client, headers);
                        break;
                    case "3":
                        await ExecutarReajusteAsync(client, headers);
                        break;
                    case "0":
                        sair = true;
                        Console.WriteLine("\nEncerrando aplicação...");
                        break;
                    default:
                        Console.WriteLine("\n[Erro]: Opção inválida.");
                        break;
                }

                if (!sair)
                {
                    Console.WriteLine("\nPressione qualquer tecla para continuar...");
                    Console.ReadKey();
                }
            }
        }

        private static async Task ExecutarCadastroAsync(Greeter.GreeterClient client, Metadata headers)
        {
            Console.WriteLine("\n--- Novo Cadastro de Cliente ---");

            string nome = "";
            while (string.IsNullOrWhiteSpace(nome))
            {
                Console.Write("Nome Completo: ");
                nome = Console.ReadLine() ?? "";
                if (string.IsNullOrWhiteSpace(nome)) Console.WriteLine("[Erro] O nome não pode ser vazio.");
            }

            Console.Write("Data de Nascimento (ex: 10/05/1992): ");
            string dataNasc = Console.ReadLine() ?? "";

            string sexo = "";
            while (true)
            {
                Console.Write("Sexo ([M]asculino / [F]eminino): ");
                string entrada = Console.ReadLine()?.Trim().ToUpper() ?? "";
                if (entrada == "M" || entrada == "F") { sexo = entrada; break; }
                Console.WriteLine("[Erro] Digite apenas M ou F.");
            }

            Console.Write("Endereço completo: ");
            string endereco = Console.ReadLine() ?? "";

            int situacao = 1;
            while (true)
            {
                Console.Write("Situação (1 - Ativo, 2 - Inativo): ");
                if (int.TryParse(Console.ReadLine(), out situacao) && (situacao == 1 || situacao == 2)) break;
                Console.WriteLine("[Erro] Digite 1 para Ativo ou 2 para Inativo.");
            }

            double salario = 0;
            while (true)
            {
                Console.Write("Salário (ex: 5000.00): ");
                if (double.TryParse(Console.ReadLine(), out salario) && salario > 0) break;
                Console.WriteLine("[Erro] Insira um valor de salário válido e maior que zero.");
            }

            try
            {
                // Passamos os headers com o token JWT como argumento
                var resposta = await client.CadastrarClienteAsync(new CadastroRequest
                {
                    NomeCompleto = nome,
                    DataNascimento = dataNasc,
                    Sexo = sexo,
                    Endereco = endereco,
                    Situacao = situacao == 0 ? 1 : situacao,
                    Salario = salario
                }, headers);

                Console.WriteLine($"\n[Resposta do Servidor]: {resposta.Mensagem}");
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"\n[Erro de Autorização/Servidor]: {ex.Status.Detail}");
            }
        }

        private static async Task ExecutarBuscaAsync(Greeter.GreeterClient client, Metadata headers)
        {
            Console.WriteLine("\n--- Consultar Cliente por ID ---");
            Console.Write("Digite o ID (identificador): ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                try
                {
                    var resposta = await client.BuscarClienteAsync(new BuscaClienteRequest { Identificador = id }, headers);

                    Console.WriteLine($"\n[Encontrado]: {resposta.Mensagem}");
                    Console.WriteLine($"  - Nome: {resposta.NomeCompleto}");
                    Console.WriteLine($"  - Salário: R$ {resposta.Salario:F2}");
                }
                catch (RpcException ex)
                {
                    Console.WriteLine($"\n[Erro do Servidor]: {ex.Status.Detail}");
                }
            }
        }

        private static async Task ExecutarReajusteAsync(Greeter.GreeterClient client, Metadata headers)
        {
            Console.WriteLine("\n--- Atualizar Salário ---");
            Console.Write("Digite o ID do cliente: ");

            // Apenas conversão básica. Se o usuário digitar algo errado ou um ID que não existe, 
            // deixamos o Servidor validar e retornar a exceção tratada!
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                Console.Write("Digite o percentual de reajuste (ex: 7.5): ");
                if (double.TryParse(Console.ReadLine(), out double percentual))
                {
                    try
                    {
                        var resposta = await client.ReajustarSalarioAsync(new ReajusteRequest
                        {
                            Identificador = id,
                            PercentualReajuste = percentual
                        }, headers);

                        Console.WriteLine($"\n[Sucesso]: {resposta.Mensagem}");
                        Console.WriteLine($"Novo Salário: R$ {resposta.SalarioAtualizado:F2}");
                    }
                    catch (RpcException ex)
                    {
                        Console.WriteLine($"\n[Erro do Servidor]: {ex.Status.Detail}");
                    }
                }
                else
                {
                    Console.WriteLine("\n[Erro]: Percentual inválido.");
                }
            }
            else
            {
                Console.WriteLine("\n[Erro]: ID inválido.");
            }
        }
    }
}