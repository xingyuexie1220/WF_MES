using SqlSugar;
using WF.MES.Application.Barcode;
using WF.MES.Application.Common;
using WF.MES.Domain.Entities.Barcode;

namespace WF.MES.Infrastructure.Services.Barcode;

public class BarcodeScaffoldService(ISqlSugarClient db, IFactoryContext factoryContext) : IBarcodeScaffoldService
{
    public async Task<List<BarcodeMaterialRuleDto>> GetMaterialRulesAsync(CancellationToken cancellationToken = default)
    {
        var factoryId = factoryContext.CurrentFactoryId ?? 0;
        return await db.Queryable<Domain.Entities.Barcode.BcdMaterialRule>()
            .Where(r => r.FactoryId == factoryId)
            .OrderBy(r => r.MaterialNo)
            .Select(r => new BarcodeMaterialRuleDto
            {
                RuleId = r.RuleId,
                CustomerId = r.CustomerId,
                MaterialNo = r.MaterialNo,
                BarcodeLength = r.BarcodeLength,
                QaStatus = r.QaStatus
            })
            .ToListAsync(cancellationToken);
    }

    public Task<PrintJobDto> CreatePrintJobAsync(CreatePrintJobRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new PrintJobDto
        {
            JobId = Guid.NewGuid().ToString("N"),
            Status = "pending",
            Barcodes = Enumerable.Range(1, Math.Max(1, request.Quantity)).Select(i => $"SCAFFOLD-{request.RuleId}-{i:D4}").ToList()
        });
    }

    public Task ConfirmPrintedAsync(string jobId, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
