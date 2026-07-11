namespace WF.MES.Domain.Common;

public abstract class FactoryScopedEntity : BaseEntity, IFactoryScoped
{
    public long FactoryId { get; set; }
}
