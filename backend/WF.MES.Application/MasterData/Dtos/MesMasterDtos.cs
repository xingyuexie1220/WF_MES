namespace WF.MES.Application.MasterData.Dtos;

public class MesProcessDto
{
    public long Id { get; set; }
    public string ProcessCode { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public int DefaultSeq { get; set; }
    public bool Enabled { get; set; }
    public string? Remark { get; set; }
}

public class SaveMesProcessRequest
{
    public long Id { get; set; }
    public string ProcessCode { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public int DefaultSeq { get; set; }
    public bool Enabled { get; set; } = true;
    public string? Remark { get; set; }
}

public class MesRoutingStepDto
{
    public string ProcessCode { get; set; } = string.Empty;
    public string? ProcessName { get; set; }
    public int Seq { get; set; }
}

public class MesRoutingDto
{
    public long Id { get; set; }
    public string RoutingCode { get; set; } = string.Empty;
    public string RoutingName { get; set; } = string.Empty;
    public string? MaterialNo { get; set; }
    public bool Enabled { get; set; }
    public string? Remark { get; set; }
    public List<MesRoutingStepDto> Steps { get; set; } = [];
}

public class SaveMesRoutingRequest
{
    public long Id { get; set; }
    public string RoutingCode { get; set; } = string.Empty;
    public string RoutingName { get; set; } = string.Empty;
    public string? MaterialNo { get; set; }
    public bool Enabled { get; set; } = true;
    public string? Remark { get; set; }
    public List<MesRoutingStepDto> Steps { get; set; } = [];
}

public class MesMaterialDto
{
    public long Id { get; set; }
    public string MaterialNo { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string? Spec { get; set; }
    public string? Unit { get; set; }
    public string Source { get; set; } = "local";
    public bool Enabled { get; set; }
}

public class SaveMesMaterialRequest
{
    public long Id { get; set; }
    public string MaterialNo { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string? Spec { get; set; }
    public string? Unit { get; set; }
    public bool Enabled { get; set; } = true;
}

public class MesMachineDto
{
    public long Id { get; set; }
    public string MachineNo { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string? Remark { get; set; }
}

public class SaveMesMachineRequest
{
    public long Id { get; set; }
    public string MachineNo { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public string? Remark { get; set; }
}

public class MesDefectCodeDto
{
    public long Id { get; set; }
    public string DefectCode { get; set; } = string.Empty;
    public string DefectName { get; set; } = string.Empty;
    public int Sort { get; set; }
    public bool Enabled { get; set; }
}
