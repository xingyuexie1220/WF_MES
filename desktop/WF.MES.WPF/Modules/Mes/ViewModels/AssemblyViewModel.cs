using WF.MES.Core.Interfaces;
using WF.MES.WPF.Ui;

namespace WF.MES.WPF.Modules.Mes.ViewModels;

public class AssemblyViewModel : LocalizedViewModelBase
{
    public AssemblyViewModel(ILocalizationService localization)
        : base(localization)
    {
    }

    public string PageTitle => L("ui.mes.assemblyTitle");

    public string HintText => L("ui.mes.assemblyHint");
}
