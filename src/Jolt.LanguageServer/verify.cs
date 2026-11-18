#!/usr/bin/env dotnet run
#:exclude **/*.cs
#:property PublishAot=false
using System.Diagnostics;
using System.Text;
using System.Text.Json;

using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
var token = timeout.Token;
var project = Directory.CreateTempSubdirectory("jolt-lsp-");
var path = Path.Combine(project.FullName, "Counter.jazor");
var uri = new Uri(path).AbsoluteUri;
var start = new ProcessStartInfo("dotnet") { RedirectStandardInput = true, RedirectStandardOutput = true };
foreach (var arg in new[] { "run", "--project", "src/Jolt.LanguageServer", "--no-build" })
    start.ArgumentList.Add(arg);
using var process = Process.Start(start)!;
try
{
    await Send("initialize", new { rootUri = new Uri(project.FullName).AbsoluteUri }, 1);
    if ((await Response(1)).GetProperty("capabilities").GetProperty("positionEncoding").GetString() != "utf-16")
        throw new InvalidOperationException("Unexpected position encoding.");
    await Send("initialized", new { });
    await Send("textDocument/didOpen", new { textDocument = new { uri, version = 1, languageId = "jazor", text = "<p>@Count</p>\n@code { private int Count = 1; }" } });
    await Send("textDocument/definition", new { textDocument = new { uri }, position = new { line = 0, character = 6 } }, 2);
    var location = await Response(2);
    if (location.GetProperty("uri").GetString() != uri || location.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32() != 1)
        throw new InvalidOperationException("C# definition did not map back to the author source.");
    await Send("textDocument/completion", new { textDocument = new { uri }, position = new { line = 0, character = 8 } }, 3);
    if (!(await Response(3)).EnumerateArray().Any(item => item.GetProperty("label").GetString() == "Count"))
        throw new InvalidOperationException("Missing Roslyn completion.");
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
    var dependencyFixture = Path.GetFullPath("samples/JazorComponentProbe/Npm/current");
    foreach (var file in new[] { "package.json", "pnpm-lock.yaml" })
        File.Copy(Path.Combine(dependencyFixture, file), Path.Combine(project.FullName, file));
    const string npmMarkup = "@using Npm.RekaUi\n<CollapsibleRoot Open=\"@(true)\" />";
    await Send("textDocument/didChange", new { textDocument = new { uri, version = 3 }, contentChanges = new[] { new { text = npmMarkup } } });
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
    await At("completion", 10, 1, slot.Split('\n')[1].IndexOf("#default", StringComparison.Ordinal) + 2);
    var scoped = (await Response(10)).EnumerateArray().Single(item => item.GetProperty("label").GetString() == "default");
    if (!scoped.GetProperty("detail").GetString()!.Contains("open: boolean", StringComparison.Ordinal))
        throw new InvalidOperationException("Scoped slot completion lost the prop type.");
    const string literals = "@using Npm.RekaUi\n<Separator Orientation=\"hor\" />";
    await Send("textDocument/didChange", new { textDocument = new { uri, version = 7 }, contentChanges = new[] { new { text = literals } } });
    await At("completion", 11, 1, 26);
    var literal = (await Response(11)).EnumerateArray().Single(item => item.GetProperty("label").GetString() == "horizontal");
    CheckEdit(literal, "horizontal", 24, 27);
    var completionTime = Stopwatch.StartNew();
    await At("completion", 12, 1, 26);
    await Response(12);
    Console.WriteLine($"Warm npm literal completion round trip took {completionTime.ElapsedMilliseconds} ms.");
    await Send("shutdown", new { }, 13);
    await Response(13);
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

Task At(string request, int id, int line, int character) =>
    Send($"textDocument/{request}", new { textDocument = new { uri }, position = new { line, character } }, id);

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
        if (!message.TryGetProperty("id", out var value) || value.GetInt32() != id)
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
        return JsonSerializer.Deserialize<JsonElement>(bytes);
    }
}
