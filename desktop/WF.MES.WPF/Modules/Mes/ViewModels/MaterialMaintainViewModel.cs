using WF.MES.Core.Interfaces;
using WF.MES.WPF.Infrastructure;

namespace WF.MES.WPF.Modules.Mes.ViewModels;

/// <summary>物料维护（待实现）。</summary>
public class MaterialMaintainViewModel : LocalizedViewModelBase
{
    public MaterialMaintainViewModel(ILocalizationService localization)
        : base(localization)
    {
    }

    public string PageTitle => L("ui.mes.materialMaintain");

    protected override void RefreshLocalizedProperties()
    {
        RaisePropertyChanged(nameof(PageTitle));
    }
}
