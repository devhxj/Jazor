using System.Text.Encodings.Web;
using Microsoft.JavaScript.NodeApi;
using Microsoft.JavaScript.NodeApi.Runtime;

namespace Jazor.NodeHost;

/// <summary>Owns one JS thread and its module environment inside the current .NET process.</summary>
internal sealed class NodeEnvironment : IDisposable
{
    private static readonly Lazy<NodeEmbeddingPlatform> Platform = new(() =>
    {
        var platform = new NodeEmbeddingPlatform(null);
        AppDomain.CurrentDomain.ProcessExit += (_, _) => platform.Dispose();
        return platform;
    });

    private readonly NodeEmbeddingThreadRuntime _runtime;

    public NodeEnvironment(string projectDirectory)
    {
        var projectFile = Path.Combine(Path.GetFullPath(projectDirectory), "package.json");
        var requireBase = JavaScriptEncoder.Default.Encode(projectFile);
        _runtime = Platform.Value.CreateThreadRuntime(Path.GetDirectoryName(projectFile),
            new NodeEmbeddingRuntimeSettings
            {
                MainScript = "globalThis.require = require('node:module').createRequire(\"" + requireBase + "\");"
            });
    }

    public Task<T> RunAsync<T>(Func<NodeEmbeddingThreadRuntime, Task<T>> operation)
        => _runtime.RunAsync(() => operation(_runtime));

    // Arguments enter the JS thread as managed strings. Module references must
    // outlive callback scopes across await, while returned JS values must not.
    public Task<string> InvokeAsync(string modulePath, string exportName, params string[] arguments)
        => RunAsync(async runtime =>
        {
            using var module = new JSReference(runtime.Import(Path.GetFullPath(modulePath)));
            var values = arguments.Select(value => (JSValue)value).ToArray();
            var result = module.GetValue()[exportName].Call(module.GetValue(), values);
            return await ((JSPromise)result).AsTask(value => (string)value);
        });

    public void Dispose() => _runtime.Dispose();
}
