using BaoXia.Service.FileService.Models;
using static BaoXia.Service.FileService.Middleware.CrossDomainControlMiddleware;

namespace BaoXia.Service.FileService.Middleware
{
        public static class CrossDomainControlMiddlewareExtension
        {
                public static IApplicationBuilder UseCrossDomainControl(
                    this IApplicationBuilder builder,
                    Func<CrossDomainControlInfo?> toGetCrossDomainControlInfo)
                {
                        return builder.UseMiddleware<CrossDomainControlMiddleware>(toGetCrossDomainControlInfo);
                }
        }
}
