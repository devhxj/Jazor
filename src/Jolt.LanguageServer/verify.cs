#!/usr/bin/env dotnet run
#:exclude **/*.cs
#:property PublishAot=false
using System.Diagnostics;
using System.Text;
using System.Text.Json;

var benchmarkRounds = args is ["--benchmark-mixed", var rounds, _] && int.TryParse(rounds, out var count) && count > 0
    ? count : 0;
using var timeout = new CancellationTokenSource(benchmarkRounds > 0 ? TimeSpan.FromMinutes(10) : TimeSpan.FromMinutes(2));
var token = timeout.Token;
var scratch = Directory.CreateDirectory(Path.GetFullPath(".tmp"));
var project = scratch.CreateSubdirectory("lsp-" + Guid.NewGuid().ToString("N")[..8]);
var path = Path.Combine(project.FullName, "Counter.jazor");
var canonicalUri = new Uri(path).AbsoluteUri;
var uri = ClientUri(path);
var semanticRefreshes = 0;
var (executable, arguments) = args switch
{
    ["--benchmark-mixed", _, var cli] when benchmarkRounds > 0 => (Path.GetFullPath(cli), new[] { "lsp" }),
    ["--router", var cli] => (Path.GetFullPath(cli), new[] { "lsp" }),
    ["--editor-only", var cli] => (Path.GetFullPath(cli), new[] { "lsp" }),
    ["--editor-only"] => ("dotnet", new[] { "run", "--project", "src/Jolt.LanguageServer", "--no-build" }),
    ["--npm-values", var cli] => (Path.GetFullPath(cli), new[] { "lsp" }),
    ["--npm-values"] => ("dotnet", new[] { "run", "--project", "src/Jolt.LanguageServer", "--no-build" }),
    ["--local-vue", var cli] => (Path.GetFullPath(cli), new[] { "lsp" }),
    ["--local-vue"] => ("dotnet", new[] { "run", "--project", "src/Jolt.LanguageServer", "--no-build" }),
    [var cli] => (Path.GetFullPath(cli), new[] { "lsp" }),
    [] => ("dotnet", new[] { "run", "--project", "src/Jolt.LanguageServer", "--no-build" }),
    _ => throw new ArgumentException("Usage: verify.cs [--editor-only | --npm-values | --local-vue] [path to Jolt CLI apphost] | verify.cs --router <path to Jolt CLI apphost> | verify.cs --benchmark-mixed <rounds> <path to Jolt CLI apphost>"),
};
var start = new ProcessStartInfo(executable) { RedirectStandardInput = true, RedirectStandardOutput = true };
start.Environment["JazorPnpmStoreDirectory"] = Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ??
    Path.Combine(scratch.FullName, benchmarkRounds > 0 ? "nodeapi-store" : "jolt-lsp-pnpm-store");
foreach (var arg in arguments)
    start.ArgumentList.Add(arg);
var startup = Stopwatch.StartNew();
using var process = Process.Start(start)!;
try
{
    await Send("initialize", new
    {
        rootUri = ClientUri(project.FullName),
        initializationOptions = new { visualStudioClassifications = true },
        capabilities = new { workspace = new
        {
            workspaceEdit = new { documentChanges = true, resourceOperations = new[] { "rename" }, changeAnnotationSupport = new { } },
            semanticTokens = new { refreshSupport = true },
        } },
    }, 1);
    var capabilities = (await Response(1)).GetProperty("capabilities");
    var initializeMs = startup.Elapsed.TotalMilliseconds;
    if (capabilities.GetProperty("positionEncoding").GetString() != "utf-16")
        throw new InvalidOperationException("Unexpected position encoding.");
    await Send("initialized", new { });
    if (benchmarkRounds > 0)
    {
        await BenchmarkMixedAsync(initializeMs);
        await Send("shutdown", new { }, 40);
        await Response(40);
        await Send("exit", new { });
        await process.WaitForExitAsync(token);
        return;
    }
    if (args.FirstOrDefault() == "--router")
    {
        await VerifyRouterAsync();
        await Send("shutdown", new { }, 40);
        await Response(40);
        await Send("exit", new { });
        await process.WaitForExitAsync(token);
        return;
    }
    if (args.FirstOrDefault() == "--local-vue")
    {
        await VerifyLocalVueAsync();
        await Send("shutdown", new { }, 40);
        await Response(40);
        await Send("exit", new { });
        await process.WaitForExitAsync(token);
        return;
    }
    if (args.FirstOrDefault() == "--npm-values")
    {
        await VerifyNpmValuesAsync();
        await VerifyDateFnsAsync();
        await VerifyI18nSpecializationAsync();
        await Send("shutdown", new { }, 40);
        await Response(40);
        await Send("exit", new { });
        await process.WaitForExitAsync(token);
        return;
    }
    await Send("textDocument/didOpen", new { textDocument = new { uri, version = 1, languageId = "jazor", text = "<p>@Count</p>\n@code { private int Count = 1; }" } });
    await Send("textDocument/definition", new { textDocument = new { uri }, position = new { line = 0, character = 6 } }, 2);
    var location = await Response(2);
    if (location.GetProperty("uri").GetString() != canonicalUri || location.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32() != 1)
        throw new InvalidOperationException("C# definition did not map back to the author source.");
    await Send("textDocument/completion", new { textDocument = new { uri }, position = new { line = 0, character = 8 } }, 3);
    var countItem = (await Response(3)).EnumerateArray().Single(item => item.GetProperty("label").GetString() == "Count");
    if (countItem.GetProperty("kind").GetInt32() != 5)
        throw new InvalidOperationException("Roslyn completion did not identify Count as a field.");
    await Send("completionItem/resolve", countItem, 20);
    if ((await Response(20)).GetProperty("documentation").GetString()?.Contains("int", StringComparison.Ordinal) != true)
        throw new InvalidOperationException("Roslyn completion did not return type information for Count.");
    var metadataDirectory = Path.Combine(project.FullName, ".jazor", $"vue-metadata-{Guid.NewGuid():N}");
    var metadataResources = new[] { "metadata.mjs", "vue-contract.mjs", "vue-source.mjs", "vue-template-text.mjs" };
    // Replay create/delete notifications for resources that may already be gone when the editor reports them.
    await Send("workspace/didChangeWatchedFiles", new
    {
        changes = metadataResources.SelectMany(file => new[] { 1, 3 }.Select(type =>
            new { uri = ClientUri(Path.Combine(metadataDirectory, file)), type })),
    });
    await Send("completionItem/resolve", countItem, 210);
    if (!(await Response(210)).TryGetProperty("documentation", out _))
        throw new InvalidOperationException("Internal Vue tooling events invalidated the current Jolt snapshot.");

    await Send("workspace/didChangeWatchedFiles", new
    {
        changes = new[] { new { uri = ClientUri(Path.Combine(project.FullName, "vue-source.mjs")), type = 2 } },
    });
    await Send("completionItem/resolve", countItem, 211);
    if ((await Response(211)).TryGetProperty("documentation", out _))
        throw new InvalidOperationException("An authored JavaScript module event failed to invalidate the Jolt snapshot.");
    await Send("textDocument/didChange", new
    {
        textDocument = new { uri, version = 2 },
        contentChanges = new[]
        {
            new { range = new { start = new { line = 0, character = 4 }, end = new { line = 0, character = 9 } }, text = "Missing" },
        },
    });
    while (true)
    {
        var message = await Read();
        if (!message.TryGetProperty("method", out var name) || name.GetString() != "textDocument/publishDiagnostics")
            continue;
        var publication = message.GetProperty("params");
        if (publication.GetProperty("version").GetInt32() != 2)
            continue;
        if (!publication.GetProperty("diagnostics").EnumerateArray().Any(item => item.GetProperty("code").GetString() == "CS0103"))
            throw new InvalidOperationException("Expected CS0103 after incrementally replacing Count.");
        break;
    }
    await VerifyEditorAsync(capabilities.GetProperty("semanticTokensProvider").GetProperty("legend").GetProperty("tokenTypes"));
    await VerifyAutoImportAsync();
    await VerifyRefactoringAsync();
    await VerifyEditingAsync();
    await VerifyLocalGenericAsync();
    await VerifySharedImportsAsync();
    await VerifyLocalModelAsync();
    await VerifyVueBuiltInsAsync();
    await VerifyDirectivesAsync(capabilities.GetProperty("semanticTokensProvider").GetProperty("legend").GetProperty("tokenTypes"));
    if (args.FirstOrDefault() == "--editor-only")
    {
        await Send("shutdown", new { }, 40);
        await Response(40);
        await Send("exit", new { });
        await process.WaitForExitAsync(token);
        return;
    }
    await VerifyNpmValuesAsync();
    await VerifyDateFnsAsync();
    var dependencyFixture = Path.GetFullPath("samples/JazorComponentProbe/Npm/current");
    foreach (var file in new[] { "package.json", "pnpm-lock.yaml" })
        File.Copy(Path.Combine(dependencyFixture, file), Path.Combine(project.FullName, file));
    await Send("workspace/didChangeWatchedFiles", new { changes = new[] { new { uri = ClientUri(Path.Combine(project.FullName, "package.json")), type = 1 } } });
    const string npmMarkup = "@using Npm.RekaUi\n<CollapsibleRoot Open=\"@(Count > 0)\" />\n@code { private int Count = 1; }";
    var npmTime = Stopwatch.StartNew();
    await Send("textDocument/didChange", new { textDocument = new { uri, version = 3 }, contentChanges = new[] { new { text = npmMarkup } } });
    await At("definition", 41, 1, 27);
    if ((await Response(41)).GetProperty("range").GetProperty("start").GetProperty("line").GetInt32() != 2)
        throw new InvalidOperationException("Local navigation must work during the first npm restore.");
    var definitionTime = npmTime.ElapsedMilliseconds;
    await At("completion", 42, 1, 28);
    if (!(await Response(42)).EnumerateArray().Any(item => item.GetProperty("label").GetString() == "Count"))
        throw new InvalidOperationException("Local completion must work during the first npm restore.");
    Console.WriteLine($"During npm restore, local navigation took {definitionTime} ms and local completion was available after {npmTime.ElapsedMilliseconds} ms.");
    await WaitForNpmAsync(3);
    Console.WriteLine($"Initial npm declarations were ready after {npmTime.ElapsedMilliseconds} ms.");
    await Send("textDocument/completion", new { textDocument = new { uri }, position = new { line = 1, character = 5 } }, 4);
    if (!(await Response(4)).EnumerateArray().Any(item => item.GetProperty("label").GetString() == "CollapsibleRoot"))
        throw new InvalidOperationException("Missing package export completion.");
    await Send("textDocument/hover", new { textDocument = new { uri }, position = new { line = 1, character = 18 } }, 5);
    if ((await Response(5)).GetProperty("contents").GetProperty("value").GetString() != "Open: boolean")
        throw new InvalidOperationException("Missing npm prop type.");
    await Send("textDocument/definition", new { textDocument = new { uri }, position = new { line = 1, character = 4 } }, 6);
    var packageDefinition = (await Response(6)).GetProperty("uri").GetString();
    if (packageDefinition is null || !packageDefinition.Contains("/reka-ui/", StringComparison.Ordinal) || !packageDefinition.EndsWith(".d.ts", StringComparison.Ordinal))
        throw new InvalidOperationException($"Unexpected npm component declaration: {packageDefinition}");
    const string signatureSource = "<p>@(Format(1))</p>\n@code { private string Format(int value) => value.ToString(); }";
    await Send("textDocument/didChange", new { textDocument = new { uri, version = 4 }, contentChanges = new[] { new { text = signatureSource } } });
    await Send("textDocument/didChange", new { textDocument = new { uri, version = 3 }, contentChanges = new[] { new { text = "<p>Stale</p>" } } });
    await Send("textDocument/signatureHelp", new { textDocument = new { uri }, position = new { line = 0, character = 12 } }, 7);
    var signature = (await Response(7)).GetProperty("signatures")[0].GetProperty("label").GetString();
    if (signature is null || !signature.Contains("Format(int value)", StringComparison.Ordinal))
        throw new InvalidOperationException($"Missing signature in latest buffer: {signature}");
    var forms = Directory.CreateDirectory(Path.Combine(project.FullName, "Forms")).FullName;
    var fieldPath = Path.Combine(forms, "Field.jazor");
    await File.WriteAllTextAsync(fieldPath, """
        <p>@Value</p>
        @code { [Parameter] public int Value { get; set; } }
        """, token);
    await Send("textDocument/didChange", new
    {
        textDocument = new { uri, version = 5 },
        contentChanges = new[] { new { text = "@using App.Forms\n<Field Value=\"@(1)\" />" } },
    });
    await At("completion", 8, 1, 9);
    var prop = (await Response(8)).EnumerateArray().Single(item => item.GetProperty("label").GetString() == "Value");
    CheckEdit(prop, "Value", 7, 12);
    await At("definition", 9, 1, 9);
    var field = await Response(9);
    if (field.GetProperty("uri").GetString() != new Uri(fieldPath).AbsoluteUri ||
        field.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32() != 1)
        throw new InvalidOperationException($"Incorrect local prop declaration: {field}");
    const string slot = "@using Npm.RekaUi\n<CollapsibleRoot><template #default context=\"scope\"><p>@scope.open</p></template></CollapsibleRoot>";
    await Send("textDocument/didChange", new { textDocument = new { uri, version = 6 }, contentChanges = new[] { new { text = slot } } });
    await WaitForNpmAsync(6);
    await At("completion", 10, 1, slot.Split('\n')[1].IndexOf("#default", StringComparison.Ordinal) + 2);
    var scoped = (await Response(10)).EnumerateArray().Single(item => item.GetProperty("label").GetString() == "default");
    if (!scoped.GetProperty("detail").GetString()!.Contains("open: boolean", StringComparison.Ordinal))
        throw new InvalidOperationException("Scoped slot completion lost the prop type.");
    const string literals = "@using Npm.RekaUi\n<Separator Orientation=\"hor\" />";
    await Send("textDocument/didChange", new { textDocument = new { uri, version = 7 }, contentChanges = new[] { new { text = literals } } });
    await WaitForNpmAsync(7);
    await At("completion", 11, 1, 26);
    var literal = (await Response(11)).EnumerateArray().Single(item => item.GetProperty("label").GetString() == "horizontal");
    CheckEdit(literal, "horizontal", 24, 27);
    await MeasureWarmAsync();
    if (semanticRefreshes == 0)
        throw new InvalidOperationException("The server must refresh semantic tokens when the npm declarations are ready.");
    await VerifyNpmFailureAsync();
    await CheckTrigger("<", 8, 1, 13, "<", "div");
    await CheckTrigger("<p>@Text.</p>", 9, 9, 14, ".", "Length");
    const string generic = """
        @using Npm.RekaUi
        <ListboxVirtualizer Options="@Names">
            <template #default context="row"><span>@row.option.Len</span></template>
        </ListboxVirtualizer>
        @code { private string[] Names = ["One"]; }
        """;
    await Send("textDocument/didChange", new { textDocument = new { uri, version = 10 }, contentChanges = new[] { new { text = generic } } });
    await WaitForNpmAsync(10);
    await At("completion", 152, 2, generic.Split('\n')[2].IndexOf(".Len", StringComparison.Ordinal) + 4);
    if (!(await Response(152)).EnumerateArray().Any(item => item.GetProperty("label").GetString() == "Length"))
        throw new InvalidOperationException("The published generic slot should expose the inferred string members.");
    const string modelMarkup = "@using Npm.RekaUi\n<CollapsibleRoot @bind-Open=\"open\" />\n@code { private bool open; }";
    await Send("textDocument/didChange", new { textDocument = new { uri, version = 11 }, contentChanges = new[] { new { text = modelMarkup } } });
    await WaitForNpmAsync(11);
    await At("completion", 153, 1, 18);
    var modelItems = (await Response(153)).EnumerateArray().ToArray();
    var model = modelItems.Single(item => item.GetProperty("label").GetString() == "@bind-Open");
    CheckEdit(model, "@bind-Open", 17, 27);
    if (modelItems.Any(item => item.GetProperty("label").GetString() == "@bind-Disabled"))
        throw new InvalidOperationException("A prop without an update event must not be offered as a model.");
    await Send("shutdown", new { }, 15);
    await Response(15);
    await Send("exit", new { });
    await process.WaitForExitAsync(token);
    Console.WriteLine("Verified Jolt stdio, Roslyn completion, definition and signature help, versioned diagnostics, npm tag completion, prop hover and declaration.");
}
finally
{
    if (!process.HasExited)
    {
        process.Kill(entireProcessTree: true);
        await process.WaitForExitAsync();
    }
    project.Delete(recursive: true);
}

