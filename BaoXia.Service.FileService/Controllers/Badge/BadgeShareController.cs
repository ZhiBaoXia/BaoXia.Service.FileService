using BaoXia.Constants;
using BaoXia.Service.FileService.Extensions;
using BaoXia.Service.FileService.LogFiles;
using BaoXia.Service.FileService.Utils;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

namespace BaoXia.Service.FileService.Controllers.Badge
{
        [Route("/badge/shareCard_{__userId__}.{__imageType__}")]
        public class ShareCardController : Controller
        {

                ////////////////////////////////////////////////
                // @自身属性
                ////////////////////////////////////////////////

                #region 自身属性

                static ImageCard _shareImageCard = new("Resources/ShareCardFor_Badge/");

                #endregion


                ////////////////////////////////////////////////
                // @自身实现
                ////////////////////////////////////////////////
                #region 自身实现

                [HttpGet]
                public async Task<IActionResult> Create(
                        [FromQuery] int userId,
                        [FromQuery] string? imageType = null,
                        [FromQuery] int imageQuality = 100,
                        [FromQuery] string? fileDownloadName = null,
                        [FromQuery] bool isTestDataEnable = false)
                {
                        var response = new ViewModels.Response();
                        try
                        {
                                if (userId == 0)
                                {
                                        var imageTypeInPath = this.RouteData.Values["__userId__"] as string;
                                        int.TryParse(imageTypeInPath, out userId);
                                }
                                if (imageType == null
                                        || imageType.Length < 1)
                                {
                                        imageType = this.RouteData.Values["__imageType__"] as string;
                                }

                                var imageCardInfo = await _shareImageCard.Create<Models.UserBadgeListResponseBody>(
                                        imageType,
                                        imageQuality,
                                        fileDownloadName,
                                        isTestDataEnable,
                                        async (httpClient,
                                        imageDownloadFromAsync,
                                        isTestDataEnable) =>
                                        {
                                                var userBadgeListInfoResponse
                                                = await httpClient.GetAsync("https://api.baoxiaruanjian.com/@/userData/asset/getUserBadgeList?userId=" + userId);
                                                if (userBadgeListInfoResponse == null)
                                                {
                                                        return null;
                                                }

                                                var openAwardInfoResponseBodyString
                                                = userBadgeListInfoResponse.Content.ReadAsStringAsync();
                                                var userBadgeListResponse
                                                = await userBadgeListInfoResponse.Content.ReadFromJsonAsync<Models.UserBadgeListResponseBody>(
                                                        BaoXia.Utils.Environment.JsonSerializerOptions);
                                                if (userBadgeListResponse != null
                                                && isTestDataEnable)
                                                {
                                                        // userBadgeListResponse.OpenAwardTitle = "重返帝国标准版PSN激活码";
                                                        // userBadgeListResponse.PrizePrice = 76;
                                                        // userBadgeListResponse.OpenAwardDescription = "4月29日，天美旗下的SLG新游《重返帝国》迎来不删档上线一个月整，在这期间游戏不仅在TAPTAP上仍保持热搜第4的位置，同时贴吧的史官、攻略等优质内容也被玩家不断讨论。大众对《重返帝国》的印象正在从一个后来者，变为一匹直奔前方的黑马。哈哈❤";
                                                        // userBadgeListResponse.EndTime = "2小时3分钟后截止";
                                                        // userBadgeListResponse.NumberOfWinners = 2898;
                                                }

                                                if (userBadgeListResponse != null)
                                                {
                                                        var tasksToDownloadImage = new List<Task>();
                                                        var currentBadge = userBadgeListResponse.CurrentBadge;

                                                        // 用户头像
                                                        var headImageUrl = currentBadge?.UserAvatar;
                                                        if (headImageUrl?.Length > 0)
                                                        {
                                                                tasksToDownloadImage.Add(Task.Run(async () =>
                                                                {
                                                                        userBadgeListResponse.HeadImage = await imageDownloadFromAsync(headImageUrl);
                                                                }));
                                                        }

                                                        // 用户当前佩戴徽章
                                                        var badgeBigImageUrl = currentBadge?.BigImageUrl;
                                                        var badgeName = currentBadge?.BadgeName;
                                                        if (badgeBigImageUrl?.Length > 0
                                                                && badgeName != null)
                                                        {
                                                                tasksToDownloadImage.Add(Task.Run(async () =>
                                                                {
                                                                        userBadgeListResponse.CurrentBadgeBigImageUrl
                                                                        = await imageDownloadFromAsync(badgeBigImageUrl);
                                                                }));
                                                        }

                                                        // 用户徽章列表所有图片
                                                        var userBadgeList = userBadgeListResponse.List;
                                                        if (userBadgeList != null)
                                                        {
                                                                foreach (var userBadgeInfo in userBadgeList)
                                                                {
                                                                        if (userBadgeInfo.IsAcquired)
                                                                        {
                                                                                var userBadgeBigImageUrl = userBadgeInfo.BigImageUrl;
                                                                                tasksToDownloadImage.Add(Task.Run(async () =>
                                                                                {
                                                                                        var bigImage = await imageDownloadFromAsync(userBadgeInfo.BigImageUrl);
                                                                                        if (bigImage != null)
                                                                                        {
                                                                                                userBadgeInfo.BigImage = bigImage;
                                                                                        }
                                                                                }));
                                                                        }
                                                                }
                                                        }

                                                        // !!!
                                                        Task.WaitAll(tasksToDownloadImage.ToArray());
                                                        // !!!
                                                }
                                                return userBadgeListResponse;
                                        },
                                        (Models.UserBadgeListResponseBody? userBadgeListInfo,
                                                SKCanvas canvas,
                                                //
                                                int canvasWidth,
                                                int canvasHeightMax,
                                                //
                                                Padding canvasPadding,
                                                //
                                                SKTypeface fontFamily_SourceHanSansSC_Regular,
                                                SKTypeface fontFamily_SourceHanSansSC_Midium,
                                                SKTypeface fontFamily_SourceHanSansSC_Bold,
                                                //
                                                float textRenderOffsetY,
                                                //
                                                HttpClient httpClient,
                                                Func<string, SKBitmap> imageFileNamed,
                                                Func<uint, SKColor> colorFromArgbHex,
                                                Func<string, SKTypeface?> fontFamilyFileNamed) =>
                                        {
                                                // 当前佩戴徽章信息
                                                var currentBadge = userBadgeListInfo?.CurrentBadge;
                                                if (currentBadge == null)
                                                {
                                                        return 0;
                                                }

                                                // 当前已获得徽章信息
                                                var badgeList = userBadgeListInfo?.List;

                                                ////////////////////////////////////////////////
                                                // 获取活动信息
                                                ////////////////////////////////////////////////
                                                if (userBadgeListInfo == null)
                                                {
                                                        return 0;
                                                }

                                                var SKPaintDefault = new SKPaint()
                                                {
                                                        IsAntialias = true,
                                                        FilterQuality = SKFilterQuality.High,
                                                        HintingLevel = SKPaintHinting.Normal,
                                                        Typeface = fontFamily_SourceHanSansSC_Regular
                                                };

                                                ////////////////////////////////////////////////
                                                // 初始化画布。
                                                ////////////////////////////////////////////////

                                                canvas.Clear(colorFromArgbHex(0xFF222226));

                                                ////////////////////////////////////////////////
                                                // 绘制：头部背景图。
                                                ////////////////////////////////////////////////

                                                {
                                                        var topImage
                                                                = imageFileNamed("image_Badge_Top.png");
                                                        var topImageFrame
                                                                = SKRect.Create(0, 0, canvasWidth, 232);
                                                        canvas.DrawBitmap(
                                                                topImage,
                                                                topImageFrame,
                                                                SKPaintDefault);

                                                }

                                                ////////////////////////////////////////////////
                                                // 绘制：头像、用户昵称。
                                                ////////////////////////////////////////////////
                                                SKRect userHeadImageFrame;
                                                {
                                                        // 头像占位填充
                                                        var userHeadImagePaint_Fill = new SKPaint()
                                                        {
                                                                IsAntialias = true,
                                                                Style = SKPaintStyle.Fill,
                                                                Color = SKColors.LightGray,
                                                                StrokeWidth = 2.0F
                                                        };

                                                        // 头像占位轮廓
                                                        var userHeadImagePaint_Stroke = new SKPaint()
                                                        {
                                                                IsAntialias = true,
                                                                Style = SKPaintStyle.Stroke,
                                                                Color = SKColors.White,
                                                                StrokeWidth = 2.0F,
                                                        };

                                                        // 头像框架填充
                                                        var userHeadImageFrameRadius = 40.0F;
                                                        userHeadImageFrame = SKRect.Create(
                                                                146,
                                                                96,
                                                                userHeadImageFrameRadius * 2,
                                                                userHeadImageFrameRadius * 2);
                                                        SKRect userHeadImageBorderFrame = new(
                                                                userHeadImageFrame.Left,
                                                                userHeadImageFrame.Top,
                                                                userHeadImageFrame.Right,
                                                                userHeadImageFrame.Bottom);
                                                        {
                                                                userHeadImageBorderFrame.Inflate(0.0F, 0.0F);
                                                        }

                                                        canvas.DrawCircle(
                                                                userHeadImageFrame.MidX,
                                                                userHeadImageFrame.MidY,
                                                                userHeadImageFrameRadius,
                                                                userHeadImagePaint_Fill);

                                                        // 头像图
                                                        var userHeadImage = userBadgeListInfo.HeadImage;
                                                        if (userHeadImage != null)
                                                        {
                                                                canvas.Save();
                                                                {
                                                                        var clipPath = new SKPath();
                                                                        {
                                                                                clipPath.AddCircle(
                                                                                        userHeadImageFrame.MidX,
                                                                                        userHeadImageFrame.MidY,
                                                                                        userHeadImageFrameRadius);
                                                                        }
                                                                        canvas.ClipPath(
                                                                                clipPath,
                                                                                SKClipOperation.Intersect,
                                                                                true);
                                                                        // !!!
                                                                        canvas.DrawBitmap(
                                                                                userHeadImage,
                                                                                userHeadImageFrame);
                                                                        // !!!
                                                                }
                                                                canvas.Restore();
                                                        }

                                                        // 头像框架轮廓
                                                        canvas.DrawCircle(
                                                                userHeadImageBorderFrame.MidX,
                                                                userHeadImageBorderFrame.MidY,
                                                                userHeadImageBorderFrame.Width / 2,
                                                                userHeadImagePaint_Stroke);

                                                        // 用户昵称
                                                        var userName = currentBadge.UserName;
                                                        if (userName?.Length > 0)
                                                        {
                                                                var skPaint = SKPaintDefault.Clone();
                                                                {
                                                                        skPaint.Typeface = fontFamily_SourceHanSansSC_Bold;
                                                                        skPaint.TextSize = 14;
                                                                        skPaint.Color = colorFromArgbHex(0xFFFFFFFD);
                                                                }
                                                                SKRect textBounds = new();
                                                                skPaint.MeasureText(userName, ref textBounds);
                                                                var textPosition = new SKPoint(
                                                                        userHeadImageFrame.MidX - textBounds.Width / 2,
                                                                        userHeadImageFrame.MidY + 50);
                                                                canvas.DrawTextToLeftTop(userName, textPosition, skPaint);
                                                        }
                                                }

                                                ////////////////////////////////////////////////
                                                // 绘制：已点亮徽章数量信息。
                                                ////////////////////////////////////////////////
                                                {
                                                        // 左引号
                                                        {
                                                                var yinhaoleft = imageFileNamed("yinhaoleft.png");
                                                                var yinhaoleftFrame = SKRect.Create(110, 225, 14, 14);
                                                                canvas.DrawBitmap(yinhaoleft, yinhaoleftFrame, SKPaintDefault);

                                                        }
                                                        var badgeCountDesc1 = "已点亮";
                                                        var skPaint = SKPaintDefault.Clone();
                                                        {
                                                                skPaint.Typeface = fontFamily_SourceHanSansSC_Regular;
                                                                skPaint.TextSize = 16;
                                                                skPaint.Color = colorFromArgbHex(0xFF999999);
                                                        }
                                                        var textPosition = new SKPoint(110 + 18, 225);
                                                        canvas.DrawTextToLeftTop(badgeCountDesc1, textPosition, skPaint);

                                                        var badgeCount = currentBadge.BadgeCount + "";
                                                        // badgeCount = "10";
                                                        skPaint = SKPaintDefault.Clone();
                                                        {
                                                                skPaint.Typeface = fontFamily_SourceHanSansSC_Midium;
                                                                skPaint.TextSize = 20;
                                                                skPaint.Color = colorFromArgbHex(0xFFFFFFFD);
                                                        }
                                                        SKRect textBounds = new();
                                                        skPaint.MeasureText(badgeCount, ref textBounds);
                                                        textPosition = new SKPoint(188 - textBounds.Width / 2, 223);
                                                        canvas.DrawTextToLeftTop(badgeCount, textPosition, skPaint);

                                                        var badgeCountDesc2 = "个徽章";
                                                        skPaint = SKPaintDefault.Clone();
                                                        {
                                                                skPaint.Typeface = fontFamily_SourceHanSansSC_Regular;
                                                                skPaint.TextSize = 16;
                                                                skPaint.Color = colorFromArgbHex(0xFF999999);
                                                        }
                                                        textPosition = new SKPoint(252 - 8 - 48 + 4, 225);
                                                        canvas.DrawTextToLeftTop(badgeCountDesc2, textPosition, skPaint);


                                                        // 右引号
                                                        {
                                                                var yinhaoright = imageFileNamed("yinhaoright.png");
                                                                var yinhaorightFrame = SKRect.Create(252, 225, 14, 14);
                                                                canvas.DrawBitmap(yinhaoright, yinhaorightFrame, SKPaintDefault);

                                                        }
                                                }

                                                ////////////////////////////////////////////////
                                                // 绘制：已点亮徽章列表。
                                                ////////////////////////////////////////////////
                                                {
                                                        var userBadgeList = userBadgeListInfo.List;
                                                        if (userBadgeList != null)
                                                        {
                                                                int badgeIndex = 0;
                                                                int bigImageUrlX = 0;
                                                                int bigImageUrlY = 0;
                                                                float badgeTopicNameX = 0;
                                                                int badgeTopicNameY = 0;
                                                                float badgeLevelX = 0;
                                                                int badgeLevelY = 0;
                                                                float activateTimeX = 0;
                                                                float activateTimeY = 0;
                                                                foreach (var userBadgeInfo in userBadgeList)
                                                                {
                                                                        if (userBadgeInfo.IsAcquired)
                                                                        {
                                                                                bigImageUrlX = 22 + badgeIndex % 3 * 125;
                                                                                bigImageUrlY = 308 + badgeIndex / 3 * 169;
                                                                                badgeTopicNameX = 62.5F + badgeIndex % 3 * 125;
                                                                                badgeTopicNameY = 396 + badgeIndex / 3 * 169;
                                                                                badgeLevelX = 62.5F + badgeIndex % 3 * 125;
                                                                                badgeLevelY = 417 + badgeIndex / 3 * 169;
                                                                                activateTimeX = 62.5F + badgeIndex % 3 * 125;
                                                                                activateTimeY = 437 + badgeIndex / 3 * 169;
                                                                                {
                                                                                        // 绘制图标
                                                                                        var bigImageUrl = userBadgeInfo.BigImage;
                                                                                        canvas.DrawBitmap(
                                                                                                bigImageUrl,
                                                                                                SKRect.Create(bigImageUrlX, bigImageUrlY, 80, 80),
                                                                                                SKPaintDefault);

                                                                                        // 绘制主题名称
                                                                                        var topicName = userBadgeInfo.TopicName;
                                                                                        var skPaint = SKPaintDefault.Clone();
                                                                                        {
                                                                                                skPaint.Typeface = fontFamily_SourceHanSansSC_Regular;
                                                                                                skPaint.TextSize = 12;
                                                                                                skPaint.Color = colorFromArgbHex(0xFFFFFFFD);
                                                                                        }
                                                                                        SKRect textBounds = new();
                                                                                        skPaint.MeasureText(topicName, ref textBounds);
                                                                                        var textPosition = new SKPoint(badgeTopicNameX - textBounds.Width / 2, badgeTopicNameY);
                                                                                        canvas.DrawTextToLeftTop(topicName, textPosition, skPaint);
                                                                                        // 绘制主题等级
                                                                                        var level = "Lv" + userBadgeInfo.Level;
                                                                                        skPaint = SKPaintDefault.Clone();
                                                                                        {
                                                                                                skPaint.Typeface = fontFamily_SourceHanSansSC_Midium;
                                                                                                skPaint.TextSize = 12;
                                                                                                skPaint.Color = colorFromArgbHex(0xFFFFFFFD);
                                                                                        }
                                                                                        textBounds = new();
                                                                                        skPaint.MeasureText(level, ref textBounds);
                                                                                        textPosition = new SKPoint(badgeLevelX - textBounds.Width / 2, badgeLevelY);
                                                                                        canvas.DrawTextToLeftTop(level, textPosition, skPaint);
                                                                                        // 绘制点亮时间
                                                                                        var createTime = userBadgeInfo.CreateTime;
                                                                                        skPaint = SKPaintDefault.Clone();
                                                                                        {
                                                                                                skPaint.Typeface = fontFamily_SourceHanSansSC_Regular;
                                                                                                skPaint.TextSize = 10;
                                                                                                skPaint.Color = colorFromArgbHex(0xFF666666);
                                                                                        }
                                                                                        textBounds = new();
                                                                                        skPaint.MeasureText(createTime, ref textBounds);
                                                                                        textPosition = new SKPoint(activateTimeX, activateTimeY);
                                                                                        textPosition = new SKPoint(activateTimeX - textBounds.Width / 2, activateTimeY);
                                                                                        canvas.DrawTextToLeftTop(createTime, textPosition, skPaint);
                                                                                }
                                                                                badgeIndex++;
                                                                        }
                                                                }
                                                        }
                                                }


                                                //////////////////////////////////////////////////
                                                //// 绘制：绘制页脚。
                                                //////////////////////////////////////////////////
                                                var row = currentBadge.BadgeCount % 3 == 0
                                                        ? currentBadge.BadgeCount / 3
                                                        : currentBadge.BadgeCount / 3 + 1;
                                                SKRect pageFooterFrame = SKRect.Create(
                                                        0,
                                                        308 + row * 169 + 10,
                                                        canvasWidth,
                                                        78);
                                                {
                                                        var image_PageFooter_375x78
                                                        = imageFileNamed("image_Badge_PageFooter_375x78@3x.png");
                                                        if (pageFooterFrame.Bottom > canvasHeightMax)
                                                        {
                                                                pageFooterFrame.Top = canvasHeightMax - pageFooterFrame.Height;
                                                        }


                                                        canvas.Save();
                                                        {
                                                                canvas.ClipRect(pageFooterFrame);
                                                                canvas.Clear();

                                                                canvas.DrawBitmap(
                                                                        image_PageFooter_375x78,
                                                                        pageFooterFrame,
                                                                        SKPaintDefault);
                                                        }
                                                        canvas.Restore();
                                                }
                                                return pageFooterFrame.Bottom;
                                        }
                                        );
                                if (imageCardInfo?.TryEndResponse(
                                        this.Response,
                                        out var actionResult) == true)
                                {
                                        return actionResult;
                                }
                                response.Error = Error.InvalidRequest;
                        }
                        catch (Exception exception)
                        {
                                Log.Exception.Logs(this, "创建分享卡片失败，程序异常。", exception);
                                //
                                response.Error = Error.ProgramError;
                        }
                        return Json(response);
                }

                #endregion
        }
}
