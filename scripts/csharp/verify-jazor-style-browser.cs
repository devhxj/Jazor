#!/usr/bin/env dotnet run
#:project ../../src/Jazor.Style/Jazor.Style.csproj
#:property NoWarn=IL2026;IL2075

using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

var repoRoot = RequireRepoRoot();
var browserPath = ResolveBrowserExecutable()
    ?? throw new FileNotFoundException(
        "Microsoft Edge, Google Chrome, or Chromium is required for the Jazor.Style browser smoke.");
var root = Path.Combine(repoRoot, ".tmp", "jazor-css-browser-" + Environment.ProcessId);
var profileRoot = Path.Combine(root, "browser-profile");

EnsureDirectoryDeletedWithinRepo(repoRoot, root);
Directory.CreateDirectory(root);

try
{
    var assembly = typeof(global::Jazor.Style.css).Assembly;
    var runtime = ReadSingleCatalogItem(assembly, "Jazor.Generated.ModuleCatalog");
    var runtimePath = Path.Combine(root, ReadProperty(runtime, "RelativePath").Replace('/', Path.DirectorySeparatorChar));
    Directory.CreateDirectory(Path.GetDirectoryName(runtimePath)!);
    await File.WriteAllTextAsync(
        runtimePath,
        ReadProperty(runtime, "Content"),
        new UTF8Encoding(false));

    var sourceMap = ReadSingleCatalogItem(assembly, "Jazor.Generated.ModuleSourceMapCatalog");
    await File.WriteAllTextAsync(
        Path.Combine(root, ReadProperty(sourceMap, "SourceMapRelativePath").Replace('/', Path.DirectorySeparatorChar)),
        ReadProperty(sourceMap, "SourceMapContent"),
        new UTF8Encoding(false));

    var indexPath = Path.Combine(root, "index.html");
    await File.WriteAllTextAsync(indexPath, GetBrowserHarness(), new UTF8Encoding(false));

    var startInfo = new ProcessStartInfo(browserPath)
    {
        WorkingDirectory = root,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    foreach (var argument in new[]
    {
        "--headless=new",
        "--disable-gpu",
        "--disable-dev-shm-usage",
        "--no-first-run",
        "--no-default-browser-check",
        "--no-sandbox",
        "--allow-file-access-from-files",
        "--run-all-compositor-stages-before-draw",
        "--virtual-time-budget=5000",
        "--dump-dom",
        "--user-data-dir=" + profileRoot,
        new Uri(Path.GetFullPath(indexPath)).AbsoluteUri
    })
    {
        startInfo.ArgumentList.Add(argument);
    }

    using var process = Process.Start(startInfo)
        ?? throw new InvalidOperationException("The browser process could not be started.");
    var standardOutput = process.StandardOutput.ReadToEndAsync();
    var standardError = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(45));
    var output = await standardOutput;
    var error = await standardError;
    if (process.ExitCode != 0)
    {
        throw new InvalidOperationException(
            $"Browser exited with code {process.ExitCode}.{Environment.NewLine}{error}");
    }

    var match = Regex.Match(
        output,
        "data-jazor-css-smoke=\"(?<payload>[A-Za-z0-9+/=]+)\"",
        RegexOptions.CultureInvariant);
    if (!match.Success)
    {
        throw new InvalidOperationException(
            "Browser DOM did not contain the Jazor.Style smoke marker." + Environment.NewLine + output + Environment.NewLine + error);
    }

    var json = Encoding.UTF8.GetString(Convert.FromBase64String(match.Groups["payload"].Value));
    using var result = JsonDocument.Parse(json);
    var payload = result.RootElement;
    if (!payload.GetProperty("ok").GetBoolean())
        throw new InvalidOperationException("Jazor.Style browser smoke failed: " + payload.GetRawText());

    RequireEqual(payload, "firstName", payload.GetProperty("secondName").GetString());
    RequireEqual(payload, "backgroundColor", "rgb(23, 105, 170)");
    RequireEqual(payload, "display", "inline-flex");
    RequireEqual(payload, "nonce", "jazor-css-nonce");
    RequireIntEqual(payload, "styleCount", 1);
    RequireTrue(payload, "unchangedAfterReload");
    RequireTrue(payload, "ownedStyle");
    RequireTrue(payload, "unicodeExtracted");
    RequireEqual(payload, "shadowColor", "rgb(0, 128, 0)");
    RequireIntEqual(payload, "shadowStyleCount", 1);
    RequireTrue(payload, "shadowAdopted");
    RequireTrue(payload, "hydrationAdopted");
    RequireIntEqual(payload, "hydrationStyleCount", 1);

    Console.WriteLine("Jazor.Style browser verification passed.");
    Console.WriteLine(payload.GetRawText());
}
finally
{
    EnsureDirectoryDeletedWithinRepo(repoRoot, root);
}

