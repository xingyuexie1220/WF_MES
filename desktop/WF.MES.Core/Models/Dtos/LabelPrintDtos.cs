namespace WF.MES.Models.Dtos;

/// <summary>BarTender 批量打印请求。</summary>
public class LabelPrintRequestDto
{
    public string TemplatePath { get; set; } = string.Empty;

    public string PrinterName { get; set; } = string.Empty;

    public IReadOnlyList<IReadOnlyDictionary<string, string>> Jobs { get; set; } = [];
}

/// <summary>打印完成结果。</summary>
public class LabelPrintResultDto
{
    public string Message { get; init; } = string.Empty;
}

/// <summary>打印进度（绑定 UI 状态文本）。</summary>
public class LabelPrintProgressDto
{
    public int Current { get; init; }

    public int Total { get; init; }

    public string StatusText => $"正在打印 {Current}/{Total}";
}
