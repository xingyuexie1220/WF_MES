using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Serilog;
using WF.MES.Core.Exceptions;
using WF.MES.Core.Interfaces;

namespace WF.MES.Infrastructure.Services;

public class SopService : ISopService
{
    private readonly string? _pdfUrl;

    public SopService(IConfiguration configuration)
    {
        _pdfUrl = configuration["Sop:PdfUrl"]?.Trim();
    }

    public void OpenPdf()
    {
        if (string.IsNullOrWhiteSpace(_pdfUrl))
        {
            throw new BusinessException("err.sopPdfUrlNotConfigured");
        }

        Log.Information("打开 SOP 文档: {PdfUrl}", _pdfUrl);

        Process.Start(new ProcessStartInfo
        {
            FileName = _pdfUrl,
            UseShellExecute = true
        });
    }
}
