using SixLabors.ImageSharp;
using SkiaSharp;

namespace BaoXia.Service.FileService.ViewModels
{
    public class ImageArea
    {
        public int Left { get; set; }

        public int Top { get; set; }

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

        public SKRectI ToSKRectI()
        {
            return new SKRectI(
                this.Left,
                this.Top,
                this.Left + this.Width,
                this.Top + this.Height);
        }

        public Rectangle ToISRectangle()
        {
            return new Rectangle(
                this.Left,
                this.Top,
                this.Width,
                this.Height);
        }
    }
}
