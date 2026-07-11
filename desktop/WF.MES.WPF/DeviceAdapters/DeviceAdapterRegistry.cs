using Microsoft.Extensions.Configuration;

namespace WF.MES.WPF.DeviceAdapters;

public sealed class DeviceAdapterRegistry
{
    public ILabelPrinter LabelPrinter { get; }
    public IEquipmentTester EquipmentTester { get; }
    public IScannerInput ScannerInput { get; }

    public DeviceAdapterRegistry(IConfiguration configuration)
    {
        var printer = configuration["Devices:LabelPrinter"] ?? "Mock";
        LabelPrinter = printer.Equals("BarTender", StringComparison.OrdinalIgnoreCase)
            ? new MockLabelPrinter()
            : new MockLabelPrinter();

        var tester = configuration["Devices:EquipmentTester"] ?? "Mock";
        EquipmentTester = tester.Equals("SerialPort", StringComparison.OrdinalIgnoreCase)
            ? new MockEquipmentTester()
            : new MockEquipmentTester();

        ScannerInput = new KeyboardWedgeScanner();
    }
}
