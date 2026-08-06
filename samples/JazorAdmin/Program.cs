// Composes the JazorAdmin ASP.NET Core host, including its API, identity server, and RazorVue frontend.
// 组装 JazorAdmin ASP.NET Core 宿主，在同一进程承载 API、身份中心与 RazorVue 前端。
using Jazor.AspNetCore;

namespace JazorAdmin;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = JazorWebApplication.CreateBuilder(args);
        builder.Services.AddJazorAdminHost(builder.Configuration, builder.Environment);

        var app = builder.Build();

        app.UseJazorAdminHost();
        app.MapJazorAdminEndpoints();
        app.UseJazorHost();
        app.UseJazorSpaFallback(JazorAdminShell.WriteAsync);

        await app.RunAsync();
    }
}
