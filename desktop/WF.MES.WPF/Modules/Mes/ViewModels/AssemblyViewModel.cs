using WF.MES.Core.Interfaces;
using WF.MES.WPF.Infrastructure;

namespace WF.MES.WPF.Modules.Mes.ViewModels;

public class AssemblyViewModel : LocalizedViewModelBase
{
    public AssemblyViewModel(ILocalizationService localization, IDesktopUiText ui)
        : base(localization)
    {
        Ui = ui;
    }

    public IDesktopUiText Ui { get; }

    public string PageTitle => L("desktop.mes.assemblyTitle");

    public string HintText => L("desktop.mes.assemblyHint");

    protected override void RefreshLocalizedProperties()
    {
        RaisePropertyChanged(nameof(PageTitle));
        RaisePropertyChanged(nameof(HintText));
    }
}
