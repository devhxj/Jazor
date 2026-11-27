using Furion.ConfigurableOptions;

namespace Jero.Application.Modules.ReZero;

public sealed class ReZeroOptions : IConfigurableOptions
{
    /// <summary>
    /// AccessTokenKey
    /// </summary>
    public string AccessTokenKey { get; set; } = string.Empty;

    /// <summary>
    /// RefreshAccessTokenKey
    /// </summary>
    public string RefreshAccessTokenKey { get; set; } = string.Empty;
}
