using Jazor.Vite;
using Jazor.Vite.VueHost;
using Jazor.VueContracts.Protocol;
using System.Diagnostics;
using System.Text.Json;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorViteTests
{
    private static readonly JsonSerializerOptions SmokeJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [TestMethod]
    public void JazorViteOptions_Parse_RecognizesHostAndBunSettings()
    {
        var options = JazorViteOptions.Parse(
        [
            "--run-dev",
            "--bun-command=bunx",
            "--bun-args=vite dev --host",
            "--vuehost-command=dotnet",
            "--vuehost-args=run --project src/Jazor.VueHost/Jazor.VueHost.csproj -- --stdio",
            "--working-directory=src/Frontend"
        ]);

        Assert.AreEqual(JazorViteMode.RunDevServer, options.Mode);
        Assert.AreEqual("bunx", options.BunCommand);
        Assert.AreEqual("vite dev --host", options.BunArguments);
        Assert.AreEqual("dotnet", options.VueHostCommand);
        Assert.AreEqual("run --project src/Jazor.VueHost/Jazor.VueHost.csproj -- --stdio", options.VueHostArguments);
        Assert.AreEqual("src/Frontend", options.WorkingDirectory);
    }

    [TestMethod]
    public async Task JazorVite_ProcessVueHostRpcClient_GetHostInfo_InteropsWithVueHostProcess()
    {
        var hostAssemblyPath = GetBuiltAssemblyPath("Jazor.VueHost", "Jazor.VueHost.dll");
        using var client = new ProcessVueHostRpcClient(
            "dotnet",
            $"\"{hostAssemblyPath}\" --stdio");

        var hostInfo = await client.GetHostInfoAsync(CancellationToken.None);

        Assert.AreEqual("Jazor.VueHost", hostInfo.HostName);
        Assert.AreEqual("0.1", hostInfo.ProtocolVersion);
        Assert.IsTrue(hostInfo.Capabilities.Any(static capability => capability.Name == VueHostRpcMethodNames.GetHostInfo));
    }

    [TestMethod]
    public async Task JazorVite_ProcessVueHostRpcClient_GetVirtualArtifact_InteropsWithVueHostProcess()
    {
        var hostAssemblyPath = GetBuiltAssemblyPath("Jazor.VueHost", "Jazor.VueHost.dll");
        var analysisHostAssemblyPath = GetBuiltAssemblyPath("Jazor.Vue.Analysis.Host", "Jazor.Vue.Analysis.Host.dll");
        using var client = new ProcessVueHostRpcClient(
            "dotnet",
            BuildVueHostArguments(hostAssemblyPath, analysisHostAssemblyPath));

        var artifactResponse = await client.GetVirtualArtifactAsync(
            new GetVirtualArtifactRequest(
                documentPath: "Virtual/Counter.jazor",
                artifactKind: "vue-sfc",
                text:
                """
                @jsimport dayjs from "dayjs"

                <template>
                  <div>{{ dayjs }}</div>
                </template>
                """,
                version: "1"),
            CancellationToken.None);

        Assert.AreEqual("vue-sfc", artifactResponse.Artifact.ArtifactKind);
        StringAssert.Contains(artifactResponse.Artifact.Content, "<script setup>");
        StringAssert.Contains(artifactResponse.Artifact.Content, "import dayjs from \"dayjs\";");
    }

    [TestMethod]
    public async Task JazorVite_ProcessVueHostRpcClient_AnalyzeJazor_ReturnsVueArtifact()
    {
        var repositoryRoot = GetRepositoryRoot();
        var hostAssemblyPath = GetBuiltAssemblyPath("Jazor.VueHost", "Jazor.VueHost.dll");
        var analysisHostAssemblyPath = GetBuiltAssemblyPath("Jazor.Vue.Analysis.Host", "Jazor.Vue.Analysis.Host.dll");
        using var client = new ProcessVueHostRpcClient(
            "dotnet",
            BuildVueHostArguments(hostAssemblyPath, analysisHostAssemblyPath));

        var response = await client.AnalyzeJazorAsync(
            new AnalyzeJazorRequest(
                new DocumentSnapshot(
                    "Features/Counter.jazor",
                    DocumentKind.Jazor,
                    """
                    @jsimport dayjs from "dayjs"

                    <template>
                      <div>{{ dayjs }}</div>
                    </template>
                    """,
                    "vite-test"),
                relatedDocuments: Array.Empty<DocumentSnapshot>(),
                frontendContext: null),
            CancellationToken.None);

        Assert.AreEqual(1, response.Imports.Count);
        Assert.AreEqual("dayjs", response.Imports[0].LocalName);
        Assert.IsTrue(response.Artifacts.Any(static artifact => artifact.ArtifactKind == "vue-sfc"));
    }

    [TestMethod]
    public async Task JazorVite_ProcessVueHostRpcClient_ReusesProcessAcrossCallsAndPersistsWorkspaceState()
    {
        var hostAssemblyPath = GetBuiltAssemblyPath("Jazor.VueHost", "Jazor.VueHost.dll");
        using var client = new ProcessVueHostRpcClient(
            "dotnet",
            $"\"{hostAssemblyPath}\" --stdio");

        var hostInfo = await client.GetHostInfoAsync(CancellationToken.None);
        var firstProcessId = client.ProcessId;
        await client.OpenDocumentAsync(
            new DocumentSnapshot(
                "Session/Counter.jazor",
                DocumentKind.Jazor,
                "<template><div>persisted</div></template>",
                "1"),
            CancellationToken.None);
        var openDocuments = await client.GetOpenDocumentsAsync(CancellationToken.None);
        var ping = await client.PingAsync(CancellationToken.None);
        var secondProcessId = client.ProcessId;
        await client.CloseDocumentAsync("Session/Counter.jazor", CancellationToken.None);
        var finalDocuments = await client.GetOpenDocumentsAsync(CancellationToken.None);

        Assert.AreEqual("Jazor.VueHost", hostInfo.HostName);
        Assert.AreEqual("pong", ping.Message);
        Assert.IsTrue(firstProcessId.HasValue);
        Assert.AreEqual(firstProcessId, secondProcessId);
        Assert.AreEqual(1, openDocuments.Count);
        Assert.AreEqual("Session/Counter.jazor", openDocuments[0].DocumentPath);
        Assert.AreEqual(0, finalDocuments.Count);
    }

    [TestMethod]
    public async Task JazorVite_BunPlugin_Load_ReturnsVueArtifactText()
    {
        var repositoryRoot = GetRepositoryRoot();
        var hostAssemblyPath = GetBuiltAssemblyPath("Jazor.VueHost", "Jazor.VueHost.dll");
        var analysisHostAssemblyPath = GetBuiltAssemblyPath("Jazor.Vue.Analysis.Host", "Jazor.Vue.Analysis.Host.dll");
        var pluginModulePath = Path.Combine(
            repositoryRoot,
            "src",
            "Jazor.Vite",
            "src",
            "index.ts");
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), "jazor-vite-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            var jazorPath = Path.Combine(temporaryDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                jazorPath,
                """
                @vueimport UserCard from "./UserCard.vue"

                <template>
                  <UserCard />
                </template>
                """);
            var runnerPath = Path.Combine(temporaryDirectory, "runner.mjs");
            await File.WriteAllTextAsync(
                runnerPath,
                CreateBunRunnerScript(pluginModulePath, hostAssemblyPath, analysisHostAssemblyPath));

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bun",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.StartInfo.ArgumentList.Add(runnerPath);
            process.StartInfo.ArgumentList.Add(jazorPath);

            Assert.IsTrue(process.Start(), "Expected bun process to start.");

            var stdout = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync(CancellationToken.None);

            Assert.AreEqual(0, process.ExitCode, stderr);
            StringAssert.Contains(stdout, "<template>");
            StringAssert.Contains(stdout, "UserCard");
            StringAssert.Contains(stdout, "ChangedCard");
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task JazorVite_BunPlugin_Load_ReturnsSourceMap()
    {
        var repositoryRoot = GetRepositoryRoot();
        var hostAssemblyPath = GetBuiltAssemblyPath("Jazor.VueHost", "Jazor.VueHost.dll");
        var analysisHostAssemblyPath = GetBuiltAssemblyPath("Jazor.Vue.Analysis.Host", "Jazor.Vue.Analysis.Host.dll");
        var pluginModulePath = Path.Combine(
            repositoryRoot,
            "src",
            "Jazor.Vite",
            "src",
            "index.ts");
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), "jazor-vite-sourcemap-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            var jazorPath = Path.Combine(temporaryDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                jazorPath,
                """
                <template>
                  <div>sourcemap-check</div>
                </template>
                """);
            var runnerPath = Path.Combine(temporaryDirectory, "runner-sourcemap.mjs");
            await File.WriteAllTextAsync(
                runnerPath,
                CreateSourceMapRunnerScript(pluginModulePath, hostAssemblyPath, analysisHostAssemblyPath));

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bun",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.StartInfo.ArgumentList.Add(runnerPath);
            process.StartInfo.ArgumentList.Add(jazorPath);

            Assert.IsTrue(process.Start(), "Expected bun process to start.");

            var stdout = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync(CancellationToken.None);

            Assert.AreEqual(0, process.ExitCode, stderr);

            var payload = JsonSerializer.Deserialize<PluginLoadSmokeResult>(stdout.Trim(), SmokeJsonOptions);
            Assert.IsNotNull(payload);
            Assert.IsNotNull(payload.Map);
            Assert.AreEqual(3, payload.Map.Version);
            CollectionAssert.AreEqual(new[] { jazorPath.Replace("\\", "/", StringComparison.Ordinal) }, payload.Map.Sources);
            CollectionAssert.AreEqual(new[] { """
                <template>
                  <div>sourcemap-check</div>
                </template>
                """ }, payload.Map.SourcesContent);
            Assert.IsFalse(string.IsNullOrWhiteSpace(payload.Map.Mappings));
            StringAssert.Contains(payload.Code, "sourcemap-check");
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task JazorVite_BunPlugin_Load_UsesStructuredBootstrapArgsJson()
    {
        var repositoryRoot = GetRepositoryRoot();
        var hostAssemblyPath = GetBuiltAssemblyPath("Jazor.VueHost", "Jazor.VueHost.dll");
        var pluginModulePath = Path.Combine(
            repositoryRoot,
            "src",
            "Jazor.Vite",
            "src",
            "index.ts");
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), "jazor-vite-env-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            var jazorPath = Path.Combine(temporaryDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                jazorPath,
                """
                <template>
                  <div>env-json-bootstrap</div>
                </template>
                """);
            var runnerPath = Path.Combine(temporaryDirectory, "runner-env-json.mjs");
            await File.WriteAllTextAsync(
                runnerPath,
                CreateBunEnvBootstrapRunnerScript(pluginModulePath, hostAssemblyPath));

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bun",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.StartInfo.ArgumentList.Add(runnerPath);
            process.StartInfo.ArgumentList.Add(jazorPath);

            Assert.IsTrue(process.Start(), "Expected bun process to start.");

            var stdout = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync(CancellationToken.None);

            Assert.AreEqual(0, process.ExitCode, stderr);
            StringAssert.Contains(stdout, "env-json-bootstrap");
            StringAssert.Contains(stdout, "<template>");
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task JazorVite_BunPersistentSession_TracksDocumentsAndReusesHostProcess()
    {
        var repositoryRoot = GetRepositoryRoot();
        var hostAssemblyPath = GetBuiltAssemblyPath("Jazor.VueHost", "Jazor.VueHost.dll");
        var pluginModulePath = Path.Combine(
            repositoryRoot,
            "src",
            "Jazor.Vite",
            "src",
            "vue-host-session.ts");
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), "jazor-vite-session-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            var runnerPath = Path.Combine(temporaryDirectory, "runner.mjs");
            await File.WriteAllTextAsync(
                runnerPath,
                CreatePersistentSessionRunnerScript(pluginModulePath, hostAssemblyPath));

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bun",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.StartInfo.ArgumentList.Add(runnerPath);

            Assert.IsTrue(process.Start(), "Expected bun process to start.");

            var stdout = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync(CancellationToken.None);

            Assert.AreEqual(0, process.ExitCode, stderr);

            var payload = JsonSerializer.Deserialize<PersistentSessionSmokeResult>(stdout.Trim(), SmokeJsonOptions);
            Assert.IsNotNull(payload);
            Assert.AreEqual("Jazor.VueHost", payload.HostName);
            Assert.IsTrue(payload.FirstProcessId > 0);
            Assert.AreEqual(payload.FirstProcessId, payload.SecondProcessId);
            Assert.AreEqual(1, payload.OpenDocumentCount);
            Assert.AreEqual(0, payload.FinalOpenDocumentCount);
            StringAssert.Contains(payload.ArtifactContent, "<template>");
            StringAssert.Contains(payload.ArtifactContent, "SessionCard");
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task JazorVite_BunPlugin_HandleHotUpdate_InvalidatesModulesAndRefreshesArtifact()
    {
        var repositoryRoot = GetRepositoryRoot();
        var hostAssemblyPath = GetBuiltAssemblyPath("Jazor.VueHost", "Jazor.VueHost.dll");
        var pluginModulePath = Path.Combine(
            repositoryRoot,
            "src",
            "Jazor.Vite",
            "src",
            "index.ts");
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), "jazor-vite-hmr-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            var runnerPath = Path.Combine(temporaryDirectory, "runner.mjs");
            await File.WriteAllTextAsync(
                runnerPath,
                CreateHotUpdateRunnerScript(pluginModulePath, hostAssemblyPath));

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bun",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.StartInfo.ArgumentList.Add(runnerPath);

            Assert.IsTrue(process.Start(), "Expected bun process to start.");

            var stdout = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync(CancellationToken.None);

            Assert.AreEqual(0, process.ExitCode, stderr);

            var payload = JsonSerializer.Deserialize<HotUpdateSmokeResult>(stdout.Trim(), SmokeJsonOptions);
            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.InvalidatedCount);
            Assert.AreEqual(1, payload.ReturnedModulesCount);
            StringAssert.Contains(payload.InitialCode, "OriginalCard");
            StringAssert.Contains(payload.UpdatedCode, "UpdatedCard");
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task JazorVite_BunPlugin_HandleHotUpdate_UsesHostPlanForVueDependency()
    {
        var repositoryRoot = GetRepositoryRoot();
        var hostAssemblyPath = GetBuiltAssemblyPath("Jazor.VueHost", "Jazor.VueHost.dll");
        var analysisHostAssemblyPath = GetBuiltAssemblyPath("Jazor.Vue.Analysis.Host", "Jazor.Vue.Analysis.Host.dll");
        var pluginModulePath = Path.Combine(
            repositoryRoot,
            "src",
            "Jazor.Vite",
            "src",
            "index.ts");
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), "jazor-vite-vue-hmr-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            var runnerPath = Path.Combine(temporaryDirectory, "runner-vue-hmr.mjs");
            await File.WriteAllTextAsync(
                runnerPath,
                CreateFrontendDependencyHotUpdateRunnerScript(pluginModulePath, hostAssemblyPath, analysisHostAssemblyPath));

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bun",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.StartInfo.ArgumentList.Add(runnerPath);

            Assert.IsTrue(process.Start(), "Expected bun process to start.");

            var stdout = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync(CancellationToken.None);

            Assert.AreEqual(0, process.ExitCode, stderr);

            var payload = JsonSerializer.Deserialize<FrontendDependencyHotUpdateSmokeResult>(stdout.Trim(), SmokeJsonOptions);
            Assert.IsNotNull(payload);
            Assert.AreEqual(1, payload.InvalidatedCount);
            Assert.AreEqual(1, payload.ReturnedModulesCount);
            StringAssert.Contains(payload.InvalidatedModuleIds[0], "Counter.jazor");
            StringAssert.Contains(payload.ReturnedModuleIds[0], "Counter.jazor");
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task JazorVite_BunPlugin_HandleHotUpdate_RefreshesTrackedDocument()
    {
        var repositoryRoot = GetRepositoryRoot();
        var hostAssemblyPath = GetBuiltAssemblyPath("Jazor.VueHost", "Jazor.VueHost.dll");
        var analysisHostAssemblyPath = GetBuiltAssemblyPath("Jazor.Vue.Analysis.Host", "Jazor.Vue.Analysis.Host.dll");
        var pluginModulePath = Path.Combine(
            repositoryRoot,
            "src",
            "Jazor.Vite",
            "src",
            "index.ts");
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), "jazor-vite-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            var jazorPath = Path.Combine(temporaryDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                jazorPath,
                """
                <template>
                  <div>first</div>
                </template>
                """);
            var runnerPath = Path.Combine(temporaryDirectory, "runner-hot-update.mjs");
            await File.WriteAllTextAsync(
                runnerPath,
                CreateBunHotUpdateRunnerScript(pluginModulePath, hostAssemblyPath, analysisHostAssemblyPath));

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bun",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            process.StartInfo.ArgumentList.Add(runnerPath);
            process.StartInfo.ArgumentList.Add(jazorPath);

            Assert.IsTrue(process.Start(), "Expected bun process to start.");

            var stdout = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync(CancellationToken.None);

            Assert.AreEqual(0, process.ExitCode, stderr);
            StringAssert.Contains(stdout, "FIRST_LOAD_START");
            StringAssert.Contains(stdout, "first");
            StringAssert.Contains(stdout, "SECOND_LOAD_START");
            StringAssert.Contains(stdout, "second");
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
        }
    }

    private static string BuildVueHostArguments(string hostAssemblyPath, string analysisHostAssemblyPath)
        => $"\"{hostAssemblyPath}\" --stdio --analysis-client=transport --analysis-command=dotnet \"--analysis-args={analysisHostAssemblyPath} --stdio\"";

    private static string CreateBunRunnerScript(string pluginModulePath, string hostAssemblyPath, string analysisHostAssemblyPath)
    {
        var pluginPathLiteral = ToJavaScriptString(new Uri(pluginModulePath).AbsoluteUri);
        var hostArgsLiteral = ToJavaScriptString(BuildVueHostArguments(hostAssemblyPath, analysisHostAssemblyPath));

        return $$"""
        import { createJazorPlugin } from "{{pluginPathLiteral}}";

        const jazorPath = process.argv[2];
        const plugin = createJazorPlugin({
          vueHost: {
            command: "dotnet",
            args: "{{hostArgsLiteral}}",
            rpcMode: "process-stdio"
          }
        });

        await plugin.buildStart?.();
        const resolved = await plugin.resolveId?.(jazorPath);
        const first = await plugin.load?.(resolved);
        await plugin.handleHotUpdate?.({
          file: jazorPath,
          modules: [],
          server: {
            moduleGraph: {
              invalidateModule() {}
            },
            watcher: {
              on() { return this; },
              off() { return this; }
            }
          },
          read: async () => `@vueimport ChangedCard from "./ChangedCard.vue"\n\n<template>\n  <ChangedCard />\n</template>\n`
        });
        const second = await plugin.load?.(resolved);
        await plugin.buildEnd?.();
        await plugin.closeBundle?.();
        console.log(JSON.stringify({ first, second }));
        """;
    }

    private static string CreatePersistentSessionRunnerScript(string sessionModulePath, string hostAssemblyPath)
    {
        var sessionPathLiteral = ToJavaScriptString(new Uri(sessionModulePath).AbsoluteUri);
        var hostArgsLiteral = ToJavaScriptString($"\"{hostAssemblyPath}\" --stdio");

        return $$"""
        import { createPersistentVueHostSession } from "{{sessionPathLiteral}}";

        const session = createPersistentVueHostSession({
          command: "dotnet",
          args: "{{hostArgsLiteral}}",
          rpcMode: "process-stdio"
        });

        const hostInfo = await session.getHostInfo();
        const firstProcessId = session.processId;
        await session.openDocument({
          documentPath: "Session/Counter.jazor",
          documentKind: "Jazor",
          text: "@vueimport SessionCard from \"./SessionCard.vue\"\n\n<template>\n  <SessionCard />\n</template>",
          version: "1"
        });

        const openDocuments = await session.getOpenDocuments();
        const artifactResponse = await session.getVirtualArtifact({
          documentPath: "Session/Counter.jazor",
          artifactKind: "vue-sfc",
          text: null,
          version: null
        });

        await session.closeDocument("Session/Counter.jazor");
        const finalDocuments = await session.getOpenDocuments();
        const secondProcessId = session.processId;
        await session.dispose();

        console.log(JSON.stringify({
          hostName: hostInfo.hostName,
          firstProcessId,
          secondProcessId,
          openDocumentCount: openDocuments.length,
          finalOpenDocumentCount: finalDocuments.length,
          artifactContent: artifactResponse.artifact.content
        }));
        """;
    }

    private static string CreateBunEnvBootstrapRunnerScript(string pluginModulePath, string hostAssemblyPath)
    {
        var pluginPathLiteral = ToJavaScriptString(new Uri(pluginModulePath).AbsoluteUri);
        var hostPathLiteral = ToJavaScriptString(hostAssemblyPath);

        return $$"""
        import { createJazorPlugin } from "{{pluginPathLiteral}}";

        process.env.JAZOR_VUEHOST_COMMAND = "dotnet";
        process.env.JAZOR_VUEHOST_ARGS = "";
        process.env.JAZOR_VUEHOST_ARGS_JSON = JSON.stringify(["{{hostPathLiteral}}", "--stdio"]);
        process.env.JAZOR_VUEHOST_RPC_MODE = "process-stdio";

        const jazorPath = process.argv[2];
        const plugin = createJazorPlugin();

        await plugin.buildStart?.();
        const resolved = await plugin.resolveId?.(jazorPath);
        const result = await plugin.load?.(resolved);
        await plugin.buildEnd?.();
        await plugin.closeBundle?.();
        console.log(typeof result === "string" ? result : result?.code ?? "");
        """;
    }

    private static string CreateSourceMapRunnerScript(string pluginModulePath, string hostAssemblyPath, string analysisHostAssemblyPath)
    {
        var pluginPathLiteral = ToJavaScriptString(new Uri(pluginModulePath).AbsoluteUri);
        var hostArgsLiteral = ToJavaScriptString(BuildVueHostArguments(hostAssemblyPath, analysisHostAssemblyPath));

        return $$"""
        import { createJazorPlugin } from "{{pluginPathLiteral}}";

        const jazorPath = process.argv[2];
        const plugin = createJazorPlugin({
          vueHost: {
            command: "dotnet",
            args: "{{hostArgsLiteral}}",
            rpcMode: "process-stdio"
          }
        });

        await plugin.buildStart?.();
        const resolved = await plugin.resolveId?.(jazorPath);
        const result = await plugin.load?.(resolved);
        await plugin.buildEnd?.();
        await plugin.closeBundle?.();
        console.log(JSON.stringify(result));
        """;
    }

    private static string CreateHotUpdateRunnerScript(string pluginModulePath, string hostAssemblyPath)
    {
        var pluginPathLiteral = ToJavaScriptString(new Uri(pluginModulePath).AbsoluteUri);
        var hostArgsLiteral = ToJavaScriptString($"\"{hostAssemblyPath}\" --stdio");

        return $$"""
        import { mkdtemp, writeFile } from "node:fs/promises";
        import { tmpdir } from "node:os";
        import { join } from "node:path";
        import { createJazorPlugin } from "{{pluginPathLiteral}}";

        const tempDir = await mkdtemp(join(tmpdir(), "jazor-vite-hmr-run-"));
        const filePath = join(tempDir, "Counter.jazor");
        await writeFile(filePath, "@vueimport OriginalCard from \"./OriginalCard.vue\"\n\n<template>\n  <OriginalCard />\n</template>", "utf8");

        const invalidated = [];
        const moduleNode = {};
        const plugin = createJazorPlugin({
          vueHost: {
            command: "dotnet",
            args: "{{hostArgsLiteral}}",
            rpcMode: "process-stdio"
          }
        });

        const watcherHandlers = new Map();
        const server = {
          moduleGraph: {
            invalidateModule(mod) {
              invalidated.push(mod);
            }
          },
          watcher: {
            on(event, handler) {
              watcherHandlers.set(event, handler);
              return this;
            },
            off(event) {
              watcherHandlers.delete(event);
              return this;
            }
          }
        };

        await plugin.buildStart?.();
        await plugin.configureServer?.(server);
        const resolved = await plugin.resolveId?.(filePath);
        const initialLoad = await plugin.load?.(resolved);
        const initialCode = typeof initialLoad === "string" ? initialLoad : initialLoad?.code ?? "";

        const updatedText = "@vueimport UpdatedCard from \"./UpdatedCard.vue\"\n\n<template>\n  <UpdatedCard />\n</template>";
        await writeFile(filePath, updatedText, "utf8");
        const updatedModules = await plugin.handleHotUpdate?.({
          file: filePath,
          modules: [moduleNode],
          server,
          read: async () => updatedText
        });

        const updatedLoad = await plugin.load?.(resolved);
        const updatedCode = typeof updatedLoad === "string" ? updatedLoad : updatedLoad?.code ?? "";
        await plugin.closeBundle?.();

        console.log(JSON.stringify({
          invalidatedCount: invalidated.length,
          returnedModulesCount: updatedModules?.length ?? 0,
          initialCode,
          updatedCode
        }));
        """;
    }

    private static string CreateFrontendDependencyHotUpdateRunnerScript(string pluginModulePath, string hostAssemblyPath, string analysisHostAssemblyPath)
    {
        var pluginPathLiteral = ToJavaScriptString(new Uri(pluginModulePath).AbsoluteUri);
        var hostArgsLiteral = ToJavaScriptString(BuildVueHostArguments(hostAssemblyPath, analysisHostAssemblyPath));

        return $$"""
        import { mkdtemp, writeFile } from "node:fs/promises";
        import { tmpdir } from "node:os";
        import { join } from "node:path";
        import { createJazorPlugin } from "{{pluginPathLiteral}}";

        const tempDir = await mkdtemp(join(tmpdir(), "jazor-vite-vue-hmr-run-"));
        const jazorPath = join(tempDir, "Counter.jazor");
        const vuePath = join(tempDir, "UserCard.vue");
        await writeFile(jazorPath, "@vueimport UserCard from \"./UserCard.vue\"\\n\\n<template>\\n  <UserCard />\\n</template>", "utf8");
        await writeFile(vuePath, "<template><div>first</div></template>", "utf8");

        const plugin = createJazorPlugin({
          vueHost: {
            command: "dotnet",
            args: "{{hostArgsLiteral}}",
            rpcMode: "process-stdio"
          }
        });

        const moduleNode = { id: "\0jazor:" + jazorPath.replace(/\\\\/g, "/") };
        const invalidated = [];
        const server = {
          moduleGraph: {
            getModuleById(id) {
              return id === moduleNode.id ? moduleNode : null;
            },
            invalidateModule(node) {
              invalidated.push(node.id ?? "");
            }
          },
          watcher: {
            on() { return this; },
            off() { return this; }
          }
        };

        await plugin.buildStart?.();
        await plugin.configureServer?.(server);
        const resolved = await plugin.resolveId?.(jazorPath);
        await plugin.load?.(resolved);

        const updatedText = "<template><div>second</div></template>";
        await writeFile(vuePath, updatedText, "utf8");
        const returnedModules = await plugin.handleHotUpdate?.({
          file: vuePath,
          modules: [],
          server,
          read: async () => updatedText
        });
        await plugin.buildEnd?.();

        console.log(JSON.stringify({
          invalidatedCount: invalidated.length,
          returnedModulesCount: returnedModules?.length ?? 0,
          invalidatedModuleIds: invalidated,
          returnedModuleIds: (returnedModules ?? []).map((module) => module.id ?? "")
        }));
        """;
    }

    private static string CreateBunHotUpdateRunnerScript(string pluginModulePath, string hostAssemblyPath, string analysisHostAssemblyPath)
    {
        var pluginPathLiteral = ToJavaScriptString(new Uri(pluginModulePath).AbsoluteUri);
        var hostArgsLiteral = ToJavaScriptString(BuildVueHostArguments(hostAssemblyPath, analysisHostAssemblyPath));

        return $$"""
        import { createJazorPlugin } from "{{pluginPathLiteral}}";
        import { writeFile } from "node:fs/promises";

        const jazorPath = process.argv[2];
        const plugin = createJazorPlugin({
          vueHost: {
            command: "dotnet",
            args: "{{hostArgsLiteral}}",
            rpcMode: "process-stdio"
          }
        });

        const invalidated = [];
        const moduleNode = { id: "\0jazor:" + jazorPath };
        const server = {
          moduleGraph: {
            getModuleById(id) {
              return id === moduleNode.id ? moduleNode : undefined;
            },
            invalidateModule(node) {
              invalidated.push(node.id);
            }
          }
        };

        const resolved = await plugin.resolveId?.(jazorPath);
        const first = await plugin.load?.(resolved);
        console.log("FIRST_LOAD_START");
        console.log(first);
        console.log("FIRST_LOAD_END");

        await writeFile(jazorPath, `<template>\n  <div>second</div>\n</template>\n`, "utf8");
        const hotModules = await plugin.handleHotUpdate?.({
          file: jazorPath,
          server,
          modules: []
        });
        const second = await plugin.load?.(resolved);
        console.log("SECOND_LOAD_START");
        console.log(second);
        console.log("SECOND_LOAD_END");
        console.log(JSON.stringify({ invalidated, hotModulesLength: hotModules?.length ?? 0 }));
        await plugin.buildEnd?.();
        """;
    }

    private static string ToJavaScriptString(string value)
        => value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);

    private static string GetBuiltAssemblyPath(string projectDirectoryName, string assemblyFileName)
    {
        var assemblyPath = Path.Combine(
            GetRepositoryRoot(),
            "src",
            projectDirectoryName,
            "bin",
            "Debug",
            "net10.0",
            assemblyFileName);
        Assert.IsTrue(File.Exists(assemblyPath), $"Expected built assembly '{assemblyPath}' to exist.");
        return assemblyPath;
    }

    private static string GetRepositoryRoot()
        => Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            ".."));

    private sealed class PersistentSessionSmokeResult
    {
        public string HostName { get; set; } = string.Empty;

        public int FirstProcessId { get; set; }

        public int SecondProcessId { get; set; }

        public int OpenDocumentCount { get; set; }

        public int FinalOpenDocumentCount { get; set; }

        public string ArtifactContent { get; set; } = string.Empty;
    }

    private sealed class HotUpdateSmokeResult
    {
        public int InvalidatedCount { get; set; }

        public int ReturnedModulesCount { get; set; }

        public string InitialCode { get; set; } = string.Empty;

        public string UpdatedCode { get; set; } = string.Empty;
    }

    private sealed class FrontendDependencyHotUpdateSmokeResult
    {
        public int InvalidatedCount { get; set; }

        public int ReturnedModulesCount { get; set; }

        public string[] InvalidatedModuleIds { get; set; } = [];

        public string[] ReturnedModuleIds { get; set; } = [];
    }

    private sealed class PluginLoadSmokeResult
    {
        public string Code { get; set; } = string.Empty;

        public PluginLoadSourceMapSmokeResult? Map { get; set; }
    }

    private sealed class PluginLoadSourceMapSmokeResult
    {
        public int Version { get; set; }

        public string[] Sources { get; set; } = [];

        public string[] SourcesContent { get; set; } = [];

        public string Mappings { get; set; } = string.Empty;
    }
}
