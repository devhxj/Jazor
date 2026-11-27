#!/usr/bin/env dotnet run
#:sdk Microsoft.NET.Sdk.Web
#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.NodeHost/Jazor.NodeHost.csproj

using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Jazor.NodeHost;

if (args is not ([_] or [_, "--browser", _]))
    throw new ArgumentException("Usage: verify-jolt-npm-runtime.cs <Jolt CLI apphost> [--browser <Chromium executable>]");

var cli = Path.GetFullPath(args[0]);
var scripts = Path.GetDirectoryName(ScriptPath())!;
var repo = Path.GetFullPath(Path.Combine(scripts, "..", ".."));
var fixture = Path.Combine(scripts, "fixtures", "jolt-npm-runtime");
var store = Path.GetFullPath(Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory")
    ?? Path.Combine(repo, ".tmp", "nodeapi-store"));
using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(2));
var stop = deadline.Token;
var project = Directory.CreateDirectory(Path.Combine(repo, ".tmp", $"jolt-npm-runtime-{Guid.NewGuid():N}"));
try
{
    foreach (var name in new[] { "package.json", "pnpm-lock.yaml", "App.jazor", "Floating.jazor", "Vee.jazor",
        "Query.jazor", "Events.jazor", "Behavior.jazor", "Getters.jazor", "Constant.jazor", "verify.mjs", "verify-query.mjs",
        "verify-events.mjs", "verify-behavior.mjs", "verify-getters.mjs", "Submit.jazor", "FormScope.jazor", "verify-submit.mjs", "verify-i18n.mjs",
        "Storage.jazor", "verify-storage.mjs", "Model.jazor", "verify-model.mjs", "UrlParams.jazor", "verify-url-params.mjs",
        "ColorMode.jazor", "verify-color-mode.mjs", "Clipboard.jazor", "verify-clipboard.mjs", "EventHook.jazor" })
        File.Copy(Path.Combine(fixture, name), Path.Combine(project.FullName, name));
    File.Copy(Path.Combine(scripts, "fixtures", "jolt-binding-projection", "I18nPlugin.jazor"),
        Path.Combine(project.FullName, "I18nPlugin.jazor"));

    await NodeToolchain.RestoreAsync(project.FullName, store, stop, Console.Out);
    deadline.CancelAfter(TimeSpan.FromMinutes(2));
    var build = await Build();
    if (build.ExitCode != 0)
        throw new InvalidOperationException(build.Output);

    deadline.CancelAfter(TimeSpan.FromMinutes(2));
    Console.WriteLine(await NodeModuleRunner.RunAsync(project.FullName,
        Path.Combine(project.FullName, "verify.mjs"), cancellationToken: stop));
    Console.WriteLine(await NodeModuleRunner.RunAsync(project.FullName,
        Path.Combine(project.FullName, "verify-events.mjs"), cancellationToken: stop));
    Console.WriteLine(await NodeModuleRunner.RunAsync(project.FullName,
        Path.Combine(project.FullName, "verify-behavior.mjs"), cancellationToken: stop));
    Console.WriteLine(await NodeModuleRunner.RunAsync(project.FullName,
        Path.Combine(project.FullName, "verify-submit.mjs"), cancellationToken: stop));
    Console.WriteLine(await NodeModuleRunner.RunAsync(project.FullName,
        Path.Combine(project.FullName, "verify-getters.mjs"), cancellationToken: stop));
    Console.WriteLine(await NodeModuleRunner.RunAsync(project.FullName,
        Path.Combine(project.FullName, "verify-query.mjs"), cancellationToken: stop));
    Console.WriteLine(await NodeModuleRunner.RunAsync(project.FullName,
        Path.Combine(project.FullName, "verify-i18n.mjs"), cancellationToken: stop));
    Console.WriteLine(await NodeModuleRunner.RunAsync(project.FullName,
        Path.Combine(project.FullName, "verify-storage.mjs"), cancellationToken: stop));
    Console.WriteLine(await NodeModuleRunner.RunAsync(project.FullName,
        Path.Combine(project.FullName, "verify-model.mjs"), cancellationToken: stop));
    Console.WriteLine(await NodeModuleRunner.RunAsync(project.FullName,
        Path.Combine(project.FullName, "verify-url-params.mjs"), cancellationToken: stop));
    Console.WriteLine(await NodeModuleRunner.RunAsync(project.FullName,
        Path.Combine(project.FullName, "verify-color-mode.mjs"), cancellationToken: stop));
    Console.WriteLine(await NodeModuleRunner.RunAsync(project.FullName,
        Path.Combine(project.FullName, "verify-clipboard.mjs"), cancellationToken: stop));

    // Compile all invalid components together so each locked package is read once.
    var getterComponent = Path.Combine(project.FullName, "Getters.jazor");
    var getterSource = await File.ReadAllTextAsync(getterComponent, stop);
    await File.WriteAllTextAsync(getterComponent, getterSource
        .Replace("count.Value = 4;", "calculated.Value = 4;", StringComparison.Ordinal)
        .Replace("VueUse.UseKeyModifierModifierInput.Shift", "\"NoSuchKey\"", StringComparison.Ordinal)
        .Replace("() => count.Value > 2 ? \"high\" : \"low\"", "() => true", StringComparison.Ordinal), stop);
    var fieldComponent = Path.Combine(project.FullName, "Vee.jazor");
    var fieldSource = await File.ReadAllTextAsync(fieldComponent, stop);
    await File.WriteAllTextAsync(fieldComponent, fieldSource.Replace(
        "value => value.Address == \"ada@example.test\" ? true : \"invalid-email\"", "(bool value) => value", StringComparison.Ordinal)
        .Replace("double? numberValue", "bool? numberValue", StringComparison.Ordinal)
        .Replace("bool? booleanValue", "double? booleanValue", StringComparison.Ordinal)
        .Replace("typedArray.Push(\"pushed\")", "typedArray.Push(true)", StringComparison.Ordinal), stop);
    await File.WriteAllTextAsync(Path.Combine(project.FullName, "InvalidInitialValue.jazor"), fieldSource.Replace(
        "InitialValue = new Email(\"invalid\")", "InitialValue = true", StringComparison.Ordinal), stop);

    var queryComponent = Path.Combine(project.FullName, "Query.jazor");
    var querySource = await File.ReadAllTextAsync(queryComponent, stop);
    await File.WriteAllTextAsync(queryComponent,
        querySource.Replace("new QueryData(\"fetched\")", "true", StringComparison.Ordinal)
            .Replace("new QueryFilter(3)", "new ECMAScript.Date(0)", StringComparison.Ordinal)
            .Replace("InitialPageParam = 0", "InitialPageParam = true", StringComparison.Ordinal)
            .Replace("lastPageParam == 0 ? 1 : null", "lastPageParam == 0 ? \"wrong\" : null", StringComparison.Ordinal)
            .Replace("MutateAsync(\"fixture\")", "MutateAsync(true)", StringComparison.Ordinal), stop);
    var component = Path.Combine(project.FullName, "App.jazor");
    var source = await File.ReadAllTextAsync(component, stop);
    await File.WriteAllTextAsync(component,
        source.Replace("VueUse.IsClient ?", "VueUse.IsClient() ?", StringComparison.Ordinal)
            .Replace("Format = DateFns.ISOStringFormat.Basic", "Format = \"basic\"", StringComparison.Ordinal)
            .Replace("), 1).GetDate()", "), true).GetDate()", StringComparison.Ordinal)
            .Replace("DateFns.Min(choices)", "DateFns.Min(new ECMAScript.Date[] { date })", StringComparison.Ordinal)
            .Replace("WeekStartsOn = Day.Value1", "WeekStartsOn = FirstWeekContainsDate.Value4", StringComparison.Ordinal)
            .Replace("FirstWeekContainsDate = FirstWeekContainsDate.Value4", "FirstWeekContainsDate = 0", StringComparison.Ordinal)
            .Replace("Locale = DateLocales.Fr", "Locale = start", StringComparison.Ordinal), stop);
    var eventComponent = Path.Combine(project.FullName, "Events.jazor");
    var eventSource = await File.ReadAllTextAsync(eventComponent, stop);
    await File.WriteAllTextAsync(eventComponent, eventSource.Replace("Target, \"click\", OnEvent",
        "Target, true, OnEvent", StringComparison.Ordinal)
        .Replace("Keydown, OnKeyboard", "Keydown, OnClickOnly", StringComparison.Ordinal)
        .Replace("    private void OnKeyboard", "    private void OnClickOnly(ECMAScript.MouseEvent value) { }\n\n    private void OnKeyboard", StringComparison.Ordinal)
        .Replace("var stopKeyboard", "VueUse.UseEventListener(Target, VueUse.KeyboardEventName.Click, OnKeyboard);\n        var stopKeyboard", StringComparison.Ordinal), stop);
    deadline.CancelAfter(TimeSpan.FromMinutes(2));
    var behaviorComponent = Path.Combine(project.FullName, "Behavior.jazor");
    var behaviorSource = await File.ReadAllTextAsync(behaviorComponent, stop);
    await File.WriteAllTextAsync(behaviorComponent, behaviorSource
        .Replace("debounce.Invoke(\"keep\")", "debounce.Invoke(true)", StringComparison.Ordinal)
        .Replace("timer.Start(\"timer\")", "timer.Start(true)", StringComparison.Ordinal), stop);
    var modelComponent = Path.Combine(project.FullName, "Model.jazor");
    var modelSource = await File.ReadAllTextAsync(modelComponent, stop);
    await File.WriteAllTextAsync(modelComponent,
        modelSource.Replace("_model.Value = 0", "_model.Value = false", StringComparison.Ordinal)
            .Replace("(double value) => value + 1", "(double value) => true", StringComparison.Ordinal)
            .Replace("string Label);", "string Label, IVueRef<double> RawCount = default!);", StringComparison.Ordinal), stop);
    var urlComponent = Path.Combine(project.FullName, "UrlParams.jazor");
    var urlSource = await File.ReadAllTextAsync(urlComponent, stop);
    await File.WriteAllTextAsync(urlComponent, urlSource.Replace("Current = () =>", "query.Set(\"tag\", false);\n        query.Set(\"tag\", null);\n        Current = () =>",
        StringComparison.Ordinal), stop);
    var clipboardComponent = Path.Combine(project.FullName, "Clipboard.jazor");
    var clipboardSource = await File.ReadAllTextAsync(clipboardComponent, stop);
    await File.WriteAllTextAsync(clipboardComponent,
        clipboardSource.Replace("Copy(\"plain\")", "Copy(true)", StringComparison.Ordinal), stop);
    var invalid = await Build();
    string[] expected = [
        @"Getters\.jazor.*CS0200", @"Getters\.jazor.*CS0029", @"Getters\.jazor.*CS1503",
        @"Vee\.jazor.*CS1660.*string", @"Vee\.jazor.*CS0029.*double.*bool", @"Vee\.jazor.*CS1503.*bool.*string",
        @"Vee\.jazor.*CS0029.*bool.*double", @"InvalidInitialValue\.jazor.*CS0029",
        @"App\.jazor.*CS1955", @"App\.jazor.*CS1503.*ECMAScript\.Date\[\]", @"App\.jazor.*CS1503.*bool",
        @"App\.jazor.*CS0029.*FirstWeekContainsDate.*Day", @"App\.jazor.*CS0029.*int.*FirstWeekContainsDate",
        @"App\.jazor.*CS0029.*ISOStringFormat", @"App\.jazor.*CS0029.*Locale",
        @"Query\.jazor.*CS0266", @"Query\.jazor.*CS1503", @"Query\.jazor.*CS0029.*ECMAScript\.Date",
        @"Query\.jazor.*CS0029.*bool.*double", @"Query\.jazor.*CS0029.*string.*double",
        @"Events\.jazor.*CS0117.*Click",
        @"Model\.jazor.*CS0029.*bool.*double",
        @"Clipboard\.jazor.*CS1503.*bool",
        @"Model\.jazor.*CS1662",
        @"Model\.jazor.*Record\.Info\.RawCount.*Vue unwraps refs at ordinary object keys",
        @"UrlParams\.jazor.*CS1503.*bool",
    ];
    var missing = expected.Where(pattern => !Regex.IsMatch(invalid.Output, pattern)).ToArray();
    if (invalid.ExitCode == 0 || missing.Length > 0 ||
        !Regex.IsMatch(invalid.Output, @"UrlParams\.jazor.*CS0037") ||
        Regex.Matches(invalid.Output, @"Events\.jazor.*CS1503").Count < 2 ||
        Regex.Matches(invalid.Output, @"Behavior\.jazor.*CS1503").Count < 2)
        throw new InvalidOperationException($"Projection diagnostic checks failed. Missing patterns [{string.Join(", ", missing)}].\n{invalid.Output}");
    Console.WriteLine("Jolt rejected invalid VueUse, VeeValidate, date-fns and Vue Query calls at the C# use sites.");

    if (args is [_, "--browser", var browser])
        await VerifyMonaco(Path.GetFullPath(browser));
}
finally
{
    project.Delete(recursive: true);
}

