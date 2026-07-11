using System.Text.Json;
using WF.MES.Core.Constants;

namespace WF.MES.Models.Dtos;

/// <summary>规则段 Config_Json 与编辑 DTO 互转（Literal/Date/Serial）。</summary>
public static class RuleSegmentConfigHelper
{
    public sealed class SerialConfig
    {
        public int Radix { get; init; } = 10;

        public int Digits { get; init; } = 4;
    }

    public static string ToConfigJson(RuleSegmentEditDto dto) => dto.SegmentType switch
    {
        BarcodeSegmentTypes.Literal => JsonSerializer.Serialize(new { value = dto.LiteralValue }),
        BarcodeSegmentTypes.Date => JsonSerializer.Serialize(new { format = dto.DateFormat }),
        BarcodeSegmentTypes.Serial => JsonSerializer.Serialize(new
        {
            radix = dto.SerialRadix,
            digits = dto.SerialDigits
        }),
        _ => "{}"
    };

    public static void ApplyFromConfigJson(RuleSegmentEditDto dto, string configJson)
    {
        using var doc = JsonDocument.Parse(NormalizeJson(configJson));
        var root = doc.RootElement;

        switch (dto.SegmentType)
        {
            case BarcodeSegmentTypes.Literal:
                dto.LiteralValue = root.TryGetProperty("value", out var value)
                    ? value.GetString() ?? string.Empty
                    : string.Empty;
                break;
            case BarcodeSegmentTypes.Date:
                dto.DateFormat = root.TryGetProperty("format", out var format)
                    ? format.GetString() ?? DatePartFormats.Default
                    : DatePartFormats.Default;
                break;
            case BarcodeSegmentTypes.Serial:
                dto.SerialRadix = root.TryGetProperty("radix", out var radix) ? radix.GetInt32() : 10;
                dto.SerialDigits = root.TryGetProperty("digits", out var digits) ? digits.GetInt32() : 4;
                break;
        }
    }

    public static string ParseLiteral(string configJson)
    {
        using var doc = JsonDocument.Parse(NormalizeJson(configJson));
        return doc.RootElement.TryGetProperty("value", out var value) ? value.GetString() ?? string.Empty : string.Empty;
    }

    public static string ParseDateFormat(string configJson)
    {
        using var doc = JsonDocument.Parse(NormalizeJson(configJson));
        return doc.RootElement.TryGetProperty("format", out var format)
            ? format.GetString() ?? DatePartFormats.Default
            : DatePartFormats.Default;
    }

    public static SerialConfig ParseSerial(string configJson)
    {
        using var doc = JsonDocument.Parse(NormalizeJson(configJson));
        var root = doc.RootElement;
        return new SerialConfig
        {
            Radix = root.TryGetProperty("radix", out var radix) ? radix.GetInt32() : 10,
            Digits = root.TryGetProperty("digits", out var digits) ? digits.GetInt32() : 4
        };
    }

    private static string NormalizeJson(string configJson) => string.IsNullOrWhiteSpace(configJson) ? "{}" : configJson;
}
