namespace BaoXia.Service.FileService.Extensions
{
    public static class GraphicsPathExtension
    {
        /*
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:验证平台兼容性", Justification = "<挂起>")]
        public static void AddRoundRect(
                this GraphicsPath graphicsPath,
                Rectangle rectFrame,
                int leftTopCornerRadius,
                int rightTopCornerRadius,
                int rightBottomCornerRadius,
                int leftBottomCornerRadius)
        {
                graphicsPath.StartFigure();
                {
                        var leftTopCornerFrame
                                = new Rectangle(
                                        rectFrame.Left,
                                        rectFrame.Top,
                                        leftTopCornerRadius * 2,
                                        leftTopCornerRadius * 2);
                        var rightTopCornerFrame
                                = new Rectangle(
                                        rectFrame.Right - rightTopCornerRadius * 2,
                                        rectFrame.Top,
                                        rightTopCornerRadius * 2,
                                        rightTopCornerRadius * 2);
                        var rightBottomCornerFrame
                                = new Rectangle(
                                        rectFrame.Right - rightBottomCornerRadius * 2,
                                        rectFrame.Bottom - rightBottomCornerRadius * 2,
                                        rightBottomCornerRadius * 2,
                                        rightBottomCornerRadius * 2);
                        var leftBottomCornerFrame
                                = new Rectangle(
                                        rectFrame.Left,
                                        rectFrame.Bottom - leftBottomCornerRadius * 2,
                                        leftBottomCornerRadius * 2,
                                        leftBottomCornerRadius * 2);

                        graphicsPath.AddArc(
                                leftTopCornerFrame,
                                180.0F,
                                90.0F);

                        graphicsPath.AddArc(
                                rightTopCornerFrame,
                                270.0F,
                                90.0F);

                        graphicsPath.AddArc(
                                rightBottomCornerFrame,
                                0.0F,
                                90.0F);

                        graphicsPath.AddArc(
                                leftBottomCornerFrame,
                                90.0F,
                                90.0F);
                }
                graphicsPath.CloseFigure();
        }
        public static void AddRoundRect(
                this GraphicsPath graphicsPath,
                Rectangle rectFrame,
                int cornerRadius)
        {
                GraphicsPathExtension.AddRoundRect(
                        graphicsPath,
                        rectFrame,
                        cornerRadius,
                        cornerRadius,
                        cornerRadius,
                        cornerRadius);
        }
        */
    }
}