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

- [x] Fix projection range-end / EOF mapping failures in [ProjectionSegment.cs](/D:/repository/own/jazor/Jazor/src/Jolt/VirtualDocuments/Mapping/ProjectionSegment.cs) and [ProjectionMap.cs](/D:/repository/own/jazor/Jazor/src/Jolt/VirtualDocuments/Mapping/ProjectionMap.cs). Resolved in the current baseline by using boundary/end-preference mapping logic with explicit EOF regression coverage.

- [x] Fix stderr backpressure deadlock risk in [ProcessAnalysisRpcTransport.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Analysis/ProcessAnalysisRpcTransport.cs). Resolved in the current baseline by continuously draining stderr with bounded capture while stdout waits for the RPC response.

- [x] Add per-batch exception isolation to the file-change pump in [DevHttpServer.cs](/D:/repository/own/jazor/Jazor/src/Jolt/DevServer/DevHttpServer.cs). Resolved in the current baseline by catching per-batch failures, broadcasting an HMR error, and keeping the pump alive.

- [x] Record `.js` dependencies in dev mode in [OnDemandCompiler.cs](/D:/repository/own/jazor/Jazor/src/Jolt/DevServer/OnDemandCompiler.cs). Resolved in the current baseline by extracting JavaScript dependencies for both dev pass-through and build transforms, with dependency-graph regression coverage.

- [ ] Replace regex-based import rewriting/inference with syntax-aware handling in [BundlerModuleProxyServer.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Build/BundlerModuleProxyServer.cs), [DenoBundleRunner.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Build/DenoBundleRunner.cs), [BuildOrchestrator.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Build/BuildOrchestrator.cs), and [BuildOrchestrator.CssPipeline.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Build/BuildOrchestrator.CssPipeline.cs). Current regexes can match imports inside comments and string literals.

- [ ] Preserve valid `calc()` spacing in the CSS minifier in [BuildOrchestrator.CssPipeline.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Build/BuildOrchestrator.CssPipeline.cs). Current whitespace removal around `+` breaks valid CSS expressions such as `calc(100% + 1rem)`.

- [x] Synchronize first-use Deno worker startup in [DenoFrontendHost.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Frontend/Deno/Hosting/DenoFrontendHost.cs) and [DenoWorkerProcess.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Frontend/Deno/Hosting/DenoWorkerProcess.cs). Resolved by serializing worker start/stop in `DenoWorkerProcess` and retaining host lifecycle gating.

- [x] Fix code-action de-duplication in [LspResultAggregator.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Lsp/Aggregation/LspResultAggregator.cs). Resolved in the current baseline by including workspace-edit content in the aggregation key, so actions with the same title/kind but different edits are preserved.

- [x] Fix `JAZORVUE001` quick-fix targeting in [JazorLspDocumentService.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Lsp/JazorLspDocumentService.cs). Resolved by matching the diagnostic range to the private method declaration and only falling back when there is a single unambiguous private method.

- [ ] Implement real `typeDefinition` behavior in [LspSession.DocumentRequestHandlers.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Lsp/LspSession.DocumentRequestHandlers.cs). The current non-Roslyn fallback incorrectly calls definition and returns the wrong semantic result.

- [ ] Either implement real extension sandboxing or narrow the security claims in [ExtensionSecurityPolicy.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionSecurityPolicy.cs), [ExtensionWorkerServer.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionWorkerServer.cs), and [ExtensionWorkerClient.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionWorkerClient.cs). `ProcessIsolation` is not an OS-level sandbox and does not actually prevent file or network access by malicious extensions.

- [ ] Add a bounded bootstrap timeout for out-of-process extensions in [ExtensionLoader.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionLoader.cs) and [ExtensionWorkerServer.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionWorkerServer.cs). A hung `InitializeAsync` or `ActivateAsync` can currently stall the whole LSP startup/load path.

- [x] Fail or cancel pending CDP requests when the connection closes in [CdpClient.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Debug/CdpClient.cs). Resolved in the current baseline by completing pending requests on EOF/close and canceling during dispose.

## Medium

- [x] Ensure `useLsp && useDev` cleans up an already-started dev server on later construction failures in [Program.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Program.cs). Resolved by moving dev-server startup inside the `try/finally` that disposes `DevServerRuntime`.

