namespace Jazor.VueHost.Extensions;

internal sealed class ExtensionSandboxProfile
{
    private static readonly string[] DefaultLoopbackHosts =
    [
        "localhost",
        "127.0.0.1",
        "::1"
    ];

    public static ExtensionSandboxProfile Unrestricted { get; } = new()
    {
        IoCapability = ExtensionHostOptions.IoCapabilityReadWrite,
        NetworkCapability = ExtensionHostOptions.NetworkCapabilityInternet,
        ReadRoots = Array.Empty<string>(),
        WriteRoots = Array.Empty<string>(),
        AllowedHosts = Array.Empty<string>()
    };

    public required string IoCapability { get; init; }

    public required string NetworkCapability { get; init; }

    public required string[] ReadRoots { get; init; }

    public required string[] WriteRoots { get; init; }

    public required string[] AllowedHosts { get; init; }

    public bool IsReadPathAllowed(string path)
    {
        if (string.Equals(IoCapability, ExtensionHostOptions.IoCapabilityNone, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return IsPathAllowed(path, ReadRoots);
    }

    public bool IsWritePathAllowed(string path)
    {
        if (!string.Equals(IoCapability, ExtensionHostOptions.IoCapabilityReadWrite, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return IsPathAllowed(path, WriteRoots);
    }

    public bool IsNetworkHostAllowed(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return false;
        }

        var normalizedHost = host.Trim().ToLowerInvariant();
        if (string.Equals(NetworkCapability, ExtensionHostOptions.NetworkCapabilityNone, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.Equals(NetworkCapability, ExtensionHostOptions.NetworkCapabilityLoopback, StringComparison.OrdinalIgnoreCase))
        {
            if (!IsLoopbackHost(normalizedHost))
            {
                return false;
            }

            var effectiveHosts = AllowedHosts.Length == 0
                ? DefaultLoopbackHosts
                : AllowedHosts;
            if (effectiveHosts.Contains("*", StringComparer.Ordinal))
            {
                return true;
            }

            return effectiveHosts.Contains(normalizedHost, StringComparer.OrdinalIgnoreCase);
        }

        if (AllowedHosts.Length == 0)
        {
            return true;
        }

        if (AllowedHosts.Contains("*", StringComparer.Ordinal))
        {
            return true;
        }

        return AllowedHosts.Contains(normalizedHost, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsPathAllowed(string path, IReadOnlyList<string> roots)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        if (roots.Count == 0)
        {
            return true;
        }

        var normalizedPath = Path.GetFullPath(path);
        foreach (var root in roots)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                continue;
            }

            var normalizedRoot = Path.GetFullPath(root);
            if (IsPathInsideDirectory(normalizedRoot, normalizedPath)
                || string.Equals(normalizedRoot, normalizedPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsPathInsideDirectory(string directoryPath, string candidatePath)
    {
        var relativePath = Path.GetRelativePath(directoryPath, candidatePath);
        return !string.IsNullOrWhiteSpace(relativePath)
            && !relativePath.StartsWith("..", StringComparison.Ordinal)
            && !Path.IsPathRooted(relativePath);
    }

    private static bool IsLoopbackHost(string host)
    {
        return string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
            || string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase);
    }
}