async Task VerifyRouterAsync()
{
    var package = Path.Combine(project.FullName, "package.json");
    const string page = """
        @page "/drafts/{id:int}"
        @layout DraftLayout
        <p>@Id</p>
        @code { [Parameter] public int Id { get; set; } }
        """;
    await File.WriteAllTextAsync(package, "{}", token);
    await File.WriteAllTextAsync(path, page, token);
    await File.WriteAllTextAsync(Path.Combine(project.FullName, "DraftLayout.jazor"), "<main><slot /></main>", token);
    await Send("textDocument/didOpen", new { textDocument = new { uri, languageId = "jazor", version = 1, text = page } });
    await ExpectDiagnostics(1, ["JOLT1022", "JOLT1022"]);

    await File.WriteAllTextAsync(package, """{"jolt":{"router":true}}""", token);
    await NotifyManifest();
    await ExpectDiagnostics(1, []);
    await Build(expectedSuccess: true);
    var generated = Path.Combine(project.FullName, ".jolt", "_routes.mjs");
    var first = await File.ReadAllTextAsync(generated, token);

    var renamed = page.Replace("/drafts/", "/notes/", StringComparison.Ordinal);
    await File.WriteAllTextAsync(path, renamed, token);
    await Send("textDocument/didChange", new { textDocument = new { uri, version = 2 }, contentChanges = new[] { new { text = renamed } } });
    await ExpectDiagnostics(2, []);
    await Build(expectedSuccess: true);
    var second = await File.ReadAllTextAsync(generated, token);
    if (!first.Contains("/drafts/:id(", StringComparison.Ordinal) ||
        second != first.Replace("/drafts/:id(", "/notes/:id(", StringComparison.Ordinal) ||
        !File.Exists(Path.ChangeExtension(generated, ".d.mts")))
        throw new InvalidOperationException("The generated routes should follow the saved page and include a TypeScript declaration.");

    await File.WriteAllTextAsync(package, "{}", token);
    await NotifyManifest();
    await ExpectDiagnostics(2, ["JOLT1022", "JOLT1022"]);
    await Build(expectedSuccess: false);
    await File.WriteAllTextAsync(path, "<p />", token);
    await Build(expectedSuccess: true);
    if (File.Exists(generated) || File.Exists(Path.ChangeExtension(generated, ".d.mts")))
        throw new InvalidOperationException("A successful build without the adapter should remove the previous generated router files.");
    Console.WriteLine("Verified LSP configuration changes, route diagnostics, deterministic CLI rebuilds, and generated route cleanup.");

    Task NotifyManifest() => Send("workspace/didChangeWatchedFiles",
        new { changes = new[] { new { uri = ClientUri(package), type = 2 } } });

    async Task ExpectDiagnostics(int version, string[] expected)
    {
        while (true)
        {
            var message = await Read();
            if (!message.TryGetProperty("method", out var method) || method.GetString() != "textDocument/publishDiagnostics")
                continue;
            var publication = message.GetProperty("params");
            if (publication.GetProperty("uri").GetString() != canonicalUri || publication.GetProperty("version").GetInt32() != version)
                continue;
            var codes = publication.GetProperty("diagnostics").EnumerateArray()
                .Where(item => item.GetProperty("severity").GetInt32() == 1)
                .Select(item => item.GetProperty("code").GetString()).Order().ToArray();
            if (!codes.SequenceEqual(expected))
                throw new InvalidOperationException($"Expected diagnostics [{string.Join(", ", expected)}], got [{string.Join(", ", codes)}].");
            return;
        }
    }

    async Task Build(bool expectedSuccess)
    {
        var info = new ProcessStartInfo(executable) { RedirectStandardOutput = true, RedirectStandardError = true };
        info.ArgumentList.Add("build");
        info.ArgumentList.Add(project.FullName);
        using var build = Process.Start(info)!;
        var stdout = build.StandardOutput.ReadToEndAsync(token);
        var stderr = build.StandardError.ReadToEndAsync(token);
        await build.WaitForExitAsync(token);
        if ((build.ExitCode == 0) != expectedSuccess)
            throw new InvalidOperationException($"Unexpected CLI build result {build.ExitCode}\n{await stdout}\n{await stderr}");
    }
}

Task At(string request, int id, int line, int character) =>
    Send($"textDocument/{request}", new { textDocument = new { uri }, position = new { line, character } }, id);

