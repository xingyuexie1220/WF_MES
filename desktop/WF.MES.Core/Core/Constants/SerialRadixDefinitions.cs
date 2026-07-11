namespace WF.MES.Core.Constants;

/// <summary>流水号进制定义与字符集（10/16/32/34/36）。</summary>
public static class SerialRadixDefinitions
{
    public static readonly int[] SupportedRadices = [10, 16, 32, 34, 36];

    /// <summary>32 进制字符集（排除易混字符 I、L、O、U）。</summary>
    public static readonly char[] Base32Chars = "0123456789ABCDEFGHJKLMNPQRSTUV".ToCharArray();

    /// <summary>34 进制字符集：0-9 + A-Z（排除 I、O）。</summary>
    public static readonly char[] Base34Chars = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZ".ToCharArray();

    /// <summary>36 进制字符集：0-9 + A-Z。</summary>
    public static readonly char[] Base36Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    public static bool IsSupported(int radix) => SupportedRadices.Contains(radix);

    public static string GetDisplayName(int radix) => radix switch
    {
        10 => "10进制",
        16 => "16进制",
        32 => "32进制",
        34 => "34进制",
        36 => "36进制",
        _ => $"{radix}进制"
    };

    public static string GetDescription(int radix) => radix switch
    {
        10 => "纯数字 0~9",
        16 => "数字 0~9 与字母 A~F",
        32 => "数字 0~9 与 24 个字母（不含 I、L、O、U）",
        34 => "数字 0~9 与 24 个字母（不含 I、O）",
        36 => "数字 0~9 与字母 A~Z",
        _ => string.Empty
    };
}
