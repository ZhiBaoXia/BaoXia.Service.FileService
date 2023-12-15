using SkiaSharp;

namespace BaoXia.Service.FileService.Extensions
{
    public static class SKBitmapExtension
    {
        public static SKImage ImageByScaleToSize(
            this SKBitmap sourceImage,
            SKSizeI objectImageSize,
            bool isTransparentCanvas,
            SKTextAlign sourceAreaHorizationAlign = SKTextAlign.Center,
            SKTextAlign sourceAreaVerticalAlign = SKTextAlign.Center)
        {
            var objectImage = new SKBitmap(
                objectImageSize.Width,
                objectImageSize.Height);

            var canvasInfo = new SKImageInfo(
                width: objectImageSize.Width,
                height: objectImageSize.Height,
                colorType: SKColorType.Rgba8888,
                alphaType: SKAlphaType.Premul);
            using var skCanvasSurface
            = SKSurface.Create(canvasInfo);
            var skCanvas = skCanvasSurface.Canvas;

            var objectImageWidth = objectImageSize.Width;
            var objectImageHeight = objectImageSize.Height;
            var sourceImageWidth = sourceImage.Width;
            var sourceImageHeight = sourceImage.Height;

            var objectImageWidthZoomRatio
            = (float)objectImageWidth
            / (float)sourceImageWidth;
            var objectImageHeightZoomRatio
            = (float)objectImageHeight
            / (float)sourceImageHeight;
            var objectImageZoomRatio
            = objectImageWidthZoomRatio > objectImageHeightZoomRatio
            ? objectImageWidthZoomRatio
            : objectImageHeightZoomRatio;

            // !!! 绘制图片 !!!
            int imageSourceAreaWidth
            = (int)(objectImageSize.Width / objectImageZoomRatio);
            int imageSourceAreaHeight
            = (int)(objectImageSize.Height / objectImageZoomRatio);

            int imageSourceAreaLeft;
            if (sourceAreaHorizationAlign == SKTextAlign.Left)
            {
                imageSourceAreaLeft = 0;
            }
            else if (sourceAreaHorizationAlign == SKTextAlign.Right)
            {
                imageSourceAreaLeft = sourceImageWidth - imageSourceAreaWidth;
            }
            else
            {
                imageSourceAreaLeft
               = (sourceImageWidth - imageSourceAreaWidth)
               / 2;
            }

            int imageSourceAreaTop;
            if (sourceAreaVerticalAlign == SKTextAlign.Left)
            {
                imageSourceAreaTop = 0;
            }
            else if (sourceAreaVerticalAlign == SKTextAlign.Right)
            {
                imageSourceAreaTop = sourceImageHeight - imageSourceAreaHeight;
            }
            else
            {
                imageSourceAreaTop
               = (sourceImageHeight - imageSourceAreaHeight)
               / 2;
            }

            var imageSourceArea = SKRect.Create(
                imageSourceAreaLeft,
                imageSourceAreaTop,
                imageSourceAreaWidth,
                imageSourceAreaHeight);
            var imageObjectArea = SKRect.Create(
                0,
                0,
                objectImageSize.Width,
                objectImageSize.Height);
            {
                if (isTransparentCanvas != true)
                {
                    skCanvas.Clear(SKColors.White);
                }

                skCanvas.DrawBitmap(
                    sourceImage,
                    imageSourceArea,
                    imageObjectArea,
                    new SKPaint()
                    {
                        FilterQuality = SKFilterQuality.High,
                        IsAntialias = true
                    });
            }
            var finalObjectImage = skCanvasSurface.Snapshot();
            { }
            return finalObjectImage;
        }
    }
}

