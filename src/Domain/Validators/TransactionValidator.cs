using System;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;

namespace Domain.Validators;

public class TransactionValidator : AbstractValidator<Transaction>
{
    public TransactionValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid transaction type");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Transaction date is required")
            .LessThanOrEqualTo(DateTimeOffset.UtcNow.AddDays(1)).WithMessage("Transaction date cannot be in the future");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Transaction amount must be greater than or equal to zero");

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("CPF is required")
            .Length(11).WithMessage("CPF must be 11 digits")
            .Matches(@"^\d{11}$").WithMessage("CPF must contain only numbers");

        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Card number is required")
            .Length(12).WithMessage("Card number must be 12 digits")
            .Matches(@"^\d{12}$").WithMessage("Card number must contain only numbers");

        RuleFor(x => x.Store)
            .NotNull().WithMessage("Store is required");
    }
}
