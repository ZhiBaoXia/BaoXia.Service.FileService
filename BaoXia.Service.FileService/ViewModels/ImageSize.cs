using SixLabors.ImageSharp;
using SkiaSharp;

namespace BaoXia.Service.FileService.ViewModels
{
        public class ImageSize
        {
                private double v;

                ////////////////////////////////////////////////
                // @自身属性
                ////////////////////////////////////////////////

                #region 自身属性
                public int Width { get; set; }
                public int Height { get; set; }

                public bool IsValid
                {
                        get
                        {
                                if (this.Width > 0
                                        && this.Height > 0)
                                {
                                        return true;
                                }
                                return false;
                        }
                }

                #endregion


                ////////////////////////////////////////////////
                // @自身实现
                ////////////////////////////////////////////////

                #region 自身实现

                public ImageSize()
                { }

                public ImageSize(
                        int width,
                        int height)
                {
                        this.Width = width;
                        this.Height = height;
                }

                public ImageSize(double v, int height)
                {
                        this.v = v;
                        Height = height;
                }

                public ImageSize SizeByFitTo(ImageSize containerSize)
                {
                        var widthZoomRatio
                                = this.Width > 0
                                ? (double)containerSize.Width / (double)this.Width
                                : 0.0;
                        var heightZoomRatio
                                = this.Height > 0
                                ? (double)containerSize.Height / (double)this.Height
                                : 0.0;
                        var zoomRatio
                                = widthZoomRatio < heightZoomRatio
                                ? widthZoomRatio
                                : heightZoomRatio;
                        { }
                        return new ImageSize(
                                (int)Math.Ceiling(this.Width * zoomRatio),
                                (int)Math.Ceiling(this.Height * zoomRatio));
                }

                public ImageSize SizeByZoomWithMaxSize(ImageSize maxSize)
                {
                        var widthZoomRatio
                                = (this.Width > 0
                                && maxSize.Width > 0
                                && this.Width > maxSize.Width)
                                ? (double)maxSize.Width / (double)this.Width
                                : 1.0;
                        var heightZoomRatio
                                = (this.Height > 0
                                && maxSize.Height > 0
                                && this.Height > maxSize.Height)
                                ? (double)maxSize.Height / this.Height
                                : 1.0;
                        var zoomRatio
                                = widthZoomRatio < heightZoomRatio
                                ? widthZoomRatio
                                : heightZoomRatio;
                        { }
                        return new ImageSize(
                                (int)Math.Ceiling(this.Width * zoomRatio),
                                (int)Math.Ceiling(this.Height * zoomRatio));
                }

                public SKSizeI ToSKSizeI()
                {
                        return new SKSizeI(
                            this.Width,
                            this.Height);
                }
                public Size ToISSize()
                {
                        return new Size(
                            this.Width,
                            this.Height);
                }

                #endregion
        }
}
