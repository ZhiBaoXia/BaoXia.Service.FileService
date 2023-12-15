using BaoXia.Utils.Extensions;

namespace BaoXia.Service.FileService.Models
{
        public class CrossDomainControlInfo
        {
                protected string[]? _access_Control_Allow_Origins = new string[] { "*" };

                /// <summary>
                /// 请求，可信任的来源域名数组。
                /// </summary>
                public string[]? Access_Control_Allow_Origins
                {
                        get
                        {
                                return _access_Control_Allow_Origins;
                        }
                        set
                        {
                                _isAnyDomainAccessControllAllow = false;
                                _access_Control_Allow_Origins = value;
                                if (_access_Control_Allow_Origins?.Length > 0)
                                {
                                        foreach (var credibleDomain
                                            in
                                            _access_Control_Allow_Origins)
                                        {
                                                if (credibleDomain.EqualsIgnoreCase("*"))
                                                {
                                                        // !!!
                                                        _isAnyDomainAccessControllAllow = true;
                                                        // !!!
                                                        break;
                                                }
                                        }
                                }
                        }
                }

                protected bool _isAnyDomainAccessControllAllow = true;

                /// <summary>
                /// 是否任意域名都是可信任的。
                /// </summary>
                public bool IsAnyDomainAccessControllAllow
                {
                        get
                        {
                                return _isAnyDomainAccessControllAllow;
                        }
                }

                /// <summary>
                /// 请求，允许的请求方法字符串，会直接返回给客户端的“OPTION”请求，
                /// 以英文逗号分隔的请求方法名，如：POST,GET。
                /// </summary>
                public string? Access_Control_Allow_Methods { get; set; } = "POST,GET,OPTIONS,DELETE";

                /// <summary>
                /// 请求，允许的HTTP头部参数，会直接返回给客户端的“OPTION”请求。
                /// </summary>
                public string? Access_Control_Allow_Headers { get; set; } = "Accept,Accept-Language,Content-Language,Content-Type,X-Custom-Header,Cookie,User-Agent";

                /// <summary>
                /// 请求，是否使用凭证进行实际请求。
                /// </summary>
                public bool Access_Control_Allow_Credentials { get; set; } = true;
        }
}
