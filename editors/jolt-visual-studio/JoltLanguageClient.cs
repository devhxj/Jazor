using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;

namespace Jolt.VisualStudio;

public sealed class JoltContentType
{
    internal const string Name = "jazor";

    [Export, Name(Name), BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
    [BaseDefinition("code++")] // Visual Studio's built-in TextMate classifier opts in through this base.
    internal static ContentTypeDefinition? Definition = null;

    [Export, FileExtension(".jazor"), ContentType(Name)]
    internal static FileExtensionToContentTypeDefinition? FileExtension = null;
}

[Export(typeof(ILanguageClient)), ContentType(JoltContentType.Name)]
public sealed class JoltLanguageClient : ILanguageClient
{
    public string Name => "Jolt";
    public IEnumerable<string> ConfigurationSections => Array.Empty<string>();
    public object InitializationOptions => new { visualStudioClassifications = true };
    public IEnumerable<string> FilesToWatch { get; } = new[]
        { "**/*.jazor", "**/package.json", "**/pnpm-lock.yaml", "**/.npmrc", "**/node_modules/**/*.d.ts", "**/node_modules/**/*.d.mts", "**/node_modules/**/*.d.cts" };
    public bool ShowNotificationOnInitializeFailed => true;

    public event AsyncEventHandler<EventArgs>? StartAsync;
    // Visual Studio owns the connection lifetime. Closing stdin stops Jolt's stdio server.
    public event AsyncEventHandler<EventArgs>? StopAsync { add { } remove { } }

    public Task OnLoadedAsync() => StartAsync.InvokeAsync(this, EventArgs.Empty);

    public Task<Connection?> ActivateAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string cliHome = Environment.GetEnvironmentVariable("DOTNET_CLI_HOME")
            ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string toolsDirectory = Path.Combine(cliHome, ".dotnet", "tools");
        if (!File.Exists(Path.Combine(toolsDirectory, "jolt.cmd")))
            throw new FileNotFoundException("Install the Jolt CLI with 'dotnet tool install --global Jazor.Jolt --version 2.1.0-preview.4'.");

        // The .NET SDK writes a .cmd shim for RID tools. It launches the packaged CET-disabled apphost.
        var startInfo = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "cmd.exe"), @"/d /c .\jolt.cmd lsp")
        {
            WorkingDirectory = toolsDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
        };
        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start the Jolt language server.");
        return Task.FromResult<Connection?>(new Connection(process.StandardOutput.BaseStream, process.StandardInput.BaseStream));
    }

    public Task OnServerInitializedAsync() => Task.CompletedTask;

    public Task<InitializationFailureContext?> OnServerInitializeFailedAsync(ILanguageClientInitializationInfo info) =>
        Task.FromResult<InitializationFailureContext?>(new InitializationFailureContext { FailureMessage = info.InitializationException?.Message });
}
