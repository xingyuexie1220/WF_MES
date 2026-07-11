using FluentValidation;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.Models.Entities;

namespace WF.MES.Infrastructure.Validation.Barcode;

/// <summary><see cref="MaterialRuleEditDto"/> 校验（含规则段与条码长度）。</summary>
public sealed class MaterialRuleEditValidator : AbstractValidator<MaterialRuleEditDto>
{
    public MaterialRuleEditValidator(IBarcodeBuilder barcodeBuilder)
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage("请选择客户");

        RuleFor(x => x.MaterialNo)
            .NotEmpty().WithMessage("料号不能为空")
            .MaximumLength(50).WithMessage("料号不能超过 50 个字符");

        RuleFor(x => x.BarcodeLength)
            .GreaterThan(0)
            .WithMessage("请填写条码长度");

        RuleFor(x => x.Segments)
            .NotEmpty()
            .WithMessage("请至少添加一个组成段");

        RuleForEach(x => x.Segments)
            .SetValidator(new RuleSegmentEditValidator());

        RuleFor(x => x)
            .Custom((dto, context) =>
            {
                if (dto.Segments.Count == 0)
                {
                    return;
                }

                var entities = dto.Segments
                    .Select(s => new BarcodeRuleSegment
                    {
                        SegmentType = s.SegmentType,
                        SortOrder = s.SortOrder,
                        ConfigJson = RuleSegmentConfigHelper.ToConfigJson(s),
                        IncludeInResetKey = s.IncludeInResetKey ? 1 : 0
                    })
                    .ToList();

                try
                {
                    barcodeBuilder.ValidateSegments(entities);
                }
                catch (InvalidOperationException ex)
                {
                    context.AddFailure(ex.Message);
                    return;
                }

                var calculatedLength = barcodeBuilder.CalculateBarcodeLength(entities);
                if (dto.BarcodeLength != calculatedLength)
                {
                    context.AddFailure(
                        $"条码长度({dto.BarcodeLength})与生成总长度({calculatedLength})不一致，请检查组成段或条码长度");
                }
            });
    }
}
