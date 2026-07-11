namespace WF.MES.WPF.DeviceAdapters;

public sealed class MockEquipmentTester : IEquipmentTester
{
    public Task<EquipmentTestResult> RunTestAsync(string serialNo, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new EquipmentTestResult(true, $"Mock 测试通过: {serialNo}", new Dictionary<string, string>
        {
            ["voltage"] = "3.3",
            ["current"] = "0.5"
        }));
    }
}