- [x] Thread the cancellation token into blocking stdio operations in [StdioVueAnalysisRpcServer.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Analysis/StdioVueAnalysisRpcServer.cs). Resolved in the current baseline by passing the token into `ReadLineAsync`, `WriteLineAsync`, and `FlushAsync`.

- [x] Make workspace filesystem enumeration robust against lazy iteration failures in [JoltWorkspaceResolver.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Workspace/JoltWorkspaceResolver.cs). Resolved in the current baseline by wrapping lazy file and directory enumeration in `SafeEnumerate`.

- [ ] Reduce hot-update planning complexity in [JoltService.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Services/JoltService.cs) and [JazorRelatedDocumentResolver.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Workspace/JazorRelatedDocumentResolver.cs). Each frontend change can trigger repeated re-parse/re-resolve work across all open `.jazor` documents.

- [ ] For pure CSS changes, emit inline-style updates for dependent `.vue`/`.jazor` modules in [ChangeProcessor.cs](/D:/repository/own/jazor/Jazor/src/Jolt/DevServer/ChangeProcessor.cs) and [DenoFrontendModuleCompiler.cs](/D:/repository/own/jazor/Jazor/src/Jolt/DevServer/DenoFrontendModuleCompiler.cs). External `<style src>` changes currently refresh only CSS URLs, not already injected inline style blocks.

- [ ] Fix HMR accepted-boundary routing in [ChangeProcessor.cs](/D:/repository/own/jazor/Jazor/src/Jolt/DevServer/ChangeProcessor.cs) and [HtmlTransformer.cs](/D:/repository/own/jazor/Jazor/src/Jolt/DevServer/HtmlTransformer.cs). `AcceptedPath` is currently fixed to the changed module, so importer-level dependency accept callbacks are missed and updates degrade to full reloads.

- [ ] Add visited-path tracking and symlink/junction/reparse-point guards in [DevServerFileSnapshotPoller.cs](/D:/repository/own/jazor/Jazor/src/Jolt/DevServer/DevServerFileSnapshotPoller.cs). Current recursive polling can loop forever on cyclic links.

- [x] Include companion document state in the cache key, or bypass the normal cache when companions are present, in [OnDemandCompiler.cs](/D:/repository/own/jazor/Jazor/src/Jolt/DevServer/OnDemandCompiler.cs). Resolved by hashing companion `.cs` paths, versions, and text into the normal cache key.

- [x] Clean per-start Deno launch workspaces on normal stop/restart in [DenoWorkerProcess.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Frontend/Deno/Hosting/DenoWorkerProcess.cs). Resolved in the current baseline by calling `CleanupLaunchWorkingDirectory()` from `StopAsync` and failure paths, with `ProcessExit` retained only as a fallback.

- [ ] Harden `public/` static asset traversal against transient IO/ACL failures in [StaticAssetHandler.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Build/StaticAssetHandler.cs). Current directory traversal can fail the entire build on disappearing or inaccessible subdirectories.

- [x] Compare output modification time, not just path and length, in stabilization logic in [DenoBundleRunner.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Build/DenoBundleRunner.cs). Resolved by comparing `LastWriteTimeUtcTicks` in output file snapshots.

- [x] Fix `prepareRename` token/range detection in [LspSession.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Lsp/LspSession.cs) and [LspSession.TextAndFormatting.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Lsp/LspSession.TextAndFormatting.cs). Resolved in the current baseline by probing rename token bounds for word-end, kebab-case, and `:`-qualified tag names.

- [x] Normalize URIs before projection remap in rename flows in [VolarLaneService.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Lsp/Lanes/VolarLaneService.cs). Resolved by comparing normalized file URIs before mapping projected edits back to source edits.

- [x] Stop returning definition results for implementation requests in [JazorLaneService.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Lsp/Lanes/JazorLaneService.cs). Resolved in the current baseline by returning an empty implementation result until real logic exists.

- [x] Make `RegisterExtension` atomic or add rollback on provider registration failure in [ExtensionRegistry.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionRegistry.cs). Resolved by unregistering the extension and any providers if provider registration fails.

- [ ] Bound worker shutdown waits in [ExtensionWorkerClient.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionWorkerClient.cs). `Kill` failure currently falls through to `WaitForExitAsync(CancellationToken.None)`, which can hang disposal indefinitely.

- [ ] Propagate caller cancellation into worker stdin writes in [ExtensionWorkerClient.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionWorkerClient.cs). Request timeouts/cancellation currently cannot interrupt a blocked pipe write.

