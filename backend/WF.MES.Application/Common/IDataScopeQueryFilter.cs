namespace WF.MES.Application.Common;

/// <summary>按角色 DataScope + 部门树过滤（骨架接口，模块 Service 可显式调用）。</summary>
public interface IDataScopeQueryFilter
{
    Task<IReadOnlyList<long>> GetAccessibleDeptIdsAsync(CancellationToken cancellationToken = default);
}