static object ReadSingleCatalogItem(Assembly assembly, string catalogTypeName)
{
    var catalogType = assembly.GetType(catalogTypeName, throwOnError: true, ignoreCase: false)!;
    var getModules = catalogType.GetMethod(
        "GetModules",
        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException(catalogTypeName + " does not expose GetModules().");
    var items = ((IEnumerable?)getModules.Invoke(null, null))?.Cast<object>().ToArray()
        ?? throw new InvalidOperationException(catalogTypeName + " returned null.");
    return items.Single();
}

static string ReadProperty(object item, string name)
{
    var property = item.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("Catalog item does not expose " + name + ".");
    return (string?)property.GetValue(item) ?? string.Empty;
}

static void RequireEqual(JsonElement payload, string name, string? expected)
{
    var actual = payload.GetProperty(name).GetString();
    if (!string.Equals(actual, expected, StringComparison.Ordinal))
        throw new InvalidOperationException($"Expected {name} '{expected}', but found '{actual}'.");
}

static void RequireIntEqual(JsonElement payload, string name, int expected)
{
    var actual = payload.GetProperty(name).GetInt32();
    if (actual != expected)
        throw new InvalidOperationException($"Expected {name} {expected}, but found {actual}.");
}

static void RequireTrue(JsonElement payload, string name)
{
    if (!payload.GetProperty(name).GetBoolean())
        throw new InvalidOperationException("Expected " + name + " to be true.");
}

static string RequireRepoRoot()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
            return directory.FullName;

        directory = directory.Parent;
    }

    throw new DirectoryNotFoundException("Repository root containing Jazor.slnx was not found.");
}

