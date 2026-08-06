using System.Text;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgLibraryStylesRuntimeTests
{
    [TestMethod]
    public async Task LibraryStylesRuntime_DeduplicatesExistingAndRepeatedStylesheets()
    {
        var runtimeAssembly = typeof(Jazor.RazorVue.RazorSdk.GeneratedCSharpBinder).Assembly;
        await using var stream = runtimeAssembly.GetManifestResourceStream(
            "Jazor.RazorVue.Runtime.library-styles.mjs");
        Assert.IsNotNull(stream);
        using var reader = new StreamReader(stream!, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var runtimeSource = await reader.ReadToEndAsync();

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/library-styles-host.mjs",
            "export default {};",
            "library-styles-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { ensureLibraryStyles as ensureFirst } from "./node_modules/@jazor/vue-runtime/library-styles.mjs?first";
            import { ensureLibraryStyles as ensureSecond } from "./node_modules/@jazor/vue-runtime/library-styles.mjs?second";

            test("stylesheet links are loaded once", () => {
                ensureFirst(["https://cdn.example.test/server.css"]);

                const links = [createLink("https://cdn.example.test/server.css")];
                globalThis.document = {
                    baseURI: "https://app.example.test/",
                    querySelectorAll: () => links,
                    createElement: () => createLink(),
                    head: { appendChild: link => links.push(link) }
                };

                ensureFirst(["https://cdn.example.test/server.css", "https://cdn.example.test/client.css"]);
                ensureSecond(["https://cdn.example.test/client.css", " "]);

                assert.deepEqual(links.map(link => link.getAttribute("href")), [
                    "https://cdn.example.test/server.css",
                    "https://cdn.example.test/client.css"
                ]);
                delete globalThis.document;
            });

            function createLink(initialHref) {
                const attributes = new Map();
                if (initialHref) {
                    attributes.set("rel", "stylesheet");
                    attributes.set("href", initialHref);
                }

                return {
                    get href() { return attributes.get("href"); },
                    getAttribute: name => attributes.get(name) ?? null,
                    setAttribute: (name, value) => attributes.set(name, value)
                };
            }
            """,
            new Dictionary<string, string>
            {
                ["node_modules/@jazor/vue-runtime/package.json"] = """{"type":"module"}""",
                ["node_modules/@jazor/vue-runtime/library-styles.mjs"] = runtimeSource
            });
    }
}
