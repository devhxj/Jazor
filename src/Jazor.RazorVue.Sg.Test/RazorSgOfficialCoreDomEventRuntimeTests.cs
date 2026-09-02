using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialCoreDomEventRuntimeTests
{
    [TestMethod]
    public void BlazorReferenceEventRegistry_MapsCoreDomNamesToTypedArguments()
    {
        var handlers = typeof(EventHandlers)
            .GetCustomAttributes<EventHandlerAttribute>(inherit: false)
            .ToDictionary(static attribute => attribute.AttributeName, static attribute => attribute.EventArgsType, StringComparer.Ordinal);

        CollectionAssert.AreEquivalent(
            new Dictionary<string, Type>(StringComparer.Ordinal)
            {
                ["onclick"] = typeof(MouseEventArgs),
                ["onkeydown"] = typeof(KeyboardEventArgs),
                ["onfocus"] = typeof(FocusEventArgs),
                ["onchange"] = typeof(ChangeEventArgs),
                ["oninput"] = typeof(ChangeEventArgs)
            },
            handlers
                .Where(static pair => pair.Key is "onclick" or "onkeydown" or "onfocus" or "onchange" or "oninput")
                .ToDictionary(static pair => pair.Key, static pair => pair.Value, StringComparer.Ordinal));
    }

    [TestMethod]
    public void BlazorReferenceChangeEventReader_ShapesStringBooleanAndStringArrayValues()
    {
        var stringValue = ReadReferenceChangeEvent("{\"value\":\"draft\"}");
        Assert.AreEqual("draft", stringValue.Value);

        var boolValue = ReadReferenceChangeEvent("{\"value\":true}");
        Assert.AreEqual(true, boolValue.Value);

        var arrayValue = ReadReferenceChangeEvent("{\"value\":[\"one\",\"two\"]}");
        CollectionAssert.AreEqual(new[] { "one", "two" }, (string?[])arrayValue.Value!);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorCoreDomTypedHandlers_ReadNativeMouseKeyboardFocusEventsOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/CoreDomEvents.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web
            @using Demo.Components

            <div>
                <button id="mouse" @onclick="HandleMouse">Mouse</button>
                <input id="keyboard" @onkeydown="@(args => HandleKeyboard(args))" />
                <input id="focus" @onfocus="HandleFocus" />
                <NativeEventForwarder OnMouse="HandleForwardedMouse" />
                <span id="mouse-type">@MouseType</span>
                <span id="mouse-x">@MouseX</span>
                <span id="keyboard-key">@KeyboardKey</span>
                <span id="keyboard-code">@KeyboardCode</span>
                <span id="focus-type">@FocusType</span>
                <span id="forwarded-x">@ForwardedMouseX</span>
            </div>
            """,
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components.Web;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/native-event-forwarder")]
                public sealed class NativeEventForwarder : ComponentBase, IVueComponent
                {
                    [Parameter] public EventCallback<MouseEventArgs> OnMouse { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/core-dom-events")]
                public partial class CoreDomEvents : ComponentBase, IVueComponent
                {
                    private string MouseType { get; set; } = "none";
                    private double MouseX { get; set; }
                    private string KeyboardKey { get; set; } = "none";
                    private string KeyboardCode { get; set; } = "none";
                    private string FocusType { get; set; } = "none";
                    private double ForwardedMouseX { get; set; }

                    private void HandleMouse(MouseEventArgs args)
                    {
                        MouseType = args.Type;
                        MouseX = args.ClientX;
                    }

                    private void HandleKeyboard(KeyboardEventArgs args)
                    {
                        KeyboardKey = args.Key;
                        KeyboardCode = args.Code;
                    }

                    private void HandleFocus(FocusEventArgs args)
                        => FocusType = args.Type ?? "none";

                    private void HandleForwardedMouse(MouseEventArgs args)
                        => ForwardedMouseX = args.ClientX;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.CoreDomEvents");

        StringAssert.Contains(
            observation.GeneratedCSharp,
            "EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.Web.MouseEventArgs>",
            StringComparison.Ordinal);
        StringAssert.Contains(
            observation.GeneratedCSharp,
            "EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.Web.KeyboardEventArgs>",
            StringComparison.Ordinal);
        StringAssert.Contains(
            observation.GeneratedCSharp,
            "EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.Web.FocusEventArgs>",
            StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "onClick", StringComparison.Ordinal);
        StringAssert.Contains(script, "onKeydown", StringComparison.Ordinal);
        StringAssert.Contains(script, "onFocus", StringComparison.Ordinal);
        StringAssert.Contains(script, "clientX", StringComparison.Ordinal);
        StringAssert.Contains(script, "args.key", StringComparison.Ordinal);
        StringAssert.Contains(script, "args.type", StringComparison.Ordinal);
        StringAssert.Contains(script, "native-event-forwarder", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("MouseEventArgsModule.js", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("KeyboardEventArgsModule.js", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("FocusEventArgsModule.js", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/core-dom-events.mjs",
            script,
            "official-core-dom-events-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/core-dom-events.mjs";
            import forwarder from "./components/native-event-forwarder.mjs";

            function findNode(node, predicate) {
              if (Array.isArray(node)) {
                for (const child of node) {
                  const found = findNode(child, predicate);
                  if (found) return found;
                }
                return null;
              }

              if (!node || typeof node !== "object") return null;
              if (predicate(node)) return node;
              return findNode(node.children, predicate);
            }

            test("native mouse, keyboard, and focus events reach typed handlers and forwarded callbacks", async () => {
              const render = component.setup({}, { slots: {} });
              const initial = render();
              const mouse = findNode(initial, node => node.name === "button" && node.props?.id === "mouse");
              const keyboard = findNode(initial, node => node.name === "input" && node.props?.id === "keyboard");
              const focus = findNode(initial, node => node.name === "input" && node.props?.id === "focus");
              const child = findNode(initial, node => node.name === forwarder);
              assert.ok(mouse);
              assert.ok(keyboard);
              assert.ok(focus);
              assert.ok(child);

              await Promise.resolve(mouse.props.onClick({ type: "click", clientX: 37 }));
              await Promise.resolve(keyboard.props.onKeydown({ key: "Enter", code: "Enter" }));
              await Promise.resolve(focus.props.onFocus({ type: "focus" }));
              await Promise.resolve(child.props.OnMouse({ type: "click", clientX: 51 }));

              const updated = render();
              assert.equal(findNode(updated, node => node.props?.id === "mouse-type").children, "click");
              assert.deepEqual(findNode(updated, node => node.props?.id === "mouse-x").children, [37]);
              assert.equal(findNode(updated, node => node.props?.id === "keyboard-key").children, "Enter");
              assert.equal(findNode(updated, node => node.props?.id === "keyboard-code").children, "Enter");
              assert.equal(findNode(updated, node => node.props?.id === "focus-type").children, "focus");
              assert.deepEqual(findNode(updated, node => node.props?.id === "forwarded-x").children, [51]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/native-event-forwarder.mjs"] = "export default { name: \"native-event-forwarder\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorChangeHandlers_CaptureEventTimeValueAndKeepBindDirectOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/CoreDomChangeEvents.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <input id="change" @bind="BoundText" @bind:event="oninput" @onchange="HandleChange" />
            <input id="checkbox" type="checkbox" @onchange="HandleCheckbox" />
            <select id="multiple" multiple @onchange="HandleMultiple">
                <option value="one">One</option>
                <option value="two">Two</option>
            </select>
            <span id="bound">@BoundText</span>
            <span id="captured-before">@CapturedBefore</span>
            <span id="captured-after">@CapturedAfter</span>
            <span id="checkbox-value">@CheckboxValue</span>
            <span id="multiple-value">@MultipleValue</span>
            """,
            codeBehindSource:
            """
            using System.Threading.Tasks;
            using Microsoft.AspNetCore.Components.Web;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/core-dom-change-events")]
            public partial class CoreDomChangeEvents : ComponentBase, IVueComponent
            {
                private string BoundText { get; set; } = "initial";
                private string CapturedBefore { get; set; } = "none";
                private string CapturedAfter { get; set; } = "none";
                private bool CheckboxValue { get; set; }
                private string MultipleValue { get; set; } = "none";

                private async Task HandleChange(ChangeEventArgs args)
                {
                    CapturedBefore = (string)args.Value!;
                    await Task.Yield();
                    CapturedAfter = (string)args.Value!;
                }

                private void HandleCheckbox(ChangeEventArgs args)
                    => CheckboxValue = (bool)args.Value!;

                private void HandleMultiple(ChangeEventArgs args)
                {
                    var values = (string?[])args.Value!;
                    MultipleValue = values.Length == 0
                        ? "0"
                        : values.Length + ":" + values[0];
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.CoreDomChangeEvents");

        StringAssert.Contains(
            observation.GeneratedCSharp,
            "EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.ChangeEventArgs>",
            StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "CreateBinder", StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "captureChangeEvent(event)", StringComparison.Ordinal);
        StringAssert.Contains(script, "getChangeEventValue(", StringComparison.Ordinal);
        StringAssert.Contains(script, "await Promise.resolve()", StringComparison.Ordinal);
        StringAssert.Contains(script, "event => state.BoundText = event.target[\"value\"]", StringComparison.Ordinal);
        StringAssert.Contains(script, "onChange", StringComparison.Ordinal);
        StringAssert.Contains(script, "onInput", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("CreateBinder", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("eventOrValue", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/core-dom-change-events.mjs",
            script,
            "official-core-dom-change-events-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            globalThis.HTMLInputElement = class HTMLInputElement {
              constructor(type, value, checked) {
                this.type = type;
                this.value = value;
                this.checked = checked;
              }
            };

            globalThis.HTMLTextAreaElement = class HTMLTextAreaElement {
              constructor(value) {
                this.value = value;
              }
            };

            globalThis.HTMLOptionElement = class HTMLOptionElement {
              constructor(value) {
                this.value = value;
              }
            };

            globalThis.HTMLSelectElement = class HTMLSelectElement {
              constructor(values) {
                this.multiple = true;
                this.value = values[0] || "";
                const options = values.map(value => new HTMLOptionElement(value));
                this.selectedOptions = {
                  length: options.length,
                  item(index) { return options[index]; }
                };
              }
            };

            import component from "./components/core-dom-change-events.mjs";

            function findNode(node, predicate) {
              if (Array.isArray(node)) {
                for (const child of node) {
                  const found = findNode(child, predicate);
                  if (found) return found;
                }
                return null;
              }

              if (!node || typeof node !== "object") return null;
              if (predicate(node)) return node;
              return findNode(node.children, predicate);
            }

            test("ChangeEventArgs captures string, bool, and multiple-select values at listener time", async () => {
              const render = component.setup({}, { slots: {} });
              const initial = render();
              const change = findNode(initial, node => node.name === "input" && node.props?.id === "change");
              const checkbox = findNode(initial, node => node.name === "input" && node.props?.id === "checkbox");
              const multiple = findNode(initial, node => node.name === "select" && node.props?.id === "multiple");
              assert.ok(change);
              assert.ok(checkbox);
              assert.ok(multiple);
              assert.equal(change.props.value, "initial");

              const changeTarget = new HTMLInputElement("text", "first", false);
              const pending = change.props.onChange({ target: changeTarget });
              changeTarget.value = "second";
              await pending;
              assert.equal(findNode(render(), node => node.props?.id === "captured-before").children, "first");
              assert.equal(findNode(render(), node => node.props?.id === "captured-after").children, "first");

              await Promise.resolve(change.props.onInput({ target: { value: "bound" } }));
              assert.equal(findNode(render(), node => node.props?.id === "bound").children, "bound");
              assert.equal(findNode(render(), node => node.name === "input" && node.props?.id === "change").props.value, "bound");

              await Promise.resolve(checkbox.props.onChange({
                target: new HTMLInputElement("checkbox", "on", true)
              }));
              assert.deepEqual(findNode(render(), node => node.props?.id === "checkbox-value").children, [true]);

              await Promise.resolve(multiple.props.onChange({
                target: new HTMLSelectElement(["one", "two"])
              }));
              assert.equal(findNode(render(), node => node.props?.id === "multiple-value").children, "2:one");
            });
            """);
    }

    private static ChangeEventArgs ReadReferenceChangeEvent(string json)
    {
        using var document = JsonDocument.Parse(json);
        var readerType = typeof(EventHandlers).Assembly.GetType(
            "Microsoft.AspNetCore.Components.Web.ChangeEventArgsReader",
            throwOnError: true)!;
        var readMethod = readerType.GetMethod(
            "Read",
            BindingFlags.Static | BindingFlags.NonPublic,
            binder: null,
            [typeof(JsonElement)],
            modifiers: null)!;

        return (ChangeEventArgs)readMethod.Invoke(null, [document.RootElement])!;
    }
}
