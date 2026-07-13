using FluentValidation;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using Serilog;
using WF.MES.Core.Exceptions;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.Infrastructure.Validation;

namespace WF.MES.Infrastructure.Services.Printing;

/// <summary>BarTender COM 打印门面；每 job 在 STA 线程启动独立 Application 实例。</summary>
public class BarTenderLabelPrintService : ILabelPrintService
{
    private readonly IValidator<LabelPrintRequestDto> _validator;

    public BarTenderLabelPrintService(IValidator<LabelPrintRequestDto> validator)
    {
        _validator = validator;
    }
    public IReadOnlyList<string> GetInstalledPrinters()
    {
        var printers = new List<string>();
        foreach (string printer in PrinterSettings.InstalledPrinters)
        {
            printers.Add(printer);
        }

        return printers;
    }

    public void OpenTemplateFolder(string materialNo)
    {
        if (string.IsNullOrWhiteSpace(materialNo))
        {
            throw new BusinessException("err.materialNoRequired");
        }

        var folder = BarcodeLabelPrintRequests.GetTemplateFolderPath();
        Directory.CreateDirectory(folder);

        var templatePath = BarcodeLabelPrintRequests.GetTemplatePath(materialNo);
        Log.Information("打开标签模板文件夹: {Folder}，料号 {MaterialNo}", folder, materialNo);

        if (File.Exists(templatePath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{templatePath}\"",
                UseShellExecute = true
            });
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = folder,
            UseShellExecute = true
        });

        throw new BusinessException("err.labelTemplateNotFound", materialNo);
    }

    public LabelPrintRequestDto CreatePrintRequest(string materialNo, string printerName, IEnumerable<string> barcodes) =>
        BarcodeLabelPrintRequests.Create(materialNo, printerName, barcodes);

    /// <remarks>BarTender COM 打印为同步整批 PrintOut，无法在打印过程中取消。</remarks>
    public Task<LabelPrintResultDto> PrintAsync(
        LabelPrintRequestDto request,
        IProgress<LabelPrintProgressDto>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _validator.ValidateRequest(request);

        if (!File.Exists(request.TemplatePath))
        {
            throw new BusinessException("err.labelTemplatePathNotFound", request.TemplatePath);
        }

        var total = request.Jobs.Count;
        Log.Information(
            "BarTender COM 打印：打印机 {Printer}，模板 {Template}，条码数量 {Count}",
            request.PrinterName,
            request.TemplatePath,
            total);

        return StaTaskRunner.Run(() =>
        {
            try
            {
                using var engine = BarTenderComPrintEngine.Create();

                engine.PrintJobs(
                    request.TemplatePath,
                    request.PrinterName,
                    request.Jobs,
                    new Progress<int>(current =>
                        progress?.Report(new LabelPrintProgressDto { Current = current, Total = total })));

                return new LabelPrintResultDto
                {
                    PrintedCount = total
                };
            }
            catch (Exception ex)
            {
                throw UnwrapException(ex);
            }
        });
    }

    private static Exception UnwrapException(Exception ex)
    {
        if (ex is TargetInvocationException tie)
        {
            return tie.InnerException ?? ex;
        }

        if (ex is AggregateException { InnerExceptions.Count: 1 } aggregate && aggregate.InnerExceptions[0] is Exception inner)
        {
            return UnwrapException(inner);
        }

        return ex;
    }
}
