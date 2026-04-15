using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Jazor.Emit;
using Jazor.VueHost.DevServer;

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
            Assert.AreEqual(RazorVueHmrBoundaryKind.FullReloadRequired, result.HotReloadManifestEntry.HmrBoundaryKind);
            Assert.AreEqual("/Counter.jazor", result.HotReloadManifestEntry.ComponentId);
            Assert.AreEqual("/Counter.jazor", result.HotReloadManifestEntry.RelativeModulePath);
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
    public async Task ChangeProcessor_ProcessChanges_WhenJazorTemplateChangesInsideFullReloadBoundary_ReturnsFullReload()
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

            Assert.AreEqual(ChangeUpdateKind.FullReload, result.UpdateKind);
            Assert.AreEqual("HMR boundary does not prove a hot-safe update.", result.FullReloadReason);
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
                @jsimport helper from "./helper-a"

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
                @jsimport helper from "./helper-b"

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
    public async Task ChangeProcessor_ProcessChanges_WhenCssHasDependents_ReturnsFullReload()
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

            Assert.AreEqual(ChangeUpdateKind.FullReload, result.UpdateKind);
            Assert.AreEqual("frontend-change-with-dependents", result.FullReloadReason);
            CollectionAssert.AreEquivalent(
                new[] { stylePath, mainPath },
                result.AffectedPaths.ToArray());
            Assert.AreEqual(0, result.ChangedCssUrls.Count);
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
            var port = GetFreePort();
            var baseAddress = $"http://127.0.0.1:{port}/";
            var listener = new HttpListener();
            listener.Prefixes.Add(baseAddress);
            listener.Start();
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

        private static int GetFreePort()
        {
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
    }
}
