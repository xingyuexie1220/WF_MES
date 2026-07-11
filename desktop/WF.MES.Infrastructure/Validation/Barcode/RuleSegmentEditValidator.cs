using FluentValidation;
using WF.MES.Core.Constants;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Validation.Barcode;

/// <summary><see cref="RuleSegmentEditDto"/> 校验。</summary>
public sealed class RuleSegmentEditValidator : AbstractValidator<RuleSegmentEditDto>
{
    public RuleSegmentEditValidator()
    {
        RuleFor(x => x.SegmentType)
            .Must(type => BarcodeSegmentTypes.TypeOptions.Any(o => o.Value == type))
            .WithMessage("段类型无效");

        When(x => x.SegmentType == BarcodeSegmentTypes.Literal, () =>
        {
            RuleFor(x => x.LiteralValue)
                .NotEmpty().WithMessage("固定符号内容不能为空");

            RuleFor(x => x.LiteralValue)
                .Must(v => v == v.Trim())
                .WithMessage("固定符号内容首尾不能有空格");
        });

        When(x => x.SegmentType == BarcodeSegmentTypes.Date, () =>
        {
            RuleFor(x => x.DateFormat)
                .Must(DatePartFormats.IsValid)
                .WithMessage(x => $"日期格式无效: {x.DateFormat}");
        });

        When(x => x.SegmentType == BarcodeSegmentTypes.Serial, () =>
        {
            RuleFor(x => x.SerialRadix)
                .Must(SerialRadixDefinitions.IsSupported)
                .WithMessage("流水码进制仅支持 10 / 16 / 32 / 34 / 36");

            RuleFor(x => x.SerialDigits)
                .InclusiveBetween(1, 20)
                .WithMessage("流水号位数必须在 1 ~ 20 之间");
        });
    }
}
