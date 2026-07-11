using WF.MES.Core.Interfaces;
using WF.MES.WPF.Infrastructure;

namespace WF.MES.WPF.Modules.Mes.ViewModels;

/// <summary>物料维护（待实现）。</summary>
public class MaterialMaintainViewModel : LocalizedViewModelBase
{
    public MaterialMaintainViewModel(ILocalizationService localization, IDesktopUiText ui)
        : base(localization)
    {
        Ui = ui;
    }

    public IDesktopUiText Ui { get; }

    public string PageTitle => L("desktop.mes.materialMaintain");

    public string DisplayText => PageTitle + L("desktop.mes.stubSuffix");

    protected override void RefreshLocalizedProperties()
    {
        RaisePropertyChanged(nameof(PageTitle));
        RaisePropertyChanged(nameof(DisplayText));
    }
}
