using BaoXia.Constants;
using BaoXia.Utils.Extensions;
using BaoXia.Service.FileService.ConfigFiles;
using BaoXia.Service.FileService.Extensions;
using BaoXia.Service.FileService.LogFiles;
using BaoXia.Service.FileService.Models;
using BaoXia.Service.FileService.Utils;
using BaoXia.Service.FileService.ViewModels;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics.CodeAnalysis;
using BaoXia.Utils.ConcurrentTools;
using BaoXia.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static System.Net.Mime.MediaTypeNames;
using Image = SixLabors.ImageSharp.Image;
using BaoXia.Utils.Cache;
using BaoXia.Service.FileService.Constants;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using StringExtension = BaoXia.Utils.Extensions.StringExtension;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using BaoXia.Utils.MathTools;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing.Processors.Filters;
using BaoXia.Constants.Models;
using BaoXia.Service.FileService.Attributes;

namespace BaoXia.Service.FileService.Controllers
{
        public class UploadController : Controller
        {

                ////////////////////////////////////////////////
                // @静态常量
                ////////////////////////////////////////////////

                #region 静态常量

                public class FontFamilyInfo
                {
                        public string FontFilePath { get; set; }

                        public SixLabors.Fonts.FontFamily FontFamily { get; set; }

                        public FontFamilyInfo(
                                string fontFilePath,
                                SixLabors.Fonts.FontFamily fontFamily)
                        {
                                this.FontFilePath = fontFilePath;
                                this.FontFamily = fontFamily;
                        }
                }

                #endregion


                ////////////////////////////////////////////////
                // @静态变量
                ////////////////////////////////////////////////

                #region 静态变量

                protected static readonly HttpClient _httpClient = new HttpClient();

                private static readonly List<Models.FileInfo> _fileInfoStorageQueue = new();

                private static readonly LoopTask _taskToStorageFileInfo = new(
                        async cancellationToken =>
                        {
                                Models.FileInfo[]? fileInfesNeedStorage = null;
                                lock (_fileInfoStorageQueue)
                                {
                                        if (_fileInfoStorageQueue.Count < 1)
                                        {
                                                return false;
                                        }
                                        fileInfesNeedStorage = _fileInfoStorageQueue.ToArray();
                                        _fileInfoStorageQueue.Clear();
                                }
                                if (fileInfesNeedStorage == null
                                || fileInfesNeedStorage.Length < 1)
                                {
                                        return false;
                                }

                                using var scope = BaoXia.Utils.Environment.ApplicationBuilder?.ApplicationServices.CreateScope();
                                var db = scope?.ServiceProvider.GetRequiredService<Data.FileInfoDbContext>();
                                if (db == null)
                                {
                                        Log.Warning.Logs(
                                                null,
                                                "无法在“持久化文件信息”的任务中，获取数据库上下文。",
                                                null,
                                                "UploadController");
                                        //
                                        return false;
                                }

                                foreach (var fileInfo in fileInfesNeedStorage)
                                {
                                        try
                                        {
                                                var fileInfoEntity = db.Entry(fileInfo);
                                                // !!!
                                                fileInfoEntity
                                                .Property(fileInfoModel => fileInfoModel.SecondsToDatabaseOperation)
                                                .IsModified = true;
                                                // !!!
                                                {
                                                        fileInfo.StorageTime = DateTime.Now;
                                                }
                                        }
                                        catch (Exception exception)
                                        {
                                                Log.Exception.Logs(
                                                        null,
                                                        "讲文件信息加入数据库上下文失败，程序异常。",
                                                        exception,
                                                        "UploadController");
                                        }
                                }
                                await db.SaveChangesAsync();

                                return true;
                        },
                        () =>
                        {
                                if (Config.Service.FileInfoStorageIntervalSeconds > 0)
                                {
                                        return Config.Service.FileInfoStorageIntervalSeconds;
                                }
                                return ServiceConfig.FileInfoStorageIntervalSecondsDefault;
                        });

                private static readonly AsyncItemsCache<string, Image, object> _imageCache = new(
                        async (watermarkImageAbsoluteFilePath, _) =>
                        {
                                if (string.IsNullOrEmpty(watermarkImageAbsoluteFilePath))
                                {
                                        return null;
                                }

                                var image
                                = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(
                                        watermarkImageAbsoluteFilePath);
                                { }
                                return image;
                        },
                        null,
                        null);


                private static readonly SixLabors.Fonts.FontCollection _fontCollection = new();
                private static readonly ItemsCache<string, FontFamilyInfo, object> _fontCache = new(
                         (fontFileName, _) =>
                        {
                                if (string.IsNullOrEmpty(fontFileName))
                                {
                                        return null;
                                }

                                var fontFilePath = fontFileName.ToAbsoluteFilePathInRootPath(
                                        FilePaths.FontsDictionaryPath);
                                var fontFamily = _fontCollection.Add(fontFilePath);
                                if (string.IsNullOrEmpty(fontFamily.Name))
                                {
                                        return null;
                                }

                                var fontFamilyInfo = new FontFamilyInfo(
                                        fontFilePath,
                                        fontFamily);
                                { }
                                return fontFamilyInfo; ;
                        },
                        null,
                        null);

                private static readonly AsyncItemsCache<string, string, ImageFilterRequest> _imageUriAfterImageFilter_AiDaoRi = new(
                        async (imageUri, ImageFilterRequest) =>
                        {
                                try
                                {
                                        var imageFileExtensionName = imageUri.ToFileExtensionName();
                                        var imageAfterFilterFileInfo = Config.Service.ImageAfterFilterFileInfo;
                                        var imageFilePath = imageAfterFilterFileInfo?.GetFileAbsolutePathWithFileId(
                                                imageUri,
                                                imageFileExtensionName,
                                                ImageFilterNames.AiDaoRi);
                                        if (string.IsNullOrEmpty(imageFilePath))
                                        {
                                                return null;
                                        }

                                        // !!!
                                        var imageUriAfterImageFilter
                                        = imageAfterFilterFileInfo?.GetFileAbsoluteUriWithFileId(
                                                imageUri,
                                                imageFileExtensionName,
                                                ImageFilterNames.AiDaoRi);
                                        // !!!
                                        if (System.IO.File.Exists(imageFilePath))
                                        {
                                                return imageUriAfterImageFilter;
                                        }


                                        var imageEncoder
                                        = ISIImageFormatExtension.GetImageEncoderWithFileExtensionName(
                                                imageFileExtensionName);
                                        if (imageEncoder == null)
                                        {
                                                return null;
                                        }
                                        var imageSaveQuality = 100;
                                        if (imageEncoder
                                        is SixLabors.ImageSharp.Formats.Webp.WebpEncoder)
                                        {
                                                imageSaveQuality
                                                = imageAfterFilterFileInfo?.ImageSaveQualityForWebpDefault ?? 100;
                                        }
                                        else if (imageEncoder
                                        is SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder)
                                        {
                                                imageSaveQuality
                                                = imageAfterFilterFileInfo?.ImageSaveQualityForJpegDefault ?? 100;
                                        }
                                        if (imageSaveQuality <= 0)
                                        {
                                                imageSaveQuality = 100;
                                        }


                                        var imageBytes = await _httpClient.DownloadBytesAsync(imageUri);
                                        var image = SixLabors.ImageSharp.Image.Load<Rgba32>(imageBytes);
                                        image.Mutate(imageProcessingContext =>
                                        {
                                                var blackWhiteProcessor = new GrayscaleBt709Processor(1);
                                                //
                                                imageProcessingContext.ApplyProcessor(blackWhiteProcessor);
                                                //
                                        });


                                        var imageDirectoryPath
                                        = imageFilePath.ToFileSystemDirectoryPath(true);
                                        {
                                                Directory.CreateDirectory(imageDirectoryPath);
                                        }

                                        SaveImageToFilePath(
                                                imageFilePath,
                                                image,
                                                imageEncoder,
                                                imageSaveQuality);

                                        return imageUriAfterImageFilter;
                                }
                                catch
                                { }
                                return null;
                        },
                        null,
                        null);

                #endregion


