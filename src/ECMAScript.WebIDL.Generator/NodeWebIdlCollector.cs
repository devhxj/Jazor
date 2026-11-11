using System.Text.Json;
using Jazor.NodeHost;
using Microsoft.JavaScript.NodeApi;

namespace ECMAScript.WebIDL.Generator;

internal sealed class NodeWebIdlCollector(GeneratorOptions options)
{
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
        using var environment = new NodeEnvironment(projectDirectory);
        Console.Error.WriteLine($"[webidl] Embedded NodeApi host: .NET PID {Environment.ProcessId}.");

        return await environment.RunAsync(async runtime =>
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
