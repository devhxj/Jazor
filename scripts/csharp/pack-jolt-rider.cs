#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

const string sdkVersion = "2026.3.0.2";
const string sdkBuild = "263.7307.355";
const string sdkSha256 = "F3C9A151AA9D529CF574EE91CA19A9A35F54046C22A7ED8050E94FB9B85376FC";
const string sdkUrl = "https://download.jetbrains.com/rider/JetBrains.Rider-" + sdkVersion + ".win.zip";

if (args.Length is < 2 or > 3 || !Regex.IsMatch(args[0], @"\A[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z.-]+)?\z"))
    throw new ArgumentException("Usage: pack-jolt-rider.cs <version> <output.zip> [Rider SDK directory]");

var root = Directory.GetCurrentDirectory();
var destination = Path.GetFullPath(args[1]);
Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
var source = Path.Combine(root, "editors", "jolt-rider", "src", "main");
var resources = Path.Combine(source, "resources");
var manifestPath = Path.Combine(resources, "META-INF", "plugin.xml");
var manifest = XDocument.Load(manifestPath);
manifest.Root!.SetElementValue("version", args[0]);

var work = Directory.CreateDirectory(Path.Combine(root, ".tmp", $"pack-jolt-rider-{Guid.NewGuid():N}")).FullName;
try
{
    var sdk = args.Length == 3 ? Path.GetFullPath(args[2]) : await DownloadSdkAsync();
    using var product = JsonDocument.Parse(await File.ReadAllBytesAsync(Path.Combine(sdk, "product-info.json")));
    if (product.RootElement.GetProperty("productCode").GetString() != "RD" ||
        product.RootElement.GetProperty("version").GetString() != sdkVersion ||
        product.RootElement.GetProperty("buildNumber").GetString() != sdkBuild)
        throw new InvalidOperationException($"Expected Rider SDK {sdkVersion} (RD-{sdkBuild}) at '{sdk}'.");

    var classes = Directory.CreateDirectory(Path.Combine(work, "classes")).FullName;
    var javac = Path.Combine(sdk, "jbr", "bin", OperatingSystem.IsWindows() ? "javac.exe" : "javac");
    var compile = new ProcessStartInfo(javac) { UseShellExecute = false };
    // The plugin targets the 263 platform APIs and runs on the JDK that ships with Rider.
    foreach (var argument in new[] { "--release", "25", "-encoding", "UTF-8", "-proc:none", "-classpath", Path.Combine(sdk, "lib", "*"), "-d", classes })
        compile.ArgumentList.Add(argument);
    foreach (var file in Directory.EnumerateFiles(Path.Combine(source, "java"), "*.java", SearchOption.AllDirectories)
                 .Order(StringComparer.Ordinal))
        compile.ArgumentList.Add(file);
    using (var compiler = Process.Start(compile)!)
    {
        await compiler.WaitForExitAsync();
        if (compiler.ExitCode != 0)
            throw new InvalidOperationException($"Rider javac failed with exit code {compiler.ExitCode}.");
    }

    var jarPath = Path.Combine(work, "jolt-rider.jar");
    using (var jar = ZipFile.Open(jarPath, ZipArchiveMode.Create))
    {
        AddDirectory(jar, classes);
        AddDirectory(jar, resources, manifestPath);
        using (var stream = CreateEntry(jar, "META-INF/plugin.xml").Open())
            manifest.Save(stream);
        AddFile(jar, Path.Combine(root, "LICENSE.txt"), "META-INF/LICENSE.txt");
    }

    var zipPath = Path.Combine(work, "jolt-rider.zip");
    using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        AddFile(zip, jarPath, "jolt-rider/lib/jolt-rider.jar");
    File.Move(zipPath, destination, overwrite: true);
    Console.WriteLine($"Packed Rider {sdkVersion} plugin: {destination}");
}
finally
{
    Directory.Delete(work, recursive: true);
}

async Task<string> DownloadSdkAsync()
{
    if (!OperatingSystem.IsWindows())
        throw new PlatformNotSupportedException("Pass a local Rider SDK directory to package on this platform.");

    var archivePath = Path.Combine(work, "rider-sdk.zip");
    using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(15) };
    await using (var download = await client.GetStreamAsync(sdkUrl))
    await using (var archive = File.Create(archivePath))
        await download.CopyToAsync(archive);
    await using (var archive = File.OpenRead(archivePath))
        if (Convert.ToHexString(await SHA256.HashDataAsync(archive)) != sdkSha256)
            throw new InvalidDataException($"The downloaded Rider {sdkVersion} SDK does not match its pinned SHA-256.");

    var sdk = Directory.CreateDirectory(Path.Combine(work, "sdk")).FullName;
    using var zip = ZipFile.OpenRead(archivePath);
    foreach (var entry in zip.Entries.Where(entry => entry.Name.Length != 0 && (
                 entry.FullName == "product-info.json" || entry.FullName.StartsWith("jbr/", StringComparison.Ordinal) ||
                 entry.FullName == "lib/" + entry.Name && entry.Name.EndsWith(".jar", StringComparison.Ordinal))))
    {
        var path = Path.Combine(sdk, entry.FullName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        entry.ExtractToFile(path);
    }
    return sdk;
}

static void AddDirectory(ZipArchive archive, string directory, string? excluded = null)
{
    foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        if (file != excluded)
            AddFile(archive, file, Path.GetRelativePath(directory, file).Replace('\\', '/'));
}

static void AddFile(ZipArchive archive, string file, string entryName)
{
    using var source = File.OpenRead(file);
    using var entry = CreateEntry(archive, entryName).Open();
    source.CopyTo(entry);
}

static ZipArchiveEntry CreateEntry(ZipArchive archive, string name)
{
    var entry = archive.CreateEntry(name);
    // ZIP stores DOS timestamps. Use its earliest date for every JAR and ZIP entry.
    entry.LastWriteTime = new DateTimeOffset(1980, 1, 1, 0, 0, 0, TimeSpan.Zero);
    return entry;
}
