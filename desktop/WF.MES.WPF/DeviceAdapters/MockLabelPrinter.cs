using Serilog;

namespace WF.MES.WPF.DeviceAdapters;

/// <summary>骨架 Mock：记录日志，不实际出纸；BarTender 实现后续替换。</summary>
public sealed class MockLabelPrinter : ILabelPrinter
{
    public Task PrintAsync(IReadOnlyList<string> barcodes, CancellationToken cancellationToken = default)
    {
        Log.Information("Mock 打印 {Count} 个条码: {Sample}", barcodes.Count, barcodes.FirstOrDefault());
        return Task.CompletedTask;
    }
}
