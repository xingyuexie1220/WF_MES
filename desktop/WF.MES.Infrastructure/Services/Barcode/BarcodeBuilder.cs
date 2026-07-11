using WF.MES.Core.Constants;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.Models.Entities;

namespace WF.MES.Infrastructure.Services.Barcode;

/// <summary>规则段校验、拼接条码与 ResetKey，计算条码总长度。</summary>
public class BarcodeBuilder : IBarcodeBuilder
{
    private readonly ISerialNumberFormatter _formatter;

    public BarcodeBuilder(ISerialNumberFormatter formatter)
    {
        _formatter = formatter;
    }

    public void ValidateSegments(IReadOnlyList<BarcodeRuleSegment> segments)
    {
        if (segments.Count == 0)
        {
            throw new InvalidOperationException("条码规则至少需要一个组成段");
        }

        var serialCount = segments.Count(s => s.SegmentType == BarcodeSegmentTypes.Serial);
        if (serialCount != 1)
        {
            throw new InvalidOperationException("每条料号条码规则必须有且仅有一个流水号段");
        }

        var serialSegment = segments.First(s => s.SegmentType == BarcodeSegmentTypes.Serial);
        ValidateSerialConfig(serialSegment.ConfigJson);

        foreach (var segment in segments.Where(s => s.SegmentType == BarcodeSegmentTypes.Date))
        {
            ValidateDatePartConfig(segment.ConfigJson);
        }

        foreach (var segment in segments.Where(s => s.SegmentType == BarcodeSegmentTypes.Literal))
        {
            ValidateLiteralConfig(segment.ConfigJson, segment.SortOrder);
        }
    }

    private static void ValidateLiteralConfig(string configJson, int sortOrder)
    {
        var value = RuleSegmentConfigHelper.ParseLiteral(configJson);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"第{sortOrder}段固定符号内容不能为空");
        }

        if (value != value.Trim())
        {
            throw new InvalidOperationException($"第{sortOrder}段固定符号内容首尾不能有空格");
        }
    }

    private static void ValidateDatePartConfig(string configJson)
    {
        var format = RuleSegmentConfigHelper.ParseDateFormat(configJson);
        if (!DatePartFormats.IsValid(format))
        {
            throw new InvalidOperationException($"日期格式无效: {format}");
        }
    }

    private static void ValidateSerialConfig(string configJson)
    {
        var config = RuleSegmentConfigHelper.ParseSerial(configJson);
        if (!SerialRadixDefinitions.IsSupported(config.Radix))
        {
            throw new InvalidOperationException("流水码进制仅支持 10 / 16 / 32 / 34 / 36");
        }

        if (config.Digits <= 0 || config.Digits > 20)
        {
            throw new InvalidOperationException("流水号位数必须在 1 ~ 20 之间");
        }
    }

    public BarcodeBuildResult Build(IReadOnlyList<BarcodeRuleSegment> segments, BarcodeBuildContext context, long serialValue)
    {
        ValidateSegments(segments);
        var ordered = segments.OrderBy(s => s.SortOrder).ToList();
        var barcode = string.Concat(ordered.Select(s => ResolvePart(s, context, serialValue)));
        return new BarcodeBuildResult
        {
            Barcode = barcode
        };
    }

    public string BuildResetKey(IReadOnlyList<BarcodeRuleSegment> segments, BarcodeBuildContext context)
    {
        var ordered = segments.OrderBy(s => s.SortOrder).ToList();
        return ComposeResetKey(ordered, context);
    }

    public string PreviewSample(IReadOnlyList<BarcodeRuleSegment> segments, BarcodeBuildContext context)
    {
        return Build(segments, context, BarcodeSegmentTypes.SerialStartValue).Barcode;
    }

    public int CalculateBarcodeLength(IReadOnlyList<BarcodeRuleSegment> segments)
    {
        return PreviewSample(segments, new BarcodeBuildContext { PrintDate = DateTime.Today }).Length;
    }

    private string ComposeResetKey(IList<BarcodeRuleSegment> ordered, BarcodeBuildContext context)
    {
        var parts = new List<string>();
        foreach (var segment in ordered)
        {
            if (segment.SegmentType == BarcodeSegmentTypes.Serial || segment.IncludeInResetKey != 1)
            {
                continue;
            }

            parts.Add(ResolvePart(segment, context, 1));
        }

        return string.Join("|", parts);
    }

    private string ResolvePart(BarcodeRuleSegment segment, BarcodeBuildContext context, long serialValue)
    {
        return segment.SegmentType switch
        {
            BarcodeSegmentTypes.Literal => RuleSegmentConfigHelper.ParseLiteral(segment.ConfigJson),
            BarcodeSegmentTypes.Date => DatePartFormatter.Format(context.PrintDate, RuleSegmentConfigHelper.ParseDateFormat(segment.ConfigJson)),
            BarcodeSegmentTypes.Serial => FormatSerial(segment.ConfigJson, serialValue),
            _ => throw new InvalidOperationException($"未知段类型: {segment.SegmentType}")
        };
    }

    private string FormatSerial(string configJson, long serialValue)
    {
        var config = RuleSegmentConfigHelper.ParseSerial(configJson);
        return _formatter.Format(serialValue, config.Radix, config.Digits);
    }
}
