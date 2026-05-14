using Jazor.AspNetCore;

namespace Todo.Host;

internal static class Program
{
    private static void Main(string[] args)
    {
        var builder = JazorWebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.UseJazorHost();
        app.UseJazorSpaFallback("/index.html");

        app.Run();
    }
}
