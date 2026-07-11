using WF.MES.Core.Interfaces;
using WF.MES.WPF.DeviceAdapters;
using WF.MES.WPF.Infrastructure;

namespace WF.MES.WPF.Modules.Equipment.ViewModels;

public class EquipmentTestViewModel : LocalizedViewModelBase
{
    private readonly DeviceAdapterRegistry _devices;
    private string _serialNo = string.Empty;
    private string _resultMessage;

    public EquipmentTestViewModel(
        DeviceAdapterRegistry devices,
        ILocalizationService localization,
        IDesktopUiText ui)
        : base(localization)
    {
        _devices = devices;
        Ui = ui;
        _resultMessage = L("desktop.mes.waitingTest");

        SubmitCommand = new DelegateCommand(async () => await SubmitAsync());
    }

    public IDesktopUiText Ui { get; }

    public string PageTitle => L("desktop.mes.equipmentTitle");

    public string HintText => L("desktop.mes.equipmentHint");

    public string SubmitText => L("desktop.actions.submitTest");

    public string SerialNo
    {
        get => _serialNo;
        set => SetProperty(ref _serialNo, value);
    }

    public string ResultMessage
    {
        get => _resultMessage;
        set => SetProperty(ref _resultMessage, value);
    }

    public DelegateCommand SubmitCommand { get; }

    protected override void RefreshLocalizedProperties()
    {
        RaisePropertyChanged(nameof(PageTitle));
        RaisePropertyChanged(nameof(HintText));
        RaisePropertyChanged(nameof(SubmitText));
    }

    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(SerialNo))
        {
            ResultMessage = L("desktop.mes.serialRequired");
            return;
        }

        var result = await _devices.EquipmentTester.RunTestAsync(SerialNo.Trim());
        ResultMessage = result.Passed
            ? string.Format(L("desktop.mes.testPassed"), result.Message)
            : string.Format(L("desktop.mes.testFailed"), result.Message);
    }
}