async Task BenchmarkMixedAsync(double initializeMs)
{
    var fixture = Path.GetFullPath("samples/Jolt.Mixed");
    const string authorFile = "editors/MixedEditor.jazor";
    const string vueFile = "editors/NameInput.vue";
    foreach (var name in new[] { "package.json", "pnpm-lock.yaml", "tsconfig.json", "index.html", "main.ts",
        "probe.tsx", "App.vue", authorFile, vueFile, "editors/draft.svg" })
    {
        var destination = Path.Combine(project.FullName, name);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(Path.Combine(fixture, name), destination);
    }
    var authorPath = Path.Combine(project.FullName, authorFile);
    var vuePath = Path.Combine(project.FullName, vueFile);
    var authorUri = ClientUri(authorPath);
    var publishedUri = new Uri(authorPath).AbsoluteUri;
    var vueUri = ClientUri(vuePath);
    var author = await File.ReadAllTextAsync(authorPath, token);
    var vue = await File.ReadAllTextAsync(vuePath, token);
    var (authorLine, limitStart) = Locate(author, "Limit=\"@Limit\"", "Limit=\"@".Length);
    var (vueLine, numberStart) = Locate(vue, "limit: number", "limit: ".Length);
    var authorVersion = 1;
    var vueVersion = 1;
    var firstCheck = Stopwatch.StartNew();
    // Open Vue first so the initial author publication uses the same overlays as the edit rounds.
    await Send("textDocument/didOpen", new { textDocument = new { uri = vueUri, version = vueVersion, languageId = "vue", text = vue } });
    await Send("textDocument/didOpen", new { textDocument = new { uri = authorUri, version = authorVersion, languageId = "jazor", text = author } });
    await WaitForDiagnosticsAsync(null);
    var firstDiagnosticsMs = firstCheck.Elapsed.TotalMilliseconds;

    var typo = new List<double>(benchmarkRounds);
    var typoRepair = new List<double>(benchmarkRounds);
    var propChange = new List<double>(benchmarkRounds);
    var propRepair = new List<double>(benchmarkRounds);
    for (var round = 0; round <= benchmarkRounds; round++)
    {
        var typoMs = await MeasureAsync(authorUri, ++authorVersion, authorLine, limitStart, "Limit", "Limti", "CS0103");
        var typoRepairMs = await MeasureAsync(authorUri, ++authorVersion, authorLine, limitStart, "Limti", "Limit", null);
        var propChangeMs = await MeasureAsync(vueUri, ++vueVersion, vueLine, numberStart, "number", "string", "CS1503");
        var propRepairMs = await MeasureAsync(vueUri, ++vueVersion, vueLine, numberStart, "string", "number", null);
        if (round == 0)
            continue;
        typo.Add(typoMs);
        typoRepair.Add(typoRepairMs);
        propChange.Add(propChangeMs);
        propRepair.Add(propRepairMs);
    }
    Console.WriteLine(JsonSerializer.Serialize(new
    {
        benchmark = "jolt-mixed-lsp",
        rounds = benchmarkRounds,
        warmupRounds = 1,
        initializeMs,
        firstDiagnosticsMs,
        typo = Summary(typo),
        typoRepair = Summary(typoRepair),
        vuePropChange = Summary(propChange),
        vuePropRepair = Summary(propRepair),
    }));

    async Task<double> MeasureAsync(string documentUri, int version, int line, int start, string previous, string text, string? error)
    {
        var clock = Stopwatch.StartNew();
        await Send("textDocument/didChange", new
        {
            textDocument = new { uri = documentUri, version },
            contentChanges = new[]
            {
                new { range = new { start = new { line, character = start }, end = new { line, character = start + previous.Length } }, text },
            },
        });
        await WaitForDiagnosticsAsync(error);
        return clock.Elapsed.TotalMilliseconds;
    }

    async Task WaitForDiagnosticsAsync(string? error)
    {
        while (true)
        {
            var message = await Read();
            if (!message.TryGetProperty("method", out var method) || method.GetString() != "textDocument/publishDiagnostics")
                continue;
            var publication = message.GetProperty("params");
            if (publication.GetProperty("uri").GetString() != publishedUri ||
                !publication.TryGetProperty("version", out var version) ||
                version.ValueKind != JsonValueKind.Number || version.GetInt32() != authorVersion)
                continue;
            // Vue-only changes are republished at the unchanged Jolt document version.
            var errors = publication.GetProperty("diagnostics").EnumerateArray()
                .Where(diagnostic => diagnostic.GetProperty("severity").GetInt32() == 1).ToArray();
            if (error is null ? errors.Length == 0 : errors is [var diagnostic] && Matches(diagnostic, error))
                return;
            throw new InvalidOperationException($"Expected {error ?? "no error"} in {authorFile} v{authorVersion}: {publication}");
        }
    }

    bool Matches(JsonElement diagnostic, string error)
    {
        var range = diagnostic.GetProperty("range");
        var start = range.GetProperty("start");
        var end = range.GetProperty("end");
        return diagnostic.GetProperty("code").GetString() == error &&
            start.GetProperty("line").GetInt32() == authorLine && start.GetProperty("character").GetInt32() == limitStart &&
            end.GetProperty("line").GetInt32() == authorLine && end.GetProperty("character").GetInt32() == limitStart + "Limit".Length;
    }

    static (int Line, int Character) Locate(string source, string fragment, int offset)
    {
        var lines = source.Split('\n');
        var line = Array.FindIndex(lines, line => line.Contains(fragment, StringComparison.Ordinal));
        return line < 0 ? throw new InvalidOperationException($"Missing sample fragment: {fragment}") :
            (line, lines[line].IndexOf(fragment, StringComparison.Ordinal) + offset);
    }

    static object Summary(List<double> samples)
    {
        var ordered = samples.Order().ToArray();
        return new { samplesMs = samples, p50Ms = Percentile(0.50), p95Ms = Percentile(0.95) };

        double Percentile(double percentile) => ordered[(int)Math.Ceiling(percentile * ordered.Length) - 1];
    }
}

async Task VerifyLocalVueAsync()
{
    var fixture = Path.GetFullPath("samples/Jolt.Mixed");
    foreach (var name in new[] { "package.json", "pnpm-lock.yaml" })
        File.Copy(Path.Combine(fixture, name), Path.Combine(project.FullName, name));
    var vuePath = Path.Combine(project.FullName, "LocalCard.vue");
    var vueUri = ClientUri(vuePath);
    var contractPath = Path.Combine(project.FullName, "Contract.jazor");
    var contractUri = ClientUri(contractPath);
    var declarationPath = Path.Combine(project.FullName, "title-contract.d.mts");
    const string contract = """
        <div />
        @code { [Parameter] public string Title { get; set; } = ""; }
        """;
    const string vue = """
        <script setup lang="ts">
        import Contract from './Contract.jazor'
        import type { Title } from './title-contract.mjs'
        defineProps<{ title: Title & NonNullable<InstanceType<typeof Contract>['$props']['Title']> }>()
        defineSlots<{ header: (scope: { count: number }) => unknown }>()
        </script>
        <template><slot name="header" :count="1" /></template>
        """;
    const string jolt = """<LocalCard Title="Hello"><template #header context="scope"><p>@scope.count</p></template></LocalCard>""";
    await File.WriteAllTextAsync(vuePath, vue, token);
    await File.WriteAllTextAsync(contractPath, contract, token);
    await File.WriteAllTextAsync(declarationPath, "export type Title = string | number;", token);
    await WaitForAuthorDiagnostics(initial: true, expectErrors: false);
    await UpdateDeclarationAsync("export type Title = number;", expectErrors: true);
    await UpdateDeclarationAsync("export type Title = string | number;", expectErrors: false);
    await Send("textDocument/didOpen", new
    {
        textDocument = new { uri = contractUri, version = 1, languageId = "jazor", text = contract },
    });
    await WaitForAuthorDiagnostics(initial: false, expectErrors: false);
    await Send("textDocument/didChange", new
    {
        textDocument = new { uri = contractUri, version = 2 },
        contentChanges = new[] { new { text = contract.Replace("string Title { get; set; } = \"\";", "double Title { get; set; }", StringComparison.Ordinal) } },
    });
    await WaitForAuthorDiagnostics(initial: false, expectErrors: true);
    await Send("textDocument/didClose", new { textDocument = new { uri = contractUri } });
    await WaitForAuthorDiagnostics(initial: false, expectErrors: false);
    await Definition(50, "LocalCard", expectedLine: null);
    await Definition(51, "Title", expectedLine: 3);
    await Definition(52, "header", expectedLine: 4);
    await Definition(54, "scope.count", expectedLine: 4, insideWord: "scope.".Length + 1);
    await Send("textDocument/didOpen", new
    {
        textDocument = new { uri = vueUri, version = 1, languageId = "vue", text = "\n\n" + vue },
    });
    await WaitForAuthorDiagnostics(initial: false, expectErrors: false);
    await Definition(53, "Title", expectedLine: 5);
    await Send("textDocument/didChange", new
    {
        textDocument = new { uri = vueUri, version = 2 },
        contentChanges = new[] { new { text = InvalidVue() } },
    });
    await WaitForAuthorDiagnostics(initial: false, expectErrors: true);
    await Send("textDocument/didClose", new { textDocument = new { uri = vueUri } });
    await WaitForAuthorDiagnostics(initial: false, expectErrors: false);
    await File.WriteAllTextAsync(vuePath, InvalidVue(), token);
    await Send("workspace/didChangeWatchedFiles", new { changes = new[] { new { uri = vueUri, type = 2 } } });
    await WaitForAuthorDiagnostics(initial: false, expectErrors: true);
    Console.WriteLine("Verified Vue source definition ranges and dependent diagnostics on Jolt and Vue buffer changes and Vue and TypeScript declaration disk changes.");

    string InvalidVue() => vue.Replace("Title & NonNullable<InstanceType<typeof Contract>['$props']['Title']>", "number", StringComparison.Ordinal);

    async Task UpdateDeclarationAsync(string source, bool expectErrors)
    {
        await File.WriteAllTextAsync(declarationPath, source, token);
        await Send("workspace/didChangeWatchedFiles", new
        {
            changes = new[] { new { uri = ClientUri(declarationPath), type = 2 } },
        });
        await WaitForAuthorDiagnostics(initial: false, expectErrors);
    }

    async Task Definition(int id, string word, int? expectedLine, int insideWord = 1)
    {
        await Send("textDocument/definition", new
        {
            textDocument = new { uri },
            position = new { line = 0, character = jolt.IndexOf(word, StringComparison.Ordinal) + insideWord },
        }, id);
        var location = await Response(id);
        var line = location.ValueKind == JsonValueKind.Object
            ? location.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32() : -1;
        if (line < 0 || location.GetProperty("uri").GetString() != new Uri(vuePath).AbsoluteUri ||
            expectedLine is int expected && line != expected)
            throw new InvalidOperationException($"Expected definition of {word} in {vuePath} at line {expectedLine}: {location}");
    }

    async Task WaitForAuthorDiagnostics(bool initial, bool expectErrors)
    {
        if (initial)
            await Send("textDocument/didOpen", new { textDocument = new { uri, version = 1, languageId = "jazor", text = jolt } });
        while (true)
        {
            var notification = await Read();
            if (!notification.TryGetProperty("method", out var method) || method.GetString() != "textDocument/publishDiagnostics")
                continue;
            var publication = notification.GetProperty("params");
            if (publication.GetProperty("uri").GetString() != canonicalUri)
                continue;
            var diagnostics = publication.GetProperty("diagnostics");
            var hasErrors = diagnostics.EnumerateArray().Any(diagnostic => diagnostic.GetProperty("severity").GetInt32() == 1);
            if (hasErrors != expectErrors)
                throw new InvalidOperationException($"Expected errors: {expectErrors}. Received {diagnostics}");
            break;
        }
    }
}

