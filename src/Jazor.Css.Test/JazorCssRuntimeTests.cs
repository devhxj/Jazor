using System.Text.Json;

namespace Jazor.Css.Tests;

[TestClass]
public sealed class JazorCssRuntimeTests
{
    [TestMethod]
    public async Task Runtime_HashV1_MatchesFixedVector()
    {
        var result = await JazorCssModuleTestHost.RunDenoAsync(
            """
            import { css } from "./runtime.mjs";
            console.log(css({ color: "red" }));
            """);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        Assert.AreEqual("jz-12qlzgy-2ugla2", result.StandardOutput.Trim());
    }

    [TestMethod]
    public async Task Runtime_NoDom_GeneratesDeterministicRulesAndExtractsCss()
    {
        var result = await JazorCssModuleTestHost.RunDenoAsync(
            """
            import { css as cssClass, keyframes, global as cssGlobal, extract } from "./runtime.mjs";

            const rule = {
              display: "block",
              color: "red",
              $additional: [
                { name: "display", value: "-webkit-box", important: false },
                { name: "display", value: "flex", important: true }
              ],
              $children: [
                { kind: "selector", prelude: "&:hover, &:focus", rule: { color: "blue" } },
                { kind: "media", prelude: "(min-width: 40rem)", rule: { display: "grid" } },
                { kind: "supports", prelude: "(display: subgrid)", rule: {
                    $children: [{ kind: "selector", prelude: "> .item", rule: { display: "subgrid" } }]
                } }
              ]
            };

            const first = cssClass(rule);
            const second = cssClass({ color: "red", display: "block", $additional: rule.$additional, $children: rule.$children });
            const animation = keyframes([
              { selector: "from", declarations: { opacity: "0" } },
              { selector: "50%, to", declarations: { opacity: "1" } }
            ]);
            cssGlobal("html, body", { margin: "0", color: "black" });
            const css = extract();
            console.log(JSON.stringify({ first, second, animation, css, repeated: extract() }));
            """);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput.Trim());
        var root = json.RootElement;
        var first = root.GetProperty("first").GetString();
        var second = root.GetProperty("second").GetString();
        var animation = root.GetProperty("animation").GetString();
        var css = root.GetProperty("css").GetString() ?? string.Empty;