                ////////////////////////////////////////////////
                // @类方法
                ////////////////////////////////////////////////

                #region 类方法

                public static void DidServiceConfigChanged(ServiceConfig serviceConfig)
                {
                        // !!!
                        _imageCache.Clear();
                        // !!!
                }

                private static void AddFileInfoToStorageQueue(Models.FileInfo fileInfo)
                {
                        if (fileInfo.Id == 0)
                        {
                                return;
                        }

                        lock (_fileInfoStorageQueue)
                        {
                                _fileInfoStorageQueue.Add(fileInfo);
                        }
                        _taskToStorageFileInfo.Start();
                }

                private static long SaveImageToFilePath(
                    string objectFileAbsolutePath,
                    Image image,
                    IImageEncoder imageEncoder,
                    int imageSaveQuality)
                {
                        if (image == null
                            || imageEncoder == null)
                        {
                                return 0;
                        }

                        if (imageEncoder
                            is SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder jpgEncoder)
                        {
                                jpgEncoder.Quality = imageSaveQuality;
                        }

                        ////////////////////////////////////////////////
                        // !!! 去掉无用的媒体信息 !!!
                        ////////////////////////////////////////////////
                        image.Metadata.ExifProfile = null;
                        image.Metadata.XmpProfile = null;
                        if (imageEncoder is PngEncoder pngEncoder)
                        {
                                pngEncoder.IgnoreMetadata = true;
                        }
                        // !!!



                        image.Save(
                            objectFileAbsolutePath,
                            imageEncoder);

                        var imageFileInfo = new System.IO.FileInfo(objectFileAbsolutePath);
                        { }
                        return imageFileInfo.Length;
                }

