using WF.MES.Core.Interfaces;
using WF.MES.WPF.Ui;

namespace WF.MES.WPF.Modules.Material.ViewModels;

/// <summary>物料扫描（待实现）。</summary>
public class MaterialScanViewModel : LocalizedViewModelBase
{
    public MaterialScanViewModel(ILocalizationService localization)
        : base(localization)
    {
    }

    public string PageTitle => L("ui.mes.materialScan");
}
