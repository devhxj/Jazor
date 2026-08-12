using Microsoft.Extensions.Hosting;

namespace Jazor.AspNetCore.Dev;

internal sealed class JazorDevelopmentRuntimeEnvironmentSignals(IHostEnvironment environment) : IJazorDevelopmentRuntimeSignals
{
    private readonly IHostEnvironment _environment = environment ?? throw new ArgumentNullException(nameof(environment));

	public bool IsExternalBrowserRefreshActive
        => _environment.IsDevelopment()
            && JazorDevelopmentExternalBrowserRefreshDetector.IsActive(Environment.GetEnvironmentVariable);
}