async Task VerifyMonaco(string browser)
{
    deadline.CancelAfter(TimeSpan.FromMinutes(2));
    var workspace = project.CreateSubdirectory("monaco").FullName;
    var inputs = Path.Combine(scripts, "fixtures", "jolt-monaco-browser");
    foreach (var name in new[] { "App.jazor", "index.html", "main.js", "editor-worker.js", "package.json", "pnpm-lock.yaml" })
        File.Copy(Path.Combine(inputs, name), Path.Combine(workspace, name));
    await NodeToolchain.RestoreAsync(workspace, store, stop, Console.Out);

    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    var port = ((IPEndPoint)listener.LocalEndpoint).Port.ToString(CultureInfo.InvariantCulture);
    listener.Stop();
    var dev = StartInfo(cli, workspace, "dev", ".", "--port", port);
    dev.RedirectStandardOutput = true;
    using (var server = Process.Start(dev) ?? throw new InvalidOperationException("Could not start Jolt."))
    {
        try
        {
            Uri? origin = null;
            while (await server.StandardOutput.ReadLineAsync(stop) is { } line)
            {
                Console.WriteLine(line);
                if (Uri.TryCreate(line, UriKind.Absolute, out var candidate) && candidate.Scheme == "http" && candidate.IsLoopback)
                {
                    origin = candidate;
                    break;
                }
            }
            await VerifyBrowserAsync(origin ?? throw new InvalidOperationException("Jolt stopped before starting the Monaco development server."));
        }
        finally
        {
            await Stop(server);
        }
    }

    using (var bundle = Process.Start(StartInfo(cli, workspace, "bundle", "."))
        ?? throw new InvalidOperationException("Could not start the Monaco production bundle build."))
    {
        try
        {
            await bundle.WaitForExitAsync(stop);
            if (bundle.ExitCode != 0)
                throw new InvalidOperationException($"Monaco production bundle build exited with code {bundle.ExitCode}.");
        }
        finally
        {
            await Stop(bundle);
        }
    }

    var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { WebRootPath = Path.Combine(workspace, ".jazor", "dist") });
    await using var production = builder.Build();
    production.Urls.Add("http://127.0.0.1:0");
    int workerResponses = 0;
    production.Use(async (context, next) =>
    {
        // A fetch of the same URL by the page would not prove Chromium loaded a worker.
        if (context.Request.Headers["Sec-Fetch-Dest"] == "worker" && context.Request.Path.StartsWithSegments("/assets"))
            context.Response.OnStarting(() =>
            {
                if (context.Response.StatusCode == 200)
                    Interlocked.Increment(ref workerResponses);
                return Task.CompletedTask;
            });
        await next(context);
    });
    production.UseDefaultFiles();
    production.UseStaticFiles();
    await production.StartAsync(stop);
    await VerifyBrowserAsync(new Uri(production.Urls.Single()));
    if (Volatile.Read(ref workerResponses) == 0)
        throw new InvalidOperationException("Chromium did not fetch the bundled Monaco worker from the production server.");
    await production.StopAsync(stop);
    Console.WriteLine("Verified production Monaco worker loading and model synchronization in Chromium.");

    async Task VerifyBrowserAsync(Uri origin)
    {
        var driver = Path.Combine(repo, "samples", "Jolt.Counter", "verify-browser.cs");
        using var check = Process.Start(StartInfo("dotnet", workspace, "run", "--file", driver,
            "--", browser, origin.AbsoluteUri, "--monaco"))
            ?? throw new InvalidOperationException("Could not start the Chromium check.");
        try
        {
            await check.WaitForExitAsync(stop);
            if (check.ExitCode != 0)
                throw new InvalidOperationException($"Monaco browser check exited with code {check.ExitCode} at {origin}.");
        }
        finally
        {
            await Stop(check);
        }
    }
}

