using WF.MES.Core.Interfaces;
using WF.MES.WPF.Modules.Equipment.DeviceAdapters;
using WF.MES.WPF.Ui;

namespace WF.MES.WPF.Modules.Equipment.ViewModels;

public class EquipmentTestViewModel : LocalizedViewModelBase
{
    private readonly DeviceAdapterRegistry _devices;
    private string _serialNo = string.Empty;
    private string _resultMessage = string.Empty;
    private string _resultMessageKey = "ui.mes.waitingTest";
    private object[] _resultMessageArgs = [];

    public EquipmentTestViewModel(
        DeviceAdapterRegistry devices,
        ILocalizationService localization)
        : base(localization)
    {
        _devices = devices;
        ApplyResultMessage();

        SubmitCommand = new DelegateCommand(async () => await SubmitAsync());
    }

    public string PageTitle => L("ui.mes.equipmentTitle");

    public string HintText => L("ui.mes.equipmentHint");

    public string SubmitText => L("ui.actions.submitTest");

    public string SerialNo
    {
        get => _serialNo;
        set => SetProperty(ref _serialNo, value);
    }

    public string ResultMessage
    {
        get => _resultMessage;
        private set => SetProperty(ref _resultMessage, value);
    }

    public DelegateCommand SubmitCommand { get; }

    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(SerialNo))
        {
            SetResultMessage("ui.mes.serialRequired");
            return;
        }

        var result = await _devices.EquipmentTester.RunTestAsync(SerialNo.Trim());
        SetResultMessage(
            result.Passed ? "ui.mes.testPassed" : "ui.mes.testFailed",
            result.Message);
    }

    private void SetResultMessage(string key, params object[] args)
    {
        _resultMessageKey = key;
        _resultMessageArgs = args;
        ApplyResultMessage();
    }

    private void ApplyResultMessage()
    {
        ResultMessage = _resultMessageArgs.Length == 0
            ? L(_resultMessageKey)
            : string.Format(L(_resultMessageKey), _resultMessageArgs);
    }
}
