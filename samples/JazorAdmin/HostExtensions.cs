using JazorAdmin.Authentication;
using JazorAdmin.Authorization;
using JazorAdmin.Data;
using JazorAdmin.Features.Accounts;
using JazorAdmin.Features.Audit;
using JazorAdmin.Features.Identity;
using JazorAdmin.Features.Notifications;
using JazorAdmin.Features.Organizations;
using JazorAdmin.Features.Overview;
using JazorAdmin.Features.Scheduling;
using JazorAdmin.Features.Settings;
using JazorAdmin.Features.Sso;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using Quartz;

namespace JazorAdmin;

/// <summary>
/// Registers and orders the shared host services for the RazorVue SPA, Web API, Identity, and OpenIddict server.
/// 注册并排序 RazorVue SPA、Web API、Identity 和 OpenIddict Server 共用的宿主服务。
/// </summary>
public static class HostExtensions
{
    public static IServiceCollection AddAdminHost(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.Configure<OpenIddictOptions>(
            configuration.GetSection(OpenIddictOptions.SectionName));
        services.Configure<DemoClientOptions>(
            configuration.GetSection(DemoClientOptions.SectionName));
        services.Configure<BootstrapOptions>(
            configuration.GetSection(BootstrapOptions.SectionName));
        services.AddProblemDetails();
        services.AddHttpContextAccessor();
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddMemoryCache();
        services.AddSingleton<CaptchaService>();
        services.AddHealthChecks();
        // Resolve configuration when the context is created so WebApplicationFactory can replace the
        // database per test host. 注册阶段捕获连接串会绕过测试宿主的隔离配置。
        services.AddDbContext<AdminDbContext>((provider, options) =>
        {
            var currentConfiguration = provider.GetRequiredService<IConfiguration>();
            var connectionString = currentConfiguration.GetConnectionString("JazorAdmin")
                ?? throw new InvalidOperationException("Connection string 'JazorAdmin' is required.");
            options.UseSqlite(connectionString);
            options.AddInterceptors(provider.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        services.AddIdentity<AdminUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
            })
            .AddEntityFrameworkStores<AdminDbContext>()
            .AddDefaultTokenProviders();
        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/login";
            options.Events.OnRedirectToLogin = context =>
            {
                // API callers need a machine-readable 401; only browser authorization navigation may redirect.
                // API 调用方必须收到可处理的 401，只有浏览器授权导航允许跳转到登录页。
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                // A dynamic resource grant denial is a 403 API response, not a redirect to the SPA
                // fallback. Otherwise clients that follow redirects can mistake denial for success.
                // 动态资源授权拒绝必须返回 403，不能跳到 SPA 回退页，否则跟随重定向的客户端会误判成功。
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
        });
        services.AddAdminAuthentication();

        services.AddAuthorization(options => options.AddPolicy(
            PolicyKeys.PlatformAdministrator,
            policy => policy.RequireRole(RoleKeys.PlatformAdministrator)));
        services.AddSingleton<IAuthorizationPolicyProvider, ResourceOperationAuthorizationPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, ResourceOperationAuthorizationHandler>();

        services.AddOpenIddict()
            .AddCore(options => options.UseEntityFrameworkCore().UseDbContext<AdminDbContext>())
            .AddServer(options =>
            {
                options.SetConfigurationEndpointUris(".well-known/openid-configuration")
                    .SetAuthorizationEndpointUris("connect/authorize")
                    .SetIntrospectionEndpointUris("connect/introspect")
                    .SetRevocationEndpointUris("connect/revoke")
                    .SetTokenEndpointUris("connect/token")
                    .SetEndSessionEndpointUris("connect/logout")
                    .AllowAuthorizationCodeFlow()
                    .AllowClientCredentialsFlow()
                    .AllowRefreshTokenFlow()
                    .RegisterScopes(
                        OpenIddictConstants.Scopes.Email,
                        OpenIddictConstants.Scopes.Profile,
                        OpenIddictConstants.Scopes.Roles,
                        ScopeKeys.Api)
                    .SetAccessTokenLifetime(TimeSpan.FromMinutes(20))
                    .SetRefreshTokenLifetime(TimeSpan.FromDays(14));

                // This sample validates the deployment shape locally. Production deployments replace these
                // development certificates with managed signing and encryption credentials.
                // 此处验证本地部署形态；生产部署需替换为受管的签名和加密证书。
                if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
                {
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                }
                else
                {
                    options.AddEphemeralEncryptionKey()
                        .AddEphemeralSigningKey();
                }

                var aspNetCore = options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableTokenEndpointPassthrough();
                if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
                    aspNetCore.DisableTransportSecurityRequirement();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        services.AddQuartz();
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
        services.AddScoped<IManagedTask, OpenIddictPruneTask>();
        services.AddSingleton<ScheduleService>();
        services.AddHostedService<DatabaseInitializer>();
        services.AddHostedService<ScheduleInitializer>();
        return services;
    }

    public static WebApplication UseAdminHost(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler();
            app.UseHsts();
        }

        if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
            app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health/live").AllowAnonymous();
        app.MapIdentityEndpoints();
        app.MapAccountEndpoints();
        app.MapOpenIddictEndpoints();
        app.MapOrganizationEndpoints();
        app.MapSsoEndpoints();
        app.MapSettingEndpoints();
        app.MapScheduleEndpoints();
        app.MapNotificationEndpoints();
        app.MapOverviewEndpoints();
        app.MapAuditEndpoints();
        return app;
    }
}
