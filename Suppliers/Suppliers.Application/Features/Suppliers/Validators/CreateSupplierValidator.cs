using FluentValidation;

public class CreateSupplierValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierValidator()
    {
        RuleFor(x => x.SupplierData.name).NotEmpty().WithMessage("Supplier name is required.");
        RuleFor(x => x.SupplierData.phone).Length(10).WithMessage("Phone number must be 10 digits.");

    }
}