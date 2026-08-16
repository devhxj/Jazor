using Jazor.AspNetCore;
using Jazor.AspNetCore.Dev;

namespace Todo.Host;

internal static class Program
{
    private static void Main(string[] args)
    {
        var builder = JazorWebApplication.CreateBuilder(args);
        builder.Services.AddJazorReload();

        // SSR is an explicit deployment mode. The release publish must be built with
        // JazorSSR=true so the jazor/ssr graph exists; Todo:Ssr only switches the fallback
        // from the CSR shell to server rendering plus browser hydration.
        var useSsr = string.Equals(builder.Configuration["Todo:Ssr"], "true", StringComparison.OrdinalIgnoreCase);
        if (useSsr)
            builder.Services.AddJazorSsr();

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

        app.UseJazorHost();
        app.UseJazorReload();
        if (useSsr)
        {
            // The module path mirrors TodoApp's [ECMAScriptModule("./components/todo-app")];
            // TodoApp declares no parameters, so the SSR request stays prop-less.
            app.UseJazorSsr(new JazorSsrRequest("components/todo-app.mjs"));
        }
        else
        {
            app.UseJazorSpaFallback(TodoHostShell.WriteAsync);
        }

        app.Run();
    }
}
