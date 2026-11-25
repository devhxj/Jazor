#:property PublishAot=false
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

var baseline = args.Contains("--baseline", StringComparer.Ordinal);
args = args.Where(argument => argument != "--baseline").ToArray();
if (args.Length is < 2 or > 3)
    throw new ArgumentException("Usage: verify-entry-loading.cs <isolated-publish-directory> <chromium-executable> [evidence-directory] [--baseline]");

var target = Path.GetFullPath(args[0]);
var chromium = Path.GetFullPath(args[1]);
if (!File.Exists(Path.Combine(target, "Jero.Web.dll")) || !File.Exists(chromium))
    throw new ArgumentException("Provide an isolated Jero.Web publish directory and a Chromium executable.");
var work = Directory.CreateTempSubdirectory("jero-entry-loading-").FullName;
var output = Path.GetFullPath(args.Length == 3 ? args[2] : "output/entry-loading");
Directory.CreateDirectory(output);
Console.WriteLine($"Browser profile and host log: {work}");
using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(4));
var stop = timeout.Token;
var hostPort = FreePort();
var chromePort = FreePort();
var hostOrigin = $"http://127.0.0.1:{hostPort}";
var cdpOrigin = $"http://127.0.0.1:{chromePort}";
using var host = new Process
{
    StartInfo = new ProcessStartInfo("dotnet")
    {
        WorkingDirectory = target,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true,
    }
};
host.StartInfo.ArgumentList.Add("Jero.Web.dll");
host.StartInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
host.StartInfo.Environment["DOTNET_ENVIRONMENT"] = "Production";
host.StartInfo.Environment["Kestrel__Endpoints__Http__Url"] = hostOrigin;
var hostLog = new StringBuilder();
void Log(object sender, DataReceivedEventArgs e)
{
    if (e.Data is not null)
        lock (hostLog)
            hostLog.AppendLine(e.Data);
}
host.OutputDataReceived += Log;
host.ErrorDataReceived += Log;
using var chrome = new Process
{
    StartInfo = new ProcessStartInfo(chromium)
    {
        WorkingDirectory = work,
        UseShellExecute = false,
        CreateNoWindow = true,
    }
};
foreach (var argument in new[] { "--headless=new", "--no-first-run", "--no-default-browser-check",
         "--remote-debugging-address=127.0.0.1", "--remote-debugging-port=" + chromePort, "--user-data-dir=" + Path.Combine(work, "chromium-profile"),
         "--disable-background-networking", "about:blank" })
    chrome.StartInfo.ArgumentList.Add(argument);