                private static Image ProcessImageFileWithSourceImage(
                        Image sourceImage,
                        ImageSize? objectImageSize,
                        bool isWatermarkEnable,
                        ImageWatermarkInfo? watermarkInfo,
                        string? watermarkCaption)
                {
                        var isNeedResizeImage = false;
                        if (objectImageSize != null)
                        {
                                if (objectImageSize.Width != sourceImage.Width
                                        || objectImageSize.Height != sourceImage.Height)
                                {
                                        isNeedResizeImage = true;
                                }
                        }
                        var isNeedStampWatermark = false;
                        if (isWatermarkEnable == true
                                && watermarkInfo?.IsValid == true)
                        {
                                isNeedStampWatermark = true;
                        }

                        if (isNeedResizeImage == false
                                && isNeedStampWatermark == false)
                        {
                                return sourceImage;
                        }

                        Image objectImage = sourceImage.Clone(
                                imageProcessingContext =>
                                {
                                        var objectImageWidth = sourceImage.Width;
                                        var objectImageHeight = sourceImage.Height;
                                        if (isNeedResizeImage
                                        && objectImageSize != null)
                                        {
                                                objectImageWidth = objectImageSize.Width;
                                                objectImageHeight = objectImageSize.Height;
                                                imageProcessingContext.Resize(new ResizeOptions()
                                                {
                                                        Mode = ResizeMode.Crop,
                                                        Size = new Size(objectImageWidth, objectImageHeight)
                                                        //,
                                                        //TargetRectangle = new Rectangle(
                                                        //        (sourceImage.Width - objectImageWidth) / 2,
                                                        //        (sourceImage.Height - objectImageHeight) / 2,
                                                        //        objectImageWidth,
                                                        //        objectImageHeight)
                                                });
                                        }
                                        if (isNeedStampWatermark == false
                                        || watermarkInfo == null)
                                        {
                                                return;
                                        }

                                        var watermarkImageAbsoluteFilePath
                                        = watermarkInfo
                                        .WatermarkImageFilePath
                                        ?.ToAbsoluteFilePathInRootPath(
                                                BaoXia.Utils.Environment.ApplicationDirectoryPath);
                                        if (string.IsNullOrEmpty(watermarkImageAbsoluteFilePath))
                                        {
                                                return;
                                        }

                                        var loadWatermarkImageTask
                                        = _imageCache.GetAsync(
                                                watermarkImageAbsoluteFilePath,
                                                null);
                                        // !!!
                                        loadWatermarkImageTask?.Wait();
                                        // !!!
                                        var watermarkImage = loadWatermarkImageTask?.Result;
                                        if (watermarkImage == null)
                                        {
                                                return;
                                        }


                                        ////////////////////////////////////////////////
                                        // 开始绘制水印：
                                        ////////////////////////////////////////////////

                                        var watermarkMarginLeft = watermarkInfo.MarginBottom;
                                        var watermarkMarginTop = watermarkInfo.MarginTop;
                                        var watermarkMarginRight = watermarkInfo.MarginRight;
                                        var watermarkMarginBottom = watermarkInfo.MarginBottom;
                                        var isWatermarkDrawImageFirst
                                        = watermarkInfo.IsWatermarkDrawImageFirst;
                                        var watermarkImageWidth
                                        = watermarkImage.Width;
                                        var watermarkImageHeight
                                        = watermarkImage.Height;
                                        var watermarkImageAndCaptionSeparatorSize
                                        = watermarkInfo.WatermarkImageAndCaptionSeparatorSize;
                                        var watermarkCaptionFontFileName
                                        = watermarkInfo.WatermarkCaptionFontFileName;
                                        SixLabors.Fonts.Font? watermarkCaptionFont = null;
                                        var watermarkCaptionFontSize
                                        = watermarkInfo.WatermarkCaptionFontSize;
                                        int watermarkCaptionWidth = 0;
                                        int watermarkCaptionHeight = 0;
                                        if (watermarkCaptionFontFileName?.Length > 0
                                                && watermarkCaption?.Length > 0)
                                        {
                                                watermarkCaptionFont
                                                = _fontCache.Get(watermarkCaptionFontFileName, null)
                                                ?.FontFamily
                                                .CreateFont(watermarkCaptionFontSize);
                                                if (watermarkCaptionFont != null)
                                                {
                                                        var watermarkCaptionSize
                                                        = SixLabors.Fonts.TextMeasurer.Measure(
                                                                watermarkCaption,
                                                                new SixLabors.Fonts.TextOptions(watermarkCaptionFont));
                                                        // !!!
                                                        watermarkCaptionWidth = (int)Math.Ceiling(watermarkCaptionSize.Width);
                                                        watermarkCaptionHeight = (int)Math.Ceiling(watermarkCaptionSize.Height);
                                                        // !!!
                                                }
                                        }


                                        ////////////////////////////////////////////////

                                        var watermarkWidth = 0;
                                        var watermarkHeight = 0;
                                        var watermarkImageLeft = 0;
                                        var watermarkImageTop = 0;
                                        var watermarkCaptionLeft = 0;
                                        var watermarkCaptionTop = 0;
                                        switch (watermarkInfo.WatermarkLayout)
                                        {
                                                default:
                                                case ImageWatermarkInfo.LayoutType.HorizontalLayout:
                                                        {
                                                                watermarkWidth = watermarkImageWidth;
                                                                if (watermarkCaptionWidth > 0)
                                                                {
                                                                        watermarkWidth
                                                                        += watermarkImageAndCaptionSeparatorSize
                                                                        + watermarkCaptionWidth;
                                                                }
                                                                watermarkHeight
                                                                = watermarkImageHeight > watermarkCaptionHeight
                                                                ? watermarkImageHeight
                                                                : watermarkCaptionHeight;


                                                                if (watermarkInfo.IsWatermarkDrawImageFirst)
                                                                {
                                                                        watermarkImageLeft = 0;
                                                                        watermarkCaptionLeft
                                                                        = watermarkImageLeft
                                                                        + watermarkImageWidth
                                                                        + watermarkImageAndCaptionSeparatorSize;
                                                                }
                                                                else
                                                                {
                                                                        watermarkCaptionLeft = 0;
                                                                        watermarkImageLeft
                                                                        = watermarkCaptionLeft
                                                                        + watermarkCaptionWidth
                                                                        + watermarkImageAndCaptionSeparatorSize;
                                                                }

                                                                switch (watermarkInfo.WatermarkLayoutAlignType)
                                                                {
                                                                        case ImageWatermarkInfo.AlignType.Left:
                                                                        case ImageWatermarkInfo.AlignType.Top:
                                                                                {
                                                                                        watermarkImageTop = 0;
                                                                                        watermarkCaptionTop = 0;
                                                                                }
                                                                                break;
                                                                        case ImageWatermarkInfo.AlignType.Right:
                                                                        case ImageWatermarkInfo.AlignType.Bottom:
                                                                                {
                                                                                        watermarkImageTop
                                                                                        = watermarkHeight
                                                                                        - watermarkImageHeight;
                                                                                        watermarkCaptionTop
                                                                                        = watermarkHeight
                                                                                        - watermarkCaptionHeight;
                                                                                }
                                                                                break;
                                                                        default:
                                                                        case ImageWatermarkInfo.AlignType.Center:
                                                                                {
                                                                                        watermarkImageTop
                                                                                        = (watermarkHeight
                                                                                        - watermarkImageHeight)
                                                                                        / 2;
                                                                                        watermarkCaptionTop
                                                                                        = (watermarkHeight
                                                                                        - watermarkCaptionHeight)
                                                                                        / 2;
                                                                                }
                                                                                break;
                                                                }
                                                        }
                                                        break;
                                                case ImageWatermarkInfo.LayoutType.VerticalLayout:
                                                        {
                                                                watermarkWidth
                                                                = watermarkImageWidth > watermarkCaptionWidth
                                                                ? watermarkImageWidth
                                                                : watermarkCaptionWidth;
                                                                watermarkHeight
                                                                = watermarkImageHeight;
                                                                {
                                                                        watermarkHeight
                                                                        += watermarkImageAndCaptionSeparatorSize
                                                                        + watermarkCaptionHeight;
                                                                }


                                                                if (watermarkInfo.IsWatermarkDrawImageFirst)
                                                                {
                                                                        watermarkImageTop = 0;
                                                                        watermarkCaptionTop
                                                                        = watermarkImageTop
                                                                        + watermarkImageHeight
                                                                        + watermarkImageAndCaptionSeparatorSize;
                                                                }
                                                                else
                                                                {
                                                                        watermarkCaptionTop = 0;
                                                                        watermarkImageTop
                                                                        = watermarkCaptionTop
                                                                        + watermarkCaptionHeight
                                                                        + watermarkImageAndCaptionSeparatorSize;
                                                                }

                                                                switch (watermarkInfo.WatermarkLayoutAlignType)
                                                                {
                                                                        case ImageWatermarkInfo.AlignType.Left:
                                                                        case ImageWatermarkInfo.AlignType.Top:
                                                                                {
                                                                                        watermarkImageLeft = 0;
                                                                                        watermarkCaptionLeft = 0;
                                                                                }
                                                                                break;
                                                                        case ImageWatermarkInfo.AlignType.Right:
                                                                        case ImageWatermarkInfo.AlignType.Bottom:
                                                                                {
                                                                                        watermarkImageLeft
                                                                                        = watermarkWidth
                                                                                        - watermarkImageWidth;
                                                                                        watermarkCaptionLeft
                                                                                        = watermarkWidth
                                                                                        - watermarkCaptionWidth;
                                                                                }
                                                                                break;
                                                                        default:
                                                                        case ImageWatermarkInfo.AlignType.Center:
                                                                                {
                                                                                        watermarkImageLeft
                                                                                        = (watermarkWidth
                                                                                        - watermarkImageWidth)
                                                                                        / 2;
                                                                                        watermarkCaptionLeft
                                                                                        = (watermarkWidth
                                                                                        - watermarkCaptionWidth)
                                                                                        / 2;
                                                                                }
                                                                                break;
                                                                }
                                                        }
                                                        break;
                                        }

                                        var watermarkLeft = 0;
                                        switch (watermarkInfo.HorizontalAlignType)
                                        {
                                                case ImageWatermarkInfo.AlignType.Left:
                                                case ImageWatermarkInfo.AlignType.Top:
                                                        {
                                                                watermarkLeft = watermarkInfo.MarginLeft;
                                                        }
                                                        break;
                                                default:
                                                case ImageWatermarkInfo.AlignType.Right:
                                                case ImageWatermarkInfo.AlignType.Bottom:
                                                        {
                                                                watermarkLeft
                                                                = objectImageWidth
                                                                - watermarkInfo.MarginRight
                                                                - watermarkWidth;
                                                        }
                                                        break;
                                                case ImageWatermarkInfo.AlignType.Center:
                                                        {
                                                                watermarkLeft
                                                                = (objectImageWidth
                                                                - watermarkWidth)
                                                                / 2;
                                                        }
                                                        break;
                                        }
                                        var watermarkTop = 0;
                                        switch (watermarkInfo.VerticalAlignType)
                                        {
                                                case ImageWatermarkInfo.AlignType.Left:
                                                case ImageWatermarkInfo.AlignType.Top:
                                                        {
                                                                watermarkTop = watermarkInfo.MarginTop;
                                                        }
                                                        break;
                                                default:
                                                case ImageWatermarkInfo.AlignType.Right:
                                                case ImageWatermarkInfo.AlignType.Bottom:
                                                        {
                                                                watermarkTop
                                                                = objectImageHeight
                                                                - watermarkInfo.MarginBottom
                                                                - watermarkHeight;
                                                        }
                                                        break;
                                                case ImageWatermarkInfo.AlignType.Center:
                                                        {
                                                                watermarkTop
                                                                = (objectImageHeight
                                                                - watermarkHeight)
                                                                / 2;
                                                        }
                                                        break;
                                        }

                                        watermarkImageLeft += watermarkLeft;
                                        watermarkImageTop += watermarkTop;
                                        {
                                                if (watermarkImageLeft < 0)
                                                {
                                                        watermarkImageLeft = 0;
                                                }
                                                var watermarkImageDrawWidth = watermarkImage.Width;
                                                if (watermarkImageLeft + watermarkImage.Width
                                                > objectImageWidth)
                                                {
                                                        watermarkImageDrawWidth
                                                        = objectImageWidth
                                                        - watermarkImageLeft;
                                                }
                                                if (watermarkImageTop < 0)
                                                {
                                                        watermarkImageTop = 0;
                                                }
                                                var watermarkImageDrawHeight = watermarkImage.Height;
                                                if (watermarkImageTop + watermarkImage.Height
                                                > objectImageHeight)
                                                {
                                                        watermarkImageDrawHeight
                                                        = objectImageHeight
                                                        - watermarkImageTop;
                                                }

                                                if (watermarkImageDrawWidth != watermarkImage.Width
                                                || watermarkImageDrawHeight != watermarkImage.Height)
                                                {
                                                        watermarkImage.Mutate(watermarkImageProcessingContext =>
                                                        {
                                                                imageProcessingContext.Resize(
                                                                        watermarkImageDrawWidth,
                                                                        watermarkImageDrawHeight);
                                                        });
                                                }
                                                imageProcessingContext.DrawImage(
                                                        watermarkImage,
                                                        new Point(watermarkImageLeft, watermarkImageTop),
                                                        1.0F);
                                        }

                                        watermarkCaptionLeft += watermarkLeft;
                                        watermarkCaptionTop += watermarkTop;
                                        if (watermarkCaptionFont != null)
                                        {
                                                if (watermarkInfo.WatermarkCaptionColor?.ToRGBA(
                                                        out var watermarkCaptionColorRed,
                                                        out var watermarkCaptionColorGreen,
                                                        out var watermarkCaptionColorBlue,
                                                        out var watermarkCaptionColorAlpha) == true)
                                                {
                                                        var watermarkCaptionColor
                                                        = Color.FromRgba(
                                                                watermarkCaptionColorRed,
                                                                watermarkCaptionColorGreen,
                                                                watermarkCaptionColorBlue,
                                                                (byte)(255.0F * watermarkCaptionColorAlpha));
                                                        var watermarkCaptionColorWithoutAlpha
                                                        = Color.FromRgba(
                                                                watermarkCaptionColorRed,
                                                                watermarkCaptionColorGreen,
                                                                watermarkCaptionColorBlue,
                                                                255);

                                                        var watermarkCaptionBorderSize
                                                        = watermarkInfo.WatermarkCaptionBorderSize;
                                                        if (watermarkInfo.WatermarkCaptionBorderColor?.ToRGBA(
                                                                out var red,
                                                                out var green,
                                                                out var blue,
                                                                out var alpha) == true)
                                                        {
                                                                var watermarkCaptionBorderColor
                                                                = Color.FromRgba(
                                                                        red,
                                                                        green,
                                                                        blue,
                                                                        (byte)(255.0F * alpha));
                                                                imageProcessingContext.DrawText(
                                                                        new DrawingOptions(),
                                                                        watermarkCaption,
                                                                        watermarkCaptionFont,
                                                                        new SolidBrush(watermarkCaptionColor),
                                                                        new Pen(watermarkCaptionBorderColor, watermarkCaptionBorderSize),
                                                                        new Point(
                                                                                watermarkCaptionLeft,
                                                                                watermarkCaptionTop));
                                                        }
                                                        else
                                                        {
                                                                imageProcessingContext.DrawText(
                                                                        watermarkCaption,
                                                                        watermarkCaptionFont,
                                                                        SixLabors.ImageSharp.Color.FromRgba(0, 0, 0, 16),
                                                                        new Point(
                                                                                watermarkCaptionLeft,
                                                                                watermarkCaptionTop));
                                                        }
                                                }
                                        }
                                });
                        { }
                        return objectImage;
                }


