using System.Text.Encodings.Web;
using System.Runtime.InteropServices;
using Microsoft.JavaScript.NodeApi;
using Microsoft.JavaScript.NodeApi.Interop;
using Microsoft.JavaScript.NodeApi.Runtime;

namespace Jazor.NodeHost;

/// <summary>Owns one JS thread and its module environment inside the current .NET process.</summary>
internal sealed class NodeEnvironment : IDisposable
{
    private static readonly Lazy<NodeEmbeddingPlatform> Platform = new(() =>
    {
        if (OperatingSystem.IsLinux()) ExposeLinuxNodeApiSymbols();
        var platform = new NodeEmbeddingPlatform(null);
        AppDomain.CurrentDomain.ProcessExit += (_, _) => platform.Dispose();
        return platform;
    });

    private static void ExposeLinuxNodeApiSymbols()
    {
        // .NET loads libnode with RTLD_LOCAL. Linux .node addons resolve napi_*
        // from the global loader scope, so pnpm otherwise fails with an undefined
        // napi_create_promise symbol. Promote the exact library selected by .NET;
        // RTLD_NOLOAD keeps this the same instance for the process-wide platform.
        var library = NativeLibrary.Load("libnode", typeof(NodeEmbeddingPlatform).Assembly, null);
        if (DlAddr(NativeLibrary.GetExport(library, "napi_create_promise"), out var info) == 0)
            throw new DllNotFoundException("Cannot locate the loaded libnode library for Node-API addons.");
        const int rtldLazy = 1, rtldNoLoad = 4, rtldGlobal = 0x100;
        if (DlOpen(Marshal.PtrToStringUTF8(info.FileName)!, rtldLazy | rtldNoLoad | rtldGlobal) == 0)
            throw new DllNotFoundException("Cannot expose libnode symbols: " + Marshal.PtrToStringUTF8(DlError()));
        // NodeApi owns a single platform for the process lifetime. Keep its
        // global loader reference alive while native addons can retain pointers.
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DlInfo
    {
        public nint FileName;
        public nint FileBase;
        public nint SymbolName;
        public nint SymbolAddress;
    }

    [DllImport("libdl.so.2", EntryPoint = "dladdr")]
    private static extern int DlAddr(nint address, out DlInfo info);

    [DllImport("libdl.so.2", EntryPoint = "dlopen")]
    private static extern nint DlOpen([MarshalAs(UnmanagedType.LPUTF8Str)] string path, int flags);

    [DllImport("libdl.so.2", EntryPoint = "dlerror")]
    private static extern nint DlError();

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
        => await InvokeModuleAsync(() => Import(modulePath), exportName, cancellationToken, arguments).ConfigureAwait(false);

    public Task<string> InvokeSourceAsync(string moduleSource, string modulePath, string exportName,
        CancellationToken cancellationToken, params string[] arguments)
        => InvokeModuleAsync(() =>
        {
            // Use Node's normal CommonJS compiler/cache with a virtual filename.
            // Production SSR can load the embedded bridge from a read-only
            // deployment; no temporary tool file or package restore is needed.
            var constructor = JSValueScope.Current.RuntimeContext.Import("node:module");
            var cache = constructor["_cache"];
            var module = cache[modulePath];
            if (module.IsUndefined())
            {
                module = constructor.CallAsConstructor(modulePath);
                module.CallMethod("_compile", moduleSource, modulePath);
                cache[modulePath] = module;
            }
            return module["exports"];
        }, exportName, cancellationToken, arguments);

    private async Task<string> InvokeModuleAsync(Func<JSValue> loadModule, string exportName,
        CancellationToken cancellationToken, string[] arguments)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var operation = RunAsync(async environment =>
        {
            Task<string> resultTask;
            using (var module = new JSReference(loadModule()))
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
