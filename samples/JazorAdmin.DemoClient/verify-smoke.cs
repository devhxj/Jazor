#!/usr/bin/env dotnet run

using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

var options = DemoSmokeOptions.Parse(args);
var repoRoot = FindRepositoryRoot(Directory.GetCurrentDirectory());
var runRoot = Path.Combine(repoRoot, ".tmp", "sample-smoke", "JazorAdmin.DemoClient", "run-" + Environment.ProcessId);
EnsureDirectoryWithinRepo(runRoot, repoRoot);
if (Directory.Exists(runRoot))
    Directory.Delete(runRoot, recursive: true);
Directory.CreateDirectory(runRoot);

var adminPort = options.AdminPort == 0 ? ReservePort() : options.AdminPort;
var demoPort = options.DemoPort == 0 ? ReservePort() : options.DemoPort;
if (adminPort == demoPort)
    throw new InvalidOperationException("Admin and DemoClient ports must be different.");

var adminUri = new Uri($"http://127.0.0.1:{adminPort}");
var demoUri = new Uri($"http://127.0.0.1:{demoPort}");
var clientSecret = "demo-smoke-secret-" + Environment.ProcessId;
var databasePath = Path.Combine(runRoot, "jazoradmin.db");
var baseOutputPath = Path.Combine(runRoot, "build-out");
var baseIntermediateOutputPath = Path.Combine(runRoot, "build-obj");
var demoJazorDirectory = Path.Combine(runRoot, "demo-jazor");
var adminAssembly = Path.Combine(baseOutputPath, "JazorAdmin", "bin", options.Configuration, "net11.0", "JazorAdmin.dll");
var demoAssembly = Path.Combine(baseOutputPath, "JazorAdmin.DemoClient", "bin", options.Configuration, "net11.0", "JazorAdmin.DemoClient.dll");

