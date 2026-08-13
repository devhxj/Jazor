using Jazor.AspNetCore;
using Jazor.AspNetCore.Dev;

namespace Todo.Host;

internal static class Program
{
    private static void Main(string[] args)
    {
        var builder = JazorWebApplication.CreateBuilder(args);
        builder.Services.AddJazorReload();

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
        app.UseJazorSpaFallback(TodoHostShell.WriteAsync);
        app.Run();
    }
}
