namespace WF.MES.WPF.DeviceAdapters;

public interface ILabelPrinter
{
    Task PrintAsync(IReadOnlyList<string> barcodes, CancellationToken cancellationToken = default);
}

public interface IEquipmentTester
{
    Task<EquipmentTestResult> RunTestAsync(string serialNo, CancellationToken cancellationToken = default);
}

public interface IScannerInput
{
    event EventHandler<string>? BarcodeScanned;
    void Start();
    void Stop();
}

public sealed record EquipmentTestResult(bool Passed, string Message, IReadOnlyDictionary<string, string>? Metrics = null);
