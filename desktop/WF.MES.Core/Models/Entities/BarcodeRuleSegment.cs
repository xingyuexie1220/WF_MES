using SqlSugar;

namespace WF.MES.Models.Entities;

/// <summary>条码规则段（Barcode_RuleSegment）。</summary>
[SugarTable("Barcode_RuleSegment")]
public class BarcodeRuleSegment
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "Segment_Id")]
    public int SegmentId { get; set; }

    [SugarColumn(ColumnName = "Rule_Id")]
    public int RuleId { get; set; }

    [SugarColumn(ColumnName = "Sort_Order")]
    public int SortOrder { get; set; }

    [SugarColumn(ColumnName = "Segment_Type")]
    public string SegmentType { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "Config_Json")]
    public string ConfigJson { get; set; } = "{}";

    [SugarColumn(ColumnName = "Include_In_ResetKey")]
    public int IncludeInResetKey { get; set; } = 1;
}