async Task<(int ExitCode, string Output)> Build()
{
    deadline.CancelAfter(TimeSpan.FromMinutes(2));
    var start = StartInfo(cli, project.FullName, "build");
    start.RedirectStandardOutput = true;
    start.RedirectStandardError = true;
    using var process = Process.Start(start) ?? throw new InvalidOperationException($"Cannot start {cli}.");
    var output = process.StandardOutput.ReadToEndAsync(stop);
    var errors = process.StandardError.ReadToEndAsync(stop);
    try
    {
        await process.WaitForExitAsync(stop);
        return (process.ExitCode, (await output) + (await errors));
    }
    finally
    {
        await Stop(process);
    }
}

ProcessStartInfo StartInfo(string command, string directory, params string[] arguments)
{
    var start = new ProcessStartInfo(command) { WorkingDirectory = directory, UseShellExecute = false };
    start.Environment["JazorPnpmStoreDirectory"] = store;
    foreach (var argument in arguments)
        start.ArgumentList.Add(argument);
    return start;
}

static async Task Stop(Process process)
{
    if (!process.HasExited)
        process.Kill(entireProcessTree: true);
    await process.WaitForExitAsync();
}

static string ScriptPath([System.Runtime.CompilerServices.CallerFilePath] string path = "") => path;
