using FluentValidation;
using GrpcService;

namespace GrpcService.Validators
{
    public class ReajusteRequestValidator : AbstractValidator<ReajusteRequest>
    {
        public ReajusteRequestValidator()
        {
            RuleFor(x => x.Identificador)
                .GreaterThan(0).WithMessage("O identificador do cliente deve ser válido.");

            RuleFor(x => x.PercentualReajuste)
                .InclusiveBetween(0, 100).WithMessage("O percentual de reajuste deve estar entre 0% e 100%.");
        }
    }
}