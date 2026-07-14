using WF.MES.Models.Dtos;
using WF.MES.Models.Entities;

namespace WF.MES.Core.Interfaces;

/// <summary>按规则段拼接条码、ResetKey 与长度计算（无 DB 依赖）。</summary>
public interface IBarcodeBuilder
{
    void ValidateSegments(IReadOnlyList<BarcodeRuleSegment> segments);

    BarcodeBuildResult Build(IReadOnlyList<BarcodeRuleSegment> segments, BarcodeBuildContext context, long serialValue);

    string BuildResetKey(IReadOnlyList<BarcodeRuleSegment> segments, BarcodeBuildContext context);

    string PreviewSample(IReadOnlyList<BarcodeRuleSegment> segments, BarcodeBuildContext context);

    int CalculateBarcodeLength(IReadOnlyList<BarcodeRuleSegment> segments);
}
