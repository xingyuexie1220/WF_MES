namespace WF.MES.Infrastructure.Security;

public interface IFactoryContextAccessor
{
    long? FactoryId { get; set; }
    string? FactoryCode { get; set; }
    string? FactoryName { get; set; }
    bool FilterEnabled { get; set; }
}

public class FactoryContextAccessor : IFactoryContextAccessor
{
    public long? FactoryId { get; set; }
    public string? FactoryCode { get; set; }
    public string? FactoryName { get; set; }
    public bool FilterEnabled { get; set; } = true;
}