static void EnsureDirectoryDeletedWithinRepo(string repoRoot, string path)
{
    var fullRepoRoot = Path.GetFullPath(repoRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
    var fullPath = Path.GetFullPath(path);
    if (!fullPath.StartsWith(fullRepoRoot, StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Refusing to delete a directory outside the repository: " + fullPath);

    if (Directory.Exists(fullPath))
        Directory.Delete(fullPath, recursive: true);
}

static string? ResolveBrowserExecutable()
{
    var explicitPath = Environment.GetEnvironmentVariable("JAZOR_CSS_BROWSER_EXE")?.Trim();
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

    return candidates.Select(TryResolveExecutable).FirstOrDefault(static path => path is not null);
}

static string? TryResolveExecutable(string candidate)
{
    if (Path.IsPathFullyQualified(candidate))
        return File.Exists(candidate) ? candidate : null;

    var extensions = OperatingSystem.IsWindows()
        ? (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT")
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
        : [""];
    foreach (var directory in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
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

static string GetBrowserHarness() => """
    <!doctype html>
    <html lang="en">
      <head>
        <meta charset="utf-8">
        <title>Jazor.Style browser smoke</title>
      </head>
      <body>
        <button id="target">Styled</button>
        <script type="module">
          import * as firstModule from "./Jazor.Style/runtime.mjs";
          import * as secondModule from "./Jazor.Style/runtime.mjs?hmr=1";

          function finish(value) {
            const bytes = new TextEncoder().encode(JSON.stringify(value));
            let binary = "";
            for (const byte of bytes) binary += String.fromCharCode(byte);
            document.documentElement.setAttribute("data-jazor-css-smoke", btoa(binary));
          }

          try {
            firstModule.configure({ nonce: "jazor-css-nonce" });
            const rule = {
              display: "inline-flex",
              "background-color": "rgb(23, 105, 170)",
              color: "white",
              content: "'\u6c49\u5b57'"
            };
            const target = document.getElementById("target");
            const firstName = firstModule.style(rule);
            target.className = firstName;
            const style = document.getElementById("jazor-css");
            const beforeReload = style.textContent;
            const firstComputed = getComputedStyle(target);
            const backgroundColor = firstComputed.backgroundColor;
            const display = firstComputed.display;

            secondModule.configure({ nonce: "jazor-css-nonce" });
            const secondName = secondModule.style({
              color: "white",
              content: "'\u6c49\u5b57'",
              "background-color": "rgb(23, 105, 170)",
              display: "inline-flex"
            });

            const shadowHost = document.createElement("div");
            document.body.appendChild(shadowHost);
            const shadowRoot = shadowHost.attachShadow({ mode: "open" });
            const shadowButton = document.createElement("button");
            shadowRoot.appendChild(shadowButton);
            const shadowContext = firstModule.createContext({ target: shadowRoot, styleId: "shadow-css" });
            shadowButton.className = firstModule.classIn(shadowContext, { color: "rgb(0, 128, 0)" });
            const shadowStyle = shadowRoot.getElementById("shadow-css");
            const shadowBeforeAdoption = shadowStyle.textContent;
            const reloadedShadowContext = secondModule.createContext({ target: shadowRoot, styleId: "shadow-css" });
            secondModule.classIn(reloadedShadowContext, { color: "rgb(0, 128, 0)" });

            const serverContext = firstModule.createContext({
              detached: true,
              styleId: "hydrated-css",
              nonce: "jazor-css-nonce"
            });
            const hydrationRule = { "border-top-width": "3px", "border-top-style": "solid" };
            const serverName = firstModule.classIn(serverContext, hydrationRule);
            const serverSnapshot = firstModule.snapshotFrom(serverContext);
            const hydrationStyle = document.createElement("style");
            hydrationStyle.id = serverSnapshot.styleId;
            hydrationStyle.nonce = serverSnapshot.nonce;
            hydrationStyle.textContent = serverSnapshot.hydrationText;
            document.head.appendChild(hydrationStyle);
            const hydrationBeforeAdoption = hydrationStyle.textContent;
            const browserContext = secondModule.createContext({
              styleId: "hydrated-css",
              nonce: "jazor-css-nonce"
            });
            const browserName = secondModule.classIn(browserContext, hydrationRule);

            finish({
              ok: true,
              firstName,
              secondName,
              backgroundColor,
              display,
              nonce: style.nonce,
              styleCount: document.querySelectorAll("style#jazor-css").length,
              unchangedAfterReload: beforeReload === style.textContent,
              ownedStyle: style.textContent.startsWith("/*jazor-css:v1*//*jz:v1:"),
              unicodeExtracted: secondModule.extract().includes("\u6c49\u5b57"),
              shadowColor: getComputedStyle(shadowButton).color,
              shadowStyleCount: shadowRoot.querySelectorAll("style#shadow-css").length,
              shadowAdopted: shadowBeforeAdoption === shadowStyle.textContent,
              hydrationAdopted:
                serverName === browserName && hydrationBeforeAdoption === hydrationStyle.textContent,
              hydrationStyleCount: document.querySelectorAll("style#hydrated-css").length
            });
          } catch (error) {
            finish({ ok: false, error: error?.stack || String(error) });
          }
        </script>
      </body>
    </html>
    """;
