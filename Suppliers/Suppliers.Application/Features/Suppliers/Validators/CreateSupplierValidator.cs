using FluentValidation;

public class CreateSupplierValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierValidator()
    {
        RuleFor(x => x.SupplierData.name).NotEmpty().WithMessage("Supplier name is required.");
        RuleFor(x => x.SupplierData.phone).Length(10).WithMessage("Phone number must be 10 digits.");
        RuleFor(x => x.SupplierData.gstIn).Matches(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$")
            .When(x => !string.IsNullOrEmpty(x.SupplierData.gstIn))
            .WithMessage("Invalid GST format.");
    }
}