                private static ImageFileInfo? SaveImageToFilePathAndCreateImageFileInfo(
                        Image? image,
                        string objectFileAbsolutePath,
                        string? objectFileAbsoluteUri,
                        IImageEncoder imageEncoder,
                        int imageSaveQuality)
                {
                        int imageWidth;
                        int imageHeight;
                        if (image != null)
                        {
                                imageWidth = image.Width;
                                imageHeight = image.Height;
                        }
                        else
                        {
                                return null;
                        }


                        var imageBytesCountSaved
                        = SaveImageToFilePath(
                                objectFileAbsolutePath,
                                image,
                                imageEncoder,
                                imageSaveQuality);

                        var imageFileInfo = new ImageFileInfo()
                        {
                                FileUrl = objectFileAbsoluteUri,
                                FileSizeInKB = imageBytesCountSaved / 1024,

                                Width = imageWidth,
                                Height = imageHeight
                        };
                        return imageFileInfo;
                }

                #endregion



                ////////////////////////////////////////////////
                // @自身属性
                ////////////////////////////////////////////////

                #region 自身属性

                private Data.FileInfoDbContext _fileInfoDbContext;

                #endregion

                ////////////////////////////////////////////////
                // @自身实现
                ////////////////////////////////////////////////

                #region 自身实现

                public UploadController(Data.FileInfoDbContext fileInfoDbContext)
                {
                        _fileInfoDbContext = fileInfoDbContext;
                }

                private async Task<UploadResponseType> ProcessUploadRequestAsync<UploadResponseType>(
                        UploadRequest? request,
                        Data.FileInfoDbContext fileInfoDbContext,
                        Func<FilePathRule,
                            string,
                            string?,
                            IFormFile,
                            //
                            UploadRequest,
                            int,
                            int,
                            string?,
                            string[]?,
                            //
                            FileUploadStatisticsInfo,
                            //
                            Task<UploadResponseType>>?
                        toGetUploadResponseBySaveFileAtAbsolutePathAsync = null)
                        where UploadResponseType : UploadResponse, new()
                {
                        Models.FileInfo? fileInfoInDb = null;
                        var fileUploadStatisticsInfo = new FileUploadStatisticsInfo();
                        var responseBeginTime = DateTime.Now;
                        await using var _ = new CodesAtFunctionEnd(
                                async () =>
                                {
                                        if (fileInfoInDb == null
                                        || fileInfoInDb.Id == 0)
                                        {
                                                return;
                                        }

                                        var responseEndTime = DateTime.Now;
                                        fileUploadStatisticsInfo.SecondsToResponse
                                                = (responseEndTime - responseBeginTime)
                                                .TotalSeconds;

                                        ////////////////////////////////////////////////
                                        // !!! 存储最终的文件信息 !!!
                                        ////////////////////////////////////////////////
                                        var updateFileInfoToDatabaseBeginTime = DateTime.Now;
                                        {
                                                var updateFileInfoToDatabaseBeinTime = DateTime.Now;
                                                {
                                                        fileInfoInDb.SecondsToResponse = fileUploadStatisticsInfo.SecondsToResponse;
                                                        fileInfoInDb.SecondsToDatabaseOperation
                                                        = fileUploadStatisticsInfo.SecondsToDatabaseOperation_CreateFileInfo
                                                        + fileUploadStatisticsInfo.SecondsToDatabaseOperation_UpadateFileInfo;
                                                        fileInfoInDb.SecondsToFileProcessing = fileUploadStatisticsInfo.SecondsToFileProcessing;
                                                        fileInfoInDb.SecondsToFileStorage = fileUploadStatisticsInfo.SecondsToFileStorage;
                                                        {
                                                                // !!!
                                                                await fileInfoDbContext.SaveChangesAsync();
                                                                // !!!
                                                        }
                                                        fileInfoDbContext.Entry(fileInfoInDb).State = EntityState.Detached;
                                                }
                                                var updateFileInfoToDatabaseEndTime = DateTime.Now;
                                                fileUploadStatisticsInfo.SecondsToDatabaseOperation_UpadateFileInfo
                                                = (updateFileInfoToDatabaseEndTime - updateFileInfoToDatabaseBeginTime)
                                                .TotalSeconds;
                                                // !!!
                                                fileInfoInDb.SecondsToDatabaseOperation
                                                = fileUploadStatisticsInfo.SecondsToDatabaseOperation_CreateFileInfo
                                                + fileUploadStatisticsInfo.SecondsToDatabaseOperation_UpadateFileInfo;
                                                // !!!
                                                UploadController.AddFileInfoToStorageQueue(fileInfoInDb);
                                                // !!!
                                        }
                                        ////////////////////////////////////////////////
                                });

                        if (request == null
                            || request.IsValid == false)
                        {
                                return new UploadResponseType()
                                {
                                        Error = Error.InvalidRequest
                                };
                        }

                        var requestFile = request.File;
                        var requestFileName = requestFile?.FileName;
                        string? requestFileExtenionName = null;
                        if (requestFileName?.Length > 0)
                        {
                                requestFileExtenionName
                                    = requestFileName.ToFileExtensionName();
                        }
                        var fileTags = request.FileTags;
                        var filePathRuleMatched
                            = Config.Service.GetFilePathRuleWithFileExtensionName(
                                requestFileExtenionName,
                                fileTags);
                        if (filePathRuleMatched == null)
                        {
                                return new UploadResponseType()
                                {
                                        Error = Error.ObjectNotExisted
                                };
                        }

                        var createFileInfoToDatabaseBeginTime = DateTime.Now;
                        ////////////////////////////////////////////////
                        var currentUserId = 0;// @will
                        var now = DateTime.Now;
                        fileInfoInDb = new Models.FileInfo()
                        {
                                CreateTime = now,
                                UpdateTime = now,
                        };
                        {
                                fileInfoDbContext.FileInfo?.Add(fileInfoInDb);
                        }
                        await fileInfoDbContext.SaveChangesAsync();
                        ////////////////////////////////////////////////
                        var createFileInfoToDatabaseEndTime = DateTime.Now;
                        fileUploadStatisticsInfo.SecondsToDatabaseOperation_CreateFileInfo
                                = (createFileInfoToDatabaseEndTime
                                - createFileInfoToDatabaseBeginTime).TotalSeconds;

                        string? objectFileAbsolutePath
                                = filePathRuleMatched.GetFileAbsolutePathWithFileId(
                                        currentUserId,
                                        fileInfoInDb.Id,
                                        requestFileExtenionName,
                                        fileTags,
                                        0,
                                        0);
                        if (string.IsNullOrEmpty(objectFileAbsolutePath))
                        {
                                throw new ApplicationException(
                                        "无法生成文件的绝对路径：\r\n"
                                        + "用户Id：" + currentUserId + "，"
                                        + "文件Id：" + fileInfoInDb.Id + "，"
                                        + "文件扩展名：" + requestFileExtenionName + "，"
                                        + "文件标签：" + StringExtension.StringWithStrings(request.FileTags) + "。");
                        }

                        var objectFileAbsoluteDictionaryPath
                                = objectFileAbsolutePath.ToFileSystemDirectoryPath(true);
                        {
                                System.IO.Directory.CreateDirectory(objectFileAbsoluteDictionaryPath);
                        }
                        string? objectFileAbsoluteUri
                                = filePathRuleMatched.GetFileAbsoluteUriWithFileId(
                                        currentUserId,
                                        fileInfoInDb.Id,
                                        requestFileExtenionName,
                                        fileTags,
                                        0,
                                        0);

                        UploadResponseType? response = null;
                        var sourceFile = request.File!;
                        if (toGetUploadResponseBySaveFileAtAbsolutePathAsync != null)
                        {
                                response
                                    = await toGetUploadResponseBySaveFileAtAbsolutePathAsync(
                                    filePathRuleMatched,
                                    objectFileAbsolutePath,
                                    objectFileAbsoluteUri,
                                    sourceFile,
                                    //
                                    request,
                                    currentUserId,
                                    fileInfoInDb.Id,
                                    requestFileExtenionName,
                                    fileTags,
                                    //
                                    fileUploadStatisticsInfo);
                                {
                                        response.FileUploadStatisticsInfo = fileUploadStatisticsInfo;
                                }
                        }
                        else
                        {
                                var fileStorageBeginTime = DateTime.Now;
                                ////////////////////////////////////////////////
                                var objectFileWriteStream = new FileStream(objectFileAbsolutePath, FileMode.CreateNew);
                                {
                                        await sourceFile.CopyToAsync(objectFileWriteStream);
                                }
                                var fileSaveBytesCount = objectFileWriteStream.Length;
                                ////////////////////////////////////////////////
                                var fileStorageEndTime = DateTime.Now;
                                fileUploadStatisticsInfo.SecondsToDatabaseOperation_CreateFileInfo
                                        = (fileStorageEndTime - fileStorageBeginTime).TotalSeconds;

                                // !!!
                                response = new UploadResponseType
                                {
                                        FileInfo = new FileInfo(
                                            objectFileAbsoluteUri,
                                            fileSaveBytesCount / 1024 / 1024),

                                        FileUploadStatisticsInfo = fileUploadStatisticsInfo
                                };
                                // !!!
                        }
                        return response;
                }

