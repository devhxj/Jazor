using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Threading.Channels;

namespace Jazor.VueHost.DevServer;

internal sealed class DevHttpServer : IAsyncDisposable
{
    private static readonly TimeSpan FileChangeDebounceInterval = TimeSpan.FromMilliseconds(100);

    private readonly DevServerOptions _options;
    private readonly OnDemandCompiler _compiler;
    private readonly ModuleResolver _moduleResolver;
    private readonly HtmlTransformer _htmlTransformer;
    private readonly DevServerProxy? _proxy;
    private readonly DevServerReloadHub _reloadHub = new();
    private readonly Channel<IReadOnlyList<string>> _fileChangeChannel = Channel.CreateUnbounded<IReadOnlyList<string>>();
    private readonly ChangeProcessor _changeProcessor;
    private WebApplication? _application;
    private FileSystemWatcher? _fileWatcher;
    private FileChangeDebouncer? _fileChangeDebouncer;
    private Task? _fileChangePump;
    private CancellationTokenSource? _fileChangeCancellationSource;

    public Uri? ListeningUri { get; private set; }

    public DevHttpServer(
        DevServerOptions options,
        OnDemandCompiler compiler,
        ModuleResolver moduleResolver,
        HtmlTransformer htmlTransformer)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
        _moduleResolver = moduleResolver ?? throw new ArgumentNullException(nameof(moduleResolver));
        _htmlTransformer = htmlTransformer ?? throw new ArgumentNullException(nameof(htmlTransformer));
        _proxy = options.ProxyRules.Count == 0 ? null : new DevServerProxy(options.ProxyRules);
        _changeProcessor = new ChangeProcessor(
            _compiler,
            _moduleResolver,
            compiler.DependencyGraph ?? new DependencyGraph(_moduleResolver));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_application is not null)
        {
            return;
        }

        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls($"http://{_options.Host}:{_options.Port}");

        var application = builder.Build();
        if (_options.HmrEnabled)
        {
            application.UseWebSockets();
            application.Map(
                "/@jazor/hmr",
                async context =>
                {
                    if (!context.WebSockets.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        return;
                    }

                    using var socket = await context.WebSockets.AcceptWebSocketAsync();
                    await _reloadHub.AcceptAsync(socket, context.RequestAborted);
                });
        }

        application.MapGet(
            "/@jazor/client",
            static (HttpContext context) =>
            {
                ApplyNoCacheHeaders(context.Response);
                return Results.Text(HtmlTransformer.GetDevClientScript(), "text/javascript");
            });
        application.Map(
            "/{**requestPath}",
            async (HttpContext context) =>
            {
                if (_proxy is not null && await _proxy.TryProxyAsync(context))
                {
                    return;
                }

                if (!HttpMethods.IsGet(context.Request.Method)
                    && !HttpMethods.IsHead(context.Request.Method))
                {
                    context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                    return;
                }

                var result = await HandleRequestAsync(context);
                await result.ExecuteAsync(context);
            });

        await application.StartAsync(cancellationToken);
        _application = application;
        ListeningUri = ResolveListeningUri(application);
        StartFileWatcher();
    }

    public async ValueTask DisposeAsync()
    {
        _fileWatcher?.Dispose();
        _fileWatcher = null;
        _fileChangeDebouncer?.Dispose();
        _fileChangeDebouncer = null;

        if (_fileChangeCancellationSource is not null)
        {
            await _fileChangeCancellationSource.CancelAsync();
            _fileChangeCancellationSource.Dispose();
            _fileChangeCancellationSource = null;
        }

        if (_fileChangePump is not null)
        {
            try
            {
                await _fileChangePump;
            }
            catch (OperationCanceledException)
            {
            }

            _fileChangePump = null;
        }

        await _reloadHub.DisposeAsync();
        _proxy?.Dispose();

        if (_application is null)
        {
            return;
        }

        await _application.DisposeAsync();
        _application = null;
        ListeningUri = null;
    }

    private async Task<IResult> HandleRequestAsync(HttpContext context)
    {
        var requestPath = context.Request.Path.HasValue
            ? context.Request.Path.Value!
            : "/";
        var resolved = _moduleResolver.Resolve(requestPath);
        if (!resolved.Found)
        {
            return Results.NotFound(resolved.Error);
        }

        ApplyNoCacheHeaders(context.Response);

        if (resolved.ResolvedUrl.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            var html = await File.ReadAllTextAsync(resolved.AbsolutePath, context.RequestAborted);
            return Results.Text(_htmlTransformer.Transform(html, resolved.AbsolutePath), "text/html");
        }

        var result = await _compiler.CompileAsync(resolved.AbsolutePath, context.RequestAborted);
        if (result.IsError)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }

        return Results.Text(result.Content, result.ContentType);
    }

    private void StartFileWatcher()
    {
        if (!_options.HmrEnabled || !Directory.Exists(_options.RootDirectory))
        {
            return;
        }

        _fileChangeCancellationSource = new CancellationTokenSource();
        _fileChangePump = PumpFileChangesAsync(_fileChangeCancellationSource.Token);
        _fileChangeDebouncer = new FileChangeDebouncer(FileChangeDebounceInterval);
        _fileChangeDebouncer.DebouncedChange += OnDebouncedFileChanges;
        _fileWatcher = new FileSystemWatcher(_options.RootDirectory)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName
                | NotifyFilters.DirectoryName
                | NotifyFilters.LastWrite
                | NotifyFilters.Size
        };
        _fileWatcher.Changed += OnFileChanged;
        _fileWatcher.Created += OnFileChanged;
        _fileWatcher.Deleted += OnFileChanged;
        _fileWatcher.Renamed += OnFileRenamed;
        _fileWatcher.EnableRaisingEvents = true;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs eventArgs)
        => QueueFileChange(eventArgs.FullPath);

    private void OnFileRenamed(object sender, RenamedEventArgs eventArgs)
    {
        QueueFileChange(eventArgs.OldFullPath);
        QueueFileChange(eventArgs.FullPath);
    }

    private void QueueFileChange(string path)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            _fileChangeDebouncer?.Record(path);
        }
    }

    private void OnDebouncedFileChanges(IReadOnlyList<string> changedPaths)
        => _fileChangeChannel.Writer.TryWrite(changedPaths);

    private async Task PumpFileChangesAsync(CancellationToken cancellationToken)
    {
        await foreach (var changedPaths in _fileChangeChannel.Reader.ReadAllAsync(cancellationToken))
        {
            var result = await _changeProcessor.ProcessChangesAsync(changedPaths, cancellationToken);
            if (result.UpdateKind == ChangeUpdateKind.StyleUpdate)
            {
                await _reloadHub.BroadcastStyleUpdateAsync(
                    result.ChangedCssUrls,
                    result.InlineStyleUpdates,
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    cancellationToken);
                continue;
            }

            if (result.UpdateKind == ChangeUpdateKind.JavaScriptUpdate)
            {
                await _reloadHub.BroadcastJavaScriptUpdateAsync(
                    result.JavaScriptUpdates,
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    cancellationToken);
                continue;
            }

            await _reloadHub.BroadcastReloadAsync(result.FullReloadReason, cancellationToken);
        }
    }

    private static Uri? ResolveListeningUri(WebApplication application)
    {
        foreach (var address in application.Urls)
        {
            if (Uri.TryCreate(address, UriKind.Absolute, out var uri))
            {
                return uri;
            }
        }

        return null;
    }

    private static void ApplyNoCacheHeaders(HttpResponse response)
    {
        response.Headers.CacheControl = "no-store";
        response.Headers.Pragma = "no-cache";
        response.Headers.Expires = "0";
    }
}
