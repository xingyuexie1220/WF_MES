namespace WF.MES.Application.Common;

public interface IFactoryContext
{
    long? CurrentFactoryId { get; }
    string? CurrentFactoryCode { get; }
    string? CurrentFactoryName { get; }
    bool IsFilterEnabled { get; }
    void SetFactory(long factoryId, string? factoryCode = null, string? factoryName = null);
    void DisableFilter();
}