RunningHost? adminHost = null;
RunningHost? demoHost = null;
var completed = false;
try
{
    var buildScript = Path.Combine(repoRoot, "samples", "JazorAdmin", "build-local.cs");
    var build = await RunProcessAsync(
        "dotnet",
        repoRoot,
        [
            "run", "--no-launch-profile", "--file", buildScript, "--",
            "--configuration", options.Configuration,
            "--base-output-path", baseOutputPath,
            "--base-intermediate-output-path", baseIntermediateOutputPath,
            "--demo-jazor-dir", demoJazorDirectory
        ],
        TimeSpan.FromMinutes(20));
    if (build.ExitCode != 0)
        throw new InvalidOperationException("JazorAdmin and DemoClient package build failed." + Environment.NewLine + build);

    AssertFile(adminAssembly, "JazorAdmin host assembly");
    AssertFile(demoAssembly, "DemoClient host assembly");
    AssertDirectory(demoJazorDirectory, "DemoClient generated Jazor directory");
    AssertDemoArtifacts(demoJazorDirectory);

    adminHost = StartHost(
        adminAssembly,
        repoRoot,
        adminUri,
        Path.Combine(runRoot, "admin"),
        new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Development",
            ["DOTNET_ENVIRONMENT"] = "Development",
            ["ConnectionStrings__JazorAdmin"] = "Data Source=" + databasePath,
            ["JazorAdmin__Bootstrap__Email"] = "admin@jazor.local",
            ["JazorAdmin__Bootstrap__Password"] = "JazorAdmin123!",
            ["JazorAdmin__Bootstrap__DisplayName"] = "Platform Administrator",
            ["JazorAdmin__OpenIddict__RedirectUris__0"] = new Uri(adminUri, "/auth/callback").AbsoluteUri,
            ["JazorAdmin__OpenIddict__PostLogoutRedirectUris__0"] = new Uri(adminUri, "/login").AbsoluteUri,
            ["JazorAdmin__DemoClient__ClientId"] = "jazoradmin-demo-client",
            ["JazorAdmin__DemoClient__ClientSecret"] = clientSecret,
            ["JazorAdmin__DemoClient__LaunchUri"] = demoUri.AbsoluteUri,
            ["JazorAdmin__DemoClient__RedirectUris__0"] = new Uri(demoUri, "/signin-oidc").AbsoluteUri,
            ["JazorAdmin__DemoClient__PostLogoutRedirectUris__0"] = new Uri(demoUri, "/signout-callback-oidc").AbsoluteUri,
            ["Logging__LogLevel__Default"] = "Warning"
        });
    demoHost = StartHost(
        demoAssembly,
        repoRoot,
        demoUri,
        Path.Combine(runRoot, "demo"),
        new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Development",
            ["DOTNET_ENVIRONMENT"] = "Development",
            ["JazorAdminDemo__Authority"] = adminUri.AbsoluteUri,
            ["JazorAdminDemo__ClientId"] = "jazoradmin-demo-client",
            ["JazorAdminDemo__ClientSecret"] = clientSecret,
            ["Logging__LogLevel__Default"] = "Warning"
        });

    var cookieContainer = new CookieContainer();
    using var handler = new HttpClientHandler
    {
        AllowAutoRedirect = false,
        UseCookies = true,
        CookieContainer = cookieContainer,
        AutomaticDecompression = DecompressionMethods.All
    };
    using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
    await WaitForHealthAsync(client, adminUri, adminHost);
    await WaitForHealthAsync(client, demoUri, demoHost);

    var challenge = await IssueCaptchaAsync(client, adminUri);
    var captchaAnswer = await ReadCaptchaAnswerAsync(client, adminUri, challenge.ImageUrl);
    using (var login = await client.PostAsJsonAsync(
               new Uri(adminUri, "/api/auth/login"),
               new LoginRequest("admin@jazor.local", "JazorAdmin123!", true, challenge.Id, captchaAnswer),
               DemoSmokeJsonContext.Default.LoginRequest))
    {
        AssertStatus(login, HttpStatusCode.NoContent, "admin CAPTCHA login");
    }

    using (var adminSession = await client.GetAsync(new Uri(adminUri, "/api/auth/session")))
    {
        AssertStatus(adminSession, HttpStatusCode.OK, "admin session after login");
    }

    var signIn = await client.GetAsync(new Uri(demoUri, "/auth/signin?returnUrl=%2F"));
    var authorizeUri = RequireRedirect(signIn, "DemoClient sign-in challenge");
    signIn.Dispose();

    var authorizeResponse = await client.GetAsync(authorizeUri);
    Uri callbackUri;
    if (authorizeResponse.StatusCode == HttpStatusCode.OK)
    {
        var consentHtml = await authorizeResponse.Content.ReadAsStringAsync();
        callbackUri = await AcceptConsentAsync(client, authorizeUri, consentHtml);
    }
    else
    {
        callbackUri = RequireRedirect(authorizeResponse, "OpenID authorization endpoint");
    }
    authorizeResponse.Dispose();

    using (var callbackResponse = await client.GetAsync(callbackUri))
    using (var landingResponse = await FollowRedirectsAsync(client, callbackResponse, callbackUri, 8))
    {
        AssertStatus(landingResponse, HttpStatusCode.OK, "DemoClient OIDC callback landing page");
    }

    using (var demoSession = await client.GetAsync(new Uri(demoUri, "/api/session")))
    {
        AssertStatus(demoSession, HttpStatusCode.OK, "DemoClient authenticated session");
        using var payload = await JsonDocument.ParseAsync(await demoSession.Content.ReadAsStreamAsync());
        var root = payload.RootElement;
        AssertJsonString(root, "name", "Platform Administrator", "DemoClient identity claim");
        if (!root.GetProperty("hasAccessToken").GetBoolean())
            throw new InvalidOperationException("DemoClient session did not retain an access token.");
    }

    using (var protectedOverview = await client.GetAsync(new Uri(demoUri, "/api/platform/overview")))
    {
        AssertStatus(protectedOverview, HttpStatusCode.OK, "DemoClient protected platform API");
        using var payload = await JsonDocument.ParseAsync(await protectedOverview.Content.ReadAsStreamAsync());
        var root = payload.RootElement;
        if (root.GetProperty("accounts").GetInt32() < 1 || root.GetProperty("applications").GetInt32() < 1)
            throw new InvalidOperationException("DemoClient protected overview did not return platform metrics.");
    }

    using (var adminOverview = await client.GetAsync(new Uri(adminUri, "/api/overview/")))
    {
        AssertStatus(adminOverview, HttpStatusCode.OK, "admin overview after OIDC token issuance");
        using var payload = await JsonDocument.ParseAsync(await adminOverview.Content.ReadAsStreamAsync());
        var root = payload.RootElement;
        if (root.GetProperty("tokenIssuances").GetInt32() < 1)
            throw new InvalidOperationException("Admin overview did not record an OIDC token issuance.");
        var portalApplications = root.GetProperty("portalApplications");
        if (!portalApplications.EnumerateArray().Any(item => item.GetProperty("clientId").GetString() == "jazoradmin-demo-client"))
            throw new InvalidOperationException("Admin overview did not expose the configured DemoClient portal entry.");
    }

    using (var auditResponse = await client.GetAsync(new Uri(adminUri, "/api/audit/?object=oidc-token")))
    {
        AssertStatus(auditResponse, HttpStatusCode.OK, "OIDC token audit query");
        using var payload = await JsonDocument.ParseAsync(await auditResponse.Content.ReadAsStreamAsync());
        if (!payload.RootElement.EnumerateArray().Any(item => item.GetProperty("action").GetString() == "issued"))
            throw new InvalidOperationException("OIDC token issuance was not present in the audit log.");
    }

    var signOut = await client.GetAsync(new Uri(demoUri, "/auth/signout"));
    var logoutUri = RequireRedirect(signOut, "DemoClient single logout challenge");
    signOut.Dispose();
    using (var logoutResponse = await client.GetAsync(logoutUri))
    using (var logoutLanding = await FollowRedirectsAsync(client, logoutResponse, logoutUri, 8))
    {
        AssertStatus(logoutLanding, HttpStatusCode.OK, "single logout landing page");
    }

    using (var demoSessionAfterLogout = await client.GetAsync(new Uri(demoUri, "/api/session")))
    {
        AssertDemoSessionChallenge(demoSessionAfterLogout);
    }
    if (cookieContainer.GetCookies(demoUri).Cast<Cookie>().Any(cookie => cookie.Name == "jazoradmin.demo.session"))
        throw new InvalidOperationException("DemoClient authentication cookie remained after single logout.");
    using (var adminSessionAfterLogout = await client.GetAsync(new Uri(adminUri, "/api/auth/session")))
    {
        AssertStatus(adminSessionAfterLogout, HttpStatusCode.Unauthorized, "admin session after single logout");
    }

    Console.WriteLine("JazorAdmin DemoClient smoke verification passed.");
    Console.WriteLine($"Verified: CAPTCHA login, authorization code + PKCE, protected bearer API, audit token issuance, portal registration, and single logout on {adminUri} / {demoUri}.");
    completed = true;
}
finally
{
    if (demoHost is not null)
        await StopHostAsync(demoHost);
    if (adminHost is not null)
        await StopHostAsync(adminHost);
    if (completed)
    {
        TryDeleteDirectory(runRoot);
    }
    else
    {
        Console.Error.WriteLine("DemoClient smoke preserved diagnostics at: " + runRoot);
    }
}

