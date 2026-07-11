namespace WF.MES.Application.Dicts.Dtos;

public class DictTypeDto
{
    public long Id { get; set; }
    public string DictName { get; set; } = string.Empty;
    public string DictType { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? Remark { get; set; }
    public long? CreateBy { get; set; }
    public string? CreateByName { get; set; }
    public DateTime CreateTime { get; set; }
    public long? UpdateBy { get; set; }
    public string? UpdateByName { get; set; }
    public DateTime? UpdateTime { get; set; }
}

public class DictDataDto
{
    public long Id { get; set; }
    public long DictTypeId { get; set; }
    public string DictType { get; set; } = string.Empty;
    public string DictLabel { get; set; } = string.Empty;
    public string DictValue { get; set; } = string.Empty;
    public int Sort { get; set; }
    public int Status { get; set; }
    public string? Remark { get; set; }
    public long? CreateBy { get; set; }
    public string? CreateByName { get; set; }
    public DateTime CreateTime { get; set; }
    public long? UpdateBy { get; set; }
    public string? UpdateByName { get; set; }
    public DateTime? UpdateTime { get; set; }
}

public class DictTypeQueryRequest
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? DictName { get; set; }
    public string? DictType { get; set; }
    public int? Status { get; set; }
}

public class DictDataQueryRequest
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? DictType { get; set; }
    public long? DictTypeId { get; set; }
    public string? DictLabel { get; set; }
    public int? Status { get; set; }
}

public class CreateDictTypeRequest
{
    public string DictName { get; set; } = string.Empty;
    public string DictType { get; set; } = string.Empty;
    public int Status { get; set; } = 1;
    public string? Remark { get; set; }
}

public class UpdateDictTypeRequest
{
    public string DictName { get; set; } = string.Empty;
    public int Status { get; set; } = 1;
    public string? Remark { get; set; }
}

public class CreateDictDataRequest
{
    public long DictTypeId { get; set; }
    public string DictLabel { get; set; } = string.Empty;
    public string DictValue { get; set; } = string.Empty;
    public int Sort { get; set; }
    public int Status { get; set; } = 1;
    public string? Remark { get; set; }
}

public class UpdateDictDataRequest
{
    public string DictLabel { get; set; } = string.Empty;
    public string DictValue { get; set; } = string.Empty;
    public int Sort { get; set; }
    public int Status { get; set; } = 1;
    public string? Remark { get; set; }
}
