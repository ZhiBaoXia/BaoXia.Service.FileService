using BaoXia.Utils;

namespace BaoXia.Service.FileService.LogFiles
{
        public class Log
        {
                /// <summary>
                /// 日常普通日志。
                /// </summary>
                public static readonly LogFile Logs = new("", "Log");

                /// <summary>
                /// Http请求日志。
                /// </summary>
                public static LogFile HttpRequest = new("Http请求日志", "HttpRequest");

                /// <summary>
                /// 用户操作日志。
                /// </summary>
                public static LogFile UserOperate = new("用户操作日志", "UserOperate");

                /// <summary>
                /// 开发调试日志。
                /// </summary>
                public static LogFile Debug = new("调试日志", "Debug");

                /// <summary>
                /// SQL语句日志。
                /// </summary>
                public static LogFile SQL = new("SQL", "SQL");

                /// <summary>
                /// 性能日志。
                /// </summary>
                public static LogFile Performance = new("性能日志", "Performance");

                /// <summary>
                /// 峰值日志。
                /// </summary>
                public static LogFile RPSPeak = new("峰值日志", "RPSPeak");

                /// <summary>
                /// 程序异常日志。
                /// </summary>
                public static LogFile Exception = new("异常日志", "Exception");

                /// <summary>
                /// 运营警告日志。
                /// </summary>
                public static LogFile Warning = new("⚠警告日志", "Warning");
        }
}
