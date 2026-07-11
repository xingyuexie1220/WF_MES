namespace WF.MES.Models.Dtos;

/// <summary>审计字段在列表中的显示格式。</summary>
public static class AuditFieldDisplay
{
    public static string FormatUser(string? value) => string.IsNullOrWhiteSpace(value) ? "-" : value;

    public static string FormatDateTime(DateTime? value) => value?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
}

/// <summary>含 CreatedBy/UpdatedBy 等审计字段的 DTO 契约。</summary>
public interface IAuditFieldsDto
{
    string? CreatedBy { get; }

    DateTime? CreateDate { get; }

    string? UpdatedBy { get; }

    DateTime? UpdatedAt { get; }
}

/// <summary>审计字段绑定列 Text 扩展。</summary>
public static class AuditFieldsDtoExtensions
{
    public static string GetCreatedByText(this IAuditFieldsDto dto) => AuditFieldDisplay.FormatUser(dto.CreatedBy);

    public static string GetCreateDateText(this IAuditFieldsDto dto) => AuditFieldDisplay.FormatDateTime(dto.CreateDate);

    public static string GetUpdatedByText(this IAuditFieldsDto dto) => AuditFieldDisplay.FormatUser(dto.UpdatedBy);

    public static string GetUpdatedAtText(this IAuditFieldsDto dto) => AuditFieldDisplay.FormatDateTime(dto.UpdatedAt);
}
