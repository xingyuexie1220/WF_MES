using FluentValidation;
using WF.MES.Core.Constants;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Validation.Barcode;

/// <summary><see cref="BarcodeGenerateRequestDto"/> 校验。</summary>
public sealed class BarcodeGenerateRequestValidator : AbstractValidator<BarcodeGenerateRequestDto>
{
    public BarcodeGenerateRequestValidator()
    {
        RuleFor(x => x.RuleId)
            .GreaterThan(0)
            .WithMessage("请选择料号条码规则");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("打印数量必须大于 0")
            .LessThanOrEqualTo(BarcodeGenerateLimits.MaxQuantityPerBatch)
            .WithMessage($"条码生成数量不能超过 {BarcodeGenerateLimits.MaxQuantityPerBatch}");
    }
}
