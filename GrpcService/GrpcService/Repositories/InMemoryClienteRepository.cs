using GrpcService.Models;

namespace GrpcService.Repositories
{
    public class InMemoryClienteRepository : IClienteRepository
    {
        private readonly List<ClienteModel> _clientes = new();
        private readonly object _lock = new();

        public int Adicionar(ClienteModel cliente)
        {
            lock (_lock) //bloqueia transação para evitar concorrencia
            {
                _clientes.Add(cliente);
                return _clientes.Count; // Retorna a posição (ID)
            }
        }

        public ClienteModel? ObterPorId(int id)
        {
            lock (_lock)
            {
                int index = id - 1;
                if (index < 0 || index >= _clientes.Count)
                    return null;

                return _clientes[index];
            }
        }
    }
}