using CaseItau.Application.DTOs;
using FluentValidation;

namespace CaseItau.Application.Validators;

/// <summary>
/// Validates the data provided for creating a new fund.
/// </summary>
public class CreateFundoDtoValidator : AbstractValidator<CreateFundoDto>
{
    /// <summary>
    /// Initializes a new instance of <see cref="CreateFundoDtoValidator"/> with all validation rules.
    /// </summary>
    public CreateFundoDtoValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("Codigo is required.")
            .MaximumLength(20).WithMessage("Codigo must not exceed 20 characters.");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome is required.")
            .MaximumLength(100).WithMessage("Nome must not exceed 100 characters.");

        RuleFor(x => x.Cnpj)
            .NotEmpty().WithMessage("Cnpj is required.")
            .Length(14).WithMessage("Cnpj must be exactly 14 characters.")
            .Matches(@"^\d{14}$").WithMessage("Cnpj must contain only digits.");

        RuleFor(x => x.CodigoTipo)
            .GreaterThan(0).WithMessage("CodigoTipo must be greater than zero.");

        RuleFor(x => x.Patrimonio)
            .GreaterThanOrEqualTo(0).WithMessage("Patrimonio must be greater than or equal to zero.")
            .When(x => x.Patrimonio.HasValue);
    }
}
