using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore.Dev;

internal sealed class JazorFrontendOptionsValidator : IValidateOptions<JazorFrontendOptions>
{
    public ValidateOptionsResult Validate(string? name, JazorFrontendOptions options)
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

        if (!IsCanonicalPrefix(options.RequestPath, allowEmpty: false))
            return ValidateOptionsResult.Fail("Jazor frontend RequestPath must be a non-root path without a trailing slash.");

        if (!IsCanonicalPrefix(options.PathBase, allowEmpty: true))
            return ValidateOptionsResult.Fail("Jazor frontend PathBase must be empty or a non-root path without a trailing slash.");

        if (string.IsNullOrWhiteSpace(options.ProjectRootPath))
            return ValidateOptionsResult.Fail("Jazor frontend ProjectRootPath cannot be empty.");

        if (!IsSafeRelativePath(options.DevelopmentEntryRelativePath))
            return ValidateOptionsResult.Fail("Jazor frontend DevelopmentEntryRelativePath must be a project-relative path.");

        if (!IsSafeRelativePath(options.ReleaseEntryRelativePath))
            return ValidateOptionsResult.Fail("Jazor frontend ReleaseEntryRelativePath must be a project-relative path.");

        if (string.IsNullOrWhiteSpace(options.Vite.TaskName))
            return ValidateOptionsResult.Fail("Jazor Vite TaskName cannot be empty.");

        if (options.Vite.StartupTimeout <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Jazor Vite StartupTimeout must be greater than zero.");

        if (options.Vite.ShutdownTimeout <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Jazor Vite ShutdownTimeout must be greater than zero.");

        if (options.Vite.LaunchServer && options.Vite.ServerOrigin.Scheme != Uri.UriSchemeHttp)
        {
            return ValidateOptionsResult.Fail(
                "DenoHost-managed Vite launch requires an HTTP ServerOrigin. Set LaunchServer=false for an external HTTPS server.");
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
