using WF.MES.Core.Interfaces;
using WF.MES.WPF.Infrastructure;

namespace WF.MES.WPF.Modules.Mes.ViewModels;

/// <summary>MES 工单扫描（待实现）。</summary>
public class WorkOrderScanViewModel : LocalizedViewModelBase
{
    public WorkOrderScanViewModel(ILocalizationService localization)
        : base(localization)
    {
    }

    public string PageTitle => L("ui.mes.workOrderScan");

    protected override void RefreshLocalizedProperties()
    {
        RaisePropertyChanged(nameof(PageTitle));
    }
}
