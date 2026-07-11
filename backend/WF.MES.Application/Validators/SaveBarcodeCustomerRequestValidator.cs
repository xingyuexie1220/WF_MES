using FluentValidation;
using WF.MES.Application.Barcode;

namespace WF.MES.Application.Validators;

public class SaveBarcodeCustomerRequestValidator : AbstractValidator<SaveBarcodeCustomerRequest>
{
    public SaveBarcodeCustomerRequestValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Enable).InclusiveBetween(0, 1);
    }
}
