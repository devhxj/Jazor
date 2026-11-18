#!/usr/bin/env dotnet run
#:property PublishAot=false

using System.Diagnostics;
using System.Net.WebSockets;
using System.Text.Json;

if (args.Length is < 2 or > 3)
    throw new ArgumentException("Usage: verify-browser.cs <browser-executable> <app-origin> [CounterButton.jazor]");

var origin = new Uri(args[1]);
var profile = Directory.CreateTempSubdirectory("jolt-browser-");
try
{
    var start = new ProcessStartInfo(args[0]);
    foreach (var arg in new[] { "--headless", "--no-sandbox", "--disable-gpu", "--remote-debugging-port=0",
        $"--user-data-dir={profile.FullName}", "about:blank" })
        start.ArgumentList.Add(arg);
    using var browser = Process.Start(start) ?? throw new InvalidOperationException("Could not start Chromium.");
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
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
            var port = (await File.ReadAllLinesAsync(portFile, token)).FirstOrDefault();
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
        await SendAsync("Page.navigate", new { url = origin.ToString() });
        await RetryAsync(() => EvaluateAsync("document.querySelector('#count')?.textContent === '0' && document.querySelector('#increment')?.textContent.trim() === 'Increment one' && document.querySelector('#scope')?.textContent === 'closed'"), token);
        await EvaluateAsync("document.querySelector('#increment').click(); window.joltBrowserSentinel = true");
        await RetryAsync(() => EvaluateAsync("document.querySelector('#count')?.textContent === '1' && document.querySelector('#indicator [data-state]')?.dataset.state === 'open' && document.querySelector('#scope')?.textContent === 'open'"), token);
        Console.WriteLine("Counter click updated the parent count and Reka UI state.");

        if (args.Length == 3)
        {
            var file = args[2];
            var original = await File.ReadAllTextAsync(file, token);
            try
            {
                await File.WriteAllTextAsync(file, original.Replace("> one</small>", "> edited</small>", StringComparison.Ordinal), token);
                await RetryAsync(() => EvaluateAsync("document.querySelector('#child-label')?.textContent.trim() === 'edited' && document.querySelector('#count')?.textContent === '1' && window.joltBrowserSentinel === true"), token);
                Console.WriteLine("A child .jazor edit updated the button while preserving the parent state and page.");
            }
            finally
            {
                await File.WriteAllTextAsync(file, original);
            }
        }

        async Task<bool> EvaluateAsync(string expression)
        {
            var response = await SendAsync("Runtime.evaluate", new { expression, returnByValue = true });
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
                    continue;
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
    profile.Delete(recursive: true);
}

static async Task RetryAsync(Func<Task<bool>> verify, CancellationToken token)
{
    while (!await verify())
        await Task.Delay(100, token);
}
