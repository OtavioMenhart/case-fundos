using CaseItau.Application.Constants;
using CaseItau.Application.DTOs;
using CaseItau.Domain.Constants;
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
            .NotEmpty().WithMessage(ValidationMessages.Codigo.Required)
            .MaximumLength(FundoConstants.CodigoMaxLength).WithMessage(ValidationMessages.Codigo.MaxLength);

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage(ValidationMessages.Nome.Required)
            .MaximumLength(FundoConstants.NomeMaxLength).WithMessage(ValidationMessages.Nome.MaxLength);

        RuleFor(x => x.Cnpj)
            .NotEmpty().WithMessage(ValidationMessages.Cnpj.Required)
            .Length(FundoConstants.CnpjLength).WithMessage(ValidationMessages.Cnpj.Length)
            .Matches(FundoConstants.CnpjPattern).WithMessage(ValidationMessages.Cnpj.OnlyDigits);

        RuleFor(x => x.CodigoTipo)
            .GreaterThan(0).WithMessage(ValidationMessages.CodigoTipo.GreaterThanZero);

        RuleFor(x => x.Patrimonio)
            .GreaterThanOrEqualTo(0).WithMessage(ValidationMessages.Patrimonio.GreaterThanOrEqualToZero)
            .When(x => x.Patrimonio.HasValue);
    }
}
