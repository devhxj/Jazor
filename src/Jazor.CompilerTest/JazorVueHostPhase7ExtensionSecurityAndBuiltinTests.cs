using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.DevServer;
using Jazor.VueHost.Extensions;
using Jazor.VueHost.Extensions.Builtin;
using Jazor.VueHost.Jazor.Projection;
using Jazor.VueHost.Lsp;
using Jazor.VueHost.Lsp.Aggregation;
using Jazor.VueHost.Lsp.Coordination;
using Jazor.VueHost.Lsp.Lanes;
using Jazor.VueHost.Lsp.Routing;
using Jazor.VueHost.VirtualDocuments.Registry;
using Jazor.VueHost.Workspace;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostPhase7ExtensionSecurityAndBuiltinTests
{
    [TestMethod]
    public void ExtensionHostOptionsResolver_Resolve_MergesSecurityConfigAndCliOverrides()
    {
        var options = ExtensionHostOptionsResolver.Resolve(
            [
                "--extensions-trusted=trusted.cli.a,trusted.cli.b",
                "--extensions-require-hash=true",
                "--extensions-enforce-provider-permissions=false"
            ],
            rootDirectory: @"D:\repo\phase7",
            config: new JazorConfig
            {
                Extensions = new JazorExtensionsConfig
                {
                    Trusted = ["trusted.config"],
                    RequireAssemblyHash = false,
                    EnforceProviderPermissions = true
                }
            });

        CollectionAssert.AreEquivalent(
            new[] { "trusted.cli.a", "trusted.cli.b" },
            options.TrustedExtensionIds.ToArray());
        Assert.IsTrue(options.RequireAssemblyHash);
        Assert.IsFalse(options.EnforceProviderPermissions);
    }

    [TestMethod]
    public void ExtensionSecurityPolicy_IsAssemblyHashSatisfied_AcceptsNormalizedSha256()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"phase7-hash-{Guid.NewGuid():N}.bin");
        File.WriteAllText(tempFile, "phase7-hash");
        try
        {
            var expectedHash = ComputeSha256Hex(tempFile);
            var prefixedHash = "sha256:" + expectedHash.ToLowerInvariant();

            Assert.IsTrue(ExtensionSecurityPolicy.IsAssemblyHashSatisfied(tempFile, prefixedHash));
            Assert.IsFalse(ExtensionSecurityPolicy.IsAssemblyHashSatisfied(tempFile, "sha256:deadbeef"));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [TestMethod]
    public void ExtensionSecurityPolicy_IsProviderPermissionSatisfied_RejectsMissingCapabilities()
    {
        var deniedManifest = new ExtensionManifest
        {
            Permissions = new ExtensionPermissionManifest
            {
                Providers = ["hover"]
            }
        };
        var allowedManifest = new ExtensionManifest
        {
            Permissions = new ExtensionPermissionManifest
            {
                Providers = ["hover", "completion", "unknown-capability"]
            }
        };

        var denied = ExtensionSecurityPolicy.IsProviderPermissionSatisfied(
            typeof(ManifestLoadableTestExtension),
            deniedManifest,
            out var deniedReason);
        var allowed = ExtensionSecurityPolicy.IsProviderPermissionSatisfied(
            typeof(ManifestLoadableTestExtension),
            allowedManifest,
            out var allowedReason);

        Assert.IsFalse(denied);
        Assert.IsNotNull(deniedReason);
        StringAssert.Contains(deniedReason, "completion", StringComparison.OrdinalIgnoreCase);
        Assert.IsTrue(allowed);
        Assert.IsNull(allowedReason);

        var normalized = ExtensionSecurityPolicy.NormalizeAllowedCapabilities(allowedManifest);
        CollectionAssert.AreEquivalent(
            new[] { "hover", "completion" },
            normalized.ToArray());
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithValidManifest_LoadsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"]);

            var registry = new ExtensionRegistry();
            var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(sandbox.RootDirectory, sandbox.ExtensionsDirectory),
                CancellationToken.None);

            Assert.AreEqual(1, registry.GetExtensions().Count);
            Assert.IsTrue(registry.GetExtensions().ContainsKey(ManifestLoadableTestExtension.ExtensionId));
            Assert.AreEqual(1, registry.GetLspHoverProviders().Count);
            Assert.AreEqual(1, registry.GetLspCompletionProviders().Count);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithProviderPermissionMismatch_SkipsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover"]);

            var registry = new ExtensionRegistry();
            var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    enforceProviderPermissions: true),
                CancellationToken.None);

            Assert.AreEqual(0, registry.GetExtensions().Count);
            Assert.AreEqual(0, registry.GetLspHoverProviders().Count);
            Assert.AreEqual(0, registry.GetLspCompletionProviders().Count);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithMissingAssemblyHash_WhenRequired_SkipsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: null,
                providers: ["hover", "completion"]);

            var registry = new ExtensionRegistry();
            var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    requireAssemblyHash: true),
                CancellationToken.None);

            Assert.AreEqual(0, registry.GetExtensions().Count);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithManifestIdMismatch_SkipsExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: "phase7.manifest.id-mismatch",
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"]);

            var registry = new ExtensionRegistry();
            var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(sandbox.RootDirectory, sandbox.ExtensionsDirectory),
                CancellationToken.None);

            Assert.AreEqual(0, registry.GetExtensions().Count);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task ExtensionLoader_LoadUserExtensionsAsync_WithTrustedAllowList_SkipsUntrustedExtension()
    {
        var sandbox = CreateExtensionSandbox();
        try
        {
            WriteManifest(
                sandbox.ExtensionDirectory,
                id: ManifestLoadableTestExtension.ExtensionId,
                assembly: sandbox.AssemblyFileName,
                type: typeof(ManifestLoadableTestExtension).FullName!,
                assemblySha256: sandbox.AssemblySha256,
                providers: ["hover", "completion"]);

            var registry = new ExtensionRegistry();
            var loader = new ExtensionLoader(registry);
            await loader.LoadUserExtensionsAsync(
                CreateHostOptions(
                    sandbox.RootDirectory,
                    sandbox.ExtensionsDirectory,
                    trustedExtensionIds: new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {
                        "phase7.other-extension"
                    }),
                CancellationToken.None);

            Assert.AreEqual(0, registry.GetExtensions().Count);
        }
        finally
        {
            sandbox.Dispose();
        }
    }

    [TestMethod]
    public async Task BuiltinExtensionCatalog_LoadBuiltinExtensionsAsync_RegistersProductionProviders()
    {
        var registry = new ExtensionRegistry();
        var loader = new ExtensionLoader(registry);
        await loader.LoadBuiltinExtensionsAsync(
            BuiltinExtensionCatalog.Create(),
            rootDirectory: Path.GetFullPath(Path.GetTempPath()),
            cancellationToken: CancellationToken.None);

        CollectionAssert.Contains(
            registry.GetLspDiagnosticProviders().Select(static provider => provider.Name).ToArray(),
            "BuiltinStructureDiagnosticProvider");
        CollectionAssert.Contains(
            registry.GetLspCompletionProviders().Select(static provider => provider.Name).ToArray(),
            "BuiltinDirectiveCompletionProvider");
        CollectionAssert.Contains(
            registry.GetLspCodeActionProviders().Select(static provider => provider.Name).ToArray(),
            "BuiltinComponentCodeActionProvider");
        CollectionAssert.Contains(
            registry.GetLspWorkspaceSymbolProviders().Select(static provider => provider.Name).ToArray(),
            "BuiltinWorkspaceSymbolProvider");
    }

    [TestMethod]
    public async Task BuiltinStructureDiagnosticProvider_ReportsTemplateAndCodeShapeIssues()
    {
        var registry = new ExtensionRegistry();
        var loader = new ExtensionLoader(registry);
        await loader.LoadBuiltinExtensionsAsync(
            BuiltinExtensionCatalog.Create(),
            rootDirectory: Path.GetFullPath(Path.GetTempPath()),
            cancellationToken: CancellationToken.None);

        var provider = registry.GetLspDiagnosticProviders()
            .Single(static item => string.Equals(item.Name, "BuiltinStructureDiagnosticProvider", StringComparison.Ordinal));
        var document = new DocumentSnapshot(
            documentPath: Path.Combine(Path.GetTempPath(), $"phase7-structure-{Guid.NewGuid():N}.jazor"),
            documentKind: DocumentKind.Jazor,
            text: """
                  <div>hello</div>
                  @code {
                    private int count = 0;
                  """,
            version: "1");
        var diagnostics = await provider.ProvideDiagnosticsAsync(
            new LspDiagnosticProviderContext(document, Array.Empty<LspDiagnostic>()),
            CancellationToken.None);
        var diagnosticCodes = diagnostics.Select(static item => item.Code).ToArray();

        CollectionAssert.Contains(diagnosticCodes, "JAZORVUEEXTSTR004");
        CollectionAssert.Contains(diagnosticCodes, "JAZORVUEEXTSTR005");
    }

    [TestMethod]
    public async Task BuiltinDirectiveCompletionProvider_ServesDirectiveCompletionsThroughLspSession()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
        var registry = new ExtensionRegistry();
        var loader = new ExtensionLoader(registry);
        await loader.LoadBuiltinExtensionsAsync(
            BuiltinExtensionCatalog.Create(),
            rootDirectory: Path.GetFullPath(Path.GetTempPath()),
            cancellationToken: CancellationToken.None);

        var documentPath = Path.Combine(Path.GetTempPath(), $"phase7-completion-{Guid.NewGuid():N}.jazor");
        await workspaceStore.UpsertDocumentAsync(
            new DocumentSnapshot(documentPath, DocumentKind.Jazor, "@c", version: "1"),
            CancellationToken.None);

        using var outputStream = new MemoryStream();
        var session = CreateSession(
            workspaceStore,
            virtualDocumentRegistry,
            [new EmptyJazorLane()],
            outputStream,
            registry);

        var response = await session.HandleRequestAsync(
            new LspRequestMessage
            {
                Id = 3101,
                Method = "textDocument/completion",
                Params = new LspCompletionParams
                {
                    TextDocument = new LspTextDocumentIdentifier
                    {
                        Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                    },
                    Position = new LspPosition { Line = 0, Character = 2 }
                }
            },
            CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsNull(response!.Error);
        var items = response.Result as IReadOnlyList<LspCompletionItem>;
        Assert.IsNotNull(items);
        Assert.IsTrue(items.Any(static item => string.Equals(item.Label, "@code", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task BuiltinComponentCodeActionProvider_OffersVueImportQuickFixThroughLspSession()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
        var registry = new ExtensionRegistry();
        var loader = new ExtensionLoader(registry);
        await loader.LoadBuiltinExtensionsAsync(
            BuiltinExtensionCatalog.Create(),
            rootDirectory: Path.GetFullPath(Path.GetTempPath()),
            cancellationToken: CancellationToken.None);

        var rootDirectory = Path.Combine(Path.GetTempPath(), $"phase7-code-action-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootDirectory);
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var vuePath = Path.Combine(rootDirectory, "CounterWidget.vue");
            await File.WriteAllTextAsync(vuePath, "<template><div /></template>");
            await workspaceStore.UpsertDocumentAsync(
                new DocumentSnapshot(
                    documentPath,
                    DocumentKind.Jazor,
                    """
                    <template>
                      <CounterWidget />
                    </template>
                    """,
                    version: "1"),
                CancellationToken.None);

            using var outputStream = new MemoryStream();
            var session = CreateSession(
                workspaceStore,
                virtualDocumentRegistry,
                [new EmptyJazorLane()],
                outputStream,
                registry);

            var response = await session.HandleRequestAsync(
                new LspRequestMessage
                {
                    Id = 3102,
                    Method = "textDocument/codeAction",
                    Params = new LspCodeActionParams
                    {
                        TextDocument = new LspTextDocumentIdentifier
                        {
                            Uri = LspProtocolHelpers.ToDocumentUri(documentPath)
                        },
                        Range = new LspRange
                        {
                            Start = new LspPosition { Line = 1, Character = 3 },
                            End = new LspPosition { Line = 1, Character = 18 }
                        },
                        Context = new LspCodeActionContext
                        {
                            Diagnostics =
                            [
                                new LspDiagnostic
                                {
                                    Range = new LspRange
                                    {
                                        Start = new LspPosition { Line = 1, Character = 3 },
                                        End = new LspPosition { Line = 1, Character = 18 }
                                    },
                                    Severity = 1,
                                    Code = "JAZORVUEFRONTEND001",
                                    Source = "Jazor.VueHost.Frontend",
                                    Message = "Unable to resolve component CounterWidget."
                                }
                            ]
                        }
                    }
                },
                CancellationToken.None);

            Assert.IsNotNull(response);
            Assert.IsNull(response!.Error);
            var actions = response.Result as IReadOnlyList<LspCodeAction>;
            Assert.IsNotNull(actions);

            var importAction = actions.FirstOrDefault(static action =>
                string.Equals(action.Title, "Add @vueimport for CounterWidget", StringComparison.Ordinal));
            Assert.IsNotNull(importAction);
            Assert.IsNotNull(importAction!.Edit);

            var uri = LspProtocolHelpers.ToDocumentUri(documentPath);
            Assert.IsTrue(importAction.Edit.Changes.ContainsKey(uri));
            var inserted = importAction.Edit.Changes[uri].Single();
            StringAssert.Contains(inserted.NewText, "@vueimport CounterWidget from \"./CounterWidget.vue\"", StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task BuiltinWorkspaceSymbolProvider_IndexesOpenDocumentsWithStableOrdering()
    {
        var workspaceStore = new InMemoryWorkspaceStore();
        var virtualDocumentRegistry = new InMemoryVirtualDocumentRegistry();
        var registry = new ExtensionRegistry();
        var loader = new ExtensionLoader(registry);
        await loader.LoadBuiltinExtensionsAsync(
            BuiltinExtensionCatalog.Create(),
            rootDirectory: Path.GetFullPath(Path.GetTempPath()),
            cancellationToken: CancellationToken.None);

        var rootDirectory = Path.Combine(Path.GetTempPath(), $"phase7-workspace-symbol-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootDirectory);
        try
        {
            var jazorPath = Path.Combine(rootDirectory, "Counter.jazor");
            var csharpPath = Path.Combine(rootDirectory, "DataService.cs");
            await workspaceStore.UpsertDocumentAsync(
                new DocumentSnapshot(
                    jazorPath,
                    DocumentKind.Jazor,
                    """
                    <template>
                      <TodoItem />
                    </template>
                    @code {
                        public void LoadData() { }
                    }
                    """,
                    version: "1"),
                CancellationToken.None);
            await workspaceStore.UpsertDocumentAsync(
                new DocumentSnapshot(
                    csharpPath,
                    DocumentKind.CSharp,
                    "public class DataService { public void SaveRecord() { } }",
                    version: "1"),
                CancellationToken.None);

            using var outputStream = new MemoryStream();
            var session = CreateSession(
                workspaceStore,
                virtualDocumentRegistry,
                [new EmptyJazorLane()],
                outputStream,
                registry);

            var filteredResponse = await session.HandleRequestAsync(
                new LspRequestMessage
                {
                    Id = 3103,
                    Method = "workspace/symbol",
                    Params = new LspWorkspaceSymbolParams
                    {
                        Query = "Load"
                    }
                },
                CancellationToken.None);

            Assert.IsNotNull(filteredResponse);
            Assert.IsNull(filteredResponse!.Error);
            var filteredSymbols = filteredResponse.Result as IReadOnlyList<LspWorkspaceSymbol>;
            Assert.IsNotNull(filteredSymbols);
            Assert.IsTrue(filteredSymbols.Any(static symbol => string.Equals(symbol.Name, "LoadData", StringComparison.Ordinal)));

            var allResponse = await session.HandleRequestAsync(
                new LspRequestMessage
                {
                    Id = 3104,
                    Method = "workspace/symbol",
                    Params = new LspWorkspaceSymbolParams
                    {
                        Query = string.Empty
                    }
                },
                CancellationToken.None);

            Assert.IsNotNull(allResponse);
            Assert.IsNull(allResponse!.Error);
            var allSymbols = allResponse.Result as IReadOnlyList<LspWorkspaceSymbol>;
            Assert.IsNotNull(allSymbols);
            Assert.IsTrue(allSymbols.Count >= 2);

            var actualOrder = allSymbols.Select(static symbol => symbol.Name).ToArray();
            var expectedOrder = actualOrder
                .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            CollectionAssert.AreEqual(expectedOrder, actualOrder);
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    private static ExtensionHostOptions CreateHostOptions(
        string rootDirectory,
        string extensionsDirectory,
        IReadOnlySet<string>? trustedExtensionIds = null,
        bool requireAssemblyHash = true,
        bool enforceProviderPermissions = true)
    {
        return new ExtensionHostOptions
        {
            RootDirectory = rootDirectory,
            Enabled = true,
            ExtensionsDirectory = extensionsDirectory,
            AllowExternalDirectory = false,
            TrustedExtensionIds = trustedExtensionIds ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            RequireAssemblyHash = requireAssemblyHash,
            EnforceProviderPermissions = enforceProviderPermissions
        };
    }

    private static void WriteManifest(
        string extensionDirectory,
        string id,
        string assembly,
        string type,
        string? assemblySha256,
        string[] providers)
    {
        var payload = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["id"] = id,
            ["assembly"] = assembly,
            ["type"] = type,
            ["permissions"] = new Dictionary<string, object?>
            {
                ["providers"] = providers
            }
        };
        if (!string.IsNullOrWhiteSpace(assemblySha256))
        {
            payload["assemblySha256"] = assemblySha256;
        }

        var manifestPath = Path.Combine(extensionDirectory, "extension.json");
        File.WriteAllText(
            manifestPath,
            JsonSerializer.Serialize(payload));
    }

    private static ExtensionSandbox CreateExtensionSandbox()
    {
        var rootDirectory = Path.Combine(Path.GetTempPath(), $"phase7-extension-sandbox-{Guid.NewGuid():N}");
        var extensionsDirectory = Path.Combine(rootDirectory, ".jazor", "extensions");
        var extensionDirectory = Path.Combine(extensionsDirectory, "manifest-loadable");
        Directory.CreateDirectory(extensionDirectory);

        var sourceAssemblyPath = typeof(ManifestLoadableTestExtension).Assembly.Location;
        var assemblyFileName = "manifest-loadable.dll";
        var copiedAssemblyPath = Path.Combine(extensionDirectory, assemblyFileName);
        File.Copy(sourceAssemblyPath, copiedAssemblyPath, overwrite: true);

        return new ExtensionSandbox(
            rootDirectory,
            extensionsDirectory,
            extensionDirectory,
            assemblyFileName,
            copiedAssemblyPath,
            ComputeSha256Hex(copiedAssemblyPath));
    }

    private static string ComputeSha256Hex(string filePath)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(filePath)));

    private static LspSession CreateSession(
        IVueHostWorkspaceStore workspaceStore,
        IVirtualDocumentRegistry virtualDocumentRegistry,
        ILspLane[] lanes,
        Stream outputStream,
        IExtensionRegistry extensionRegistry)
    {
        var laneRouter = new LspLaneRouter();
        var projectionResolver = new DocumentProjectionResolver(
            new DocumentRegionClassifier(),
            virtualDocumentRegistry);
        var projectionService = new JazorProjectionService();
        var resultAggregator = new LspResultAggregator();
        var markupBridgeService = new MarkupComponentBridgeService(workspaceStore);
        var markupBridgeFanout = new MarkupBridgeFanoutCoordinator(markupBridgeService, resultAggregator);
        var laneMap = lanes.ToDictionary(static lane => lane.LaneKind);

        return new LspSession(
            workspaceStore,
            lanes,
            laneRouter,
            new LspMessageWriter(outputStream),
            projectionService,
            virtualDocumentRegistry,
            projectionResolver,
            resultAggregator,
            markupBridgeFanout,
            new ReferenceCoordinator(laneMap, laneRouter, markupBridgeFanout),
            new RenameCoordinator(laneMap, laneRouter, resultAggregator, markupBridgeFanout),
            new CodeActionCoordinator(laneMap, laneRouter, resultAggregator),
            workspaceDocumentChangeSink: null,
            extensionRegistry: extensionRegistry);
    }

    private sealed class EmptyJazorLane : ILspLane
    {
        public LaneKind LaneKind => LaneKind.Jazor;

        public ValueTask<IReadOnlyList<LspDiagnostic>> GetDiagnosticsAsync(DocumentSnapshot document, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspDiagnostic>>(Array.Empty<LspDiagnostic>());

        public ValueTask<LspHoverResult?> GetHoverAsync(DocumentSnapshot document, LspPosition position, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<LspHoverResult?>(null);

        public ValueTask<IReadOnlyList<LspCompletionItem>> GetCompletionItemsAsync(DocumentSnapshot document, LspPosition position, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspCompletionItem>>(Array.Empty<LspCompletionItem>());

        public ValueTask<IReadOnlyList<LspDocumentSymbol>> GetDocumentSymbolsAsync(DocumentSnapshot document, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspDocumentSymbol>>(Array.Empty<LspDocumentSymbol>());

        public ValueTask<IReadOnlyList<LspSemanticToken>> GetSemanticTokensAsync(DocumentSnapshot document, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspSemanticToken>>(Array.Empty<LspSemanticToken>());

        public ValueTask<LspSignatureHelp?> GetSignatureHelpAsync(DocumentSnapshot document, LspPosition position, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<LspSignatureHelp?>(null);

        public ValueTask<IReadOnlyList<LspLocation>> GetDefinitionAsync(DocumentSnapshot document, LspPosition position, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

        public ValueTask<IReadOnlyList<LspLocation>> GetReferencesAsync(DocumentSnapshot document, LspPosition position, bool includeDeclaration, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspLocation>>(Array.Empty<LspLocation>());

        public ValueTask<LspWorkspaceEdit?> GetRenameAsync(DocumentSnapshot document, LspPosition position, string newName, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<LspWorkspaceEdit?>(null);

        public ValueTask<IReadOnlyList<LspCodeAction>> GetCodeActionsAsync(DocumentSnapshot document, LspRange range, IReadOnlyList<LspDiagnostic> diagnostics, ProjectionTarget projectionTarget, CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspCodeAction>>(Array.Empty<LspCodeAction>());
    }

    private sealed class ExtensionSandbox(
        string rootDirectory,
        string extensionsDirectory,
        string extensionDirectory,
        string assemblyFileName,
        string assemblyPath,
        string assemblySha256) : IDisposable
    {
        public string RootDirectory { get; } = rootDirectory;

        public string ExtensionsDirectory { get; } = extensionsDirectory;

        public string ExtensionDirectory { get; } = extensionDirectory;

        public string AssemblyFileName { get; } = assemblyFileName;

        public string AssemblyPath { get; } = assemblyPath;

        public string AssemblySha256 { get; } = assemblySha256;

        public void Dispose()
        {
            if (Directory.Exists(RootDirectory))
            {
                Directory.Delete(RootDirectory, recursive: true);
            }
        }
    }
}

public sealed class ManifestLoadableTestExtension : IExtension, ILspHoverProvider, ILspCompletionProvider
{
    public const string ExtensionId = "phase7.manifest-loadable";

    private static readonly ExtensionMetadata MetadataValue = new(
        Id: ExtensionId,
        Name: "Manifest Loadable Test Extension",
        Version: "1.0.0");

    ExtensionMetadata IExtension.Metadata => MetadataValue;

    string ILspHoverProvider.Name => "ManifestLoadableHoverProvider";

    int ILspHoverProvider.Priority => 10;

    string ILspCompletionProvider.Name => "ManifestLoadableCompletionProvider";

    int ILspCompletionProvider.Priority => 10;

    ValueTask IExtension.InitializeAsync(ExtensionContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask IExtension.ActivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask IExtension.DeactivateAsync(CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask<LspHoverResult?> ILspHoverProvider.ProvideHoverAsync(
        LspHoverProviderContext context,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<LspHoverResult?>(null);
    }

    ValueTask<IReadOnlyList<LspCompletionItem>> ILspCompletionProvider.ProvideCompletionItemsAsync(
        LspCompletionProviderContext context,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<IReadOnlyList<LspCompletionItem>>(Array.Empty<LspCompletionItem>());
    }
}
