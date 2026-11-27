using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore.Dev;

internal sealed class JazorSPAOptionsValidator : IValidateOptions<JazorSPAOptions>
{
    public ValidateOptionsResult Validate(string? name, JazorSPAOptions options)
    {
        if (!options.Vite.ServerOrigin.IsAbsoluteUri ||
            options.Vite.ServerOrigin.Scheme is not ("http" or "https") ||
            options.Vite.ServerOrigin.AbsolutePath != "/" ||
            !string.IsNullOrEmpty(options.Vite.ServerOrigin.Query) ||
            !string.IsNullOrEmpty(options.Vite.ServerOrigin.Fragment))
        {
            return ValidateOptionsResult.Fail(
                "Jazor Vite ServerOrigin must be an absolute HTTP(S) origin without a path, query, or fragment.");
        }

        if (!IsCanonicalPrefix(options.RequestPath, allowEmpty: options.IsJoltApp))
            return ValidateOptionsResult.Fail("Jazor SPA RequestPath must be a path without a trailing slash, or empty for a root Jolt site.");

        if (!IsCanonicalPrefix(options.PathBase, allowEmpty: true))
            return ValidateOptionsResult.Fail("Jazor frontend PathBase must be empty or a non-root path without a trailing slash.");

        if (string.IsNullOrWhiteSpace(options.ProjectRootPath))
            return ValidateOptionsResult.Fail("Jazor frontend ProjectRootPath cannot be empty.");

        if (!IsSafeRelativePath(options.DevelopmentEntryRelativePath))
            return ValidateOptionsResult.Fail("Jazor frontend DevelopmentEntryRelativePath must be a project-relative path.");

        if (!IsSafeRelativePath(options.ReleaseEntryRelativePath))
            return ValidateOptionsResult.Fail("Jazor frontend ReleaseEntryRelativePath must be a project-relative path.");

        if (options.IsJoltApp && options.ReleaseEntryRelativePath.Split('/', '\\')
                .Count(segment => segment.Length > 0 && segment != ".") < 2)
            return ValidateOptionsResult.Fail("Jolt ReleaseEntryRelativePath must be inside a build output directory, such as dist/index.html.");

        if (options.Vite.StartupTimeout <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Jazor Vite StartupTimeout must be greater than zero.");

        if (options.Vite.ShutdownTimeout <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Jazor Vite ShutdownTimeout must be greater than zero.");

        if (options.Vite.LaunchServer && options.Vite.ServerOrigin.Scheme != Uri.UriSchemeHttp)
        {
            return ValidateOptionsResult.Fail(
                "NodeApi-managed Vite startup requires an HTTP ServerOrigin. Set LaunchServer=false for an external HTTPS server.");
        }

        return ValidateOptionsResult.Success;
    }

    private static bool IsCanonicalPrefix(Microsoft.AspNetCore.Http.PathString path, bool allowEmpty)
    {
        if (!path.HasValue)
            return allowEmpty;

        var value = path.Value!;
        return value.StartsWith('/', StringComparison.Ordinal) &&
               value != "/" &&
               !value.EndsWith('/', StringComparison.Ordinal);
    }

    private static bool IsSafeRelativePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path) || Uri.TryCreate(path, UriKind.Absolute, out _))
            return false;

        return !path.Replace('\\', '/').Split('/').Any(static segment => segment == "..");
    }
}
