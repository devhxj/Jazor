namespace Jazor.AspNetCore;

/// <summary>Configures response security headers applied by <c>UseJazorSecurityHeaders</c>.</summary>
public sealed class JazorSecurityHeaderOptions
{
    /// <summary>Default restrictive browser permissions policy applied by Jazor hosting.</summary>
    public const string DefaultPermissionsPolicy =
        "accelerometer=(), autoplay=(), camera=(), display-capture=(), geolocation=(), gyroscope=(), " +
        "hid=(), microphone=(), payment=(), usb=()";

    /// <summary>Initializes the additional-header collection with case-insensitive keys.</summary>
    public JazorSecurityHeaderOptions()
    {
        AdditionalHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Value for <c>X-Content-Type-Options</c>; set to <see langword="null"/> to omit it.</summary>
    public string? XContentTypeOptions { get; set; } = "nosniff";

    /// <summary>Value for <c>Referrer-Policy</c>; set to <see langword="null"/> to omit it.</summary>
    public string? ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";

    /// <summary>Value for <c>X-Frame-Options</c>; set to <see langword="null"/> to omit it.</summary>
    public string? XFrameOptions { get; set; } = "DENY";

    /// <summary>Value for <c>Cross-Origin-Opener-Policy</c>; set to <see langword="null"/> to omit it.</summary>
    public string? CrossOriginOpenerPolicy { get; set; } = "same-origin";

    /// <summary>Value for <c>Cross-Origin-Resource-Policy</c>; set to <see langword="null"/> to omit it.</summary>
    public string? CrossOriginResourcePolicy { get; set; } = "same-origin";

    /// <summary>Value for <c>Permissions-Policy</c>; set to <see langword="null"/> to omit it.</summary>
    public string? PermissionsPolicy { get; set; } = DefaultPermissionsPolicy;

    /// <summary>Value for <c>X-Permitted-Cross-Domain-Policies</c>; set to <see langword="null"/> to omit it.</summary>
    public string? XPermittedCrossDomainPolicies { get; set; } = "none";

    /// <summary>Additional headers applied only when the response has not set them already.</summary>
    public IDictionary<string, string> AdditionalHeaders { get; }
}
