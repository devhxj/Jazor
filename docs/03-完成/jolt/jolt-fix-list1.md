# Jolt Fix List

> Updated: 2026-04-21
> Source: repository-wide multi-agent code review of `src/Jolt`
> Scope: open issues only; generated from review findings, not yet triaged into milestones

## Notes

- The review covered `src/Jolt` code files and excluded generated `obj` files.
- This list de-duplicates overlapping findings from multiple reviewers.
- Comment quality is generally acceptable. No systemic "misleading comment" issue was identified.
- Most pending work is in correctness, exception handling, cancellation, lifecycle cleanup, and syntax-aware parsing/rewrite paths.

## High

- [ ] Fix projection range-end / EOF mapping failures in [ProjectionSegment.cs](/D:/repository/own/jazor/Jazor/src/Jolt/VirtualDocuments/Mapping/ProjectionSegment.cs) and [ProjectionMap.cs](/D:/repository/own/jazor/Jazor/src/Jolt/VirtualDocuments/Mapping/ProjectionMap.cs). Current half-open segment logic treats `Range.End` like a normal offset, so ranges ending exactly at segment boundaries or EOF can fail to map.

- [ ] Fix stderr backpressure deadlock risk in [ProcessAnalysisRpcTransport.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Analysis/ProcessAnalysisRpcTransport.cs). The transport continuously reads `stdout` but does not continuously drain `stderr`, so the child can block before producing a response.

- [ ] Add per-batch exception isolation to the file-change pump in [DevHttpServer.cs](/D:/repository/own/jazor/Jazor/src/Jolt/DevServer/DevHttpServer.cs). A single unexpected exception can fault `_fileChangePump` and permanently disable HMR while producers keep writing to the channel.

- [ ] Record `.js` dependencies in dev mode in [OnDemandCompiler.cs](/D:/repository/own/jazor/Jazor/src/Jolt/DevServer/OnDemandCompiler.cs). Pure `.js` modules currently bypass dependency extraction, so dependent modules can be missed during invalidation and HMR planning.

- [ ] Replace regex-based import rewriting/inference with syntax-aware handling in [BundlerModuleProxyServer.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Build/BundlerModuleProxyServer.cs), [DenoBundleRunner.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Build/DenoBundleRunner.cs), [BuildOrchestrator.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Build/BuildOrchestrator.cs), and [BuildOrchestrator.CssPipeline.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Build/BuildOrchestrator.CssPipeline.cs). Current regexes can match imports inside comments and string literals.

- [ ] Preserve valid `calc()` spacing in the CSS minifier in [BuildOrchestrator.CssPipeline.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Build/BuildOrchestrator.CssPipeline.cs). Current whitespace removal around `+` breaks valid CSS expressions such as `calc(100% + 1rem)`.

- [ ] Synchronize first-use Deno worker startup in [DenoFrontendHost.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Frontend/Deno/Hosting/DenoFrontendHost.cs) and [DenoWorkerProcess.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Frontend/Deno/Hosting/DenoWorkerProcess.cs). Concurrent callers can race and launch multiple workers against shared mutable state.

- [ ] Fix code-action de-duplication in [LspResultAggregator.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Lsp/Aggregation/LspResultAggregator.cs). Actions with the same title/kind but different edits are currently collapsed into one result.

- [ ] Fix `JAZORVUE001` quick-fix targeting in [JazorLspDocumentService.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Lsp/JazorLspDocumentService.cs). The fix currently edits the first `private` method in the file instead of the declaration that actually triggered the diagnostic.

- [ ] Implement real `typeDefinition` behavior in [LspSession.DocumentRequestHandlers.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Lsp/LspSession.DocumentRequestHandlers.cs). The current non-Roslyn fallback incorrectly calls definition and returns the wrong semantic result.

- [ ] Either implement real extension sandboxing or narrow the security claims in [ExtensionSecurityPolicy.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionSecurityPolicy.cs), [ExtensionWorkerServer.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionWorkerServer.cs), and [ExtensionWorkerClient.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionWorkerClient.cs). `ProcessIsolation` is not an OS-level sandbox and does not actually prevent file or network access by malicious extensions.

- [ ] Add a bounded bootstrap timeout for out-of-process extensions in [ExtensionLoader.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionLoader.cs) and [ExtensionWorkerServer.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionWorkerServer.cs). A hung `InitializeAsync` or `ActivateAsync` can currently stall the whole LSP startup/load path.

