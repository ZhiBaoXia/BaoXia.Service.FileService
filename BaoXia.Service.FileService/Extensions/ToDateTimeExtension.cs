namespace BaoXia.Service.FileService.Extensions
{
    public static class ToDateTimeExtension
    {
        public static string ToValorantDateTime(this DateTime time)
        {

            if (time == DateTime.MinValue)
            {
                return "";
            }
            return time.ToString("yyyy.MM.dd");
        }
    }
}
