using System.Text.Json;

namespace ECMAScript.Style.Tests;

[TestClass]
public sealed class EcmaScriptStyleRuntimeTests
{
    [TestMethod]
    public void RuntimeModule_UsesEcmaScriptStyleProtocol()
    {
        var module = EcmaScriptStyleModuleTestHost.GetRuntimeModule();
        StringAssert.Contains(module.Content, "ecmascript-style:v1");
        StringAssert.Contains(module.Content, ".__ecmascript_style_root__");
        StringAssert.Contains(module.Content, "ecs-");
        StringAssert.Contains(module.Content, "/*ecs:v1:");
    }

    [TestMethod]
    public async Task Runtime_TypedFactories_EmitPlainCssStringsAndComposeValues()
    {
        var result = await EcmaScriptStyleModuleTestHost.RunDenoAsync(
            """
            import {
              style, px, rem, percent, rgba, hex, variable, varOr, ms,
              fr, minMax, repeat, translateY, rotate, deg, transform, extract
            } from "./runtime.mjs";

            const name = style({
              width: `calc(${percent(100)} - ${rem(2)})`,
              gap: variable("--space"),
              color: varOr("--button-color", rgba(23, 105, 170, 0.8)),
              "border-color": hex("fff8"),
              "transition-duration": ms(180),
              "grid-template-columns": repeat(3, minMax(px(0), fr(1))),
              transform: transform([translateY(px(2)), rotate(deg(4))])
            });
            console.log(JSON.stringify({ name, css: extract() }));
            """);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput.Trim());
        var name = json.RootElement.GetProperty("name").GetString();
        var css = json.RootElement.GetProperty("css").GetString() ?? string.Empty;
        Assert.IsNotNull(name);
        StringAssert.Contains(css, "." + name + "{");
        StringAssert.Contains(css, "width:calc(100% - 2rem);");
        StringAssert.Contains(css, "gap:var(--space);");
        StringAssert.Contains(css, "color:var(--button-color,rgb(23 105 170 / 0.8));");
        StringAssert.Contains(css, "border-color:#fff8;");
        StringAssert.Contains(css, "transition-duration:180ms;");
        StringAssert.Contains(css, "grid-template-columns:repeat(3,minmax(0px,1fr));");
        StringAssert.Contains(css, "transform:translateY(2px) rotate(4deg);");
    }

    [TestMethod]
    public async Task Runtime_Shadows_SerializesStructuredTokensInStableOrder()
    {
        var result = await EcmaScriptStyleModuleTestHost.RunDenoAsync(
            """
            import { style, shadows, px, hex, variable, extract } from "./runtime.mjs";

            const name = style({
              "box-shadow": shadows([
                { offsetX: px(10), offsetY: px(0), color: "currentColor" },
                { offsetX: px(0), offsetY: px(10), blur: px(2), spread: px(-1), color: variable("--surface") },
                { inset: true, offsetX: px(0), offsetY: px(0), blur: px(0), spread: px(4), color: hex("dce8ff") }
              ])
            });
            console.log(JSON.stringify({ name, css: extract() }));
            """);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput.Trim());
        var name = json.RootElement.GetProperty("name").GetString();
        var css = json.RootElement.GetProperty("css").GetString() ?? string.Empty;
        Assert.IsNotNull(name);
        StringAssert.Contains(
            css,
            "." + name + "{box-shadow:10px 0px currentColor,0px 10px 2px -1px var(--surface),inset 0px 0px 0px 4px #dce8ff;}");
    }

    [TestMethod]
    public async Task Runtime_TypedFactories_RejectInvalidValuesWithActionableErrors()
    {
        var result = await EcmaScriptStyleModuleTestHost.RunDenoAsync(
            """
            import {
              px, hex, rgb, rgba, hsl, ratio, repeat, fr, transform, shadows,
              keyword, ident, variable
            } from "./runtime.mjs";

            const messages = [];
            const capture = action => {
              try { action(); messages.push("missing-error"); }
              catch (error) { messages.push(error.message); }
            };

            capture(() => px(Infinity));
            capture(() => hex("12xz"));
            capture(() => rgb(256, 0, 0));
            capture(() => rgba(0, 0, 0, 2));
            capture(() => hsl(0, -1, 50));
            capture(() => ratio(1, 0));
            capture(() => repeat(0, fr(1)));
            capture(() => transform([]));
            capture(() => shadows([]));
            capture(() => keyword("1invalid"));
            capture(() => ident("-1invalid"));
            capture(() => variable("brand"));
            console.log(JSON.stringify(messages));
            """);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        var messages = JsonSerializer.Deserialize<string[]>(result.StandardOutput.Trim());
        Assert.IsNotNull(messages);
        Assert.HasCount(12, messages);
        Assert.IsFalse(messages.Contains("missing-error", StringComparer.Ordinal));
        StringAssert.Contains(messages[0], "finite");
        StringAssert.Contains(messages[1], "hexadecimal");
        StringAssert.Contains(messages[2], "between 0 and 255");
        StringAssert.Contains(messages[3], "between 0 and 1");
        StringAssert.Contains(messages[4], "between 0 and 100");
        StringAssert.Contains(messages[5], "greater than zero");
        StringAssert.Contains(messages[6], "greater than zero");
        StringAssert.Contains(messages[7], "at least one function");
        StringAssert.Contains(messages[8], "at least one shadow");
        StringAssert.Contains(messages[9], "must start");
        StringAssert.Contains(messages[10], "must start");
        StringAssert.Contains(messages[11], "start with '--'");
    }

    [TestMethod]
    public async Task Runtime_HashV1_MatchesFixedVector()
    {
        var result = await EcmaScriptStyleModuleTestHost.RunDenoAsync(
            """
            import { style } from "./runtime.mjs";
            console.log(style({ color: "red" }));
            """);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        Assert.AreEqual("ecs-48pape-1cqt43e", result.StandardOutput.Trim());
    }

    [TestMethod]
    public async Task Runtime_NoDom_GeneratesDeterministicRulesAndExtractsCss()
    {
        var result = await EcmaScriptStyleModuleTestHost.RunDenoAsync(
            """
            import { style, keyframes, global as cssGlobal, extract } from "./runtime.mjs";

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

            const first = style(rule);
            const second = style({ color: "red", display: "block", $additional: rule.$additional, $children: rule.$children });
            const animation = keyframes([
              { selector: "from", declarations: { opacity: "0" } },
              { selector: "50%, to", declarations: { opacity: "1" } }
            ]);
            cssGlobal("html,\nbody", { margin: "0", color: "black" });
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
        StringAssert.StartsWith(first, "ecs-");
        Assert.IsNotNull(animation);
        StringAssert.StartsWith(animation, "ecs-k-");
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
        var result = await EcmaScriptStyleModuleTestHost.RunDenoAsync(
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
            const firstName = firstModule.style({ color: "red", content: "'汉字'" });
            const style = documentHost.getElementById("ecmascript-style");
            const beforeReload = style.textContent;

            const secondModule = await import("./runtime.mjs?reload=1");
            secondModule.configure({ nonce: "nonce-1" });
            const secondName = secondModule.style({ content: "'汉字'", color: "red" });

            console.log(JSON.stringify({
              firstName,
              secondName,
              nonce: style.nonce,
              styleCount: documentHost.head.children.length,
              unchanged: beforeReload === style.textContent,
              marker: style.textContent.startsWith("/*ecmascript-style:v1*//*ecs:v1:"),
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
        StringAssert.Contains(root.GetProperty("text").GetString() ?? string.Empty, "/*ecs:v1:");
    }

    [TestMethod]
    public async Task Runtime_InvalidInputs_FailWithActionableErrors()
    {
        var result = await EcmaScriptStyleModuleTestHost.RunDenoAsync(
            """
            import { style, keyframes, configure, context, atRule } from "./runtime.mjs";

            const messages = [];
            const capture = action => {
              try { action(); messages.push("missing-error"); }
              catch (error) { messages.push(error.message); }
            };

            capture(() => configure({ styleId: "  " }));
            capture(() => keyframes([]));
            capture(() => keyframes([{ selector: "101%", declarations: { opacity: "1" } }]));
            capture(() => style({ $additional: [{ name: "bad:name", value: "x", important: false }] }));
            capture(() => style({ $children: [{ kind: "selector", prelude: "@layer x", rule: { color: "red" } }] }));
            capture(() => style({ $children: [{ kind: "selector", prelude: ":is(&:hover", rule: { color: "red" } }] }));
            capture(() => style({ $children: [{ kind: "media", prelude: "screen; @import 'x'", rule: { color: "red" } }] }));
            capture(() => style({ $children: [{ kind: "container", prelude: null, rule: { color: "red" } }] }));
            capture(() => style({ $children: [{ kind: "starting-style", prelude: "invalid", rule: { color: "red" } }] }));
            capture(() => atRule({ name: "@font-face", declarations: { src: "url(font.woff2)" } }));
            capture(() => atRule({ name: "1invalid", declarations: {} }));
            capture(() => atRule({ name: "-1invalid", declarations: {} }));
            capture(() => context({ detached: true, target: {} }));

            style({ color: "red" });
            capture(() => configure({ nonce: "late" }));
            console.log(JSON.stringify(messages));
            """);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        var messages = JsonSerializer.Deserialize<string[]>(result.StandardOutput.Trim());
        Assert.IsNotNull(messages);
        Assert.HasCount(14, messages);
        Assert.IsFalse(messages.Contains("missing-error", StringComparer.Ordinal));
        StringAssert.Contains(messages[0], "StyleId");
        StringAssert.Contains(messages[1], "at least one frame");
        StringAssert.Contains(messages[2], "percentage");
        StringAssert.Contains(messages[3], "declaration name");
        StringAssert.Contains(messages[4], "at-rule");
        StringAssert.Contains(messages[5], "unclosed");
        StringAssert.Contains(messages[6], "structural delimiter");
        StringAssert.Contains(messages[7], "cannot be empty");
        StringAssert.Contains(messages[8], "does not accept a prelude");
        StringAssert.Contains(messages[9], "must not include '@'");
        StringAssert.Contains(messages[10], "Invalid at-rule name");
        StringAssert.Contains(messages[11], "Invalid at-rule name");
        StringAssert.Contains(messages[12], "detached CSS context");
        StringAssert.Contains(messages[13], "before the first style registration");
    }

    [TestMethod]
    public async Task Runtime_SelectorScanner_PreservesNestedCommasAndAttributeValues()
    {
        var result = await EcmaScriptStyleModuleTestHost.RunDenoAsync(
            """
            import { style, extract } from "./runtime.mjs";
            const name = style({
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
        var result = await EcmaScriptStyleModuleTestHost.RunDenoAsync(
            """
            import { style, keyframes, global, extract } from "./runtime.mjs";

            const additional = [
              { name: "display", value: "-webkit-box", important: false },
              { name: "display", value: "flex", important: false }
            ];
            const first = style({ color: null, margin: "0", display: "", $additional: additional });
            const reordered = style({ display: "", margin: "0", color: null, $additional: additional });
            const reversedFallback = style({ display: "", margin: "0", $additional: [...additional].reverse() });
            const different = style({ margin: "1px" });
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
        StringAssert.StartsWith(first, "ecs-");
        StringAssert.StartsWith(root.GetProperty("animation").GetString(), "ecs-k-");
        Assert.IsTrue(root.GetProperty("duplicateStable").GetBoolean());

        var css = root.GetProperty("css").GetString() ?? string.Empty;
        StringAssert.Contains(css, "." + first + "{display:;margin:0;display:-webkit-box;display:flex;}");
        Assert.IsFalse(css.Contains("color:", StringComparison.Ordinal), css);
    }

    [TestMethod]
    public async Task Runtime_NestedConditionsPreserveSelectorContextAndSiblingOrder()
    {
        var result = await EcmaScriptStyleModuleTestHost.RunDenoAsync(
            """
            import { style, extract } from "./runtime.mjs";
            const name = style({
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
        var result = await EcmaScriptStyleModuleTestHost.RunDenoAsync(
            """
            const memoryModule = await import("./runtime.mjs");
            const name = memoryModule.style({ color: "red" });

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
            const cachedName = memoryModule.style({ color: "red" });
            const attached = document.getElementById("ecmascript-style");

            const messages = [];
            const foreignElementModule = await import("./runtime.mjs?foreign-element");
            const foreignStyleModule = await import("./runtime.mjs?foreign-style");
            const nonceMismatchModule = await import("./runtime.mjs?nonce-mismatch");
            for (const [query, module, existing] of [
              ["foreign-element", foreignElementModule, { localName: "div", id: "ecmascript-style" }],
              ["foreign-style", foreignStyleModule, { localName: "style", id: "ecmascript-style", nonce: "", textContent: "body{}" }],
              ["nonce-mismatch", nonceMismatchModule, { localName: "style", id: "ecmascript-style", nonce: "old", textContent: "/*ecmascript-style:v1*/" }]
            ]) {
              globalThis.document = createDocument();
              document.byId.set("ecmascript-style", existing);
              try {
                if (query === "nonce-mismatch") module.configure({ nonce: "new" });
                module.style({ color: "blue" });
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
        StringAssert.Contains(root.GetProperty("attachedText").GetString() ?? string.Empty, "/*ecmascript-style:v1*//*ecs:v1:");
        var messages = root.GetProperty("messages").EnumerateArray().Select(static item => item.GetString() ?? string.Empty).ToArray();
        Assert.HasCount(3, messages);
        StringAssert.Contains(messages[0], "non-style element");
        StringAssert.Contains(messages[1], "not owned");
        StringAssert.Contains(messages[2], "nonce does not match");
    }

    [TestMethod]
    public async Task Runtime_IsolatedContextsProvideDeterministicNamesSnapshotsAndAtRules()
    {
        var result = await EcmaScriptStyleModuleTestHost.RunDenoAsync(
            """
            import {
              context,
              styleIn,
              keyframesIn,
              globalIn,
              atRuleIn,
              extract,
              extractFrom,
              snapshotFrom
            } from "./runtime.mjs";

            const first = context({ detached: true, styleId: "ssr-css", nonce: "nonce-ssr" });
            const second = context({ detached: true });
            const rule = { color: "red", display: "grid" };
            const firstName = styleIn(first, rule);
            const secondName = styleIn(second, rule);
            const animation = keyframesIn(first, [
              { selector: "from", declarations: { opacity: "0" } },
              { selector: "to", declarations: { opacity: "1" } }
            ]);
            globalIn(first, "html, body", { margin: "0" });
            atRuleIn(first, {
              name: "PAGE",
              prelude: ":first",
              declarations: { margin: "0" },
              children: [{
                name: "top-left",
                declarations: { content: "'Example'" }
              }]
            });

            const snapshot = snapshotFrom(first);
            console.log(JSON.stringify({
              firstName,
              secondName,
              animation,
              firstCss: extractFrom(first),
              secondCss: extractFrom(second),
              defaultCss: extract(),
              snapshot
            }));
            """);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput.Trim());
        var root = json.RootElement;
        Assert.AreEqual(root.GetProperty("firstName").GetString(), root.GetProperty("secondName").GetString());
        Assert.AreEqual(string.Empty, root.GetProperty("defaultCss").GetString());
        StringAssert.Contains(root.GetProperty("secondCss").GetString() ?? string.Empty, "{color:red;display:grid;}");

        var firstCss = root.GetProperty("firstCss").GetString() ?? string.Empty;
        StringAssert.Contains(firstCss, "@keyframes " + root.GetProperty("animation").GetString());
        StringAssert.Contains(firstCss, "html,body{margin:0;}");
        StringAssert.Contains(firstCss, "@page :first{margin:0;@top-left{content:'Example';}}");

        var snapshot = root.GetProperty("snapshot");
        Assert.AreEqual("ssr-css", snapshot.GetProperty("styleId").GetString());
        Assert.AreEqual("nonce-ssr", snapshot.GetProperty("nonce").GetString());
        Assert.AreEqual(firstCss, snapshot.GetProperty("cssText").GetString());
        StringAssert.StartsWith(snapshot.GetProperty("hydrationText").GetString(), "/*ecmascript-style:v1*//*ecs:v1:");
    }

    [TestMethod]
    public async Task Runtime_ModernGroupingAtRulesPreserveStructuredNestingAndOrder()
    {
        var result = await EcmaScriptStyleModuleTestHost.RunDenoAsync(
            """
            import { style, extract } from "./runtime.mjs";

            const name = style({
              color: "black",
              $children: [
                { kind: "layer", prelude: "components", rule: { color: "red" } },
                { kind: "container", prelude: "card (width > 20rem)", rule: { display: "grid" } },
                { kind: "scope", prelude: "(.shell) to (.limit)", rule: {
                    $children: [{ kind: "selector", prelude: "> button", rule: { color: "blue" } }]
                } },
                { kind: "starting-style", prelude: null, rule: { opacity: "0" } }
              ]
            });

            console.log(JSON.stringify({ name, css: extract() }));
            """);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput.Trim());
        var name = json.RootElement.GetProperty("name").GetString();
        Assert.IsNotNull(name);
        Assert.AreEqual(
            "." + name + "{color:black;}" +
            "@layer components{." + name + "{color:red;}}" +
            "@container card (width > 20rem){." + name + "{display:grid;}}" +
            "@scope (.shell) to (.limit){." + name + " > button{color:blue;}}" +
            "@starting-style{." + name + "{opacity:0;}}",
            json.RootElement.GetProperty("css").GetString());
    }

    [TestMethod]
    public async Task Runtime_TargetContextCreatesAndAdoptsOneOwnedStylePerFragment()
    {
        var result = await EcmaScriptStyleModuleTestHost.RunDenoAsync(
            """
            import { context, styleIn, snapshotFrom } from "./runtime.mjs";

            class Element {
              constructor(localName, ownerDocument) {
                this.localName = localName;
                this.ownerDocument = ownerDocument;
                this.id = "";
                this.nonce = "";
                this.textContent = "";
              }
              appendChild(node) {
                if (typeof node.data === "string") this.textContent += node.data;
                return node;
              }
            }
            const document = {
              createElement(localName) { return new Element(localName, this); },
              createTextNode(data) { return { data }; }
            };
            const target = {
              ownerDocument: document,
              children: [],
              byId: new Map(),
              getElementById(id) { return this.byId.get(id) ?? null; },
              appendChild(node) {
                this.children.push(node);
                this.byId.set(node.id, node);
                return node;
              }
            };

            const first = context({ target, styleId: "shadow-css", nonce: "shadow-nonce" });
            const firstName = styleIn(first, { color: "red" });
            const beforeReload = target.children[0].textContent;
            const second = context({ target, styleId: "shadow-css", nonce: "shadow-nonce" });
            const secondName = styleIn(second, { color: "red" });
            const snapshot = snapshotFrom(second);

            console.log(JSON.stringify({
              firstName,
              secondName,
              styleCount: target.children.length,
              unchanged: beforeReload === target.children[0].textContent,
              nonce: target.children[0].nonce,
              snapshot
            }));
            """);

        Assert.AreEqual(0, result.ExitCode, result.StandardError);
        using var json = JsonDocument.Parse(result.StandardOutput.Trim());
        var root = json.RootElement;
        Assert.AreEqual(root.GetProperty("firstName").GetString(), root.GetProperty("secondName").GetString());
        Assert.AreEqual(1, root.GetProperty("styleCount").GetInt32());
        Assert.IsTrue(root.GetProperty("unchanged").GetBoolean());
        Assert.AreEqual("shadow-nonce", root.GetProperty("nonce").GetString());
        StringAssert.Contains(root.GetProperty("snapshot").GetProperty("cssText").GetString() ?? string.Empty, "{color:red;}");
    }
}
