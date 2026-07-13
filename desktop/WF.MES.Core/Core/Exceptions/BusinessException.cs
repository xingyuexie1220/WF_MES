namespace WF.MES.Core.Exceptions;

/// <summary>业务层可预期错误；<see cref="MessageCode"/> 对应 i18n 键，由 UI 层 <c>L()</c> 解析展示。</summary>
public sealed class BusinessException : Exception
{
    public BusinessException(string messageCode, params object?[] formatArgs) : base(messageCode)
    {
        MessageCode = messageCode;
        FormatArgs = formatArgs;
    }

    public BusinessException(string messageCode, Exception innerException, params object?[] formatArgs): base(messageCode, innerException)
    {
        MessageCode = messageCode;
        FormatArgs = formatArgs;
    }

    public string MessageCode { get; }

    public object?[] FormatArgs { get; }
}