try
{
    host.Start();
    host.BeginOutputReadLine();
    host.BeginErrorReadLine();
    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
    await WaitFor(async () =>
    {
        if (host.HasExited)
            throw new InvalidOperationException($"Jero.Web exited with {host.ExitCode}.");
        try { return await http.GetStringAsync(hostOrigin + "/plugin", stop); }
        catch (HttpRequestException exception) when (exception.StatusCode is null) { return null; }
    }, stop);
    chrome.Start();
    var targetJson = await WaitFor(async () =>
    {
        if (chrome.HasExited)
            throw new InvalidOperationException($"Chrome exited with {chrome.ExitCode}.");
        try { return await http.GetStringAsync(cdpOrigin + "/json", stop); }
        catch (HttpRequestException) { return null; }
    }, stop);
    using var targets = JsonDocument.Parse(targetJson);
    var websocket = targets.RootElement.EnumerateArray()
        .Single(item => item.GetProperty("type").GetString() == "page")
        .GetProperty("webSocketDebuggerUrl").GetString()!;
    using var browser = new ClientWebSocket();
    await browser.ConnectAsync(new Uri(websocket), stop);
    var paused = new Queue<(string Id, string Url)>();
    var sockets = new List<string>();
    var sequenceId = 0;
    var exceptions = new List<string>();
    async Task<JsonElement> Send(string method, object? arguments = null)
    {
        var sequence = ++sequenceId;
        await browser.SendAsync(JsonSerializer.SerializeToUtf8Bytes(new { id = sequence, method, @params = arguments ?? new { } }),
            WebSocketMessageType.Text, true, stop);
        while (true)
        {
            using var message = new MemoryStream();
            var buffer = new byte[16384];
            ValueWebSocketReceiveResult part;
            do
            {
                part = await browser.ReceiveAsync(buffer.AsMemory(), stop);
                message.Write(buffer, 0, part.Count);
            } while (!part.EndOfMessage);
            using var response = JsonDocument.Parse(message.ToArray());
            var root = response.RootElement;
            if (root.TryGetProperty("method", out var name))
            {
                var data = root.GetProperty("params");
                switch (name.GetString())
                {
                    case "Runtime.exceptionThrown":
                        exceptions.Add(data.GetProperty("exceptionDetails").GetRawText());
                        break;
                    case "Fetch.requestPaused":
                        paused.Enqueue((data.GetProperty("requestId").GetString()!,
                            data.GetProperty("request").GetProperty("url").GetString()!));
                        break;
                    case "Network.webSocketCreated":
                        sockets.Add(data.GetProperty("url").GetString()!);
                        break;
                }
            }
            if (!root.TryGetProperty("id", out var responseId) || responseId.GetInt32() != sequence)
                continue;
            if (root.TryGetProperty("error", out var error))
                throw new InvalidOperationException($"{method} failed: {error}");
            return root.GetProperty("result").Clone();
        }
    }

    await Send("Page.enable");
    await Send("Runtime.enable");
    await Send("Page.addScriptToEvaluateOnNewDocument", new { source = """
        if (location.pathname === '/login')
            localStorage.removeItem('jero.jazor.access-token');
        else
            localStorage.setItem('jero.jazor.access-token', 'mock-lazy-loading-test');
        performance.setResourceTimingBufferSize(5000);
        window.__jeroRequests = [];
        const originalFetch = globalThis.fetch;
        globalThis.fetch = (input, init) => {
            const url = new URL(input instanceof Request ? input.url : String(input), location.href);
            if (!url.pathname.includes('/api/') && !url.pathname.includes('/hubs/'))
                return originalFetch(input, init);
            const path = url.pathname.toLowerCase();
            window.__jeroRequests.push([path, init?.body, url.search]);
            if (path.includes('/negotiate'))
                return Promise.resolve(new Response('{"negotiateVersion":1,"connectionId":"fixture","connectionToken":"fixture","availableTransports":[{"transport":"WebSockets","transferFormats":["Text"]}]}', { headers: { 'Content-Type': 'application/json' } }));
            let result = [];
            if (path.endsWith('/sysauth/userinfo')) result = {
                id: '1', account: 'smoke', realName: 'Lazy loading smoke', accountType: 777,
                tenantId: '1', currentTenantId: '1', homePath: '/plugin', buttons: ['sysPlugin:add', 'sysPrint:add', 'sysFile:page'], roleIds: []
            };
            else if (path.endsWith('/sysmenu/loginmenutree')) result = [
                { id: '1', path: '/plugin', type: 2, meta: { title: '插件管理' }, children: [] },
                { id: '2', path: '/print', type: 2, meta: { title: '打印模板' }, children: [] },
                { id: '3', path: '/file', type: 2, meta: { title: '文件管理' }, children: [] }
            ];
            else if (path.endsWith('/sysmenu/ownbtnpermlist')) result = ['sysPlugin:add', 'sysPrint:add', 'sysFile:page'];
            else if (path.endsWith('/sysconfig/sysinfo')) result = { title: 'Lazy loading smoke' };
            else if (path.endsWith('/sysplugin/page')) result = { total: 1, items: [
                { id: '101', tenantId: '1', name: 'Smoke plugin', assemblyName: 'SmokePlugin', status: 1 } ] };
            else if (path.endsWith('/sysfile/page')) result = { total: 2, items: [
                { id: '201', fileName: 'Smoke document', suffix: '.docx', sizeKb: '1', sizeInfo: '1 KB', isPublic: false },
                { id: '202', fileName: 'Smoke workbook', suffix: '.xlsx', sizeKb: '1', sizeInfo: '1 KB', isPublic: false }
            ] };
            else if (path.includes('/sysfile/preview/'))
                return Promise.resolve(new Response(Uint8Array.from(atob(path.endsWith('/201') ? __DOCX__ : __XLSX__), c => c.charCodeAt(0)), {
                    headers: { 'Content-Type': path.endsWith('/201') ? 'application/vnd.openxmlformats-officedocument.wordprocessingml.document' : 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' }
                }));
            else if (path.endsWith('/sysprint/page')) result = { total: 0, items: [] };
            return Promise.resolve(new Response(JSON.stringify({ code: 200, result }), { headers: { 'Content-Type': 'application/json' } }));
        };
        class MockSocket extends EventTarget {
            readyState = 0;
            constructor() {
                super();
                queueMicrotask(() => { this.readyState = 1; const event = new Event('open'); this.dispatchEvent(event); this.onopen?.(event); });
            }
            send(message) {
                if (!String(message).includes('"protocol"')) return;
                queueMicrotask(() => { const event = new MessageEvent('message', {data: '{}\u001e'}); this.dispatchEvent(event); this.onmessage?.(event); });
            }
            close() { this.readyState = 3; const event = new Event('close'); this.dispatchEvent(event); this.onclose?.(event); }
        }
        MockSocket.OPEN = 1;
        window.WebSocket = MockSocket;
        """.Replace("__DOCX__", JsonSerializer.Serialize(OfficeFixture(true)), StringComparison.Ordinal)
        .Replace("__XLSX__", JsonSerializer.Serialize(OfficeFixture(false)), StringComparison.Ordinal) });
    await Send("Network.enable");
    await Send("Network.setCacheDisabled", new { cacheDisabled = true });
    var version = await Send("Browser.getVersion");
    var observations = new List<object>();
    var checks = new List<string>();

    async Task<JsonElement> Evaluate(string expression)
    {
        var result = await Send("Runtime.evaluate", new { expression, returnByValue = true });
        if (result.TryGetProperty("exceptionDetails", out var details))
            throw new InvalidOperationException($"Evaluation failed: {expression}. {details}");
        return result.GetProperty("result").TryGetProperty("value", out var value) ? value : default;
    }

    async Task Expect(string expression)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(stop);
        deadline.CancelAfter(TimeSpan.FromSeconds(15));
        try
        {
            await WaitFor(async () => (await Evaluate(expression)).ValueKind == JsonValueKind.True ? "ok" : null, deadline.Token);
        }
        catch (OperationCanceledException) when (!stop.IsCancellationRequested)
        {
            var state = await Evaluate("({ path: location.pathname, text: document.body?.innerText?.slice(-1800), requests: window.__jeroRequests })");
            throw new InvalidOperationException($"Browser check timed out: {expression}. Page: {state}");
        }
    }

    async Task Navigate(string path, string readyMarker)
    {
        var marker = Guid.NewGuid().ToString("N");
        await Send("Page.navigate", new { url = $"{hostOrigin}{path}?smoke={marker}" });
        await Expect($"location.search === '?smoke={marker}' && document.body?.innerText.includes({JsonSerializer.Serialize(readyMarker)})");
    }

    async Task Click(string selector, string label)
    {
        var target = $"[...document.querySelectorAll({JsonSerializer.Serialize(selector)})].find(element => element.getClientRects().length && element.textContent.trim() === {JsonSerializer.Serialize(label)})";
        await Expect($"Boolean({target})");
        await Evaluate($"{target}.click()");
    }

    const string editorPath = "*/assets/editor.api-*.js";
    const string printPath = "*/assets/vue-plugin-hiprint-*.js";
    const string initialHeavy = "performance.getEntriesByType('resource').filter(entry => /\\/(?:editor\\.api-|register-|vue-plugin-hiprint-|stackblur-)[^/]+\\.js$/.test(new URL(entry.name).pathname)).map(entry => entry.name)";
    const string editorSelector = "#jero-plugin-monaco .monaco-editor";
    const string printSelector = "#jero-print-template .hiprint-printPanel";
    const string pluginReady = "SmokePlugin";
    const string printReady = "管理租户打印模板";
    const string editorLoading = "正在加载代码编辑器";
    const string printLoading = "正在初始化打印设计器";

    string Contains(string text) => $"document.body.innerText.includes({JsonSerializer.Serialize(text)})";
    string One(string selector) => $"document.querySelectorAll({JsonSerializer.Serialize(selector)}).length === 1";
    string Empty(string selector) => $"document.querySelector({JsonSerializer.Serialize(selector)}) === null";

    async Task OpenEditor()
    {
        await Click("button", "新增并安装");
        await Click("[role=tab]", "C# 代码");
    }

    Task OpenPrint() => Click("button", "新增");

    async Task CloseDialog()
    {
        await Click("[role=dialog] button", "取消");
        await Expect("![...document.querySelectorAll('[role=dialog]')].some(element => element.getClientRects().length)");
    }

    async Task EnableFetch(string pattern) =>
        await Send("Fetch.enable", new { patterns = new[] { new { urlPattern = pattern, requestStage = "Request" } } });

    async Task Paused()
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(stop);
        deadline.CancelAfter(TimeSpan.FromSeconds(15));
        try
        {
            await WaitFor(async () =>
            {
                await Evaluate("true");
                return paused.Count != 0 ? "ok" : null;
            }, deadline.Token);
        }
        catch (OperationCanceledException) when (!stop.IsCancellationRequested)
        {
            var state = await Evaluate("({ text: document.body.innerText.slice(-1800), assets: performance.getEntriesByType('resource').map(entry => new URL(entry.name).pathname).filter(path => /editor|hiprint|register/.test(path)) })");
            throw new InvalidOperationException($"Expected an intercepted lazy resource. Page: {state}");
        }
    }

    async Task MeasurePage(string path, string readyMarker)
    {
        sockets.Clear();
        await Navigate(path, readyMarker);
        await Expect("document.readyState === 'complete'");
        var captured = await Evaluate("""
            ({
                Resources: performance.getEntriesByType('resource')
                    .filter(entry => /\.(?:m?js|css)$/i.test(new URL(entry.name).pathname))
                    .map(entry => ({ Path: new URL(entry.name).pathname, DecodedBodySize: entry.decodedBodySize })),
                Requests: window.__jeroRequests
            })
            """);
        var resources = captured.GetProperty("Resources");
        const string shellResources = @"(?:^|/)(?:shell-state|ShellFrame|ShellHeader|realtime-state|signalr)[^/]*\.(?:js|css)$";
        const string officeResources = @"(?:^|/)vue-office-(?:docx|excel)-[^/]*\.(?:js|css)$";
        if (!baseline)
        {
            if (path == "/login")
            {
                if (sockets.Count != 0 || resources.EnumerateArray().Any(resource => System.Text.RegularExpressions.Regex.IsMatch(resource.GetProperty("Path").GetString()!, shellResources, System.Text.RegularExpressions.RegexOptions.IgnoreCase)) ||
                    captured.GetProperty("Requests").EnumerateArray().Any(request => request[0].GetString()!.Contains("/hubs/")))
                    throw new InvalidOperationException("The anonymous login loaded shell or realtime resources.");
            }
            if (path == "/file" && resources.EnumerateArray().Any(resource => System.Text.RegularExpressions.Regex.IsMatch(resource.GetProperty("Path").GetString()!, officeResources, System.Text.RegularExpressions.RegexOptions.IgnoreCase)))
                throw new InvalidOperationException("The file list loaded an Office viewer before preview.");
        }
        observations.Add(new
        {
            Scenario = "cold-entry",
            Browser = version.GetProperty("product").GetString(),
            Path = path,
            CacheDisabled = true,
            ResourceCount = resources.GetArrayLength(),
            DecodedBodySize = resources.EnumerateArray().Sum(resource => resource.GetProperty("DecodedBodySize").GetInt64()),
            Resources = resources,
            Requests = captured.GetProperty("Requests"),
        });
    }

    await MeasurePage("/login", "登录 Jero");
    await MeasurePage("/file", "Smoke document");
    await File.WriteAllTextAsync(Path.Combine(output, "cold-entry.json"),
        JsonSerializer.Serialize(observations, new JsonSerializerOptions { WriteIndented = true }), stop);
    if (baseline)
        return;

    async Task CheckOfficeLoad(string fileName, string chunk)
    {
        await Navigate("/file", fileName);
        await EnableFetch($"*/assets/{chunk}-*.js");
        var row = $"[...document.querySelectorAll('tr')].find(row => row.innerText.includes({JsonSerializer.Serialize(fileName)}))";
        var button = $"[...({row})?.querySelectorAll('button') ?? []].find(button => button.innerText.trim() === '预览')";
        await Expect($"Boolean({button})");
        await Evaluate($"({button}).click()");
        await Paused();
        await Expect(Contains("正在加载预览"));
        var request = paused.Dequeue();
        await Send("Fetch.failRequest", new { requestId = request.Id, errorReason = "Failed" });
        await Expect(Contains("加载预览组件失败"));
        await Send("Fetch.disable");
        paused.Clear();
        await Evaluate("document.querySelector('[role=dialog] .el-dialog__headerbtn').click()");
        await Expect(Empty(".file-preview-shell"));
    }

    await CheckOfficeLoad("Smoke document", "vue-office-docx");
    await CheckOfficeLoad("Smoke workbook", "vue-office-excel");
    checks.Add("DOCX and XLSX rows request their selected viewer; each reports a failed import and closes");

    async Task CheckOfficeSuccess(string fileName, string oppositeChunk, string rendered)
    {
        await Navigate("/file", fileName);
        var row = $"[...document.querySelectorAll('tr')].find(row => row.innerText.includes({JsonSerializer.Serialize(fileName)}))";
        var button = $"[...({row})?.querySelectorAll('button') ?? []].find(button => button.innerText.trim() === '预览')";
        var opposite = JsonSerializer.Serialize(oppositeChunk);
        for (var attempt = 0; attempt < 2; attempt++)
        {
            await Expect($"Boolean({button})");
            await Evaluate($"({button}).click()");
            await Expect(rendered);
            await Expect($"!performance.getEntriesByType('resource').some(entry => entry.name.includes({opposite}))");
            await Evaluate("document.querySelector('[role=dialog] .el-dialog__headerbtn').click()");
            await Expect(Empty(".file-preview-shell"));
        }
    }

    await CheckOfficeSuccess("Smoke document", "vue-office-excel",
        "document.querySelector('.file-preview-shell')?.innerText.includes('DOCX fixture paragraph') === true");
    await CheckOfficeSuccess("Smoke workbook", "vue-office-docx",
        "Boolean(document.querySelector('.file-preview-shell canvas')) && document.querySelector('.file-preview-shell')?.innerText.includes('Smoke worksheet')");
    checks.Add("DOCX text and XLSX worksheet render on two separate opens without requesting the opposite renderer");

    for (var sample = 1; sample <= 3; sample++)
    {
        await Navigate("/plugin", pluginReady);
        await Expect($"({initialHeavy}).length === 0");
        var captured = await Evaluate("""
            ({
                ReadyMilliseconds: performance.now(),
                Resources: performance.getEntriesByType('resource')
                  .filter(entry => new URL(entry.name).pathname.includes('/assets/'))
                  .map(entry => ({ Path: new URL(entry.name).pathname, DurationMilliseconds: entry.duration,
                    DecodedBodySize: entry.decodedBodySize, EncodedBodySize: entry.encodedBodySize, TransferSize: entry.transferSize }))
            })
            """);
        observations.Add(new
        {
            Scenario = "first-screen",
            Sample = sample,
            Browser = version.GetProperty("product").GetString(),
            Url = (await Evaluate("location.href")).GetString(),
            CacheDisabled = true,
            ReadyMilliseconds = captured.GetProperty("ReadyMilliseconds"),
            Resources = captured.GetProperty("Resources"),
        });
    }
    checks.Add("first screen excludes Monaco, C# language registration, Hiprint and stackblur");

    async Task CheckDelayedMount(string pattern, Func<Task> open, string loading, string selector)
    {
        await EnableFetch(pattern);
        await open();
        await Paused();
        await Expect(Contains(loading));
        await CloseDialog();

        var heldUrl = JsonSerializer.Serialize(paused.Peek().Url);
        // Release every paused request. Monaco can request two editor API chunks in parallel.
        await Send("Fetch.disable");
        paused.Clear();
        await Expect($"performance.getEntriesByName({heldUrl}).length > 0");
        await Expect(Empty(selector));
        await open();
        await Expect(One(selector));
        await CloseDialog();
        await Expect(Empty(selector));
    }

    async Task CheckFailure(string route, string ready, string pattern, Func<Task> open, string loading, string error, string selector)
    {
        await Navigate(route, ready);
        await EnableFetch(pattern);
        await open();
        await Paused();
        while (paused.TryDequeue(out var request))
            await Send("Fetch.failRequest", new { requestId = request.Id, errorReason = "Failed" });
        await Expect($"{Contains(error)} && !{Contains(loading)} && {Empty(selector)}");
        await Send("Fetch.disable");
        paused.Clear();
        await CloseDialog();
    }

    await CheckDelayedMount(editorPath, OpenEditor, editorLoading, editorSelector);
    checks.Add("held Monaco load respects dialog close and mounts once on reopen");
    await CheckFailure("/plugin", pluginReady, editorPath, OpenEditor, editorLoading, "代码编辑器加载失败", editorSelector);
    checks.Add("failed Monaco load clears loading and displays an error");

    await Navigate("/print", printReady);
    await Expect($"({initialHeavy}).length === 0");
    await CheckDelayedMount(printPath, OpenPrint, printLoading, printSelector);
    checks.Add("print list excludes Hiprint; held designer load respects close and mounts once on reopen");
    await CheckFailure("/print", printReady, printPath, OpenPrint, printLoading, "打印设计器初始化失败", printSelector);
    checks.Add("failed Hiprint load clears loading and displays an error");

    if (exceptions.Count > 0)
        throw new InvalidOperationException(string.Join(Environment.NewLine, exceptions));
    if (sockets.Any(url => new Uri(url) is { Port: 17521, IsLoopback: true }))
        throw new InvalidOperationException("Hiprint opened its default local client WebSocket.");

    await File.WriteAllTextAsync(Path.Combine(output, "boundary.json"), JsonSerializer.Serialize(new
    {
        passed = checks,
        firstScreenObservations = observations,
        webSockets = sockets,
    }, new JsonSerializerOptions { WriteIndented = true }), stop);
    Console.WriteLine($"{checks.Count} browser checks passed. Evidence: {output}");
}
finally
{
    foreach (var process in new[] { chrome, host })
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }
        }
        catch (InvalidOperationException) { }
    File.WriteAllText(Path.Combine(work, "host.log"), hostLog.ToString());
}

