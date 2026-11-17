using GrpcService.Models;

namespace GrpcService.Repositories
{
    public interface IClienteRepository
    {
        int Adicionar(ClienteModel cliente);
        ClienteModel? ObterPorId(int id); 
    }
}