namespace BaoXia.Service.FileService.Utils
{
        public class Padding
        {
                public int Left { get; set; }
                public int Right { get; set; }

                public int Width
                {
                        get
                        {
                                return this.Left + this.Right;
                        }
                }

                public int Top { get; set; }
                public int Bottom { get; set; }
                public int Height
                {
                        get
                        {
                                return this.Top + this.Bottom;
                        }
                }

                public Padding(int left, int top, int right, int bottom)
                {
                        this.Left = left;
                        this.Right = right;

                        this.Top = top;
                        this.Bottom = bottom;
                }
        }
}