                public class UploadRequest
                {
                        [MemberNotNullWhen(true, "IsValid")]
                        public IFormFile? File { get; set; }

                        protected string? _fileTagsString;

                        protected string[]? _fileTags;

                        public string? FileTagsString
                        {
                                get
                                {
                                        return _fileTagsString;
                                }

                                set
                                {
                                        _fileTagsString = value;
                                        _fileTags = _fileTagsString?.Split(",");
                                }
                        }

                        public string[]? FileTags
                        {
                                get
                                {
                                        return _fileTags;
                                }
                                set
                                {
                                        _fileTags = value;
                                        _fileTagsString
                                                = _fileTags != null
                                                ? String.Join(',', _fileTags)
                                                : null;
                                }
                        }

                        public bool IsValid
                        {
                                get
                                {
                                        if (this.File != null)
                                        {
                                                return true;
                                        }
                                        return false;
                                }
                        }
                }

                public class FileInfo
                {
                        public string? FileUrl { get; set; }

                        public string? FileExtensionName
                        {
                                get
                                {
                                        return this.FileUrl?.ToFileExtensionName();
                                }
                        }

                        public long FileSizeInKB { get; set; }
                        public float FileSizeInMB
                        {
                                get
                                {
                                        return this.FileSizeInKB / 1024;
                                }
                        }

                        public FileInfo()
                        { }

                        public FileInfo(
                                string? fileUrl,
                                long fileSizeInKB)
                        {
                                this.FileUrl = fileUrl;
                                this.FileSizeInKB = fileSizeInKB;
                        }
                }

                public class UploadResponse : ViewModels.Response
                {
                        public FileInfo? FileInfo { get; set; }

                        public FileUploadStatisticsInfo? FileUploadStatisticsInfo { get; set; }
                }

                public async Task<IActionResult> Index([FromForm] UploadRequest? request)
                {
                        var response = new UploadResponse();
                        try
                        {
                                response
                                    = await this.ProcessUploadRequestAsync<UploadResponse>(
                                        request,
                                        _fileInfoDbContext);
                        }
                        catch (Exception exception)
                        {
                                Log.Exception.Logs(this, "上传文件失败，程序异常。", exception);
                                //
                                response.Error = Error.ProgramError;
                        }
                        return Json(response);
                }

                public class ImageFileInfo : FileInfo
                {
                        public int Width { get; set; }

                        public int Height { get; set; }
                }

                public class ImageUploadRequest : UploadRequest
                {
                        protected ImageArea? _imageArea;

                        public ImageArea? ImageSaveArea
                        {
                                get
                                {
                                        return _imageArea;
                                }
                                set
                                {
                                        if (_imageArea == null)
                                        {
                                                lock (this)
                                                {
                                                        _imageArea = _imageArea ?? new ImageArea();
                                                }
                                        }
                                        _imageArea = value;
                                }
                        }

                        public int ImageSaveAreaLeft
                        {
                                get
                                {
                                        if (_imageArea != null)
                                        {
                                                return _imageArea.Left;
                                        }
                                        return 0;
                                }

                                set
                                {
                                        if (_imageArea == null)
                                        {
                                                lock (this)
                                                {
                                                        _imageArea = _imageArea ?? new ImageArea();
                                                }
                                        }
                                        _imageArea.Left = value;
                                }
                        }

                        public int ImageSaveAreaTop
                        {
                                get
                                {
                                        if (_imageArea != null)
                                        {
                                                return _imageArea.Top;
                                        }
                                        return 0;
                                }

                                set
                                {
                                        if (_imageArea == null)
                                        {
                                                lock (this)
                                                {
                                                        _imageArea = _imageArea ?? new ImageArea();
                                                }
                                        }
                                        _imageArea.Top = value;
                                }
                        }

                        public int ImageSaveAreaWidth
                        {
                                get
                                {
                                        if (_imageArea != null)
                                        {
                                                return _imageArea.Width;
                                        }
                                        return 0;
                                }

                                set
                                {
                                        if (_imageArea == null)
                                        {
                                                lock (this)
                                                {
                                                        _imageArea = _imageArea ?? new ImageArea();
                                                }
                                        }
                                        _imageArea.Width = value;
                                }
                        }

                        public int ImageSaveAreaHeight
                        {
                                get
                                {
                                        if (_imageArea != null)
                                        {
                                                return _imageArea.Height;
                                        }
                                        return 0;
                                }

                                set
                                {
                                        if (_imageArea == null)
                                        {
                                                lock (this)
                                                {
                                                        _imageArea = _imageArea ?? new ImageArea();
                                                }
                                        }
                                        _imageArea.Height = value;
                                }
                        }




                        protected ImageSize? _imageSize;

                        public ImageSize? ImageSaveSize
                        {
                                get
                                {
                                        return _imageSize;
                                }
                                set
                                {
                                        if (_imageSize == null)
                                        {
                                                lock (this)
                                                {
                                                        _imageSize = _imageSize ?? new ImageSize();
                                                }
                                        }
                                }
                        }

                        public int ImageSaveSizeWidth
                        {
                                get
                                {
                                        if (_imageSize != null)
                                        {
                                                return _imageSize.Width;
                                        }
                                        return 0;
                                }

                                set
                                {
                                        if (_imageSize == null)
                                        {
                                                lock (this)
                                                {
                                                        _imageSize = _imageSize ?? new ImageSize();
                                                }
                                        }
                                        _imageSize.Width = value;
                                }
                        }

