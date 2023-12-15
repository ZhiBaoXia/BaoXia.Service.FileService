using SkiaSharp;

namespace BaoXia.Service.FileService.Controllers.Badge.Models
{
        public class UserBadgeListResponseBody
        {
                ////////////////////////////////////////////////
                // 当前佩戴的徽章信息。
                ////////////////////////////////////////////////
                public UserCurrentBadge? CurrentBadge { get; set; }

                ////////////////////////////////////////////////
                // 所有。
                ////////////////////////////////////////////////
                public UserBadgeInfo[]? List { get; set; }
                
                public SKBitmap? HeadImage { get; set; }
                
                public SKBitmap? CurrentBadgeBigImageUrl { get; set; }
                
                
                public List<SKBitmap>? badgeListBigImageUrlList { get; set; }
        }
}
