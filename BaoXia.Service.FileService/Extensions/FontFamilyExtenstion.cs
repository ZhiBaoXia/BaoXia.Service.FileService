namespace BaoXia.Service.FileService.Extensions
{
    /*
    public static class FontFamilyExtenstion
    {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:验证平台兼容性", Justification = "<挂起>")]
            public static int FontSizeInEmFromSizeInPixel(
                    this FontFamily fontFamily,
                    int fontSizeInPixel,
                    FontStyle fontStyle = FontStyle.Regular)
            {
                    // fontSizeInPixel = fontSizeInEm * fontAscent / fontEmSize;
                    // fontEmSize = fontSizeInEm * fontAscent / fontSizeInPixel;
                    // fontSizeInEm =  fontSizeInPixel * fontEmSize  / fontAscent;

                    var fontAscent = fontFamily.GetCellAscent(fontStyle);
                    var fontEmSize = fontFamily.GetEmHeight(fontStyle);
                    var fontSizeInEm = fontSizeInPixel * fontEmSize / fontAscent;
                    { }
                    return fontSizeInEm;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:验证平台兼容性", Justification = "<挂起>")]
            public static Font FontWithFontSizeInPixel(
                    this FontFamily fontFamily,
                    int fontSizeInPixel,
                    FontStyle fontStyle = FontStyle.Regular)
            {
                    var fontSizeInEm = fontFamily.FontSizeInEmFromSizeInPixel(
                            fontSizeInPixel,
                            fontStyle);
                    var font = new Font(fontFamily, fontSizeInEm, fontStyle);
                    { }
                    return font;
            }
    }
    */
}