                        public int ImageSaveSizeHeight
                        {
                                get
                                {
                                        if (_imageSize != null)
                                        {
                                                return _imageSize.Height;
                                        }
                                        return 0;
                                }

                                set
                                {
                                        if (_imageSize == null)
                                        {
                                                lock (this)
                                                {
                                                        _imageSize = _imageSize ?? new ImageSize();
                                                }
                                        }
                                        _imageSize.Height = value;
                                }
                        }



                        public string? ImageSaveFormat { get; set; }

                        public int ImageSaveQuality { get; set; }

                        public bool IsAutoCreateListAndContentImage { get; set; }

                        public bool IsWatermarkEnable { get; set; }

                        public string? WatermarkCaption { get; set; }
                }

                public class ImageUploadResponse : UploadResponse
                {
                        public ImageFileInfo? ListImageFileInfo { get; set; }

                        public ImageFileInfo? ListSquareImageFileInfo { get; set; }

                        public ImageFileInfo? ContentImageFileInfo { get; set; }

                        public ImageFileInfo? SourceImageFileInfo { get; set; }
                }

                public async Task<IActionResult> Image([FromForm] ImageUploadRequest? request)
                {
                        var response = new ImageUploadResponse();
                        try
                        {
                                response = await this.ProcessUploadRequestAsync<ImageUploadResponse>(
                                        request,
                                        _fileInfoDbContext,
                                        async (
                                            filePathRuleMatched,
                                            objectFileAbsolutePath,
                                            objectFileAbsoluteUri,
                                            sourceFile,
                                            uploadRequest,
                                            fileId,
                                            userId,
                                            fileExtensionName,
                                            fileTags,
                                            fileUploadStatisticsInfo) =>
                                        {
                                                var sourceFileStream = sourceFile.OpenReadStream();
                                                var image = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(
                                                        sourceFileStream);

                                                if (image == null)
                                                {
                                                        throw new ApplicationException("无法解析图象数据。");
                                                }

                                                ImageArea? imageSaveArea = null;
                                                ImageSize? imageSaveSize = null;
                                                string? imageSaveFileExtensionName = null;
                                                var imageSaveQuality = 100;
                                                bool isAutoCreateListAndContentImage = false;
                                                var watermarkCaption = request?.WatermarkCaption;

                                                if (uploadRequest is ImageUploadRequest imageUploadRequest)
                                                {
                                                        imageSaveArea = imageUploadRequest.ImageSaveArea;
                                                        imageSaveSize = imageUploadRequest.ImageSaveSize;

                                                        imageSaveFileExtensionName = imageUploadRequest.ImageSaveFormat;
                                                        if (string.IsNullOrEmpty(imageSaveFileExtensionName))
                                                        {
                                                                imageSaveFileExtensionName
                                                                = objectFileAbsolutePath.ToFileExtensionName();
                                                                if ("gif".EqualsIgnoreCase(imageSaveFileExtensionName) == false)
                                                                {
                                                                        var imageSaveFormatExceptGifDefault
                                                                        = filePathRuleMatched.ImageSaveFormatExceptGifDefault;
                                                                        if (string.IsNullOrEmpty(imageSaveFormatExceptGifDefault) == false)
                                                                        {
                                                                                imageSaveFileExtensionName = imageSaveFormatExceptGifDefault;
                                                                        }
                                                                }
                                                        }

                                                        imageSaveQuality = imageUploadRequest.ImageSaveQuality;

                                                        isAutoCreateListAndContentImage = imageUploadRequest.IsAutoCreateListAndContentImage;
                                                }

                                                var imageEncoder
                                                = ISIImageFormatExtension.GetImageEncoderWithFileExtensionName(
                                                        imageSaveFileExtensionName);
                                                if (imageEncoder == null)
                                                {
                                                        throw new ApplicationException("未知的图片格式：" + objectFileAbsolutePath + "。");
                                                }

                                                if (imageSaveQuality <= 0)
                                                {
                                                        if (imageEncoder
                                                        is SixLabors.ImageSharp.Formats.Webp.WebpEncoder)
                                                        {
                                                                imageSaveQuality
                                                                = filePathRuleMatched.ImageSaveQualityForWebpDefault;
                                                        }
                                                        else if (imageEncoder
                                                        is SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder)
                                                        {
                                                                imageSaveQuality
                                                                = filePathRuleMatched.ImageSaveQualityForJpegDefault;
                                                        }
                                                }

                                                var fileProcessingBeginTime_CropSourceImage = DateTime.Now;
                                                if (imageSaveArea?.IsValid == true
                                                || imageSaveSize?.IsValid == true)
                                                {
                                                        image.Mutate(imageProcessingContext =>
                                                        {
                                                                if (imageSaveArea?.IsValid == true)
                                                                {
                                                                        if (imageSaveArea.Left < 0)
                                                                        {
                                                                                imageSaveArea.Width += imageSaveArea.Left;
                                                                                imageSaveArea.Left = 0;
                                                                        }
                                                                        if (imageSaveArea.Left + imageSaveArea.Width >= image.Width)
                                                                        {
                                                                                imageSaveArea.Width
                                                                                = image.Width
                                                                                - imageSaveArea.Left;
                                                                        }

                                                                        if (imageSaveArea.Top < 0)
                                                                        {
                                                                                imageSaveArea.Height += imageSaveArea.Top;
                                                                                imageSaveArea.Top = 0;
                                                                        }
                                                                        if (imageSaveArea.Top + imageSaveArea.Height >= image.Height)
                                                                        {
                                                                                imageSaveArea.Height
                                                                                = image.Height
                                                                                - imageSaveArea.Top;
                                                                        }

                                                                        imageProcessingContext
                                                                        = imageProcessingContext
                                                                        .Crop(imageSaveArea.ToISRectangle());
                                                                }

                                                                if (imageSaveSize?.IsValid == true)
                                                                {
                                                                        imageProcessingContext.Resize(
                                                                                imageSaveSize.Width,
                                                                                imageSaveSize.Height);
                                                                }
                                                        });
                                                }
                                                var fileProcessingEndTime_CropSourceImage = DateTime.Now;
                                                var fileProcessingSeconds
                                                = (fileProcessingEndTime_CropSourceImage
                                                - fileProcessingBeginTime_CropSourceImage)
                                                .TotalSeconds;

                                                var response = new ImageUploadResponse();
                                                var tasksToSaveImageFile = new List<Task>();

                                                // 保存图片文件，原始图片：
                                                var sourceImageFileProcessingSeconds = 0.0;
                                                var sourceImageFileStorageSeconds = 0.0;
                                                var taskToSaveSourceImageFile
                                                = Task.Run(() =>
                                                {
                                                        var imageSizeMax
                                                                        = filePathRuleMatched.ImageSizeMax;
                                                        if (imageSizeMax == null)
                                                        {
                                                                imageSizeMax = new ImageSize();
                                                        }
                                                        var imageSize
                                                        = new ImageSize(image.Width, image.Height)
                                                        .SizeByZoomWithMaxSize(imageSizeMax);

                                                        var imageFileAbsolutePath
                                                        = filePathRuleMatched.GetFileAbsolutePathWithFileId(
                                                            userId,
                                                            fileId,
                                                            imageSaveFileExtensionName,
                                                            fileTags,
                                                            image.Width,
                                                            image.Height);
                                                        if (string.IsNullOrEmpty(imageFileAbsolutePath))
                                                        {
                                                                throw new ApplicationException("无法保存图片文件，指定的文件路径无效。");
                                                        }

                                                        var imageFileAbsoluteUri
                                                        = filePathRuleMatched.GetFileAbsoluteUriWithFileId(
                                                            userId,
                                                            fileId,
                                                            imageSaveFileExtensionName,
                                                            fileTags,
                                                            image.Width,
                                                            image.Height);
                                                        ////////////////////////////////////////////////


                                                        var sourceImageFileProcessingBeginTime = DateTime.Now;
                                                        //////////////////////////////////////////////
                                                        var sourceImage = UploadController.ProcessImageFileWithSourceImage(
                                                                image,
                                                                imageSize,
                                                                request?.IsWatermarkEnable == true,
                                                                filePathRuleMatched.GetWatermarkInfoForImage(
                                                                        image.Width,
                                                                        image.Height),
                                                                watermarkCaption);
                                                        //////////////////////////////////////////////
                                                        var sourceImageFileProcessingEndTime = DateTime.Now;
                                                        sourceImageFileProcessingSeconds
                                                        = (sourceImageFileProcessingEndTime - sourceImageFileProcessingBeginTime)
                                                        .TotalSeconds;



                                                        var sourceImageFileStorageBeginTime = DateTime.Now;
                                                        ////////////////////////////////////////////////
                                                        var imageFileInfo
                                                        = UploadController.SaveImageToFilePathAndCreateImageFileInfo(
                                                                sourceImage,
                                                                imageFileAbsolutePath,
                                                                imageFileAbsoluteUri,
                                                                imageEncoder,
                                                                imageSaveQuality);
                                                        ////////////////////////////////////////////////
                                                        var sourceImageFileStorageEndTime = DateTime.Now;
                                                        sourceImageFileStorageSeconds
                                                        = (sourceImageFileStorageEndTime - sourceImageFileStorageBeginTime)
                                                        .TotalSeconds;


                                                        // !!!
                                                        response.FileInfo = imageFileInfo;
                                                        response.SourceImageFileInfo = imageFileInfo;
                                                        // !!!
                                                });
                                                tasksToSaveImageFile.Add(taskToSaveSourceImageFile);

                                                var listImageFileProcessingSeconds = 0.0;
                                                var listImageFileStorageSeconds = 0.0;
                                                var listSquareImageFileProcessingSeconds = 0.0;
                                                var listSquareImageFileStorageSeconds = 0.0;
                                                var contentImageFileProcessingSeconds = 0.0;
                                                var contentImageFileStorageSeconds = 0.0;
                                                if (isAutoCreateListAndContentImage)
                                                {
                                                        // 保存图片文件，【列表缩略图】：
                                                        tasksToSaveImageFile.Add(Task.Run(
                                                                () =>
                                                                {
                                                                        if (ISIImageFormatExtension.TryGetImageFormatWithFileExtensionName(
                                                                            filePathRuleMatched.ListImageFormat,
                                                                            out var listImageFormat,
                                                                            out var listImageEncoder,
                                                                            out _) != true)
                                                                        {
                                                                                throw new ApplicationException("无法创建列表图片，指定的列表图片的格式无效。");
                                                                        }
                                                                        var listImageFileExtensionName
                                                                        = listImageFormat.FileExtensions.First();

                                                                        var listImageSize
                                                                        = filePathRuleMatched.ListImageSize;
                                                                        if (listImageSize?.IsValid != true)
                                                                        {
                                                                                throw new ApplicationException("无法创建列表图片，指定的列表图片尺寸无效。");
                                                                        }

                                                                        var listImageFileAbsolutePath
                                                                        = filePathRuleMatched.GetListImageFileAbsolutePathWithFileId(
                                                                                userId,
                                                                                fileId,
                                                                                listImageFileExtensionName,
                                                                                fileTags,
                                                                                listImageSize.Width,
                                                                                listImageSize.Height);
                                                                        if (string.IsNullOrEmpty(listImageFileAbsolutePath))
                                                                        {
                                                                                throw new ApplicationException("无法创建列表图片，指定的文件路径无效。");
                                                                        }

                                                                        var listImageSaveQuality
                                                                        = filePathRuleMatched.ListImageSaveQuality;

                                                                        var listImageFileAbsoluteUri
                                                                        = filePathRuleMatched.GetListImageFileAbsoluteUriWithFileId(
                                                                                userId,
                                                                                fileId,
                                                                                listImageFileExtensionName,
                                                                                fileTags,
                                                                                listImageSize.Width,
                                                                                listImageSize.Height);
                                                                        ////////////////////////////////////////////////


                                                                        var listImageFileProcessingBeginTime = DateTime.Now;
                                                                        ////////////////////////////////////////////////
                                                                        var listImage = UploadController.ProcessImageFileWithSourceImage(
                                                                                image,
                                                                                listImageSize,
                                                                                request?.IsWatermarkEnable == true,
                                                                                //null,
                                                                                filePathRuleMatched.GetWatermarkInfoForImage(
                                                                                        listImageSize.Width,
                                                                                        listImageSize.Height),
                                                                                watermarkCaption);
                                                                        ////////////////////////////////////////////////
                                                                        var listImageFileProcessingEndTime = DateTime.Now;
                                                                        listImageFileProcessingSeconds
                                                                        = (listImageFileProcessingEndTime - listImageFileProcessingBeginTime)
                                                                        .TotalSeconds;


                                                                        var listImageFileStorageBeginTime = DateTime.Now;
                                                                        ////////////////////////////////////////////////
                                                                        var imageFileInfo
                                                                        = UploadController.SaveImageToFilePathAndCreateImageFileInfo(
                                                                            listImage,
                                                                            listImageFileAbsolutePath,
                                                                            listImageFileAbsoluteUri,
                                                                            listImageEncoder,
                                                                            listImageSaveQuality);
                                                                        ////////////////////////////////////////////////
                                                                        var listImageFileStorageEndTime = DateTime.Now;
                                                                        listImageFileStorageSeconds
                                                                        = (listImageFileStorageEndTime - listImageFileStorageBeginTime)
                                                                        .TotalSeconds;

                                                                        // !!!
                                                                        response.ListImageFileInfo = imageFileInfo;
                                                                        // !!!
                                                                }));


                                                        // 保存图片文件，【列表“正方形”缩略图】：
                                                        tasksToSaveImageFile.Add(Task.Run(
                                                                () =>
                                                                {
                                                                        if (ISIImageFormatExtension.TryGetImageFormatWithFileExtensionName(
                                                                            filePathRuleMatched.ListSquareImageFormat,
                                                                            out var listSquareImageFormat,
                                                                            out var listSquareImageEncoder,
                                                                            out _) != true)
                                                                        {
                                                                                throw new ApplicationException("无法创建列表正方形图片，指定的列表正方形图片的格式无效。");
                                                                        }
                                                                        var listSquareImageFileExtensionName
                                                                        = listSquareImageFormat.FileExtensions.First();

                                                                        var listSquareImageSize
                                                                        = filePathRuleMatched.ListSquareImageSize;
                                                                        if (listSquareImageSize?.IsValid != true)
                                                                        {
                                                                                throw new ApplicationException("无法创建列表正方形图片，指定的列表正方形图片尺寸无效。");
                                                                        }

                                                                        var listSquareImageFileAbsolutePath
                                                                        = filePathRuleMatched.GetListSquareImageFileAbsolutePathWithFileId(
                                                                                userId,
                                                                                fileId,
                                                                                listSquareImageFileExtensionName,
                                                                                fileTags,
                                                                                listSquareImageSize.Width,
                                                                                listSquareImageSize.Height);
                                                                        if (string.IsNullOrEmpty(listSquareImageFileAbsolutePath))
                                                                        {
                                                                                throw new ApplicationException("无法创建列表正方形图片，指定的文件路径无效。");
                                                                        }

                                                                        var listSquareImageSaveQuality
                                                                        = filePathRuleMatched.ListSquareImageSaveQuality;

                                                                        var listSquareImageFileAbsoluteUri
                                                                        = filePathRuleMatched.GetListSquareImageFileAbsoluteUriWithFileId(
                                                                                userId,
                                                                                fileId,
                                                                                listSquareImageFileExtensionName,
                                                                                fileTags,
                                                                                listSquareImageSize.Width,
                                                                                listSquareImageSize.Height);
                                                                        ////////////////////////////////////////////////


                                                                        var listSquareImageFileProcessingBeginTime = DateTime.Now;
                                                                        ////////////////////////////////////////////////
                                                                        var listSquareImage = UploadController.ProcessImageFileWithSourceImage(
                                                                                image,
                                                                                listSquareImageSize,
                                                                                request?.IsWatermarkEnable == true,
                                                                                //null,
                                                                                filePathRuleMatched.GetWatermarkInfoForImage(
                                                                                        listSquareImageSize.Width,
                                                                                        listSquareImageSize.Height),
                                                                                watermarkCaption);
                                                                        ////////////////////////////////////////////////
                                                                        var listSquareImageFileProcessingEndTime = DateTime.Now;
                                                                        listSquareImageFileProcessingSeconds
                                                                        = (listSquareImageFileProcessingEndTime - listSquareImageFileProcessingBeginTime)
                                                                        .TotalSeconds;


                                                                        var listSquareImageFileStorageBeginTime = DateTime.Now;
                                                                        ////////////////////////////////////////////////
                                                                        var imageFileInfo
                                                                        = UploadController.SaveImageToFilePathAndCreateImageFileInfo(
                                                                            listSquareImage,
                                                                            listSquareImageFileAbsolutePath,
                                                                            listSquareImageFileAbsoluteUri,
                                                                            listSquareImageEncoder,
                                                                            listSquareImageSaveQuality);
                                                                        ////////////////////////////////////////////////
                                                                        var listSquareImageFileStorageEndTime = DateTime.Now;
                                                                        listSquareImageFileStorageSeconds
                                                                        = (listSquareImageFileStorageEndTime - listSquareImageFileStorageBeginTime)
                                                                        .TotalSeconds;

                                                                        // !!!
                                                                        response.ListSquareImageFileInfo = imageFileInfo;
                                                                        // !!!
                                                                }));


                                                        // 保存图片文件，【内容适中】图片：
                                                        tasksToSaveImageFile.Add(Task.Run(
                                                                () =>
                                                                {
                                                                        if (ISIImageFormatExtension.TryGetImageFormatWithFileExtensionName(
                                                                            filePathRuleMatched.ContentImageFormat,
                                                                            out var contentImageFormat,
                                                                            out var contentImageEncoder,
                                                                            out _) != true)
                                                                        {
                                                                                throw new ApplicationException("无法创建内容图片，指定的内容图片的格式无效。");
                                                                        }
                                                                        var contentImageFileExtensionName
                                                                        = contentImageFormat.FileExtensions.First();

                                                                        var contentImageSizeMax
                                                                        = filePathRuleMatched.ContentImageSizeMax;
                                                                        if (contentImageSizeMax == null)
                                                                        {
                                                                                contentImageSizeMax = new ImageSize();
                                                                        }
                                                                        var contentImageSize
                                                                        = new ImageSize(image.Width, image.Height)
                                                                        .SizeByZoomWithMaxSize(contentImageSizeMax);

                                                                        var contentImageFileAbsolutePath
                                                                        = filePathRuleMatched.GetContentImageFileAbsolutePathWithFileId(
                                                                                userId,
                                                                                fileId,
                                                                                contentImageFileExtensionName,
                                                                                fileTags,
                                                                                contentImageSize.Width,
                                                                                contentImageSize.Height);
                                                                        if (string.IsNullOrEmpty(contentImageFileAbsolutePath))
                                                                        {
                                                                                throw new ApplicationException("无法创建内容图片，指定的文件路径无效。");
                                                                        }

                                                                        var contentImageSaveQuality
                                                                        = filePathRuleMatched.ContentImageSaveQuality;

                                                                        var contentImageFileAbsoluteUri
                                                                        = filePathRuleMatched.GetContentImageFileAbsoluteUriWithFileId(
                                                                                userId,
                                                                                fileId,
                                                                                contentImageFileExtensionName,
                                                                                fileTags,
                                                                                contentImageSize.Width,
                                                                                contentImageSize.Height);
                                                                        ////////////////////////////////////////////////


                                                                        var contentImageFileProcessingBeginTime = DateTime.Now;
                                                                        ////////////////////////////////////////////////
                                                                        var contentImage = UploadController.ProcessImageFileWithSourceImage(
                                                                                image,
                                                                                contentImageSize,
                                                                                request?.IsWatermarkEnable == true,
                                                                                //null,
                                                                                filePathRuleMatched.GetWatermarkInfoForImage(
                                                                                        contentImageSize.Width,
                                                                                        contentImageSize.Height),
                                                                                watermarkCaption);
                                                                        ////////////////////////////////////////////////
                                                                        var contentImageFileProcessingEndTime = DateTime.Now;
                                                                        contentImageFileProcessingSeconds
                                                                        = (contentImageFileProcessingEndTime - contentImageFileProcessingBeginTime)
                                                                        .TotalSeconds;


                                                                        var contentImageFileStorageBeginTime = DateTime.Now;
                                                                        ////////////////////////////////////////////////
                                                                        var imageFileInfo
                                                                        = UploadController.SaveImageToFilePathAndCreateImageFileInfo(
                                                                            contentImage,
                                                                            contentImageFileAbsolutePath,
                                                                            contentImageFileAbsoluteUri,
                                                                            contentImageEncoder,
                                                                            contentImageSaveQuality);
                                                                        ////////////////////////////////////////////////
                                                                        var contentImageFileStorageEndTime = DateTime.Now;
                                                                        contentImageFileStorageSeconds
                                                                        = (contentImageFileStorageEndTime - contentImageFileStorageBeginTime)
                                                                        .TotalSeconds;

                                                                        // !!!
                                                                        response.ContentImageFileInfo = imageFileInfo;
                                                                        // !!!
                                                                }));
                                                }

                                                // !!!
                                                await Task.WhenAll(tasksToSaveImageFile.ToArray());
                                                // !!!


                                                ////////////////////////////////////////////////
                                                ////////////////////////////////////////////////
                                                // !!!
                                                fileUploadStatisticsInfo.SecondsToFileProcessing
                                                = Max.Of(
                                                        sourceImageFileProcessingSeconds
                                                        + listImageFileProcessingSeconds
                                                        + listSquareImageFileProcessingSeconds
                                                        + contentImageFileProcessingSeconds);
                                                fileUploadStatisticsInfo.SecondsToFileStorage
                                                = Max.Of(
                                                        sourceImageFileStorageSeconds,
                                                         listImageFileStorageSeconds,
                                                         listSquareImageFileStorageSeconds,
                                                         contentImageFileStorageSeconds);
                                                // !!!
                                                ////////////////////////////////////////////////
                                                ////////////////////////////////////////////////


                                                return response;
                                        });
                        }
                        catch (Exception exception)
                        {
                                Log.Exception.Logs(this, "上传图片文件失败，程序异常。", exception);
                                //
                                response.Error = Error.ProgramError;
                        }
                        return Json(response);
                }

