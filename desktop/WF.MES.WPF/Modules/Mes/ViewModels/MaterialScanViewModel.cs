using WF.MES.Core.Interfaces;
using WF.MES.WPF.Infrastructure;

namespace WF.MES.WPF.Modules.Mes.ViewModels;

/// <summary>物料扫描（待实现）。</summary>
public class MaterialScanViewModel : LocalizedViewModelBase
{
    public MaterialScanViewModel(ILocalizationService localization, IDesktopUiText ui)
        : base(localization)
    {
        Ui = ui;
    }

    public IDesktopUiText Ui { get; }

    public string PageTitle => L("desktop.mes.materialScan");

    public string DisplayText => PageTitle + L("desktop.mes.stubSuffix");

    protected override void RefreshLocalizedProperties()
    {
        RaisePropertyChanged(nameof(PageTitle));
        RaisePropertyChanged(nameof(DisplayText));
    }
}