static async Task<CaptchaChallenge> IssueCaptchaAsync(HttpClient client, Uri adminUri)
{
    using var response = await client.GetAsync(new Uri(adminUri, "/api/auth/captcha"));
    AssertStatus(response, HttpStatusCode.OK, "CAPTCHA challenge");
    using var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
    var root = payload.RootElement;
    return new CaptchaChallenge(root.GetProperty("id").GetString()!, root.GetProperty("imageUrl").GetString()!);
}

static async Task<string> ReadCaptchaAnswerAsync(HttpClient client, Uri adminUri, string imageUrl)
{
    using var response = await client.GetAsync(new Uri(adminUri, imageUrl));
    AssertStatus(response, HttpStatusCode.OK, "CAPTCHA image");
    var svg = await response.Content.ReadAsStringAsync();
    var values = Regex.Matches(svg, @"<text[^>]*>(?<value>[^<]+)</text>", RegexOptions.IgnoreCase)
        .Select(match => WebUtility.HtmlDecode(match.Groups["value"].Value))
        .ToArray();
    if (values.Length != 4)
        throw new InvalidOperationException("CAPTCHA SVG did not contain four text glyphs.");
    return string.Concat(values).ToUpperInvariant();
}

static async Task<Uri> AcceptConsentAsync(HttpClient client, Uri authorizeUri, string html)
{
    var formMatch = Regex.Match(html, "<form[^>]+action=\"(?<action>[^\"]+)\"", RegexOptions.IgnoreCase);
    if (!formMatch.Success)
        throw new InvalidOperationException("OpenID authorization returned HTML without a consent form.");

    var fields = Regex.Matches(html, "<input[^>]+type=\"hidden\"[^>]+name=\"(?<name>[^\"]+)\"[^>]+value=\"(?<value>[^\"]*)\"", RegexOptions.IgnoreCase)
        .Select(match => new KeyValuePair<string, string>(
            WebUtility.HtmlDecode(match.Groups["name"].Value),
            WebUtility.HtmlDecode(match.Groups["value"].Value)))
        .Where(pair => !string.Equals(pair.Key, "decision", StringComparison.Ordinal))
        .Append(new KeyValuePair<string, string>("decision", "accept"))
        .ToArray();
    var action = new Uri(authorizeUri, WebUtility.HtmlDecode(formMatch.Groups["action"].Value));
    using var response = await client.PostAsync(action, new FormUrlEncodedContent(fields));
    return RequireRedirect(response, "OpenID consent response");
}

