using System.Text.Json;
using Jazor.AspNetCore;
using Microsoft.AspNetCore.Builder;

namespace Jazor.EmitTest;

public sealed partial class JazorSsrHostingTests
{
    [TestMethod]
    [TestCategory("Browser")]
    [DataRow("invalid-state")]
    [DataRow("concurrent")]
    [DataRow("import-error")]
    [DataRow("mount-error")]
    public async Task UseJazorSsr_BootstrapPreservesFailureAndSingleMountInRealBrowser(string scenario)
    {
        var browserPath = BrowserSmokeTestHelper.ResolveBrowserExecutable();
        if (browserPath is null)
            Assert.Inconclusive("A Chromium browser is required to verify hydration execution.");

        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        await File.WriteAllTextAsync(Path.Combine(artifactRoot, "components", "bootstrap-probe.mjs"), """
            import { h } from "vue";
            if (typeof window !== "undefined") {
              window.probeImports++;
              window.importStarted();
              await window.importGate;
              if (window.scenario === "import-error") throw new Error("probe import failed");
            }
            export default {
              props: ["Title"],
              setup(props) {
                if (typeof window !== "undefined") {
                  window.probeSetups++;
                  if (window.scenario === "mount-error") throw new Error("probe setup failed");
                }
                return () => h("main", { id: "probe-output" }, props.Title);
              }
            };
            """);
        await using var host = await CreateNetworkHostAsync(workspace.RootPath, artifactRoot, app =>
        {
            app.UsePathBase("/docs");
            app.UseJazorArtifacts();
            app.UseJazorSsr(new JazorSsrRequest("components/bootstrap-probe.mjs", new { Title = "snapshot" }));
        });
        var address = new Uri(host.Urls.Single());
        using var client = new HttpClient { BaseAddress = address };
        using var request = new HttpRequestMessage(HttpMethod.Get, "/docs/features/probe");
        request.Headers.Accept.ParseAdd("text/html");
        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        // Execute the actual host-generated bootstrap as separate ESM entries. The browser-only
        // import gate exposes overlap deterministically without changing production bootstrap code.
        html = html.Replace("<script type=\"module\">", "<script id=\"bootstrap\" type=\"text/plain\">", StringComparison.Ordinal);
        var scriptStart = html.IndexOf("<script id=\"bootstrap\" type=\"text/plain\">", StringComparison.Ordinal) + "<script id=\"bootstrap\" type=\"text/plain\">".Length;
        var scriptEnd = html.IndexOf("</script>", scriptStart, StringComparison.Ordinal);
        await File.WriteAllTextAsync(Path.Combine(artifactRoot, "bootstrap-entry.mjs"), html[scriptStart..scriptEnd]);
        html = html.Replace("</body>", "<script type=\"module\">\nwindow.scenario = " + JsonSerializer.Serialize(scenario) + ";\n" + BootstrapProbe + "\n</script></body>", StringComparison.Ordinal);
        await File.WriteAllTextAsync(Path.Combine(artifactRoot, "bootstrap-test.html"), html);
        var browser = await BrowserSmokeTestHelper.RunBrowserDumpDomAsync(browserPath,
            new Uri(address, "/docs/jazor/bootstrap-test.html"), workspace.RootPath, 8000);
        Assert.AreEqual(0, browser.ExitCode, browser.ToString());
        using var payload = BrowserSmokeTestHelper.ReadBrowserSmokePayload(browser, "SSR bootstrap " + scenario);
        Assert.IsTrue(payload.RootElement.GetProperty("passed").GetBoolean(), payload.RootElement.ToString());
    }

    private const string BootstrapProbe = """
        const check = (condition, message) => { if (!condition) throw new Error(message); };
        const stateElement = document.getElementById("__jazor_ssr_state");
        const mount = document.getElementById("app");
        const originalState = stateElement.textContent;
        window.probeImports = 0;
        window.probeSetups = 0;
        let releaseImport;
        window.importGate = new Promise(resolve => releaseImport = resolve);
        const started = new Promise(resolve => window.importStarted = resolve);
        let entry = 0;
        const execute = () => import(`./bootstrap-entry.mjs?run=${entry++}`);
        const rejected = async (task, message) => {
          try { await task; } catch (error) {
            check(String(error).includes(message), `Expected '${message}', got ${error}`);
            return;
          }
          throw new Error(`Expected rejection: ${message}`);
        };
        let passed = false;
        let failure = "";
        try {
          if (window.scenario === "invalid-state") {
            releaseImport();
            for (const value of [null, 1, "text", [], {},
                { schema: "other", version: 1, props: {}, providers: [] },
                { schema: "jazor-ssr-state", version: 2, props: {}, providers: [] },
                { schema: "jazor-ssr-state", version: 1, providers: [] },
                { schema: "jazor-ssr-state", version: 1, props: {}, providers: null },
                ...[null, { key: " \t" }, { key: 42 }].map(provider =>
                  ({ schema: "jazor-ssr-state", version: 1, props: {}, providers: [provider] })),
                { schema: "jazor-ssr-state", version: 1, props: {}, providers: [{key:"a"}, {key:"a"}] }]) {
              stateElement.textContent = JSON.stringify(value);
              await rejected(execute(), "Jazor SSR state envelope");
            }
            stateElement.textContent = "{";
            await rejected(execute(), "SyntaxError");
            check(window.probeImports === 0 && window.probeSetups === 0, "Invalid state executed component code");
            check(mount.textContent === "snapshot", "Invalid state changed server HTML");
            stateElement.textContent = originalState;
            await execute();
            check(window.probeSetups === 1, "Corrected state did not mount");
          } else if (window.scenario === "concurrent") {
            const first = execute();
            await started;
            const second = execute();
            // Second entry must reject while the first is still awaiting its component import.
            await Promise.race([
              rejected(second, "already"),
              new Promise((_, reject) => setTimeout(() => reject(new Error("Duplicate entry waited on component import")), 1000))
            ]);
            releaseImport();
            await first;
            await rejected(execute(), "already");
            check(window.probeSetups === 1, "Root mounted more than once");
          } else {
            releaseImport();
            await rejected(execute(), window.scenario === "import-error" ? "probe import failed" : "probe setup failed");
            check(mount.dataset.jazorSsrHydrated !== "1", "Failure marked hydration as successful");
            await rejected(execute(), "already");
            check(window.probeSetups === (window.scenario === "import-error" ? 0 : 1), "Failure repeated setup");
          }
          passed = true;
        } catch (error) { failure = String(error); }
        finally {
          releaseImport();
          document.documentElement.setAttribute("data-jazor-smoke", btoa(JSON.stringify({ passed, failure, imports: window.probeImports, setups: window.probeSetups })));
        }
        """;
}
