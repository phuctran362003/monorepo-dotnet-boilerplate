using FluentValidation;
using {{Namespace}}.ViewModels;

namespace {{Namespace}}.Services.Validators;

public sealed class CreateProductViewModelValidator : AbstractValidator<CreateProductViewModel>
{
    public CreateProductViewModelValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Price)
            .GreaterThanOrEqualTo(0);
    }
}
