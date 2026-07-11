namespace WF.MES.WPF.DeviceAdapters;

/// <summary>键盘楔入式扫码枪占位（事件由 UI 手动触发）。</summary>
public sealed class KeyboardWedgeScanner : IScannerInput
{
    public event EventHandler<string>? BarcodeScanned;

    public void Start() { }

    public void Stop() { }

    public void SimulateScan(string barcode) => BarcodeScanned?.Invoke(this, barcode);
}
