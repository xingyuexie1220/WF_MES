using WF.MES.Core.Interfaces;
using WF.MES.WPF.Ui;

namespace WF.MES.WPF.Modules.Mes.ViewModels;

/// <summary>MES 返工装箱（待实现）。</summary>
public class BoxReworkViewModel : LocalizedViewModelBase
{
    public BoxReworkViewModel(ILocalizationService localization)
        : base(localization)
    {
    }

    public string PageTitle => L("ui.mes.boxRework");
}
