namespace Jazor.AspNetCore;

/// <summary>Groups the options consumed by <c>UseJazorHost</c>.</summary>
public sealed class JazorHostOptions
{
    /// <summary>Initializes the default security-header and asset-hosting configuration.</summary>
    public JazorHostOptions()
    {
        SecurityHeaders = new JazorSecurityHeaderOptions();
        Assets = new JazorAssetOptions();
    }

    /// <summary>Response security-header settings.</summary>
    public JazorSecurityHeaderOptions SecurityHeaders { get; }

    /// <summary>Generated and ordinary static-file settings.</summary>
    public JazorAssetOptions Assets { get; }
}
