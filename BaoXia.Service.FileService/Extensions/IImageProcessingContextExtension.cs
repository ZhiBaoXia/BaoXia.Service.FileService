using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace BaoXia.Service.FileService.Extensions
{
        public static class IImageProcessingContextExtension
        {
                public static IImageProcessingContext CropToFitSize(
                        this IImageProcessingContext imageProcessingContext,
                        int objectWidth,
                        int objectHeight,
                        SixLabors.Fonts.HorizontalAlignment cropHorizontalAlignment = HorizontalAlignment.Center,
                        SixLabors.Fonts.VerticalAlignment cropVerticalAlignment = VerticalAlignment.Center)
                {
                        var currentImageSize = imageProcessingContext.GetCurrentSize();
                        if (currentImageSize.Width <= 0
                                || currentImageSize.Height <= 0)
                        {
                                return imageProcessingContext;
                        }

                        var widthZoomRatio = objectWidth / currentImageSize.Width;
                        var heightZoomRatio = objectHeight / currentImageSize.Height;
                        var finalZoomRatio
                                        = widthZoomRatio > heightZoomRatio
                                        ? widthZoomRatio
                                        : heightZoomRatio;

                        var finalWidth = currentImageSize.Width * finalZoomRatio;
                        var finalHeight = currentImageSize.Height * finalZoomRatio;
                        int finalLeft;
                        switch (cropHorizontalAlignment)
                        {
                                case HorizontalAlignment.Left:
                                        {
                                                finalLeft = 0;
                                        }
                                        break;
                                case HorizontalAlignment.Right:
                                        {
                                                finalLeft = currentImageSize.Width - finalWidth;
                                        }
                                        break;
                                default:
                                case HorizontalAlignment.Center:
                                        {
                                                finalLeft = (currentImageSize.Width - finalWidth) / 2;
                                        }
                                        break;
                        }
                        int finalTop;
                        switch (cropVerticalAlignment)
                        {
                                case VerticalAlignment.Top:
                                        {
                                                finalTop = 0;
                                        }
                                        break;
                                case VerticalAlignment.Bottom:
                                        {
                                                finalTop = currentImageSize.Height - finalHeight;
                                        }
                                        break;
                                default:
                                case VerticalAlignment.Center:
                                        {
                                                finalTop = (currentImageSize.Height - finalHeight) / 2;
                                        }
                                        break;
                        }


                        imageProcessingContext
                                = imageProcessingContext.Crop(
                                new Rectangle(
                                        finalLeft,
                                        finalTop,
                                        finalWidth,
                                        finalHeight));
                        { }
                        return imageProcessingContext;
                }
        }
}
