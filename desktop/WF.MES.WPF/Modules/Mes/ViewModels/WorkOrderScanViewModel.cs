using WF.MES.Core.Interfaces;
using WF.MES.WPF.Infrastructure;

namespace WF.MES.WPF.Modules.Mes.ViewModels;

/// <summary>MES 工单扫描（待实现）。</summary>
public class WorkOrderScanViewModel : LocalizedViewModelBase
{
    public WorkOrderScanViewModel(ILocalizationService localization, IDesktopUiText ui)
        : base(localization)
    {
        Ui = ui;
    }

    public IDesktopUiText Ui { get; }

    public string PageTitle => L("desktop.mes.workOrderScan");

    public string DisplayText => PageTitle + L("desktop.mes.stubSuffix");

    protected override void RefreshLocalizedProperties()
    {
        RaisePropertyChanged(nameof(PageTitle));
        RaisePropertyChanged(nameof(DisplayText));
    }
}
