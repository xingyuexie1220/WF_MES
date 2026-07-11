using System.ComponentModel;

namespace WF.MES.WPF.Infrastructure;

/// <summary>桌面端通用 UI 文案（字段名、按钮、列头等），随语言切换刷新。</summary>
public interface IDesktopUiText : INotifyPropertyChanged
{
    string Add { get; }
    string Edit { get; }
    string Refresh { get; }
    string Save { get; }
    string Query { get; }
    string Export { get; }
    string Enable { get; }
    string Preview { get; }
    string Close { get; }
    string Delete { get; }
    string All { get; }
    string Cancel { get; }
    string Confirm { get; }
    string Upload { get; }
    string Review { get; }
    string Processing { get; }

    string Customer { get; }
    string CustomerName { get; }
    string MaterialNo { get; }
    string Status { get; }
    string GenerateNo { get; }
    string PrintDate { get; }
    string Quantity { get; }
    string SerialRange { get; }
    string PrintStatus { get; }
    string CreatedAt { get; }
    string CreatedBy { get; }
    string UpdatedBy { get; }
    string UpdatedAt { get; }
    string Operator { get; }
    string Printer { get; }
    string BarcodeLength { get; }
    string Segment { get; }
    string Drawing { get; }
    string SampleImage { get; }
    string Reviewer { get; }
    string ReviewedAt { get; }
    string RejectReason { get; }
    string SerialValue { get; }
    string FullBarcode { get; }
    string ReprintAt { get; }
    string ReprintBy { get; }
    string GenerateTimeFrom { get; }
    string MaterialFilter { get; }
}
