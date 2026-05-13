namespace Jazor.AspNetCore;

public sealed class JazorHostOptions
{
    public JazorHostOptions()
    {
        SecurityHeaders = new JazorSecurityHeaderOptions();
        WebAssets = new JazorWebAssetOptions();
    }

    public JazorSecurityHeaderOptions SecurityHeaders { get; }

    public JazorWebAssetOptions WebAssets { get; }
}
