using System.IO;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Services.Printing;

/// <summary>BarTender 模板路径与打印请求构造（Labels/{料号}.btw）。</summary>
public static class BarcodeLabelPrintRequests
{
    public const string TemplateFolderName = "Labels";

    public const string BarcodeFieldName = "Barcode";

    public static string GetTemplatePath(string materialNo) => Path.Combine(AppContext.BaseDirectory, TemplateFolderName, $"{materialNo}.btw");

    public static string GetTemplateFolderPath() => Path.Combine(AppContext.BaseDirectory, TemplateFolderName);

    public static LabelPrintRequestDto Create(string materialNo, string printerName, IEnumerable<string> barcodes) =>
        new()
        {
            TemplatePath = GetTemplatePath(materialNo),
            PrinterName = printerName,
            Jobs = barcodes
                .Select(barcode => (IReadOnlyDictionary<string, string>)new Dictionary<string, string>
                {
                    [BarcodeFieldName] = barcode
                })
                .ToList()
        };
}
