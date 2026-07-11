using System.ComponentModel;
using WF.MES.Core.Interfaces;

namespace WF.MES.WPF.Infrastructure;

public sealed class DesktopUiTextService : IDesktopUiText
{
    private readonly ILocalizationService _localization;

    public DesktopUiTextService(ILocalizationService localization)
    {
        _localization = localization;
        _localization.LocaleChanged += (_, _) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }

    public string Add => T("desktop.actions.add");
    public string Edit => T("desktop.actions.edit");
    public string Refresh => T("desktop.actions.refresh");
    public string Save => T("desktop.actions.save");
    public string Query => T("desktop.actions.query");
    public string Export => T("desktop.actions.export");
    public string Enable => T("desktop.actions.enable");
    public string Preview => T("desktop.actions.preview");
    public string Close => T("desktop.actions.close");
    public string Delete => T("desktop.actions.delete");
    public string All => T("desktop.actions.all");
    public string Cancel => T("common.cancel");
    public string Confirm => T("common.confirm");
    public string Upload => T("desktop.actions.upload");
    public string Review => T("desktop.actions.review");
    public string Processing => T("desktop.actions.processing");

    public string Customer => T("desktop.fields.customer");
    public string CustomerName => T("desktop.fields.customerName");
    public string MaterialNo => T("desktop.fields.materialNo");
    public string Status => T("desktop.fields.status");
    public string GenerateNo => T("desktop.fields.generateNo");
    public string PrintDate => T("desktop.fields.printDate");
    public string Quantity => T("desktop.fields.quantity");
    public string SerialRange => T("desktop.fields.serialRange");
    public string PrintStatus => T("desktop.fields.printStatus");
    public string CreatedAt => T("desktop.fields.createdAt");
    public string CreatedBy => T("desktop.fields.createdBy");
    public string UpdatedBy => T("desktop.fields.updatedBy");
    public string UpdatedAt => T("desktop.fields.updatedAt");
    public string Operator => T("desktop.fields.operator");
    public string Printer => T("desktop.fields.printer");
    public string BarcodeLength => T("desktop.fields.barcodeLength");
    public string Segment => T("desktop.fields.segment");
    public string Drawing => T("desktop.fields.drawing");
    public string SampleImage => T("desktop.fields.sampleImage");
    public string Reviewer => T("desktop.fields.reviewer");
    public string ReviewedAt => T("desktop.fields.reviewedAt");
    public string RejectReason => T("desktop.fields.rejectReason");
    public string SerialValue => T("desktop.fields.serialValue");
    public string FullBarcode => T("desktop.fields.fullBarcode");
    public string ReprintAt => T("desktop.fields.reprintAt");
    public string ReprintBy => T("desktop.fields.reprintBy");
    public string GenerateTimeFrom => T("desktop.fields.generateTimeFrom");
    public string MaterialFilter => T("desktop.fields.materialFilter");

    public event PropertyChangedEventHandler? PropertyChanged;

    private string T(string key) => _localization.T(key);
}
