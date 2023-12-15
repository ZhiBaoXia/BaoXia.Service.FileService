using BaoXia.Utils.Extensions;
using BaoXia.Service.FileService.LogFiles;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;
using System.Diagnostics.CodeAnalysis;
using BaoXia.Service.FileService.Constants;

namespace BaoXia.Service.FileService.Utils;

public class ImageCardInfo
{

        ////////////////////////////////////////////////
        // @自身属性
        ////////////////////////////////////////////////

        #region 自身属性

        public String? ImageType { get; set; }

        public int ImageQuality { get; set; }

        public String? FileDownloadName { get; set; }

        public SKImage? ImageCard { get; set; }
        public double SecondsToGetData { get; set; }

        public double SecondsToRenderCard { get; set; }

        #endregion


        ////////////////////////////////////////////////
        // @自身实现
        ////////////////////////////////////////////////

        #region 自身实现

        public bool TryEndResponse(
                HttpResponse httpResponse,
                [NotNullWhen(true)]
                out IActionResult? actionResult)
        {
                actionResult = null;

                httpResponse.Headers[HttpHeaderKeys.SecondsToGetImageCardData] = this.SecondsToGetData.ToString("F3");
                httpResponse.Headers[HttpHeaderKeys.SecondsToRenderImageCard] = this.SecondsToRenderCard.ToString("F3");

                var imageCard = this.ImageCard;
                if (imageCard == null)
                {
                        Log.Debug.Logs(this, "生成图片卡片失败，“imageCard”为“null”。");

                        return false;
                }

                byte[] imageCardBytes;
                string imageCardMIME;
                var imageType = this.ImageType;
                var imageQuality = this.ImageQuality;
                {
                        if (imageQuality < 0)
                        {
                                imageQuality = 0;
                        }
                        else if (imageQuality > 100)
                        {
                                imageQuality = 100;
                        }
                }
                if ("webp".EqualsIgnoreCase(imageType))
                {
                        imageCardBytes = imageCard.Encode(SKEncodedImageFormat.Webp, imageQuality).ToArray();
                        imageCardMIME = "image/webp";
                }
                else if ("jpg".EqualsIgnoreCase(imageType)
                        || "jpeg".EqualsIgnoreCase(imageType))
                {
                        imageCardBytes = imageCard.Encode(SKEncodedImageFormat.Jpeg, imageQuality).ToArray();
                        imageCardMIME = "image/jpg";
                }
                else if ("gif".EqualsIgnoreCase(imageType))
                {
                        imageCardBytes = imageCard.Encode(SKEncodedImageFormat.Gif, imageQuality).ToArray();
                        imageCardMIME = "image/gif";
                }
                else
                {
                        imageCardBytes = imageCard.Encode(SKEncodedImageFormat.Png, imageQuality).ToArray();
                        imageCardMIME = "image/png";
                }


                // !!!
                var fileContentResult = new FileContentResult(
                        imageCardBytes,
                        imageCardMIME);
                // !!!
                {
                        fileContentResult.FileDownloadName = this.FileDownloadName;
                }
                actionResult = fileContentResult;

                return true;
        }

        #endregion

}
