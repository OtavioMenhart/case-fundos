using CaseItau.Application.Constants;
using CaseItau.Application.DTOs;
using CaseItau.Domain.Constants;
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
            .NotEmpty().WithMessage(ValidationMessages.Nome.Required)
            .MaximumLength(FundoConstants.NomeMaxLength).WithMessage(ValidationMessages.Nome.MaxLength);

        RuleFor(x => x.Cnpj)
            .NotEmpty().WithMessage(ValidationMessages.Cnpj.Required)
            .Length(FundoConstants.CnpjLength).WithMessage(ValidationMessages.Cnpj.Length)
            .Matches(FundoConstants.CnpjPattern).WithMessage(ValidationMessages.Cnpj.OnlyDigits);

        RuleFor(x => x.CodigoTipo)
            .GreaterThan(0).WithMessage(ValidationMessages.CodigoTipo.GreaterThanZero);
    }
}
