using System.Drawing;

namespace BaoXia.Service.FileService.Extensions
{
        public static class SizeExtension
        {
                public static SizeF ToSizeF(this Size size)
                {
                        return new SizeF(
                                size.Width,
                                size.Height);
                }
        }
}