- [ ] Fail or cancel pending CDP requests when the connection closes in [CdpClient.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Debug/CdpClient.cs). The read loop currently `break`s on EOF/close without completing `_pendingById`, so `SendCommandAsync` can wait forever.

## Medium

- [ ] Ensure `useLsp && useDev` cleans up an already-started dev server on later construction failures in [Program.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Program.cs). The server is started before all lane/session setup completes.

- [ ] Thread the cancellation token into blocking stdio operations in [StdioVueAnalysisRpcServer.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Analysis/StdioVueAnalysisRpcServer.cs). The loop checks cancellation, but `ReadLineAsync`, `WriteLineAsync`, and `FlushAsync` still block uncancelably.

- [ ] Make workspace filesystem enumeration robust against lazy iteration failures in [JoltWorkspaceResolver.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Workspace/JoltWorkspaceResolver.cs). Exceptions currently escape if directories disappear or become inaccessible during `foreach`.

- [ ] Reduce hot-update planning complexity in [JoltService.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Services/JoltService.cs) and [JazorRelatedDocumentResolver.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Workspace/JazorRelatedDocumentResolver.cs). Each frontend change can trigger repeated re-parse/re-resolve work across all open `.jazor` documents.

- [ ] For pure CSS changes, emit inline-style updates for dependent `.vue`/`.jazor` modules in [ChangeProcessor.cs](/D:/repository/own/jazor/Jazor/src/Jolt/DevServer/ChangeProcessor.cs) and [DenoFrontendModuleCompiler.cs](/D:/repository/own/jazor/Jazor/src/Jolt/DevServer/DenoFrontendModuleCompiler.cs). External `<style src>` changes currently refresh only CSS URLs, not already injected inline style blocks.

- [ ] Fix HMR accepted-boundary routing in [ChangeProcessor.cs](/D:/repository/own/jazor/Jazor/src/Jolt/DevServer/ChangeProcessor.cs) and [HtmlTransformer.cs](/D:/repository/own/jazor/Jazor/src/Jolt/DevServer/HtmlTransformer.cs). `AcceptedPath` is currently fixed to the changed module, so importer-level dependency accept callbacks are missed and updates degrade to full reloads.

- [ ] Add visited-path tracking and symlink/junction/reparse-point guards in [DevServerFileSnapshotPoller.cs](/D:/repository/own/jazor/Jazor/src/Jolt/DevServer/DevServerFileSnapshotPoller.cs). Current recursive polling can loop forever on cyclic links.

- [ ] Include companion document state in the cache key, or bypass the normal cache when companions are present, in [OnDemandCompiler.cs](/D:/repository/own/jazor/Jazor/src/Jolt/DevServer/OnDemandCompiler.cs). `.jazor.cs` changes can otherwise reuse stale cached results for the parent `.jazor`.

- [ ] Clean per-start Deno launch workspaces on normal stop/restart in [DenoWorkerProcess.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Frontend/Deno/Hosting/DenoWorkerProcess.cs). Cleanup currently relies on `ProcessExit`, so long-lived sessions accumulate temp directories.

- [ ] Harden `public/` static asset traversal against transient IO/ACL failures in [StaticAssetHandler.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Build/StaticAssetHandler.cs). Current directory traversal can fail the entire build on disappearing or inaccessible subdirectories.

- [ ] Compare output modification time, not just path and length, in stabilization logic in [DenoBundleRunner.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Build/DenoBundleRunner.cs). Same-size rewrites can currently look quiescent too early.

- [ ] Fix `prepareRename` token/range detection in [LspSession.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Lsp/LspSession.cs) and [LspSession.TextAndFormatting.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Lsp/LspSession.TextAndFormatting.cs). Word-end positions, kebab-case names, and `:`-qualified tag names are handled incorrectly.

- [ ] Normalize URIs before projection remap in rename flows in [VolarLaneService.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Lsp/Lanes/VolarLaneService.cs). Current `Ordinal` comparison can leave edits on projected URIs if the host returns different file URI normalization.

- [ ] Stop returning definition results for implementation requests in [JazorLaneService.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Lsp/Lanes/JazorLaneService.cs). Unimplemented `implementation` should return empty until real logic exists.

