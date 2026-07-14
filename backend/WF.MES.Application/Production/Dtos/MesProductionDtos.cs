namespace WF.MES.Application.Production.Dtos;

public class MesWorkOrderDto
{
    public long WorkOrderId { get; set; }
    public string WorkOrderNo { get; set; } = string.Empty;
    public string MaterialNo { get; set; } = string.Empty;
    public string? MaterialName { get; set; }
    public long? RoutingId { get; set; }
    public string? RoutingName { get; set; }
    public int PlanQty { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = "open";
    public string Source { get; set; } = "local";
    public List<MesProcessProgressDto> Progress { get; set; } = [];
}

public class MesProcessProgressDto
{
    public string ProcessCode { get; set; } = string.Empty;
    public string? ProcessName { get; set; }
    public int Seq { get; set; }
    public int GoodQty { get; set; }
    public int DefectQty { get; set; }
    public int RemainQty { get; set; }
}

public class SaveMesWorkOrderRequest
{
    public long Id { get; set; }
    public string WorkOrderNo { get; set; } = string.Empty;
    public string MaterialNo { get; set; } = string.Empty;
    public long? RoutingId { get; set; }
    public int PlanQty { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Remark { get; set; }
}

public class MesReportRequest
{
    /// <summary>工单号；兼容 barcode 字段</summary>
    public string? WorkOrderNo { get; set; }
    public string? Barcode { get; set; }
    public string? ProcessCode { get; set; }
    public string? StationCode { get; set; }
    public int GoodQty { get; set; }
    public int DefectQty { get; set; }
    public string? DefectCode { get; set; }
    public string? Disposition { get; set; }
    public string? ReworkToProcess { get; set; }
    public string? MachineNo { get; set; }
}

public class MesReportResultDto
{
    public long Id { get; set; }
    public string WorkOrderNo { get; set; } = string.Empty;
    public string ProcessCode { get; set; } = string.Empty;
    public int GoodQty { get; set; }
    public int DefectQty { get; set; }
    public int RemainQty { get; set; }
    public DateTime ReportTime { get; set; }
}

public class MesReportRecordDto
{
    public long Id { get; set; }
    public string WorkOrderNo { get; set; } = string.Empty;
    public string ProcessCode { get; set; } = string.Empty;
    public int GoodQty { get; set; }
    public int DefectQty { get; set; }
    public string? DefectCode { get; set; }
    public string? MachineNo { get; set; }
    public string? OperatorName { get; set; }
    public DateTime ReportTime { get; set; }
}
