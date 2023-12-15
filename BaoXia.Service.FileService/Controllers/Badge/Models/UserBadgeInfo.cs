using SkiaSharp;

namespace BaoXia.Service.FileService.Controllers.Badge.Models;

public class UserBadgeInfo
{
        public int TopicId { get; set; }

        public string? TopicName { get; set; }

        public int OrderNum { get; set; }

        public int BadgeId { get; set; }

        public string? BadgeName { get; set; }

        public string? Description { get; set; }

        public string? ObtainRemark { get; set; }

        public int Level { get; set; }

        public string? BigImageUrl { get; set; }

        public SKBitmap? BigImage { get; set; }

        public string? SmallImageUrl { get; set; }

        public string? InformImageUrl { get; set; }

        public string? CreateTime { get; set; }

        public bool IsAcquired { get; set; }

        public bool IsActivated { get; set; }
}