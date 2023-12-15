using System.Drawing;

namespace BaoXia.Service.FileService.Extensions
{
        public static class RectangleExtension
        {
                public static int MidX(this Rectangle rectangle)
                {
                        return rectangle.X + rectangle.Width / 2;
                }

                public static int MidY(this Rectangle rectangle)
                {
                        return rectangle.Y + rectangle.Height / 2;
                }
        }
}
