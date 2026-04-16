using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Jazor.VueHost.DevServer;

namespace Jazor.VueHost.Build;

/// <summary>
/// Minimal HTTP API server for esbuild plugin callbacks.
/// Wraps OnDemandCompiler to provide compilation endpoints.
/// </summary>
internal sealed class BuildServer : IAsyncDisposable
{
    private readonly BuildContext _context;
    private readonly OnDemandCompiler _compiler;
    private readonly JsonSerializerOptions _jsonOptions;
    private WebApplication? _app;
    private int _port;

    /// <summary>
    /// Gets the port the server is listening on.
    /// </summary>
    public int Port => _port;

    /// <summary>
    /// Gets whether the server is currently running.
    /// </summary>
    public bool IsRunning => _app is not null;

    public BuildServer(BuildContext context, OnDemandCompiler compiler)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Starts the build server asynchronously.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_app is not null)
        {
            throw new InvalidOperationException("Build server is already running.");
        }

        _port = GetAvailablePort();

        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Production"
        });

        // Configure Kestrel to listen on loopback with the allocated port
        builder.WebHost.UseUrls($"http://127.0.0.1:{_port}");

        _app = builder.Build();

        _app.MapPost("/compile", (HttpContext httpContext) => HandleCompileAsync(httpContext));
        _app.MapGet("/health", () => Results.Ok("ok"));

        await _app.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Stops the build server.
    /// </summary>
    public async Task StopAsync()
    {
        if (_app is null)
        {
            return;
        }

        await _app.StopAsync();
        await _app.DisposeAsync();
        _app = null;
    }

    /// <summary>
    /// Disposes the build server asynchronously.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }

    /// <summary>
    /// Handles the compile endpoint.
    /// </summary>
    private async Task<IResult> HandleCompileAsync(HttpContext httpContext)
    {
        try
        {
            var request = await httpContext.Request.ReadFromJsonAsync<BuildCompileRequest>(_jsonOptions, httpContext.RequestAborted);
            if (request is null)
            {
                return Results.BadRequest(new BuildCompileResponse
                {
                    IsError = true,
                    ErrorMessage = "Invalid request body."
                });
            }

            // Try to get from cache first
            if (_context.CompilationCache.TryGetValue(request.Id, out var artifact))
            {
                return Results.Json(new BuildCompileResponse
                {
                    Js = artifact.JavaScript,
                    Css = artifact.Css,
                    SourceMap = artifact.SourceMap,
                    Dependencies = artifact.Dependencies,
                    IsError = false
                }, _jsonOptions);
            }

            // Compile the file
            var result = await _compiler.CompileAsync(request.Id, httpContext.RequestAborted);

            if (result.IsError)
            {
                return Results.Json(new BuildCompileResponse
                {
                    IsError = true,
                    ErrorMessage = result.ErrorMessage ?? "Compilation failed."
                }, _jsonOptions);
            }

            // Create artifact and cache it
            artifact = new CompilationArtifact
            {
                SourcePath = request.Id,
                JavaScript = result.Content,
                Css = result.StyleContent,
                SourceMap = result.SourceMap,
                Dependencies = result.Dependencies
            };
            _context.CompilationCache[request.Id] = artifact;

            return Results.Json(new BuildCompileResponse
            {
                Js = result.Content,
                Css = result.StyleContent,
                SourceMap = result.SourceMap,
                Dependencies = result.Dependencies,
                IsError = false
            }, _jsonOptions);
        }
        catch (Exception ex)
        {
            return Results.Json(new BuildCompileResponse
            {
                IsError = true,
                ErrorMessage = ex.Message
            }, _jsonOptions);
        }
    }

    /// <summary>
    /// Gets an available port on the loopback interface.
    /// </summary>
    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}

/// <summary>
/// Request body for the compile endpoint.
/// </summary>
internal sealed class BuildCompileRequest
{
    public required string Id { get; init; }
    public string? ResolveDir { get; init; }
    public bool SourceMap { get; init; } = true;
}

/// <summary>
/// Response body for the compile endpoint.
/// </summary>
internal sealed class BuildCompileResponse
{
    public string? Js { get; init; }
    public string? Css { get; init; }
    public string? SourceMap { get; init; }
    public IReadOnlyList<string>? Dependencies { get; init; }
    public required bool IsError { get; init; }
    public string? ErrorMessage { get; init; }
}
