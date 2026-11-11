using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.JavaScript.NodeApi;
using Microsoft.JavaScript.NodeApi.Runtime;

namespace ECMAScript.WebIDL.Generator;

internal sealed class NodeWebIdlCollector(GeneratorOptions options)
{
    // Node allows one platform initialization per process. Environments are owned
    // per operation and disposed after their actual JS work has completed.
    private static readonly Lazy<NodeEmbeddingPlatform> Platform = new(() =>
    {
        var platform = new NodeEmbeddingPlatform(null);
        AppDomain.CurrentDomain.ProcessExit += (_, _) => platform.Dispose();
        return platform;
    });

    public async Task<WebIdlInventory> CollectAsync(CancellationToken cancellationToken)
    {
        var json = await RunAsync("collect", cancellationToken);
        return JsonSerializer.Deserialize<WebIdlInventory>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("The NodeApi collector returned an invalid WebIDL inventory.");
    }

    public Task<string> TestAsync(CancellationToken cancellationToken)
        => RunAsync("test", cancellationToken);

    private async Task<string> RunAsync(string operation, CancellationToken cancellationToken)
    {
        var projectDirectory = Path.GetDirectoryName(options.NodeProjectPath)!;
        var bootstrap = await PnpmBootstrap.EnsureAsync(projectDirectory, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var requireBase = JavaScriptEncoder.Default.Encode(options.NodeProjectPath);
        using var runtime = Platform.Value.CreateThreadRuntime(projectDirectory,
            new NodeEmbeddingRuntimeSettings
            {
                MainScript = "globalThis.require = require('node:module').createRequire(\"" + requireBase + "\");"
            });
        Console.Error.WriteLine($"[webidl] Embedded NodeApi host: .NET PID {Environment.ProcessId}.");

        return await runtime.RunAsync(async () =>
        {
            // An await closes the initial callback scope even when continuation
            // returns to the JS thread. Keep the module alive through a reference.
            using var host = new JSReference(runtime.Import(Path.Combine(projectDirectory, "host.cjs")));
            var restore = (JSPromise)host.GetValue().CallMethod("restore", bootstrap, options.NodeProjectPath,
                Path.Combine(options.RepositoryRoot, ".tmp", "webidl-pnpm-store"));
            // pnpm's API has no cancellation argument. Never cancel just the C#
            // wait and dispose an environment while native installation still writes.
            var restored = await restore.AsTask(value => (string)value);
            Console.Error.WriteLine("[webidl] NodeApi/pnpm restore: " + restored);
            cancellationToken.ThrowIfCancellationRequested();
            JSValue operationArgument = operation == "test" ? bootstrap : options.SkipSpecificationProse;
            var collect = (JSPromise)host.GetValue().CallMethod(operation, options.NodeProjectPath,
                options.WorkerPath, operationArgument);
            var result = await collect.AsTask(value => (string)value);
            cancellationToken.ThrowIfCancellationRequested();
            return result;
        });
    }
}