async Task VerifyDateFnsAsync()
{
    var directory = project.CreateSubdirectory("DateFns").FullName;
    var fixture = Path.GetFullPath("scripts/csharp/fixtures/jolt-npm-runtime");
    foreach (var file in new[] { "package.json", "pnpm-lock.yaml" })
        File.Copy(Path.Combine(fixture, file), Path.Combine(directory, file));
    var sourcePath = Path.Combine(directory, "Dates.jazor");
    var document = new { uri = ClientUri(sourcePath) };
    var sourceUri = new Uri(sourcePath).AbsoluteUri;
    const string import = "@using Dates = Npm.DateFns.Exports\n";
    const string body = "<p>@(Dates.AddDays(new ECMAScript.Date(2026, 0, 15), 2))</p>";
    await Send("textDocument/didOpen", new { textDocument = new { document.uri, version = 1, languageId = "jazor", text = import + body } });
    var diagnostics = await WaitForNpmAsync(1, sourceUri);
    if (diagnostics.Any(item => item.GetProperty("severity").GetInt32() == 1))
        throw new InvalidOperationException($"Expected the Date overload to compile: {string.Join(", ", diagnostics.AsEnumerable())}");

    var methodStart = body.IndexOf("AddDays", StringComparison.Ordinal);
    await Request("completion", 220, methodStart + 2);
    var completions = (await Response(220)).EnumerateArray();
    if (!completions.Any(item => item.GetProperty("label").GetString() == "AddDays" && item.GetProperty("kind").GetInt32() == 2))
        throw new InvalidOperationException("Expected date-fns AddDays method completion.");
    await Request("hover", 221, methodStart + 2);
    var hover = (await Response(221)).GetProperty("contents").GetProperty("value").GetString()!;
    if (!hover.Contains("AddDays", StringComparison.Ordinal) || !hover.Contains("Date", StringComparison.Ordinal) ||
        !hover.Contains("double amount", StringComparison.Ordinal))
        throw new InvalidOperationException($"Expected the selected Date overload in hover: {hover}");
    await Request("definition", 222, methodStart + 2);
    var definition = await Response(222);
    var declarationPath = new Uri(definition.GetProperty("uri").GetString()!).LocalPath;
    var declarationLine = definition.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32();
    if (!declarationPath.Replace('\\', '/').Contains("/date-fns/", StringComparison.Ordinal) ||
        !File.ReadLines(declarationPath).ElementAt(declarationLine).Contains("addDays", StringComparison.Ordinal))
        throw new InvalidOperationException($"Expected navigation to the locked date-fns addDays declaration: {definition}");

    const string invalid = "\"two\"";
    var badBody = body.Replace("2))", invalid + "))", StringComparison.Ordinal);
    await Send("textDocument/didChange", new { textDocument = new { document.uri, version = 2 }, contentChanges = new[] { new { text = import + badBody } } });
    var error = (await WaitForNpmAsync(2, sourceUri)).Single(item => item.GetProperty("code").GetString() == "CS1503");
    var range = error.GetProperty("range");
    var start = range.GetProperty("start");
    var end = range.GetProperty("end");
    var argumentStart = badBody.IndexOf(invalid, StringComparison.Ordinal);
    if (start.GetProperty("line").GetInt32() != 1 || end.GetProperty("line").GetInt32() != 1 ||
        start.GetProperty("character").GetInt32() != argumentStart || end.GetProperty("character").GetInt32() != argumentStart + invalid.Length)
        throw new InvalidOperationException($"Expected the Date overload diagnostic over the authored argument: {error}");
    await Send("textDocument/didClose", new { textDocument = document });
    Console.WriteLine("Verified date-fns Date overload completion, selected-signature hover, locked source definition, and the invalid argument's authored range.");

    Task Request(string method, int id, int character) =>
        Send("textDocument/" + method, new { textDocument = document, position = new { line = 1, character } }, id);
}

async Task VerifyI18nSpecializationAsync()
{
    var directory = project.CreateSubdirectory("I18nSpecialization").FullName;
    var fixture = Path.GetFullPath("scripts/csharp/fixtures/jolt-binding-projection");
    foreach (var file in new[] { "package.json", "pnpm-lock.yaml" })
        File.Copy(Path.Combine(fixture, file), Path.Combine(directory, file));

    var path = Path.Combine(directory, "Plugin.jazor");
    var textDocument = new { uri = ClientUri(path) };
    var sourceUri = new Uri(path).AbsoluteUri;
    const string method = "CreateI18nWithLegacyFalse";
    const string call = "<p>@(Messages." + method + "(new() { Locale = \"fr\" }).Global.Locale.Value)</p>";
    const string source = "@using Messages = Npm.VueI18n.Exports\n" + call;
    await Send("textDocument/didOpen", new { textDocument = new { textDocument.uri, version = 1, languageId = "jazor", text = source } });
    var diagnostics = await WaitForNpmAsync(1, sourceUri);
    if (diagnostics.Any(item => item.GetProperty("severity").GetInt32() == 1))
        throw new InvalidOperationException($"Expected the standalone I18n specialization to compile. {string.Join(", ", diagnostics.AsEnumerable())}");

    var character = call.IndexOf(method, StringComparison.Ordinal) + "CreateI18nWith".Length;
    var position = new { line = 1, character };
    await Send("textDocument/completion", new { textDocument, position }, 240);
    if (!(await Response(240)).EnumerateArray().Any(item => item.GetProperty("label").GetString() == method))
        throw new InvalidOperationException("Expected completion for the standalone I18n specialization.");

    await Send("textDocument/definition", new { textDocument, position }, 241);
    var definition = await Response(241);
    var target = new Uri(definition.GetProperty("uri").GetString()!).LocalPath;
    var declarationLine = definition.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32();
    var installedPackage = Path.Combine(directory, "node_modules", "vue-i18n");
    using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(installedPackage, "package.json")));
    if (manifest.RootElement.GetProperty("version").GetString() != "11.4.12" ||
        !target.Replace('\\', '/').Contains("/vue-i18n/", StringComparison.Ordinal) ||
        !File.ReadLines(target).ElementAt(declarationLine).Contains("createI18n", StringComparison.Ordinal))
        throw new InvalidOperationException($"Expected the locked Vue I18n createI18n declaration. {definition}");

    await Send("textDocument/didClose", new { textDocument });
    Console.WriteLine("Verified standalone Vue I18n specialization completion and locked declaration navigation.");
}

async Task VerifyNpmValuesAsync()
{
    var directory = project.CreateSubdirectory("Values").FullName;
    var manifest = Path.Combine(directory, "package.json");
    var lockfile = Path.Combine(directory, "pnpm-lock.yaml");
    var imports = new { uri = ClientUri(Path.Combine(directory, "_Imports.jazor")) };
    var sourcePath = Path.Combine(directory, "Value.jazor");
    var document = new { uri = ClientUri(sourcePath) };
    var sourceUri = new Uri(sourcePath).AbsoluteUri;
    const string importText = "@using Text = Npm.EsToolkit.String.Exports";
    const string call = "<p>@(Text.CamelCase(\"Order Total\"))</p>";
    const string first = "1.53.1";
    const string second = "1.53.2";
    await WriteDependency(first, "sha512-VhMc6IXmexu8iYFfL2aaAww6IvJTdHlqBLruLoEUs33YD4IcP5DdZcLOybMsMmmLsiy1c8PsiLsrM0DdIuuRUA==");
    await Send("textDocument/didOpen", new { textDocument = new { imports.uri, version = 1, languageId = "jazor", text = importText } });
    await Send("textDocument/didOpen", new { textDocument = new { document.uri, version = 1, languageId = "jazor", text = "<p>@Text.</p>" } });
    await Ready(1);
    await Request("completion", 200, 9);
    var completion = (await Response(200)).EnumerateArray().Single(item => item.GetProperty("label").GetString() == "CamelCase");
    if (completion.GetProperty("kind").GetInt32() != 2)
        throw new InvalidOperationException("Expected a method completion for the projected npm function.");
    CheckEdit(completion, "CamelCase", 9, 9);
    await Send("completionItem/resolve", completion, 201);
    CheckSignature((await Response(201)).GetProperty("documentation").GetString());

    await Change(document.uri, 2, call);
    await Ready(2, expectClean: true);
    await Request("hover", 202, 11);
    CheckSignature((await Response(202)).GetProperty("contents").GetProperty("value").GetString());
    await Request("signatureHelp", 203, 20);
    CheckSignature((await Response(203)).GetProperty("signatures")[0].GetProperty("label").GetString());
    await CheckDefinition(204, first);

    await Change(document.uri, 3, "<p>@(Text.CamelCase(42))</p>");
    var invalid = (await Ready(3)).Single(item => item.GetProperty("code").GetString() == "CS1503");
    var range = invalid.GetProperty("range");
    if (range.GetProperty("start").GetProperty("character").GetInt32() != 20 || range.GetProperty("end").GetProperty("character").GetInt32() != 22)
        throw new InvalidOperationException($"Expected the npm argument type error over the authored integer: {invalid}");

    await Change(imports.uri, 2, importText.Replace("Text =", "Other =", StringComparison.Ordinal));
    await Change(document.uri, 4, call);
    if (!(await Ready(4)).Any(item => item.GetProperty("code").GetString() == "CS0103"))
        throw new InvalidOperationException("The previous npm alias must disappear after changing _Imports.jazor in memory.");
    await Request("definition", 205, 11);
    if ((await Response(205)).ValueKind != JsonValueKind.Null)
        throw new InvalidOperationException("The removed npm alias still resolves to its former declaration.");

    await Change(imports.uri, 3, importText);
    await Change(document.uri, 5, call);
    await Ready(5, expectClean: true);
    await CheckDefinition(206, first);
    await Request("completion", 207, 13);
    var previous = (await Response(207)).EnumerateArray().Single(item => item.GetProperty("label").GetString() == "CamelCase");

    await WriteDependency(second, "sha512-iUqZ+NlwH4jUgRmXZUxFBsV/9762TQEa1O4IXLFBmDEjVHScBytKuseX23I0+v9ShZ5MYxEx6WKNzm6IPnex8g==");
    await Send("workspace/didChangeWatchedFiles", new { changes = new[] { new { uri = ClientUri(lockfile), type = 2 } } });
    await Ready(5, expectClean: true);
    await CheckDefinition(208, second);
    await Send("completionItem/resolve", previous, 209);
    if ((await Response(209)).TryGetProperty("documentation", out _))
        throw new InvalidOperationException("A completion item from the previous npm snapshot was resolved against the new dependency.");

    await Send("textDocument/didClose", new { textDocument = document });
    await Send("textDocument/didClose", new { textDocument = imports });
    Console.WriteLine("Verified npm value completion, hover, signature help, source diagnostics, and locked declarations across unsaved import edits and dependency updates.");

    async Task WriteDependency(string version, string integrity)
    {
        var package = new { name = "jolt-lsp-values", @private = true, dependencies = new Dictionary<string, string> { ["es-toolkit"] = version } };
        await File.WriteAllTextAsync(manifest, JsonSerializer.Serialize(package), token);
        await File.WriteAllTextAsync(lockfile, $$"""
            lockfileVersion: '9.0'

            importers:
              .:
                dependencies:
                  es-toolkit:
                    specifier: {{version}}
                    version: {{version}}

            packages:
              es-toolkit@{{version}}:
                resolution: {integrity: {{integrity}}}

            snapshots:
              es-toolkit@{{version}}: {}
            """, token);
    }

    Task Change(string targetUri, int version, string text) =>
        Send("textDocument/didChange", new { textDocument = new { uri = targetUri, version }, contentChanges = new[] { new { text } } });

    Task Request(string method, int id, int character) =>
        Send("textDocument/" + method, new { textDocument = document, position = new { line = 0, character } }, id);

    async Task<JsonElement[]> Ready(int version, bool expectClean = false)
    {
        var diagnostics = await WaitForNpmAsync(version, sourceUri);
        if (expectClean && diagnostics.Any(item => item.GetProperty("severity").GetInt32() == 1))
            throw new InvalidOperationException("Unexpected npm value errors: " + string.Join(", ", diagnostics.Select(item => item.GetRawText())));
        return diagnostics;
    }

    async Task CheckDefinition(int id, string version)
    {
        await Request("definition", id, 11);
        var definition = await Response(id);
        var target = new Uri(definition.GetProperty("uri").GetString()!).LocalPath;
        var normalized = target.Replace('\\', '/');
        var installedPackage = Path.Combine(directory, "node_modules", "es-toolkit", "package.json");
        using var installed = JsonDocument.Parse(await File.ReadAllTextAsync(installedPackage, token));
        var start = definition.GetProperty("range").GetProperty("start");
        if (!normalized.Contains("/es-toolkit/", StringComparison.Ordinal) || !File.Exists(target) ||
            installed.RootElement.GetProperty("version").GetString() != version ||
            !(target.EndsWith(".d.ts", StringComparison.Ordinal) || target.EndsWith(".d.mts", StringComparison.Ordinal)) ||
            !File.ReadLines(target).ElementAt(start.GetProperty("line").GetInt32()).Contains("camelCase", StringComparison.Ordinal))
            throw new InvalidOperationException($"Expected camelCase in the declaration locked to {version}: {definition}");
    }

    static void CheckSignature(string? signature)
    {
        if (signature is null || !signature.Contains("string", StringComparison.Ordinal) || !signature.Contains("CamelCase(string", StringComparison.Ordinal))
            throw new InvalidOperationException("Missing typed camelCase signature: " + signature);
    }
}

