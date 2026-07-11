using SqlSugar;

namespace WF.MES.Domain.Common;

public abstract class BaseEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;

    public long? CreateBy { get; set; }

    public DateTime? UpdateTime { get; set; }

    public long? UpdateBy { get; set; }

    public bool IsDeleted { get; set; }
}