static int FreePort()
{
    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    return ((IPEndPoint)listener.LocalEndpoint).Port;
}

static async Task<string> WaitFor(Func<Task<string?>> action, CancellationToken stop)
{
    while (true)
    {
        stop.ThrowIfCancellationRequested();
        if (await action() is { } value)
            return value;
        await Task.Delay(150, stop);
    }
}

static string OfficeFixture(bool document)
{
    const string office = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    const string relationships = "http://schemas.openxmlformats.org/package/2006/relationships";
    const string types = "http://schemas.openxmlformats.org/package/2006/content-types";
    const string word = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
    const string sheet = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    const string format = "application/vnd.openxmlformats-officedocument";
    var main = document ? "word/document.xml" : "xl/workbook.xml";
    var type = document ? "wordprocessingml.document.main+xml" : "spreadsheetml.sheet.main+xml";
    using var output = new MemoryStream();
    using (var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
    {
        void Add(string name, string contents)
        {
            using var writer = new StreamWriter(zip.CreateEntry(name).Open());
            writer.Write(contents);
        }

        var worksheet = document ? "" : $"""<Override PartName="/xl/worksheets/sheet1.xml" ContentType="{format}.spreadsheetml.worksheet+xml"/>""";
        Add("[Content_Types].xml", $"""<Types xmlns="{types}"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/{main}" ContentType="{format}.{type}"/>{worksheet}</Types>""");
        Add("_rels/.rels", $"""<Relationships xmlns="{relationships}"><Relationship Id="rId1" Type="{office}/officeDocument" Target="{main}"/></Relationships>""");
        if (document)
        {
            Add(main, $"""<w:document xmlns:w="{word}"><w:body><w:p><w:r><w:t>DOCX fixture paragraph</w:t></w:r></w:p></w:body></w:document>""");
        }
        else
        {
            Add(main, $"""<workbook xmlns="{sheet}" xmlns:r="{office}"><sheets><sheet name="Smoke worksheet" sheetId="1" r:id="rId1"/></sheets></workbook>""");
            Add("xl/_rels/workbook.xml.rels", $"""<Relationships xmlns="{relationships}"><Relationship Id="rId1" Type="{office}/worksheet" Target="worksheets/sheet1.xml"/></Relationships>""");
            Add("xl/worksheets/sheet1.xml", $"""<worksheet xmlns="{sheet}"><dimension ref="A1"/><sheetData><row r="1"><c r="A1" t="inlineStr"><is><t>XLSX fixture cell</t></is></c></row></sheetData></worksheet>""");
        }
    }
    return Convert.ToBase64String(output.ToArray());
}
