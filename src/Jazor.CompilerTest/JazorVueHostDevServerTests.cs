using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Jazor.Emit;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.DevServer;
using Jazor.VueHost.SourceMap;
using Jazor.VueHost.Workspace;
using static Jazor.CompilerTest.SourceMapTestHelpers;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostDevServerTests
{
    [TestMethod]
    public void ModuleResolver_Resolve_RootPath_ReturnsIndexHtml()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var indexPath = Path.Combine(rootDirectory, "index.html");
            File.WriteAllText(indexPath, "<html></html>");
            var resolver = new ModuleResolver(rootDirectory);

            var result = resolver.Resolve("/");

            Assert.IsTrue(result.Found);
            Assert.AreEqual(indexPath, result.AbsolutePath);
            Assert.AreEqual("/index.html", result.ResolvedUrl);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void ModuleResolver_Resolve_RelativeImport_UsesImporterDirectory()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var scriptsDirectory = Path.Combine(rootDirectory, "features");
            Directory.CreateDirectory(scriptsDirectory);
            var componentPath = Path.Combine(scriptsDirectory, "Counter.jazor");
            File.WriteAllText(componentPath, "<div />");

            var resolver = new ModuleResolver(rootDirectory);
            var result = resolver.Resolve("./Counter", Path.Combine(scriptsDirectory, "main.js"));

            Assert.IsTrue(result.Found);
            Assert.AreEqual(componentPath, result.AbsolutePath);
            Assert.AreEqual("/features/Counter.jazor", result.ResolvedUrl);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void ModuleResolver_Resolve_AliasImport_UsesAliasTargetDirectory()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var scriptsDirectory = Path.Combine(rootDirectory, "src");
            Directory.CreateDirectory(scriptsDirectory);
            var componentPath = Path.Combine(scriptsDirectory, "Counter.jazor");
            File.WriteAllText(componentPath, "<div />");

            var resolver = new ModuleResolver(
                rootDirectory,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["@"] = "/src"
                });
            var result = resolver.Resolve("@/Counter", Path.Combine(rootDirectory, "main.ts"));

            Assert.IsTrue(result.Found);
            Assert.AreEqual(componentPath, result.AbsolutePath);
            Assert.AreEqual("/src/Counter.jazor", result.ResolvedUrl);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void ModuleResolver_Resolve_AliasOutsideRoot_ReturnsNotFound()
    {
        var rootDirectory = CreateTemporaryDirectory();
        var resolver = new ModuleResolver(
            rootDirectory,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["@"] = "../shared"
            });

        try
        {
            var result = resolver.Resolve("@/Counter.ts");

            Assert.IsFalse(result.Found);
            StringAssert.Contains(result.Error!, "escapes the dev-server root");
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void ModuleResolver_Resolve_StripsQueryAndHash()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            File.WriteAllText(documentPath, "<div />");
            var resolver = new ModuleResolver(rootDirectory);

            var result = resolver.Resolve("/Counter.jazor?t=123#render");

            Assert.IsTrue(result.Found);
            Assert.AreEqual(documentPath, result.AbsolutePath);
            Assert.AreEqual("/Counter.jazor", result.ResolvedUrl);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void ModuleResolver_Resolve_VirtualClientPath_ReturnsVirtualResult()
    {
        var rootDirectory = CreateTemporaryDirectory();
        var resolver = new ModuleResolver(rootDirectory);

        try
        {
            var result = resolver.Resolve("/@jazor/client");

            Assert.IsTrue(result.Found);
            Assert.IsTrue(result.IsVirtual);
            Assert.AreEqual("/@jazor/client", result.AbsolutePath);
            Assert.AreEqual("/@jazor/client", result.ResolvedUrl);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void ModuleResolver_Resolve_PathEscapesRoot_ReturnsNotFound()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var scriptsDirectory = Path.Combine(rootDirectory, "features");
            Directory.CreateDirectory(scriptsDirectory);
            var importerPath = Path.Combine(scriptsDirectory, "main.js");
            File.WriteAllText(importerPath, "export {};");

            var resolver = new ModuleResolver(rootDirectory);
            var result = resolver.Resolve("../../outside.js", importerPath);

            Assert.IsFalse(result.Found);
            StringAssert.Contains(result.Error!, "escapes the dev-server root");
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void DevServerOptionsParser_Parse_CollectsProxyRules()
    {
        var options = DevServerOptionsParser.Parse(
        [
            "--dev-proxy=/api=http://localhost:5000",
            "--dev-proxy=/auth=https://example.com/base"
        ]);

        Assert.AreEqual(2, options.ProxyRules.Count);
        Assert.AreEqual("http://localhost:5000", options.ProxyRules["/api"].Target);
        Assert.AreEqual("https://example.com/base", options.ProxyRules["/auth"].Target);
    }

    [TestMethod]
    public void DevServerOptionsParser_Parse_CollectsAliasRules()
    {
        var options = DevServerOptionsParser.Parse(
        [
            "--dev-alias=@=/src",
            "--dev-alias=@shared=./shared"
        ]);

        Assert.AreEqual(2, options.ResolveAliases.Count);
        Assert.AreEqual("/src", options.ResolveAliases["@"]);
        Assert.AreEqual("./shared", options.ResolveAliases["@shared"]);
    }

    [TestMethod]
    public void DevServerOptionsParser_Parse_LoadsServerAndProxyRulesFromJazorConfig()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            File.WriteAllText(
                Path.Combine(rootDirectory, "jazor.config.json"),
                """
                {
                  "server": {
                    "port": 6123,
                    "host": "127.0.0.1",
                    "open": true,
                    "hmr": false
                  },
                  "proxy": {
                    "/api": {
                      "target": "https://backend.example/base",
                      "secure": true,
                      "websocket": false,
                      "rewritePath": "/gateway"
                    }
                  },
                  "resolve": {
                    "alias": {
                      "@": "/src",
                      "@shared": "./shared"
                    }
                  }
                }
                """);

            var options = DevServerOptionsParser.Parse(
            [
                $"--dev-root={rootDirectory}"
            ]);

            Assert.AreEqual(Path.GetFullPath(rootDirectory), options.RootDirectory);
            Assert.AreEqual(6123, options.Port);
            Assert.AreEqual("127.0.0.1", options.Host);
            Assert.IsTrue(options.OpenBrowser);
            Assert.IsFalse(options.HmrEnabled);
            Assert.AreEqual(1, options.ProxyRules.Count);
            Assert.AreEqual("https://backend.example/base", options.ProxyRules["/api"].Target);
            Assert.IsTrue(options.ProxyRules["/api"].Secure);
            Assert.IsFalse(options.ProxyRules["/api"].WebSocket);
            Assert.AreEqual("/gateway", options.ProxyRules["/api"].RewritePath);
            Assert.AreEqual(2, options.ResolveAliases.Count);
            Assert.AreEqual("/src", options.ResolveAliases["@"]);
            Assert.AreEqual("./shared", options.ResolveAliases["@shared"]);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void DevServerOptionsParser_Parse_CommandLineOverridesJazorConfig()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            File.WriteAllText(
                Path.Combine(rootDirectory, "jazor.config.json"),
                """
                {
                  "server": {
                    "port": 6123,
                    "host": "127.0.0.1",
                    "open": false,
                    "hmr": true
                  },
                  "proxy": {
                    "/api": {
                      "target": "https://backend.example/base",
                      "websocket": false
                    }
                  },
                  "resolve": {
                    "alias": {
                      "@": "/src",
                      "@shared": "./shared"
                    }
                  }
                }
                """);

            var options = DevServerOptionsParser.Parse(
            [
                $"--dev-root={rootDirectory}",
                "--dev-port=7001",
                "--dev-host=0.0.0.0",
                "--open-browser",
                "--no-hmr",
                "--dev-frontend=stub",
                "--dev-proxy=/api=http://localhost:5000",
                "--dev-alias=@=/client-src"
            ]);

            Assert.AreEqual(7001, options.Port);
            Assert.AreEqual("0.0.0.0", options.Host);
            Assert.IsTrue(options.OpenBrowser);
            Assert.IsFalse(options.HmrEnabled);
            Assert.AreEqual("stub", options.FrontendCompiler);
            Assert.AreEqual(1, options.ProxyRules.Count);
            Assert.AreEqual("http://localhost:5000", options.ProxyRules["/api"].Target);
            Assert.IsTrue(options.ProxyRules["/api"].WebSocket);
            Assert.AreEqual("/client-src", options.ResolveAliases["@"]);
            Assert.AreEqual("./shared", options.ResolveAliases["@shared"]);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void DevServerFileWatchFilter_ShouldObserve_SupportedSourceFileUnderRoot()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var filePath = Path.Combine(rootDirectory, "Features", "Counter.jazor");

            Assert.IsTrue(DevServerFileWatchFilter.ShouldObserve(rootDirectory, filePath));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void DevServerFileWatchFilter_ShouldObserve_JsonFileUnderRoot()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var filePath = Path.Combine(rootDirectory, "jazor.config.json");

            Assert.IsTrue(DevServerFileWatchFilter.ShouldObserve(rootDirectory, filePath));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void DevServerFileWatchFilter_ShouldObserve_JazorCodeBehindFileUnderRoot()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Features", "Counter.jazor");
            Directory.CreateDirectory(Path.GetDirectoryName(documentPath)!);
            File.WriteAllText(documentPath, "<div />");

            var filePath = Path.Combine(rootDirectory, "Features", "Counter.jazor.cs");

            Assert.IsTrue(DevServerFileWatchFilter.ShouldObserve(rootDirectory, filePath));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void DevServerFileWatchFilter_ShouldIgnore_UnrelatedCSharpFileUnderRoot()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var filePath = Path.Combine(rootDirectory, "Features", "Helpers.cs");

            Assert.IsFalse(DevServerFileWatchFilter.ShouldObserve(rootDirectory, filePath));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void DevServerFileWatchFilter_ShouldIgnore_JazorCodeBehindUnderIgnoredDirectory()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "obj", "Generated", "Counter.jazor");
            Directory.CreateDirectory(Path.GetDirectoryName(documentPath)!);
            File.WriteAllText(documentPath, "<div />");

            var filePath = Path.Combine(rootDirectory, "obj", "Generated", "Counter.jazor.cs");

            Assert.IsFalse(DevServerFileWatchFilter.ShouldObserve(rootDirectory, filePath));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void DevServerFileWatchFilter_ShouldIgnore_BuildAndPackageDirectoriesAndUnsupportedExtensions()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var objPath = Path.Combine(rootDirectory, "obj", "Debug", "generated.jazor");
            var packagePath = Path.Combine(rootDirectory, "node_modules", "vue", "dist", "vue.js");
            var binaryPath = Path.Combine(rootDirectory, "artifacts", "bundle.dll");

            Assert.IsFalse(DevServerFileWatchFilter.ShouldObserve(rootDirectory, objPath));
            Assert.IsFalse(DevServerFileWatchFilter.ShouldObserve(rootDirectory, packagePath));
            Assert.IsFalse(DevServerFileWatchFilter.ShouldObserve(rootDirectory, binaryPath));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevServerProxy_TryProxyAsync_ForwardsPostRequestBodyQueryAndResponse()
    {
        var handler = new CapturingHttpMessageHandler();
        using var proxy = new DevServerProxy(
            new Dictionary<string, ProxyTarget>(StringComparer.OrdinalIgnoreCase)
            {
                ["/api"] = new()
                {
                    Target = "http://upstream.test/base"
                }
            },
            handler);
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/todos";
        context.Request.QueryString = new QueryString("?page=1");
        context.Request.Headers.ContentType = "application/json";
        context.Request.Body = new MemoryStream("""{"title":"Ship"}"""u8.ToArray());
        context.Request.ContentLength = context.Request.Body.Length;
        context.Response.Body = new MemoryStream();

        var proxied = await proxy.TryProxyAsync(context);

        Assert.IsTrue(proxied);
        Assert.IsNotNull(handler.LastRequest);
        Assert.AreEqual(HttpMethod.Post, handler.LastRequest.Method);
        Assert.AreEqual("http://upstream.test/base/todos?page=1", handler.LastRequest.RequestUri!.ToString());
        Assert.AreEqual("""{"title":"Ship"}""", handler.LastBody);
        Assert.AreEqual((int)HttpStatusCode.Accepted, context.Response.StatusCode);
        Assert.AreEqual("application/json", context.Response.Headers.ContentType.ToString());
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        Assert.AreEqual("""{"ok":true}""", await reader.ReadToEndAsync());
    }

    [TestMethod]
    public async Task DevServerProxy_TryProxyAsync_WhenNoRuleMatches_ReturnsFalse()
    {
        var handler = new CapturingHttpMessageHandler();
        using var proxy = new DevServerProxy(
            new Dictionary<string, ProxyTarget>(StringComparer.OrdinalIgnoreCase)
            {
                ["/api"] = new()
                {
                    Target = "http://upstream.test"
                }
            },
            handler);
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/main.ts";
        context.Response.Body = new MemoryStream();

        var proxied = await proxy.TryProxyAsync(context);

        Assert.IsFalse(proxied);
        Assert.IsNull(handler.LastRequest);
    }

    [TestMethod]
    public void HtmlTransformer_Transform_InsertsImportMapAndClientBeforeHeadClose()
    {
        var transformer = new HtmlTransformer(new DevServerOptions());

        var result = transformer.Transform("<html><head></head><body></body></html>");

        StringAssert.Contains(result, "type=\"importmap\"");
        StringAssert.Contains(result, "https://esm.sh/vue@3?dev");
        Assert.IsTrue(result.IndexOf("/@jazor/client", StringComparison.Ordinal) < result.IndexOf("</head>", StringComparison.Ordinal));
    }

    [TestMethod]
    public void HtmlTransformer_Transform_RewritesLocalTypeScriptEntryToModuleScript()
    {
        var transformer = new HtmlTransformer(new DevServerOptions());

        var result = transformer.Transform("<html><body><script src=\"/main.ts\"></script></body></html>");

        StringAssert.Contains(result, "<script type=\"module\" src=\"/main.ts\">");
    }

    [TestMethod]
    public void HtmlTransformer_Transform_DoesNotRewriteExternalOrTypedScripts()
    {
        var transformer = new HtmlTransformer(new DevServerOptions());

        var result = transformer.Transform("""
            <html><body>
            <script src="https://cdn.example.com/app.js"></script>
            <script type="module" src="/main.ts"></script>
            </body></html>
            """);

        Assert.AreEqual(1, CountOccurrences(result, "type=\"module\" src=\"/main.ts\""));
        Assert.AreEqual(0, CountOccurrences(result, "type=\"module\" src=\"https://cdn.example.com/app.js\""));
    }

    [TestMethod]
    public void HtmlTransformer_Transform_WhenHmrDisabled_DoesNotInjectClient()
    {
        var transformer = new HtmlTransformer(new DevServerOptions { HmrEnabled = false });

        var result = transformer.Transform("<html><body></body></html>");

        StringAssert.Contains(result, "type=\"importmap\"");
        Assert.IsFalse(result.Contains("/@jazor/client", StringComparison.Ordinal));
    }

    [TestMethod]
    public void HtmlTransformer_Transform_WithoutHeadOrBody_PrependsInjection()
    {
        var transformer = new HtmlTransformer(new DevServerOptions());

        var result = transformer.Transform("<div>app</div>");

        Assert.IsTrue(result.StartsWith("<script type=\"importmap\">", StringComparison.Ordinal));
        Assert.IsTrue(result.EndsWith("<div>app</div>", StringComparison.Ordinal));
    }

    [TestMethod]
    public void HtmlTransformer_GetDevClientScript_ContainsReloadSocketEndpoint()
    {
        var script = HtmlTransformer.GetDevClientScript();

        StringAssert.Contains(script, "/@jazor/hmr");
        StringAssert.Contains(script, "location.reload()");
        StringAssert.Contains(script, "new WebSocket");
        StringAssert.Contains(script, "style-update");
        StringAssert.Contains(script, "full-reload");
        StringAssert.Contains(script, "connected");
        StringAssert.Contains(script, "js-update");
        StringAssert.Contains(script, "__JAZOR_HMR__");
        StringAssert.Contains(script, "createHotContext");
        StringAssert.Contains(script, "link[rel=\"stylesheet\"][href]");
        StringAssert.Contains(script, "inlineStyles");
        StringAssert.Contains(script, "scheduleReconnect");
        StringAssert.Contains(script, "setTimeout(() =>");
        StringAssert.Contains(script, "connect();");
        StringAssert.Contains(script, "type: \"ready\"");
        StringAssert.Contains(script, "type: \"heartbeat\"");
        StringAssert.Contains(script, "startHeartbeat()");
        StringAssert.Contains(script, "stopHeartbeat();");
        StringAssert.Contains(script, "showErrorOverlay");
        StringAssert.Contains(script, "__jazor-error-overlay");
        StringAssert.Contains(script, "payload?.type === \"error\"");
    }

    [TestMethod]
    public void DevServerNotificationEnvelope_Serialize_UsesClientFacingPropertyNames()
    {
        var payload = new DevServerNotificationEnvelope
        {
            Type = "style-update",
            Paths = ["/site.css"],
            InlineStyles =
            [
                new InlineStyleUpdate
                {
                    TargetId = "/Counter.vue",
                    Content = ".counter { color: red; }"
                }
            ],
            Timestamp = 123L
        };

        var json = JsonSerializer.Serialize(payload);

        StringAssert.Contains(json, "\"type\":\"style-update\"");
        StringAssert.Contains(json, "\"paths\":[\"/site.css\"]");
        StringAssert.Contains(json, "\"inlineStyles\":[{");
        StringAssert.Contains(json, "\"path\":\"/Counter.vue\"");
        StringAssert.Contains(json, "\"content\":\".counter { color: red; }\"");
        StringAssert.Contains(json, "\"timestamp\":123");
        Assert.IsFalse(json.Contains("\"Type\"", StringComparison.Ordinal));
        Assert.IsFalse(json.Contains("\"InlineStyles\"", StringComparison.Ordinal));
    }

    [TestMethod]
    public void DevServerNotificationEnvelope_Serialize_FullReloadIncludesReason()
    {
        var payload = new DevServerNotificationEnvelope
        {
            Type = "full-reload",
            Reason = "index-html-change"
        };

        var json = JsonSerializer.Serialize(payload);

        StringAssert.Contains(json, "\"type\":\"full-reload\"");
        StringAssert.Contains(json, "\"reason\":\"index-html-change\"");
    }

    [TestMethod]
    public void DevServerNotificationEnvelope_Serialize_ErrorIncludesMessage()
    {
        var payload = new DevServerNotificationEnvelope
        {
            Type = "error",
            Message = "Vue SFC compilation is not available because the frontend compiler is unavailable."
        };

        var json = JsonSerializer.Serialize(payload);

        StringAssert.Contains(json, "\"type\":\"error\"");
        StringAssert.Contains(json, "\"message\":\"Vue SFC compilation is not available because the frontend compiler is unavailable.\"");
    }

    [TestMethod]
    public void DevServerNotificationEnvelope_Serialize_JavaScriptUpdatesUsesViteCompatibleShape()
    {
        var payload = new DevServerNotificationEnvelope
        {
            Type = "update",
            Updates =
            [
                new JavaScriptHotUpdate
                {
                    Path = "/Counter.vue",
                    AcceptedPath = "/Counter.vue"
                }
            ],
            Timestamp = 123L
        };

        var json = JsonSerializer.Serialize(payload);

        StringAssert.Contains(json, "\"type\":\"update\"");
        StringAssert.Contains(json, "\"updates\":[{");
        StringAssert.Contains(json, "\"type\":\"js-update\"");
        StringAssert.Contains(json, "\"path\":\"/Counter.vue\"");
        StringAssert.Contains(json, "\"acceptedPath\":\"/Counter.vue\"");
        StringAssert.Contains(json, "\"timestamp\":123");
    }

    [TestMethod]
    public void DependencyGraph_Record_ReplacesPreviousDependenciesAndDependents()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var srcDirectory = Path.Combine(rootDirectory, "src");
            Directory.CreateDirectory(srcDirectory);
            var modulePath = Path.Combine(srcDirectory, "main.ts");
            var counterPath = Path.Combine(srcDirectory, "Counter.vue");
            var appPath = Path.Combine(srcDirectory, "App.vue");
            File.WriteAllText(modulePath, "export {};");
            File.WriteAllText(counterPath, "<template />");
            File.WriteAllText(appPath, "<template />");

            var graph = new DependencyGraph(new ModuleResolver(rootDirectory));

            graph.Record(modulePath, ["vue", "./Counter.vue", "https://esm.sh/vue@3"]);
            graph.Record(modulePath, ["./Counter.vue", "/src/App.vue"]);

            CollectionAssert.AreEqual(
                new[] { appPath, counterPath },
                graph.GetDependencies(modulePath).ToArray());
            CollectionAssert.AreEqual(
                new[] { modulePath },
                graph.GetDependents(counterPath).ToArray());
            Assert.AreEqual(0, graph.GetDependents("vue").Count);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void DenoFrontendModuleCompiler_ExtractJavaScriptDependencies_ReturnsUniqueStaticAndDynamicSpecifiers()
    {
        var dependencies = DenoFrontendModuleCompiler.ExtractJavaScriptDependencies(
            """
            import { ref } from "vue";
            import "./counter.css";
            export { mount } from "./mount.ts";
            const loader = () => import("./async.ts");
            import { ref as refAgain } from "vue";
            """);

        CollectionAssert.AreEqual(
            new[] { "vue", "./counter.css", "./mount.ts", "./async.ts" },
            dependencies.ToArray());
    }

    [TestMethod]
    public void DependencyGraph_GetAllAffectedModules_ReturnsTransitiveDependents()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var srcDirectory = Path.Combine(rootDirectory, "src");
            Directory.CreateDirectory(srcDirectory);
            var mainPath = Path.Combine(srcDirectory, "main.ts");
            var appPath = Path.Combine(srcDirectory, "App.vue");
            var childPath = Path.Combine(srcDirectory, "Child.vue");
            File.WriteAllText(mainPath, "export {};");
            File.WriteAllText(appPath, "<template />");
            File.WriteAllText(childPath, "<template />");

            var graph = new DependencyGraph(new ModuleResolver(rootDirectory));
            graph.Record(mainPath, ["./App.vue"]);
            graph.Record(appPath, ["./Child.vue"]);

            CollectionAssert.AreEqual(
                new[] { appPath, mainPath },
                graph.GetAllAffectedModules(childPath).ToArray());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task FileChangeDebouncer_Record_BatchesDistinctPathsIntoSingleNotification()
    {
        using var debouncer = new FileChangeDebouncer(TimeSpan.FromMilliseconds(25));
        var changesReceived = new TaskCompletionSource<IReadOnlyList<string>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstPath = Path.Combine(Path.GetTempPath(), "debouncer", "App.vue");
        var secondPath = Path.Combine(Path.GetTempPath(), "debouncer", "Child.vue");
        debouncer.DebouncedChange += changes =>
        {
            changesReceived.TrySetResult(changes);
        };

        debouncer.Record(firstPath);
        debouncer.Record(secondPath);
        debouncer.Record(firstPath);

        var changes = await changesReceived.Task.WaitAsync(TimeSpan.FromSeconds(2));

        CollectionAssert.AreEqual(
            new[] { Path.GetFullPath(firstPath), Path.GetFullPath(secondPath) },
            changes.ToArray());
    }

    [TestMethod]
    public async Task FileChangeDebouncer_Record_AfterDispose_IsIgnored()
    {
        using var debouncer = new FileChangeDebouncer(TimeSpan.FromMilliseconds(25));
        var notificationReceived = false;
        debouncer.DebouncedChange += _ => notificationReceived = true;

        debouncer.Dispose();
        debouncer.Record(Path.Combine(Path.GetTempPath(), "debouncer", "App.vue"));

        await Task.Delay(TimeSpan.FromMilliseconds(100));
        Assert.IsFalse(notificationReceived);
    }

    [TestMethod]
    public void DevServerFileSnapshotPoller_GetChangedPaths_DetectsCreateModifyDeleteAndIgnoresFilteredFiles()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var existingPath = Path.Combine(rootDirectory, "App.vue");
            var deletedPath = Path.Combine(rootDirectory, "Old.ts");
            var ignoredPath = Path.Combine(rootDirectory, "node_modules", "pkg", "index.js");
            Directory.CreateDirectory(Path.GetDirectoryName(ignoredPath)!);
            File.WriteAllText(existingPath, "<template><div>old</div></template>");
            File.WriteAllText(deletedPath, "export const oldValue = 1;");
            File.WriteAllText(ignoredPath, "export const ignored = true;");

            var previousSnapshot = DevServerFileSnapshotPoller.CaptureSnapshot(rootDirectory);

            File.WriteAllText(existingPath, "<template><div>new</div></template>");
            var createdPath = Path.Combine(rootDirectory, "site.css");
            File.WriteAllText(createdPath, "body { color: red; }");
            File.Delete(deletedPath);
            File.WriteAllText(ignoredPath, "export const ignored = false;");

            var currentSnapshot = DevServerFileSnapshotPoller.CaptureSnapshot(rootDirectory);
            var changedPaths = DevServerFileSnapshotPoller.GetChangedPaths(previousSnapshot, currentSnapshot);

            CollectionAssert.AreEquivalent(
                new[]
                {
                    Path.GetFullPath(existingPath),
                    Path.GetFullPath(createdPath),
                    Path.GetFullPath(deletedPath)
                },
                changedPaths.ToArray());
            Assert.IsFalse(changedPaths.Contains(Path.GetFullPath(ignoredPath), StringComparer.OrdinalIgnoreCase));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_ServesTransformedHtmlAndCompiledModule()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(rootDirectory, "index.html"),
                """
                <html>
                <head></head>
                <body><script src="/main.ts"></script></body>
                </html>
                """);
            await File.WriteAllTextAsync(
                Path.Combine(rootDirectory, "main.ts"),
                "export const message: string = 'hello';");

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                TypeScriptResult = new FrontendModuleCompilation
                {
                    JavaScript = "export const message = 'hello';",
                    Dependencies = ["vue"]
                }
            };
            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = false
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache());
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);

            Assert.IsNotNull(server.ListeningUri);
            using var client = new HttpClient { BaseAddress = server.ListeningUri };
            var html = await client.GetStringAsync("/");
            var module = await client.GetStringAsync("/main.ts");
            var htmlHeaders = await client.GetAsync("/");
            var moduleHeaders = await client.GetAsync("/main.ts");

            StringAssert.Contains(html, "type=\"importmap\"");
            StringAssert.Contains(html, "<script type=\"module\" src=\"/main.ts\">");
            Assert.IsFalse(html.Contains("/@jazor/client", StringComparison.Ordinal));
            Assert.AreEqual("export const message = 'hello';", module);
            Assert.AreEqual(1, frontendCompiler.TypeScriptCompileCount);
            Assert.AreEqual("no-store", htmlHeaders.Headers.CacheControl?.ToString());
            Assert.AreEqual("no-store", moduleHeaders.Headers.CacheControl?.ToString());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_ServesCompiledTypeScriptEntryPoint_WithInlineSourceMap()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body><script src=\"/main.ts\"></script></body></html>");
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "main.ts"), "export const message: string = 'hello';");

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                TypeScriptResult = new FrontendModuleCompilation
                {
                    JavaScript = "export const message = 'hello';",
                    SourceMap = """
                        {"version":3,"sources":["main.ts"],"sourcesContent":["export const message: string = 'hello';"],"names":[],"mappings":"AAAA","file":"main.js"}
                        """,
                    Dependencies = []
                }
            };
            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = false
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache());
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);

            Assert.IsNotNull(server.ListeningUri);
            using var client = new HttpClient { BaseAddress = server.ListeningUri };
            var module = await client.GetStringAsync("/main.ts");

            StringAssert.Contains(module, "export const message = 'hello';");
            StringAssert.Contains(module, "sourceMappingURL=data:application/json;base64,");

            var sourceMap = DecodeInlineSourceMap(module);
            Assert.AreEqual(3, sourceMap.RootElement.GetProperty("version").GetInt32());
            Assert.AreEqual("main.ts", sourceMap.RootElement.GetProperty("sources")[0].GetString());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_ServesCompiledTypeScriptEntryPoint_SourceMapJsonAtMapUrl()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body><script src=\"/main.ts\"></script></body></html>");
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "main.ts"), "export const message: string = 'hello';");

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                TypeScriptResult = new FrontendModuleCompilation
                {
                    JavaScript = "export const message = 'hello';",
                    SourceMap = """
                        {"version":3,"sources":["main.ts"],"sourcesContent":["export const message: string = 'hello';"],"names":[],"mappings":"AAAA","file":"main.js"}
                        """,
                    Dependencies = []
                }
            };
            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = false
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache());
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);

            Assert.IsNotNull(server.ListeningUri);
            using var client = new HttpClient { BaseAddress = server.ListeningUri };
            var response = await client.GetAsync("/main.ts.map");
            var content = await response.Content.ReadAsStringAsync();
            using var sourceMap = JsonDocument.Parse(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json", response.Content.Headers.ContentType?.MediaType);
            Assert.AreEqual("no-store", response.Headers.CacheControl?.ToString());
            Assert.AreEqual(3, sourceMap.RootElement.GetProperty("version").GetInt32());
            Assert.AreEqual("main.ts", sourceMap.RootElement.GetProperty("sources")[0].GetString());
            Assert.AreEqual("export const message: string = 'hello';", sourceMap.RootElement.GetProperty("sourcesContent")[0].GetString());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_ServesCompiledVueEntryPoint_WithInlineSourceMap()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body><script src=\"/Counter.vue\"></script></body></html>");
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "Counter.vue"), "<template><div>Counter</div></template>");

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    SourceMap = """
                        {"version":3,"sources":["Counter.vue"],"sourcesContent":["<template><div>Counter</div></template>"],"names":[],"mappings":"AAAA","file":"Counter.vue"}
                        """,
                    Dependencies = []
                }
            };
            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = false
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache());
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);

            Assert.IsNotNull(server.ListeningUri);
            using var client = new HttpClient { BaseAddress = server.ListeningUri };
            var module = await client.GetStringAsync("/Counter.vue");

            StringAssert.Contains(module, "export default { name: 'Counter' };");
            StringAssert.Contains(module, "sourceMappingURL=data:application/json;base64,");

            var sourceMap = DecodeInlineSourceMap(module);
            Assert.AreEqual(3, sourceMap.RootElement.GetProperty("version").GetInt32());
            Assert.AreEqual("Counter.vue", sourceMap.RootElement.GetProperty("sources")[0].GetString());
            Assert.AreEqual("<template><div>Counter</div></template>", sourceMap.RootElement.GetProperty("sourcesContent")[0].GetString());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_ServesCompiledVueEntryPoint_SourceMapJsonAtMapUrl()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body><script src=\"/Counter.vue\"></script></body></html>");
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "Counter.vue"), "<template><div>Counter</div></template>");

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    SourceMap = """
                        {"version":3,"sources":["Counter.vue"],"sourcesContent":["<template><div>Counter</div></template>"],"names":[],"mappings":"AAAA","file":"Counter.vue"}
                        """,
                    Dependencies = []
                }
            };
            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = false
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache());
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);

            Assert.IsNotNull(server.ListeningUri);
            using var client = new HttpClient { BaseAddress = server.ListeningUri };
            var response = await client.GetAsync("/Counter.vue.map");
            var content = await response.Content.ReadAsStringAsync();
            using var sourceMap = JsonDocument.Parse(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json", response.Content.Headers.ContentType?.MediaType);
            Assert.AreEqual("no-store", response.Headers.CacheControl?.ToString());
            Assert.AreEqual("Counter.vue", sourceMap.RootElement.GetProperty("sources")[0].GetString());
            Assert.AreEqual("<template><div>Counter</div></template>", sourceMap.RootElement.GetProperty("sourcesContent")[0].GetString());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_ServesVueSourceMap_FromUnsavedWorkspaceDocument()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body><script src=\"/Counter.vue\"></script></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.vue");
            await File.WriteAllTextAsync(documentPath, "<template><div>Counter</div></template>");

            var unsavedSource = "<template><div>Counter updated</div></template>";
            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'CounterUpdated' };",
                    SourceMap = """
                        {"version":3,"sources":["Counter.vue"],"sourcesContent":["<template><div>Counter updated</div></template>"],"names":[],"mappings":"AAAA","file":"Counter.vue"}
                        """,
                    Dependencies = []
                });

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = false
            };
            var workspaceStore = new InMemoryWorkspaceStore();
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options),
                workspaceStore);

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            await workspaceStore.UpsertDocumentAsync(
                new DocumentSnapshot(documentPath, DocumentKind.Vue, unsavedSource, version: "2"),
                CancellationToken.None);

            using var client = new HttpClient { BaseAddress = server.ListeningUri };
            var response = await client.GetAsync("/Counter.vue.map");
            var content = await response.Content.ReadAsStringAsync();
            using var sourceMap = JsonDocument.Parse(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Counter.vue", sourceMap.RootElement.GetProperty("sources")[0].GetString());
            Assert.AreEqual(unsavedSource, sourceMap.RootElement.GetProperty("sourcesContent")[0].GetString());
            Assert.AreEqual(unsavedSource, frontendCompiler.LastSfcText);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_ServesJazorSourceMap_ChainedToOriginalSourceAtMapUrl()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body><script src=\"/Counter.jazor\"></script></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var source = """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [State] private int Count = 1;
                }
                """;
            await File.WriteAllTextAsync(documentPath, source);

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    SourceMap = """
                        {"version":3,"sources":["Counter.jazor"],"sourcesContent":["<script setup>\nconst count = ref(1);\n</script>"],"names":[],"mappings":"AACA","file":"Counter.js"}
                        """,
                    Dependencies = []
                }
            };
            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = false
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);

            Assert.IsNotNull(server.ListeningUri);
            using var client = new HttpClient { BaseAddress = server.ListeningUri };
            var response = await client.GetAsync("/Counter.jazor.map");
            var content = await response.Content.ReadAsStringAsync();
            using var sourceMap = JsonDocument.Parse(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json", response.Content.Headers.ContentType?.MediaType);
            Assert.AreEqual("no-store", response.Headers.CacheControl?.ToString());
            Assert.AreEqual("Counter.jazor", sourceMap.RootElement.GetProperty("sources")[0].GetString());
            Assert.AreEqual(source, sourceMap.RootElement.GetProperty("sourcesContent")[0].GetString());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_ServesJazorSourceMap_FromUnsavedWorkspaceDocument()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body><script src=\"/Counter.jazor\"></script></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>Saved</button>
                </template>
                """);
            var unsavedSource = """
                <template>
                  <button>Unsaved</button>
                </template>

                @code {
                    [State] private int Count = 2;
                }
                """;

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'CounterUnsaved' };",
                    SourceMap = """
                        {"version":3,"sources":["Counter.jazor"],"sourcesContent":["<script setup>\nconst count = ref(2);\n</script>"],"names":[],"mappings":"AACA","file":"Counter.js"}
                        """,
                    Dependencies = []
                }
            };
            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = false
            };
            var workspaceStore = new InMemoryWorkspaceStore();
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options),
                workspaceStore);

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            await workspaceStore.UpsertDocumentAsync(
                new DocumentSnapshot(documentPath, DocumentKind.Jazor, unsavedSource, version: "2"),
                CancellationToken.None);

            using var client = new HttpClient { BaseAddress = server.ListeningUri };
            var response = await client.GetAsync("/Counter.jazor.map");
            var content = await response.Content.ReadAsStringAsync();
            using var sourceMap = JsonDocument.Parse(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Counter.jazor", sourceMap.RootElement.GetProperty("sources")[0].GetString());
            Assert.AreEqual(unsavedSource, sourceMap.RootElement.GetProperty("sourcesContent")[0].GetString());
            StringAssert.Contains(frontendCompiler.LastSfcText!, "Unsaved");
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_ServesCompiledJazorModule()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <div>Hello from Jazor</div>
                </template>
                """);

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    Dependencies = ["vue"]
                }
            };
            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = false
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache());
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);

            Assert.IsNotNull(server.ListeningUri);
            using var client = new HttpClient { BaseAddress = server.ListeningUri };
            var module = await client.GetStringAsync("/Counter.jazor");

            Assert.AreEqual("export default { name: 'Counter' };", module);
            Assert.AreEqual(1, frontendCompiler.SfcCompileCount);
            Assert.AreEqual(documentPath, frontendCompiler.LastDocumentPath);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_ServesDevClientWithNoStoreCacheHeaders()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache());
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);

            Assert.IsNotNull(server.ListeningUri);
            using var client = new HttpClient { BaseAddress = server.ListeningUri };
            var response = await client.GetAsync("/@jazor/client");
            var script = await response.Content.ReadAsStringAsync();

            Assert.AreEqual("no-store", response.Headers.CacheControl?.ToString());
            StringAssert.Contains(script, "/@jazor/hmr");
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenCssChanges_BroadcastsStyleUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var stylePath = Path.Combine(rootDirectory, "site.css");
            await File.WriteAllTextAsync(stylePath, "body { color: red; }");

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache(),
                new DependencyGraph(new ModuleResolver(rootDirectory)));
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);

            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            await File.WriteAllTextAsync(stylePath, "body { color: blue; }");

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("style-update", updateMessage.GetProperty("type").GetString());
            CollectionAssert.AreEqual(
                new[] { "/site.css" },
                updateMessage.GetProperty("paths").EnumerateArray().Select(static item => item.GetString()).ToArray());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenCssChanges_DoesNotBroadcastDuplicateUpdateFromPoller()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var stylePath = Path.Combine(rootDirectory, "site.css");
            await File.WriteAllTextAsync(stylePath, "body { color: red; }");

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);

            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            await File.WriteAllTextAsync(stylePath, "body { color: blue; }");

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("style-update", updateMessage.GetProperty("type").GetString());
            CollectionAssert.AreEqual(
                new[] { "/site.css" },
                updateMessage.GetProperty("paths").EnumerateArray().Select(static item => item.GetString()).ToArray());

            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenUnsavedCssWorkspaceChangeOccurs_UsesWorkspaceTextAndSuppressesMatchingWatcherSave()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var stylePath = Path.Combine(rootDirectory, "site.css");
            const string initialText = "body { color: red; }";
            const string updatedText = "body { color: blue; }";
            await File.WriteAllTextAsync(stylePath, initialText);

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var workspaceStore = new InMemoryWorkspaceStore();
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options),
                workspaceStore);

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using var httpClient = new HttpClient { BaseAddress = server.ListeningUri };
            Assert.AreEqual(initialText, await httpClient.GetStringAsync("/site.css"));

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);

            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            var workspaceDocument = new DocumentSnapshot(stylePath, DocumentKind.Css, updatedText, version: "2");
            await workspaceStore.UpsertDocumentAsync(workspaceDocument, CancellationToken.None);
            await server.OnWorkspaceDocumentChangedAsync(workspaceDocument, CancellationToken.None);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("style-update", updateMessage.GetProperty("type").GetString());
            CollectionAssert.AreEqual(
                new[] { "/site.css" },
                updateMessage.GetProperty("paths").EnumerateArray().Select(static item => item.GetString()).ToArray());
            Assert.AreEqual(
                updatedText,
                await httpClient.GetStringAsync($"/site.css?t={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"));

            await File.WriteAllTextAsync(stylePath, updatedText);
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenClientSendsReadyAndHeartbeat_ReportsClientMetadataAndKeepsConnectionOpen()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var stylePath = Path.Combine(rootDirectory, "site.css");
            await File.WriteAllTextAsync(stylePath, "body { color: red; }");

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache(),
                new DependencyGraph(new ModuleResolver(rootDirectory)));
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);

            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());
            Assert.AreEqual(1, connectedMessage.GetProperty("connectedClientCount").GetInt32());
            var clientId = connectedMessage.GetProperty("clientId").GetString();
            Assert.IsFalse(string.IsNullOrWhiteSpace(clientId));

            await SendWebSocketJsonAsync(socket, """{"type":"ready"}""");
            await SendWebSocketJsonAsync(socket, """{"type":"heartbeat"}""");
            await File.WriteAllTextAsync(stylePath, "body { color: blue; }");

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("style-update", updateMessage.GetProperty("type").GetString());
            CollectionAssert.AreEqual(
                new[] { "/site.css" },
                updateMessage.GetProperty("paths").EnumerateArray().Select(static item => item.GetString()).ToArray());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenDiskSyncedWorkspaceChangePrecedesWatcher_BroadcastsOnce()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.vue");
            await File.WriteAllTextAsync(documentPath, "<template><div>Counter</div></template>");

            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    SupportsHmr = true
                });

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using (var httpClient = new HttpClient { BaseAddress = server.ListeningUri })
            {
                var initialModule = await httpClient.GetStringAsync("/Counter.vue");
                Assert.AreEqual("export default { name: 'Counter' };", initialModule);
            }

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);

            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            var updatedText = "<template><div>Counter updated</div></template>";
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'CounterUpdated' };",
                    SupportsHmr = true
                });
            await File.WriteAllTextAsync(documentPath, updatedText);
            await server.OnWorkspaceDocumentChangedAsync(
                new DocumentSnapshot(documentPath, DocumentKind.Vue, updatedText, version: "2"),
                CancellationToken.None);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", updateMessage.GetProperty("type").GetString());
            var updates = updateMessage.GetProperty("updates").EnumerateArray().ToArray();
            Assert.AreEqual(1, updates.Length);
            Assert.AreEqual("js-update", updates[0].GetProperty("type").GetString());
            Assert.AreEqual("/Counter.vue", updates[0].GetProperty("path").GetString());

            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenUnsavedWorkspaceChangeOccurs_BroadcastsImmediatelyAndSuppressesMatchingWatcherSave()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.vue");
            await File.WriteAllTextAsync(documentPath, "<template><div>Counter</div></template>");

            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    SupportsHmr = true
                });

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var workspaceStore = new InMemoryWorkspaceStore();
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options),
                workspaceStore);

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using var httpClient = new HttpClient { BaseAddress = server.ListeningUri };
            _ = await httpClient.GetStringAsync("/Counter.vue");

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);

            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            var unsavedText = "<template><div>Unsaved edit</div></template>";
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'CounterUnsaved' };",
                    SupportsHmr = true
                });
            var workspaceDocument = new DocumentSnapshot(documentPath, DocumentKind.Vue, unsavedText, version: "2");
            await workspaceStore.UpsertDocumentAsync(workspaceDocument, CancellationToken.None);
            await server.OnWorkspaceDocumentChangedAsync(
                workspaceDocument,
                CancellationToken.None);

            var firstUpdateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", firstUpdateMessage.GetProperty("type").GetString());
            var firstUpdates = firstUpdateMessage.GetProperty("updates").EnumerateArray().ToArray();
            Assert.AreEqual(1, firstUpdates.Length);
            Assert.AreEqual("/Counter.vue", firstUpdates[0].GetProperty("path").GetString());
            Assert.AreEqual(2, frontendCompiler.SfcCompileCount);
            Assert.AreEqual(unsavedText, frontendCompiler.LastSfcText);
            Assert.AreEqual("export default { name: 'CounterUnsaved' };", await httpClient.GetStringAsync("/Counter.vue?t=2"));
            Assert.AreEqual(unsavedText, frontendCompiler.LastSfcText);

            await File.WriteAllTextAsync(documentPath, unsavedText);
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenUnsavedVueStyleOnlyWorkspaceChangeOccurs_BroadcastsInlineStyleUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.vue");
            var initialText =
                """
                <template><div>Counter</div></template>
                <style>
                .counter { color: red; }
                </style>
                """;
            await File.WriteAllTextAsync(documentPath, initialText);

            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    StyleContent = ".counter { color: red; }",
                    SupportsHmr = true
                });

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using (var httpClient = new HttpClient { BaseAddress = server.ListeningUri })
            {
                _ = await httpClient.GetStringAsync("/Counter.vue");
            }

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            var updatedText =
                """
                <template><div>Counter</div></template>
                <style>
                .counter { color: blue; }
                </style>
                """;
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    StyleContent = ".counter { color: blue; }",
                    SupportsHmr = true
                });
            await server.OnWorkspaceDocumentChangedAsync(
                new DocumentSnapshot(documentPath, DocumentKind.Vue, updatedText, version: "2"),
                CancellationToken.None);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("style-update", updateMessage.GetProperty("type").GetString());
            var inlineStyles = updateMessage.GetProperty("inlineStyles").EnumerateArray().ToArray();
            Assert.AreEqual(1, inlineStyles.Length);
            Assert.AreEqual("/Counter.vue", inlineStyles[0].GetProperty("path").GetString());
            Assert.AreEqual(".counter { color: blue; }", inlineStyles[0].GetProperty("content").GetString());
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenUnsavedVueStyleOnlyWorkspaceChangeIsLaterSaved_DoesNotBroadcastDuplicateUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.vue");
            var initialText =
                """
                <template><div>Counter</div></template>
                <style>
                .counter { color: red; }
                </style>
                """;
            await File.WriteAllTextAsync(documentPath, initialText);

            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    StyleContent = ".counter { color: red; }",
                    SupportsHmr = true
                });

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using (var httpClient = new HttpClient { BaseAddress = server.ListeningUri })
            {
                _ = await httpClient.GetStringAsync("/Counter.vue");
            }

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            var updatedText =
                """
                <template><div>Counter</div></template>
                <style>
                .counter { color: blue; }
                </style>
                """;
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    StyleContent = ".counter { color: blue; }",
                    SupportsHmr = true
                });
            await server.OnWorkspaceDocumentChangedAsync(
                new DocumentSnapshot(documentPath, DocumentKind.Vue, updatedText, version: "2"),
                CancellationToken.None);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("style-update", updateMessage.GetProperty("type").GetString());

            await File.WriteAllTextAsync(documentPath, updatedText);
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenUnsavedTypeScriptWorkspaceChangeOccurs_BroadcastsJavaScriptUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "main.ts");
            await File.WriteAllTextAsync(documentPath, "export const count: number = 1;");

            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetTypeScriptResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export const count = 1;",
                    Dependencies = []
                });

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var workspaceStore = new InMemoryWorkspaceStore();
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options),
                workspaceStore);

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using var httpClient = new HttpClient { BaseAddress = server.ListeningUri };
            _ = await httpClient.GetStringAsync("/main.ts");

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            var updatedSource = "export const count: number = 2;";
            frontendCompiler.SetTypeScriptResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export const count = 2;",
                    Dependencies = []
                });
            var workspaceDocument = new DocumentSnapshot(documentPath, DocumentKind.TypeScript, updatedSource, version: "2");
            await workspaceStore.UpsertDocumentAsync(workspaceDocument, CancellationToken.None);
            await server.OnWorkspaceDocumentChangedAsync(
                workspaceDocument,
                CancellationToken.None);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", updateMessage.GetProperty("type").GetString());
            var updates = updateMessage.GetProperty("updates").EnumerateArray().ToArray();
            Assert.AreEqual(1, updates.Length);
            Assert.AreEqual("js-update", updates[0].GetProperty("type").GetString());
            Assert.AreEqual("/main.ts", updates[0].GetProperty("path").GetString());
            Assert.AreEqual("/main.ts", updates[0].GetProperty("acceptedPath").GetString());
            Assert.AreEqual("export const count = 2;", await httpClient.GetStringAsync("/main.ts?t=2"));
            Assert.AreEqual(updatedSource, frontendCompiler.LastTypeScriptText);
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_ServesTypeScriptSourceMap_FromUnsavedWorkspaceDocument()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body><script src=\"/main.ts\"></script></body></html>");
            var documentPath = Path.Combine(rootDirectory, "main.ts");
            await File.WriteAllTextAsync(documentPath, "export const count: number = 1;");

            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetTypeScriptResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export const count = 2;",
                    SourceMap = """
                        {"version":3,"sources":["main.ts"],"sourcesContent":["export const count: number = 2;"],"names":[],"mappings":"AAAA","file":"main.js"}
                        """,
                    Dependencies = []
                });

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = false
            };
            var workspaceStore = new InMemoryWorkspaceStore();
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options),
                workspaceStore);

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            var unsavedSource = "export const count: number = 2;";
            await workspaceStore.UpsertDocumentAsync(
                new DocumentSnapshot(documentPath, DocumentKind.TypeScript, unsavedSource, version: "2"),
                CancellationToken.None);

            using var client = new HttpClient { BaseAddress = server.ListeningUri };
            var response = await client.GetAsync("/main.ts.map");
            var content = await response.Content.ReadAsStringAsync();
            using var sourceMap = JsonDocument.Parse(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("main.ts", sourceMap.RootElement.GetProperty("sources")[0].GetString());
            Assert.AreEqual(unsavedSource, sourceMap.RootElement.GetProperty("sourcesContent")[0].GetString());
            Assert.AreEqual(unsavedSource, frontendCompiler.LastTypeScriptText);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenUnsavedTypeScriptWorkspaceChangeIsLaterSaved_DoesNotBroadcastDuplicateUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "main.ts");
            await File.WriteAllTextAsync(documentPath, "export const count: number = 1;");

            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetTypeScriptResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export const count = 1;",
                    Dependencies = []
                });

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using (var httpClient = new HttpClient { BaseAddress = server.ListeningUri })
            {
                _ = await httpClient.GetStringAsync("/main.ts");
            }

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            var updatedSource = "export const count: number = 2;";
            frontendCompiler.SetTypeScriptResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export const count = 2;",
                    Dependencies = []
                });
            await server.OnWorkspaceDocumentChangedAsync(
                new DocumentSnapshot(documentPath, DocumentKind.TypeScript, updatedSource, version: "2"),
                CancellationToken.None);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", updateMessage.GetProperty("type").GetString());

            await File.WriteAllTextAsync(documentPath, updatedSource);
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenUnsavedJazorWorkspaceLogicChangeOccurs_BroadcastsJavaScriptUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    public int Increment()
                    {
                        return Count + 1;
                    }
                }
                """);

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', methods: { increment() { return 1; } } };",
                    SupportsHmr = true
                }
            };

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var workspaceStore = new InMemoryWorkspaceStore();
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options),
                workspaceStore);

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using var httpClient = new HttpClient { BaseAddress = server.ListeningUri };
            _ = await httpClient.GetStringAsync("/Counter.jazor");

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            frontendCompiler.SfcResult = new FrontendModuleCompilation
            {
                JavaScript = "export default { name: 'Counter', methods: { increment() { return 2; } } };",
                SupportsHmr = true
            };
            var updatedText =
                """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    public int Increment()
                    {
                        return Count + 2;
                    }
                }
                """;
            var workspaceDocument = new DocumentSnapshot(documentPath, DocumentKind.Jazor, updatedText, version: "2");
            await workspaceStore.UpsertDocumentAsync(workspaceDocument, CancellationToken.None);
            await server.OnWorkspaceDocumentChangedAsync(
                workspaceDocument,
                CancellationToken.None);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", updateMessage.GetProperty("type").GetString());
            var updates = updateMessage.GetProperty("updates").EnumerateArray().ToArray();
            Assert.AreEqual(1, updates.Length);
            Assert.AreEqual("js-update", updates[0].GetProperty("type").GetString());
            Assert.AreEqual("/Counter.jazor", updates[0].GetProperty("path").GetString());
            Assert.AreEqual("/Counter.jazor", updates[0].GetProperty("acceptedPath").GetString());
            Assert.AreEqual(
                "export default { name: 'Counter', methods: { increment() { return 2; } } };",
                await httpClient.GetStringAsync("/Counter.jazor?t=2"));
            StringAssert.Contains(frontendCompiler.LastSfcText!, "return Count + 2;");
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenUnsavedJazorWorkspaceDescriptorChangeOccurs_BroadcastsFullReload()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <div>Hello</div>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                }
                """);

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', render() { return null; } };",
                    SupportsHmr = true
                }
            };

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using (var httpClient = new HttpClient { BaseAddress = server.ListeningUri })
            {
                _ = await httpClient.GetStringAsync("/Counter.jazor");
            }

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            frontendCompiler.SfcResult = new FrontendModuleCompilation
            {
                JavaScript = "export default { name: 'Counter', render() { return 'updated'; } };",
                SupportsHmr = true
            };
            var updatedText =
                """
                <template>
                  <div>Hello updated</div>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                    [Prop] public string Title { get; set; } = string.Empty;
                }
                """;
            await server.OnWorkspaceDocumentChangedAsync(
                new DocumentSnapshot(documentPath, DocumentKind.Jazor, updatedText, version: "2"),
                CancellationToken.None);

            var reloadMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("full-reload", reloadMessage.GetProperty("type").GetString());
            Assert.AreEqual("Public component descriptor changed.", reloadMessage.GetProperty("reason").GetString());
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenUnsavedJazorWorkspaceChangeIsLaterSavedWithSameText_DoesNotBroadcastDuplicateUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var initialText =
                """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    public int Increment()
                    {
                        return Count + 1;
                    }
                }
                """;
            await File.WriteAllTextAsync(documentPath, initialText);

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', methods: { increment() { return 1; } } };",
                    SupportsHmr = true
                }
            };

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using (var httpClient = new HttpClient { BaseAddress = server.ListeningUri })
            {
                _ = await httpClient.GetStringAsync("/Counter.jazor");
            }

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            frontendCompiler.SfcResult = new FrontendModuleCompilation
            {
                JavaScript = "export default { name: 'Counter', methods: { increment() { return 2; } } };",
                SupportsHmr = true
            };
            var updatedText =
                """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    public int Increment()
                    {
                        return Count + 2;
                    }
                }
                """;
            await server.OnWorkspaceDocumentChangedAsync(
                new DocumentSnapshot(documentPath, DocumentKind.Jazor, updatedText, version: "2"),
                CancellationToken.None);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", updateMessage.GetProperty("type").GetString());

            await File.WriteAllTextAsync(documentPath, updatedText);
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenDiskSyncedJazorCodeBehindWorkspaceChangePrecedesWatcher_BroadcastsOnce()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var codeBehindPath = Path.Combine(rootDirectory, "Counter.jazor.cs");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@count</button>
                </template>
                """);
            await File.WriteAllTextAsync(
                codeBehindPath,
                """
                public partial class Counter
                {
                    [State] private int count = 1;
                }
                """);

            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', render() { return 1; } };",
                    SupportsHmr = true
                });

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using (var httpClient = new HttpClient { BaseAddress = server.ListeningUri })
            {
                var initialModule = await httpClient.GetStringAsync("/Counter.jazor");
                Assert.AreEqual("export default { name: 'Counter', render() { return 1; } };", initialModule);
            }

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);

            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            var updatedCodeBehindText =
                """
                public partial class Counter
                {
                    [State] private int count = 2;
                }
                """;
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', render() { return 2; } };",
                    SupportsHmr = true
                });
            await File.WriteAllTextAsync(codeBehindPath, updatedCodeBehindText);
            await server.OnWorkspaceDocumentChangedAsync(
                new DocumentSnapshot(codeBehindPath, DocumentKind.CSharp, updatedCodeBehindText, version: "2"),
                CancellationToken.None);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", updateMessage.GetProperty("type").GetString());
            var updates = updateMessage.GetProperty("updates").EnumerateArray().ToArray();
            Assert.AreEqual(1, updates.Length);
            Assert.AreEqual("js-update", updates[0].GetProperty("type").GetString());
            Assert.AreEqual("/Counter.jazor", updates[0].GetProperty("path").GetString());

            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenUnsavedJazorCodeBehindWorkspaceLogicChangeOccurs_BroadcastsJavaScriptUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var codeBehindPath = Path.Combine(rootDirectory, "Counter.jazor.cs");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@count</button>
                </template>
                """);
            await File.WriteAllTextAsync(
                codeBehindPath,
                """
                public partial class Counter
                {
                    [State] private int count = 1;
                }
                """);

            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', render() { return 1; } };",
                    SupportsHmr = true
                });

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using (var httpClient = new HttpClient { BaseAddress = server.ListeningUri })
            {
                _ = await httpClient.GetStringAsync("/Counter.jazor");
            }

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            var updatedCodeBehindText =
                """
                public partial class Counter
                {
                    [State] private int count = 2;
                }
                """;
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', render() { return 2; } };",
                    SupportsHmr = true
                });
            await server.OnWorkspaceDocumentChangedAsync(
                new DocumentSnapshot(codeBehindPath, DocumentKind.CSharp, updatedCodeBehindText, version: "2"),
                CancellationToken.None);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", updateMessage.GetProperty("type").GetString());
            var updates = updateMessage.GetProperty("updates").EnumerateArray().ToArray();
            Assert.AreEqual(1, updates.Length);
            Assert.AreEqual("js-update", updates[0].GetProperty("type").GetString());
            Assert.AreEqual("/Counter.jazor", updates[0].GetProperty("path").GetString());
            Assert.AreEqual("/Counter.jazor", updates[0].GetProperty("acceptedPath").GetString());
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenUnsavedJazorCodeBehindWorkspaceSignatureChangeOccurs_BroadcastsFullReload()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var codeBehindPath = Path.Combine(rootDirectory, "Counter.jazor.cs");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Increment(1)</button>
                </template>
                """);
            await File.WriteAllTextAsync(
                codeBehindPath,
                """
                public partial class Counter
                {
                    public int Increment(int delta)
                    {
                        return delta + 1;
                    }
                }
                """);

            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', methods: { increment(delta) { return delta + 1; } } };",
                    SupportsHmr = true
                });

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using (var httpClient = new HttpClient { BaseAddress = server.ListeningUri })
            {
                _ = await httpClient.GetStringAsync("/Counter.jazor");
            }

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            var updatedCodeBehindText =
                """
                public partial class Counter
                {
                    public long Increment(long delta)
                    {
                        return delta + 1;
                    }
                }
                """;
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', methods: { increment(delta) { return Number(delta) + 1; } } };",
                    SupportsHmr = true
                });
            await server.OnWorkspaceDocumentChangedAsync(
                new DocumentSnapshot(codeBehindPath, DocumentKind.CSharp, updatedCodeBehindText, version: "2"),
                CancellationToken.None);

            var reloadMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("full-reload", reloadMessage.GetProperty("type").GetString());
            Assert.AreEqual("Public component descriptor changed.", reloadMessage.GetProperty("reason").GetString());
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenUnsavedJazorCodeBehindWorkspaceChangeIsLaterSavedWithSameText_DoesNotBroadcastDuplicateUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var codeBehindPath = Path.Combine(rootDirectory, "Counter.jazor.cs");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@count</button>
                </template>
                """);
            await File.WriteAllTextAsync(
                codeBehindPath,
                """
                public partial class Counter
                {
                    [State] private int count = 1;
                }
                """);

            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', render() { return 1; } };",
                    SupportsHmr = true
                });

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using (var httpClient = new HttpClient { BaseAddress = server.ListeningUri })
            {
                _ = await httpClient.GetStringAsync("/Counter.jazor");
            }

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);
            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            var updatedCodeBehindText =
                """
                public partial class Counter
                {
                    [State] private int count = 2;
                }
                """;
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', render() { return 2; } };",
                    SupportsHmr = true
                });
            await server.OnWorkspaceDocumentChangedAsync(
                new DocumentSnapshot(codeBehindPath, DocumentKind.CSharp, updatedCodeBehindText, version: "2"),
                CancellationToken.None);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", updateMessage.GetProperty("type").GetString());

            await File.WriteAllTextAsync(codeBehindPath, updatedCodeBehindText);
            await AssertNoWebSocketJsonAsync(socket, TimeSpan.FromMilliseconds(1600));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenVueChanges_BroadcastsJavaScriptUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.vue");
            await File.WriteAllTextAsync(documentPath, "<template><div>Counter</div></template>");

            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    SupportsHmr = true
                });

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using (var httpClient = new HttpClient { BaseAddress = server.ListeningUri })
            {
                var initialModule = await httpClient.GetStringAsync("/Counter.vue");
                Assert.AreEqual("export default { name: 'Counter' };", initialModule);
            }

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);

            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'CounterUpdated' };",
                    SupportsHmr = true
                });
            await File.WriteAllTextAsync(documentPath, "<template><div>Counter updated</div></template>");

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", updateMessage.GetProperty("type").GetString());
            var updates = updateMessage.GetProperty("updates").EnumerateArray().ToArray();
            Assert.AreEqual(1, updates.Length);
            Assert.AreEqual("js-update", updates[0].GetProperty("type").GetString());
            Assert.AreEqual("/Counter.vue", updates[0].GetProperty("path").GetString());
            Assert.AreEqual("/Counter.vue", updates[0].GetProperty("acceptedPath").GetString());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenVueRecompileFails_BroadcastsErrorMessage()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.vue");
            await File.WriteAllTextAsync(documentPath, "<template><div>Counter</div></template>");

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    SupportsHmr = true
                }
            };

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using (var httpClient = new HttpClient { BaseAddress = server.ListeningUri })
            {
                var initialModule = await httpClient.GetStringAsync("/Counter.vue");
                Assert.AreEqual("export default { name: 'Counter' };", initialModule);
            }

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);

            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            frontendCompiler.SfcResult = null;
            await File.WriteAllTextAsync(documentPath, "<template><div>Counter broken</div></template>");

            var errorMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("error", errorMessage.GetProperty("type").GetString());
            StringAssert.Contains(
                errorMessage.GetProperty("message").GetString()!,
                "frontend compiler is unavailable");
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenIndexHtmlChanges_BroadcastsFullReload()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var indexHtmlPath = Path.Combine(rootDirectory, "index.html");
            await File.WriteAllTextAsync(indexHtmlPath, "<html><body>before</body></html>");

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache());
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);

            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            await File.WriteAllTextAsync(indexHtmlPath, "<html><body>after</body></html>");

            var reloadMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("full-reload", reloadMessage.GetProperty("type").GetString());
            Assert.AreEqual("index-html-change", reloadMessage.GetProperty("reason").GetString());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenConfigChanges_BroadcastsFullReload()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body>before</body></html>");
            var configPath = Path.Combine(rootDirectory, "jazor.config.json");
            await File.WriteAllTextAsync(configPath, """{"server":{"hmr":true}}""");

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache());
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);

            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            await File.WriteAllTextAsync(configPath, """{"server":{"hmr":false}}""");

            var reloadMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("full-reload", reloadMessage.GetProperty("type").GetString());
            Assert.AreEqual("config-change", reloadMessage.GetProperty("reason").GetString());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenFrontendFileIsDeleted_BroadcastsFullReload()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.vue");
            await File.WriteAllTextAsync(documentPath, "<template><div>Counter</div></template>");

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);

            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            File.Delete(documentPath);

            var reloadMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("full-reload", reloadMessage.GetProperty("type").GetString());
            Assert.AreEqual("missing-file-change", reloadMessage.GetProperty("reason").GetString());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenFrontendFileIsRenamed_BroadcastsFullReload()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.vue");
            var renamedDocumentPath = Path.Combine(rootDirectory, "CounterRenamed.vue");
            await File.WriteAllTextAsync(documentPath, "<template><div>Counter</div></template>");

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);

            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            File.Move(documentPath, renamedDocumentPath);

            var reloadMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("full-reload", reloadMessage.GetProperty("type").GetString());
            Assert.AreEqual("missing-file-change", reloadMessage.GetProperty("reason").GetString());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenCssFileIsCreated_BroadcastsStyleUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var stylePath = Path.Combine(rootDirectory, "created.css");

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);

            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            await File.WriteAllTextAsync(stylePath, "body { color: green; }");

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("style-update", updateMessage.GetProperty("type").GetString());
            CollectionAssert.AreEqual(
                new[] { "/created.css" },
                updateMessage.GetProperty("paths").EnumerateArray().Select(static item => item.GetString()).ToArray());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenJazorTemplateChanges_BroadcastsJavaScriptUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <div>Hello</div>
                </template>
                """);

            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', render() { return null; } };",
                    SupportsHmr = true
                });

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using (var httpClient = new HttpClient { BaseAddress = server.ListeningUri })
            {
                var initialModule = await httpClient.GetStringAsync("/Counter.jazor");
                Assert.AreEqual("export default { name: 'Counter', render() { return null; } };", initialModule);
            }

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);

            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', render() { return 'updated'; } };",
                    SupportsHmr = true
                });
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <div>Hello updated</div>
                </template>
                """);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", updateMessage.GetProperty("type").GetString());
            var updates = updateMessage.GetProperty("updates").EnumerateArray().ToArray();
            Assert.AreEqual(1, updates.Length);
            Assert.AreEqual("js-update", updates[0].GetProperty("type").GetString());
            Assert.AreEqual("/Counter.jazor", updates[0].GetProperty("path").GetString());
            Assert.AreEqual("/Counter.jazor", updates[0].GetProperty("acceptedPath").GetString());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenJazorCodeBehindChanges_BroadcastsJavaScriptUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var codeBehindPath = Path.Combine(rootDirectory, "Counter.jazor.cs");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                }
                """);
            await File.WriteAllTextAsync(
                codeBehindPath,
                """
                public partial class Counter
                {
                    [State] private int count = 1;
                }
                """);

            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', setup() { return { count: 1 }; } };",
                    SupportsHmr = true
                });

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using (var httpClient = new HttpClient { BaseAddress = server.ListeningUri })
            {
                var initialModule = await httpClient.GetStringAsync("/Counter.jazor");
                Assert.AreEqual("export default { name: 'Counter', setup() { return { count: 1 }; } };", initialModule);
            }

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);

            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', setup() { return { count: 2 }; } };",
                    SupportsHmr = true
                });
            await File.WriteAllTextAsync(
                codeBehindPath,
                """
                public partial class Counter
                {
                    [State] private int count = 2;
                }
                """);

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", updateMessage.GetProperty("type").GetString());
            var updates = updateMessage.GetProperty("updates").EnumerateArray().ToArray();
            Assert.AreEqual(1, updates.Length);
            Assert.AreEqual("js-update", updates[0].GetProperty("type").GetString());
            Assert.AreEqual("/Counter.jazor", updates[0].GetProperty("path").GetString());
            Assert.AreEqual("/Counter.jazor", updates[0].GetProperty("acceptedPath").GetString());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenJazorCodeBehindMethodSignatureChanges_BroadcastsFullReload()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var codeBehindPath = Path.Combine(rootDirectory, "Counter.jazor.cs");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Increment(1)</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                }
                """);
            await File.WriteAllTextAsync(
                codeBehindPath,
                """
                public partial class Counter
                {
                    public int Increment(int delta)
                    {
                        return Count + delta;
                    }
                }
                """);

            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', methods: { increment(delta) { return delta; } } };",
                    SupportsHmr = true
                });

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using (var httpClient = new HttpClient { BaseAddress = server.ListeningUri })
            {
                _ = await httpClient.GetStringAsync("/Counter.jazor");
            }

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);

            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', methods: { increment(delta) { return Number(delta); } } };",
                    SupportsHmr = true
                });
            await File.WriteAllTextAsync(
                codeBehindPath,
                """
                public partial class Counter
                {
                    public int Increment(long delta)
                    {
                        return (int)(Count + delta);
                    }
                }
                """);

            var reloadMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("full-reload", reloadMessage.GetProperty("type").GetString());
            Assert.AreEqual("Public component descriptor changed.", reloadMessage.GetProperty("reason").GetString());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_HmrWebSocket_WhenTypeScriptChanges_BroadcastsJavaScriptUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");
            var documentPath = Path.Combine(rootDirectory, "main.ts");
            await File.WriteAllTextAsync(documentPath, "export const count: number = 1;");

            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetTypeScriptResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export const count = 1;",
                    Dependencies = []
                });

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                new DependencyGraph(moduleResolver),
                moduleResolver);
            await using var server = new DevHttpServer(
                options,
                compiler,
                moduleResolver,
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);
            Assert.IsNotNull(server.ListeningUri);

            using (var httpClient = new HttpClient { BaseAddress = server.ListeningUri })
            {
                var initialModule = await httpClient.GetStringAsync("/main.ts");
                Assert.AreEqual("export const count = 1;", initialModule);
            }

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(ToWebSocketUri(server.ListeningUri!, "/@jazor/hmr"), CancellationToken.None);

            var connectedMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("connected", connectedMessage.GetProperty("type").GetString());

            frontendCompiler.SetTypeScriptResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export const count = 2;",
                    Dependencies = []
                });
            await File.WriteAllTextAsync(documentPath, "export const count: number = 2;");

            var updateMessage = await ReceiveWebSocketJsonAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("update", updateMessage.GetProperty("type").GetString());
            var updates = updateMessage.GetProperty("updates").EnumerateArray().ToArray();
            Assert.AreEqual(1, updates.Length);
            Assert.AreEqual("js-update", updates[0].GetProperty("type").GetString());
            Assert.AreEqual("/main.ts", updates[0].GetProperty("path").GetString());
            Assert.AreEqual("/main.ts", updates[0].GetProperty("acceptedPath").GetString());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_WhenProxyConfigured_ForwardsPostRequestsToUpstream()
    {
        var rootDirectory = CreateTemporaryDirectory();
        var requestReceived = new TaskCompletionSource<UpstreamRequestSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var upstream = await TestUpstreamServer.StartAsync(
            async context =>
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                var body = await reader.ReadToEndAsync();
                requestReceived.TrySetResult(
                    new UpstreamRequestSnapshot(
                        context.Request.HttpMethod,
                        context.Request.Url!.AbsolutePath,
                        context.Request.Url.Query,
                        body));

                context.Response.StatusCode = (int)HttpStatusCode.Accepted;
                context.Response.ContentType = "application/json";
                await using var writer = new StreamWriter(context.Response.OutputStream);
                await writer.WriteAsync("""{"proxied":true}""");
            });

        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body>proxy</body></html>");

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = false,
                ProxyRules = new Dictionary<string, ProxyTarget>(StringComparer.OrdinalIgnoreCase)
                {
                    ["/api"] = new()
                    {
                        Target = upstream.BaseAddress + "backend"
                    }
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache());
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);

            using var client = new HttpClient { BaseAddress = server.ListeningUri };
            using var response = await client.PostAsync(
                "/api/todos?page=2",
                new StringContent("""{"title":"Ship"}"""));
            var responseText = await response.Content.ReadAsStringAsync();
            var proxiedRequest = await requestReceived.Task.WaitAsync(TimeSpan.FromSeconds(2));

            Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
            Assert.AreEqual("""{"proxied":true}""", responseText);
            Assert.AreEqual("POST", proxiedRequest.Method);
            Assert.AreEqual("/backend/todos", proxiedRequest.AbsolutePath);
            Assert.AreEqual("?page=2", proxiedRequest.Query);
            Assert.AreEqual("""{"title":"Ship"}""", proxiedRequest.Body);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_WhenProxyRewritePathConfigured_ForwardsRequestToRewrittenPath()
    {
        var rootDirectory = CreateTemporaryDirectory();
        var requestReceived = new TaskCompletionSource<UpstreamRequestSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var upstream = await TestUpstreamServer.StartAsync(
            async context =>
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                var body = await reader.ReadToEndAsync();
                requestReceived.TrySetResult(
                    new UpstreamRequestSnapshot(
                        context.Request.HttpMethod,
                        context.Request.Url!.AbsolutePath,
                        context.Request.Url.Query,
                        body));

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await using var writer = new StreamWriter(context.Response.OutputStream);
                await writer.WriteAsync("rewritten");
            });

        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body>proxy</body></html>");

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = false,
                ProxyRules = new Dictionary<string, ProxyTarget>(StringComparer.OrdinalIgnoreCase)
                {
                    ["/api"] = new()
                    {
                        Target = upstream.BaseAddress + "backend",
                        RewritePath = "/gateway"
                    }
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache());
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);

            using var client = new HttpClient { BaseAddress = server.ListeningUri };
            using var response = await client.GetAsync("/api/todos?page=3");
            var responseText = await response.Content.ReadAsStringAsync();
            var proxiedRequest = await requestReceived.Task.WaitAsync(TimeSpan.FromSeconds(2));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("rewritten", responseText);
            Assert.AreEqual("GET", proxiedRequest.Method);
            Assert.AreEqual("/backend/gateway", proxiedRequest.AbsolutePath);
            Assert.AreEqual("?page=3", proxiedRequest.Query);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_WhenHttpsProxySecureFalse_ForwardsRequestsToSelfSignedUpstream()
    {
        var rootDirectory = CreateTemporaryDirectory();
        var requestReceived = new TaskCompletionSource<UpstreamRequestSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var upstream = await TestHttpsUpstreamServer.StartAsync(
            async context =>
            {
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                requestReceived.TrySetResult(
                    new UpstreamRequestSnapshot(
                        context.Request.Method,
                        context.Request.Path,
                        context.Request.QueryString.Value ?? string.Empty,
                        body));

                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("secure-ok");
            });

        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body>proxy</body></html>");

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = false,
                ProxyRules = new Dictionary<string, ProxyTarget>(StringComparer.OrdinalIgnoreCase)
                {
                    ["/api"] = new()
                    {
                        Target = upstream.BaseAddress + "backend",
                        Secure = false
                    }
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache());
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);

            using var client = new HttpClient { BaseAddress = server.ListeningUri };
            using var response = await client.PostAsync(
                "/api/todos?page=9",
                new StringContent("""{"title":"Secure"}"""));
            var responseText = await response.Content.ReadAsStringAsync();
            var proxiedRequest = await requestReceived.Task.WaitAsync(TimeSpan.FromSeconds(2));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("secure-ok", responseText);
            Assert.AreEqual("POST", proxiedRequest.Method);
            Assert.AreEqual("/backend/todos", proxiedRequest.AbsolutePath);
            Assert.AreEqual("?page=9", proxiedRequest.Query);
            Assert.AreEqual("""{"title":"Secure"}""", proxiedRequest.Body);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_WhenProxyRuleDoesNotMatch_DoesNotForwardToUpstream()
    {
        var rootDirectory = CreateTemporaryDirectory();
        var requestReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var upstream = await TestUpstreamServer.StartAsync(
            context =>
            {
                requestReceived.TrySetResult(true);
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.Close();
                return Task.CompletedTask;
            });

        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body>proxy</body></html>");

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = false,
                ProxyRules = new Dictionary<string, ProxyTarget>(StringComparer.OrdinalIgnoreCase)
                {
                    ["/api"] = new()
                    {
                        Target = upstream.BaseAddress + "backend"
                    }
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache());
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);

            using var client = new HttpClient { BaseAddress = server.ListeningUri };
            using var response = await client.GetAsync("/auth/health");
            var completedTask = await Task.WhenAny(requestReceived.Task, Task.Delay(300));

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreNotSame(requestReceived.Task, completedTask);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_WhenWebSocketProxyUsesSelfSignedUpstream_ForwardsSubprotocolsAndSelectedProtocol()
    {
        var rootDirectory = CreateTemporaryDirectory();
        var requestReceived = new TaskCompletionSource<UpstreamWebSocketSubProtocolSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var upstream = await TestSecureWebSocketUpstreamServer.StartAsync(
            async (context, socket) =>
            {
                requestReceived.TrySetResult(
                    new UpstreamWebSocketSubProtocolSnapshot(
                        context.Request.Method,
                        context.Request.Path,
                        context.Request.QueryString.Value ?? string.Empty,
                        context.WebSockets.WebSocketRequestedProtocols.ToArray(),
                        socket.SubProtocol));

                var requestText = await ReceiveWebSocketTextAsync(socket, TimeSpan.FromSeconds(5));
                await socket.SendAsync(
                    Encoding.UTF8.GetBytes("secure-echo:" + requestText),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    CancellationToken.None);
                await TryCloseWebSocketAsync(socket);
            },
            static context => context.WebSockets.WebSocketRequestedProtocols.Contains("chat.v2", StringComparer.Ordinal)
                ? "chat.v2"
                : null);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body>proxy</body></html>");

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = false,
                ProxyRules = new Dictionary<string, ProxyTarget>(StringComparer.OrdinalIgnoreCase)
                {
                    ["/ws"] = new()
                    {
                        Target = upstream.BaseAddress + "backend",
                        WebSocket = true,
                        Secure = false
                    }
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache());
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);

            using var socket = new ClientWebSocket();
            socket.Options.AddSubProtocol("json.v1");
            socket.Options.AddSubProtocol("chat.v2");
            await socket.ConnectAsync(
                ToWebSocketUri(server.ListeningUri!, "/ws/chat?room=42"),
                CancellationToken.None);
            await socket.SendAsync(
                Encoding.UTF8.GetBytes("ping"),
                WebSocketMessageType.Text,
                endOfMessage: true,
                CancellationToken.None);

            var responseText = await ReceiveWebSocketTextAsync(socket, TimeSpan.FromSeconds(5));
            var proxiedRequest = await requestReceived.Task.WaitAsync(TimeSpan.FromSeconds(2));

            Assert.AreEqual("secure-echo:ping", responseText);
            Assert.AreEqual("chat.v2", socket.SubProtocol);
            Assert.AreEqual("GET", proxiedRequest.Method);
            Assert.AreEqual("/backend/chat", proxiedRequest.AbsolutePath);
            Assert.AreEqual("?room=42", proxiedRequest.Query);
            CollectionAssert.AreEqual(
                new[] { "json.v1", "chat.v2" },
                proxiedRequest.RequestedProtocols.ToArray());
            Assert.AreEqual("chat.v2", proxiedRequest.SelectedProtocol);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_WhenWebSocketProxyUsesSelfSignedUpstream_PropagatesUpstreamCloseToClient()
    {
        var rootDirectory = CreateTemporaryDirectory();
        var releaseUpstream = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var upstream = await TestSecureWebSocketUpstreamServer.StartAsync(
            async (context, socket) =>
            {
                var requestText = await ReceiveWebSocketTextAsync(socket, TimeSpan.FromSeconds(5));
                await socket.SendAsync(
                    Encoding.UTF8.GetBytes("secure-echo:" + requestText),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    CancellationToken.None);
                await socket.CloseOutputAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "upstream-finished",
                    CancellationToken.None);
                await releaseUpstream.Task.WaitAsync(TimeSpan.FromSeconds(5));
            },
            static context => context.WebSockets.WebSocketRequestedProtocols.Contains("chat.v2", StringComparer.Ordinal)
                ? "chat.v2"
                : null);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body>proxy</body></html>");

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = false,
                ProxyRules = new Dictionary<string, ProxyTarget>(StringComparer.OrdinalIgnoreCase)
                {
                    ["/ws"] = new()
                    {
                        Target = upstream.BaseAddress + "backend",
                        WebSocket = true,
                        Secure = false
                    }
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache());
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);

            using var socket = new ClientWebSocket();
            socket.Options.AddSubProtocol("chat.v2");
            await socket.ConnectAsync(
                ToWebSocketUri(server.ListeningUri!, "/ws/chat?room=42"),
                CancellationToken.None);
            await socket.SendAsync(
                Encoding.UTF8.GetBytes("ping"),
                WebSocketMessageType.Text,
                endOfMessage: true,
                CancellationToken.None);

            var responseText = await ReceiveWebSocketTextAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("secure-echo:ping", responseText);
            Assert.AreEqual("chat.v2", socket.SubProtocol);

            var closeStatus = await ReceiveWebSocketCloseAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual(WebSocketCloseStatus.NormalClosure, closeStatus);
            releaseUpstream.TrySetResult();
        }
        finally
        {
            releaseUpstream.TrySetResult();
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_WhenWebSocketProxyRewritePathConfigured_ForwardsMessagesToRewrittenPath()
    {
        var rootDirectory = CreateTemporaryDirectory();
        var requestReceived = new TaskCompletionSource<UpstreamWebSocketRequestSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var upstream = await TestWebSocketUpstreamServer.StartAsync(
            async (context, socket) =>
            {
                requestReceived.TrySetResult(
                    new UpstreamWebSocketRequestSnapshot(
                        context.Request.HttpMethod,
                        context.Request.Url!.AbsolutePath,
                        context.Request.Url.Query));

                var requestText = await ReceiveWebSocketTextAsync(socket, TimeSpan.FromSeconds(5));
                await socket.SendAsync(
                    Encoding.UTF8.GetBytes("echo:" + requestText),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    CancellationToken.None);
                await TryCloseWebSocketAsync(socket);
            });

        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body>proxy</body></html>");

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = false,
                ProxyRules = new Dictionary<string, ProxyTarget>(StringComparer.OrdinalIgnoreCase)
                {
                    ["/ws"] = new()
                    {
                        Target = upstream.BaseAddress + "backend",
                        WebSocket = true,
                        RewritePath = "/gateway"
                    }
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache());
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(
                ToWebSocketUri(server.ListeningUri!, "/ws/chat?room=7"),
                CancellationToken.None);
            await socket.SendAsync(
                Encoding.UTF8.GetBytes("ping"),
                WebSocketMessageType.Text,
                endOfMessage: true,
                CancellationToken.None);

            var responseText = await ReceiveWebSocketTextAsync(socket, TimeSpan.FromSeconds(5));
            var proxiedRequest = await requestReceived.Task.WaitAsync(TimeSpan.FromSeconds(2));

            Assert.AreEqual("echo:ping", responseText);
            Assert.AreEqual("GET", proxiedRequest.Method);
            Assert.AreEqual("/backend/gateway", proxiedRequest.AbsolutePath);
            Assert.AreEqual("?room=7", proxiedRequest.Query);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_WhenWebSocketProxyClientInitiatesClose_PropagatesCloseToUpstream()
    {
        var rootDirectory = CreateTemporaryDirectory();
        var upstreamObservedClose = new TaskCompletionSource<WebSocketCloseStatus?>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var upstream = await TestWebSocketUpstreamServer.StartAsync(
            async (context, socket) =>
            {
                var requestText = await ReceiveWebSocketTextAsync(socket, TimeSpan.FromSeconds(5));
                await socket.SendAsync(
                    Encoding.UTF8.GetBytes("echo:" + requestText),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    CancellationToken.None);

                var buffer = new byte[256];
                var closeFrame = await socket.ReceiveAsync(buffer, CancellationToken.None);
                if (closeFrame.MessageType == WebSocketMessageType.Close)
                {
                    upstreamObservedClose.TrySetResult(socket.CloseStatus);
                    await socket.CloseOutputAsync(
                        socket.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                        socket.CloseStatusDescription ?? "ack",
                        CancellationToken.None);
                }
            });

        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body>proxy</body></html>");

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = false,
                ProxyRules = new Dictionary<string, ProxyTarget>(StringComparer.OrdinalIgnoreCase)
                {
                    ["/ws"] = new()
                    {
                        Target = upstream.BaseAddress + "backend",
                        WebSocket = true
                    }
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache());
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(
                ToWebSocketUri(server.ListeningUri!, "/ws/chat?room=1"),
                CancellationToken.None);
            await socket.SendAsync(
                Encoding.UTF8.GetBytes("ping"),
                WebSocketMessageType.Text,
                endOfMessage: true,
                CancellationToken.None);

            var responseText = await ReceiveWebSocketTextAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual("echo:ping", responseText);

            await socket.CloseOutputAsync(
                WebSocketCloseStatus.NormalClosure,
                "client-finished",
                CancellationToken.None);

            var closeStatus = await ReceiveWebSocketCloseAsync(socket, TimeSpan.FromSeconds(5));
            Assert.AreEqual(WebSocketCloseStatus.NormalClosure, closeStatus);
            Assert.AreEqual(
                WebSocketCloseStatus.NormalClosure,
                await upstreamObservedClose.Task.WaitAsync(TimeSpan.FromSeconds(2)));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_WhenWebSocketProxyDisabled_DoesNotForwardUpgradeRequest()
    {
        var rootDirectory = CreateTemporaryDirectory();
        var requestReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var upstream = await TestWebSocketUpstreamServer.StartAsync(
            (context, socket) =>
            {
                requestReceived.TrySetResult(true);
                return Task.CompletedTask;
            });

        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body>proxy</body></html>");

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = false,
                ProxyRules = new Dictionary<string, ProxyTarget>(StringComparer.OrdinalIgnoreCase)
                {
                    ["/ws"] = new()
                    {
                        Target = upstream.BaseAddress + "backend",
                        WebSocket = false
                    }
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache());
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);

            using var socket = new ClientWebSocket();
            try
            {
                await socket.ConnectAsync(
                    ToWebSocketUri(server.ListeningUri!, "/ws/chat"),
                    CancellationToken.None);
                Assert.Fail("Expected websocket upgrade to fail when proxy websocket forwarding is disabled.");
            }
            catch (WebSocketException)
            {
            }

            var completedTask = await Task.WhenAny(requestReceived.Task, Task.Delay(300));

            Assert.AreNotSame(requestReceived.Task, completedTask);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_PrefersBuiltInDevClientOverProxyRule()
    {
        var rootDirectory = CreateTemporaryDirectory();
        var requestReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var upstream = await TestUpstreamServer.StartAsync(
            context =>
            {
                requestReceived.TrySetResult(true);
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.Close();
                return Task.CompletedTask;
            });

        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body></body></html>");

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = true,
                ProxyRules = new Dictionary<string, ProxyTarget>(StringComparer.OrdinalIgnoreCase)
                {
                    ["/@jazor"] = new()
                    {
                        Target = upstream.BaseAddress
                    }
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache());
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);

            using var client = new HttpClient { BaseAddress = server.ListeningUri };
            var script = await client.GetStringAsync("/@jazor/client");
            var completedTask = await Task.WhenAny(requestReceived.Task, Task.Delay(300));

            StringAssert.Contains(script, "/@jazor/hmr");
            Assert.AreNotSame(requestReceived.Task, completedTask);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task DevHttpServer_WhenWebSocketProxyConfigured_ForwardsMessagesToUpstream()
    {
        var rootDirectory = CreateTemporaryDirectory();
        var requestReceived = new TaskCompletionSource<UpstreamWebSocketRequestSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var upstream = await TestWebSocketUpstreamServer.StartAsync(
            async (context, socket) =>
            {
                requestReceived.TrySetResult(
                    new UpstreamWebSocketRequestSnapshot(
                        context.Request.HttpMethod,
                        context.Request.Url!.AbsolutePath,
                        context.Request.Url.Query));

                var requestText = await ReceiveWebSocketTextAsync(socket, TimeSpan.FromSeconds(5));
                await socket.SendAsync(
                    Encoding.UTF8.GetBytes("echo:" + requestText),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    CancellationToken.None);
                await TryCloseWebSocketAsync(socket);
            });

        try
        {
            await File.WriteAllTextAsync(Path.Combine(rootDirectory, "index.html"), "<html><body>proxy</body></html>");

            var options = new DevServerOptions
            {
                RootDirectory = rootDirectory,
                Host = "127.0.0.1",
                Port = 0,
                HmrEnabled = false,
                ProxyRules = new Dictionary<string, ProxyTarget>(StringComparer.OrdinalIgnoreCase)
                {
                    ["/ws"] = new()
                    {
                        Target = upstream.BaseAddress + "backend",
                        WebSocket = true
                    }
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache());
            await using var server = new DevHttpServer(
                options,
                compiler,
                new ModuleResolver(rootDirectory),
                new HtmlTransformer(options));

            await server.StartAsync(CancellationToken.None);

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(
                ToWebSocketUri(server.ListeningUri!, "/ws/chat?room=1"),
                CancellationToken.None);
            await socket.SendAsync(
                Encoding.UTF8.GetBytes("ping"),
                WebSocketMessageType.Text,
                endOfMessage: true,
                CancellationToken.None);

            var responseText = await ReceiveWebSocketTextAsync(socket, TimeSpan.FromSeconds(5));
            var proxiedRequest = await requestReceived.Task.WaitAsync(TimeSpan.FromSeconds(2));

            Assert.AreEqual("echo:ping", responseText);
            Assert.AreEqual("GET", proxiedRequest.Method);
            Assert.AreEqual("/backend/chat", proxiedRequest.AbsolutePath);
            Assert.AreEqual("?room=1", proxiedRequest.Query);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_JazorFile_UsesFrontendSfcCompiler()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <div>Hello</div>
                </template>
                """);

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export const compiled = true;",
                    Dependencies = ["vue"]
                }
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                moduleResolver: moduleResolver);

            var result = await compiler.CompileAsync(documentPath, CancellationToken.None);

            Assert.IsFalse(result.IsError);
            Assert.AreEqual("text/javascript", result.ContentType);
            Assert.AreEqual("export const compiled = true;", result.Content);
            Assert.IsNotNull(result.HotReloadManifestEntry);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.HotReloadManifestEntry.DescriptorHash));
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.HotReloadManifestEntry.TemplateHash));
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.HotReloadManifestEntry.LogicHash));
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.HotReloadManifestEntry.ContentHash));
            Assert.AreEqual(RazorVueHmrBoundaryKind.TemplateOnly, result.HotReloadManifestEntry.HmrBoundaryKind);
            Assert.AreEqual("/Counter.jazor", result.HotReloadManifestEntry.ComponentId);
            Assert.AreEqual("/Counter.jazor", result.HotReloadManifestEntry.RelativeModulePath);
            Assert.AreEqual(result.ModuleSignature, result.HotReloadManifestEntry.ContentHash);
            Assert.AreEqual(1, frontendCompiler.SfcCompileCount);
            Assert.AreEqual(documentPath, frontendCompiler.LastDocumentPath);
            StringAssert.Contains(frontendCompiler.LastSfcText!, "<template>");
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_JazorFile_UsesSemanticMetadataForMultilineProps()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <div>@Title</div>
                </template>

                @code {
                    [Prop]
                    public string? Title
                    {
                        get;
                        set;
                    } = string.Empty;

                    [Prop] public IReadOnlyList<int> Items { get; set; } = [];
                }
                """);

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                moduleResolver: new ModuleResolver(rootDirectory));

            var first = await compiler.CompileAsync(documentPath, CancellationToken.None);

            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <div>@Title</div>
                </template>

                @code {
                    [Prop] public string? Title { get; set; } = "";

                    [Prop]
                    public IReadOnlyList<int> Items { get; set; } = Array.Empty<int>();
                }
                """);

            var second = await compiler.RecompileAsync(documentPath, CancellationToken.None);

            Assert.IsNotNull(first.HotReloadManifestEntry);
            Assert.IsNotNull(second.HotReloadManifestEntry);
            Assert.AreEqual(RazorVueHmrBoundaryKind.TemplateOnly, first.HotReloadManifestEntry.HmrBoundaryKind);
            Assert.AreEqual(RazorVueHmrBoundaryKind.TemplateOnly, second.HotReloadManifestEntry.HmrBoundaryKind);
            Assert.AreEqual(first.HotReloadManifestEntry.DescriptorHash, second.HotReloadManifestEntry.DescriptorHash);
            Assert.AreEqual(first.HotReloadManifestEntry.TemplateHash, second.HotReloadManifestEntry.TemplateHash);
            Assert.AreEqual(first.HotReloadManifestEntry.LogicHash, second.HotReloadManifestEntry.LogicHash);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_JazorFile_StateInitializerChange_ChangesLogicHashOnly()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                    [State] private int count = 1;
                }
                """);

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                moduleResolver: new ModuleResolver(rootDirectory));

            var first = await compiler.CompileAsync(documentPath, CancellationToken.None);

            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                    [State] private int count = Count + 1;
                }
                """);

            var second = await compiler.RecompileAsync(documentPath, CancellationToken.None);

            Assert.IsNotNull(first.HotReloadManifestEntry);
            Assert.IsNotNull(second.HotReloadManifestEntry);
            Assert.AreEqual(first.HotReloadManifestEntry.DescriptorHash, second.HotReloadManifestEntry.DescriptorHash);
            Assert.AreEqual(first.HotReloadManifestEntry.TemplateHash, second.HotReloadManifestEntry.TemplateHash);
            Assert.AreNotEqual(first.HotReloadManifestEntry.LogicHash, second.HotReloadManifestEntry.LogicHash);
            Assert.AreEqual(RazorVueHmrBoundaryKind.LogicSafe, second.HotReloadManifestEntry.HmrBoundaryKind);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_JazorFile_BlockComputedBodyChange_ChangesLogicHashOnly()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <p>@Label</p>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    [Computed]
                    public string Label
                    {
                        get
                        {
                            return $"Count: {Count}";
                        }
                    }
                }
                """);

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                moduleResolver: new ModuleResolver(rootDirectory));

            var first = await compiler.CompileAsync(documentPath, CancellationToken.None);

            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <p>@Label</p>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    [Computed]
                    public string Label
                    {
                        get
                        {
                            return $"Total: {Count}";
                        }
                    }
                }
                """);

            var second = await compiler.RecompileAsync(documentPath, CancellationToken.None);

            Assert.IsNotNull(first.HotReloadManifestEntry);
            Assert.IsNotNull(second.HotReloadManifestEntry);
            Assert.AreEqual(first.HotReloadManifestEntry.DescriptorHash, second.HotReloadManifestEntry.DescriptorHash);
            Assert.AreEqual(first.HotReloadManifestEntry.TemplateHash, second.HotReloadManifestEntry.TemplateHash);
            Assert.AreNotEqual(first.HotReloadManifestEntry.LogicHash, second.HotReloadManifestEntry.LogicHash);
            Assert.AreEqual(RazorVueHmrBoundaryKind.LogicSafe, second.HotReloadManifestEntry.HmrBoundaryKind);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_JazorFile_MethodBodyChange_ChangesLogicHashOnly()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Increment(1)</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    public int Increment(int delta)
                    {
                        return Count + delta;
                    }
                }
                """);

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                moduleResolver: new ModuleResolver(rootDirectory));

            var first = await compiler.CompileAsync(documentPath, CancellationToken.None);

            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Increment(1)</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    public int Increment(int delta)
                    {
                        return Count + delta + 1;
                    }
                }
                """);

            var second = await compiler.RecompileAsync(documentPath, CancellationToken.None);

            Assert.IsNotNull(first.HotReloadManifestEntry);
            Assert.IsNotNull(second.HotReloadManifestEntry);
            Assert.AreEqual(first.HotReloadManifestEntry.DescriptorHash, second.HotReloadManifestEntry.DescriptorHash);
            Assert.AreEqual(first.HotReloadManifestEntry.TemplateHash, second.HotReloadManifestEntry.TemplateHash);
            Assert.AreNotEqual(first.HotReloadManifestEntry.LogicHash, second.HotReloadManifestEntry.LogicHash);
            Assert.AreEqual(RazorVueHmrBoundaryKind.LogicSafe, second.HotReloadManifestEntry.HmrBoundaryKind);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_JazorFile_MethodSignatureChange_ChangesDescriptorHash()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Increment(1)</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    public int Increment(int delta)
                    {
                        return Count + delta;
                    }
                }
                """);

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                moduleResolver: new ModuleResolver(rootDirectory));

            var first = await compiler.CompileAsync(documentPath, CancellationToken.None);

            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Increment(1)</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    public int Increment(long delta)
                    {
                        return (int)(Count + delta);
                    }
                }
                """);

            var second = await compiler.RecompileAsync(documentPath, CancellationToken.None);

            Assert.IsNotNull(first.HotReloadManifestEntry);
            Assert.IsNotNull(second.HotReloadManifestEntry);
            Assert.AreNotEqual(first.HotReloadManifestEntry.DescriptorHash, second.HotReloadManifestEntry.DescriptorHash);
            Assert.AreEqual(first.HotReloadManifestEntry.TemplateHash, second.HotReloadManifestEntry.TemplateHash);
            Assert.AreNotEqual(first.HotReloadManifestEntry.LogicHash, second.HotReloadManifestEntry.LogicHash);
            Assert.AreEqual(RazorVueHmrBoundaryKind.LogicSafe, second.HotReloadManifestEntry.HmrBoundaryKind);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_JazorFile_JazorCodeBehindChange_ChangesLogicHashOnly()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var codeBehindPath = Path.Combine(rootDirectory, "Counter.jazor.cs");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                }
                """);
            await File.WriteAllTextAsync(
                codeBehindPath,
                """
                public partial class Counter
                {
                    [State] private int count = 1;
                }
                """);

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                moduleResolver: new ModuleResolver(rootDirectory));

            var first = await compiler.CompileAsync(documentPath, CancellationToken.None);

            await File.WriteAllTextAsync(
                codeBehindPath,
                """
                public partial class Counter
                {
                    [State] private int count = 2;
                }
                """);

            var second = await compiler.RecompileAsync(documentPath, CancellationToken.None);

            Assert.IsNotNull(first.HotReloadManifestEntry);
            Assert.IsNotNull(second.HotReloadManifestEntry);
            Assert.AreEqual(first.HotReloadManifestEntry.DescriptorHash, second.HotReloadManifestEntry.DescriptorHash);
            Assert.AreEqual(first.HotReloadManifestEntry.TemplateHash, second.HotReloadManifestEntry.TemplateHash);
            Assert.AreNotEqual(first.HotReloadManifestEntry.LogicHash, second.HotReloadManifestEntry.LogicHash);
            Assert.AreEqual(RazorVueHmrBoundaryKind.LogicSafe, second.HotReloadManifestEntry.HmrBoundaryKind);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_JazorFile_CoLocatedCounterCsChange_ChangesLogicHashOnly()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var codeBehindPath = Path.Combine(rootDirectory, "Counter.cs");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                }
                """);
            await File.WriteAllTextAsync(
                codeBehindPath,
                """
                public partial class Counter
                {
                    [Computed]
                    public int Total => 1;
                }
                """);

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                moduleResolver: new ModuleResolver(rootDirectory));

            var first = await compiler.CompileAsync(documentPath, CancellationToken.None);

            await File.WriteAllTextAsync(
                codeBehindPath,
                """
                public partial class Counter
                {
                    [Computed]
                    public int Total => 2;
                }
                """);

            var second = await compiler.RecompileAsync(documentPath, CancellationToken.None);

            Assert.IsNotNull(first.HotReloadManifestEntry);
            Assert.IsNotNull(second.HotReloadManifestEntry);
            Assert.AreEqual(first.HotReloadManifestEntry.DescriptorHash, second.HotReloadManifestEntry.DescriptorHash);
            Assert.AreEqual(first.HotReloadManifestEntry.TemplateHash, second.HotReloadManifestEntry.TemplateHash);
            Assert.AreNotEqual(first.HotReloadManifestEntry.LogicHash, second.HotReloadManifestEntry.LogicHash);
            Assert.AreEqual(RazorVueHmrBoundaryKind.LogicSafe, second.HotReloadManifestEntry.HmrBoundaryKind);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_SfcStyle_UsesResolvedUrlAsStyleTargetId()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.vue");
            await File.WriteAllTextAsync(documentPath, "<template><div>Hello</div></template>");

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    StyleContent = ".counter { color: red; }"
                }
            };
            var moduleResolver = new ModuleResolver(rootDirectory);
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                moduleResolver: moduleResolver);

            var result = await compiler.CompileAsync(documentPath, CancellationToken.None);

            Assert.IsFalse(result.IsError);
            StringAssert.Contains(result.Content, "const __jazorStyleId = \"/Counter.vue\";");
            StringAssert.Contains(result.Content, "style.setAttribute(\"data-jazor-vuehost\", __jazorStyleId);");
            Assert.IsFalse(result.Content.Contains(documentPath, StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_ReusesCacheForUnchangedContent()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(documentPath, "<template><div>Hello</div></template>");

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export const cached = true;"
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache());

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);
            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);

            Assert.AreEqual(1, frontendCompiler.SfcCompileCount);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_JazorStyleOnly_ChangesServedContentButPreservesJsHashes()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <div>@Count</div>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                }
                """);

            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    StyleContent = ".counter { color: red; }",
                    SupportsHmr = true
                });
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                moduleResolver: new ModuleResolver(rootDirectory));

            var first = await compiler.CompileAsync(documentPath, CancellationToken.None);

            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    StyleContent = ".counter { color: blue; }",
                    SupportsHmr = true
                });
            var second = await compiler.RecompileAsync(documentPath, CancellationToken.None);

            Assert.AreNotEqual(first.Content, second.Content);
            Assert.AreEqual(first.ModuleSignature, second.ModuleSignature);
            Assert.IsNotNull(first.HotReloadManifestEntry);
            Assert.IsNotNull(second.HotReloadManifestEntry);
            Assert.AreEqual(first.HotReloadManifestEntry.ContentHash, second.HotReloadManifestEntry.ContentHash);
            Assert.AreEqual(second.ModuleSignature, second.HotReloadManifestEntry.ContentHash);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_RecordsDependencies()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.vue");
            await File.WriteAllTextAsync(documentPath, "<template><div>Hello</div></template>");

            var childPath = Path.Combine(rootDirectory, "Child.vue");
            await File.WriteAllTextAsync(childPath, "<template><div>Child</div></template>");

            var graph = new DependencyGraph(new ModuleResolver(rootDirectory));
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default {};",
                    Dependencies = ["vue", "./Child.vue"]
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph);

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);

            Assert.IsTrue(graph.GetDependencies(documentPath).Contains(childPath));
            Assert.IsTrue(graph.GetDependents(childPath).Contains(documentPath));
            Assert.AreEqual(1, graph.GetDependencies(documentPath).Count);

            compiler.Invalidate(documentPath);

            Assert.AreEqual(0, graph.GetDependencies(documentPath).Count);
            Assert.AreEqual(0, graph.GetDependents(childPath).Count);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessChanges_WhenVueComponentChanges_ReturnsJavaScriptUpdateWithoutInvalidatingDependents()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var mainPath = Path.Combine(rootDirectory, "main.ts");
            var appPath = Path.Combine(rootDirectory, "App.vue");
            var childPath = Path.Combine(rootDirectory, "Child.vue");
            await File.WriteAllTextAsync(mainPath, "import App from './App.vue'; export { App };");
            await File.WriteAllTextAsync(appPath, "<template><div>App</div></template>");
            await File.WriteAllTextAsync(childPath, "<template><div>Child</div></template>");

            var graph = new DependencyGraph(new ModuleResolver(rootDirectory));
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                TypeScriptResult = new FrontendModuleCompilation
                {
                    JavaScript = "import App from './App.vue'; export { App };",
                    Dependencies = ["./App.vue"]
                }
            };
            frontendCompiler.SetSfcResult(
                appPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "import Child from './Child.vue'; export default { components: { Child } };",
                    Dependencies = ["./Child.vue"]
                });
            frontendCompiler.SetSfcResult(
                childPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Child' };",
                    SupportsHmr = true
                });
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph);

            _ = await compiler.CompileAsync(mainPath, CancellationToken.None);
            _ = await compiler.CompileAsync(appPath, CancellationToken.None);
            _ = await compiler.CompileAsync(childPath, CancellationToken.None);

            Assert.AreEqual(1, frontendCompiler.TypeScriptCompileCount);
            Assert.AreEqual(2, frontendCompiler.SfcCompileCount);

            frontendCompiler.SetSfcResult(
                childPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'ChildUpdated' };",
                    SupportsHmr = true
                });
            await File.WriteAllTextAsync(childPath, "<template><div>Child updated</div></template>");

            var processor = new ChangeProcessor(compiler, new ModuleResolver(rootDirectory), graph);
            var result = await processor.ProcessChangesAsync([childPath], CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.JavaScriptUpdate, result.UpdateKind);
            Assert.IsNull(result.FullReloadReason);
            CollectionAssert.AreEquivalent(
                new[] { childPath, appPath, mainPath },
                result.AffectedPaths.ToArray());
            Assert.AreEqual(1, result.JavaScriptUpdates.Count);
            Assert.AreEqual("/Child.vue", result.JavaScriptUpdates[0].Path);
            Assert.AreEqual("/Child.vue", result.JavaScriptUpdates[0].AcceptedPath);

            _ = await compiler.CompileAsync(mainPath, CancellationToken.None);
            _ = await compiler.CompileAsync(appPath, CancellationToken.None);
            _ = await compiler.CompileAsync(childPath, CancellationToken.None);

            Assert.AreEqual(1, frontendCompiler.TypeScriptCompileCount);
            Assert.AreEqual(3, frontendCompiler.SfcCompileCount);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessChanges_WhenTypeScriptChanges_ReturnsJavaScriptUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var mainPath = Path.Combine(rootDirectory, "main.ts");
            var utilPath = Path.Combine(rootDirectory, "util.ts");
            await File.WriteAllTextAsync(mainPath, "import { count } from './util'; export { count };");
            await File.WriteAllTextAsync(utilPath, "export const count: number = 1;");

            var moduleResolver = new ModuleResolver(rootDirectory);
            var graph = new DependencyGraph(moduleResolver);
            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetTypeScriptResult(
                mainPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "import { count } from './util.ts'; export { count };",
                    Dependencies = ["./util.ts"]
                });
            frontendCompiler.SetTypeScriptResult(
                utilPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export const count = 1;",
                    Dependencies = []
                });
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph,
                moduleResolver);

            _ = await compiler.CompileAsync(mainPath, CancellationToken.None);
            _ = await compiler.CompileAsync(utilPath, CancellationToken.None);

            Assert.AreEqual(2, frontendCompiler.TypeScriptCompileCount);

            frontendCompiler.SetTypeScriptResult(
                utilPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export const count = 2;",
                    Dependencies = []
                });
            await File.WriteAllTextAsync(utilPath, "export const count: number = 2;");

            var processor = new ChangeProcessor(compiler, moduleResolver, graph);
            var result = await processor.ProcessChangesAsync([utilPath], CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.JavaScriptUpdate, result.UpdateKind);
            Assert.IsNull(result.FullReloadReason);
            CollectionAssert.AreEquivalent(
                new[] { utilPath, mainPath },
                result.AffectedPaths.ToArray());
            Assert.AreEqual(1, result.JavaScriptUpdates.Count);
            Assert.AreEqual("/util.ts", result.JavaScriptUpdates[0].Path);
            Assert.AreEqual("/util.ts", result.JavaScriptUpdates[0].AcceptedPath);

            _ = await compiler.CompileAsync(mainPath, CancellationToken.None);
            _ = await compiler.CompileAsync(utilPath, CancellationToken.None);

            Assert.AreEqual(3, frontendCompiler.TypeScriptCompileCount);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessChanges_WhenJazorTemplateChanges_ReturnsJavaScriptUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <div>Hello</div>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                }
                """);

            var graph = new DependencyGraph(new ModuleResolver(rootDirectory));
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', render() { return null; } };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph);

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);

            frontendCompiler.SfcResult = new FrontendModuleCompilation
            {
                JavaScript = "export default { name: 'Counter', render() { return 'updated'; } };",
                SupportsHmr = true
            };
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <div>Hello updated</div>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                }
                """);

            var processor = new ChangeProcessor(compiler, new ModuleResolver(rootDirectory), graph);
            var result = await processor.ProcessChangesAsync([documentPath], CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.JavaScriptUpdate, result.UpdateKind);
            Assert.IsNull(result.FullReloadReason);
            Assert.AreEqual(1, result.JavaScriptUpdates.Count);
            Assert.AreEqual("/Counter.jazor", result.JavaScriptUpdates[0].Path);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessChanges_WhenJazorDescriptorChanges_ReturnsFullReload()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <div>Hello</div>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                }
                """);

            var graph = new DependencyGraph(new ModuleResolver(rootDirectory));
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', render() { return null; } };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph);

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);

            frontendCompiler.SfcResult = new FrontendModuleCompilation
            {
                JavaScript = "export default { name: 'Counter', render() { return 'updated'; } };",
                SupportsHmr = true
            };
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <div>Hello updated</div>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                    [Prop] public string Title { get; set; } = string.Empty;
                }
                """);

            var processor = new ChangeProcessor(compiler, new ModuleResolver(rootDirectory), graph);
            var result = await processor.ProcessChangesAsync([documentPath], CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.FullReload, result.UpdateKind);
            Assert.AreEqual("Public component descriptor changed.", result.FullReloadReason);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessChanges_WhenJazorLogicChangesInsideLogicSafeBoundary_ReturnsJavaScriptUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    public int Increment()
                    {
                        return Count + 1;
                    }
                }
                """);

            var graph = new DependencyGraph(new ModuleResolver(rootDirectory));
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', methods: { increment() { return 1; } } };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph,
                new ModuleResolver(rootDirectory));

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);

            frontendCompiler.SfcResult = new FrontendModuleCompilation
            {
                JavaScript = "export default { name: 'Counter', methods: { increment() { return 2; } } };",
                SupportsHmr = true
            };
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    public int Increment()
                    {
                        return Count + 2;
                    }
                }
                """);

            var processor = new ChangeProcessor(compiler, new ModuleResolver(rootDirectory), graph);
            var result = await processor.ProcessChangesAsync([documentPath], CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.JavaScriptUpdate, result.UpdateKind);
            Assert.IsNull(result.FullReloadReason);
            Assert.AreEqual(1, result.JavaScriptUpdates.Count);
            Assert.AreEqual("/Counter.jazor", result.JavaScriptUpdates[0].Path);
            Assert.AreEqual("/Counter.jazor", result.JavaScriptUpdates[0].AcceptedPath);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessChanges_WhenJazorMethodSignatureChanges_ReturnsFullReload()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Increment(1)</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    public int Increment(int delta)
                    {
                        return Count + delta;
                    }
                }
                """);

            var graph = new DependencyGraph(new ModuleResolver(rootDirectory));
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', methods: { increment(delta) { return delta; } } };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph,
                new ModuleResolver(rootDirectory));

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);

            frontendCompiler.SfcResult = new FrontendModuleCompilation
            {
                JavaScript = "export default { name: 'Counter', methods: { increment(delta) { return Number(delta); } } };",
                SupportsHmr = true
            };
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Increment(1)</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    public int Increment(long delta)
                    {
                        return (int)(Count + delta);
                    }
                }
                """);

            var processor = new ChangeProcessor(compiler, new ModuleResolver(rootDirectory), graph);
            var result = await processor.ProcessChangesAsync([documentPath], CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.FullReload, result.UpdateKind);
            Assert.AreEqual("Public component descriptor changed.", result.FullReloadReason);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessWorkspaceDocumentChange_WhenJazorLogicChangesInsideLogicSafeBoundary_ReturnsJavaScriptUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var originalText =
                """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    public int Increment()
                    {
                        return Count + 1;
                    }
                }
                """;
            await File.WriteAllTextAsync(documentPath, originalText);

            var moduleResolver = new ModuleResolver(rootDirectory);
            var graph = new DependencyGraph(moduleResolver);
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', methods: { increment() { return 1; } } };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph,
                moduleResolver);

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);

            frontendCompiler.SfcResult = new FrontendModuleCompilation
            {
                JavaScript = "export default { name: 'Counter', methods: { increment() { return 2; } } };",
                SupportsHmr = true
            };
            var updatedText =
                """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    public int Increment()
                    {
                        return Count + 2;
                    }
                }
                """;

            var processor = new ChangeProcessor(compiler, moduleResolver, graph);
            var result = await processor.ProcessWorkspaceDocumentChangeAsync(
                new DocumentSnapshot(documentPath, DocumentKind.Jazor, updatedText, version: "2"),
                CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.JavaScriptUpdate, result.UpdateKind);
            Assert.IsNull(result.FullReloadReason);
            Assert.AreEqual(1, result.JavaScriptUpdates.Count);
            Assert.AreEqual("/Counter.jazor", result.JavaScriptUpdates[0].Path);
            Assert.AreEqual(2, frontendCompiler.SfcCompileCount);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessWorkspaceDocumentChange_WhenJazorDescriptorChanges_ReturnsFullReload()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var originalText =
                """
                <template>
                  <div>Hello</div>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                }
                """;
            await File.WriteAllTextAsync(documentPath, originalText);

            var graph = new DependencyGraph(new ModuleResolver(rootDirectory));
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', render() { return null; } };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph);

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);

            frontendCompiler.SfcResult = new FrontendModuleCompilation
            {
                JavaScript = "export default { name: 'Counter', render() { return 'updated'; } };",
                SupportsHmr = true
            };
            var updatedText =
                """
                <template>
                  <div>Hello updated</div>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                    [Prop] public string Title { get; set; } = string.Empty;
                }
                """;

            var processor = new ChangeProcessor(compiler, new ModuleResolver(rootDirectory), graph);
            var result = await processor.ProcessWorkspaceDocumentChangeAsync(
                new DocumentSnapshot(documentPath, DocumentKind.Jazor, updatedText, version: "2"),
                CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.FullReload, result.UpdateKind);
            Assert.AreEqual("Public component descriptor changed.", result.FullReloadReason);
            Assert.AreEqual(2, frontendCompiler.SfcCompileCount);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessWorkspaceDocumentChange_WhenTypeScriptChanges_UsesWorkspaceTextAndReturnsJavaScriptUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var utilPath = Path.Combine(rootDirectory, "util.ts");
            await File.WriteAllTextAsync(utilPath, "export const count: number = 1;");

            var moduleResolver = new ModuleResolver(rootDirectory);
            var graph = new DependencyGraph(moduleResolver);
            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetTypeScriptResult(
                utilPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export const count = 1;",
                    Dependencies = []
                });

            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph,
                moduleResolver);

            _ = await compiler.CompileAsync(utilPath, CancellationToken.None);

            var updatedSource = "export const count: number = 2;";
            frontendCompiler.SetTypeScriptResult(
                utilPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export const count = 2;",
                    Dependencies = []
                });

            var processor = new ChangeProcessor(compiler, moduleResolver, graph);
            var result = await processor.ProcessWorkspaceDocumentChangeAsync(
                new DocumentSnapshot(utilPath, DocumentKind.TypeScript, updatedSource, version: "2"),
                CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.JavaScriptUpdate, result.UpdateKind);
            Assert.IsNull(result.FullReloadReason);
            Assert.AreEqual(1, result.JavaScriptUpdates.Count);
            Assert.AreEqual("/util.ts", result.JavaScriptUpdates[0].Path);
            Assert.AreEqual(updatedSource, frontendCompiler.LastTypeScriptText);
            Assert.AreEqual(2, frontendCompiler.TypeScriptCompileCount);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessChanges_WhenJazorCodeBehindChanges_RoutesThroughOwningJazorHotUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var codeBehindPath = Path.Combine(rootDirectory, "Counter.jazor.cs");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                }
                """);
            await File.WriteAllTextAsync(
                codeBehindPath,
                """
                public partial class Counter
                {
                    [State] private int count = 1;
                }
                """);

            var graph = new DependencyGraph(new ModuleResolver(rootDirectory));
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', setup() { return { count: 1 }; } };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph,
                new ModuleResolver(rootDirectory));

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);

            frontendCompiler.SfcResult = new FrontendModuleCompilation
            {
                JavaScript = "export default { name: 'Counter', setup() { return { count: 2 }; } };",
                SupportsHmr = true
            };
            await File.WriteAllTextAsync(
                codeBehindPath,
                """
                public partial class Counter
                {
                    [State] private int count = 2;
                }
                """);

            var processor = new ChangeProcessor(compiler, new ModuleResolver(rootDirectory), graph);
            var result = await processor.ProcessChangesAsync([codeBehindPath], CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.JavaScriptUpdate, result.UpdateKind);
            Assert.IsNull(result.FullReloadReason);
            CollectionAssert.AreEquivalent(
                new[] { codeBehindPath, documentPath },
                result.AffectedPaths.ToArray());
            Assert.AreEqual(1, result.JavaScriptUpdates.Count);
            Assert.AreEqual("/Counter.jazor", result.JavaScriptUpdates[0].Path);
            CollectionAssert.AreEqual(new[] { codeBehindPath }, result.ChangedPaths.ToArray());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessWorkspaceDocumentChange_WhenUnsavedJazorCodeBehindLogicChanges_RoutesThroughOwningJazorHotUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var codeBehindPath = Path.Combine(rootDirectory, "Counter.jazor.cs");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                }
                """);
            await File.WriteAllTextAsync(
                codeBehindPath,
                """
                public partial class Counter
                {
                    [State] private int count = 1;
                }
                """);

            var moduleResolver = new ModuleResolver(rootDirectory);
            var graph = new DependencyGraph(moduleResolver);
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', setup() { return { count: 1 }; } };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph,
                moduleResolver);

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);

            frontendCompiler.SfcResult = new FrontendModuleCompilation
            {
                JavaScript = "export default { name: 'Counter', setup() { return { count: 2 }; } };",
                SupportsHmr = true
            };
            var updatedCodeBehindText =
                """
                public partial class Counter
                {
                    [State] private int count = 2;
                }
                """;

            var processor = new ChangeProcessor(compiler, moduleResolver, graph);
            var result = await processor.ProcessWorkspaceDocumentChangeAsync(
                new DocumentSnapshot(codeBehindPath, DocumentKind.CSharp, updatedCodeBehindText, version: "2"),
                CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.JavaScriptUpdate, result.UpdateKind);
            Assert.IsNull(result.FullReloadReason);
            CollectionAssert.AreEquivalent(
                new[] { codeBehindPath, documentPath },
                result.AffectedPaths.ToArray());
            Assert.AreEqual(1, result.JavaScriptUpdates.Count);
            Assert.AreEqual("/Counter.jazor", result.JavaScriptUpdates[0].Path);
            CollectionAssert.AreEqual(new[] { codeBehindPath }, result.ChangedPaths.ToArray());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessWorkspaceDocumentChange_WhenUnsavedJazorCodeBehindSignatureChanges_ReturnsFullReload()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var codeBehindPath = Path.Combine(rootDirectory, "Counter.jazor.cs");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Increment(1)</button>
                </template>
                """);
            await File.WriteAllTextAsync(
                codeBehindPath,
                """
                public partial class Counter
                {
                    public int Increment(int delta)
                    {
                        return delta + 1;
                    }
                }
                """);

            var moduleResolver = new ModuleResolver(rootDirectory);
            var graph = new DependencyGraph(moduleResolver);
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', methods: { increment(delta) { return delta + 1; } } };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph,
                moduleResolver);

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);

            frontendCompiler.SfcResult = new FrontendModuleCompilation
            {
                JavaScript = "export default { name: 'Counter', methods: { increment(delta) { return Number(delta) + 1; } } };",
                SupportsHmr = true
            };
            var updatedCodeBehindText =
                """
                public partial class Counter
                {
                    public long Increment(long delta)
                    {
                        return delta + 1;
                    }
                }
                """;

            var processor = new ChangeProcessor(compiler, moduleResolver, graph);
            var result = await processor.ProcessWorkspaceDocumentChangeAsync(
                new DocumentSnapshot(codeBehindPath, DocumentKind.CSharp, updatedCodeBehindText, version: "2"),
                CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.FullReload, result.UpdateKind);
            Assert.AreEqual("Public component descriptor changed.", result.FullReloadReason);
            CollectionAssert.AreEquivalent(
                new[] { codeBehindPath, documentPath },
                result.AffectedPaths.ToArray());
            CollectionAssert.AreEqual(new[] { codeBehindPath }, result.ChangedPaths.ToArray());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessWorkspaceDocumentChange_WhenJazorAndCodeBehindAreBothOpenUnsaved_UsesWorkspaceSnapshotsForClassification()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var codeBehindPath = Path.Combine(rootDirectory, "Counter.jazor.cs");
            var diskJazorText =
                """
                <template>
                  <button>@Increment(1)</button>
                </template>
                """;
            await File.WriteAllTextAsync(documentPath, diskJazorText);
            await File.WriteAllTextAsync(
                codeBehindPath,
                """
                public partial class Counter
                {
                    public int Increment(int delta)
                    {
                        return delta + 1;
                    }
                }
                """);

            var moduleResolver = new ModuleResolver(rootDirectory);
            var graph = new DependencyGraph(moduleResolver);
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', methods: { increment(delta) { return delta + 1; } } };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph,
                moduleResolver);

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);

            frontendCompiler.SfcResult = new FrontendModuleCompilation
            {
                JavaScript = "export default { name: 'Counter', methods: { increment(delta) { return Number(delta) + 2; } } };",
                SupportsHmr = true
            };
            var unsavedJazorText =
                """
                <template>
                  <button>@Increment(1)</button>
                </template>

                @code {
                    [Prop] public string Title { get; set; } = string.Empty;
                }
                """;
            var unsavedCodeBehindText =
                """
                public partial class Counter
                {
                    public long Increment(long delta)
                    {
                        return delta + 2;
                    }
                }
                """;

            var processor = new ChangeProcessor(compiler, moduleResolver, graph);
            var result = await processor.ProcessWorkspaceDocumentChangeAsync(
                new DocumentSnapshot(codeBehindPath, DocumentKind.CSharp, unsavedCodeBehindText, version: "2"),
                [
                    new DocumentSnapshot(documentPath, DocumentKind.Jazor, unsavedJazorText, version: "3"),
                    new DocumentSnapshot(codeBehindPath, DocumentKind.CSharp, unsavedCodeBehindText, version: "2")
                ],
                CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.FullReload, result.UpdateKind);
            Assert.AreEqual("Public component descriptor changed.", result.FullReloadReason);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessChanges_WhenJazorCodeBehindFileIsRenamed_ReturnsFullReload()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var originalCodeBehindPath = Path.Combine(rootDirectory, "Counter.jazor.cs");
            var renamedCodeBehindPath = Path.Combine(rootDirectory, "CounterRenamed.jazor.cs");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                }
                """);
            await File.WriteAllTextAsync(
                originalCodeBehindPath,
                """
                public partial class Counter
                {
                    [State] private int count = 1;
                }
                """);

            var graph = new DependencyGraph(new ModuleResolver(rootDirectory));
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', setup() { return { count: 1 }; } };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph,
                new ModuleResolver(rootDirectory));

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);

            File.Move(originalCodeBehindPath, renamedCodeBehindPath);

            var processor = new ChangeProcessor(compiler, new ModuleResolver(rootDirectory), graph);
            var result = await processor.ProcessChangesAsync(
                [originalCodeBehindPath, renamedCodeBehindPath],
                CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.FullReload, result.UpdateKind);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.FullReloadReason));
            CollectionAssert.AreEquivalent(
                new[] { originalCodeBehindPath, renamedCodeBehindPath, documentPath },
                result.AffectedPaths.Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
            CollectionAssert.AreEquivalent(
                new[] { originalCodeBehindPath, renamedCodeBehindPath },
                result.ChangedPaths.ToArray());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessChanges_WhenCoLocatedCounterCsChanges_RoutesThroughOwningJazorHotUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var codeBehindPath = Path.Combine(rootDirectory, "Counter.cs");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                }
                """);
            await File.WriteAllTextAsync(
                codeBehindPath,
                """
                public partial class Counter
                {
                    [Computed]
                    public int Total => 1;
                }
                """);

            var graph = new DependencyGraph(new ModuleResolver(rootDirectory));
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', computed: { total() { return 1; } } };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph,
                new ModuleResolver(rootDirectory));

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);

            frontendCompiler.SfcResult = new FrontendModuleCompilation
            {
                JavaScript = "export default { name: 'Counter', computed: { total() { return 2; } } };",
                SupportsHmr = true
            };
            await File.WriteAllTextAsync(
                codeBehindPath,
                """
                public partial class Counter
                {
                    [Computed]
                    public int Total => 2;
                }
                """);

            var processor = new ChangeProcessor(compiler, new ModuleResolver(rootDirectory), graph);
            var result = await processor.ProcessChangesAsync([codeBehindPath], CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.JavaScriptUpdate, result.UpdateKind);
            Assert.IsNull(result.FullReloadReason);
            CollectionAssert.AreEquivalent(
                new[] { codeBehindPath, documentPath },
                result.AffectedPaths.ToArray());
            Assert.AreEqual(1, result.JavaScriptUpdates.Count);
            Assert.AreEqual("/Counter.jazor", result.JavaScriptUpdates[0].Path);
            CollectionAssert.AreEqual(new[] { codeBehindPath }, result.ChangedPaths.ToArray());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessChanges_WhenJazorModuleContentChangesOutsideSplitHashes_ReturnsFullReload()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    public int Increment()
                    {
                        return Count + 1;
                    }
                }
                """);

            var graph = new DependencyGraph(new ModuleResolver(rootDirectory));
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', methods: { increment() { return 1; } } };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph,
                new ModuleResolver(rootDirectory));

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);

            frontendCompiler.SfcResult = new FrontendModuleCompilation
            {
                JavaScript = "export default { name: 'Counter', methods: { increment() { return 1; } }, __build: '2' };",
                SupportsHmr = true
            };
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [Prop] public int Count { get; set; }

                    public int Increment()
                    {
                        return Count + 1;
                    }
                }
                """);

            var processor = new ChangeProcessor(compiler, new ModuleResolver(rootDirectory), graph);
            var result = await processor.ProcessChangesAsync([documentPath], CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.FullReload, result.UpdateKind);
            Assert.AreEqual("Module content changed outside split hash classification.", result.FullReloadReason);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessChanges_WhenJazorTemplateOnlyComponentChanges_ReturnsJavaScriptUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <div>Hello</div>
                </template>
                """);

            var graph = new DependencyGraph(new ModuleResolver(rootDirectory));
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter', render() { return null; } };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph,
                new ModuleResolver(rootDirectory));

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);

            frontendCompiler.SfcResult = new FrontendModuleCompilation
            {
                JavaScript = "export default { name: 'Counter', render() { return 'updated'; } };",
                SupportsHmr = true
            };
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <div>Hello updated</div>
                </template>
                """);

            var processor = new ChangeProcessor(compiler, new ModuleResolver(rootDirectory), graph);
            var result = await processor.ProcessChangesAsync([documentPath], CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.JavaScriptUpdate, result.UpdateKind);
            Assert.IsNull(result.FullReloadReason);
            Assert.AreEqual(1, result.JavaScriptUpdates.Count);
            Assert.AreEqual("/Counter.jazor", result.JavaScriptUpdates[0].Path);
            Assert.AreEqual("/Counter.jazor", result.JavaScriptUpdates[0].AcceptedPath);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessChanges_WhenJazorImportContractChanges_ReturnsFullReload()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                documentPath,
                """
                @module helper from "./helper-a"

                <template>
                  <div>@Count</div>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                }
                """);

            var graph = new DependencyGraph(new ModuleResolver(rootDirectory));
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "import helper from './helper-a'; export default { name: 'Counter' };",
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph,
                new ModuleResolver(rootDirectory));

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);

            frontendCompiler.SfcResult = new FrontendModuleCompilation
            {
                JavaScript = "import helper from './helper-b'; export default { name: 'Counter' };",
                SupportsHmr = true
            };
            await File.WriteAllTextAsync(
                documentPath,
                """
                @module helper from "./helper-b"

                <template>
                  <div>@Count</div>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                }
                """);

            var processor = new ChangeProcessor(compiler, new ModuleResolver(rootDirectory), graph);
            var result = await processor.ProcessChangesAsync([documentPath], CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.FullReload, result.UpdateKind);
            Assert.AreEqual("Component identity or host contract changed.", result.FullReloadReason);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessChanges_WhenJazorStyleOnly_ReturnsInlineStyleUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(
                documentPath,
                """
                <template>
                  <div>@Count</div>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                }
                """);

            var graph = new DependencyGraph(new ModuleResolver(rootDirectory));
            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    StyleContent = ".counter { color: red; }",
                    SupportsHmr = true
                });
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph,
                new ModuleResolver(rootDirectory));

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);

            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    StyleContent = ".counter { color: blue; }",
                    SupportsHmr = true
                });

            var processor = new ChangeProcessor(compiler, new ModuleResolver(rootDirectory), graph);
            var result = await processor.ProcessChangesAsync([documentPath], CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.StyleUpdate, result.UpdateKind);
            Assert.IsNull(result.FullReloadReason);
            Assert.AreEqual(1, result.InlineStyleUpdates.Count);
            Assert.AreEqual("/Counter.jazor", result.InlineStyleUpdates[0].TargetId);
            Assert.AreEqual(".counter { color: blue; }", result.InlineStyleUpdates[0].Content);
            Assert.AreEqual(0, result.JavaScriptUpdates.Count);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessChanges_WhenJazorStyleOnlyWithDependents_StillReturnsInlineStyleUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var mainPath = Path.Combine(rootDirectory, "main.ts");
            var appPath = Path.Combine(rootDirectory, "App.vue");
            var counterPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(mainPath, "import App from './App.vue'; export { App };");
            await File.WriteAllTextAsync(appPath, "<template><Counter /></template>");
            await File.WriteAllTextAsync(
                counterPath,
                """
                <template>
                  <div>@Count</div>
                </template>

                @code {
                    [Prop] public int Count { get; set; }
                }
                """);

            var moduleResolver = new ModuleResolver(rootDirectory);
            var graph = new DependencyGraph(moduleResolver);
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                TypeScriptResult = new FrontendModuleCompilation
                {
                    JavaScript = "import App from './App.vue'; export { App };",
                    Dependencies = ["./App.vue"]
                }
            };
            frontendCompiler.SetSfcResult(
                appPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "import Counter from './Counter.jazor'; export default { components: { Counter } };",
                    Dependencies = ["./Counter.jazor"],
                    SupportsHmr = true
                });
            frontendCompiler.SetSfcResult(
                counterPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    StyleContent = ".counter { color: red; }",
                    SupportsHmr = true
                });
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph,
                moduleResolver);

            _ = await compiler.CompileAsync(mainPath, CancellationToken.None);
            _ = await compiler.CompileAsync(appPath, CancellationToken.None);
            _ = await compiler.CompileAsync(counterPath, CancellationToken.None);

            frontendCompiler.SetSfcResult(
                counterPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    StyleContent = ".counter { color: blue; }",
                    SupportsHmr = true
                });

            var processor = new ChangeProcessor(compiler, moduleResolver, graph);
            var result = await processor.ProcessChangesAsync([counterPath], CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.StyleUpdate, result.UpdateKind);
            Assert.IsNull(result.FullReloadReason);
            CollectionAssert.AreEquivalent(
                new[] { counterPath, appPath, mainPath },
                result.AffectedPaths.ToArray());
            Assert.AreEqual(1, result.InlineStyleUpdates.Count);
            Assert.AreEqual("/Counter.jazor", result.InlineStyleUpdates[0].TargetId);
            Assert.AreEqual(".counter { color: blue; }", result.InlineStyleUpdates[0].Content);
            Assert.AreEqual(0, result.JavaScriptUpdates.Count);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessChanges_WhenIndexHtmlChanges_ReturnsClassifiedFullReload()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var htmlPath = Path.Combine(rootDirectory, "index.html");
            await File.WriteAllTextAsync(htmlPath, "<html></html>");

            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache(),
                new DependencyGraph(new ModuleResolver(rootDirectory)));
            var processor = new ChangeProcessor(
                compiler,
                new ModuleResolver(rootDirectory),
                compiler.DependencyGraph!);

            var result = await processor.ProcessChangesAsync([htmlPath], CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.FullReload, result.UpdateKind);
            Assert.AreEqual("index-html-change", result.FullReloadReason);
            CollectionAssert.AreEqual(new[] { htmlPath }, result.ChangedPaths.ToArray());
            CollectionAssert.AreEqual(new[] { htmlPath }, result.AffectedPaths.ToArray());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessChanges_WhenConfigChanges_ReturnsClassifiedFullReload()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var configPath = Path.Combine(rootDirectory, "jazor.config.json");
            await File.WriteAllTextAsync(configPath, "{}");

            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache(),
                new DependencyGraph(new ModuleResolver(rootDirectory)));
            var processor = new ChangeProcessor(
                compiler,
                new ModuleResolver(rootDirectory),
                compiler.DependencyGraph!);

            var result = await processor.ProcessChangesAsync([configPath], CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.FullReload, result.UpdateKind);
            Assert.AreEqual("config-change", result.FullReloadReason);
            CollectionAssert.AreEqual(new[] { configPath }, result.AffectedPaths.ToArray());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessChanges_WhenFileIsMissing_ReturnsMissingFileReload()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var deletedPath = Path.Combine(rootDirectory, "Deleted.vue");

            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache(),
                new DependencyGraph(new ModuleResolver(rootDirectory)));
            var processor = new ChangeProcessor(
                compiler,
                new ModuleResolver(rootDirectory),
                compiler.DependencyGraph!);

            var result = await processor.ProcessChangesAsync([deletedPath], CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.FullReload, result.UpdateKind);
            Assert.AreEqual("missing-file-change", result.FullReloadReason);
            CollectionAssert.AreEqual(new[] { deletedPath }, result.AffectedPaths.ToArray());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessChanges_WhenCssOnly_ReturnsStyleUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var stylePath = Path.Combine(rootDirectory, "site.css");
            await File.WriteAllTextAsync(stylePath, "body { color: red; }");

            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                new FakeFrontendModuleCompiler(),
                new CompilationCache(),
                new DependencyGraph(new ModuleResolver(rootDirectory)));
            var processor = new ChangeProcessor(
                compiler,
                new ModuleResolver(rootDirectory),
                compiler.DependencyGraph!);

            var result = await processor.ProcessChangesAsync([stylePath], CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.StyleUpdate, result.UpdateKind);
            Assert.IsNull(result.FullReloadReason);
            CollectionAssert.AreEqual(new[] { stylePath }, result.AffectedPaths.ToArray());
            CollectionAssert.AreEqual(new[] { "/site.css" }, result.ChangedCssUrls.ToArray());
            Assert.AreEqual(0, result.InlineStyleUpdates.Count);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessChanges_WhenCssHasDependents_ReturnsStyleUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var mainPath = Path.Combine(rootDirectory, "main.ts");
            var stylePath = Path.Combine(rootDirectory, "site.css");
            await File.WriteAllTextAsync(mainPath, "import './site.css'; export {}; ");
            await File.WriteAllTextAsync(stylePath, "body { color: red; }");

            var graph = new DependencyGraph(new ModuleResolver(rootDirectory));
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                TypeScriptResult = new FrontendModuleCompilation
                {
                    JavaScript = "import './site.css'; export {};",
                    Dependencies = ["./site.css"]
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph);
            _ = await compiler.CompileAsync(mainPath, CancellationToken.None);

            var processor = new ChangeProcessor(compiler, new ModuleResolver(rootDirectory), graph);
            var result = await processor.ProcessChangesAsync([stylePath], CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.StyleUpdate, result.UpdateKind);
            Assert.IsNull(result.FullReloadReason);
            CollectionAssert.AreEquivalent(
                new[] { stylePath, mainPath },
                result.AffectedPaths.ToArray());
            CollectionAssert.AreEqual(new[] { "/site.css" }, result.ChangedCssUrls.ToArray());
            Assert.AreEqual(0, result.InlineStyleUpdates.Count);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task ChangeProcessor_ProcessChanges_WhenSfcStyleOnly_ReturnsInlineStyleUpdate()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.vue");
            await File.WriteAllTextAsync(documentPath, "<template><div>Counter</div></template>");

            var graph = new DependencyGraph(new ModuleResolver(rootDirectory));
            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    StyleContent = ".counter { color: red; }"
                });
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                graph);

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);

            frontendCompiler.SetSfcResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    StyleContent = ".counter { color: blue; }"
                });

            var processor = new ChangeProcessor(compiler, new ModuleResolver(rootDirectory), graph);
            var result = await processor.ProcessChangesAsync([documentPath], CancellationToken.None);

            Assert.AreEqual(ChangeUpdateKind.StyleUpdate, result.UpdateKind);
            Assert.IsNull(result.FullReloadReason);
            Assert.AreEqual(0, result.ChangedCssUrls.Count);
            Assert.AreEqual(1, result.InlineStyleUpdates.Count);
            Assert.AreEqual("/Counter.vue", result.InlineStyleUpdates[0].TargetId);
            Assert.AreEqual(".counter { color: blue; }", result.InlineStyleUpdates[0].Content);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_RecompilesWhenContentChanges()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(documentPath, "<template><div>One</div></template>");

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export const version = 1;"
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache());

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);
            frontendCompiler.SfcResult = new FrontendModuleCompilation
            {
                JavaScript = "export const version = 2;"
            };
            await File.WriteAllTextAsync(documentPath, "<template><div>Two</div></template>");

            var updated = await compiler.CompileAsync(documentPath, CancellationToken.None);

            Assert.AreEqual(2, frontendCompiler.SfcCompileCount);
            Assert.AreEqual("export const version = 2;", updated.Content);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_Invalidate_ForcesRecompileWithoutContentChange()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            await File.WriteAllTextAsync(documentPath, "<template><div>One</div></template>");

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export const version = 1;"
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache());

            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);
            compiler.Invalidate(documentPath);
            _ = await compiler.CompileAsync(documentPath, CancellationToken.None);

            Assert.AreEqual(2, frontendCompiler.SfcCompileCount);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_VueFile_UsesFrontendSfcCompiler()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.vue");
            var source = "<template><div>Hello from Vue</div></template>";
            await File.WriteAllTextAsync(documentPath, source);

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    Dependencies = ["vue"],
                    SupportsHmr = true
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache());

            var result = await compiler.CompileAsync(documentPath, CancellationToken.None);

            Assert.IsFalse(result.IsError);
            Assert.AreEqual("text/javascript", result.ContentType);
            Assert.AreEqual("export default { name: 'Counter' };", result.Content);
            Assert.AreEqual(1, frontendCompiler.SfcCompileCount);
            Assert.AreEqual(documentPath, frontendCompiler.LastDocumentPath);
            Assert.AreEqual(source, frontendCompiler.LastSfcText);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task StubFrontendModuleCompiler_CompileSfcAsync_WhenOnlyStyleChanges_PreservesJavaScriptAndUpdatesStyleContent()
    {
        var compiler = new StubFrontendModuleCompiler();
        var documentPath = Path.Combine(Path.GetTempPath(), "Counter.vue");
        var originalText =
            """
            <template><div>Counter</div></template>
            <style>
            .counter { color: red; }
            </style>
            """;
        var updatedText =
            """
            <template><div>Counter</div></template>
            <style>
            .counter { color: blue; }
            </style>
            """;

        var originalResult = await compiler.CompileSfcAsync(documentPath, originalText, CancellationToken.None);
        var updatedResult = await compiler.CompileSfcAsync(documentPath, updatedText, CancellationToken.None);

        Assert.IsNotNull(originalResult);
        Assert.IsNotNull(updatedResult);
        Assert.AreEqual(originalResult.JavaScript, updatedResult.JavaScript);
        Assert.AreEqual(".counter { color: red; }", originalResult.StyleContent);
        Assert.AreEqual(".counter { color: blue; }", updatedResult.StyleContent);
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_TypeScriptFile_UsesFrontendTypeScriptCompiler()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "counter.ts");
            var source = "export const count: number = 1;";
            await File.WriteAllTextAsync(documentPath, source);

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                TypeScriptResult = new FrontendModuleCompilation
                {
                    JavaScript = "export const count = 1;",
                    Dependencies = ["vue"]
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache());

            var result = await compiler.CompileAsync(documentPath, CancellationToken.None);

            Assert.IsFalse(result.IsError);
            Assert.AreEqual("text/javascript", result.ContentType);
            Assert.AreEqual("export const count = 1;", result.Content);
            Assert.AreEqual(1, frontendCompiler.TypeScriptCompileCount);
            Assert.AreEqual(documentPath, frontendCompiler.LastTypeScriptPath);
            Assert.AreEqual(source, frontendCompiler.LastTypeScriptText);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_VueFile_WithStyleContent_OffsetsInlineSourceMapGeneratedLines()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.vue");
            var source = """
                <template><div>Counter</div></template>
                <style>.counter { color: red; }</style>
                """;
            await File.WriteAllTextAsync(documentPath, source);

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { name: 'Counter' };",
                    SourceMap = """
                        {"version":3,"sources":["Counter.vue"],"sourcesContent":["<template><div>Counter</div></template>\n<style>.counter { color: red; }</style>"],"names":[],"mappings":"AAAA","file":"Counter.vue"}
                        """,
                    StyleContent = ".counter { color: red; }",
                    Dependencies = []
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                moduleResolver: new ModuleResolver(rootDirectory));

            var result = await compiler.CompileAsync(documentPath, CancellationToken.None);

            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.SourceMap);
            StringAssert.Contains(result.Content, "const __jazorStyleId = \"/Counter.vue\";");
            StringAssert.Contains(result.Content, "sourceMappingURL=data:application/json;base64,");

            var sourceMap = DecodeInlineSourceMap(result.Content);
            using var resultSourceMap = JsonDocument.Parse(result.SourceMap!);
            var mappedLine = GetLineNumberContaining(result.Content, "export default { name: 'Counter' };");
            Assert.AreEqual(
                new string(';', mappedLine - 1) + "AAAA",
                sourceMap.RootElement.GetProperty("mappings").GetString());
            Assert.AreEqual(
                sourceMap.RootElement.GetProperty("mappings").GetString(),
                resultSourceMap.RootElement.GetProperty("mappings").GetString());
            Assert.AreEqual(source, sourceMap.RootElement.GetProperty("sourcesContent")[0].GetString());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_JazorFile_WhenSourceMapAvailable_ChainsToOriginalSource()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.jazor");
            var source = """
                <template>
                  <button>@Count</button>
                </template>

                @code {
                    [State] private int Count = 1;

                    public void Increment()
                    {
                        Count++;
                    }
                }
                """;
            await File.WriteAllTextAsync(documentPath, source);

            var parser = new Jazor.Vue.JazorVueParser();
            var vueCompiler = new Jazor.Vue.JazorVueCompiler();
            var generatedSfc = vueCompiler.Compile(parser.Parse(documentPath, source));
            var frontendSourceMap = CreateSingleSourceLineMap(
                "Counter.jazor",
                generatedSfc.GeneratedVueText,
                [
                    GetLineIndexContaining(generatedSfc.GeneratedVueText, "const count = ref(1);"),
                    GetLineIndexContaining(generatedSfc.GeneratedVueText, "count.value++;")
                ]);
            var sourceMapService = new InMemorySourceMapService();
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "const count = ref(1);\ncount.value++;",
                    SourceMap = frontendSourceMap,
                    Dependencies = []
                }
            };
            var compiler = new OnDemandCompiler(
                parser,
                vueCompiler,
                frontendCompiler,
                new CompilationCache(),
                moduleResolver: new ModuleResolver(rootDirectory),
                sourceMapService: sourceMapService);

            var result = await compiler.CompileAsync(documentPath, CancellationToken.None);

            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.SourceMap);
            StringAssert.Contains(result.Content, "sourceMappingURL=data:application/json;base64,");

            var sourceMap = DecodeInlineSourceMap(result.Content);
            using var resultSourceMap = JsonDocument.Parse(result.SourceMap!);
            Assert.AreEqual("Counter.jazor", sourceMap.RootElement.GetProperty("sources")[0].GetString());
            Assert.AreEqual(source, sourceMap.RootElement.GetProperty("sourcesContent")[0].GetString());
            Assert.AreEqual(
                sourceMap.RootElement.GetProperty("sourcesContent")[0].GetString(),
                resultSourceMap.RootElement.GetProperty("sourcesContent")[0].GetString());
            var mappedLines = DecodeGeneratedLineToSourceLine(sourceMap.RootElement);
            Assert.AreEqual(GetLineIndexContaining(source, "[State] private int Count = 1;"), mappedLines[0]);
            Assert.AreEqual(GetLineIndexContaining(source, "Count++;"), mappedLines[1]);

            var originalPosition = sourceMapService.OriginalPositionFor("/Counter.jazor", 1, 0);
            Assert.IsNotNull(originalPosition);
            Assert.AreEqual(GetLineIndexContaining(source, "Count++;"), originalPosition.Line);

            var generatedPosition = sourceMapService.GeneratedPositionFor(documentPath, GetLineIndexContaining(source, "Count++;"), 0);
            Assert.IsNotNull(generatedPosition);
            Assert.AreEqual("/Counter.jazor", generatedPosition.GeneratedPath);
            Assert.AreEqual(1, generatedPosition.Line);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_TypeScriptFile_RegistersSourceMapServiceByResolvedUrl()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "counter.ts");
            const string source = "export const count: number = 1;";
            await File.WriteAllTextAsync(documentPath, source);

            var sourceMapService = new InMemorySourceMapService();
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                TypeScriptResult = new FrontendModuleCompilation
                {
                    JavaScript = "export const count = 1;",
                    SourceMap = """
                        {"version":3,"sources":["counter.ts"],"sourcesContent":["export const count: number = 1;"],"names":[],"mappings":"AAAA","file":"counter.js"}
                        """,
                    Dependencies = []
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                moduleResolver: new ModuleResolver(rootDirectory),
                sourceMapService: sourceMapService);

            var result = await compiler.CompileAsync(documentPath, CancellationToken.None);

            Assert.IsFalse(result.IsError);
            Assert.AreEqual(result.SourceMap, sourceMapService.GetSourceMapJson("/counter.ts"));

            var original = sourceMapService.OriginalPositionFor("/counter.ts", 0, 0);
            Assert.IsNotNull(original);
            Assert.AreEqual("counter.ts", original.SourcePath);
            Assert.AreEqual(0, original.Line);

            var generated = sourceMapService.GeneratedPositionFor(documentPath, 0, 0);
            Assert.IsNotNull(generated);
            Assert.AreEqual("/counter.ts", generated.GeneratedPath);
            Assert.AreEqual(0, generated.Line);

            compiler.Invalidate(documentPath);

            Assert.IsNull(sourceMapService.GetSourceMapJson("/counter.ts"));
            Assert.IsNull(sourceMapService.OriginalPositionFor("/counter.ts", 0, 0));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_TypeScriptFile_AfterInvalidate_RegistersLatestSourceMap()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "counter.ts");
            const string initialSource = "export const count: number = 1;";
            const string updatedSource = """
                export const count: number = 1;
                export const label: string = "updated";
                """;
            await File.WriteAllTextAsync(documentPath, initialSource);

            var sourceMapService = new InMemorySourceMapService();
            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetTypeScriptResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export const count = 1;",
                    SourceMap = """
                        {"version":3,"sources":["counter.ts"],"sourcesContent":["export const count: number = 1;"],"names":[],"mappings":"AAAA","file":"counter.js"}
                        """,
                    Dependencies = []
                });
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                moduleResolver: new ModuleResolver(rootDirectory),
                sourceMapService: sourceMapService);

            var initialResult = await compiler.CompileAsync(documentPath, CancellationToken.None);

            Assert.IsFalse(initialResult.IsError);
            Assert.AreEqual(initialResult.SourceMap, sourceMapService.GetSourceMapJson("/counter.ts"));
            Assert.AreEqual(0, sourceMapService.OriginalPositionFor("/counter.ts", 0, 0)?.Line);

            await File.WriteAllTextAsync(documentPath, updatedSource);
            frontendCompiler.SetTypeScriptResult(
                documentPath,
                new FrontendModuleCompilation
                {
                    JavaScript = """
                        export const count = 1;
                        export const label = "updated";
                        """,
                    SourceMap = """
                        {"version":3,"sources":["counter.ts"],"sourcesContent":["export const count: number = 1;\nexport const label: string = \"updated\";"],"names":[],"mappings":";AACS","file":"counter.js"}
                        """,
                    Dependencies = []
                });

            compiler.Invalidate(documentPath);
            var updatedResult = await compiler.CompileAsync(documentPath, CancellationToken.None);

            Assert.IsFalse(updatedResult.IsError);
            Assert.AreEqual(updatedResult.SourceMap, sourceMapService.GetSourceMapJson("/counter.ts"));
            Assert.AreNotEqual(initialResult.SourceMap, updatedResult.SourceMap);

            var updatedOriginal = sourceMapService.OriginalPositionFor("/counter.ts", 1, 0);
            Assert.IsNotNull(updatedOriginal);
            Assert.AreEqual(1, updatedOriginal.Line);
            Assert.AreEqual(9, updatedOriginal.Column);

            var updatedGenerated = sourceMapService.GeneratedPositionFor(documentPath, 1, 9);
            Assert.IsNotNull(updatedGenerated);
            Assert.AreEqual("/counter.ts", updatedGenerated.GeneratedPath);
            Assert.AreEqual(1, updatedGenerated.Line);
            Assert.AreEqual(0, updatedGenerated.Column);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_VueFile_RegistersSourceMapServiceByResolvedUrl()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.vue");
            const string source = "<template><div>{{ count }}</div></template>";
            await File.WriteAllTextAsync(documentPath, source);

            var sourceMapService = new InMemorySourceMapService();
            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                SfcResult = new FrontendModuleCompilation
                {
                    JavaScript = "export default { setup() { const count = 1; return { count }; } };",
                    SourceMap = """
                        {"version":3,"sources":["Counter.vue"],"sourcesContent":["<template><div>{{ count }}</div></template>"],"names":[],"mappings":"AAAA","file":"Counter.vue.js"}
                        """,
                    Dependencies = []
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                moduleResolver: new ModuleResolver(rootDirectory),
                sourceMapService: sourceMapService);

            var result = await compiler.CompileAsync(documentPath, CancellationToken.None);

            Assert.IsFalse(result.IsError);
            Assert.AreEqual(result.SourceMap, sourceMapService.GetSourceMapJson("/Counter.vue"));

            var original = sourceMapService.OriginalPositionFor("/Counter.vue", 0, 0);
            Assert.IsNotNull(original);
            Assert.AreEqual("Counter.vue", original.SourcePath);
            Assert.AreEqual(0, original.Line);

            var generated = sourceMapService.GeneratedPositionFor(documentPath, 0, 0);
            Assert.IsNotNull(generated);
            Assert.AreEqual("/Counter.vue", generated.GeneratedPath);
            Assert.AreEqual(0, generated.Line);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_InvalidateAll_UnregistersAllRegisteredSourceMaps()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var counterPath = Path.Combine(rootDirectory, "counter.ts");
            var todoPath = Path.Combine(rootDirectory, "todo.ts");
            await File.WriteAllTextAsync(counterPath, "export const count: number = 1;");
            await File.WriteAllTextAsync(todoPath, "export const todo: string = 'ship';");

            var sourceMapService = new InMemorySourceMapService();
            var frontendCompiler = new FakeFrontendModuleCompiler();
            frontendCompiler.SetTypeScriptResult(
                counterPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export const count = 1;",
                    SourceMap = """
                        {"version":3,"sources":["counter.ts"],"sourcesContent":["export const count: number = 1;"],"names":[],"mappings":"AAAA","file":"counter.js"}
                        """,
                    Dependencies = []
                });
            frontendCompiler.SetTypeScriptResult(
                todoPath,
                new FrontendModuleCompilation
                {
                    JavaScript = "export const todo = 'ship';",
                    SourceMap = """
                        {"version":3,"sources":["todo.ts"],"sourcesContent":["export const todo: string = 'ship';"],"names":[],"mappings":"AAAA","file":"todo.js"}
                        """,
                    Dependencies = []
                });
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache(),
                moduleResolver: new ModuleResolver(rootDirectory),
                sourceMapService: sourceMapService);

            _ = await compiler.CompileAsync(counterPath, CancellationToken.None);
            _ = await compiler.CompileAsync(todoPath, CancellationToken.None);

            Assert.IsNotNull(sourceMapService.GetSourceMapJson("/counter.ts"));
            Assert.IsNotNull(sourceMapService.GetSourceMapJson("/todo.ts"));

            compiler.InvalidateAll();

            Assert.IsNull(sourceMapService.GetSourceMapJson("/counter.ts"));
            Assert.IsNull(sourceMapService.GetSourceMapJson("/todo.ts"));
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_TypeScriptFile_WhenSourceMapAvailable_AppendsInlineSourceMapUrl()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "counter.ts");
            var source = "export const count: number = 1;";
            await File.WriteAllTextAsync(documentPath, source);

            var frontendCompiler = new FakeFrontendModuleCompiler
            {
                TypeScriptResult = new FrontendModuleCompilation
                {
                    JavaScript = "export const count = 1;",
                    SourceMap = """
                        {"version":3,"sources":["counter.ts"],"sourcesContent":["export const count: number = 1;"],"names":[],"mappings":"AAAA","file":"counter.js"}
                        """,
                    Dependencies = []
                }
            };
            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler,
                new CompilationCache());

            var result = await compiler.CompileAsync(documentPath, CancellationToken.None);

            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.SourceMap);
            StringAssert.Contains(result.Content, "sourceMappingURL=data:application/json;base64,");

            var sourceMap = DecodeInlineSourceMap(result.Content);
            Assert.AreEqual(3, sourceMap.RootElement.GetProperty("version").GetInt32());
            Assert.AreEqual("counter.ts", sourceMap.RootElement.GetProperty("sources")[0].GetString());
            Assert.AreEqual(source, sourceMap.RootElement.GetProperty("sourcesContent")[0].GetString());
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task OnDemandCompiler_CompileAsync_WhenFrontendUnavailable_ReturnsErrorModule()
    {
        var rootDirectory = CreateTemporaryDirectory();
        try
        {
            var documentPath = Path.Combine(rootDirectory, "Counter.vue");
            await File.WriteAllTextAsync(documentPath, "<template><div>Hello</div></template>");

            var compiler = new OnDemandCompiler(
                new Jazor.Vue.JazorVueParser(),
                new Jazor.Vue.JazorVueCompiler(),
                frontendCompiler: null,
                new CompilationCache());

            var result = await compiler.CompileAsync(documentPath, CancellationToken.None);

            Assert.IsTrue(result.IsError);
            Assert.AreEqual("text/javascript", result.ContentType);
            StringAssert.Contains(result.Content, "throw new Error");
            StringAssert.Contains(result.ErrorMessage!, "frontend compiler is unavailable");
            Assert.AreEqual(1, result.Diagnostics.Count);
        }
        finally
        {
            Directory.Delete(rootDirectory, recursive: true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "JazorVueHostDevServerTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static JsonDocument DecodeInlineSourceMap(string content)
    {
        const string marker = "sourceMappingURL=data:application/json;base64,";
        var markerIndex = content.IndexOf(marker, StringComparison.Ordinal);
        Assert.IsTrue(markerIndex >= 0, "Expected an inline sourceMappingURL comment.");

        var base64 = content[(markerIndex + marker.Length)..].Trim();
        return JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(base64)));
    }

    private static int GetLineNumberContaining(string text, string value)
    {
        var index = text.IndexOf(value, StringComparison.Ordinal);
        Assert.IsTrue(index >= 0, $"Expected to find '{value}' in the module output.");

        var line = 1;
        for (var position = 0; position < index; position++)
        {
            if (text[position] == '\n')
            {
                line++;
            }
        }

        return line;
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    private static Uri ToWebSocketUri(Uri baseUri, string path)
    {
        var queryIndex = path.IndexOf('?', StringComparison.Ordinal);
        var builder = new UriBuilder(baseUri)
        {
            Scheme = string.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
                ? "wss"
                : "ws",
            Path = queryIndex >= 0 ? path[..queryIndex] : path,
            Query = queryIndex >= 0 ? path[(queryIndex + 1)..] : string.Empty
        };
        return builder.Uri;
    }

    private static async Task<JsonElement> ReceiveWebSocketJsonAsync(
        WebSocket socket,
        TimeSpan timeout)
    {
        using var timeoutSource = new CancellationTokenSource(timeout);
        var buffer = new byte[4096];
        using var messageStream = new MemoryStream();

        while (true)
        {
            var result = await socket.ReceiveAsync(buffer, timeoutSource.Token);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                throw new AssertFailedException("Expected a websocket payload before close.");
            }

            messageStream.Write(buffer, 0, result.Count);
            if (result.EndOfMessage)
            {
                break;
            }
        }

        var json = Encoding.UTF8.GetString(messageStream.ToArray());
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static async Task AssertNoWebSocketJsonAsync(
        WebSocket socket,
        TimeSpan timeout)
    {
        try
        {
            var message = await ReceiveWebSocketJsonAsync(socket, timeout);
            Assert.Fail("Expected no websocket payload, but received: " + message.GetRawText());
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static Task SendWebSocketJsonAsync(WebSocket socket, string json)
        => socket.SendAsync(
            Encoding.UTF8.GetBytes(json),
            WebSocketMessageType.Text,
            endOfMessage: true,
            CancellationToken.None);

    private static async Task<WebSocketCloseStatus?> ReceiveWebSocketCloseAsync(
        WebSocket socket,
        TimeSpan timeout)
    {
        using var timeoutSource = new CancellationTokenSource(timeout);
        var buffer = new byte[256];
        var result = await socket.ReceiveAsync(buffer, timeoutSource.Token);
        if (result.MessageType != WebSocketMessageType.Close)
        {
            throw new AssertFailedException("Expected websocket close payload.");
        }

        if (socket.State == WebSocketState.CloseReceived)
        {
            await socket.CloseOutputAsync(
                socket.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                socket.CloseStatusDescription ?? "ack",
                CancellationToken.None);
        }

        return socket.CloseStatus;
    }

    private static async Task<string> ReceiveWebSocketTextAsync(
        WebSocket socket,
        TimeSpan timeout)
    {
        using var timeoutSource = new CancellationTokenSource(timeout);
        var buffer = new byte[4096];
        using var messageStream = new MemoryStream();

        while (true)
        {
            var result = await socket.ReceiveAsync(buffer, timeoutSource.Token);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                throw new AssertFailedException("Expected a websocket text payload before close.");
            }

            messageStream.Write(buffer, 0, result.Count);
            if (result.EndOfMessage)
            {
                break;
            }
        }

        return Encoding.UTF8.GetString(messageStream.ToArray());
    }

    private static async Task TryCloseWebSocketAsync(WebSocket socket)
    {
        try
        {
            if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "done",
                    CancellationToken.None);
            }
        }
        catch (WebSocketException)
        {
        }
        catch (OperationCanceledException)
        {
        }
    }

    private sealed class FakeFrontendModuleCompiler : IFrontendModuleCompiler
    {
        private readonly Dictionary<string, FrontendModuleCompilation> _sfcResultsByPath = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, FrontendModuleCompilation> _typeScriptResultsByPath = new(StringComparer.OrdinalIgnoreCase);

        public FrontendModuleCompilation? SfcResult { get; set; }

        public FrontendModuleCompilation? TypeScriptResult { get; set; }

        public int SfcCompileCount { get; private set; }

        public int TypeScriptCompileCount { get; private set; }

        public string? LastDocumentPath { get; private set; }

        public string? LastSfcText { get; private set; }

        public string? LastTypeScriptPath { get; private set; }

        public string? LastTypeScriptText { get; private set; }

        public void SetSfcResult(string documentPath, FrontendModuleCompilation result)
            => _sfcResultsByPath[documentPath] = result;

        public void SetTypeScriptResult(string documentPath, FrontendModuleCompilation result)
            => _typeScriptResultsByPath[documentPath] = result;

        public ValueTask<FrontendModuleCompilation?> CompileSfcAsync(
            string documentPath,
            string text,
            CancellationToken cancellationToken)
        {
            SfcCompileCount++;
            LastDocumentPath = documentPath;
            LastSfcText = text;
            return ValueTask.FromResult(
                _sfcResultsByPath.TryGetValue(documentPath, out var result)
                    ? result
                    : SfcResult);
        }

        public ValueTask<FrontendModuleCompilation?> CompileTypeScriptAsync(
            string documentPath,
            string text,
            CancellationToken cancellationToken)
        {
            TypeScriptCompileCount++;
            LastTypeScriptPath = documentPath;
            LastTypeScriptText = text;
            return ValueTask.FromResult(
                _typeScriptResultsByPath.TryGetValue(documentPath, out var result)
                    ? result
                    : TypeScriptResult);
        }
    }

    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        public string? LastBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent("""{"ok":true}""")
                {
                    Headers =
                    {
                        ContentType = new("application/json")
                    }
                }
            };
        }
    }

    private sealed record UpstreamRequestSnapshot(
        string Method,
        string AbsolutePath,
        string Query,
        string Body);

    private sealed record UpstreamWebSocketRequestSnapshot(
        string Method,
        string AbsolutePath,
        string Query);

    private sealed record UpstreamWebSocketSubProtocolSnapshot(
        string Method,
        string AbsolutePath,
        string Query,
        IReadOnlyList<string> RequestedProtocols,
        string? SelectedProtocol);

    private sealed class TestUpstreamServer : IAsyncDisposable
    {
        private readonly HttpListener _listener;
        private readonly Func<HttpListenerContext, Task> _handler;
        private readonly CancellationTokenSource _shutdownSource = new();
        private readonly Task _acceptLoop;

        private TestUpstreamServer(
            HttpListener listener,
            Func<HttpListenerContext, Task> handler,
            string baseAddress)
        {
            _listener = listener;
            _handler = handler;
            BaseAddress = baseAddress;
            _acceptLoop = Task.Run(AcceptLoopAsync);
        }

        public string BaseAddress { get; }

        public static Task<TestUpstreamServer> StartAsync(Func<HttpListenerContext, Task> handler)
        {
            var (listener, baseAddress) = StartHttpListenerWithRetry();
            return Task.FromResult(new TestUpstreamServer(listener, handler, baseAddress));
        }

        public async ValueTask DisposeAsync()
        {
            _shutdownSource.Cancel();
            if (_listener.IsListening)
            {
                _listener.Stop();
            }

            try
            {
                await _acceptLoop;
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _listener.Close();
                _shutdownSource.Dispose();
            }
        }

        private async Task AcceptLoopAsync()
        {
            while (!_shutdownSource.IsCancellationRequested)
            {
                HttpListenerContext? context = null;
                try
                {
                    context = await _listener.GetContextAsync();
                    await _handler(context);
                }
                catch (HttpListenerException) when (_shutdownSource.IsCancellationRequested || !_listener.IsListening)
                {
                    break;
                }
                catch (ObjectDisposedException) when (_shutdownSource.IsCancellationRequested)
                {
                    break;
                }
                finally
                {
                    context?.Response.OutputStream.Dispose();
                }
            }
        }
    }

    private sealed class TestHttpsUpstreamServer : IAsyncDisposable
    {
        private readonly WebApplication _application;
        private readonly X509Certificate2 _certificate;

        private TestHttpsUpstreamServer(
            WebApplication application,
            X509Certificate2 certificate,
            string baseAddress)
        {
            _application = application;
            _certificate = certificate;
            BaseAddress = baseAddress;
        }

        public string BaseAddress { get; }

        public static async Task<TestHttpsUpstreamServer> StartAsync(Func<HttpContext, Task> handler)
        {
            var certificate = CreateSelfSignedCertificate();
            var builder = WebApplication.CreateSlimBuilder();
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(IPAddress.Loopback, 0, listenOptions => listenOptions.UseHttps(certificate));
            });

            var application = builder.Build();
            application.Map("/{**path}", handler);
            await application.StartAsync();
            var baseAddress = ResolveApplicationBaseAddress(application, Uri.UriSchemeHttps);

            return new TestHttpsUpstreamServer(
                application,
                certificate,
                baseAddress);
        }

        public async ValueTask DisposeAsync()
        {
            await _application.StopAsync();
            await _application.DisposeAsync();
            _certificate.Dispose();
        }
    }

    private sealed class TestWebSocketUpstreamServer : IAsyncDisposable
    {
        private readonly HttpListener _listener;
        private readonly Func<HttpListenerContext, WebSocket, Task> _handler;
        private readonly CancellationTokenSource _shutdownSource = new();
        private readonly Task _acceptLoop;

        private TestWebSocketUpstreamServer(
            HttpListener listener,
            Func<HttpListenerContext, WebSocket, Task> handler,
            string baseAddress)
        {
            _listener = listener;
            _handler = handler;
            BaseAddress = baseAddress;
            _acceptLoop = Task.Run(AcceptLoopAsync);
        }

        public string BaseAddress { get; }

        public static Task<TestWebSocketUpstreamServer> StartAsync(Func<HttpListenerContext, WebSocket, Task> handler)
        {
            var (listener, baseAddress) = StartHttpListenerWithRetry();
            return Task.FromResult(new TestWebSocketUpstreamServer(listener, handler, baseAddress));
        }

        public async ValueTask DisposeAsync()
        {
            _shutdownSource.Cancel();
            if (_listener.IsListening)
            {
                _listener.Stop();
            }

            try
            {
                await _acceptLoop;
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _listener.Close();
                _shutdownSource.Dispose();
            }
        }

        private async Task AcceptLoopAsync()
        {
            while (!_shutdownSource.IsCancellationRequested)
            {
                HttpListenerContext? context = null;
                WebSocket? socket = null;
                try
                {
                    context = await _listener.GetContextAsync();
                    if (!context.Request.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.Close();
                        continue;
                    }

                    var webSocketContext = await context.AcceptWebSocketAsync(null);
                    socket = webSocketContext.WebSocket;
                    await _handler(context, socket);
                }
                catch (HttpListenerException) when (_shutdownSource.IsCancellationRequested || !_listener.IsListening)
                {
                    break;
                }
                catch (ObjectDisposedException) when (_shutdownSource.IsCancellationRequested)
                {
                    break;
                }
                finally
                {
                    if (socket is not null)
                    {
                        socket.Dispose();
                    }
                    else
                    {
                        context?.Response.OutputStream.Dispose();
                    }
                }
            }
        }

    }

    private sealed class TestSecureWebSocketUpstreamServer : IAsyncDisposable
    {
        private readonly WebApplication _application;
        private readonly X509Certificate2 _certificate;

        private TestSecureWebSocketUpstreamServer(
            WebApplication application,
            X509Certificate2 certificate,
            string baseAddress)
        {
            _application = application;
            _certificate = certificate;
            BaseAddress = baseAddress;
        }

        public string BaseAddress { get; }

        public static async Task<TestSecureWebSocketUpstreamServer> StartAsync(
            Func<HttpContext, WebSocket, Task> handler,
            Func<HttpContext, string?>? subProtocolSelector = null)
        {
            var certificate = CreateSelfSignedCertificate();
            var builder = WebApplication.CreateSlimBuilder();
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(IPAddress.Loopback, 0, listenOptions => listenOptions.UseHttps(certificate));
            });

            var application = builder.Build();
            application.UseWebSockets();
            application.Map(
                "/{**path}",
                async context =>
                {
                    if (!context.WebSockets.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        return;
                    }

                    var selectedProtocol = subProtocolSelector?.Invoke(context);
                    using var socket = await context.WebSockets.AcceptWebSocketAsync(selectedProtocol);
                    await handler(context, socket);
                });
            await application.StartAsync();
            var baseAddress = ResolveApplicationBaseAddress(application, Uri.UriSchemeHttps);

            return new TestSecureWebSocketUpstreamServer(
                application,
                certificate,
                baseAddress);
        }

        public async ValueTask DisposeAsync()
        {
            await _application.StopAsync();
            await _application.DisposeAsync();
            _certificate.Dispose();
        }
    }

    private static X509Certificate2 CreateSelfSignedCertificate()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=127.0.0.1",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                critical: false));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

        var subjectAlternativeNames = new SubjectAlternativeNameBuilder();
        subjectAlternativeNames.AddIpAddress(IPAddress.Loopback);
        subjectAlternativeNames.AddIpAddress(IPAddress.Parse("127.0.0.1"));
        subjectAlternativeNames.AddDnsName("localhost");
        request.CertificateExtensions.Add(subjectAlternativeNames.Build());

        using var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(7));
        return X509CertificateLoader.LoadPkcs12(certificate.Export(X509ContentType.Pfx), password: null);
    }

    private static (HttpListener Listener, string BaseAddress) StartHttpListenerWithRetry()
    {
        const int maxAttempts = 10;
        HttpListenerException? lastException = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var port = GetFreePort();
            var baseAddress = $"http://127.0.0.1:{port}/";
            var listener = new HttpListener();
            listener.Prefixes.Add(baseAddress);

            try
            {
                listener.Start();
                return (listener, baseAddress);
            }
            catch (HttpListenerException ex) when (attempt < maxAttempts)
            {
                lastException = ex;
                listener.Close();
                System.Threading.Thread.Sleep(20 * attempt);
            }
        }

        throw new InvalidOperationException("Failed to start HTTP listener for upstream test server.", lastException);
    }

    private static string ResolveApplicationBaseAddress(WebApplication application, string scheme)
    {
        foreach (var address in application.Urls)
        {
            if (!Uri.TryCreate(address, UriKind.Absolute, out var uri)
                || !string.Equals(uri.Scheme, scheme, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return uri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal)
                ? uri.AbsoluteUri
                : uri.AbsoluteUri + "/";
        }

        throw new InvalidOperationException($"Failed to resolve '{scheme}' listening address.");
    }

    private static int GetFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
