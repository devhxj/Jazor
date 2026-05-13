namespace Jazor.AspNetCore;

public sealed class JazorSecurityHeaderOptions
{
    public const string DefaultPermissionsPolicy =
        "accelerometer=(), autoplay=(), camera=(), display-capture=(), geolocation=(), gyroscope=(), " +
        "hid=(), microphone=(), payment=(), usb=()";

    public JazorSecurityHeaderOptions()
    {
        AdditionalHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public string? XContentTypeOptions { get; set; } = "nosniff";

    public string? ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";

    public string? XFrameOptions { get; set; } = "DENY";

    public string? CrossOriginOpenerPolicy { get; set; } = "same-origin";

    public string? CrossOriginResourcePolicy { get; set; } = "same-origin";

    public string? PermissionsPolicy { get; set; } = DefaultPermissionsPolicy;

    public string? XPermittedCrossDomainPolicies { get; set; } = "none";

    public IDictionary<string, string> AdditionalHeaders { get; }
}
