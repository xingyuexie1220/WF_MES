namespace WF.MES.Shared.Exceptions;

public class BusinessException : Exception
{
    public int Code { get; }
    public string? MessageCode { get; }
    public object? MessageArgs { get; }

    public BusinessException(string message, int code = 400, string? messageCode = null, object? messageArgs = null)
        : base(message)
    {
        Code = code;
        MessageCode = messageCode;
        MessageArgs = messageArgs;
    }
}
