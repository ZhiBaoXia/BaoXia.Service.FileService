using BaoXia.Utils.Extensions;
using SkiaSharp;

namespace BaoXia.Service.FileService.Extensions
{
        public static class SKCanvasExtension
        {
                public static SKRect Bounds(this SKCanvas canvas)
                {
                        canvas.GetDeviceClipBounds(out var boundsI);
                        var bounds = new SKRect(
                            0,
                            0,
                            boundsI.Width,
                            boundsI.Height);
                        { }
                        return bounds;
                }

                public static SKRectI BoundsI(this SKCanvas canvas)
                {
                        canvas.GetDeviceClipBounds(out var boundsI);
                        var bounds = new SKRectI(
                            0,
                            0,
                            boundsI.Width,
                            boundsI.Height);
                        { }
                        return bounds;
                }

                public static void DrawTextToLeftTop(
                    this SKCanvas canvas,
                    string? text,
                    float left,
                    float top,
                    SKFont? skFont,
                    SKPaint skPaint)
                {
                        if (string.IsNullOrEmpty(text))
                        {
                                return;
                        }

                        var textDrawX = left;
                        var textDrawY = top;

                        if (skFont != null)
                        {
                                // !!!
                                textDrawY += skFont.Metrics.CapHeight;
                                // !!!
                                canvas.DrawText(
                                    text,
                                    textDrawX,
                                    textDrawY,
                                    skFont,
                                    skPaint);
                        }
                        else
                        {
                                // !!!
                                textDrawY += skPaint.FontMetrics.CapHeight;
                                // !!!
                                canvas.DrawText(
                                    text,
                                    textDrawX,
                                    textDrawY,
                                    skPaint);
                        }
                }

                public static void DrawTextToLeftTop(
                    this SKCanvas canvas,
                    string? text,
                    SKPoint point,
                    SKFont? skFont,
                    SKPaint skPaint)
                {
                        SKCanvasExtension.DrawTextToLeftTop(
                                canvas,
                                text,
                                point.X,
                                point.Y,
                                skFont,
                                skPaint);
                }

                public static void DrawTextToLeftTop(
                    this SKCanvas canvas,
                    string? text,
                    float left,
                    float top,
                    SKPaint skPaint)
                {
                        SKCanvasExtension.DrawTextToLeftTop(
                                canvas,
                                text,
                                left,
                                top,
                                null,
                                skPaint);
                }

                public static void DrawTextToLeftTop(
                    this SKCanvas canvas,
                    string? text,
                    SKPoint point,
                    SKPaint skPaint)
                {
                        SKCanvasExtension.DrawTextToLeftTop(
                                canvas,
                                text,
                                point.X,
                                point.Y,
                                skPaint);
                }

                public static SKRect DrawTextToLeftTop(
                    this SKCanvas canvas,
                    string? text,
                    float left,
                    float top,
                    SKPaint skPaintDefault,
                    SKTypeface font,
                    float fontSize,
                    uint textColorARGBHex)
                {
                        var textLocation = new SKPoint(left, top);
                        var skPaint = skPaintDefault.Clone();
                        {
                                skPaint.Typeface = font;
                                skPaint.TextSize = fontSize;
                                skPaint.Color = new SKColor(textColorARGBHex);
                        }
                        var textBounds = new SKRect();
                        skPaint.MeasureText(text, ref textBounds);
                        // !!!!
                        canvas.DrawTextToLeftTop(
                                text,
                                textLocation,
                                skPaint);
                        // !!!
                        return SKRect.Create(
                                textLocation.X,
                                textLocation.Y,
                                textBounds.Width,
                                textBounds.Height);
                }

                public static SKRect DrawTextInRect(
                        this SKCanvas canvas,
                        string? text,
                        SKRect rect,
                        float lineHeight,
                        SKTextAlign textHorizontalAlign,
                        SKTextAlign textVerticalAlign,
                        SKPaint skPaint)
                {
                        var textBounds = SKRect.Create(
                                        rect.Left,
                                        rect.Top,
                                        rect.Width,
                                        0);
                        if (string.IsNullOrEmpty(text))
                        {
                                return textBounds;
                        }

                        var lineLeft = textBounds.Left;
                        var lineTop = textBounds.Top;
                        var lineBottom = lineTop;
                        var lineWidth = textBounds.Width;
                        var lineTextHeight = skPaint.FontMetrics.XHeight;
                        if (lineHeight < 0)
                        {
                                lineHeight = lineTextHeight + skPaint.FontSpacing;
                        }

                        var textWillDraw = text;
                        while (textWillDraw?.Length > 0)
                        {
                                var lineTextLength = skPaint.BreakText(
                                        textWillDraw,
                                        lineWidth,
                                        out var lineTextWidth,
                                        out var lineText);
                                var lineTextLeft = lineLeft;
                                if (lineTextLength > 0)
                                {
                                        textWillDraw = textWillDraw.Right(textWillDraw.Length - (int)lineTextLength);
                                }
                                else
                                {
                                        textWillDraw = null;
                                }

                                if (textHorizontalAlign == SKTextAlign.Center)
                                {
                                        lineTextLeft = lineLeft + (lineWidth - lineTextWidth) / 2.0F;
                                }
                                else if (textHorizontalAlign == SKTextAlign.Right)
                                {
                                        lineTextLeft = lineLeft + lineWidth - lineTextWidth;
                                }
                                var lineTextTop = lineTop;
                                if (textVerticalAlign == SKTextAlign.Center)
                                {
                                        lineTextTop = lineTop + (lineHeight - lineTextHeight) / 2.0F;
                                }
                                else if (textVerticalAlign == SKTextAlign.Right)
                                {
                                        lineTextTop = lineTop + lineHeight - lineTextHeight;
                                }
                                canvas.DrawTextToLeftTop(
                                        lineText,
                                        lineTextLeft,
                                        lineTextTop,
                                        skPaint);
                                // !!!
                                lineBottom = lineTop + lineHeight;
                                lineTop = lineBottom;
                                // !!!
                        }
                        // !!!
                        textBounds.Bottom = lineBottom;
                        // !!!

                        return textBounds;
                }
        }
}

