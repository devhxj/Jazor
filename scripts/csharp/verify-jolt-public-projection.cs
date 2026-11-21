#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.NodeHost/Jazor.NodeHost.csproj

using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Jazor.NodeHost;

if (args is not [var executable])
    throw new ArgumentException("Usage: verify-jolt-public-projection.cs <Jolt CLI apphost>");
var cli = Path.GetFullPath(executable);
var root = Environment.CurrentDirectory;
var fixture = Path.Combine(root, "scripts", "csharp", "fixtures", "jolt-npm-runtime");
var scratch = Directory.CreateDirectory(Path.Combine(root, ".tmp", $"public-projection-{Guid.NewGuid():N}"));
try
{
    foreach (var name in new[] { "package.json", "pnpm-lock.yaml" })
        File.Copy(Path.Combine(fixture, name), Path.Combine(scratch.FullName, name));
    var manifestPath = Path.Combine(scratch.FullName, "package.json");
    var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!;
    manifest["dependencies"]!["projection-fixture"] = "link:./fixture";
    var referenceManifest = JsonNode.Parse(File.ReadAllText(Path.Combine(fixture, "..", "jolt-binding-projection", "package.json")))!;
    manifest["dependencies"]!["vue-i18n"] = referenceManifest["dependencies"]!["vue-i18n"]!.DeepClone();
    File.WriteAllText(manifestPath, manifest.ToJsonString());
    var localPackage = scratch.CreateSubdirectory("fixture");
    File.WriteAllText(Path.Combine(localPackage.FullName, "package.json"),
        """{"name":"projection-fixture","version":"1.0.0","type":"module","exports":"./fixture.js"}""");
    File.WriteAllText(Path.Combine(localPackage.FullName, "fixture.js"), """
        export default function format(label, options) {
            if (!Object.hasOwn(options, 'nullable') || options.nullable !== null)
                throw new Error('Expected an explicit null option');
            return { label: label.toUpperCase(), nullable: 1 };
        }
        export function unprojectable() { return Symbol(); }
        let reads = 0;
        export function tryRead(present) { reads++; return present ? 0 : undefined; }
        export function readBox(present) {
            return { get value() { reads++; return present ? 0 : undefined; } };
        }
        export function takeReads() { const count = reads; reads = 0; return count; }
        export const absent = undefined;
        export const zero = 0;
        export function optionalArguments(count = 7, options = {}, label = "unset") {
            return [count, options === null ? "null" : "object", label, arguments.length].join("|");
        }
        """);
    File.WriteAllText(Path.Combine(localPackage.FullName, "fixture.d.ts"), """
        export default function format(label: string, options: { nullable?: number | null }): {
            label: string;
            nullable?: number | null;
            missing?: number;
        };
        export declare function unprojectable(): symbol;
        export declare function tryRead(present: boolean): number | undefined;
        export declare function readBox(present: boolean): { readonly value: number | undefined };
        export declare function takeReads(): number;
        export declare const absent: number | undefined;
        export declare const zero: number | undefined;
        export declare function optionalArguments(count?: number, options?: string | null, label?: string): string;
        """);
    var store = Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ?? Path.Combine(root, ".tmp", "nodeapi-store");
    await NodeToolchain.RestoreAsync(scratch.FullName, store, CancellationToken.None, Console.Out);
    var library = scratch.CreateSubdirectory("Library");
    var consumer = scratch.CreateSubdirectory("Consumer");
    var source = Path.Combine(library.FullName, "Dates.g.cs");
    string[] dateExports = ["add", "formatDistance", "formatDuration", "formatISO", "isWithinInterval", "intervalToDuration"];
    var dateArguments = new[] { "--module", "date-fns" }.Concat(dateExports.SelectMany(export => new[] { "--export", export })).ToArray();
    string[] localeArguments = ["--module", "date-fns/locale", "--export", "fr"];
    await GenerateAsync([.. dateArguments, .. localeArguments]);
    var first = await File.ReadAllTextAsync(source);
    if (first.Contains('\r'))
        throw new InvalidOperationException("Generated bindings must use LF line endings.");
    File.SetLastWriteTimeUtc(source, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    var timestamp = File.GetLastWriteTimeUtc(source);
    await GenerateAsync([.. localeArguments, "--module", "date-fns",
        .. dateExports.Reverse().SelectMany(export => new[] { "--export", export })]);
    if (await File.ReadAllTextAsync(source) != first || File.GetLastWriteTimeUtc(source) != timestamp)
        throw new InvalidOperationException("Reordered exports rewrote an identical public projection.");
    await RunAsync(cli, scratch.FullName, ["bindings", .. dateArguments, .. localeArguments,
        "--export", "noSuchExport", "--output", source], expectedError: "'date-fns/locale' has no export named 'noSuchExport'");
    await RunAsync(cli, scratch.FullName, ["bindings", .. dateArguments, "--module", "projection-fixture",
        "--export", "unprojectable", "--output", source],
        expectedError: "Unsupported npm value export 'unprojectable' from 'projection-fixture': type 'symbol'.");
    if (await File.ReadAllTextAsync(source) != first || File.GetLastWriteTimeUtc(source) != timestamp)
        throw new InvalidOperationException("A failed generation changed the existing public projection.");
    await RunAsync(cli, scratch.FullName, ["bindings", "--module", "projection-fixture", "--export", "default", "--export", "optionalArguments",
        "--export", "tryRead", "--export", "readBox", "--export", "takeReads", "--export", "absent", "--export", "zero",
        "--output", Path.Combine(library.FullName, "Fixture.g.cs")]);
    await RunAsync(cli, scratch.FullName, ["bindings", "--module", "@vueuse/core", "--export", "isClient",
        "--output", Path.Combine(library.FullName, "VueUse.g.cs")]);
    await RunAsync(cli, scratch.FullName, ["bindings", "--module", "@tanstack/vue-query", "--export", "useQuery", "--export", "VueQueryPlugin",
        "--output", Path.Combine(library.FullName, "Query.g.cs")]);
    await RunAsync(cli, scratch.FullName, ["bindings", "--module", "vue-i18n", "--export", "createI18n",
        "--output", Path.Combine(library.FullName, "Messages.g.cs")]);
    await RunAsync(cli, scratch.FullName, ["bindings", "--module", "es-toolkit/compat", "--export", "truncate",
        "--output", Path.Combine(library.FullName, "Text.g.cs")]);
    await RunAsync(cli, scratch.FullName, ["bindings", "--module", "es-toolkit/promise", "--export", "withTimeout",
        "--module", "es-toolkit/function", "--export", "retry",
        "--output", Path.Combine(library.FullName, "Async.g.cs")]);

    // Use the selected tool build's host contracts and source generator. The consumer
    // only references the projection through the compiled library's public metadata.
    File.WriteAllText(Path.Combine(scratch.FullName, "Directory.Build.props"), """
        <Project>
          <PropertyGroup>
            <TargetFramework>net11.0</TargetFramework>
            <LangVersion>preview</LangVersion>
            <Nullable>enable</Nullable>
          </PropertyGroup>
          <ItemGroup>
            <Reference Include="ECMAScript" HintPath="$(JoltToolDirectory)/ECMAScript.dll" />
            <Reference Include="ECMAScript.Contract" HintPath="$(JoltToolDirectory)/ECMAScript.Contract.dll" />
            <Reference Include="ECMAScript.Vue" HintPath="$(JoltToolDirectory)/ECMAScript.Vue.dll" />
          </ItemGroup>
        </Project>
        """);
    File.WriteAllText(Path.Combine(library.FullName, "Library.csproj"), """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup>
        </Project>
        """);
    File.WriteAllText(Path.Combine(consumer.FullName, "Consumer.csproj"), """
        <Project Sdk="Microsoft.NET.Sdk">
          <ItemGroup>
            <ProjectReference Include="../Library/Library.csproj" />
            <Analyzer Include="$(JoltToolDirectory)/Jazor.Compiler.dll;$(JoltToolDirectory)/Jazor.Common.dll;$(JoltToolDirectory)/ECMAScript.Contract.dll;$(JoltToolDirectory)/Acornima*.dll;$(JoltToolDirectory)/ICSharpCode.Decompiler.dll" />
          </ItemGroup>
        </Project>
        """);
    const string optionalResults = """
        public static bool OptionalResults()
        {
            System.Func<bool, double?> read = Fixture.TryRead;
            return Fixture.TryRead(false) == null && Fixture.TryRead(true) == 0 &&
                read(false) == null && read(true) == 0 && Fixture.Absent == null && Fixture.Zero == 0 &&
                Fixture.ReadBox(false).Value == null && Fixture.ReadBox(true).Value == 0 &&
                Fixture.TakeReads() == 6;
        }
        """;
    File.WriteAllText(Path.Combine(scratch.FullName, "Optional.jazor"),
        "@using Fixture = Npm.ProjectionFixture.Exports\n\n@code {\n" + optionalResults + "\n}\n");
    File.WriteAllText(Path.Combine(scratch.FullName, "Mode.jazor"), """
        @using Messages = Npm.VueI18n.Exports
        @code {
            public static string Locale()
            {
                var plugin = Messages.CreateI18nWithLegacyFalse(new() { Locale = "fr" });
                var locale = plugin.Global.Locale.Value;
                plugin.Dispose();
                return locale;
            }
        }
        """);
    await RunAsync(cli, scratch.FullName, ["build"]);
    var caller = Path.Combine(consumer.FullName, "Calendar.cs");
    const string calendar = """
        using Dates = Npm.DateFns.Exports;
        using Locales = Npm.DateFns.Locale.Exports;
        using Fixture = Npm.ProjectionFixture.Exports;
        using VueUse = Npm.Vueuse.Core.Exports;
        using Text = Npm.EsToolkit.Compat.Exports;
        using Timers = Npm.EsToolkit.Promise.Exports;
        using Functions = Npm.EsToolkit.Function.Exports;
        [ECMAScript.ECMAScriptModule("calendar.mjs")]
        public static class Calendar
        {
            public static string Label()
            {
                var duration = new Dates.Duration { Years = 1, Days = 2 };
                return Dates.FormatDuration(duration, new() { Delimiter = ", " });
            }

            public static string Upper()
            {
                var result = Fixture.Default(Label(), new() { Nullable = null });
                result.Nullable = null;
                return result.Missing == null && result.Nullable == null ? result.Label : "missing";
            }

            public static bool IsClient() => VueUse.IsClient;

            public static string OptionalArguments() =>
                Fixture.OptionalArguments() + ";" + Fixture.OptionalArguments(label: "named") + ";" +
                Fixture.OptionalArguments(count: 0, options: null, label: "explicit");

            // OPTIONAL_RESULTS

            public static string FrenchDistance() =>
                Dates.FormatDistance(new ECMAScript.Date(2026, 0, 17), new ECMAScript.Date(2026, 0, 15),
                    new() { Locale = Locales.Fr });

            public static string CalendarDate() =>
                Dates.FormatISO(new ECMAScript.Date(2026, 0, 15), new() {
                    Format = Dates.ISOStringFormat.Basic,
                    Representation = Dates.ISOStringRepresentation.Date,
                });

            public static string Preview() =>
                Text.Truncate("generated projection", new Text.TruncateOptions { Length = 12 });

            private sealed record Row(string Title);

            public static ECMAScript.IPromise<string> LoadTitle() =>
                Timers.WithTimeout(() => Functions.Retry(() =>
                    ECMAScript.Promise<Row>.Resolve(new Row("loaded")), 2), 1000).Then(row => row.Title);

            public static string BookingLength()
            {
                var start = new ECMAScript.Date(2026, 0, 15);
                var interval = new Dates.Interval { Start = start, End = new ECMAScript.Date(2026, 0, 17) };
                if (!Dates.IsWithinInterval(start, interval))
                    return "outside";
                return Dates.FormatDuration(Dates.IntervalToDuration(interval));
            }
        }
        """;
    File.WriteAllText(caller, calendar.Replace("// OPTIONAL_RESULTS", optionalResults));
    File.WriteAllText(Path.Combine(consumer.FullName, "MessagesProbe.cs"), """
        using Messages = Npm.VueI18n.Exports;

        [ECMAScript.ECMAScriptModule("messages-probe.mjs")]
        public static class MessagesProbe
        {
            public static string Composition()
            {
                var plugin = Messages.CreateI18nWithLegacyFalse(new() {
                    Locale = "fr",
                    Messages = new() { ["en"] = new() { ["greet"] = "Hello" } },
                });
                var global = plugin.Global;
                var before = global.Locale.Value;
                global.Locale.Value = "en";
                var result = before + "/" + global.T("greet");
                plugin.Dispose();
                return result;
            }

            public static string Legacy()
            {
                var plugin = Messages.CreateI18nWithLegacyTrue(new() {
                    Locale = "en",
                    Messages = new() { ["fr"] = new() { ["greet"] = "Salut" } },
                });
                var global = plugin.Global;
                var before = global.Locale;
                global.Locale = "fr";
                var result = before + "/" + global.T("greet");
                plugin.Dispose();
                return result;
            }
        }
        """);
    File.WriteAllText(Path.Combine(consumer.FullName, "QueryProbe.cs"), """
        using Query = Npm.Tanstack.VueQuery.Exports;
        using ECMAScript;
        using System;

        [ECMAScriptModule("query-probe.mjs")]
        public static class QueryProbe
        {
            private sealed record Payload(string Label);
            private sealed record Filter(double Page) : Vue.VueProps;

            public static void Install(Vue.VueApp app) => app.Use(Query.VueQueryPlugin);

            public static void Start(Action<string> receive)
            {
                var query = Query.UseQuery(new Query.UseQueryAsyncOptionsInput<Payload>
                {
                    QueryKey = ["public-projection", 0, false, new Filter(3)],
                    QueryFn = () => Promise<Payload>.Resolve(new Payload("query result")),
                });
                string? initial = query.Data?.Value?.Label;
                receive(initial ?? "pending");
                Vue.OnServerPrefetch(() => query.Refetch().Then(_ =>
                {
                    string? fetched = query.Data?.Value?.Label;
                    receive(fetched ?? "missing");
                }));
            }
        }
        """);
    await BuildAsync(library);
    await BuildAsync(consumer);
    var assembly = Assembly.Load(File.ReadAllBytes(Path.Combine(consumer.FullName, "bin", "Debug", "net11.0", "Consumer.dll")));
    var catalog = assembly.GetType("Jazor.Generated.ModuleCatalog", throwOnError: true)!;
    var modules = (IEnumerable)catalog.GetMethod("GetModules", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, null)!;
    foreach (var module in modules.Cast<object>())
    {
        var js = (string)module.GetType().GetProperty("Content")!.GetValue(module)!;
        var name = (string)module.GetType().GetProperty("RelativePath")!.GetValue(module)!;
        if (name == "query-probe.mjs" && !js.Contains("from \"@tanstack/vue-query\"", StringComparison.Ordinal))
            throw new InvalidOperationException("The generated query module is missing its Vue Query import.");
        if (name == "messages-probe.mjs" && new[] { "false", "true" }.Any(mode =>
            !Regex.IsMatch(js, $"""["']?\blegacy["']?\s*:\s*{mode}\b""")))
            throw new InvalidOperationException($"Expected both Vue I18n option literals from assembly metadata.\n{js}");
        File.WriteAllText(Path.Combine(scratch.FullName, name), js);
    }
    var verify = Path.Combine(scratch.FullName, "verify.mjs");
    File.WriteAllText(verify, """
        import assert from "node:assert/strict";
        import { createSSRApp, h } from "vue";
        import { renderToString } from "vue/server-renderer";
        import { BookingLength, CalendarDate, FrenchDistance, IsClient, Label, LoadTitle, OptionalArguments, OptionalResults, Preview, Upper } from "./calendar.mjs";
        import { Install, Start } from "./query-probe.mjs";
        import { Composition, Legacy } from "./messages-probe.mjs";
        import { OptionalResults as JoltOptionalResults } from "./.jolt/Optional.mjs";
        import { Locale } from "./.jolt/Mode.mjs";
        import { useQueryClient } from "@tanstack/vue-query";
        assert.equal(Label(), "1 year, 2 days");
        assert.equal(Upper(), "1 YEAR, 2 DAYS");
        assert.equal(IsClient(), false);
        assert.equal(BookingLength(), "2 days");
        assert.equal(FrenchDistance(), "2 jours");
        assert.equal(OptionalArguments(), "7|object|unset|0;7|object|named|3;0|null|explicit|3");
        assert.equal(OptionalResults(), true);
        assert.equal(JoltOptionalResults(), true);
        assert.equal(CalendarDate(), "20260115");
        assert.equal(Preview(), "generated...");
        assert.equal(Composition(), "fr/Hello");
        assert.equal(Legacy(), "en/Salut");
        assert.equal(Locale(), "fr");
        assert.equal(await LoadTitle(), "loaded");
        const observations = [];
        const app = createSSRApp({
            setup() {
                Start(value => observations.push(value));
                return () => h("p");
            },
        });
        app.config.errorHandler = error => { throw error; };
        Install(app);
        await renderToString(app);
        assert.deepEqual(observations, ["pending", "query result"]);
        const client = app.runWithContext(() => useQueryClient());
        assert.ok(client.getQueryData(["public-projection", 0, false, { Page: 3 }]));
        client.clear();
        """);
    await NodeModuleRunner.RunAsync(scratch.FullName, verify);

    File.WriteAllText(caller, calendar.Replace("Days = 2", """Days = "two" """));
    await BuildAsync(consumer, expectedError: "CS0029");
    File.WriteAllText(caller, calendar.Replace("Dates.IntervalToDuration(interval)", "Dates.IntervalToDuration(start)"));
    await BuildAsync(consumer, expectedError: "CS1503");
    File.WriteAllText(caller, calendar.Replace("Length = 12", """Length = "twelve" """));
    await BuildAsync(consumer, expectedError: "CS0029");
    File.WriteAllText(caller, calendar.Replace("row => row.Title", "row => row.Missing"));
    await BuildAsync(consumer, expectedError: "CS1061");
    File.WriteAllText(caller, calendar.Replace("Dates.ISOStringFormat.Basic", "Dates.ISOStringRepresentation.Date"));
    await BuildAsync(consumer, expectedError: "CS0266");
    File.WriteAllText(caller, calendar.Replace("Locale = Locales.Fr", "Locale = \"fr\""));
    await BuildAsync(consumer, expectedError: "CS0029");
    Console.WriteLine("The independent C# consumer ran both Vue I18n modes, Vue Query plugin registration, date-fns with a shared locale, and the inferred DTO through es-toolkit retry and withTimeout. Invalid inputs and a missing DTO member were rejected.");

    Task GenerateAsync(string[] arguments) => RunAsync(cli, scratch.FullName, ["bindings", .. arguments, "--output", source]);

    Task BuildAsync(DirectoryInfo project, string? expectedError = null) => RunAsync("dotnet", project.FullName,
        ["build", "-v:q", "-p:JoltToolDirectory=" + Path.GetDirectoryName(cli)], expectedError);
}
finally
{
    scratch.Delete(recursive: true);
}

static async Task RunAsync(string executable, string directory, string[] arguments, string? expectedError = null)
{
    var start = new ProcessStartInfo(executable)
    {
        WorkingDirectory = directory,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
    };
    foreach (var argument in arguments)
        start.ArgumentList.Add(argument);
    using var process = Process.Start(start)!;
    var output = Task.WhenAll(process.StandardOutput.ReadToEndAsync(), process.StandardError.ReadToEndAsync());
    await process.WaitForExitAsync();
    var log = string.Join(Environment.NewLine, await output);
    if (expectedError is null ? process.ExitCode != 0 :
        process.ExitCode == 0 || !log.Contains(expectedError, StringComparison.Ordinal))
        throw new InvalidOperationException($"{executable} {string.Join(' ', arguments)} exited {process.ExitCode}.{Environment.NewLine}{log}");
}
