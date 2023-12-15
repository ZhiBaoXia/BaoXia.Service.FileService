namespace BaoXia.Service.FileService.Utils
{
    /* 备份了GDIPlus的代码。
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:验证平台兼容性", Justification = "<挂起>")]
    public class ImageCard
    {
            ////////////////////////////////////////////////
            // @静态常量
            ////////////////////////////////////////////////

            #region 自身属性

            protected string _resourceImagesDirectoryPath;

            protected int _canvasWidth;
            // 2779 = iPhone 13 Pro Max 的屏幕高度。
            protected int _canvasHeightMax;
            protected Padding _canvasPadding;

            protected float _canvasDPIZoomRatio;
            protected float _canvasResolutionX;
            protected float _canvasResolutionY;

            protected int _canvasWidthInPixel;
            protected int _canvasHeightInPixel;

            protected float _textRenderOffsetY;


            ////////////////////////////////////////////////


            protected readonly HttpClient _httpClient;

            protected Utils.ObjectPool<Bitmap> _canvasBitmaps;

            protected BaoXia.Utils.Cache.AsyncItemsCache<string, Image?, object> _imagesWithUrlKey;

            protected BaoXia.Utils.Cache.ItemsCache<string, FontFamily?, object> _fontFamiliesWithFontFileName;
            protected FontFamily _fontFamily_SourceHanSansSC_Regular;
            protected FontFamily _fontFamily_SourceHanSansSC_Midium;
            protected FontFamily _fontFamily_SourceHanSansSC_Bold;

            #endregion


            ////////////////////////////////////////////////
            // @自身实现
            ////////////////////////////////////////////////

            #region 自身实现

            public ImageCard(
                    string resourceImagesDirectoryPath,
                    //
                    int canvasWidth = 375,
                    // 2779 = iPhone 13 Pro Max 的屏幕高度。
                    int canvasHeightMax = 2778 * 3,
                    //
                    float canvasDPIDefault = 96.0F,
                    float canvasDPIZoomRatio = 3.0F,
                    //
                    int canvasPaddingLeft = 16,
                    int canvasPaddingTop = 0,
                    int canvasPaddingRight = 16,
                    int canvasPaddingBottom = 0,
                    //
                    float textRenderOffsetY = 1.5F)
            {
                    _resourceImagesDirectoryPath = resourceImagesDirectoryPath;

                    _canvasWidth = canvasWidth;
                    // 2779 = iPhone 13 Pro Max 的屏幕高度。
                    _canvasHeightMax = canvasHeightMax;
                    _canvasWidthInPixel = (int)(_canvasWidth * canvasDPIZoomRatio);
                    _canvasHeightInPixel = (int)(_canvasHeightMax * canvasDPIZoomRatio);

                    _canvasDPIZoomRatio = canvasDPIZoomRatio;

                    _canvasResolutionX = (int)(canvasDPIDefault * canvasDPIZoomRatio);
                    _canvasResolutionY = _canvasResolutionX;

                    _canvasPadding = new(
                            canvasPaddingLeft,
                            canvasPaddingTop,
                            canvasPaddingRight,
                            canvasPaddingBottom);

                    _textRenderOffsetY = textRenderOffsetY;


                    ////////////////////////////////////////////////

                    _httpClient = new();

                    _canvasBitmaps
                            = new(() =>
                            {
                                    var canvasBitmap = new Bitmap(_canvasWidthInPixel, _canvasHeightInPixel);
                                    {
                                            canvasBitmap.SetResolution(
                                                    _canvasResolutionX,
                                                    _canvasResolutionY);
                                    }
                                    return canvasBitmap;
                            });

                    _imagesWithUrlKey = new(
                    async (imageUrl, _) =>
                    {
                            var image = await this.ImageDownloadFromAsync(imageUrl);
                            {}
                            return image;
                    },
                    null,
                    () => 0.0,
                    () => 0.0);


                    _fontFamiliesWithFontFileName = new(
                            (fontFileName, _) =>
                            {
                                    var fontFamily = this.FontFamilyFileNamed(fontFileName);
                                    {
                                    }
                                    return fontFamily;
                            },
                            null,
                            null,
                            null);
                    _fontFamily_SourceHanSansSC_Regular
                    = _fontFamiliesWithFontFileName.Get("PingFang Regular.ttf", new object())!;
                    _fontFamily_SourceHanSansSC_Midium
                            = _fontFamiliesWithFontFileName.Get("PingFang Medium.ttf", new object())!;
                    _fontFamily_SourceHanSansSC_Bold
                            = _fontFamiliesWithFontFileName.Get("PingFang Bold.ttf", new object())!;


                    ////////////////////////////////////////////////

            }

            protected Image ImageFileNamed(string imageFileName)
            {
                    var imageFilePath
                            = BaoXia.Utils.Environment.ApplicationDirectoryPath
                            + _resourceImagesDirectoryPath
                            + imageFileName;
                    var image
                            = Image.FromFile(imageFilePath);
                    { }
                    return image;
            }

            protected async Task<Image?> ImageDownloadFromAsync(string imageUrl)
            {
                    if (imageUrl == null
                            || imageUrl.Length < 1)
                    {
                            return null;
                    }

                    var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
                    if (imageBytes == null
                            || imageBytes.Length < 1)
                    {
                            return null;
                    }

                    using var imageBytesStream = new MemoryStream(imageBytes);
                    var image = Image.FromStream(imageBytesStream);
                    { }
                    imageBytesStream.Close();
                    return image;
            }

            protected Color ColorFromArgbHex(uint hex)
            {
                    unchecked
                    {
                            return Color.FromArgb((int)hex);
                    }
            }

            protected FontFamily FontFamilyFileNamed(string fontFileName)
            {
                    var fontFilePath
                            = BaoXia.Utils.Environment.ApplicationDirectoryPath
                            + "Resources/Fonts/"
                            + fontFileName;

                    var fontCollection = new PrivateFontCollection();
                    fontCollection.AddFontFile(fontFilePath);
                    //检测字体类型是否可用
                    //var regularFont = fontCollection.Families[0].IsStyleAvailable(FontStyle.Regular);
                    //var boldFont = fontCollection.Families[0].IsStyleAvailable(FontStyle.Bold);
                    //定义成新的字体对象
                    FontFamily fontFamily = new FontFamily(fontCollection.Families[0].Name, fontCollection);
                    {
                    }
                    return fontFamily;
            }

            public async Task<Image?> Create(
                    ImageFormat imageFormat,
                    Func<Graphics,
                    //
                    int,
                    int,
                    //
                    Padding,
                    //
                    FontFamily,
                    FontFamily,
                    FontFamily,
                    //
                    float,
                    //
                    HttpClient,
                    Func<string, Image>,
                    Func<string, Task<Image?>>,
                    Func<uint, Color>,
                    Func<string, FontFamily>,
                    Task<int>> toRenderCard)
            {
                    using var canvasBitmapContainer = _canvasBitmaps.GetObjectContainer()!;
                    var canvasBitmap = canvasBitmapContainer.Item;
                    using var graphics = Graphics.FromImage(canvasBitmap);
                    {
                            graphics.SmoothingMode = SmoothingMode.AntiAlias;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.CompositingQuality = CompositingQuality.HighQuality;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                            graphics.PageScale = _canvasDPIZoomRatio;
                            graphics.PageUnit = GraphicsUnit.Pixel;
                    }

                    var cardFinalImageHeight
                            = toRenderCard != null
                            ? await toRenderCard(
                                    graphics,
                                    //
                                    _canvasWidth,
                                    _canvasHeightMax,
                                    //
                                    _canvasPadding,
                                    //
                                    _fontFamily_SourceHanSansSC_Regular,
                                    _fontFamily_SourceHanSansSC_Midium,
                                    _fontFamily_SourceHanSansSC_Bold,
                                    //
                                    _textRenderOffsetY,
                                     //
                                     _httpClient,
                                     this.ImageFileNamed,
                                     this.ImageDownloadFromAsync,
                                     this.ColorFromArgbHex,
                                     this.FontFamilyFileNamed)
                            : 0;
                    if (cardFinalImageHeight <= 0)
                    {
                            return null;
                    }

                    var cardImageFrame = new Rectangle(
                            0, 
                            0, 
                            canvasBitmap.Width,
                            (int)(cardFinalImageHeight * _canvasDPIZoomRatio));
                    var cardImage = new Bitmap(cardImageFrame.Width, cardImageFrame.Height);
                    {
                            //cardImage.SetResolution(
                            //        canvasBitmap.HorizontalResolution,
                            //        canvasBitmap.VerticalResolution);
                    }
                    using var cardImageClipper = Graphics.FromImage(cardImage);
                    {
                            cardImageClipper.Clear(Color.Transparent);
                            cardImageClipper.DrawImage(
                                    canvasBitmap,
                                    cardImageFrame,
                                    cardImageFrame,
                                    GraphicsUnit.Pixel);

                    }
                    return cardImage;
            }

            #endregion


            ////////////////////////////////////////////////
            // @事件节点
            ////////////////////////////////////////////////

            #region 事件节点

            //protected virtual int DidRenderCardWithGraphics(
            //        Graphics graphics,
            //        //
            //        int canvasWidth,
            //        int canvasHeightMax,
            //        //
            //        Padding canvasPadding,
            //        //
            //        FontFamily fontFamily_SourceHanSansSC_Regular,
            //        FontFamily fontFamily_SourceHanSansSC_Midium,
            //        FontFamily fontFamily_SourceHanSansSC_Bold,
            //        //
            //        float textRenderOffsetY)


            #endregion
    }
    */
}