async Task MeasureWarmAsync()
{
    var completion = new double[20];
    var tokens = new double[completion.Length];
    for (int i = 0; i < completion.Length; i++)
    {
        var elapsed = Stopwatch.StartNew();
        await At("completion", 100 + i, 1, 26);
        await Response(100 + i);
        completion[i] = elapsed.Elapsed.TotalMilliseconds;
        elapsed.Restart();
        await Send("textDocument/semanticTokens/full", new { textDocument = new { uri } }, 130 + i);
        if ((await Response(130 + i)).GetProperty("data").GetArrayLength() == 0)
            throw new InvalidOperationException("npm classification should be available on the ready snapshot.");
        tokens[i] = elapsed.Elapsed.TotalMilliseconds;
    }
    Array.Sort(completion);
    Array.Sort(tokens);
    Console.WriteLine($"Warm npm completion p50/p95 {completion[9]:F1}/{completion[18]:F1} ms. Semantic tokens p50/p95 {tokens[9]:F1}/{tokens[18]:F1} ms.");
}

async Task VerifyNpmFailureAsync()
{
    // A file at the install target gives deterministic write failure
    // without relying on ACLs or a remote registry returning an error.
    var broken = project.CreateSubdirectory("Broken");
    var manifest = Path.Combine(broken.FullName, "package.json");
    await File.WriteAllTextAsync(manifest, """{"private":true,"dependencies":{"reka-ui":"2.11.0","vue":"3.5.42"}}""", token);
    var nodeModules = Path.Combine(broken.FullName, "node_modules");
    await File.WriteAllTextAsync(nodeModules, "unavailable", token);
    var brokenUri = ClientUri(Path.Combine(broken.FullName, "Counter.jazor"));
    const string brokenSource = "@using Npm.RekaUi\n<CollapsibleRoot /><p>@Count</p>\n@code { private int Count = 1; }";
    await Send("textDocument/didOpen", new { textDocument = new { uri = brokenUri, version = 1, languageId = "jazor", text = brokenSource } });
    await Send("textDocument/definition", new { textDocument = new { uri = brokenUri }, position = new { line = 1, character = 24 } }, 151);
    if ((await Response(151)).ValueKind == JsonValueKind.Null)
        throw new InvalidOperationException("A failed npm install must leave local navigation available.");
    while (true)
    {
        var notification = await Read();
        if (!notification.TryGetProperty("method", out var method) || method.GetString() != "textDocument/publishDiagnostics")
            continue;
        var publication = notification.GetProperty("params");
        if (publication.GetProperty("uri").GetString() != new Uri(Path.Combine(broken.FullName, "Counter.jazor")).AbsoluteUri)
            continue;
        if (publication.GetProperty("diagnostics").EnumerateArray().Any(item => item.GetProperty("code").GetString() == "JOLT1018"))
            continue;
        if (!publication.GetProperty("diagnostics").EnumerateArray().Any(item => item.GetProperty("code").GetString() == "JOLT1001"))
            throw new InvalidOperationException("The npm install failure must replace the pending diagnostic.");
        break;
    }
    await Send("textDocument/didClose", new { textDocument = new { uri = brokenUri } });
    Console.WriteLine("Verified npm install failure diagnostics and local navigation in another project.");
}

async Task VerifyLocalGenericAsync()
{
    var directory = project.CreateSubdirectory("Generics");
    var outletPath = Path.Combine(directory.FullName, "TypedList.jazor");
    const string outlet = """
        @typeparam TItem
        @typeparam TKey where TKey : struct
        @foreach (var item in Items) { <slot name="item" value="@item" /> }
        @code {
            [Parameter] public TItem[] Items { get; set; } = [];
            [Parameter] public Func<TItem, TKey> KeyOf { get; set; } = default!;
        }
        """;
    await File.WriteAllTextAsync(outletPath, outlet, token);
    var document = new { uri = ClientUri(Path.Combine(directory.FullName, "Use.jazor")) };
    const string consumer = """
        <TypedList TItem="string" TKey="int" Items="@Names" KeyOf="@(name => name.Length)">
            <template #item context="row">
                @{
                    var current = row.value;
                    for (var i = 0; i < 1; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                <span>@current.Length @row.value.Length</span>
                                break;
                        }
                    }
                }
            </template>
        </TypedList>
        @code { private string[] Names = ["One"]; }
        """;
    await Send("textDocument/didOpen", new { textDocument = new { document.uri, version = 1, languageId = "jazor", text = consumer } });
    var lines = consumer.Split('\n');
    var localUse = Array.FindIndex(lines, line => line.Contains("@current.Length", StringComparison.Ordinal));
    var localStart = lines[localUse].IndexOf("current", StringComparison.Ordinal);
    var memberStart = lines[localUse].LastIndexOf("Length", StringComparison.Ordinal);
    await At("completion", 75, memberStart + 3);
    if (!(await Response(75)).EnumerateArray().Any(item => item.GetProperty("label").GetString() == "Length"))
        throw new InvalidOperationException("The local generic slot should expose the inferred string members.");
    await At("definition", 76, localStart + 2);
    var definition = (await Response(76)).GetProperty("range").GetProperty("start");
    if (definition.GetProperty("line").GetInt32() != 3 ||
        definition.GetProperty("character").GetInt32() != lines[3].IndexOf("current", StringComparison.Ordinal))
        throw new InvalidOperationException("A local used inside a switch should navigate to its template declaration.");
    await At("completion", 77, localStart + 2);
    if (!(await Response(77)).EnumerateArray().Any(item => item.GetProperty("label").GetString() == "current"))
        throw new InvalidOperationException("The enclosing local must be available inside the switch.");
    var contextStart = lines[localUse].IndexOf("row.value", StringComparison.Ordinal);
    await At("definition", 79, contextStart + 1);
    var context = (await Response(79)).GetProperty("range").GetProperty("start");
    if (context.GetProperty("line").GetInt32() != 1 ||
        context.GetProperty("character").GetInt32() != lines[1].IndexOf("\"row\"", StringComparison.Ordinal) + 1)
        throw new InvalidOperationException("A slot context used inside a switch should navigate to the authored context name.");
    await At("definition", 80, contextStart + "row.va".Length);
    var member = await Response(80);
    var memberDefinition = member.GetProperty("range").GetProperty("start");
    if (member.GetProperty("uri").GetString() != new Uri(outletPath).AbsoluteUri ||
        memberDefinition.GetProperty("line").GetInt32() != 2 ||
        memberDefinition.GetProperty("character").GetInt32() != outlet.Split('\n')[2].IndexOf("value", StringComparison.Ordinal))
        throw new InvalidOperationException("A slot member must navigate to the outlet attribute that declares it.");

    var broken = consumer.Replace("row.value.Length", "row.value.Lenght", StringComparison.Ordinal);
    await Change(2, broken);
    await CheckDiagnostic(2, "CS1061", localUse, memberStart);
    await Change(3, consumer.Replace("TKey=\"int\"", "TKey=\"string\"", StringComparison.Ordinal));
    await CheckDiagnostic(3, "JOLT1021", 0, lines[0].IndexOf("TKey=\"", StringComparison.Ordinal) + 6);
    await Send("textDocument/didClose", new { textDocument = document });
    Console.WriteLine("Verified local navigation and completion in a switch, typed slot completion, and source-linked generic diagnostics.");

    Task At(string method, int id, int character) => Send("textDocument/" + method, new
    {
        textDocument = document, position = new { line = localUse, character },
    }, id);

    Task Change(int version, string text) => Send("textDocument/didChange", new
    {
        textDocument = new { document.uri, version }, contentChanges = new[] { new { text } },
    });

    async Task CheckDiagnostic(int version, string code, int line, int character)
    {
        var canonical = new Uri(Path.Combine(directory.FullName, "Use.jazor")).AbsoluteUri;
        while (true)
        {
            var message = await Read();
            if (!message.TryGetProperty("method", out var method) || method.GetString() != "textDocument/publishDiagnostics")
                continue;
            var publication = message.GetProperty("params");
            if (publication.GetProperty("uri").GetString() != canonical ||
                publication.GetProperty("version").GetInt32() != version)
                continue;
            var diagnostics = publication.GetProperty("diagnostics");
            if (!diagnostics.EnumerateArray().Any(item => item.GetProperty("code").GetString() == code &&
                item.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32() == line &&
                item.GetProperty("range").GetProperty("start").GetProperty("character").GetInt32() == character))
                throw new InvalidOperationException($"Expected {code} at ({line}, {character}), got {diagnostics}.");
            return;
        }
    }
}

