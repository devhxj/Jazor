#!/usr/bin/env dotnet run
#:property PublishAot=false

using System.Diagnostics;
using System.Globalization;
using System.Net.WebSockets;
using System.Text.Json;

if (args is ["--prepare", var directory])
{
    var sample = Path.GetDirectoryName(ScriptPath())!;
    Directory.CreateDirectory(directory);
    foreach (var path in new[] { "package.json", "pnpm-lock.yaml", "index.html", "vite.config.js" })
        File.Copy(Path.Combine(sample, path), Path.Combine(directory, path));
    foreach (var path in Directory.EnumerateFiles(Path.Combine(sample, ".fixtures"), "*.jazor"))
        File.Copy(path, Path.Combine(directory, Path.GetFileName(path)));
    File.Copy(Path.Combine(sample, ".fixtures", "main.js"), Path.Combine(directory, "main.js"));
    return;
}

var features = args.Contains("--features", StringComparer.Ordinal);
var editableOnly = args.Contains("--editable-only", StringComparer.Ordinal);
var monaco = args.Contains("--monaco", StringComparer.Ordinal);
var moduleHandoff = args.Contains("--module-handoff", StringComparer.Ordinal);
const string moduleValueOption = "--expect-module-value=";
var moduleValue = args.SingleOrDefault(argument => argument.StartsWith(moduleValueOption, StringComparison.Ordinal));
var proxyHmr = args.Contains("--proxy-hmr", StringComparer.Ordinal);
var publishedCounter = args.Contains("--published-counter", StringComparer.Ordinal);
var publishedLazy = args.Contains("--published-lazy", StringComparer.Ordinal);
var correlated = args.Contains("--correlated", StringComparer.Ordinal);
const string mixedBenchmarkOption = "--mixed-benchmark=";
var mixedBenchmark = args.SingleOrDefault(argument => argument.StartsWith(mixedBenchmarkOption, StringComparison.Ordinal));
var mixedRounds = mixedBenchmark is null ? 0 : int.Parse(mixedBenchmark[mixedBenchmarkOption.Length..], CultureInfo.InvariantCulture);
var mixed = args.Contains("--mixed", StringComparer.Ordinal) || mixedBenchmark is not null;
var ssrBenchmark = args.Contains("--ssr-benchmark", StringComparer.Ordinal);
var ssr = args.Contains("--ssr", StringComparer.Ordinal) || ssrBenchmark;
args = args.Where(argument => argument != mixedBenchmark && argument != moduleValue && argument is not ("--features" or "--editable-only" or "--mixed" or "--ssr" or "--ssr-benchmark" or "--monaco" or "--module-handoff" or "--proxy-hmr" or "--correlated" or "--published-counter" or "--published-lazy")).ToArray();
if (args.Length is < 2 or > 3 || (editableOnly || ssr) && args.Length != 2 || proxyHmr && args.Length != 3 ||
    mixedBenchmark is not null && (args.Length != 3 || mixedRounds is < 1 or > 100))
    throw new ArgumentException("Usage: verify-browser.cs <browser-executable> <app-url> [component-source or module handoff scratch root] [--features | --editable-only | --mixed | --mixed-benchmark=<rounds> | --ssr | --monaco | --module-handoff | --proxy-hmr]");

var origin = new Uri(args[1]);
var sampleDirectory = Path.GetDirectoryName(ScriptPath())!;
var profile = Directory.CreateDirectory(Path.GetFullPath(
    Path.Combine("../../test/jolt/browser", Guid.NewGuid().ToString("N")), sampleDirectory));
