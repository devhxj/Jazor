using Microsoft.Extensions.Hosting;

namespace Jazor.AspNetCore.Dev;

/// <summary>Reads the active host environment and ASP.NET Core browser-refresh markers.</summary>
internal sealed class RuntimeEnvironmentSignals(IHostEnvironment environment) : IReloadRuntimeSignals
{
    private readonly IHostEnvironment _environment = environment ?? throw new ArgumentNullException(nameof(environment));

    public bool IsExternalBrowserRefreshActive
        => _environment.IsDevelopment()
            && ExternalBrowserRefresh.IsActive(Environment.GetEnvironmentVariable);
}
