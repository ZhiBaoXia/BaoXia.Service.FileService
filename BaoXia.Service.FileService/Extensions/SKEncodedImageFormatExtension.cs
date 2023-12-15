using BaoXia.Utils.Extensions;
using SkiaSharp;

namespace BaoXia.Service.FileService.Extensions
{
        public static class SKEncodedImageFormatExtension
        {
                public static SKEncodedImageFormat? FormatFromFileExtensionName(
                        string? fileExtensionName)
                {
                        if (string.IsNullOrEmpty(fileExtensionName))
                        {
                                return null;
                        }

                        if ("webp".EqualsIgnoreCase(fileExtensionName))
                        {
                                return SKEncodedImageFormat.Webp;
                        }
                        if ("jpg".EqualsIgnoreCase(fileExtensionName)
                                || "jpeg".EqualsIgnoreCase(fileExtensionName))
                        {
                                return SKEncodedImageFormat.Jpeg;
                        }
                        if ("gif".EqualsIgnoreCase(fileExtensionName))
                        {
                                return SKEncodedImageFormat.Gif;
                        }
                        if ("png".EqualsIgnoreCase(fileExtensionName))
                        {
                                return SKEncodedImageFormat.Png;
                        }

                        return null;
                }

                public static string? FileExtensionNameFromFormat(
                        SKEncodedImageFormat? format)
                {
                        if (format == null)
                        {
                                return null;
                        }

                        switch (format.Value)
                        {
                                case SKEncodedImageFormat.Bmp:
                                        {
                                                return "bmp";
                                        }
                                case SKEncodedImageFormat.Gif:
                                        {
                                                return "gif";
                                        }
                                case SKEncodedImageFormat.Ico:
                                        {
                                                return "ico";
                                        }
                                case SKEncodedImageFormat.Jpeg:
                                        {
                                                return "jpg";
                                        }
                                case SKEncodedImageFormat.Png:
                                        {
                                                return "png";
                                        }
                                case SKEncodedImageFormat.Wbmp:
                                        {
                                                return "wbmp";
                                        }
                                case SKEncodedImageFormat.Webp:
                                        {
                                                return "webp";
                                        }
                                case SKEncodedImageFormat.Pkm:
                                        {
                                                return "pkm";
                                        }
                                case SKEncodedImageFormat.Ktx:
                                        {
                                                return "ktx";
                                        }
                                case SKEncodedImageFormat.Astc:
                                        {
                                                return "astc";
                                        }
                                case SKEncodedImageFormat.Dng:
                                        {
                                                return "dng";
                                        }
                                case SKEncodedImageFormat.Heif:
                                        {
                                                return "heif";
                                        }
                        }
                        return null;
                }

                public static string? ToFileExtensionName(this SKEncodedImageFormat format)
                {
                        return SKEncodedImageFormatExtension.FileExtensionNameFromFormat(format);
                }
        }
}
