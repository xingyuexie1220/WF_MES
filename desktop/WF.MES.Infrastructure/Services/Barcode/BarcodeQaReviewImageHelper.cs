using System.Drawing;
using System.Drawing.Imaging;

namespace WF.MES.Infrastructure.Services.Barcode;

/// <summary>条码资料审核图片读取与压缩。</summary>
internal static class BarcodeQaReviewImageHelper
{
    private const int MaxBytes = 5 * 1024 * 1024;
    private const int MaxEdgePixels = 1920;
    private const long JpegQuality = 80L;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".bmp"];

    public static byte[] PrepareImageFromFile(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
        {
            throw new InvalidOperationException("图片文件不存在");
        }

        var extension = Path.GetExtension(sourcePath);
        if (!AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("仅支持 JPG、PNG、BMP 图片");
        }

        using var image = Image.FromFile(sourcePath);
        using var normalized = NormalizeOrientation(image);
        using var resized = ResizeIfNeeded(normalized);
        var jpegBytes = EncodeJpeg(resized);

        if (jpegBytes.Length > MaxBytes)
        {
            throw new InvalidOperationException($"图片压缩后仍超过 {MaxBytes / (1024 * 1024)}MB，请换较小图片");
        }

        return jpegBytes;
    }

    public static void ValidateImageBytes(byte[]? imageBytes)
    {
        if (imageBytes == null || imageBytes.Length == 0)
        {
            return;
        }

        if (imageBytes.Length > MaxBytes)
        {
            throw new InvalidOperationException($"单张图片不能超过 {MaxBytes / (1024 * 1024)}MB");
        }
    }

    private static Image NormalizeOrientation(Image image)
    {
        const int orientationId = 0x0112;
        if (!image.PropertyIdList.Contains(orientationId))
        {
            return (Image)image.Clone();
        }

        var propertyItem = image.GetPropertyItem(orientationId);
        var orientation = propertyItem?.Value is { Length: > 0 } valueBytes ? valueBytes[0] : (byte)1;
        var clone = (Image)image.Clone();
        switch (orientation)
        {
            case 2:
                clone.RotateFlip(RotateFlipType.RotateNoneFlipX);
                break;
            case 3:
                clone.RotateFlip(RotateFlipType.Rotate180FlipNone);
                break;
            case 4:
                clone.RotateFlip(RotateFlipType.Rotate180FlipX);
                break;
            case 5:
                clone.RotateFlip(RotateFlipType.Rotate90FlipX);
                break;
            case 6:
                clone.RotateFlip(RotateFlipType.Rotate90FlipNone);
                break;
            case 7:
                clone.RotateFlip(RotateFlipType.Rotate270FlipX);
                break;
            case 8:
                clone.RotateFlip(RotateFlipType.Rotate270FlipNone);
                break;
        }

        clone.RemovePropertyItem(orientationId);
        return clone;
    }

    private static Image ResizeIfNeeded(Image image)
    {
        var maxEdge = Math.Max(image.Width, image.Height);
        if (maxEdge <= MaxEdgePixels)
        {
            return (Image)image.Clone();
        }

        var scale = MaxEdgePixels / (double)maxEdge;
        var width = Math.Max(1, (int)Math.Round(image.Width * scale));
        var height = Math.Max(1, (int)Math.Round(image.Height * scale));
        var resized = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(resized);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.DrawImage(image, 0, 0, width, height);
        return resized;
    }

    private static byte[] EncodeJpeg(Image image)
    {
        using var stream = new MemoryStream();
        var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
        using var parameters = new EncoderParameters(1);
        parameters.Param[0] = new EncoderParameter(Encoder.Quality, JpegQuality);
        image.Save(stream, encoder, parameters);
        return stream.ToArray();
    }
}
