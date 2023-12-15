using BaoXia.Service.FileService.ConfigFiles;
using BaoXia.Service.FileService.Constants;
using BaoXia.Service.FileService.Data;
using BaoXia.Service.FileService.Middleware;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var builderConfiguration = builder.Configuration;
var builderServices = builder.Services;

////////////////////////////////////////////////
// 初始化基础工具：
////////////////////////////////////////////////
BaoXia.Utils.Environment.InitializeBeforeConfigureServicesWithServerName(
        "BaoXia.Service.FileService",
        BaoXia.Utils.Environment.GetEnvironmentNameWith_ASPNETCORE_ENVIRONMENT(),
        Passwords.AESKey,
        EnvironmentParams.ConfigFilesDirectoryName,
        EnvironmentParams.LogFilesDirectoryName);

////////////////////////////////////////////////
// 增加EF服务：
////////////////////////////////////////////////
{
	var sqlConnectionString
                = builderConfiguration.GetConnectionString("FileInfoDbContext");
        if (sqlConnectionString?.Length > 0)
        {
                builderServices.AddDbContext<FileInfoDbContext>(
                                        options =>
                                        {
                                                options.UseSqlServer(sqlConnectionString);
                                        });
        }
}

// Add services to the container.
builder.Services
        .Configure<FormOptions>(options =>
        {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = long.MaxValue;
                options.MultipartHeadersLengthLimit = int.MaxValue;
        })
        .AddControllersWithViews(
        (config) =>
        {});





#if WEBSERVER_IIS

        builder.WebHost.UseIIS();

#elif WEBSERVER_KESTREL

builder.WebHost.ConfigureKestrel(kestrelOptions =>
        {
                kestrelOptions.Limits.MaxRequestBodySize = int.MaxValue;

                kestrelOptions.Limits.MaxConcurrentConnections = null;
                kestrelOptions.Limits.MaxConcurrentUpgradedConnections = null;

                //kestrelOptions.Limits.RequestHeadersTimeout = null;
                kestrelOptions.Limits.MaxRequestBodySize = null;
                kestrelOptions.Limits.MinRequestBodyDataRate = null;
                //new MinDataRate(bytesPerSecond: 100,
                //    gracePeriod: TimeSpan.FromSeconds(10));

                kestrelOptions.Limits.MinResponseDataRate = null;
                //new MinDataRate(bytesPerSecond: 100,
                //        gracePeriod: TimeSpan.FromSeconds(10));

                //kestrelOptions.Limits.KeepAliveTimeout = null;

                var httpIPAddress = IPAddress.Any;
                if (IPAddress.TryParse(Config.Service.HttpIPAddress, out var httpIPAddressInConfig))
                {
                        httpIPAddress = httpIPAddressInConfig;
                }
                var httpPort = Config.Service.HttpPort;
                if (httpPort <= 0)
                {
                        httpPort = 80;
                }
                kestrelOptions.Listen(
                                httpIPAddress,
                                httpPort);

                var httpsIPAddress = IPAddress.Any;
                if (IPAddress.TryParse(Config.Service.HttpsIPAddress, out var httpsIPAddressInConfig))
                {
                        httpsIPAddress = httpsIPAddressInConfig;
                }
                var httpsPort = Config.Service.HttpsPort;
                if (httpsPort <= 0)
                {
                        httpsPort = 443;
                }
                kestrelOptions.Listen(
                        httpsIPAddress,
                        httpsPort,
                        listenOptions =>
                        {
                                var httpsCertificateFilePath = Config.Service.HttpsCertificateFilePath;
                                if (httpsCertificateFilePath?.Length > 0)
                                {
                                        if (httpsCertificateFilePath.IndexOf(":\\") < 0)
                                        {
                                                httpsCertificateFilePath
                                                = BaoXia.Utils.Environment.ApplicationDirectoryPath
                                                + httpsCertificateFilePath;
                                        }
                                        if (System.IO.File.Exists(httpsCertificateFilePath))
                                        {
                                                listenOptions.UseHttps(
                                                httpsCertificateFilePath,
                                                Config.Service.HttpCertificatePassword);
                                        }
                                }
                        });
        });

#endif


////////////////////////////////////////////////
// 初始化基础工具：
////////////////////////////////////////////////
var app = builder.Build();
BaoXia.Utils.Environment.InitializeAfterConfigureServices(
        app,
        app.Environment,
        app.Environment.WebRootPath);


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
}


//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCrossDomainControl(() =>
{
        return Config.Service.CrossDomainControlInfo;
});
app.UseRouting();
//app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Use(async (context, next) =>
{
        var request = context.Request;
        var requestContentLength = request.Headers.ContentLength;
        if (requestContentLength > Config.Service.FileUploadSizeInBytesMax)
        {
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.RequestEntityTooLarge;

                await context.Response.CompleteAsync();
        }
        else
        {
                await next.Invoke();
        }
});

app.Run();