using CaseItau.Application.DTOs;
using FluentValidation;

namespace CaseItau.Application.Validators;

/// <summary>
/// Validates the data provided for updating an existing fund.
/// </summary>
public class UpdateFundoDtoValidator : AbstractValidator<UpdateFundoDto>
{
    /// <summary>
    /// Initializes a new instance of <see cref="UpdateFundoDtoValidator"/> with all validation rules.
    /// </summary>
    public UpdateFundoDtoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome is required.")
            .MaximumLength(100).WithMessage("Nome must not exceed 100 characters.");

        RuleFor(x => x.Cnpj)
            .NotEmpty().WithMessage("Cnpj is required.")
            .Length(14).WithMessage("Cnpj must be exactly 14 characters.")
            .Matches(@"^\d{14}$").WithMessage("Cnpj must contain only digits.");

        RuleFor(x => x.CodigoTipo)
            .GreaterThan(0).WithMessage("CodigoTipo must be greater than zero.");
    }
}