async Task VerifyLocalModelAsync()
{
    var folder = project.CreateSubdirectory("Models");
    var child = Path.Combine(folder.FullName, "Field.jazor");
    await File.WriteAllTextAsync(child, """
        <p>@Value</p>
        @code {
            [Parameter, EditorRequired] public string Value { get; set; } = "";
            [Parameter] public Action<string> ValueChanged { get; set; } = default!;
        }
        """, token);
    var document = new { uri = ClientUri(Path.Combine(folder.FullName, "Use.jazor")) };
    const string markup = """
        <Field @bind-Value:get="Text" @bind-Value:set="SetText" />
        @code {
            private string Text = "";
            private void SetText(string text) => Text = text;
        }
        """;
    await Send("textDocument/didOpen", new { textDocument = new { document.uri, version = 1, languageId = "jazor", text = markup } });
    var position = new { line = 0, character = 15 };
    await Send("textDocument/completion", new { textDocument = document, position }, 86);
    var choices = (await Response(86)).EnumerateArray().ToArray();
    CheckEdit(choices.Single(item => item.GetProperty("label").GetString() == "@bind-Value:get"), "@bind-Value:get", 7, 22);
    if (choices.Any(item => item.GetProperty("label").GetString() is "@bind-Value" or "@bind-Value:after"))
        throw new InvalidOperationException("A split model must only offer compatible binding forms.");
    await Send("textDocument/definition", new { textDocument = document, position }, 87);
    if ((await Response(87)).GetProperty("uri").GetString() != new Uri(child).AbsoluteUri)
        throw new InvalidOperationException("A split model name must navigate to its declared property.");
    await Send("textDocument/rename", new { textDocument = document, position, newName = "Caption" }, 88);
    var edits = (await Response(88)).GetProperty("documentChanges").EnumerateArray().SelectMany(change =>
        change.GetProperty("edits").EnumerateArray()).ToArray();
    if (!edits.Any(edit => IsEdit(edit, 0, 13)) || !edits.Any(edit => IsEdit(edit, 0, 36)) ||
        !edits.Any(edit => IsEdit(edit, 3, 38)))
        throw new InvalidOperationException($"Renaming a split model must preserve both directive suffixes and the paired callback. Edits: {string.Join(", ", edits.Select(edit => edit.GetRawText()))}");
    await Send("textDocument/didClose", new { textDocument = document });

    static bool IsEdit(JsonElement edit, int line, int character)
    {
        var range = edit.GetProperty("range");
        var start = range.GetProperty("start");
        return edit.GetProperty("newText").GetString() == "Caption" &&
            start.GetProperty("line").GetInt32() == line && start.GetProperty("character").GetInt32() == character &&
            range.GetProperty("end").GetProperty("character").GetInt32() == character + "Value".Length;
    }
}

async Task VerifySharedImportsAsync()
{
    var directory = project.CreateSubdirectory("Imports");
    var components = project.CreateSubdirectory("Components");
    await File.WriteAllTextAsync(Path.Combine(components.FullName, "SharedPanel.jazor"), "<p>Shared panel</p>", token);
    var importsUri = ClientUri(Path.Combine(directory.FullName, "_Imports.jazor"));
    var pageUri = ClientUri(Path.Combine(directory.FullName, "ImportsConsumer.jazor"));
    await Send("textDocument/didOpen", new
    {
        textDocument = new { uri = importsUri, version = 1, languageId = "jazor", text = "@using App.Components" },
    });
    await Send("textDocument/didOpen", new
    {
        textDocument = new { uri = pageUri, version = 1, languageId = "jazor", text = "<SharedPanel />" },
    });
    await Send("textDocument/definition", new
    {
        textDocument = new { uri = pageUri }, position = new { line = 0, character = 4 },
    }, 78);
    if ((await Response(78)).GetProperty("uri").GetString() != new Uri(Path.Combine(components.FullName, "SharedPanel.jazor")).AbsoluteUri)
        throw new InvalidOperationException("An unsaved shared import must bind component tags in descendant documents.");
    await Send("textDocument/didClose", new { textDocument = new { uri = pageUri } });
    await Send("textDocument/didClose", new { textDocument = new { uri = importsUri } });
    Console.WriteLine("Verified that unsaved shared imports bind component tags.");
}

async Task VerifyRefactoringAsync()
{
    var first = project.CreateSubdirectory("First").FullName;
    var second = project.CreateSubdirectory("Second").FullName;
    var child = Path.Combine(first, "Widget.jazor");
    var other = Path.Combine(second, "Widget.jazor");
    var closed = Path.Combine(first, "Closed.jazor");
    var open = Path.Combine(first, "Open.jazor");
    var openDocument = new { uri = ClientUri(open) };
    const string component = "<p>@数值</p><slot name=\"heading\" />\r\n@code { [Parameter] public int 数值 { get; set; } }";
    const string reference = "<Widget 数值=\"@(1)\"><template #heading>😀</template></Widget>";
    await File.WriteAllTextAsync(child, component, token);
    await File.WriteAllTextAsync(other, component, token);
    await File.WriteAllTextAsync(closed, reference, token);
    await File.WriteAllTextAsync(Path.Combine(second, "Other.jazor"), reference, token);
    await Send("textDocument/didOpen", new { textDocument = new { openDocument.uri, version = 1, languageId = "jazor", text = reference } });

    await Request("references", 60, 9, newName: "");
    var references = (await Response(60)).EnumerateArray().ToArray();
    var paths = references.Select(location => location.GetProperty("uri").GetString()).ToArray();
    if (paths.Length != 4 || !paths.Contains(new Uri(closed).AbsoluteUri) ||
        !paths.Contains(new Uri(open).AbsoluteUri) || paths.Count(path => path == new Uri(child).AbsoluteUri) != 2)
        throw new InvalidOperationException("Unexpected bound references: " + string.Join(", ", references.Select(reference => reference.GetRawText())));
    await Request("definition", 61, reference.IndexOf("heading", StringComparison.Ordinal) + 2, "");
    if ((await Response(61)).GetProperty("uri").GetString() != new Uri(child).AbsoluteUri)
        throw new InvalidOperationException("Slot definition did not reach the matching outlet.");
    await Request("rename", 62, 9, "计数");
    var renamedProps = await Response(62);
    var input = new Dictionary<string, string> { [child] = component, [closed] = reference, [open] = reference };
    Apply(renamedProps, input, "计数", 4);
    if (input[child] != component.Replace("数值", "计数") || input.Values.Any(text => text.Contains("数值")))
        throw new InvalidOperationException("Applying the property rename did not update authored C# and markup.");

    await Request("rename", 63, reference.IndexOf("heading", StringComparison.Ordinal) + 2, "title");
    var slots = new Dictionary<string, string> { [child] = component, [closed] = reference, [open] = reference };
    Apply(await Response(63), slots, "title", 3);

    await Request("prepareRename", 64, reference.LastIndexOf("Widget", StringComparison.Ordinal) + 1, "");
    if ((await Response(64)).GetProperty("placeholder").GetString() != "Widget")
        throw new InvalidOperationException("Closing tags must be rename targets.");
    await Request("rename", 65, 2, "Tile");
    var components = new Dictionary<string, string> { [closed] = reference, [open] = reference };
    var movedPath = Apply(await Response(65), components, "Tile", 4);
    if (movedPath != Path.Combine(first, "Tile.jazor") || components.Values.Any(text => text.Contains("Widget")))
        throw new InvalidOperationException("Component rename did not include matching opening and closing tags and the file.");
    if (await File.ReadAllTextAsync(other, token) != component)
        throw new InvalidOperationException("The separate namespace fixture changed.");

    await File.WriteAllTextAsync(child, input[child], token);
    await File.WriteAllTextAsync(closed, input[closed], token);
    await Send("workspace/didChangeWatchedFiles", new { changes = new[] { new { uri = ClientUri(child), type = 2 } } });
    await Send("textDocument/didChange", new { textDocument = new { openDocument.uri, version = 2 }, contentChanges = new[] { new { text = input[open] } } });
    await Request("definition", 66, 9, "");
    if ((await Response(66)).GetProperty("uri").GetString() != new Uri(child).AbsoluteUri)
        throw new InvalidOperationException("Applying edits must preserve the parameter binding after invalidation.");
    await Send("textDocument/didClose", new { textDocument = openDocument });
    var exports = new { uri = ClientUri(Path.Combine(project.FullName, "Exports.jazor")) };
    const string exportSource = "<p>@Step</p>\r\n@code { public static int Step = 1; }";
    await Send("textDocument/didOpen", new { textDocument = new { exports.uri, version = 1, languageId = "jazor", text = exportSource } });
    await Send("textDocument/rename", new { textDocument = exports, position = new { line = 0, character = 6 }, newName = "Stride" }, 67);
    var exportEdit = await Response(67);
    var namedEdits = exportEdit.GetProperty("documentChanges")[0].GetProperty("edits").EnumerateArray().ToArray();
    if (!exportEdit.GetProperty("changeAnnotations").GetProperty("export").GetProperty("needsConfirmation").GetBoolean() ||
        namedEdits.Length != 2 || namedEdits.Any(edit => edit.GetProperty("annotationId").GetString() != "export"))
        throw new InvalidOperationException($"Unexpected named export rename: {exportEdit}");
    var updatedExport = exportSource;
    foreach (var edit in namedEdits.Reverse())
    {
        var range = edit.GetProperty("range");
        updatedExport = updatedExport[..Offset(updatedExport, range.GetProperty("start"))] +
            edit.GetProperty("newText").GetString() + updatedExport[Offset(updatedExport, range.GetProperty("end"))..];
    }
    if (updatedExport != exportSource.Replace("Step", "Stride", StringComparison.Ordinal))
        throw new InvalidOperationException("Applying the rename must update the export and its use.");
    await Send("textDocument/didClose", new { textDocument = exports });
    Console.WriteLine("Verified bound references and versioned property, slot, and file rename edits, including UTF-16 and closed documents.");

    Task Request(string method, int id, int character, string newName) => Send($"textDocument/{method}", new
    {
        textDocument = openDocument, position = new { line = 0, character },
        context = new { includeDeclaration = true }, newName,
    }, id);

    string? Apply(JsonElement edit, Dictionary<string, string> files, string expected, int count)
    {
        string? moved = null;
        foreach (var change in edit.GetProperty("documentChanges").EnumerateArray())
        {
            if (change.TryGetProperty("kind", out _))
            {
                if (change.GetProperty("oldUri").GetString() != new Uri(child).AbsoluteUri)
                    throw new InvalidOperationException("RenameFile targeted a different component.");
                moved = new Uri(change.GetProperty("newUri").GetString()!).LocalPath;
                continue;
            }
            var document = change.GetProperty("textDocument");
            var path = new Uri(document.GetProperty("uri").GetString()!).LocalPath;
            if (path == open ? document.GetProperty("version").GetInt32() != 1 : document.GetProperty("version").ValueKind != JsonValueKind.Null)
                throw new InvalidOperationException("Incorrect workspace edit version.");
            var text = files[path];
            foreach (var item in change.GetProperty("edits").EnumerateArray().Reverse())
            {
                var range = item.GetProperty("range");
                int start = Offset(text, range.GetProperty("start"));
                int end = Offset(text, range.GetProperty("end"));
                var replacement = item.GetProperty("newText").GetString();
                if (replacement != expected)
                    throw new InvalidOperationException($"Unexpected rename replacement {replacement}.");
                text = text[..start] + replacement + text[end..];
                count--;
            }
            files[path] = text;
        }
        if (count != 0)
            throw new InvalidOperationException($"Expected the rename to produce {count} additional edits.");
        return moved;
    }

    static int Offset(string text, JsonElement point)
    {
        int start = 0;
        for (int line = 0; line < point.GetProperty("line").GetInt32(); line++)
            start = text.IndexOf('\n', start) + 1;
        return start + point.GetProperty("character").GetInt32();
    }
}