- [ ] Make `RegisterExtension` atomic or add rollback on provider registration failure in [ExtensionRegistry.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionRegistry.cs). A partially-registered extension can remain in `_extensions` after a later provider validation failure.

- [ ] Bound worker shutdown waits in [ExtensionWorkerClient.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionWorkerClient.cs). `Kill` failure currently falls through to `WaitForExitAsync(CancellationToken.None)`, which can hang disposal indefinitely.

- [ ] Propagate caller cancellation into worker stdin writes in [ExtensionWorkerClient.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionWorkerClient.cs). Request timeouts/cancellation currently cannot interrupt a blocked pipe write.

- [ ] Add framing / JSON exception boundaries in [DapServer.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Debug/DapServer.cs). Malformed stdio input can currently terminate the DAP server outright.

- [ ] Decode fragmented UTF-8 safely in [CdpConnection.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Debug/CdpConnection.cs). Per-chunk `Encoding.UTF8.GetString` corrupts characters that span WebSocket frames.

- [ ] Make block extraction syntax-aware in [JazorVueCompiler.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Jazor/Core/JazorVueCompiler.cs). Current brace counting does not skip strings/comments/raw strings and can truncate method bodies incorrectly.

- [ ] Stop doing global identifier replacements before string-aware scanning in [JazorVueCompiler.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Jazor/Core/JazorVueCompiler.cs). Current fallback lowering can mutate string literal contents.

- [ ] Wrap code-behind reads in IO/ACL-safe handling in [JazorHotReloadMetadataProvider.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Roslyn/InProc/JazorHotReloadMetadataProvider.cs). `File.Exists` followed by `File.ReadAllText` is race-prone.

- [ ] Add caching and tighter bounds to repeated workspace/compilation rebuild paths in [InProcRoslynCodeService.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Roslyn/InProc/InProcRoslynCodeService.cs) and [InProcRoslynCodeService.ProjectionAndContext.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Roslyn/InProc/InProcRoslynCodeService.ProjectionAndContext.cs). Hover/completion/definition currently rebuild too much for large workspaces.

## Low

- [ ] Make `SupportedExtensions` case-insensitive in [DevServerFileWatchFilter.cs](/D:/repository/own/jazor/Jazor/src/Jolt/DevServer/DevServerFileWatchFilter.cs). Mixed-case filenames such as `App.CSS` can be skipped on case-sensitive filesystems.

- [ ] Respect `trimTrailingWhitespace` in [LspSession.TextAndFormatting.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Lsp/LspSession.TextAndFormatting.cs) instead of always trimming trailing spaces/tabs.

- [ ] Pre-index open documents before directory scans in [MarkupComponentBridgeService.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Lsp/Coordination/MarkupComponentBridgeService.cs). The current `FirstOrDefault` lookup inside the file loop introduces an avoidable `O(workspaceFiles * openDocuments)` cost.

- [ ] Replace `relativePath.StartsWith("..")` style boundary checks with stricter root-escape tests in [ExtensionSandboxProfile.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionSandboxProfile.cs), [ExtensionSecurityPolicy.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionSecurityPolicy.cs), [ExtensionLoader.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionLoader.cs), and [ExtensionHostOptionsResolver.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionHostOptionsResolver.cs). Legitimate in-root paths such as `..foo` are currently false positives.

- [ ] Stop falling back to the first component tag when a diagnostic cannot be precisely resolved in [ComponentCodeActionExtension.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/Builtin/ComponentCodeActionExtension.cs). This can generate quick fixes for the wrong component.

- [ ] Avoid unconditional full-text hashing on every workspace symbol query in [WorkspaceSymbolIndex.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/Builtin/WorkspaceSymbolIndex.cs). Existing document version information is not used to short-circuit recomputation.

- [ ] Harden payload kind handling for `source.path` in [DapRequestHandler.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Debug/DapRequestHandler.cs). Unexpected JSON kinds can still throw.

- [ ] Sort SDK versions semantically, not lexicographically, in [RazorSdkToolsetResolver.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Razor/Toolset/RazorSdkToolsetResolver.cs). String sort misorders values such as `10.0.9` and `10.0.100`.

- [ ] Use an OS-appropriate comparer for code-behind path de-duplication in [JazorHotReloadMetadataProvider.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Roslyn/InProc/JazorHotReloadMetadataProvider.cs). `OrdinalIgnoreCase` can collapse distinct files on case-sensitive filesystems.
