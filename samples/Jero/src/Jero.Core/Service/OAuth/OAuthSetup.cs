


using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
namespace Jero.Core;

public static class OAuthSetup
{
    /// <summary>
    /// 三方授权登录OAuth注册
    /// </summary>
    /// <param name="services"></param>
    public static void AddOAuth(this IServiceCollection services)
    {
        var authOpt = App.GetConfig<OAuthOptions>("OAuth", true);
        var authentication = services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

        // Remote handlers validate credentials on the first request, even for a local login.
        if (authOpt?.Weixin is { ClientId.Length: > 0, ClientSecret.Length: > 0 } weixin)
            authentication.AddWeixin(options =>
            {
                options.ClientId = weixin.ClientId;
                options.ClientSecret = weixin.ClientSecret;
            });
        if (authOpt?.Gitee is { ClientId.Length: > 0, ClientSecret.Length: > 0 } gitee)
            authentication.AddGitee(options =>
            {
                options.ClientId = gitee.ClientId;
                options.ClientSecret = gitee.ClientSecret;

                options.ClaimActions.MapJsonKey(OAuthClaim.GiteeAvatarUrl, "avatar_url");
            });
    }

    public static void UseOAuth(this IApplicationBuilder app)
    {
        app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });
    }
}
