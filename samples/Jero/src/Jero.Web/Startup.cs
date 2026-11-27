using AspNetCoreRateLimit;
using Furion;
using Furion.Logging;
using Furion.SpecificationDocument;
using Furion.VirtualFileServer;
using IPTools.Core;
using Jazor.AspNetCore.Dev;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
using FurionApp = Furion.App;

namespace Jero.Web;

[AppStartup(int.MaxValue)]
public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // 开发密钥先进入配置源，后续认证注册与初始化数据才能读到同一份值。
        DevelopmentKeys.Apply(FurionApp.WebHostEnvironment, FurionApp.Configuration);
        services.AddProjectOptions();
        services.AddJazorSPA(options =>
        {
            options.PathBase = FurionApp.Configuration["SPA:PathBase"]?.TrimEnd('/') ?? "";
            // 保留服务端路由命名空间。即使这些地址未命中端点，也不应回退成 SPA HTML。
            foreach (var prefix in new[] { "/hubs", "/swagger", "/sapi", "/schedule", "/rezero" })
                options.Fallback.ExcludedPathPrefixes.Add(prefix);
        });
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
        // 由反向代理恢复请求信息，再按 SPA:PathBase 拆分路径供路由和代理使用。
        app.UseForwardedHeaders();
        app.UseJazorPathBase();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler();
            app.UseHsts();
        }

        app.Use((context, next) =>
        {
            context.Response.Headers.Append("Jero", "Jero");
            // 管理页面通过同源 iframe 嵌入 Swagger 和 ReZero。
            context.Response.Headers.XFrameOptions = "SAMEORIGIN";
            // 静态文件与 SPA 回退都在后续中间件设置 Content-Type，提交响应头时再判断缓存策略。
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

        // Furion 从请求中选定语言后，端点路由才开始匹配。
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
        app.UseJazorSPA();
    }

    private static void AddJeroAuthentication(IServiceCollection services)
    {
        services.AddJwt<JwtHandler>(enableGlobalAuthorize: true, jwtBearerConfigure: options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var httpContext = context.HttpContext;
                    // 浏览器建立 SignalR WebSocket 时通过查询参数 token 传递 JWT。
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
        services.AddProblemDetails();
        services.AddControllers()
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
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            // 服务端在这段时间内收不到客户端消息才判为断线，心跳也计入活动。
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            options.MaximumReceiveMessageSize = 1024 * 1014 * 10;
        }).AddJsonProtocol(options => WebProtocols.ConfigureJson(options.PayloadSerializerOptions));
    }

    private static void UseFileHosting(IApplicationBuilder app)
    {
        // 带缩放参数的图片请求先交给 ImageSharp，否则静态文件中间件会直接返回原图。
        app.UseImageSharp();

        var contentTypeProvider = FS.GetFileExtensionContentTypeProvider();
        app.UseStaticFiles(new StaticFileOptions
        {
            ContentTypeProvider = contentTypeProvider
        });

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
