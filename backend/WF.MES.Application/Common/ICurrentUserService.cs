using WF.MES.Shared.Enums;

namespace WF.MES.Application.Common;

public interface ICurrentUserService
{
    long? UserId { get; }
    string? UserName { get; }
    long? DeptId { get; }
    ClientType? ClientType { get; }
    long? FactoryId { get; }
    string? SessionId { get; }
    bool IsAuthenticated { get; }
}
