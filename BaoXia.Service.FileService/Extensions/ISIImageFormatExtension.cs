using BaoXia.Utils.Extensions;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using System.Diagnostics.CodeAnalysis;

namespace BaoXia.Service.FileService.Extensions;

public class ISIImageFormatExtension
{
        public static bool TryGetImageFormatWithFileExtensionName(
                string? fileExtensionName,
                [NotNullWhen(true)]
                out IImageFormat? imageFormat,
                [NotNullWhen(true)]
                out IImageEncoder? imageEncoder,
                [NotNullWhen(true)]
                out IImageDecoder? imageDecoder)
        {
                // !!!
                imageFormat = null;
                imageEncoder = null;
                imageDecoder = null;
                // !!!

                if (string.IsNullOrEmpty(fileExtensionName))
                {
                        return false;
                }
                if ("webp".EqualsIgnoreCase(fileExtensionName))
                {
                        imageFormat = WebpFormat.Instance;
                        imageEncoder = new WebpEncoder();
                        imageDecoder = new WebpDecoder();
                        return true;
                }
                if ("jpg".EqualsIgnoreCase(fileExtensionName)
                        || "jpeg".EqualsIgnoreCase(fileExtensionName))
                {
                        imageFormat = JpegFormat.Instance;
                        imageEncoder = new JpegEncoder();
                        imageDecoder = new JpegDecoder();
                        return true;
                }
                if ("gif".EqualsIgnoreCase(fileExtensionName))
                {
                        imageFormat = GifFormat.Instance;
                        imageEncoder = new GifEncoder();
                        imageDecoder = new GifDecoder();
                        return true;
                }
                if ("png".EqualsIgnoreCase(fileExtensionName))
                {
                        imageFormat = PngFormat.Instance;
                        imageEncoder = new PngEncoder();
                        imageDecoder = new PngDecoder();
                        return true;
                }
                return false;
        }

        public static IImageFormat? GetImageFormatWithFileExtensionName(
            string? fileExtensionName)
        {
                if (ISIImageFormatExtension.TryGetImageFormatWithFileExtensionName(
                        fileExtensionName,
                        out var imageFormat,
                        out _,
                        out _) == true)
                {
                        return imageFormat;
                }
                return null;
        }

        public static IImageEncoder? GetImageEncoderWithFileExtensionName(
            string? fileExtensionName)
        {
                if (ISIImageFormatExtension.TryGetImageFormatWithFileExtensionName(
                        fileExtensionName,
                        out _,
                        out var imageEncoder,
                        out _) == true)
                {
                        return imageEncoder;
                }
                return null;
        }

        public static IImageDecoder? GetImageDecoderWithFileExtensionName(
            string fileExtensionName)
        {
                if (ISIImageFormatExtension.TryGetImageFormatWithFileExtensionName(
                        fileExtensionName,
                        out _,
                        out _,
                        out var imageDecoder) == true)
                {
                        return imageDecoder;
                }
                return null;
        }
}

