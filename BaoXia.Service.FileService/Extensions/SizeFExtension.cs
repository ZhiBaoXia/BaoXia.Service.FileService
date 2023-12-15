using System.Drawing;

namespace BaoXia.Service.FileService.Extensions
{
        public static class SizeFExtension
        {
                public static Size ToSize(this SizeF size)
                {
                        return new Size(
                                (int)size.Width,
                                (int)size.Height);
                }
        }
}
