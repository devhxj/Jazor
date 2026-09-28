// Composes the JazorAdmin ASP.NET Core host, including its API, identity server, and RazorVue frontend.
// 组装 JazorAdmin ASP.NET Core 宿主，在同一进程承载 API、身份中心与 RazorVue 前端。
using Jazor.AspNetCore;
using Jazor.AspNetCore.Dev;

namespace JazorAdmin;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = JazorWebApplication.CreateBuilder(args);
        builder.Services.AddAdminHost(builder.Configuration, builder.Environment);
        builder.AddJazorFrontend(options =>
        {
            var serverOrigin = builder.Configuration.GetValue<Uri>("Jazor:JavaScriptServer");
            if (serverOrigin is not null)
                options.Vite.ServerOrigin = serverOrigin;
        });

        var app = builder.Build();

        app.UseJazorPathBase();
        app.UseAdminHost();
        app.MapAdminEndpoints();
        app.UseJazorFrontend();
        app.UseJazorSpaFallback(Shell.WriteAsync);

        await app.RunAsync();
    }
}
