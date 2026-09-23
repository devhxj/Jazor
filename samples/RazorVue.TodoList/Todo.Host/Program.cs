using Jazor.AspNetCore;
using Jazor.AspNetCore.Dev;

namespace Todo.Host;

internal static class Program
{
    private static void Main(string[] args)
    {
        var builder = JazorWebApplication.CreateBuilder(args);
        builder.Services.AddJazorViteProxy(options =>
            options.ServerOrigin = new Uri(builder.Configuration["Todo:JavaScriptServer"] ?? "http://127.0.0.1:5173"));

        // SSR is an explicit deployment mode. The release publish must be built with
        // JazorSSR=true so the standard project contains ssr-entry.js; Todo:Ssr switches the fallback
        // from the CSR shell to server rendering plus browser hydration.
        var useSsr = string.Equals(builder.Configuration["Todo:Ssr"], "true", StringComparison.OrdinalIgnoreCase);
        if (useSsr)
            builder.Services.AddJazorSsr(options =>
            {
                options.TaskName = builder.Environment.IsDevelopment() ? "ssr:dev" : "ssr";
                if (!builder.Environment.IsDevelopment())
                    options.HydrationEntryPath = "dist/hydration.js";
            });

        var app = builder.Build();
        var pathBase = builder.Configuration["Todo:PathBase"];
        if (!string.IsNullOrWhiteSpace(pathBase))
        {
            if (!pathBase.StartsWith('/', StringComparison.Ordinal))
                throw new InvalidOperationException("Todo:PathBase must start with '/'.");

            app.UsePathBase(pathBase.EndsWith('/', StringComparison.Ordinal) && pathBase.Length > 1
                ? pathBase[..^1]
                : pathBase);
        }

        if (app.Environment.IsDevelopment())
            app.UseJazorViteProxy();
        app.UseJazorHost(options => options.Assets.ServeArtifacts = true);
        if (useSsr)
        {
            // The module path mirrors TodoApp's explicit [ECMAScriptModule("./components/todo-app.js")].
            // Passing a normal prop exercises the generated ParameterView entry point in the
            // same isolated Release consumer that serves and hydrates the page.
            app.UseJazorSsr(new JazorSsrRequest(
                "components/todo-app.js",
                new { SsrTitle = "SSR ParameterView title" },
                [
                    new JazorSsrProvider(
                        "jazor:service:Todo.Library.TodoBrowserService",
                        new { Label = "ssr-provider" })
                ],
                new JazorAuthenticationState(
                    JazorAuthenticationStatus.Authenticated,
                    "ssr-user",
                    new Dictionary<string, string[]> { ["role"] = ["reader"] })));
        }
        else
        {
            app.UseJazorSpaFallback(TodoHostShell.WriteAsync);
        }

        app.Run();
    }
}
