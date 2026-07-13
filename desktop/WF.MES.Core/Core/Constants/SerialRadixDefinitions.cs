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
}
