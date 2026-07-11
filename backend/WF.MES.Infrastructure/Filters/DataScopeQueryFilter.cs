using WF.MES.Application.Common;

namespace WF.MES.Infrastructure.Filters;

/// <summary>骨架实现：返回当前用户部门；完整 DataScope 树过滤后续在 UserService 中扩展。</summary>
public class DataScopeQueryFilter(ICurrentUserService currentUser) : IDataScopeQueryFilter
{
    public Task<IReadOnlyList<long>> GetAccessibleDeptIdsAsync(CancellationToken cancellationToken = default)
    {
        if (currentUser.DeptId.HasValue && currentUser.DeptId.Value > 0)
        {
            return Task.FromResult<IReadOnlyList<long>>([currentUser.DeptId.Value]);
        }

        return Task.FromResult<IReadOnlyList<long>>([]);
    }
}