        Assert.IsNotNull(first);
        Assert.AreEqual(first, second);
        StringAssert.StartsWith(first, "jz-");
        Assert.IsNotNull(animation);
        StringAssert.StartsWith(animation, "jz-k-");
        StringAssert.Contains(css, "." + first + "{color:red;display:block;display:-webkit-box;display:flex!important;}");
        StringAssert.Contains(css, "." + first + ":hover,." + first + ":focus{color:blue;}");
        StringAssert.Contains(css, "@media (min-width: 40rem){." + first + "{display:grid;}}");
        StringAssert.Contains(css, "@supports (display: subgrid){." + first + " > .item{display:subgrid;}}");
        StringAssert.Contains(css, "@keyframes " + animation + "{from{opacity:0;}50%,to{opacity:1;}}");
        StringAssert.Contains(css, "html,body{color:black;margin:0;}");
        Assert.AreEqual(css, root.GetProperty("repeated").GetString());
    }

    [TestMethod]
    public async Task Runtime_Dom_InjectsNonceAndAdoptsLengthFramedEntriesAcrossReload()
    {
        var result = await JazorCssModuleTestHost.RunDenoAsync(
            """
            class FakeElement {
              constructor(localName, owner) {
                this.localName = localName;
                this.owner = owner;
                this.id = "";
                this.nonce = "";
                this.textContent = "";
                this.children = [];
              }
              appendChild(node) {
                this.children.push(node);
                if (typeof node.data === "string") this.textContent += node.data;
                if (node.id) this.owner.byId.set(node.id, node);
                return node;
              }
            }

            const documentHost = {
              byId: new Map(),
              getElementById(id) { return this.byId.get(id) ?? null; },
              createElement(name) { return new FakeElement(name, this); },
              createTextNode(data) { return { data }; }
            };
            documentHost.head = new FakeElement("head", documentHost);
            globalThis.document = documentHost;

            const firstModule = await import("./runtime.mjs");
            firstModule.configure({ nonce: "nonce-1" });
            const firstName = firstModule.css({ color: "red", content: "'汉字'" });
            const style = documentHost.getElementById("jazor-css");
            const beforeReload = style.textContent;

            const secondModule = await import("./runtime.mjs?reload=1");
            secondModule.configure({ nonce: "nonce-1" });
            const secondName = secondModule.css({ content: "'汉字'", color: "red" });

            console.log(JSON.stringify({
              firstName,
              secondName,
              nonce: style.nonce,
              styleCount: documentHost.head.children.length,
              unchanged: beforeReload === style.textContent,
              marker: style.textContent.startsWith("/*jazor-css:v1*//*jz:v1:"),
              extracted: secondModule.extract(),
              text: style.textContent
            }));
            """);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput.Trim());
        var root = json.RootElement;
        Assert.AreEqual(root.GetProperty("firstName").GetString(), root.GetProperty("secondName").GetString());
        Assert.AreEqual("nonce-1", root.GetProperty("nonce").GetString());
        Assert.AreEqual(1, root.GetProperty("styleCount").GetInt32());
        Assert.IsTrue(root.GetProperty("unchanged").GetBoolean());
        Assert.IsTrue(root.GetProperty("marker").GetBoolean());
        StringAssert.Contains(root.GetProperty("extracted").GetString() ?? string.Empty, "content:'汉字';");
        StringAssert.Contains(root.GetProperty("text").GetString() ?? string.Empty, "/*jz:v1:");
    }

    [TestMethod]
    public async Task Runtime_InvalidInputs_FailWithActionableErrors()
    {
        var result = await JazorCssModuleTestHost.RunDenoAsync(
            """
            import { css, keyframes, configure } from "./runtime.mjs";

            const messages = [];
            const capture = action => {
              try { action(); messages.push("missing-error"); }
              catch (error) { messages.push(error.message); }
            };

            capture(() => configure({ styleId: "  " }));
            capture(() => keyframes([]));
            capture(() => keyframes([{ selector: "101%", declarations: { opacity: "1" } }]));
            capture(() => css({ $additional: [{ name: "bad:name", value: "x", important: false }] }));
            capture(() => css({ $children: [{ kind: "selector", prelude: "@layer x", rule: { color: "red" } }] }));
            capture(() => css({ $children: [{ kind: "selector", prelude: ":is(&:hover", rule: { color: "red" } }] }));
            capture(() => css({ $children: [{ kind: "media", prelude: "screen; @import 'x'", rule: { color: "red" } }] }));

            css({ color: "red" });
            capture(() => configure({ nonce: "late" }));
            console.log(JSON.stringify(messages));
            """);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        var messages = JsonSerializer.Deserialize<string[]>(result.StandardOutput.Trim());
        Assert.IsNotNull(messages);
        Assert.HasCount(8, messages);
        Assert.IsFalse(messages.Contains("missing-error", StringComparer.Ordinal));
        StringAssert.Contains(messages[0], "StyleId");
        StringAssert.Contains(messages[1], "at least one frame");
        StringAssert.Contains(messages[2], "percentage");
        StringAssert.Contains(messages[3], "declaration name");
        StringAssert.Contains(messages[4], "at-rule");
        StringAssert.Contains(messages[5], "unclosed");
        StringAssert.Contains(messages[6], "structural delimiter");
        StringAssert.Contains(messages[7], "before the first style registration");
    }

    [TestMethod]
    public async Task Runtime_SelectorScanner_PreservesNestedCommasAndAttributeValues()
    {
        var result = await JazorCssModuleTestHost.RunDenoAsync(
            """
            import { css, extract } from "./runtime.mjs";
            const name = css({
              $children: [{
                kind: "selector",
                prelude: ":is(&:hover, &:focus), [data-value=\"a,b\"] &",
                rule: { color: "red" }
              }]
            });
            console.log(JSON.stringify({ name, text: extract() }));
            """);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput.Trim());
        var name = json.RootElement.GetProperty("name").GetString();
        var text = json.RootElement.GetProperty("text").GetString() ?? string.Empty;
        Assert.IsNotNull(name);
        StringAssert.Contains(text, ":is(." + name + ":hover, ." + name + ":focus),[data-value=\"a,b\"] ." + name + "{color:red;}");
    }

    [TestMethod]
    public async Task Runtime_NormalizationPreservesObservableOrderAndSeparatesNameDomains()
    {
        var result = await JazorCssModuleTestHost.RunDenoAsync(
            """
            import { css, keyframes, global, extract } from "./runtime.mjs";

            const additional = [
              { name: "display", value: "-webkit-box", important: false },
              { name: "display", value: "flex", important: false }
            ];
            const first = css({ color: null, margin: "0", display: "", $additional: additional });
            const reordered = css({ display: "", margin: "0", color: null, $additional: additional });
            const reversedFallback = css({ display: "", margin: "0", $additional: [...additional].reverse() });
            const different = css({ margin: "1px" });
            const animation = keyframes([{ selector: "from", declarations: { margin: "0" } }]);
            global("body", { margin: "0" });
            const beforeDuplicate = extract();
            global("body", { margin: "0" });

            console.log(JSON.stringify({
              first,
              reordered,
              reversedFallback,
              different,
              animation,
              css: extract(),
              duplicateStable: beforeDuplicate === extract()
            }));
            """);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput.Trim());
        var root = json.RootElement;
        var first = root.GetProperty("first").GetString();
        Assert.AreEqual(first, root.GetProperty("reordered").GetString());
        Assert.AreNotEqual(first, root.GetProperty("reversedFallback").GetString());
        Assert.AreNotEqual(first, root.GetProperty("different").GetString());
        StringAssert.StartsWith(first, "jz-");
        StringAssert.StartsWith(root.GetProperty("animation").GetString(), "jz-k-");
        Assert.IsTrue(root.GetProperty("duplicateStable").GetBoolean());

        var css = root.GetProperty("css").GetString() ?? string.Empty;
        StringAssert.Contains(css, "." + first + "{display:;margin:0;display:-webkit-box;display:flex;}");
        Assert.IsFalse(css.Contains("color:", StringComparison.Ordinal), css);
    }

    [TestMethod]
    public async Task Runtime_NestedConditionsPreserveSelectorContextAndSiblingOrder()
    {
        var result = await JazorCssModuleTestHost.RunDenoAsync(
            """
            import { css, extract } from "./runtime.mjs";
            const name = css({
              $children: [
                { kind: "media", prelude: "(width >= 40rem)", rule: {
                    $children: [
                      { kind: "selector", prelude: "&:hover", rule: { color: "red" } },
                      { kind: "supports", prelude: "(display: grid)", rule: { display: "grid" } }
                    ]
                } },
                { kind: "selector", prelude: "> span", rule: { color: "blue" } }
              ]
            });
            console.log(JSON.stringify({ name, css: extract() }));
            """);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput.Trim());
        var name = json.RootElement.GetProperty("name").GetString();
        var css = json.RootElement.GetProperty("css").GetString();
        Assert.AreEqual(
            "@media (width >= 40rem){." + name + ":hover{color:red;}@supports (display: grid){." + name + "{display:grid;}}}." + name + " > span{color:blue;}",
            css);
    }

    [TestMethod]
    public async Task Runtime_DomAppearingAfterRegistrationAttachesCacheAndRejectsForeignOwnership()
    {
        var result = await JazorCssModuleTestHost.RunDenoAsync(
            """
            const memoryModule = await import("./runtime.mjs");
            const name = memoryModule.css({ color: "red" });

            class Element {
              constructor(localName, owner) {
                this.localName = localName;
                this.owner = owner;
                this.id = "";
                this.nonce = "";
                this.textContent = "";
                this.children = [];
              }
              appendChild(node) {
                this.children.push(node);
                if (typeof node.data === "string") this.textContent += node.data;
                if (node.id) this.owner.byId.set(node.id, node);
                return node;
              }
            }
            const createDocument = () => {
              const document = {
                byId: new Map(),
                getElementById(id) { return this.byId.get(id) ?? null; },
                createElement(localName) { return new Element(localName, this); },
                createTextNode(data) { return { data }; }
              };
              document.head = new Element("head", document);
              return document;
            };

            globalThis.document = createDocument();
            const cachedName = memoryModule.css({ color: "red" });
            const attached = document.getElementById("jazor-css");

            const messages = [];
            const foreignElementModule = await import("./runtime.mjs?foreign-element");
            const foreignStyleModule = await import("./runtime.mjs?foreign-style");
            const nonceMismatchModule = await import("./runtime.mjs?nonce-mismatch");
            for (const [query, module, existing] of [
              ["foreign-element", foreignElementModule, { localName: "div", id: "jazor-css" }],
              ["foreign-style", foreignStyleModule, { localName: "style", id: "jazor-css", nonce: "", textContent: "body{}" }],
              ["nonce-mismatch", nonceMismatchModule, { localName: "style", id: "jazor-css", nonce: "old", textContent: "/*jazor-css:v1*/" }]
            ]) {
              globalThis.document = createDocument();
              document.byId.set("jazor-css", existing);
              try {
                if (query === "nonce-mismatch") module.configure({ nonce: "new" });
                module.css({ color: "blue" });
                messages.push("missing-error");
              } catch (error) {
                messages.push(error.message);
              }
            }

            console.log(JSON.stringify({
              name,
              cachedName,
              attachedText: attached.textContent,
              messages
            }));
            """);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput.Trim());
        var root = json.RootElement;
        Assert.AreEqual(root.GetProperty("name").GetString(), root.GetProperty("cachedName").GetString());
        StringAssert.Contains(root.GetProperty("attachedText").GetString() ?? string.Empty, "/*jazor-css:v1*//*jz:v1:");
        var messages = root.GetProperty("messages").EnumerateArray().Select(static item => item.GetString() ?? string.Empty).ToArray();
        Assert.HasCount(3, messages);
        StringAssert.Contains(messages[0], "non-style element");
        StringAssert.Contains(messages[1], "not owned");
        StringAssert.Contains(messages[2], "nonce does not match");
    }
}
