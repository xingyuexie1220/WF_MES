namespace WF.MES.WPF.Modules.Equipment.DeviceAdapters;

public interface IEquipmentTester
{
    Task<EquipmentTestResult> RunTestAsync(string serialNo, CancellationToken cancellationToken = default);
}

public sealed record EquipmentTestResult(bool Passed, string Message, IReadOnlyDictionary<string, string>? Metrics = null);
