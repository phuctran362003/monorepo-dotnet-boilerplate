using FluentValidation;
using {{ApplicationNamespace}}.Products.Dtos;

namespace {{ApplicationNamespace}}.Products.Validators;

public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Price)
            .GreaterThanOrEqualTo(0);
    }
}
