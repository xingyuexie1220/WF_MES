using SqlSugar;

namespace WF.MES.Models.Entities;

/// <summary>料号条码规则头表（Bcd_MaterialRule）。</summary>
[SugarTable("Bcd_MaterialRule")]
public class BarcodeMaterialRule
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "Rule_Id")]
    public int RuleId { get; set; }

    [SugarColumn(ColumnName = "Factory_Id")]
    public long FactoryId { get; set; } = 1;

    [SugarColumn(ColumnName = "Customer_Id")]
    public int CustomerId { get; set; }

    [SugarColumn(ColumnName = "Material_No")]
    public string MaterialNo { get; set; } = string.Empty;

    [SugarColumn(ColumnName = "Barcode_Length")]
    public int BarcodeLength { get; set; }

    [SugarColumn(ColumnName = "Qa_Status")]
    public int QaStatus { get; set; }

    [SugarColumn(ColumnName = "Attachment_Uploaded_By")]
    public string? AttachmentUploadedBy { get; set; }

    [SugarColumn(ColumnName = "Attachment_Uploaded_At")]
    public DateTime? AttachmentUploadedAt { get; set; }

    [SugarColumn(ColumnName = "Qa_Reviewed_By")]
    public string? QaReviewedBy { get; set; }

    [SugarColumn(ColumnName = "Qa_Reviewed_At")]
    public DateTime? QaReviewedAt { get; set; }

    [SugarColumn(ColumnName = "Qa_Review_Remark")]
    public string? QaReviewRemark { get; set; }

    [SugarColumn(ColumnName = "Drawing_Image", ColumnDataType = "varbinary(max)", IsNullable = true)]
    public byte[]? DrawingImage { get; set; }

    [SugarColumn(ColumnName = "Print_Sample_Image", ColumnDataType = "varbinary(max)", IsNullable = true)]
    public byte[]? PrintSampleImage { get; set; }

    [SugarColumn(ColumnName = "CreatedBy")]
    public string? CreatedBy { get; set; }

    [SugarColumn(ColumnName = "CreateDate")]
    public DateTime CreateDate { get; set; } = DateTime.Now;

    [SugarColumn(ColumnName = "UpdatedBy")]
    public string? UpdatedBy { get; set; }

    [SugarColumn(ColumnName = "UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }
}
