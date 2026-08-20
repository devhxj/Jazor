using Jazor.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace JazorAdmin.DemoClient;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = JazorWebApplication.CreateBuilder(args);
        builder.Services.Configure<DemoClientOptions>(
            builder.Configuration.GetSection(DemoClientOptions.SectionName));
        var options = builder.Configuration
            .GetSection(DemoClientOptions.SectionName)
            .Get<DemoClientOptions>()
            ?? throw new InvalidOperationException("JazorAdminDemo configuration is required.");

        if (string.IsNullOrWhiteSpace(options.Authority) ||
            string.IsNullOrWhiteSpace(options.ClientSecret))
        {
            throw new InvalidOperationException(
                "Configure JazorAdminDemo:Authority and JazorAdminDemo:ClientSecret using local user-secrets or deployment configuration.");
        }

        if (!Uri.TryCreate(options.Authority, UriKind.Absolute, out var authority))
            throw new InvalidOperationException("JazorAdminDemo:Authority must be an absolute URI.");

        var requiresSecureTransport = authority.Scheme == Uri.UriSchemeHttps;

        builder.Services.AddHttpClient("JazorAdmin", client =>
        {
            client.BaseAddress = new Uri(authority, "/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        builder.Services
            .AddAuthentication(authentication =>
            {
                authentication.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                authentication.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(cookie =>
            {
                cookie.Cookie.Name = "jazoradmin.demo.session";
                cookie.LoginPath = "/auth/signin";
                cookie.LogoutPath = "/auth/signout";
            })
            .AddOpenIdConnect(openIdConnect =>
            {
                openIdConnect.Authority = authority.AbsoluteUri.TrimEnd('/');
                openIdConnect.ClientId = options.ClientId;
                openIdConnect.ClientSecret = options.ClientSecret;
                openIdConnect.ResponseType = OpenIdConnectResponseType.Code;
                openIdConnect.ResponseMode = OpenIdConnectResponseMode.Query;
                openIdConnect.UsePkce = true;
                openIdConnect.SaveTokens = true;
                openIdConnect.GetClaimsFromUserInfoEndpoint = false;
                openIdConnect.RequireHttpsMetadata = requiresSecureTransport;
                // The documented Development profile includes an HTTP loopback pair. OIDC's
                // correlation/nonce cookies default to Secure, which cannot round-trip there;
                // deployed HTTPS authorities retain the strict Always policy.
                // 开发配置提供 HTTP loopback 双端口；默认 Secure 的 OIDC cookie 无法回传，
                // 只有 HTTP Authority 退化为 SameAsRequest，HTTPS 部署始终保持 Always。
                openIdConnect.CorrelationCookie.SecurePolicy = requiresSecureTransport
                    ? CookieSecurePolicy.Always
                    : CookieSecurePolicy.SameAsRequest;
                openIdConnect.NonceCookie.SecurePolicy = requiresSecureTransport
                    ? CookieSecurePolicy.Always
                    : CookieSecurePolicy.SameAsRequest;
                openIdConnect.CallbackPath = "/signin-oidc";
                openIdConnect.SignedOutCallbackPath = "/signout-callback-oidc";
                openIdConnect.SignedOutRedirectUri = "/";
                openIdConnect.Scope.Clear();
                openIdConnect.Scope.Add("openid");
                openIdConnect.Scope.Add("profile");
                openIdConnect.Scope.Add("email");
                openIdConnect.Scope.Add("roles");
                openIdConnect.Scope.Add("offline_access");
                openIdConnect.Scope.Add("jazoradmin_api");
                openIdConnect.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            });
        builder.Services.AddAuthorization();

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapDemoEndpoints();
        app.UseJazorHost();
        app.UseJazorSpaFallback(DemoShell.WriteAsync);

        await app.RunAsync();
    }
}
