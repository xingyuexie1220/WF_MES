using System.IO;
using System.Text;

namespace WF.MES.Infrastructure.Services.Printing;

/// <summary>为 BarTender 文本文件数据库生成临时 CSV。</summary>
internal static class BarTenderPrintDatabaseWriter
{
    public static string WriteBarcodeCsv(
        IReadOnlyList<IReadOnlyDictionary<string, string>> jobs,
        string databaseFieldName)
    {
        if (jobs.Count == 0)
        {
            throw new InvalidOperationException("没有可写入数据库的打印数据");
        }

        if (string.IsNullOrWhiteSpace(databaseFieldName))
        {
            throw new InvalidOperationException("数据库字段名不能为空");
        }

        var filePath = Path.Combine(Path.GetTempPath(), $"wf-mes-barcodes-{Guid.NewGuid():N}.csv");
        var builder = new StringBuilder();
        builder.AppendLine(EscapeCsv(databaseFieldName));

        foreach (var job in jobs)
        {
            builder.AppendLine(EscapeCsv(ResolveBarcodeValue(job)));
        }

        File.WriteAllText(filePath, builder.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        return filePath;
    }

    private static string ResolveBarcodeValue(IReadOnlyDictionary<string, string> job)
    {
        if (job.TryGetValue(BarcodeLabelPrintRequests.BarcodeFieldName, out var barcode))
        {
            return barcode;
        }

        return job.Values.FirstOrDefault() ?? string.Empty;
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
