namespace WF.MES.WPF.Modules.Equipment.DeviceAdapters;

public sealed class MockEquipmentTester : IEquipmentTester
{
    public Task<EquipmentTestResult> RunTestAsync(string serialNo, CancellationToken cancellationToken = default)
    {
        // Message 仅承载设备原始结果（如序列号），UI 文案由 ViewModel 用 i18n 格式化
        return Task.FromResult(new EquipmentTestResult(true, serialNo, new Dictionary<string, string>
        {
            ["voltage"] = "3.3",
            ["current"] = "0.5"
        }));
    }
}
