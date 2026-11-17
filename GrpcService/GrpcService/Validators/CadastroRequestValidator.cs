using FluentValidation;
using GrpcService;

namespace GrpcService.Validators
{
    public class CadastroRequestValidator : AbstractValidator<CadastroRequest>
    {
        public CadastroRequestValidator()
        {
            RuleFor(x => x.NomeCompleto)
                .NotEmpty().WithMessage("O nome completo é obrigatório.")
                .MinimumLength(3).WithMessage("O nome deve ter pelo menos 3 caracteres.");

            RuleFor(x => x.DataNascimento)
                .NotEmpty().WithMessage("A data de nascimento é obrigatória.");

            RuleFor(x => x.Sexo)
                .Must(sexo => sexo == "M" || sexo == "F")
                .WithMessage("O sexo deve ser estritamente 'M' ou 'F'.");

            RuleFor(x => x.Salario)
                .GreaterThan(0).WithMessage("O salário deve ser maior do que zero.");

            RuleFor(x => x.Situacao)
                .InclusiveBetween(1, 2).WithMessage("A situação deve ser 1 (Ativo) ou 2 (Inativo).");
        }
    }
}