static async Task<HttpResponseMessage> FollowRedirectsAsync(HttpClient client, HttpResponseMessage response, Uri requestUri, int maximum)
{
    var current = response;
    var currentUri = requestUri;
    for (var index = 0; index < maximum && IsRedirect(current.StatusCode); index++)
    {
        var location = RequireRedirect(current, "redirect chain");
        current.Dispose();
        currentUri = location;
        current = await client.GetAsync(currentUri);
    }

    if (IsRedirect(current.StatusCode))
    {
        current.Dispose();
        throw new InvalidOperationException("Redirect chain exceeded " + maximum + " hops from " + currentUri + ".");
    }

    return current;
}

static Uri RequireRedirect(HttpResponseMessage response, string operation)
{
    if (!IsRedirect(response.StatusCode) || response.Headers.Location is null)
        throw new InvalidOperationException(operation + " did not return a redirect: " + (int)response.StatusCode + " " + response.ReasonPhrase);
    return response.Headers.Location.IsAbsoluteUri
        ? response.Headers.Location
        : new Uri(response.RequestMessage?.RequestUri ?? throw new InvalidOperationException(operation + " has no request URI."), response.Headers.Location);
}

static bool IsRedirect(HttpStatusCode status)
    => status is HttpStatusCode.Moved or HttpStatusCode.Redirect or HttpStatusCode.SeeOther or
       HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect;

static async Task WaitForHealthAsync(HttpClient client, Uri baseUri, RunningHost host)
{
    var endpoint = new Uri(baseUri, "/health/live");
    var deadline = DateTime.UtcNow.AddSeconds(90);
    while (DateTime.UtcNow < deadline)
    {
        if (host.Process.HasExited)
            throw new InvalidOperationException("Host exited before health endpoint became ready: " + host.ReadDiagnostics());

        try
        {
            using var response = await client.GetAsync(endpoint);
            if (response.StatusCode == HttpStatusCode.OK)
                return;
        }
        catch (HttpRequestException)
        {
        }
        await Task.Delay(250);
    }

    throw new InvalidOperationException("Timed out waiting for " + endpoint + ". " + host.ReadDiagnostics());
}

static RunningHost StartHost(
    string assemblyPath,
    string workingDirectory,
    Uri baseUri,
    string logRoot,
    IReadOnlyDictionary<string, string?> environment)
{
    Directory.CreateDirectory(logRoot);
    var startInfo = new ProcessStartInfo
    {
        FileName = "dotnet",
        WorkingDirectory = workingDirectory,
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };
    startInfo.ArgumentList.Add(assemblyPath);
    startInfo.ArgumentList.Add("--urls");
    startInfo.ArgumentList.Add(baseUri.AbsoluteUri);
    foreach (var pair in environment)
        startInfo.Environment[pair.Key] = pair.Value;

    var process = new Process { StartInfo = startInfo };
    if (!process.Start())
        throw new InvalidOperationException("Could not start host " + assemblyPath + ".");
    var stdout = DrainAsync(process.StandardOutput, Path.Combine(logRoot, "stdout.log"));
    var stderr = DrainAsync(process.StandardError, Path.Combine(logRoot, "stderr.log"));
    return new RunningHost(process, stdout, stderr, logRoot);
}

