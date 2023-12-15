using BaoXia.Utils.Extensions;
using BaoXia.Service.FileService.ConfigFiles;
using BaoXia.Service.FileService.Constants;
using BaoXia.Service.FileService.LogFiles;
using BaoXia.Service.FileService.Models;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace BaoXia.Service.FileService.Middleware
{
        public class CrossDomainControlMiddleware
        {
                ////////////////////////////////////////////////
                // @自身属性
                ////////////////////////////////////////////////

                #region 自身属性			

                private readonly RequestDelegate _nextRequestDelegate;

                public Func<CrossDomainControlInfo?> ToGetCrossDomainControlInfo { get; set; }

                #endregion



                public CrossDomainControlMiddleware(
                        RequestDelegate nextRequestDelegate,
                        Func<CrossDomainControlInfo?> toGetCrossDomainControlInfo)
                {
                        _nextRequestDelegate = nextRequestDelegate;

                        this.ToGetCrossDomainControlInfo = toGetCrossDomainControlInfo;
                }

                public bool IsAccessControllAllowOrigin(string? domain)
                {
                        if (domain == null
                                || domain.Length < 1)
                        {
                                // !!!⚠ 没有明确的来源信息时，默认允许请求 ⚠!!!
                                return true;
                        }
                        var controlInfo = this.ToGetCrossDomainControlInfo();
                        if (controlInfo == null)
                        {
                                // !!!⚠ 没有明确的控制信息时，默认允许请求 ⚠!!!
                                return true;
                        }

                        if (controlInfo.IsAnyDomainAccessControllAllow == true)
                        {
                                return true;
                        }

                        var domainHost = domain.SubstringBetween("://", "/");
                        if (domainHost == null)
                        {
                                domainHost = domain;
                        }
                        domainHost = domainHost.SubstringBetween(null, ":");

                        if (domainHost?.Length > 0)
                        {
                                var credibleDomains = controlInfo.Access_Control_Allow_Origins;
                                if (credibleDomains?.Length > 0)
                                {
                                        foreach (var credibleDomain
                                            in
                                            credibleDomains)
                                        {
                                                if (domainHost.EndsWith(
                                                        credibleDomain,
                                                        StringComparison.OrdinalIgnoreCase)
                                                        == true)
                                                {
                                                        return true;
                                                }
                                        }
                                }
                        }
                        return false;
                }


                ////////////////////////////////////////////////
                // @实现“Middleware”定义方法
                ////////////////////////////////////////////////

                #region 实现“Middleware”

                public async Task InvokeAsync(HttpContext httpContext)
                {
                        ////////////////////////////////////////////////
                        // 1/2，跨域控制：
                        ////////////////////////////////////////////////
                        try
                        {
                                var request = httpContext.Request;
                                {
                                        var requestHeaders = request.Headers;
                                        var response = httpContext.Response;
                                        var responseHeaders = response.Headers;
                                        string? requestOrigin = null;
                                        var requestOriginHeaderValues = requestHeaders["Origin"];
                                        if (requestOriginHeaderValues.Count > 0)
                                        {
                                                requestOrigin = requestOriginHeaderValues[0];
                                        }

                                        var isValidOrigin = this.IsAccessControllAllowOrigin(requestOrigin);
                                        if (isValidOrigin)
                                        {
                                                response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                                                response.Headers.Add(
                                                    "Access-Control-Allow-Origin",
                                                    requestOrigin);
                                        }
                                        else
                                        {
                                                response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                                        }

                                        var controlInfo = this.ToGetCrossDomainControlInfo();
                                        if (controlInfo != null)
                                        {
                                                responseHeaders.Add(
                                                    "Access-Control-Allow-Methods",
                                                    controlInfo.Access_Control_Allow_Methods);

                                                responseHeaders.Add(
                                                    "Access-Control-Allow-Headers",
                                                    controlInfo.Access_Control_Allow_Headers);

                                                responseHeaders.Add(
                                                    "Access-Control-Allow-Credentials",
                                                    controlInfo.Access_Control_Allow_Credentials
                                                    ? "true"
                                                    : "false");
                                        }

                                        if (System.Net.Http.HttpMethod
                                                .Options
                                                .Method
                                                .EqualsIgnoreCase(request.Method))
                                        {
                                                return;
                                        }
                                }
                        }
                        catch (Exception exception)
                        {
                                Log.Exception.Logs(this, "跨域访问控制失败，程序异常。", exception, "Router");
                                //
                                return;
                        }


                        ////////////////////////////////////////////////
                        // 2/2，继续响应通道：
                        ////////////////////////////////////////////////
                        // !!!
                        await _nextRequestDelegate(httpContext);
                        // !!!
                }

                #endregion

        }
}