try
{
    var start = new ProcessStartInfo(args[0]);
    foreach (var arg in new[] { "--headless", "--no-sandbox", "--disable-gpu", "--remote-debugging-port=0",
        $"--user-data-dir={profile.FullName}", "about:blank" })
        start.ArgumentList.Add(arg);
    using var browser = Process.Start(start) ?? throw new InvalidOperationException("Could not start Chromium.");
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(correlated ? 120 : 45 + 20 * mixedRounds));
    try
    {
        var token = timeout.Token;
        var portFile = Path.Combine(profile.FullName, "DevToolsActivePort");
        using var http = new HttpClient();
        using var socket = new ClientWebSocket();
        await RetryAsync(async () =>
        {
            if (!File.Exists(portFile))
                return false;
            string? port;
            try
            {
                port = (await File.ReadAllLinesAsync(portFile, token)).FirstOrDefault();
            }
            catch (IOException exception) when (OperatingSystem.IsWindows() &&
                exception.HResult == unchecked((int)0x80070020))
            {
                // Chrome can create the file before releasing its Windows write handle.
                return false;
            }
            if (!int.TryParse(port, out var endpoint))
                return false;
            using var targets = JsonDocument.Parse(await http.GetStringAsync($"http://127.0.0.1:{endpoint}/json/list", token));
            var page = targets.RootElement.EnumerateArray().FirstOrDefault(target => target.GetProperty("type").GetString() == "page");
            if (page.ValueKind == JsonValueKind.Undefined)
                return false;
            await socket.ConnectAsync(new Uri(page.GetProperty("webSocketDebuggerUrl").GetString()!), token);
            return true;
        }, token);

        var nextId = 0;
        var browserErrors = new List<string>();
        if (ssr || proxyHmr)
        {
            await SendAsync("Runtime.enable", new { });
            await SendAsync("Page.enable", new { });
        }
        if (ssr)
        {
            // Install before navigation so the captured nodes come from the HTML parser.
            await SendAsync("Page.addScriptToEvaluateOnNewDocument", new
            {
                source = """
                    const probe = window.joltSsrProbe = { loading: false, transition: false };
                    new MutationObserver(records => {
                      probe.counter ??= document.querySelector('#counter');
                      probe.teleport ??= document.querySelector('#overlays #teleported-notice');
                      probe.heading ??= document.querySelector('#route-heading');
                      const count = document.querySelector('#prefetch-count')?.textContent.trim();
                      if (count && count !== '1') probe.loading = true;
                      if (records.some(record => record.type === 'attributes' &&
                          record.target.contains(probe.heading) && record.target.classList.contains('route-leave-active')))
                        probe.transition = true;
                    }).observe(document, { childList: true, characterData: true, subtree: true,
                      attributes: true, attributeFilter: ['class'] });
                    """
            });
        }
        if (proxyHmr)
        {
            await SendAsync("Page.addScriptToEvaluateOnNewDocument", new
            {
                source = """
                    window.WebSocket = class extends WebSocket {
                        constructor(url, protocols) {
                            const address = new URL(url, location.href);
                            const base = location.pathname.slice(0, location.pathname.lastIndexOf('/') + 1);
                            if (address.host !== location.host || !address.pathname.startsWith(base))
                                throw new Error('HMR socket bypassed the ASP.NET proxy: ' + address.href);
                            super(url, protocols);
                            this.addEventListener('open', () => { window.proxyHmrConnected = true; });
                        }
                    };
                    """
            });
        }
        if (!ssrBenchmark)
            await SendAsync("Page.navigate", new { url = origin.ToString() });
        if (publishedCounter)
        {
            if (proxyHmr)
                await CheckAsync("Vite connected to ASP.NET's WebSocket endpoint", "window.proxyHmrConnected === true");
            await CheckAsync("the published counter", "document.querySelector('#count')?.textContent === '0'");
            await EvaluateAsync("document.querySelector('#increment').click()");
            await CheckAsync("the published counter after a click", "document.querySelector('#count')?.textContent === '1'");
            if (publishedLazy)
            {
                const string fetched = "performance.getEntriesByType('resource').some(entry => new URL(entry.name).pathname.startsWith('/portal/frontend/assets/lazy-proof-'))";
                await CheckAsync("the lazy chunk is absent before activation",
                    $"document.querySelector('#load-lazy')?.textContent === 'Load optional module' && !({fetched})");
                await EvaluateAsync("document.querySelector('#load-lazy').click()");
                await CheckAsync("the delayed chunk was fetched and executed",
                    $"document.querySelector('#load-lazy')?.textContent === 'from the lazy chunk' && ({fetched}) && document.querySelector('#count')?.textContent === '1'");
            }
            if (proxyHmr)
            {
                await EvaluateAsync("window.joltBrowserSentinel = true");
                var path = args[2];
                var original = await File.ReadAllTextAsync(path, token);
                try
                {
                    var updated = original.Replace("<h1>Jolt counter</h1>", "<h1>Updated counter</h1>", StringComparison.Ordinal);
                    if (updated == original)
                        throw new InvalidOperationException($"Did not find the counter title in {path}.");
                    await File.WriteAllTextAsync(path, updated, token);
                    await CheckAsync("proxy HMR updated the heading and kept the counter",
                        "document.querySelector('h1')?.textContent === 'Updated counter' && document.querySelector('#count')?.textContent === '1' && window.joltBrowserSentinel === true");
                }
                finally
                {
                    await File.WriteAllTextAsync(path, original);
                }
            }
            await CloseBrowserAsync();
            return;
        }
        if (moduleValue is not null)
        {
            var expected = moduleValue[moduleValueOption.Length..];
            const string renderedValue = "document.querySelector('#module-value')?.textContent";
            await CheckAsync("the packaged C# library output", $"{renderedValue} !== undefined");
            var result = await SendAsync("Runtime.evaluate", new { expression = renderedValue, returnByValue = true });
            var actual = result.GetProperty("result").GetProperty("value").GetString();
            if (!string.Equals(actual, expected, StringComparison.Ordinal))
                throw new InvalidOperationException($"Expected packaged module value '{expected}', observed '{actual}'.");
            Console.WriteLine($"Chromium rendered packaged module value '{actual}'.");
            await CloseBrowserAsync();
            return;
        }
        if (mixed || ssr || monaco || moduleHandoff)
        {
            if (ssrBenchmark)
                await BenchmarkSsrAsync();
            else if (ssr)
                await VerifySsrAsync();
            else if (monaco)
            {
                await CheckAsync("the generated Monaco editor, model, worker, theme, and markers",
                    """
                    (() => {
                        if (document.body?.dataset.monacoError) throw new Error(document.body.dataset.monacoError);
                        return document.body?.dataset.monacoVerified === 'true';
                    })()
                    """);
                Console.WriteLine("Verified Monaco model identity, string ID and detachment, worker, disposal, theme and marker owners in Chromium.");
            }
            else if (moduleHandoff)
                await VerifyModuleHandoffAsync(args.Length == 3 ? args[2] : null);
            else
                await VerifyMixedAsync(args.Length == 3 ? args[2] : null);
            await CloseBrowserAsync();
            return;
        }
        if (correlated)
        {
            await CheckAsync("both old DTO values", """
                document.querySelector('#left')?.textContent === 'before' &&
                document.querySelector('#right')?.textContent === 'before'
                """);
            await EvaluateAsync("""
                window.dtoUpdates = [];
                new MutationObserver(() => {
                    const pair = ['#left', '#right'].map(id => document.querySelector(id)?.textContent).join(':');
                    if (pair !== window.dtoUpdates.at(-1)) window.dtoUpdates.push(pair);
                }).observe(document.querySelector('#app'), { childList: true, subtree: true, characterData: true });
                true
                """);
            File.Move(args[2] + ".next", args[2], overwrite: true);
            await CheckAsync("both new DTO values", """
                document.querySelector('#left')?.textContent === 'after' &&
                document.querySelector('#right')?.textContent === 'after'
                """);
            await Task.Delay(750, token);
            await CheckAsync("one coherent DTO update without reloading", "JSON.stringify(window.dtoUpdates) === '[\"after:after\"]'");
            Console.WriteLine("Verified a single correlated DTO update in Chromium.");
            await File.WriteAllTextAsync(args[2] + ".browser-ready", "", token);
            await RetryAsync(() => Task.FromResult(File.Exists(args[2] + ".browser-recover")), token);
            await CheckAsync("the SFC and linked npm call on the original page after declaration recovery", """
                document.querySelector('#card')?.textContent.trim() === 'Recovered Hello' &&
                document.querySelector('#linked')?.textContent === 'external declaration' &&
                document.querySelector('#left')?.textContent === 'after' &&
                document.querySelector('#right')?.textContent === 'after' &&
                window.dtoUpdates !== undefined
                """);
            return;
        }
        if (editableOnly)
        {
            await VerifyEditableListAsync();
            return;
        }
        await CheckAsync("the initial counter", "document.querySelector('#count')?.textContent === '0' && document.querySelector('#increment')?.textContent.trim() === 'Increment one' && document.querySelector('#scope')?.textContent === 'closed'");
        if (proxyHmr)
            await CheckAsync("Vite connected to ASP.NET's WebSocket endpoint", "window.proxyHmrConnected === true");
        await EvaluateAsync("document.querySelector('#increment').click(); window.joltBrowserSentinel = true");
        await RetryAsync(() => EvaluateAsync("document.querySelector('#count')?.textContent === '1' && document.querySelector('#indicator [data-state]')?.dataset.state === 'open' && document.querySelector('#scope')?.textContent === 'open'"), token);
        Console.WriteLine("Counter click updated the parent count and Reka UI state.");
        if (features)
        {
            await RetryAsync(() => EvaluateAsync("""
                getComputedStyle(document.querySelector('#count')).color === 'rgb(190, 20, 30)' &&
                getComputedStyle(document.querySelector('#increment')).color === 'rgb(20, 30, 190)' &&
                getComputedStyle(document.querySelector('#increment')).borderTopWidth === '7px' &&
                getComputedStyle(document.querySelector('.slotted')).fontWeight === '700' &&
                [...document.querySelectorAll('.typed-item')].map(item => item.textContent).join(',') === '3,4' &&
                window.joltNamedExports?.join(',') === '1,3'
                """), token);
            await RetryAsync(() => EvaluateAsync("document.querySelector('.npm-item')?.textContent === '3'"), token);
            Console.WriteLine("Component colors are isolated, parent and slot styles apply, and both typed lists rendered their scoped slot.");
            await VerifyEditableListAsync();
            await VerifyTypedTableAsync();
            await VerifyTableEditorAsync();
            await VerifyTableBindingsAsync();
            await VerifyNativeFormAsync();
            await VerifyNpmCounterAsync();
        }

        if (args.Length == 3)
        {
            var file = args[2];
            var original = await File.ReadAllTextAsync(file, token);
            try
            {
                var changed = original.Replace("> one</small>", "> edited</small>", StringComparison.Ordinal);
                if (features)
                    changed = changed.Replace("20, 30, 190", "20, 150, 50", StringComparison.Ordinal);
                await File.WriteAllTextAsync(file, changed, token);
                await RetryAsync(() => EvaluateAsync("document.querySelector('#child-label')?.textContent.trim() === 'edited' && document.querySelector('#count')?.textContent === '1' && window.joltBrowserSentinel === true"), token);
                if (features)
                {
                    await RetryAsync(() => EvaluateAsync("getComputedStyle(document.querySelector('#increment')).color === 'rgb(20, 150, 50)'"), token);
                    await VerifyNpmCounterHmrAsync(Path.Combine(Path.GetDirectoryName(file)!, "NpmCounter.jazor"));
                }
                Console.WriteLine("A child .jazor edit updated the button while preserving the parent state and page.");
            }
            finally
            {
                await File.WriteAllTextAsync(file, original);
            }
        }
        if (proxyHmr)
            await CloseBrowserAsync();

        async Task CloseBrowserAsync()
        {
            // Let Chrome release its profile databases before deleting the profile.
            await socket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(new { id = ++nextId, method = "Browser.close" }),
                WebSocketMessageType.Text, endOfMessage: true, token);
            using var shutdown = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            try
            {
                await browser.WaitForExitAsync(shutdown.Token);
            }
            catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
            {
                // The common cleanup stops a browser that does not respond.
            }
        }

        async Task VerifyModuleHandoffAsync(string? directory)
        {
            if (directory is not null)
            {
                await CheckAsync("the initial value from the referenced DLL",
                    "document.querySelector('#module-value')?.textContent.trim() === 'from DLL v1'");
                await EvaluateAsync("window.jazorModuleHandoffPage = true");
                var source = Path.Combine(directory, "Library", "Greeting.cs");
                var initial = await File.ReadAllTextAsync(source, token);
                var updated = initial.Replace("from DLL v1", "from DLL v2", StringComparison.Ordinal);
                if (updated == initial)
                    throw new InvalidOperationException($"Expected the original DLL greeting in {source}.");
                var staging = source + "." + Guid.NewGuid().ToString("N") + ".tmp";
                try
                {
                    await File.WriteAllTextAsync(staging, updated, token);
                    File.Move(staging, source, overwrite: true);
                }
                finally
                {
                    File.Delete(staging);
                }

            }

            await CheckAsync("the updated value from the referenced DLL",
                "document.querySelector('#module-value')?.textContent.trim() === 'from DLL v2'");
            if (directory is not null && !await EvaluateAsync("window.jazorModuleHandoffPage === true"))
                throw new InvalidOperationException("The DLL edit caused a full navigation instead of updating the open page.");
            Console.WriteLine(directory is null
                ? "Verified the referenced DLL module in the production page."
                : "The open page showed the rebuilt DLL module.");
        }

        async Task VerifyMixedAsync(string? directory)
        {
            await CheckAsync("the mixed component's initial model, typed prop and scoped style", """
                document.querySelector('#draft')?.value === 'First draft' &&
                document.querySelector('#editor-icon')?.naturalWidth > 0 &&
                document.querySelector('#draft').maxLength === 24 &&
                document.querySelector('#saved').textContent === 'First draft' &&
                getComputedStyle(document.querySelector('#draft').closest('label')).color === 'rgb(20, 30, 190)' &&
                getComputedStyle(document.querySelector('#outside')).color !== 'rgb(20, 30, 190)' &&
                getComputedStyle(document.querySelector('#editor-title')).color === 'rgb(160, 30, 30)'
                """);
            await EvaluateAsync("""
                window.mixedInput = document.querySelector('#draft');
                window.mixedInput.focus();
                window.mixedInput.select()
                """);
            await SendAsync("Input.insertText", new { text = "Unsaved text" });
            const string draftPreserved = """
                document.activeElement === window.mixedInput &&
                document.querySelector('#draft') === window.mixedInput &&
                window.mixedInput.value === 'Unsaved text' &&
                document.querySelector('#saved').textContent === 'First draft'
                """;
            await CheckAsync("the actual typing event left the draft unsaved", draftPreserved);
            if (mixedRounds != 0)
                await BenchmarkTemplatesAsync(directory!);
            else if (directory is not null)
            {
                var vue = Path.Combine(directory, "editors", "NameInput.vue");
                var jolt = Path.Combine(directory, "editors", "MixedEditor.jazor");
                await EditAsync(vue, ">Name</span>", ">Edited name</span>", "SFC template HMR",
                    "document.querySelector('#editor-caption')?.textContent === 'Edited name'");
                await EditAsync(vue, "20, 30, 190", "20, 150, 50", "SFC scoped CSS HMR", """
                    getComputedStyle(document.querySelector('#draft').closest('label')).color === 'rgb(20, 150, 50)' &&
                    getComputedStyle(document.querySelector('#outside')).color !== 'rgb(20, 150, 50)'
                    """);
                await EditAsync(jolt, ">Draft editor</h1>", ">Edited draft</h1>", "Jolt template HMR",
                    "document.querySelector('#editor-title')?.textContent === 'Edited draft'");
                await EditAsync(jolt, "160, 30, 30", "160, 20, 130", "Jolt scoped CSS HMR",
                    "getComputedStyle(document.querySelector('#editor-title')).color === 'rgb(160, 20, 130)'");
            }

            await EvaluateAsync("window.mixedInput.blur()");
            await CheckAsync("the Vue child's update crossed the Jolt component into the Vue parent's model",
                "document.querySelector('#saved')?.textContent === 'Unsaved text'");
            var verification = mixedRounds != 0 ? " and template HMR" : directory is null ? "" : " and template/CSS HMR";
            Console.WriteLine($"Verified mixed-source browser interaction{verification}.");

            async Task BenchmarkTemplatesAsync(string sourceDirectory)
            {
                var jolt = Path.Combine(sourceDirectory, "editors", "MixedEditor.jazor");
                var vue = Path.Combine(sourceDirectory, "editors", "NameInput.vue");
                var joltSource = await File.ReadAllTextAsync(jolt, token);
                var vueSource = await File.ReadAllTextAsync(vue, token);
                var joltMs = new double[mixedRounds];
                var vueMs = new double[mixedRounds];
                for (var round = 0; round <= mixedRounds; round++)
                {
                    // Use distinct text for every save so an old HMR update cannot satisfy a later sample.
                    var title = $"Draft editor {round}";
                    var caption = $"Name {round}";
                    var titleMs = await SaveAndObserveAsync(jolt, joltSource, "Draft editor", title, "#editor-title");
                    var captionMs = await SaveAndObserveAsync(vue, vueSource, ">Name</span>", $">{caption}</span>", "#editor-caption", caption);
                    if (round == 0)
                    {
                        Report("mixed-template-first-edit", ".jazor", [titleMs]);
                        Report("mixed-template-first-edit", ".vue", [captionMs]);
                    }
                    else
                    {
                        joltMs[round - 1] = titleMs;
                        vueMs[round - 1] = captionMs;
                    }
                }

                Report("mixed-template-warm-edit", ".jazor", joltMs);
                Report("mixed-template-warm-edit", ".vue", vueMs);

                async Task<double> SaveAndObserveAsync(string path, string source, string previous, string replacement,
                    string selector, string? expected = null)
                {
                    if (!source.Contains(previous, StringComparison.Ordinal))
                        throw new InvalidOperationException($"Missing benchmark edit in {path}.");
                    var changed = source.Replace(previous, replacement, StringComparison.Ordinal);
                    // The observer runs in Chromium so the existing 100 ms smoke-test polling does not enter the timing.
                    await EvaluateAsync($$"""
                        window.mixedEdit = new Promise(resolve => {
                            const observer = new MutationObserver(() => {
                                if (document.querySelector({{JsonSerializer.Serialize(selector)}})?.textContent !==
                                    {{JsonSerializer.Serialize(expected ?? replacement)}}) return;
                                observer.disconnect();
                                clearTimeout(timeout);
                                resolve({{draftPreserved}});
                            });
                            const timeout = setTimeout(() => {
                                observer.disconnect();
                                resolve(false);
                            }, 10000);
                            observer.observe(document, { childList: true, characterData: true, subtree: true });
                        });
                        """);
                    var started = Stopwatch.GetTimestamp();
                    await File.WriteAllTextAsync(path, changed, token);
                    var result = await SendAsync("Runtime.evaluate", new
                    {
                        expression = "window.mixedEdit",
                        awaitPromise = true,
                        returnByValue = true,
                    });
                    var elapsed = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
                    if (!result.TryGetProperty("result", out var value) ||
                        !value.TryGetProperty("value", out var preserved) || preserved.ValueKind != JsonValueKind.True)
                        throw new InvalidOperationException($"Expected {selector} to reflect {path} while keeping the focused draft. Browser response: {result}");
                    return elapsed;
                }

                static void Report(string scenario, string extension, double[] samplesMs)
                {
                    var sorted = samplesMs.Order().ToArray();
                    double Percentile(double quantile) => sorted[(int)Math.Ceiling(quantile * sorted.Length) - 1];
                    Console.WriteLine(JsonSerializer.Serialize(new
                    {
                        scenario, extension, samplesMs, p50Ms = Percentile(0.5), p95Ms = Percentile(0.95),
                    }));
                }
            }

            async Task EditAsync(string path, string previous, string replacement, string behavior, string expression)
            {
                var source = await File.ReadAllTextAsync(path, token);
                await File.WriteAllTextAsync(path, source.Replace(previous, replacement, StringComparison.Ordinal), token);
                await CheckAsync(behavior + " applied the authored edit", expression);
                if (!await EvaluateAsync(draftPreserved))
                {
                    var observed = await SendAsync("Runtime.evaluate", new
                    {
                        expression = """
                            ({ focus: document.activeElement?.id,
                                sameNode: document.querySelector('#draft') === window.mixedInput,
                                draft: document.querySelector('#draft')?.value,
                                saved: document.querySelector('#saved')?.textContent })
                            """,
                        returnByValue = true,
                    });
                    throw new InvalidOperationException($"{behavior} changed the focused draft. Observed {observed}.");
                }
            }
        }

        async Task VerifyNpmCounterHmrAsync(string file)
        {
            var original = await File.ReadAllTextAsync(file, token);
            try
            {
                var changed = original.Replace("Increment npm count", "Increment updated npm count", StringComparison.Ordinal);
                await File.WriteAllTextAsync(file, changed, token);
                await CheckAsync("reloading the npm counter reruns setup for each new instance", """
                    document.querySelector('#npm-counter-first .npm-counter-increment')?.textContent === 'Increment updated npm count' &&
                    document.querySelector('#npm-counter-first output')?.textContent === '3' &&
                    document.querySelector('#npm-counter-second output')?.textContent === '10' &&
                    window.joltNpmCounters.first.length === 3 && window.joltNpmCounters.second.length === 2 &&
                    window.joltBrowserSentinel === true && document.querySelector('#count')?.textContent === '1'
                    """);
                await EvaluateAsync("window.joltNpmCounters.first[1].counter.set(40); window.joltNpmCounters.second[0].counter.set(40)");
                await CheckAsync("HMR disposed the superseded watchers", """
                    window.joltNpmCounters.first[1].changes.join(',') === '4' &&
                    window.joltNpmCounters.second[0].changes.join(',') === '12'
                    """);
            }
            finally
            {
                await File.WriteAllTextAsync(file, original);
            }
        }

        async Task VerifyNpmCounterAsync()
        {
            await CheckAsync("published npm exports render two independent counters", """
                [...document.querySelectorAll('.npm-counter-title')].map(node => node.textContent).join(',') === 'npmCounter,npmCounter' &&
                document.querySelector('#npm-counter-first output')?.textContent === '3' &&
                document.querySelector('#npm-counter-second output')?.textContent === '10' &&
                window.joltNpmCounters.first.length === 1 && window.joltNpmCounters.second.length === 1
                """);
            await EvaluateAsync("document.querySelector('#npm-counter-first .npm-counter-increment').click()");
            await CheckAsync("VueUse callback updates the first published ref once", """
                document.querySelector('#npm-counter-first output')?.textContent === '4' &&
                document.querySelector('#npm-counter-second output')?.textContent === '10' &&
                window.joltNpmCounters.first[0].changes.join(',') === '4' &&
                window.joltNpmCounters.second[0].changes.length === 0
                """);
            await EvaluateAsync("document.querySelector('#npm-counter-second .npm-counter-write').click()");
            await CheckAsync("Jolt reads the second published ref and calls its setter", """
                document.querySelector('#npm-counter-first output')?.textContent === '4' &&
                document.querySelector('#npm-counter-second output')?.textContent === '12' &&
                window.joltNpmCounters.second[0].changes.join(',') === '12'
                """);
            await EvaluateAsync("document.querySelector('#npm-counter-toggle').click()");
            await CheckAsync("first npm counter unmounts", "document.querySelector('#npm-counter-first output') === null");
            await EvaluateAsync("window.joltNpmCounters.first[0].counter.set(40)");
            await CheckAsync("the unmounted instance's watcher was disposed", """
                window.joltNpmCounters.first[0].counter.count.value === 40 &&
                window.joltNpmCounters.first[0].changes.join(',') === '4'
                """);
            await EvaluateAsync("document.querySelector('#npm-counter-toggle').click()");
            await CheckAsync("setup runs once for the fresh instance and leaves its sibling intact", """
                document.querySelector('#npm-counter-first output')?.textContent === '3' &&
                document.querySelector('#npm-counter-second output')?.textContent === '12' &&
                window.joltNpmCounters.first.length === 2 && window.joltNpmCounters.second.length === 1 &&
                window.joltNpmCounters.first[0].counter.count !== window.joltNpmCounters.first[1].counter.count
                """);
            await EvaluateAsync("document.querySelector('#npm-counter-first .npm-counter-increment').click()");
            await CheckAsync("only the remounted watch observes the new count", """
                document.querySelector('#npm-counter-first output')?.textContent === '4' &&
                window.joltNpmCounters.first[1].changes.join(',') === '4' &&
                window.joltNpmCounters.first[0].counter.count.value === 40 &&
                window.joltNpmCounters.first[0].changes.join(',') === '4' &&
                window.joltNpmCounters.second[0].changes.join(',') === '12'
                """);
            Console.WriteLine("Published npm calls, instance-local refs, and setup watcher disposal passed.");
        }

        async Task VerifyEditableListAsync()
        {
            await CheckAsync("initial editable list", """
                document.querySelector('#edit-label').value === 'Initial' &&
                document.querySelector('#edit-filter').value === '' &&
                !document.querySelector('#edit-archived').checked &&
                document.querySelector('#edit-toggle button').dataset.state === 'closed'
                """);
            var elapsed = Stopwatch.StartNew();
            await EvaluateAsync("document.querySelector('#edit-label').focus(); document.querySelector('#edit-label').select()");
            await SendAsync("Input.insertText", new { text = "Edited" });
            await CheckAsync("default text binding waits for change", """
                document.querySelector('#edit-label').value === 'Edited' &&
                document.querySelector('#edit-label-state').textContent === 'Initial:0'
                """);
            await EvaluateAsync("document.querySelector('#edit-filter').focus()");
            await CheckAsync("default text binding writes once on blur", "document.querySelector('#edit-label-state').textContent === 'Edited:1'");

            await SendAsync("Input.insertText", new { text = "x" });
            await CheckAsync("input binding writes before blur", "document.querySelector('#edit-filter-state').textContent === 'x:1'");
            await EvaluateAsync("document.querySelector('#edit-filter').select()");
            await SendAsync("Input.imeSetComposition", new { text = "zhong", selectionStart = 5, selectionEnd = 5 });
            await SendAsync("Input.imeSetComposition", new { text = "中", selectionStart = 1, selectionEnd = 1 });
            await CheckAsync("composition updates only the DOM", """
                document.querySelector('#edit-filter').value === '中' &&
                document.querySelector('#edit-filter-state').textContent === 'x:1'
                """);
            await SendAsync("Input.insertText", new { text = "中文" });
            await CheckAsync("composition commits exactly once", "document.querySelector('#edit-filter-state').textContent === '中文:2'");

            await EvaluateAsync("document.querySelector('#edit-archived').click(); document.querySelector('#edit-toggle button').click()");
            await CheckAsync("checkbox and Reka UI write once", """
                document.querySelector('#edit-archived-state').textContent === 'checked:1' &&
                document.querySelector('#edit-open-state').textContent === 'open:1' &&
                document.querySelector('#edit-details [data-state]')?.dataset.state === 'open'
                """);
            await EvaluateAsync("document.querySelector('#edit-archived').click(); document.querySelector('#edit-toggle button').click()");
            await CheckAsync("checkbox and Reka UI write their closed values once", """
                document.querySelector('#edit-archived-state').textContent === 'unchecked:2' &&
                document.querySelector('#edit-open-state').textContent === 'closed:2'
                """);
            await EvaluateAsync("document.querySelector('#edit-reset').click()");
            await CheckAsync("parent changes update the controls without extra writebacks", """
                document.querySelector('#edit-label').value === 'Parent' &&
                document.querySelector('#edit-label-state').textContent === 'Parent:1' &&
                document.querySelector('#edit-filter').value === 'Parent filter' &&
                document.querySelector('#edit-filter-state').textContent === 'Parent filter:2' &&
                document.querySelector('#edit-archived').checked &&
                document.querySelector('#edit-archived-state').textContent === 'checked:2' &&
                document.querySelector('#edit-toggle button').dataset.state === 'open' &&
                document.querySelector('#edit-open-state').textContent === 'open:2'
                """);

            // Keep focus on the field while exercising the link's native default action and bubbling.
            await EvaluateAsync("""
                window.joltFocusedRow = document.querySelector('[data-row="alpha"] input');
                window.joltFocusedRow.focus();
                document.querySelector('#edit-sort').click()
                """);
            await CheckAsync("keyed reorder retains focused DOM and stops the click", """
                document.querySelector('#edit-rows').firstElementChild.dataset.row === 'beta' &&
                document.querySelector('[data-row="alpha"] input') === window.joltFocusedRow &&
                document.activeElement === window.joltFocusedRow &&
                document.querySelector('#edit-sort-state').textContent === '1:0:prevented' &&
                location.hash === ''
                """);
            await EvaluateAsync("document.querySelector('#edit-allow-bubble').click()");
            await EvaluateAsync("document.querySelector('#edit-sort').click()");
            await CheckAsync("dynamic stopPropagation observes the current value", """
                document.querySelector('#edit-rows').firstElementChild.dataset.row === 'alpha' &&
                document.querySelector('#edit-sort-state').textContent === '2:1:prevented' &&
                location.hash === ''
                """);
            elapsed.Stop();
            Console.WriteLine("Text change, IME, checkbox, Reka UI model, parent updates, keyed focus and event modifiers passed.");
            if (editableOnly)
                Console.WriteLine(FormattableString.Invariant($"Editable list interaction and assertions took {elapsed.Elapsed.TotalMilliseconds:F3} ms."));
        }

        async Task VerifyTypedTableAsync()
        {
            await CheckAsync("initial typed table and one key getter call per row", """
                [...document.querySelectorAll('#typed-table .table-status')].map(cell => cell.textContent.trim()).join(',') === 'Empty,Short,Long' &&
                document.querySelectorAll('#typed-table .table-name strong').length === 3 &&
                Number(document.querySelector('#table-key-reads')?.textContent.trim()) >= 3
                """);
            await CheckAsync("attribute spread merges classes and style and allows a later explicit title", """
                (() => {
                    const button = document.querySelector('#table-reverse');
                    return button.classList.contains('table-command') &&
                        button.classList.contains('table-action') &&
                        button.title === 'Reverse table rows' &&
                        getComputedStyle(button).fontWeight === '700';
                })()
                """);
            await EvaluateAsync("""
                window.joltTableRows = [...document.querySelectorAll('#typed-table tr')];
                window.joltTableKeyReads = Number(document.querySelector('#table-key-reads').textContent.trim());
                document.querySelector('#table-reverse').click()
                """);
            await CheckAsync("typed table reuses keyed rows and evaluates each key once during update", """
                (() => {
                    const rows = [...document.querySelectorAll('#typed-table tr')];
                    return rows.length === 3 &&
                        rows.every((row, index) => row === window.joltTableRows[2 - index] && row.dataset.index === String(index)) &&
                        rows.map(row => row.querySelector('.table-name').textContent.trim()).join(',') === 'Four,One,' &&
                        rows.map(row => row.querySelector('.table-status').textContent.trim()).join(',') === 'Long,Short,Empty' &&
                        Number(document.querySelector('#table-key-reads').textContent.trim()) === window.joltTableKeyReads + 3;
                })()
                """);
            Console.WriteLine("The inferred generic table rendered its typed row slot, reused keyed rows and evaluated each row key once per render.");
        }

        async Task CheckAsync(string behavior, string expression)
        {
            try
            {
                await RetryAsync(() => EvaluateAsync(expression), token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                throw new InvalidOperationException($"Browser did not observe {behavior}.");
            }
        }

        async Task VerifyTableEditorAsync()
        {
            await CheckAsync("table editor ref mounts", "document.querySelector('#table-model-state')?.textContent.trim() === 'Bound:0|Legacy:0|mounted'");
            await EvaluateAsync("document.querySelector('#table-focus').click()");
            await CheckAsync("exposed editor focuses the input", "document.activeElement === document.querySelector('#table-model input')");
            await EvaluateAsync("""
                document.querySelector('#table-model .table-editor-next').click();
                document.querySelector('#table-legacy .table-editor-next').click()
                """);
            await CheckAsync("bound and legacy local models each write once", """
                document.querySelector('#table-model-state').textContent.trim() === 'Bound!:1|Legacy!:1|mounted' &&
                document.querySelector('#table-model input').value === 'Bound!' &&
                document.querySelector('#table-legacy input').value === 'Legacy!'
                """);
            await EvaluateAsync("""
                window.joltLegacyInput = document.querySelector('#table-legacy input');
                document.querySelector('#table-toggle-editor').click()
                """);
            await CheckAsync("table editor ref clears on unmount", """
                document.querySelector('#table-model') === null &&
                document.querySelector('#table-legacy input') === window.joltLegacyInput &&
                document.querySelector('#table-model-state').textContent.trim() === 'Bound!:1|Legacy!:1|unmounted'
                """);
            await EvaluateAsync("document.querySelector('#table-toggle-editor').click()");
            await CheckAsync("table editor ref remounts with existing parent state", """
                document.querySelector('#table-model input')?.value === 'Bound!' &&
                document.querySelector('#table-legacy input') === window.joltLegacyInput &&
                document.querySelector('#table-model-state').textContent.trim() === 'Bound!:1|Legacy!:1|mounted'
                """);
            Console.WriteLine("Local model and callback each updated once, and the editor ref supported focus, unmount and remount.");

            await CheckAsync("legacy model prop renders in the plain Vue consumer", """
                document.querySelector('#vue-model-consumer input')?.value === 'Legacy'
                """);
            await EvaluateAsync("""
                window.joltVueEditorInput = document.querySelector('#vue-model-consumer input');
                document.querySelector('#vue-model-consumer .table-editor-next').click();
                """);
            await CheckAsync("legacy callback receives one update", """
                window.joltVueEditorInput.value === 'Legacy!' &&
                document.querySelector('#vue-model-writes').textContent === '1|0'
                """);
            await EvaluateAsync("document.querySelector('#vue-model-toggle').click()");
            await CheckAsync("canonical value replaces the legacy prop on the reused Vue instance", """
                document.querySelector('#vue-model-consumer input') === window.joltVueEditorInput &&
                window.joltVueEditorInput.value === 'Canonical'
                """);
            await EvaluateAsync("document.querySelector('#vue-model-consumer .table-editor-next').click()");
            await CheckAsync("canonical Vue listener receives one update", """
                window.joltVueEditorInput.value === 'Canonical!' &&
                document.querySelector('#vue-model-writes').textContent === '1|1'
                """);
            await EvaluateAsync("document.querySelector('#vue-model-toggle').click()");
            await CheckAsync("legacy value replaces the canonical prop on the reused Vue instance", """
                document.querySelector('#vue-model-consumer input') === window.joltVueEditorInput &&
                window.joltVueEditorInput.value === 'Legacy!'
                """);
            await EvaluateAsync("document.querySelector('#vue-model-consumer .table-editor-next').click()");
            await CheckAsync("restored legacy callback receives one update", """
                window.joltVueEditorInput.value === 'Legacy!!' &&
                document.querySelector('#vue-model-writes').textContent === '2|1'
                """);
        }

        async Task VerifyTableBindingsAsync()
        {
            await EvaluateAsync("document.querySelector('#table-normalized').focus()");
            await SendAsync("Input.insertText", new { text = "raw" });
            await CheckAsync("explicit binding setter normalizes once before blur", """
                document.querySelector('#table-normalized').value === 'raw!' &&
                document.querySelector('#table-text-state').textContent.trim() === 'raw!:1|:0'
                """);
            await EvaluateAsync("document.querySelector('#table-normalized').select()");
            await SendAsync("Input.insertText", new { text = "reject" });
            await CheckAsync("explicit setter rejection restores the getter value", """
                document.querySelector('#table-normalized').value === 'raw!' &&
                document.querySelector('#table-text-state').textContent.trim() === 'raw!:1|:0'
                """);
            await EvaluateAsync("document.querySelector('#table-saved').focus()");
            await SendAsync("Input.insertText", new { text = "Saved" });
            await EvaluateAsync("document.querySelector('#table-saved').blur()");
            await CheckAsync("after callback observes the assigned value once", """
                document.querySelector('#table-saved').value === 'Saved' &&
                document.querySelector('#table-text-state').textContent.trim() === 'raw!:1|Saved:1'
                """);
            Console.WriteLine("Explicit bind setter normalized the input and the after callback saw the assigned value exactly once.");
        }

        async Task VerifyNativeFormAsync()
        {
            await CheckAsync("initial native controls reflect authored state", """
                document.querySelector('#radio-first').checked &&
                document.querySelector('#select-single').value === 'red' &&
                [...document.querySelector('#select-multiple').selectedOptions].map(option => option.value).join(',') === 'one' &&
                document.querySelector('#form-number').value === '4'
                """);
            await EvaluateAsync("""
                document.querySelector('#radio-second').click();
                const single = document.querySelector('#select-single');
                single.value = 'blue';
                single.dispatchEvent(new Event('change', { bubbles: true }));
                const multiple = document.querySelector('#select-multiple');
                for (const option of multiple.options) option.selected = option.value !== 'one';
                multiple.dispatchEvent(new Event('change', { bubbles: true }));
                """);
            await CheckAsync("radio and select use their string-valued options", "document.querySelector('#native-form-state').textContent.trim() === 'second|blue|two,three|4'");

            await EvaluateAsync("document.querySelector('#form-number').focus(); document.querySelector('#form-number').select()");
            await SendAsync("Input.insertText", new { text = "-" });
            await CheckAsync("invalid number input produces an empty model", """
                document.querySelector('#form-number').validity.badInput &&
                document.querySelector('#native-form-state').textContent.trim() === 'second|blue|two,three|empty'
                """);
            await SendAsync("Input.insertText", new { text = "5" });
            await CheckAsync("valid number input updates the model", """
                !document.querySelector('#form-number').validity.badInput &&
                document.querySelector('#native-form-state').textContent.trim() === 'second|blue|two,three|-5'
                """);
            await EvaluateAsync("document.querySelector('#form-number').select()");
            await SendAsync("Input.dispatchKeyEvent", new { type = "keyDown", key = "Backspace", code = "Backspace", windowsVirtualKeyCode = 8 });
            await SendAsync("Input.dispatchKeyEvent", new { type = "keyUp", key = "Backspace", code = "Backspace", windowsVirtualKeyCode = 8 });
            await CheckAsync("clearing a nullable number writes an empty model", """
                document.querySelector('#form-number').value === '' &&
                !document.querySelector('#form-number').validity.badInput &&
                document.querySelector('#native-form-state').textContent.trim() === 'second|blue|two,three|empty'
                """);
            Console.WriteLine("Radio, single and multiple selects, and invalid and empty number input passed.");
        }

        async Task BenchmarkSsrAsync()
        {
            const int warmups = 5;
            const int samples = 20;
            var version = await SendAsync("Browser.getVersion", new { });
            Console.WriteLine(version.GetProperty("product").GetString());
            for (var i = -warmups; i < samples; i++)
            {
                await SendAsync("Page.navigate", new { url = origin.ToString() });
                // Page.navigate can return before the new execution context is installed.
                // Poll for a fresh document and mark it consumed before moving to the next navigation.
                await RetryAsync(() => EvaluateAsync("""
                    document.querySelector('#app')?.dataset.hydrated === 'true' &&
                    !window.joltSsrProbe.measured
                    """), token);
                var measured = await SendAsync("Runtime.evaluate", new
                {
                    expression = """
                        new Promise((resolve, reject) => {
                          window.joltSsrProbe.measured = true;
                          const start = performance.getEntriesByName('jolt-hydration-start')[0];
                          const end = performance.getEntriesByName('jolt-hydration-end')[0];
                          const counter = document.querySelector('#counter');
                          if (counter.textContent.trim() !== '1' || counter !== window.joltSsrProbe.counter || !start || !end) {
                            throw new Error('Expected a freshly hydrated original counter and hydration marks.');
                          }
                          const observer = new MutationObserver(() => {
                            if (counter.textContent.trim() === '2') {
                              observer.disconnect();
                              resolve([end.startTime - start.startTime, performance.now() - clicked]);
                            }
                          });
                          observer.observe(counter, { childList: true, subtree: true, characterData: true });
                          setTimeout(() => {
                            observer.disconnect();
                            reject(new Error('First click did not update the counter.'));
                          }, 2000);
                          const clicked = performance.now();
                          counter.click();
                        })
                        """,
                    awaitPromise = true,
                    returnByValue = true
                });
                if (measured.TryGetProperty("exceptionDetails", out _) || browserErrors.Count > 0)
                    throw new InvalidOperationException($"SSR browser benchmark failed. {measured}\n{string.Join(Environment.NewLine, browserErrors)}");
                var timings = measured.GetProperty("result").GetProperty("value");
                if (i >= 0)
                    Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
                        $"SSR_BROWSER {i + 1} {timings[0].GetDouble():F3} {timings[1].GetDouble():F3}"));
            }
        }

        async Task VerifySsrAsync()
        {
            await Check("the editor rendered and hydrated its original button and teleport", """
                document.querySelector('#app[data-hydrated="true"]') !== null &&
                document.querySelector('#route-heading')?.textContent.trim() === 'Editor' &&
                document.querySelector('#route-heading') === window.joltSsrProbe.heading &&
                document.querySelector('#request-name')?.textContent.trim() === 'Alice' &&
                document.querySelector('#prefetch-count')?.textContent.trim() === '1' &&
                document.querySelector('#counter') === window.joltSsrProbe.counter &&
                document.querySelector('#counter')?.textContent.trim() === '1' &&
                document.querySelector('#overlays #teleported-notice') === window.joltSsrProbe.teleport &&
                window.joltSsrProbe.teleport?.isConnected === true &&
                getComputedStyle(document.querySelector('#request-name')).color === 'rgb(160, 30, 30)'
                """);
            await EvaluateAsync("document.querySelector('#counter').click()");
            await Check("the first click invoked its callback exactly once", """
                document.querySelector('#counter')?.textContent.trim() === '2' &&
                document.querySelector('#counter') === window.joltSsrProbe.counter &&
                document.querySelector('#prefetch-count')?.textContent.trim() === '1' &&
                !window.joltSsrProbe.loading
                """);
            await EvaluateAsync("document.querySelector('#home-link').click()");
            await Check("client navigation unmounted the editor and its teleport", """
                location.pathname === '/' &&
                document.querySelector('#route-heading')?.textContent.trim() === 'Overview' &&
                document.querySelector('#cleanup-count')?.textContent.trim() === '1' &&
                !window.joltSsrProbe.heading.isConnected && !window.joltSsrProbe.teleport.isConnected &&
                window.joltSsrProbe.transition
                """);
            await EvaluateAsync("history.back()");
            await Check("browser back remounted the editor with the same request state", """
                location.pathname === '/editor' && location.search === '?name=Alice' &&
                document.querySelector('#route-heading')?.textContent.trim() === 'Editor' &&
                document.querySelector('#request-name')?.textContent.trim() === 'Alice' &&
                document.querySelector('#route-heading') !== window.joltSsrProbe.heading &&
                document.querySelector('#counter')?.textContent.trim() === '2' &&
                document.querySelector('#counter') === window.joltSsrProbe.counter &&
                document.querySelector('#overlays #teleported-notice')?.isConnected === true &&
                document.querySelector('#overlays #teleported-notice') !== window.joltSsrProbe.teleport &&
                document.querySelector('#cleanup-count')?.textContent.trim() === '1' &&
                document.querySelector('#prefetch-count')?.textContent.trim() === '1' &&
                !window.joltSsrProbe.loading
                """);
            await EvaluateAsync("""
                const draft = document.querySelector('#draft');
                draft.value = 'Friday notes';
                draft.dispatchEvent(new Event('input', { bubbles: true }));
                document.querySelector('#draft-view').click()
                """);
            await Check("the Jolt preview reused the model from the Vue input", """
                document.querySelector('#draft-preview')?.textContent === 'Friday notes' &&
                !document.querySelector('#draft')
                """);
            await EvaluateAsync("document.querySelector('#draft-view').click()");
            await Check("the new Vue input received the edited model", """
                document.querySelector('#draft')?.value === 'Friday notes' &&
                !document.querySelector('#draft-preview')
                """);
            await EvaluateAsync("document.querySelector('#draft-link').click()");
            await Check("the generated parameter route mounted within its layout", """
                location.pathname === '/drafts/42' &&
                document.querySelector('#draft-layout #route-heading')?.textContent.trim() === 'Draft details' &&
                document.querySelector('#draft-number')?.textContent.trim() === '43' &&
                document.querySelector('#cleanup-count')?.textContent.trim() === '2'
                """);
            Console.WriteLine("Verified SSR hydration, single callback, styles, teleports, navigation, and editor cleanup in Chromium.");

            async Task Check(string step, string expression)
            {
                using var checkDeadline = CancellationTokenSource.CreateLinkedTokenSource(token);
                checkDeadline.CancelAfter(TimeSpan.FromSeconds(8));
                try
                {
                    await RetryAsync(async () =>
                    {
                        var success = await EvaluateAsync(expression);
                        if (browserErrors.Count > 0)
                            throw new InvalidOperationException("Browser diagnostics during SSR verification: " + string.Join(Environment.NewLine, browserErrors));
                        return success;
                    }, checkDeadline.Token);
                }
                catch (OperationCanceledException) when (checkDeadline.IsCancellationRequested && !token.IsCancellationRequested)
                {
                    var observed = await SendAsync("Runtime.evaluate", new
                    {
                        expression = """
                            ({
                              url: location.href,
                              hydrated: document.querySelector('#app')?.dataset.hydrated,
                              counterReused: document.querySelector('#counter') === window.joltSsrProbe.counter,
                              teleportReused: document.querySelector('#teleported-notice') === window.joltSsrProbe.teleport,
                              headingReused: document.querySelector('#route-heading') === window.joltSsrProbe.heading,
                              transition: window.joltSsrProbe.transition,
                              nameColor: getComputedStyle(document.querySelector('#request-name')).color,
                              text: document.body.innerText
                            })
                            """,
                        returnByValue = true
                    });
                    throw new InvalidOperationException($"Expected {step}. Browser state: {observed}");
                }
            }
        }

        async Task<bool> EvaluateAsync(string expression)
        {
            var response = await SendAsync("Runtime.evaluate", new { expression, returnByValue = true });
            if (monaco && response.TryGetProperty("exceptionDetails", out var details))
                throw new InvalidOperationException(details.GetProperty("exception").GetProperty("description").GetString());
            return response.TryGetProperty("result", out var result) &&
                result.TryGetProperty("value", out var value) && value.ValueKind == JsonValueKind.True;
        }

        async Task<JsonElement> SendAsync(string method, object parameters)
        {
            var id = ++nextId;
            await socket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(new { id, method, @params = parameters }),
                WebSocketMessageType.Text, endOfMessage: true, token);
            var buffer = new byte[16 * 1024];
            while (true)
            {
                using var message = new MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                    if (result.MessageType == WebSocketMessageType.Close)
                        throw new InvalidOperationException("Chromium closed its DevTools connection.");
                    message.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                message.Position = 0;
                using var response = await JsonDocument.ParseAsync(message, cancellationToken: token);
                var envelope = response.RootElement;
                if (!envelope.TryGetProperty("id", out var responseId) || responseId.GetInt32() != id)
                {
                    if (ssr && envelope.TryGetProperty("method", out var eventMethod) &&
                        envelope.TryGetProperty("params", out var details))
                    {
                        if (eventMethod.GetString() == "Runtime.exceptionThrown" ||
                            eventMethod.GetString() == "Runtime.consoleAPICalled" &&
                            details.GetProperty("type").GetString() is "warning" or "error" or "assert")
                            browserErrors.Add(details.ToString());
                    }
                    continue;
                }
                if (envelope.TryGetProperty("error", out var error))
                    throw new InvalidOperationException(error.ToString());
                return envelope.GetProperty("result").Clone();
            }
        }
    }
    finally
    {
        if (!browser.HasExited)
            browser.Kill(entireProcessTree: true);
        await browser.WaitForExitAsync();
    }
}
finally
{
    // Chromium children can briefly hold profile files after the main process exits.
    for (var attempt = 0; ; attempt++)
    {
        try
        {
            profile.Delete(recursive: true);
            break;
        }
        catch (IOException) when (attempt < 30)
        {
            await Task.Delay(100);
        }
    }
}

static async Task RetryAsync(Func<Task<bool>> verify, CancellationToken token)
{
    while (!await verify())
        await Task.Delay(100, token);
}

static string ScriptPath([System.Runtime.CompilerServices.CallerFilePath] string path = "") => path;
