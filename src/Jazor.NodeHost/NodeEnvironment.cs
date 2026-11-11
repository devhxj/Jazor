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
                // The shared libnode inspector initializes process-global async
                // state. Independent hosted environments must not each own it.
                RuntimeFlags = NodejsRuntime.NodeEmbeddingRuntimeFlags.DefaultFlags |
                    NodejsRuntime.NodeEmbeddingRuntimeFlags.NoCreateInspector |
                    NodejsRuntime.NodeEmbeddingRuntimeFlags.NoStartDebugSignalHandler,
                MainScript = "globalThis.require = require('node:module').createRequire(\"" + requireBase + "\");"
            });
        _runtime.Run(() =>
        {
            var math = JSValue.Global["Math"];
            if (math["f16round"].IsUndefined())
            {
                // Current libnode predates ES2025 f16round, which Jazor's Half
                // slice already requires. .NET Half provides the exact binary16
                // conversion, including ties, signed zero, infinity and NaN.
                math["f16round"] = JSValue.CreateFunction("f16round", args =>
                    (double)(Half)(double)args[0]);
            }
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
