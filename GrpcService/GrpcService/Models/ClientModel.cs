namespace GrpcService.Models
{
    public class ClienteModel
    {
        public string NomeCompleto { get; set; } = string.Empty;
        public string DataNascimento { get; set; } = string.Empty;
        public string Sexo { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public int Situacao { get; set; }
        public double Salario { get; set; } // Novo campo
    }
}