                public class ImageFilterRequest
                {
                        public string? OriginalImageUrl { get; set; }

                        public string? FilterName { get; set; }

                        public bool IsValid
                        {
                                get
                                {
                                        if (this.OriginalImageUrl?.Length > 0
                                                && this.FilterName?.Length > 0)
                                        {
                                                return true;
                                        }
                                        return false;
                                }
                        }
                }

                public class ImageFilterResponse : ViewModels.Response
                {
                        public string? ImageUrlAfterImageFilter { get; set; }
                }

                [AuthorizationNotRequired]
                public async Task<IActionResult> ImageFilter([FromBody] ImageFilterRequest? request)
                {
                        var response = new ImageFilterResponse();
                        try
                        {
                                if (request?.IsValid == true
                                        && request.OriginalImageUrl?.Length > 0)
                                {
                                        if (ImageFilterNames.AiDaoRi.EqualsIgnoreCase(request.FilterName))
                                        {
                                                var imageUrlAfterImageFilter
                                                        = await _imageUriAfterImageFilter_AiDaoRi.GetAsync(
                                                                request.OriginalImageUrl,
                                                                request);
                                                {
                                                        // !!!
                                                        response.ImageUrlAfterImageFilter = imageUrlAfterImageFilter;
                                                        // !!!
                                                }
                                        }
                                        else
                                        {
                                                response.Error = Error.ObjectInvalid;
                                        }
                                }
                                else
                                {
                                        response.Error = Error.InvalidRequest;
                                }
                        }
                        catch (Exception exception)
                        {
                                Log.Exception.Logs(this, "应用图片滤镜失败，程序异常。", exception);
                                //
                                response.Error = Error.ProgramError;
                        }
                        return Json(response);
                }

                #endregion
        }
}
