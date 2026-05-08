namespace Jazor.AspNetCore.Dev;

internal interface IJazorDevelopmentRuntimeSignals
{
    bool IsExternalBrowserRefreshActive { get; }
}