async Task VerifyVueBuiltInsAsync()
{
    const string source = """
        <Teleport To="#overlays" Disabled="false"><p /></Teleport>
        <Transition Mode="out-in"><template #default><p /></template></Transition>
        """;
    var builtInUri = ClientUri(Path.Combine(project.FullName, "BuiltIns.jazor"));
    await Send("textDocument/didOpen", new { textDocument = new { uri = builtInUri, version = 1, languageId = "jazor", text = source } });
    var tag = (await Complete(710, 0, 5)).Single(item => item.GetProperty("label").GetString() == "Teleport");
    if (tag.TryGetProperty("additionalTextEdits", out _))
        throw new InvalidOperationException("Vue built-in completion proposed an import.");
    var props = await Complete(711, 0, 11);
    if (!props.Any(item => item.GetProperty("label").GetString() == "To"))
        throw new InvalidOperationException("Missing typed Teleport target completion.");
    var modes = await Complete(712, 1, 19);
    if (!modes.Any(item => item.GetProperty("label").GetString() == "out-in"))
        throw new InvalidOperationException("Missing Transition mode completion.");
    var slots = await Complete(713, 1, source.Split('\n')[1].IndexOf("#default", StringComparison.Ordinal) + 2);
    if (!slots.Any(item => item.GetProperty("label").GetString() == "default"))
        throw new InvalidOperationException("Missing Transition default slot completion.");
    await Send("textDocument/didClose", new { textDocument = new { uri = builtInUri } });

    async Task<JsonElement[]> Complete(int id, int line, int character)
    {
        await Send("textDocument/completion", new { textDocument = new { uri = builtInUri }, position = new { line, character } }, id);
        return (await Response(id)).EnumerateArray().ToArray();
    }
}

async Task VerifyDirectivesAsync(JsonElement legend)
{
    const string markup = """
        <input @bind="Name" @key="Missing" />
        <button @onclick="Run" @onclick:preventDefault="Cancel">Go</button>
        <input type="checkbox" @bind="Cancel" />
        @code {
            private string Name = "";
            private bool Cancel = true;
            private void Run() {}
        }
        """;
    var directiveUri = ClientUri(Path.Combine(project.FullName, "Directives.jazor"));
    await Send("textDocument/didOpen", new { textDocument = new { uri = directiveUri, version = 1, languageId = "jazor", text = markup } });
    await AtDirective("definition", 180, 0, 15);
    if ((await Response(180)).GetProperty("range").GetProperty("start").GetProperty("line").GetInt32() != 4)
        throw new InvalidOperationException("The bare bind target must navigate to the state member.");
    await AtDirective("definition", 181, 1, 51);
    if ((await Response(181)).GetProperty("range").GetProperty("start").GetProperty("line").GetInt32() != 5)
        throw new InvalidOperationException("The event modifier condition must navigate to the state member.");
    await AtDirective("completion", 182, 0, 11);
    var bind = (await Response(182)).EnumerateArray().Single(item => item.GetProperty("label").GetString() == "@bind");
    CheckEdit(bind, "@bind", 7, 12);
    await AtDirective("completion", 183, 1, 32);
    var modifier = (await Response(183)).EnumerateArray().Single(item => item.GetProperty("label").GetString() == "@onclick:stopPropagation");
    CheckEdit(modifier, "@onclick:stopPropagation", 23, 46);
    await AtDirective("completion", 184, 2, 25);
    if ((await Response(184)).EnumerateArray().Any(item => item.GetProperty("label").GetString() == "@bind:event"))
        throw new InvalidOperationException("Checkbox binding always uses change events.");
    await Send("textDocument/semanticTokens/full", new { textDocument = new { uri = directiveUri } }, 185);
    var data = (await Response(185)).GetProperty("data").EnumerateArray().Select(item => item.GetInt32()).ToArray();
    var line = 0;
    var column = 0;
    var classified = new HashSet<(int, int, int, string?)>();
    for (var i = 0; i < data.Length; i += 5)
    {
        line += data[i];
        column = data[i] == 0 ? column + data[i + 1] : data[i + 1];
        classified.Add((line, column, data[i + 2], legend[data[i + 3]].GetString()));
    }
    if (!classified.Contains((1, 23, 23, "event name")) || !classified.Contains((1, 48, 6, "field name")))
        throw new InvalidOperationException("Event modifiers and their conditions must receive semantic classification.");
    await Send("textDocument/didChange", new
    {
        textDocument = new { uri = directiveUri, version = 2 },
        contentChanges = new[] { new { text = markup } },
    });
    while (true)
    {
        var message = await Read();
        if (!message.TryGetProperty("method", out var name) || name.GetString() != "textDocument/publishDiagnostics")
            continue;
        var publication = message.GetProperty("params");
        if (publication.GetProperty("uri").GetString() != new Uri(Path.Combine(project.FullName, "Directives.jazor")).AbsoluteUri ||
            publication.GetProperty("version").GetInt32() != 2)
            continue;
        var key = publication.GetProperty("diagnostics").EnumerateArray().Single(item => item.GetProperty("code").GetString() == "CS0103");
        var start = key.GetProperty("range").GetProperty("start");
        if (start.GetProperty("line").GetInt32() != 0 || start.GetProperty("character").GetInt32() != 26)
            throw new InvalidOperationException("The key diagnostic must identify the authored expression.");
        break;
    }
    await Send("textDocument/didClose", new { textDocument = new { uri = directiveUri } });
    Console.WriteLine("Verified directive completion, source-linked diagnostics, and definition mapping.");

    Task AtDirective(string request, int id, int line, int character) =>
        Send("textDocument/" + request, new { textDocument = new { uri = directiveUri }, position = new { line, character } }, id);
}

async Task<JsonElement[]> WaitForNpmAsync(int version, string? documentUri = null)
{
    while (true)
    {
        var message = await Read();
        if (!message.TryGetProperty("method", out var method) || method.GetString() != "textDocument/publishDiagnostics")
            continue;
        var publication = message.GetProperty("params");
        if (publication.GetProperty("uri").GetString() != (documentUri ?? canonicalUri) ||
            publication.GetProperty("version") is not { ValueKind: JsonValueKind.Number } actual || actual.GetInt32() != version)
            continue;
        var diagnostics = publication.GetProperty("diagnostics").EnumerateArray().ToArray();
        if (diagnostics.Any(item => item.GetProperty("code").GetString() == "JOLT1018"))
            continue;
        if (diagnostics.FirstOrDefault(item => item.GetProperty("code").GetString() == "JOLT1001") is { ValueKind: JsonValueKind.Object } error)
            throw new InvalidOperationException(error.GetProperty("message").GetString());
        return diagnostics;
    }
}

async Task VerifyEditingAsync()
{
    var path = Path.Combine(project.FullName, "Editing.jazor");
    var document = new { uri = ClientUri(path) };
    const string source = "<Panel\r\n  />\r\n<pre>  a   &amp;  b</pre>\r\n@code { private int Value=1;private void Go(){Value+= 2;} }";
    var current = source;
    var options = new { tabSize = 4, insertSpaces = true };
    await Send("textDocument/didOpen", new { textDocument = new { document.uri, version = 1, languageId = "jazor", text = current } });
    await Send("textDocument/formatting", new { textDocument = document, options }, 70);
    Apply(await Response(70));
    if (!current.StartsWith(source[..source.IndexOf("@code", StringComparison.Ordinal)], StringComparison.Ordinal) ||
        !current.Contains("Value = 1", StringComparison.Ordinal) || !current.Contains("Value += 2", StringComparison.Ordinal) ||
        current.Replace("\r\n", "").Contains('\n'))
        throw new InvalidOperationException("Formatting did not preserve markup text or CRLF while formatting authored C#.");
    await Change(2);
    await Send("textDocument/formatting", new { textDocument = document, options }, 71);
    if ((await Response(71)).GetArrayLength() != 0)
        throw new InvalidOperationException("Formatting must be idempotent.");

    var tagRange = new { start = new { line = 0, character = 1 }, end = new { line = 0, character = 6 } };
    await Actions(72, tagRange, "JOLT1001", "Component 'Panel' was not found.");
    var import = (await Response(72)).EnumerateArray().Single(item => item.GetProperty("title").GetString() == "Add @using App.Widgets");
    ApplyAction(import, 2);
    await Change(3);
    var importedRange = new { start = new { line = 2, character = 1 }, end = new { line = 2, character = 6 } };
    await Actions(73, importedRange, "JOLT1005", "Component 'Panel' requires prop 'Value'.");
    ApplyAction((await Response(73)).EnumerateArray().Single(), 3);
    await Change(4);
    await Actions(74, importedRange, "JOLT1005", "Component 'Panel' requires prop 'Value'.");
    if ((await Response(74)).GetArrayLength() != 0 || !current.Contains("<Panel Value=\"@(this.Value)\"", StringComparison.Ordinal))
        throw new InvalidOperationException("The required parameter quick fix did not resolve the diagnostic.");
    await Send("textDocument/didClose", new { textDocument = document });
    Console.WriteLine("Verified idempotent CRLF formatting, preservation of markup whitespace, and versioned quick fixes.");

    Task Change(int version) => Send("textDocument/didChange", new
    {
        textDocument = new { document.uri, version }, contentChanges = new[] { new { text = current } },
    });

    Task Actions(int id, object range, string code, string message) => Send("textDocument/codeAction", new
    {
        textDocument = document, range, context = new { diagnostics = new[] { new { code, message, range } } },
    }, id);

    void ApplyAction(JsonElement action, int expectedVersion)
    {
        var edit = action.GetProperty("edit").GetProperty("documentChanges")[0];
        if (edit.GetProperty("textDocument").GetProperty("version").GetInt32() != expectedVersion)
            throw new InvalidOperationException("Incorrect quick fix version.");
        Apply(edit.GetProperty("edits"));
    }

    void Apply(JsonElement edits)
    {
        foreach (var edit in edits.EnumerateArray().Reverse())
        {
            var range = edit.GetProperty("range");
            int start = Offset(range.GetProperty("start"));
            int end = Offset(range.GetProperty("end"));
            current = current[..start] + edit.GetProperty("newText").GetString() + current[end..];
        }
    }

    int Offset(JsonElement point)
    {
        int start = 0;
        for (int line = 0; line < point.GetProperty("line").GetInt32(); line++)
            start = current.IndexOf('\n', start) + 1;
        return start + point.GetProperty("character").GetInt32();
    }
}

