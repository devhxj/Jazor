using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Jazor.EmitTest;

internal static class BrowserSmokeTestHelper
{
    public static async Task<BrowserSmokeProcessResult> RunBrowserDumpDomAsync(
        string browserPath,
        string indexPath,
        int virtualTimeBudgetMilliseconds = 5000)
    {
        var harnessRoot = Path.GetDirectoryName(indexPath)!;
        return await RunBrowserDumpDomAsync(
            browserPath,
            new Uri(Path.GetFullPath(indexPath)),
            harnessRoot,
            virtualTimeBudgetMilliseconds);
    }

    public static async Task<BrowserSmokeProcessResult> RunBrowserDumpDomAsync(
        string browserPath,
        Uri pageUri,
        string workingDirectory,
        int virtualTimeBudgetMilliseconds = 5000)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(browserPath);
        ArgumentNullException.ThrowIfNull(pageUri);
        ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory);

        var userDataRoot = Path.Combine(workingDirectory, ".browser-profile-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(userDataRoot);

        try
        {
            return await RunProcessAsync(
                browserPath,
                workingDirectory,
                [
                    "--headless=new",
                    "--disable-gpu",
                    "--disable-dev-shm-usage",
                    "--no-first-run",
                    "--no-default-browser-check",
                    "--no-sandbox",
                    "--allow-file-access-from-files",
                    "--run-all-compositor-stages-before-draw",
                    $"--virtual-time-budget={virtualTimeBudgetMilliseconds}",
                    "--dump-dom",
                    $"--user-data-dir={userDataRoot}",
                    pageUri.AbsoluteUri
                ],
                TimeSpan.FromSeconds(45));
        }
        finally
        {
            try
            {
                if (Directory.Exists(userDataRoot))
                    Directory.Delete(userDataRoot, recursive: true);
            }
            catch
            {
            }
        }
    }

    public static string? ResolveBrowserExecutable()
    {
        var explicitPath = Environment.GetEnvironmentVariable("RAZORVUE_BROWSER_EXE")?.Trim();
        if (string.IsNullOrWhiteSpace(explicitPath))
            explicitPath = Environment.GetEnvironmentVariable("RAZORVUE_BROWSER_PATH")?.Trim();

        if (!string.IsNullOrWhiteSpace(explicitPath))
            return File.Exists(explicitPath) ? explicitPath : null;

        var candidates = OperatingSystem.IsWindows()
            ? new[]
            {
                @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                "msedge.exe",
                "chrome.exe"
            }
            : OperatingSystem.IsMacOS()
                ? new[]
                {
                    "/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge",
                    "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
                    "microsoft-edge",
                    "google-chrome",
                    "chromium"
                }
                : new[]
                {
                    "microsoft-edge",
                    "microsoft-edge-stable",
                    "google-chrome",
                    "google-chrome-stable",
                    "chromium",
                    "chromium-browser"
                };

        foreach (var candidate in candidates)
        {
            var resolved = TryResolveBrowserExecutable(candidate);
            if (resolved is not null)
                return resolved;
        }

        return null;
    }

    public static JsonDocument ReadBrowserSmokePayload(BrowserSmokeProcessResult browser, string markerDescription)
    {
        var match = Regex.Match(
            browser.StandardOutput,
            "data-jazor-smoke=\"(?<payload>[A-Za-z0-9+/=]+)\"",
            RegexOptions.CultureInvariant);
        Assert.IsTrue(
            match.Success,
            $"Browser DOM did not contain the {markerDescription} smoke result marker." + Environment.NewLine + browser);

        var json = Encoding.UTF8.GetString(Convert.FromBase64String(match.Groups["payload"].Value));
        return JsonDocument.Parse(json);
    }

    private static async Task<BrowserSmokeProcessResult> RunProcessAsync(
        string fileName,
        string workingDirectory,
        IReadOnlyList<string> arguments,
        TimeSpan timeout)
    {
        var startInfo = new ProcessStartInfo(fileName)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();

        var timedOut = false;
        using var timeoutSource = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(timeoutSource.Token);
        }
        catch (OperationCanceledException)
        {
            timedOut = true;
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
            }

            await process.WaitForExitAsync();
        }

        var output = await standardOutput;
        var error = await standardError;
        return timedOut
            ? new BrowserSmokeProcessResult(-1, output, $"Process timed out after {timeout}." + Environment.NewLine + error)
            : new BrowserSmokeProcessResult(process.ExitCode, output, error);
    }

    private static string? TryResolveBrowserExecutable(string candidate)
    {
        if (candidate.Contains(Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
            candidate.Contains(Path.AltDirectorySeparatorChar, StringComparison.Ordinal) ||
            candidate.Contains(':', StringComparison.Ordinal))
        {
            return File.Exists(candidate) ? candidate : null;
        }

        var path = Environment.GetEnvironmentVariable("PATH") ?? "";
        var extensions = OperatingSystem.IsWindows()
            ? (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT")
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
            : [""];

        foreach (var directory in path.Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(directory))
                continue;

            foreach (var extension in extensions)
            {
                var fileName = candidate.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                    ? candidate
                    : candidate + extension;
                var fullPath = Path.Combine(directory, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }
        }

        return null;
    }
}

internal sealed record BrowserSmokeProcessResult(int ExitCode, string StandardOutput, string StandardError)
{
    public override string ToString()
        => $"ExitCode: {ExitCode}{Environment.NewLine}STDOUT:{Environment.NewLine}{StandardOutput}{Environment.NewLine}STDERR:{Environment.NewLine}{StandardError}";
}
