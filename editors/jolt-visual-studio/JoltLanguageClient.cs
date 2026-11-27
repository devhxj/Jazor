using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Shell;
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
        {
            "**/*.jazor", "**/*.cs", "**/*.vue", "**/*.ts", "**/*.tsx", "**/*.mts", "**/*.cts", "**/*.js", "**/*.jsx", "**/*.mjs",
            "**/tsconfig*.json", "**/package.json", "**/pnpm-lock.yaml", "**/.npmrc",
            "**/*.csproj", "**/*.props", "**/*.targets", "**/project.inputs", "**/project.assets.json", "**/global.json",
            "**/node_modules/**/*.d.ts", "**/node_modules/**/*.d.mts", "**/node_modules/**/*.d.cts",
        };
    public bool ShowNotificationOnInitializeFailed => true;

    public event AsyncEventHandler<EventArgs>? StartAsync;
    // Visual Studio owns the connection lifetime. Closing stdin stops Jolt's stdio server.
    public event AsyncEventHandler<EventArgs>? StopAsync { add { } remove { } }

    public Task OnLoadedAsync() => StartAsync.InvokeAsync(this, EventArgs.Empty);

    public async Task<Connection?> ActivateAsync(CancellationToken cancellationToken)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        var dte = Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
        var document = dte?.ActiveDocument;
        var project = FindProject(dte, document);
        var directory = Path.GetDirectoryName(document?.FullName) ?? Path.GetDirectoryName(project)
            ?? Path.GetDirectoryName(dte?.Solution?.FullName) ?? Environment.CurrentDirectory;
        var configuration = dte?.Solution?.SolutionBuild?.ActiveConfiguration?.Name;
        var startInfo = await JoltServerCommand.ResolveAsync(directory, project, configuration).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        startInfo.RedirectStandardInput = true;
        startInfo.RedirectStandardOutput = true;
        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start the Jolt language server.");
        return new Connection(process.StandardOutput.BaseStream, process.StandardInput.BaseStream);
    }

    private static string? FindProject(EnvDTE.DTE? dte, EnvDTE.Document? document)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        var project = document?.ProjectItem?.ContainingProject;
        if (IsCSharpProject(project)) return project!.FullName;
        foreach (var selected in (dte?.ActiveSolutionProjects as object[] ?? Array.Empty<object>()).OfType<EnvDTE.Project>())
        {
            if (IsCSharpProject(selected)) return selected.FullName;
        }
        return null;
    }

    private static bool IsCSharpProject(EnvDTE.Project? project)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        return string.Equals(Path.GetExtension(project?.FullName), ".csproj", StringComparison.OrdinalIgnoreCase);
    }

    public Task OnServerInitializedAsync() => Task.CompletedTask;

    public Task<InitializationFailureContext?> OnServerInitializeFailedAsync(ILanguageClientInitializationInfo info) =>
        Task.FromResult<InitializationFailureContext?>(new InitializationFailureContext { FailureMessage = info.InitializationException?.Message });
}
