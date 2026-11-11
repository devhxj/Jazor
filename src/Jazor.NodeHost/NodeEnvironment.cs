using System.Text.Encodings.Web;
using Microsoft.JavaScript.NodeApi;
using Microsoft.JavaScript.NodeApi.Interop;
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

    private readonly Thread _thread;
    private readonly TaskCompletionSource _ended = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly ThreadState _state;
    private int _disposeStarted;
    private volatile bool _stopping;

    public bool IsStopped => _ended.Task.IsCompleted;

    public NodeEnvironment(string projectDirectory, TextWriter? progress = null)
    {
        var projectFile = Path.Combine(Path.GetFullPath(projectDirectory), "package.json");
        var requireBase = JavaScriptEncoder.Default.Encode(projectFile);
        var ready = new TaskCompletionSource<ThreadState>(TaskCreationOptions.RunContinuationsAsynchronously);
        var platform = Platform.Value;
        _thread = new Thread(() =>
        {
            try
            {
                using var runtime = NodeEmbeddingRuntime.Create(platform, new()
                {
                    // The inspector owns process-global state. Independent
                    // environments must not each initialize its async handles.
                    RuntimeFlags = NodejsRuntime.NodeEmbeddingRuntimeFlags.DefaultFlags |
                        NodejsRuntime.NodeEmbeddingRuntimeFlags.NoCreateInspector |
                        NodejsRuntime.NodeEmbeddingRuntimeFlags.NoStartDebugSignalHandler,
                    MainScript = "globalThis.require = require('node:module').createRequire(\"" + requireBase + "\");"
                });
                using var scope = new NodeEmbeddingNodeApiScope(runtime);
                var context = JSValueScope.Current.RuntimeContext;
                context.RequireFunction = (JSFunction)JSValue.Global["require"];
                if (progress is not null)
                {
                    var log = JSValue.CreateFunction("log", args =>
                    {
                        var values = new JSValue[args.Length];
                        for (var index = 0; index < values.Length; index++) values[index] = args[index];
                        var utility = JSValue.Global["require"].Call(JSValue.Undefined, "node:util");
                        progress.WriteLine((string)utility.CallMethod("format", values));
                        return JSValue.Undefined;
                    });
                    var console = JSValue.Global["console"];
                    console["log"] = log;
                    console["error"] = log;
                    console["warn"] = log;
                }
                var completion = new JSThreadSafeFunction(maxQueueSize: 0, initialThreadCount: 1,
                    asyncResourceName: (JSValue)nameof(NodeEnvironment));
                var math = JSValue.Global["Math"];
                if (math["f16round"].IsUndefined())
                {
                    // This libnode predates ES2025 f16round. .NET Half provides
                    // the required binary16 ties, signed zero, infinity and NaN.
                    math["f16round"] = JSValue.CreateFunction("f16round", args => (double)(Half)(double)args[0]);
                }
                ready.SetResult(new(runtime, context.SynchronizationContext, completion));
                try
                {
                    runtime.RunEventLoop();
                }
                catch (JSException) when (_stopping)
                {
                    // libnode reports forced event-loop termination as an
                    // error. The caller still joins this thread before returning.
                }
                finally
                {
                    context.SynchronizationContext.Dispose();
                }
                _ended.TrySetResult();
            }
            catch (Exception exception)
            {
                ready.TrySetException(exception);
                _ended.TrySetException(exception);
            }
        }) { Name = "Jazor NodeApi" };
        _thread.Start();
        _state = ready.Task.GetAwaiter().GetResult();
    }

    // Must be called on this environment's JS thread. The runtime context owns
    // the standard createRequire function rooted at the project package.json.
    public JSValue Import(string modulePath)
        => JSValueScope.Current.RuntimeContext.Import(Path.GetFullPath(modulePath));

    public Task<T> RunAsync<T>(Func<NodeEnvironment, Task<T>> operation)
        => _state.Context.RunAsync(() => operation(this));

    // Arguments enter the JS thread as managed strings. Module references must
    // outlive callback scopes across await, while returned JS values must not.
    public Task<string> InvokeAsync(string modulePath, string exportName, params string[] arguments)
        => InvokeAsync(modulePath, exportName, CancellationToken.None, arguments);

    public async Task<string> InvokeAsync(string modulePath, string exportName,
        CancellationToken cancellationToken, params string[] arguments)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var operation = RunAsync(async environment =>
        {
            Task<string> resultTask;
            using (var module = new JSReference(environment.Import(modulePath)))
            {
                var values = arguments.Select(value => (JSValue)value).ToArray();
                var result = module.GetValue()[exportName].Call(module.GetValue(), values);
                resultTask = ((JSPromise)result).AsTask(value => (string)value);
            }
            return await resultTask;
        });
        try
        {
            return await operation.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _stopping = true;
            // TerminateEventLoop requires the JS thread in this libnode. Merely
            // cancelling WaitAsync would leave timers, IO and SSR work running.
            _state.Context.Post(() => _state.Runtime.TerminateEventLoop());
            await _ended.Task.ConfigureAwait(false);
            _thread.Join();
            throw;
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposeStarted, 1) != 0)
            return;
        if (!_ended.Task.IsCompleted)
        {
            // Owners drain operations first (pnpm) or close their server first
            // (Vite). Stop remaining JS-owned timers/IO before joining, including
            // timers left by a component's failed lifecycle.
            _stopping = true;
            _state.Context.Post(() => _state.Runtime.TerminateEventLoop());
        }
        _thread.Join();
        _ended.Task.GetAwaiter().GetResult();
    }

    private sealed record ThreadState(NodeEmbeddingRuntime Runtime,
        JSSynchronizationContext Context, JSThreadSafeFunction Completion);
}
