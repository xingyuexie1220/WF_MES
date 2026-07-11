using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WF.MES.WPF.Modules.Barcode;

/// <summary>条码资料审核弹窗图片预览。</summary>
internal static class BarcodeQaReviewImagePreview
{
    public static ImageSource? FromBytes(byte[]? bytes)
    {
        if (bytes is not { Length: > 0 })
        {
            return null;
        }

        using var stream = new MemoryStream(bytes);
        return FromStream(stream);
    }

    public static ImageSource? FromFile(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        using var stream = File.OpenRead(path);
        return FromStream(stream);
    }

    private static ImageSource FromStream(Stream stream)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        return image;
    }
}