static async Task StopHostAsync(RunningHost host)
{
    try
    {
        if (!host.Process.HasExited)
            host.Process.Kill(entireProcessTree: true);
    }
    catch (InvalidOperationException)
    {
    }

    try
    {
        await host.Process.WaitForExitAsync();
    }
    catch (InvalidOperationException)
    {
    }
    await Task.WhenAll(host.StandardOutput, host.StandardError);
    host.Process.Dispose();
}

static async Task<string> DrainAsync(StreamReader reader, string path)
{
    var text = await reader.ReadToEndAsync();
    await File.WriteAllTextAsync(path, text, new UTF8Encoding(false));
    return text;
}

static async Task<ProcessResult> RunProcessAsync(
    string fileName,
    string workingDirectory,
    IReadOnlyList<string> arguments,
    TimeSpan timeout)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = fileName,
        WorkingDirectory = workingDirectory,
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };
    foreach (var argument in arguments)
        startInfo.ArgumentList.Add(argument);

    using var process = new Process { StartInfo = startInfo };
    if (!process.Start())
        throw new InvalidOperationException("Could not start process: " + fileName);
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    var exitTask = process.WaitForExitAsync();
    if (await Task.WhenAny(exitTask, Task.Delay(timeout)) != exitTask)
    {
        try { process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { }
        await exitTask;
        return new ProcessResult(-1, await stdout, await stderr + Environment.NewLine + "Process timed out after " + timeout + ".");
    }

    return new ProcessResult(process.ExitCode, await stdout, await stderr);
}

static int ReservePort()
{
    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    return ((IPEndPoint)listener.LocalEndpoint).Port;
}

static string FindRepositoryRoot(string startDirectory)
{
    var current = new DirectoryInfo(Path.GetFullPath(startDirectory));
    while (current is not null)
    {
        if (File.Exists(Path.Combine(current.FullName, "Jazor.slnx")))
            return current.FullName;
        current = current.Parent;
    }

    throw new InvalidOperationException("Cannot locate repository root (Jazor.slnx).");
}

static void EnsureDirectoryWithinRepo(string path, string repoRoot)
{
    var fullPath = Path.GetFullPath(path);
    var fullRoot = Path.GetFullPath(repoRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
    if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Refusing to operate outside the repository root: " + fullPath);
}

static void TryDeleteDirectory(string path)
{
    try
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }
    catch (IOException error)
    {
        Console.WriteLine("DemoClient smoke could not remove temporary files: " + error.Message);
    }
    catch (UnauthorizedAccessException error)
    {
        Console.WriteLine("DemoClient smoke could not remove temporary files: " + error.Message);
    }
}

static void AssertFile(string path, string description)
{
    if (!File.Exists(path))
        throw new InvalidOperationException("Missing " + description + ": " + path);
}

static void AssertDirectory(string path, string description)
{
    if (!Directory.Exists(path))
        throw new InvalidOperationException("Missing " + description + ": " + path);
}

static void AssertDemoArtifacts(string generatedOutputRoot)
{
    var manifestPath = Path.Combine(generatedOutputRoot, "jazor-manifest.json");
    AssertFile(manifestPath, "DemoClient generated manifest");
    var manifest = File.ReadAllText(manifestPath);
    foreach (var expected in new[]
             {
                 "JazorAdmin.DemoClient.PortalPage",
                 "components/portal-page",
                 "components/portal-api-client"
             })
    {
        if (!manifest.Contains(expected, StringComparison.Ordinal))
            throw new InvalidOperationException("DemoClient generated manifest did not contain '" + expected + "'.");
    }

    var portalModule = Directory.EnumerateFiles(generatedOutputRoot, "portal-page.mjs", SearchOption.AllDirectories).FirstOrDefault();
    AssertFile(portalModule ?? string.Empty, "DemoClient portal render-function module");
    var source = File.ReadAllText(portalModule!);
    foreach (var expected in new[] { "defineComponent", "data-demo-workbench", "data-demo-command", "Protected API" })
    {
        if (!source.Contains(expected, StringComparison.Ordinal))
            throw new InvalidOperationException("DemoClient portal module did not contain '" + expected + "'.");
    }
}