async Task VerifyAutoImportAsync()
{
    var first = project.CreateSubdirectory("Widgets");
    var second = project.CreateSubdirectory("Dialogs");
    foreach (var directory in new[] { first, second })
        await File.WriteAllTextAsync(Path.Combine(directory.FullName, "Panel.jazor"),
            "@code { [Parameter, EditorRequired] public int Value { get; set; } }", token);

    var editorUri = ClientUri(Path.Combine(project.FullName, "Imports.jazor"));
    var document = new { uri = editorUri };
    const string source = "<Pa />";
    await Send("textDocument/didOpen", new { textDocument = new { uri = editorUri, version = 1, languageId = "jazor", text = source } });
    await Position("completion", 31, 0, 3);
    var options = (await Response(31)).EnumerateArray().Where(item => item.GetProperty("label").GetString()!.StartsWith("Panel (", StringComparison.Ordinal)).ToArray();
    if (options.Length != 2)
        throw new InvalidOperationException("Expected two namespace-qualified Panel options.");
    var selected = options.Single(item => item.GetProperty("label").GetString() == "Panel (App.Widgets)");
    CheckEdit(selected, "Panel", 1, 3);
    var import = selected.GetProperty("additionalTextEdits")[0];
    var importText = import.GetProperty("newText").GetString();
    if (importText != "@using App.Widgets\n\n" ||
        import.GetProperty("range").GetProperty("start").GetProperty("character").GetInt32() != 0)
        throw new InvalidOperationException($"Incorrect import edit: {import}");

    await Change(importText + "<Panel  />", 2);
    await Position("completion", 32, 2, 8);
    var prop = (await Response(32)).EnumerateArray().Single(item => item.GetProperty("label").GetString() == "Value");
    if (prop.GetProperty("detail").GetString()?.Contains("required", StringComparison.Ordinal) != true)
        throw new InvalidOperationException("Auto-import did not enable typed parameter completion.");

    await Position("completion", 33, 2, 3);
    if ((await Response(33)).EnumerateArray().Any(item => item.TryGetProperty("additionalTextEdits", out _) &&
        item.GetProperty("filterText").GetString() == "Panel"))
        throw new InvalidOperationException("An existing Panel binding must not get an ambiguous import.");

    await Change("@using System\r\n\r\n<Pa />", 3);
    await Position("completion", 34, 2, 3);
    var crlf = (await Response(34)).EnumerateArray().Single(item => item.GetProperty("label").GetString() == "Panel (App.Widgets)");
    if (crlf.GetProperty("additionalTextEdits")[0].GetProperty("newText").GetString() != "@using App.Widgets\r\n")
        throw new InvalidOperationException("Auto-import changed the author's line endings.");

    await Send("textDocument/didClose", new { textDocument = document });
    Console.WriteLine("Verified local component discovery, import edits, and parameter binding after accepting the edit.");

    Task Position(string method, int id, int line, int character) =>
        Send($"textDocument/{method}", new { textDocument = document, position = new { line, character } }, id);
    Task Change(string text, int version) =>
        Send("textDocument/didChange", new { textDocument = new { uri = editorUri, version }, contentChanges = new[] { new { text } } });
}

async Task VerifyEditorAsync(JsonElement legend)
{
    var editorUri = ClientUri(Path.Combine(project.FullName, "Editor.jazor"));
    var child = Path.Combine(project.FullName, "EditorChild.jazor");
    await File.WriteAllTextAsync(child, """
        <slot name="header" />
        @code {
            [Parameter, EditorRequired] public int Value { get; set; }
            [Parameter] public bool Enabled { get; set; }
        }
        """, token);
    const string source = """
        <EditorChild Value="@Count"  >
            <template #he><div>@(Format(Count))</div></template>
        </EditorChild>
        @code {
            private int Count;
            private string Format(int value) => value.ToString();
        }
        """;
    var lines = source.Split('\n');
    var editorDocument = new { uri = editorUri };
    await Send("textDocument/didOpen", new { textDocument = new { uri = editorUri, version = 1, languageId = "jazor", text = source } });
    await Send("textDocument/semanticTokens/full", new { textDocument = editorDocument }, 21);
    var classifications = ReadTokens(await Response(21));
    foreach (var expected in new (int, string, string)[]
    {
        (0, "EditorChild", "class name"), (0, "Value", "property name"), (0, "Count", "field name"),
        (1, "Format", "method name"), (2, "EditorChild", "class name"), (4, "Count", "field name"),
        (5, "value", "parameter name"),
    })
        if (!classifications.Contains(expected))
            throw new InvalidOperationException($"Missing classification {expected}: {string.Join(", ", classifications)}");

    await Send("textDocument/semanticTokens/range", new
    {
        textDocument = editorDocument,
        range = new { start = new { line = 4, character = 0 }, end = new { line = 5, character = 0 } },
    }, 22);
    if (!ReadTokens(await Response(22)).SequenceEqual([(4, "Count", "field name")]))
        throw new InvalidOperationException("The semantic range returned tokens outside the requested author line.");

    await Send("textDocument/completion", new
    {
        textDocument = editorDocument,
        position = new { line = 0, character = lines[0].Length - 2 },
    }, 23);
    var props = (await Response(23)).EnumerateArray().ToArray();
    if (!props.Any(item => item.GetProperty("label").GetString() == "Enabled" && item.GetProperty("kind").GetInt32() == 10) ||
        props.Any(item => item.GetProperty("label").GetString() == "Value"))
        throw new InvalidOperationException("Component completion lost the unused property or suggested the existing property.");

    await Send("textDocument/completion", new
    {
        textDocument = editorDocument,
        position = new { line = 1, character = lines[1].IndexOf("#he", StringComparison.Ordinal) + 3 },
    }, 24);
    if (!(await Response(24)).EnumerateArray().Any(item => item.GetProperty("label").GetString() == "header"))
        throw new InvalidOperationException("Local slot completion was missing.");

    await Send("textDocument/didClose", new { textDocument = editorDocument });
    File.Delete(child);
    Console.WriteLine("Verified Roslyn semantic colors, component tags and props, ranged tokens, and typed prop and slot completion.");

    List<(int, string, string)> ReadTokens(JsonElement response)
    {
        var data = response.GetProperty("data").EnumerateArray().Select(item => item.GetInt32()).ToArray();
        var result = new List<(int, string, string)>();
        var line = 0;
        var column = 0;
        for (var index = 0; index < data.Length; index += 5)
        {
            line += data[index];
            column = data[index] == 0 ? column + data[index + 1] : data[index + 1];
            result.Add((line, lines[line].Substring(column, data[index + 2]), legend[data[index + 3]].GetString()!));
        }
        return result;
    }
}

async Task CheckTrigger(string markup, int version, int character, int id, string triggerCharacter, string expected)
{
    await Send("textDocument/didChange", new
    {
        textDocument = new { uri, version },
        contentChanges = new[] { new { text = markup + "\n@code { private string Text = \"a\"; }" } },
    });
    await Send("textDocument/completion", new
    {
        textDocument = new { uri },
        position = new { line = 0, character },
        context = new { triggerKind = 2, triggerCharacter },
    }, id);
    var item = (await Response(id)).EnumerateArray().Single(item => item.GetProperty("label").GetString() == expected);
    CheckEdit(item, expected, character, character);
}

static string ClientUri(string path)
{
    var uri = new Uri(path).AbsoluteUri;
    const int driveColon = 9;
    return OperatingSystem.IsWindows() && uri[driveColon] == ':'
        ? uri[..driveColon] + "%3A" + uri[(driveColon + 1)..]
        : uri;
}

static void CheckEdit(JsonElement item, string newText, int start, int end)
{
    var edit = item.GetProperty("textEdit");
    var range = edit.GetProperty("range");
    if (edit.GetProperty("newText").GetString() != newText ||
        range.GetProperty("start").GetProperty("character").GetInt32() != start ||
        range.GetProperty("end").GetProperty("character").GetInt32() != end)
        throw new InvalidOperationException($"Incorrect completion edit: {edit}");
}
async Task Send(string method, object args, int? id = null)
{
    var json = id.HasValue
        ? JsonSerializer.SerializeToUtf8Bytes(new { jsonrpc = "2.0", id, method, @params = args })
        : JsonSerializer.SerializeToUtf8Bytes(new { jsonrpc = "2.0", method, @params = args });
    var input = process.StandardInput.BaseStream;
    await input.WriteAsync(Encoding.ASCII.GetBytes($"Content-Length: {json.Length}\r\n\r\n"), token);
    await input.WriteAsync(json, token);
    await input.FlushAsync(token);
}

async Task<JsonElement> Response(int id)
{
    while (true)
    {
        var message = await Read();
        if (!message.TryGetProperty("id", out var value) || value.ValueKind != JsonValueKind.Number || value.GetInt32() != id)
            continue;
        if (message.TryGetProperty("error", out var error))
            throw new InvalidOperationException(error.ToString());
        return message.GetProperty("result");
    }
}

async Task<JsonElement> Read()
{
    var output = process.StandardOutput.BaseStream;
    var read = new byte[1];
    while (true)
    {
        var header = new StringBuilder();
        while (!header.ToString().EndsWith("\r\n\r\n", StringComparison.Ordinal))
        {
            await output.ReadExactlyAsync(read, token);
            header.Append((char)read[0]);
        }
        var length = int.Parse(header.ToString().AsSpan("Content-Length: ".Length).Trim());
        var bytes = new byte[length];
        await output.ReadExactlyAsync(bytes, token);
        var message = JsonSerializer.Deserialize<JsonElement>(bytes);
        if (!message.TryGetProperty("method", out var method) || method.GetString() != "workspace/semanticTokens/refresh")
            return message;
        semanticRefreshes++;
        var response = JsonSerializer.SerializeToUtf8Bytes(new { jsonrpc = "2.0", id = message.GetProperty("id"), result = (object?)null });
        await process.StandardInput.BaseStream.WriteAsync(Encoding.ASCII.GetBytes($"Content-Length: {response.Length}\r\n\r\n"), token);
        await process.StandardInput.BaseStream.WriteAsync(response, token);
        await process.StandardInput.BaseStream.FlushAsync(token);
    }
}
