using System.Formats.Tar;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace Jazor.NodeHost;

internal static class PnpmBootstrap
{
    internal static readonly string Version = typeof(PnpmBootstrap).Assembly
        .GetCustomAttributes<AssemblyMetadataAttribute>()
        .Single(attribute => attribute.Key == "PnpmEngineVersion").Value!;
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromMinutes(5) };
    private static readonly SemaphoreSlim Gate = new(1, 1);

    public static async Task<string> EnsureAsync(string cacheDirectory, CancellationToken cancellationToken)
    {
        var platform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win32"
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "darwin" : "linux";
        var architecture = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
        var nativePackage = $"@pnpm/napi.{platform}-{architecture}";
        // Node keeps native addons loaded for the lifetime of the platform. A
        // project/fixture cannot own these files: deleting it on Windows would
        // fail even after its environment has been disposed.
        var root = Path.Combine(cacheDirectory, "pnpm-engine", Version, platform + "-" + architecture);

        await Gate.WaitAsync(cancellationToken);
        try
        {
            // Bootstrap only the two fixed engine packages. Application dependencies
            // are resolved by pnpm itself; there is no C# package resolver or CLI.
            await ExtractPackageAsync("@pnpm/napi", root, cancellationToken);
            await ExtractPackageAsync(nativePackage, root, cancellationToken);
            return root;
        }
        finally
        {
            Gate.Release();
        }
    }

    private static async Task ExtractPackageAsync(string name, string root, CancellationToken cancellationToken)
    {
        var destination = Path.Combine(root, "node_modules", name.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(Path.Combine(destination, "package.json")))
            return;

        Console.Error.WriteLine($"[Jazor NodeApi] Bootstrapping {name}@{Version}.");
        await using var metadataStream = await Http.GetStreamAsync(
            $"https://registry.npmjs.org/{Uri.EscapeDataString(name)}/{Version}", cancellationToken);
        using var metadata = await JsonDocument.ParseAsync(metadataStream,
            cancellationToken: cancellationToken);
        var dist = metadata.RootElement.GetProperty("dist");
        var archive = await Http.GetByteArrayAsync(dist.GetProperty("tarball").GetString(), cancellationToken);
        var actualIntegrity = "sha512-" + Convert.ToBase64String(SHA512.HashData(archive));
        if (actualIntegrity != dist.GetProperty("integrity").GetString())
            throw new InvalidDataException($"Integrity mismatch for '{name}@{Version}'.");

        var temporaryDirectory = Path.Combine(root, "extract-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            using var gzip = new GZipStream(new MemoryStream(archive), CompressionMode.Decompress);
            await TarFile.ExtractToDirectoryAsync(gzip, temporaryDirectory, overwriteFiles: false, cancellationToken);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            try
            {
                Directory.Move(Path.Combine(temporaryDirectory, "package"), destination);
            }
            catch (IOException) when (File.Exists(Path.Combine(destination, "package.json")))
            {
                // Another .NET process atomically published the same fixed
                // package while this process downloaded it.
            }
        }
        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }
}