static void AssertStatus(HttpResponseMessage response, HttpStatusCode expected, string operation)
{
    if (response.StatusCode != expected)
        throw new InvalidOperationException(operation + " returned " + (int)response.StatusCode + " " + response.ReasonPhrase + ".");
}

static void AssertDemoSessionChallenge(HttpResponseMessage response)
{
    if (response.StatusCode == HttpStatusCode.Unauthorized)
        return;
    var location = response.Headers.Location;
    var challengePath = location?.IsAbsoluteUri == true
        ? location.PathAndQuery
        : location?.OriginalString;
    if (IsRedirect(response.StatusCode) &&
        (challengePath?.StartsWith("/auth/signin", StringComparison.OrdinalIgnoreCase) == true ||
         challengePath?.StartsWith("/connect/authorize", StringComparison.OrdinalIgnoreCase) == true))
    {
        return;
    }

    throw new InvalidOperationException(
        "DemoClient session after single logout did not challenge anonymously: " +
        (int)response.StatusCode + " " + response.ReasonPhrase + " Location=" + location + ".");
}

static void AssertJsonString(JsonElement root, string property, string expected, string description)
{
    var actual = root.GetProperty(property).GetString();
    if (!string.Equals(actual, expected, StringComparison.Ordinal))
        throw new InvalidOperationException(description + " was '" + actual + "', expected '" + expected + "'.");
}

internal sealed record CaptchaChallenge(string Id, string ImageUrl);

internal sealed record LoginRequest(
    string Email,
    string Password,
    bool RememberMe,
    string CaptchaId,
    string CaptchaAnswer);

internal sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError)
{
    public override string ToString()
        => "ExitCode: " + ExitCode + Environment.NewLine +
           "STDOUT:" + Environment.NewLine + StandardOutput + Environment.NewLine +
           "STDERR:" + Environment.NewLine + StandardError;
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(LoginRequest))]
internal sealed partial class DemoSmokeJsonContext : JsonSerializerContext;

internal sealed class RunningHost(
    Process process,
    Task<string> standardOutput,
    Task<string> standardError,
    string logRoot)
{
    public Process Process { get; } = process;
    public Task<string> StandardOutput { get; } = standardOutput;
    public Task<string> StandardError { get; } = standardError;

    public string ReadDiagnostics()
    {
        var stdout = File.Exists(Path.Combine(logRoot, "stdout.log"))
            ? File.ReadAllText(Path.Combine(logRoot, "stdout.log"))
            : string.Empty;
        var stderr = File.Exists(Path.Combine(logRoot, "stderr.log"))
            ? File.ReadAllText(Path.Combine(logRoot, "stderr.log"))
            : string.Empty;
        return "STDOUT:" + Environment.NewLine + stdout + Environment.NewLine + "STDERR:" + Environment.NewLine + stderr;
    }
}

internal sealed record DemoSmokeOptions(string Configuration, int AdminPort, int DemoPort)
{
    public static DemoSmokeOptions Parse(IReadOnlyList<string> arguments)
    {
        var configuration = "Debug";
        var adminPort = 0;
        var demoPort = 0;
        for (var index = 0; index < arguments.Count; index++)
        {
            switch (arguments[index])
            {
                case "--configuration":
                case "-Configuration":
                case "-c":
                    configuration = RequireValue(arguments, ref index, arguments[index]);
                    break;
                case "--admin-port":
                    adminPort = int.Parse(RequireValue(arguments, ref index, arguments[index]));
                    break;
                case "--demo-port":
                    demoPort = int.Parse(RequireValue(arguments, ref index, arguments[index]));
                    break;
                case "--help":
                case "-h":
                    Console.WriteLine("Usage: dotnet run --no-launch-profile --file samples/JazorAdmin.DemoClient/verify-smoke.cs -- [--configuration Debug|Release] [--admin-port port] [--demo-port port]");
                    Environment.Exit(0);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported argument: " + arguments[index]);
            }
        }

        return new DemoSmokeOptions(configuration, adminPort, demoPort);
    }

    private static string RequireValue(IReadOnlyList<string> arguments, ref int index, string option)
    {
        if (++index >= arguments.Count)
            throw new InvalidOperationException("Missing value for " + option + ".");
        return arguments[index];
    }
}
