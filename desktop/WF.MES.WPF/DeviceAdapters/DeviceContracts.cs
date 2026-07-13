namespace WF.MES.WPF.DeviceAdapters;

public interface IEquipmentTester
{
    Task<EquipmentTestResult> RunTestAsync(string serialNo, CancellationToken cancellationToken = default);
}

public sealed record EquipmentTestResult(bool Passed, string Message, IReadOnlyDictionary<string, string>? Metrics = null);
