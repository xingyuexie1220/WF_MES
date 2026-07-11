namespace WF.MES.Shared.Common;

public class ApiResult<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? MessageCode { get; set; }
    public object? MessageArgs { get; set; }
    public T? Data { get; set; }
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public static ApiResult<T> Ok(T? data, string message = "success")
        => new() { Code = 200, Message = message, Data = data };

    public static ApiResult<T> Fail(string message, int code = 400, string? messageCode = null, object? messageArgs = null)
        => new() { Code = code, Message = message, MessageCode = messageCode, MessageArgs = messageArgs };
}

public class ApiResult : ApiResult<object>
{
    public static ApiResult Ok(string message = "success")
        => new() { Code = 200, Message = message };

    public new static ApiResult Fail(string message, int code = 400)
        => new() { Code = code, Message = message };
}
