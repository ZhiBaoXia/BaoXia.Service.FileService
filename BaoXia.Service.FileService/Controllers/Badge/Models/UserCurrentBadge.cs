namespace BaoXia.Service.FileService.Controllers.Badge.Models;

public class UserCurrentBadge
{
        public int UserId { get; set; }

        public string? UserName { get; set; }

        public string? UserAvatar { get; set; }

        public int BadgeCount { get; set; }

        public int BadgeId { get; set; }

        public string? BadgeName { get; set; }

        public string? BigImageUrl { get; set; }

        public string? SmallImageUrl { get; set; }
}