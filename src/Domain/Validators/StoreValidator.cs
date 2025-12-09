using Domain.Entities;
using FluentValidation;

namespace Domain.Validators;

public class StoreValidator : AbstractValidator<Store>
{
    public StoreValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Store name is required")
            .MaximumLength(19).WithMessage("Store name must be at most 19 characters");

        RuleFor(x => x.Owner)
            .NotNull().WithMessage("Store owner is required");
    }
}
