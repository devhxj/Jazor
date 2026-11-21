#!/usr/bin/env dotnet run
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
        "Query.jazor", "Events.jazor", "Behavior.jazor", "Getters.jazor", "verify.mjs", "verify-query.mjs",
        "verify-events.mjs", "verify-behavior.mjs", "verify-getters.mjs", "Submit.jazor", "verify-submit.mjs" })
        File.Copy(Path.Combine(fixture, name), Path.Combine(project.FullName, name));

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
        .Replace("bool? booleanValue", "double? booleanValue", StringComparison.Ordinal), stop);
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
            .Replace("DateFns.Day.Value1", "DateFns.FirstWeekContainsDate.Value4", StringComparison.Ordinal)
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
    var invalid = await Build();
    string[] expected = [
        @"Getters\.jazor.*CS0200", @"Getters\.jazor.*CS0029", @"Getters\.jazor.*CS1503",
        @"Vee\.jazor.*CS1661", @"Vee\.jazor.*CS0029.*double.*bool",
        @"Vee\.jazor.*CS0029.*bool.*double", @"InvalidInitialValue\.jazor.*CS0029",
        @"App\.jazor.*CS1955", @"App\.jazor.*CS1503.*ECMAScript\.Date\[\]", @"App\.jazor.*CS1503.*bool",
        @"App\.jazor.*CS0266", @"App\.jazor.*CS0029.*ISOStringFormat", @"App\.jazor.*CS0029.*Locale",
        @"Query\.jazor.*CS0266", @"Query\.jazor.*CS1503", @"Query\.jazor.*CS0029.*ECMAScript\.Date",
        @"Query\.jazor.*CS0029.*bool.*double", @"Query\.jazor.*CS0029.*string.*double",
        @"Events\.jazor.*CS0117.*Click",
    ];
    if (invalid.ExitCode == 0 || expected.Any(pattern => !Regex.IsMatch(invalid.Output, pattern)) ||
        Regex.Matches(invalid.Output, @"Events\.jazor.*CS1503").Count < 2 ||
        Regex.Matches(invalid.Output, @"Behavior\.jazor.*CS1503").Count < 2)
        throw new InvalidOperationException($"Missing projection diagnostics.\n{invalid.Output}");
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
    using var server = Process.Start(dev) ?? throw new InvalidOperationException("Could not start Jolt.");
    try
    {
        while (await server.StandardOutput.ReadLineAsync(stop) is { } line)
        {
            Console.WriteLine(line);
            if (Uri.TryCreate(line, UriKind.Absolute, out var origin) && origin.Scheme == "http" && origin.IsLoopback)
            {
                var driver = Path.Combine(repo, "samples", "Jolt.Counter", "verify-browser.cs");
                using var check = Process.Start(StartInfo("dotnet", workspace, "run", "--file", driver,
                    "--", browser, origin.AbsoluteUri, "--monaco"))
                    ?? throw new InvalidOperationException("Could not start the Chromium check.");
                try
                {
                    await check.WaitForExitAsync(stop);
                    if (check.ExitCode != 0)
                        throw new InvalidOperationException($"Monaco browser check exited with code {check.ExitCode}.");
                }
                finally
                {
                    await Stop(check);
                }
                return;
            }
        }
        throw new InvalidOperationException("Jolt stopped before starting the Monaco development server.");
    }
    finally
    {
        await Stop(server);
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
