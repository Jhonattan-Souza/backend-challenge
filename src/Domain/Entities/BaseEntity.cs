using System;
using System.Linq;
using FluentValidation;
using FluentResults;

namespace Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTimeOffset CreatedAt { get; protected set; }
    public DateTimeOffset UpdatedAt { get; protected set; } = DateTimeOffset.UtcNow;
    
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    protected static Result<T> Validate<T>(T entity, AbstractValidator<T> validator)
    {
        var validationResult = validator.Validate(entity);

        if (validationResult.IsValid)
            return Result.Ok(entity);

        var errors = validationResult.Errors.Select(e => new Error(e.ErrorMessage));
        return Result.Fail<T>(errors);
    }
}