- [ ] Add framing / JSON exception boundaries in [DapServer.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Debug/DapServer.cs). Malformed stdio input can currently terminate the DAP server outright.

- [x] Decode fragmented UTF-8 safely in [CdpConnection.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Debug/CdpConnection.cs). Resolved by using a streaming UTF-8 decoder and failing truncated partial messages.

- [ ] Make block extraction syntax-aware in [JazorVueCompiler.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Jazor/Core/JazorVueCompiler.cs). Current brace counting does not skip strings/comments/raw strings and can truncate method bodies incorrectly.

- [ ] Stop doing global identifier replacements before string-aware scanning in [JazorVueCompiler.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Jazor/Core/JazorVueCompiler.cs). Current fallback lowering can mutate string literal contents.

- [x] Wrap code-behind reads in IO/ACL-safe handling in [JazorHotReloadMetadataProvider.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Roslyn/InProc/JazorHotReloadMetadataProvider.cs). Resolved in the current baseline with `SafeFileExists` and IO/ACL exception boundaries around `File.ReadAllText`.

- [ ] Add caching and tighter bounds to repeated workspace/compilation rebuild paths in [InProcRoslynCodeService.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Roslyn/InProc/InProcRoslynCodeService.cs) and [InProcRoslynCodeService.ProjectionAndContext.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Roslyn/InProc/InProcRoslynCodeService.ProjectionAndContext.cs). Hover/completion/definition currently rebuild too much for large workspaces.

## Low

- [x] Make `SupportedExtensions` case-insensitive in [DevServerFileWatchFilter.cs](/D:/repository/own/jazor/Jazor/src/Jolt/DevServer/DevServerFileWatchFilter.cs). Resolved in the current baseline via `HashSet<string>(StringComparer.OrdinalIgnoreCase)`, so mixed-case filenames such as `App.CSS` are now observed.

- [x] Respect `trimTrailingWhitespace` in [LspSession.TextAndFormatting.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Lsp/LspSession.TextAndFormatting.cs) instead of always trimming trailing spaces/tabs. Resolved in the current baseline by honoring `LspFormattingOptions.TrimTrailingWhitespace`, with regression coverage in `JoltLspSessionSliceTests`.

- [x] Pre-index open documents before directory scans in [MarkupComponentBridgeService.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Lsp/Coordination/MarkupComponentBridgeService.cs). Resolved by indexing open documents by normalized path before file iteration.

- [x] Replace `relativePath.StartsWith("..")` style boundary checks with stricter root-escape tests in [ExtensionSandboxProfile.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionSandboxProfile.cs), [ExtensionSecurityPolicy.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionSecurityPolicy.cs), [ExtensionLoader.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionLoader.cs), and [ExtensionHostOptionsResolver.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/ExtensionHostOptionsResolver.cs). Resolved by checking exact `..`, directory-separator-prefixed `..`, and rooted relative paths so in-root names like `..foo` are not false positives.

- [x] Stop falling back to the first component tag when a diagnostic cannot be precisely resolved in [ComponentCodeActionExtension.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/Builtin/ComponentCodeActionExtension.cs). Resolved by requiring either diagnostic range overlap or an explicit quoted component name in the diagnostic message.

- [x] Avoid unconditional full-text hashing on every workspace symbol query in [WorkspaceSymbolIndex.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Extensions/Builtin/WorkspaceSymbolIndex.cs). Resolved by using document version as the primary fingerprint when present and hashing only unversioned snapshots.

- [x] Harden payload kind handling for `source.path` in [DapRequestHandler.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Debug/DapRequestHandler.cs). Resolved in the current baseline by treating non-scalar nested payloads as absent instead of throwing, with regression coverage in `JoltDebugProtocolTests`.

- [x] Sort SDK versions semantically, not lexicographically, in [RazorSdkToolsetResolver.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Razor/Toolset/RazorSdkToolsetResolver.cs). Resolved in the current baseline with `SdkVersionComparer`, so values such as `10.0.100` sort after `10.0.9`.

- [x] Use an OS-appropriate comparer for code-behind path de-duplication in [JazorHotReloadMetadataProvider.cs](/D:/repository/own/jazor/Jazor/src/Jolt/Roslyn/InProc/JazorHotReloadMetadataProvider.cs). Resolved by selecting `OrdinalIgnoreCase` on Windows and `Ordinal` on case-sensitive platforms.
