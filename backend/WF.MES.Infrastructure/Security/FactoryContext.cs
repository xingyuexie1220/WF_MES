using WF.MES.Application.Common;

namespace WF.MES.Infrastructure.Security;

public class FactoryContext(IFactoryContextAccessor accessor) : IFactoryContext
{
    public long? CurrentFactoryId => accessor.FactoryId;

    public string? CurrentFactoryCode => accessor.FactoryCode;

    public string? CurrentFactoryName => accessor.FactoryName;

    public bool IsFilterEnabled => accessor.FilterEnabled && CurrentFactoryId.HasValue;

    public void SetFactory(long factoryId, string? factoryCode = null, string? factoryName = null)
    {
        accessor.FactoryId = factoryId;
        accessor.FactoryCode = factoryCode;
        accessor.FactoryName = factoryName;
        accessor.FilterEnabled = true;
    }

    public void DisableFilter()
    {
        accessor.FilterEnabled = false;
    }
}
