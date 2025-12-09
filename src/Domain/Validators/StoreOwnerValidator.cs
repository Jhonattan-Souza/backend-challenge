using Domain.Entities;
using FluentValidation;

namespace Domain.Validators;

public class StoreOwnerValidator : AbstractValidator<StoreOwner>
{
    public StoreOwnerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Store owner name is required")
            .MaximumLength(14).WithMessage("Store owner name must be at most 14 characters");

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("CPF is required")
            .Length(11).WithMessage("CPF must be 11 digits")
            .Matches(@"^\d{11}$").WithMessage("CPF must contain only numbers");
    }
}
