using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text.Json;
using Jolt.Extensions;
using Jazor.VueContracts.Protocol;
using Jolt.Jazor.Projection;
using Jolt.Lsp.Aggregation;
using Jolt.Lsp.Coordination;
using Jolt.Lsp.Lanes;
using Jolt.Lsp.Routing;
using Jolt.VirtualDocuments.Registry;
using Jolt.Workspace;

namespace Jolt.Lsp;

internal sealed partial class LspSession
{
    private void ApplyInitializeWorkspaceFolders(LspInitializeParams? parameters)
    {
        var workspaceFolders = (parameters?.WorkspaceFolders ?? [])
            .Where(static folder => !string.IsNullOrWhiteSpace(folder.Uri))
            .Select(CloneWorkspaceFolder)
            .ToArray();

        if (workspaceFolders.Length == 0)
        {
            var fallbackRootUri = parameters?.RootUri;
            if (string.IsNullOrWhiteSpace(fallbackRootUri)
                && !string.IsNullOrWhiteSpace(parameters?.RootPath))
            {
                fallbackRootUri = new Uri(Path.GetFullPath(parameters.RootPath!)).AbsoluteUri;
            }

            if (!string.IsNullOrWhiteSpace(fallbackRootUri))
            {
                workspaceFolders =
                [
                    new LspWorkspaceFolder
                    {
                        Uri = fallbackRootUri!,
                        Name = Path.GetFileName(LspProtocolHelpers.ToDocumentPath(fallbackRootUri!))
                    }
                ];
            }
        }

        lock (_workspaceFoldersGate)
        {
            _workspaceFoldersByUri.Clear();
            foreach (var folder in workspaceFolders)
            {
                _workspaceFoldersByUri[folder.Uri] = folder;
            }
        }
    }

    private void ApplyWorkspaceFolderChanges(LspWorkspaceFoldersChangeEvent changeEvent)
    {
        lock (_workspaceFoldersGate)
        {
            foreach (var removed in changeEvent.Removed ?? [])
            {
                if (string.IsNullOrWhiteSpace(removed.Uri))
                {
                    continue;
                }

                _workspaceFoldersByUri.Remove(removed.Uri);
            }

            foreach (var added in changeEvent.Added ?? [])
            {
                if (string.IsNullOrWhiteSpace(added.Uri))
                {
                    continue;
                }

                _workspaceFoldersByUri[added.Uri] = CloneWorkspaceFolder(added);
            }
        }
    }

    private IReadOnlyList<LspWorkspaceFolder> GetWorkspaceFoldersSnapshot()
    {
        lock (_workspaceFoldersGate)
        {
            var snapshot = _workspaceFoldersByUri.Values
                .OrderBy(static folder => folder.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static folder => folder.Uri, StringComparer.OrdinalIgnoreCase)
                .Select(CloneWorkspaceFolder)
                .ToArray();
            return Array.AsReadOnly(snapshot);
        }
    }

    private IReadOnlyList<string> GetWorkspaceFolderRootPaths()
    {
        lock (_workspaceFoldersGate)
        {
            var snapshot = _workspaceFoldersByUri.Values
                .Select(static folder => TryResolveWorkspaceFolderRootPath(folder.Uri))
                .Where(static path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(static path => path!)
                .ToArray();
            return Array.AsReadOnly(snapshot);
        }
    }

    private static string? TryResolveWorkspaceFolderRootPath(string? workspaceFolderUri)
    {
        if (string.IsNullOrWhiteSpace(workspaceFolderUri))
        {
            return null;
        }

        try
        {
            return Path.GetFullPath(LspProtocolHelpers.ToDocumentPath(workspaceFolderUri));
        }
        catch (UriFormatException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
        catch (PathTooLongException)
        {
            return null;
        }
    }

    private static LspWorkspaceFolder CloneWorkspaceFolder(LspWorkspaceFolder folder)
        => new()
        {
            Uri = folder.Uri,
            Name = string.IsNullOrWhiteSpace(folder.Name)
                ? folder.Uri
                : folder.Name
        };
}
