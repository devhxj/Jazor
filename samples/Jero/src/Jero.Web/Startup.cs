using AspNetCoreRateLimit;
using Furion;
using Furion.Logging;
using Furion.SpecificationDocument;
using Furion.VirtualFileServer;
using IPTools.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.OpenApi;
using OnceMi.AspNetCore.OSS;
using Scalar.AspNetCore;
using SixLabors.ImageSharp.Web.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using Jero.Core;
using Jero.Core.ElasticSearch;
using Jero.Core.Service;
using Jero.Core.Update;
using Jero.Web.Handlers;
using Jero.Plugin.SaveLife.Service;
using FurionApp = Furion.App;

namespace Jero.Web;

[AppStartup(int.MaxValue)]
public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddProjectOptions();
        services.AddRequestTimeouts();

        services.AddCache();
        services.AddSqlSugar();
        AddJeroAuthentication(services);

        services.AddCorsAccessor();
        services.AddHttpRemote();
        services.AddTaskQueue();
        ConfigureJobScheduleServices(services, FurionApp.Configuration);
        services.AddSensitiveDetection();

        AddApi(services);

        services.AddOAuth();
        services.AddElasticSearchClients();

        // 配置Nginx转发获取客户端真实IP
        // 注1：如果负载均衡不是在本机通过 Loopback 地址转发请求的，一定要加上options.KnownNetworks.Clear()和options.KnownProxies.Clear()
        // 注2：如果设置环境变量 ASPNETCORE_FORWARDEDHEADERS_ENABLED 为 True，则不需要下面的配置代码
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        AddEventBus(services);

        services.AddImageSharp();

        var ossOptions = FurionApp.GetConfig<OSSProviderOptions>("OSSProvider", true);
        services.AddOSSService(Enum.GetName(ossOptions.Provider), "OSSProvider");

        services.AddTransient<SysFileProviderService>();
        services.AddSingleton<IOSSServiceManager, OSSServiceManager>(); // 改为单例以保持缓存
        services.AddTransient<MultiOSSFileProvider>();

        services.AddViewEngine();

        AddRealtime(services);

        services.AddLoggingSetup();
        services.AddCaptcha();
        services.AddConsoleLogo();

        // 将IP地址数据库文件完全加载到内存，提升查询速度（以空间换时间，内存将会增加60-70M）
        IpToolSettings.LoadInternationalDbToMemory = true;
        IpToolSettings.DefalutSearcherType = IpSearcherType.International;

        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
            [
                    "text/html; charset=utf-8",
                    "application/xhtml+xml",
                    "application/atom+xml",
                    "image/svg+xml"
             ]);
        });

        services.AddVirtualFileServer();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseResponseCompression();
        app.UseForwardedHeaders();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.Use((context, next) =>
        {
            context.Response.Headers.Append("Jero", "Jero");
            // Static HTML and the SPA fallback both set Content-Type later in the pipeline.
            context.Response.OnStarting(() =>
            {
                ApplyHtmlCacheHeaders(context.Response);
                return Task.CompletedTask;
            });

            return next();
        });

        UseFileHosting(app);

        app.UseOAuth();
        app.UseUnifyResultStatusCodes();

        // Furion localization must run before endpoint routing.
        app.UseAppLocalization();

        app.UseRouting();
        app.UseRequestTimeouts();

        // 启用跨域，必须在 UseRouting 和 UseAuthentication 之间注册
        app.UseCorsAccessor();

        app.UseAuthentication();
        app.UseAuthorization();

        // 限流组件（在跨域之后）
        app.UseIpRateLimiting();
        app.UseClientRateLimiting();
        app.UsePolicyRateLimit();

        UseScheduleDashboard(app);

        app.UseInject("swagger", options =>
            options.ConfigureSwagger(swagger => swagger.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0));

        app.UseAutoVersionUpdate();
        MapEndpoints(app);

        // 仅在 API、Swagger、Schedule、ReZero 等服务端 endpoint 都未处理请求后返回 Jolt 壳。
        app.UseSpaFallback();
    }

    private static void AddJeroAuthentication(IServiceCollection services)
    {
        // SignalR clients can supply their JWT in the query string.
        services.AddJwt<JwtHandler>(enableGlobalAuthorize: true, jwtBearerConfigure: options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var httpContext = context.HttpContext;
                    // 若请求 Url 包含 token 参数，则设置 Token 值
                    if (httpContext.Request.Query.ContainsKey("token"))
                        context.Token = httpContext.Request.Query["token"];
                    return Task.CompletedTask;
                }
            };
        }).AddSignatureAuthentication(options =>  // 添加 Signature 身份验证
        {
            options.Events = SysOpenAccessService.GetSignatureAuthenticationEventImpl();
        });
    }

    private static void AddApi(IServiceCollection services)
    {
        services.AddControllersWithViews()
            .AddAppLocalization()
            .AddInjectWithUnifyResult<AdminResultProvider>()
            .AddJsonOptions(options => WebProtocols.ConfigureJson(options.JsonSerializerOptions));

        services.Configure<SwaggerGenOptions>(WebProtocols.ConfigureSchemas);
    }

    private static void AddEventBus(IServiceCollection services)
    {
        services.AddEventBus(options =>
        {
            options.UseUtcTimestamp = false;
            options.LogEnabled = false;
            // 事件执行器（失败重试）
            options.AddExecutor<RetryEventHandlerExecutor>();
            // 事件执行器（重试后依然处理未处理异常的处理器）
            options.UnobservedTaskExceptionHandler = (obj, args) =>
            {
                if (args.Exception is { } exception)
                    Log.Error($"EventBus 有未处理异常：{exception.Message}", exception);
            };
            options.AddMonitor<EventHandlerMonitor>();

            // 替换事件源存储器为Redis
            var cacheOptions = FurionApp.GetConfig<CacheOptions>("Cache", true);
            if (cacheOptions.CacheType == CacheTypeEnum.Redis.ToString())
            {
                options.ReplaceStorer(serviceProvider =>
                {
                    var cacheProvider = serviceProvider.GetRequiredService<NewLife.Caching.ICacheProvider>();
                    // 创建默认内存通道事件源对象，可自定义队列路由key，如：jero_eventsource_queue
                    return new RedisEventSourceStorer(cacheProvider, "jero_eventsource_queue", 3000);
                });
            }
        });
    }

    private static void AddRealtime(IServiceCollection services)
    {
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromSeconds(15); // 服务器端向客户端ping的间隔
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30); // 客户端向服务器端ping的间隔
            options.MaximumReceiveMessageSize = 1024 * 1014 * 10; // 数据包大小10M，默认最大为32K
        }).AddJsonProtocol(options => WebProtocols.ConfigureJson(options.PayloadSerializerOptions));
    }

    private static void UseFileHosting(IApplicationBuilder app)
    {
        // ImageSharp must see source image requests before the static-file handlers.
        app.UseImageSharp();

        var contentTypeProvider = FS.GetFileExtensionContentTypeProvider();
        app.UseStaticFiles(new StaticFileOptions
        {
            ContentTypeProvider = contentTypeProvider
        });

        // Vite 生成资产挂在 /assets；根路径与业务深链由 endpoints 后的 SPA fallback 接管。
        app.UseSpaAssets();

        if (!string.IsNullOrEmpty(FurionApp.Settings.VirtualPath))
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = FurionApp.Settings.VirtualPath,
                FileProvider = FurionApp.WebHostEnvironment.WebRootFileProvider
            });
        }
    }

    private static void UseScheduleDashboard(IApplicationBuilder app)
    {
        if (!IsJobScheduleEnabled(FurionApp.Configuration))
            return;

        app.UseScheduleUI(options =>
        {
            options.RequestPath = "/schedule"; // 必须以 / 开头且不以 / 结尾
            options.DisableOnProduction = false; // 是否在生产环境中关闭
            options.DisplayEmptyTriggerJobs = true; // 是否显示空作业触发器的作业
            options.DisplayHead = false; // 是否显示页头
            options.DefaultExpandAllJobs = false; // 是否默认展开所有作业
            options.EnableDirectoryBrowsing = false; // 是否启用目录浏览
            options.Title = "定时任务看板"; // 自定义看板标题

            options.LoginConfig.OnLoging = async (username, password, httpContext) =>
            {
                var statusCode = await httpContext.RequestServices.GetRequiredService<SysAuthService>().SwaggerSubmitUrl(new SpecificationAuth { UserName = username, Password = password });
                return statusCode == StatusCodes.Status200OK;
            };
            options.LoginConfig.DefaultUsername = "";
            options.LoginConfig.DefaultPassword = "";
            options.LoginConfig.AppSecret = "0D7174503141C11DC1A79A7C72B44E0B"; // 登录客户端的授权密钥，必须要求不少于16位的密钥
        });
    }

    private static void MapEndpoints(IApplicationBuilder app)
    {
        app.UseEndpoints(endpoints =>
        {
            // 配置 Scalar 第三方 UI 集成（路由前缀一致代表独立，不同则代表共存）
            if (FurionApp.GetConfig<bool>("AppSettings:InjectSpecificationDocument", true))
            {
                endpoints.MapScalarApiReference("sapi", options =>
                {
                    options.WithTitle("Jero");

                    foreach (var groupInfo in SpecificationDocumentBuilder.GetOpenApiGroups())
                    {
                        options.AddDocument(groupInfo.Group, groupInfo.Title, groupInfo.RouteTemplate);
                    }
                });
            }

            endpoints.MapHubs();

            endpoints
                .MapAGUIServer(SaveLifeMafAgent.Name, "/api/v1/agent/ag-ui")
                .WithMetadata(new RequestSizeLimitAttribute(1024 * 1024))
                .RequireAuthorization();

            endpoints.MapControllers();
        });
    }

    private static bool IsJobScheduleEnabled(IConfiguration configuration)
    {
        return configuration.GetValue("JobSchedule:Enabled", true);
    }

    private static void ConfigureJobScheduleServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSchedule(options =>
        {
            if (!IsJobScheduleEnabled(configuration))
                return;

            options.AddPersistence<DbJobPersistence>(); // 添加作业持久化器
            options.AddMonitor<JobMonitor>(); // 添加作业执行监视器
        });
    }

    private static void ApplyHtmlCacheHeaders(HttpResponse response)
    {
        if (response.ContentType?.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) != true)
            return;

        response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        response.Headers.Pragma = "no-cache";
        response.Headers.Expires = "0";
    }
}
