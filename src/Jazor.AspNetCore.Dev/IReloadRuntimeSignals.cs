namespace Jazor.AspNetCore.Dev;

/// <summary>Abstracts process-level development tooling signals for the reload service.</summary>
internal interface IReloadRuntimeSignals
{
    bool IsExternalBrowserRefreshActive { get; }
}
