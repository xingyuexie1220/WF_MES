using WF.MES.Core.Interfaces;
using WF.MES.WPF.Infrastructure;

namespace WF.MES.WPF.Modules.Mes.ViewModels;

/// <summary>物料扫描（待实现）。</summary>
public class MaterialScanViewModel : LocalizedViewModelBase
{
    public MaterialScanViewModel(ILocalizationService localization)
        : base(localization)
    {
    }

    public string PageTitle => L("ui.mes.materialScan");

    protected override void RefreshLocalizedProperties()
    {
        RaisePropertyChanged(nameof(PageTitle));
    }
}
