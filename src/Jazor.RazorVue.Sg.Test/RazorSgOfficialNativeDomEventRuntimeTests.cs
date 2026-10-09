namespace Jazor.RazorVue.Sg.Test;

/// <summary>Exercises CLR event extensions with official SG, Vue and Happy DOM dispatch.</summary>
[TestClass]
public sealed class RazorSgOfficialNativeDomEventRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_NativeEventExtensions_PreserveIdentityTargetsAndPayloadsDuringDispatch()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/NativeDomEvents.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web
            <button id="mouse" @onclick="HandleMouse">Mouse</button>
            <input id="focus" @ref="Input" @onfocus="HandleFocus" @onkeydown="HandleKeyboard" />
            <button id="focus-button" @onclick="FocusInput">Focus</button>
            <div id="pointer" @onpointerdown="HandlePointer">Pointer</div>
            <div id="drop" @ondrop="HandleDrop" @ondrop:preventDefault>Drop</div>
            <div id="clipboard" @onpaste="HandleClipboard">Paste</div>
            <div id="touch" @ontouchstart="HandleTouch">Touch</div>
            <progress id="progress" @onprogress="HandleProgress"></progress>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/native-dom-events")]
            public partial class NativeDomEvents : ComponentBase, IVueComponent
            {
                [Parameter] public System.Action<EventRef> Seen { get; set; } = default!;
                [Parameter] public System.Action<EventTarget?, EventTarget?> Targets { get; set; } = default!;
                [Parameter] public System.Action<string, string> Stats { get; set; } = default!;
                [Parameter] public System.Action<ECMAScript.DataTransfer?, FileList?, DataTransferItemList?> Payload { get; set; } = default!;
                [Parameter] public System.Action<TouchList, TouchPoint[], Touch> Touches { get; set; } = default!;
                private ElementReference Input;

                private void FocusInput() => Input.NativeElement.Focus();

                private void HandleMouse(MouseEventArgs args)
                {
                    args.NativeEvent.PreventDefault();
                    Seen(args.NativeEvent);
                    Targets(args.Target, args.CurrentTarget);
                    Stats("mouse", args.X.ToString() + "," + args.Y.ToString());
                }

                private void HandleFocus(FocusEventArgs args)
                {
                    Seen(args.NativeEvent);
                    Targets(args.RelatedTarget, args.CurrentTarget);
                }

                private void HandleKeyboard(KeyboardEventArgs args)
                {
                    Seen(args.NativeEvent);
                    Targets(args.Target, args.CurrentTarget);
                    Stats("keyboard", args.Detail.ToString());
                }

                private void HandlePointer(PointerEventArgs args)
                {
                    Seen(args.NativeEvent);
                    Targets(args.RelatedTarget, args.Target);
                    Stats("pointer", args.TangentialPressure.ToString() + "," + args.Twist.ToString());
                }

                private void HandleDrop(DragEventArgs args)
                {
                    Seen(args.NativeEvent);
                    if (args.NativeDataTransfer == null)
                    {
                        Payload(null, args.Files, null);
                        return;
                    }
                    Payload(args.DataTransfer.NativeDataTransfer, args.DataTransfer.NativeFiles, args.DataTransfer.NativeItems);
                }

                private void HandleClipboard(ClipboardEventArgs args)
                {
                    Seen(args.NativeEvent);
                    var data = args.ClipboardData;
                    Payload(data, data?.Files, data?.Items);
                    Stats("clipboard", data?.GetData("text/plain") ?? "none");
                }

                private void HandleTouch(TouchEventArgs args)
                {
                    Seen(args.NativeEvent);
                    Touches(args.NativeTouches, args.Touches, args.Touches[0].NativeTouch);
                    Stats("touch", args.Touches[0].RadiusX.ToString() + "," + args.Touches[0].Force.ToString());
                }

                private void HandleProgress(ProgressEventArgs args)
                {
                    Seen(args.NativeEvent);
                    Stats("progress", (args.Loaded + 1).ToString() + "," + (args.Total - args.Loaded).ToString());
                    Stats("native-progress", args.NativeEvent.Loaded.ToString());
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.NativeDomEvents");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        Assert.IsFalse(observation.ModuleText.Contains(".NativeEvent", StringComparison.Ordinal), observation.ModuleText);
        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/native-dom-events.js",
            observation.ModuleText,
            "native-dom-events-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import { Window } from "npm:happy-dom@20.10.6";

            const window = new Window();
            for (const name of ["window", "document", "Node", "Element", "HTMLElement", "SVGElement"])
                globalThis[name] = name === "window" ? window : window[name];
            const { createApp, nextTick } = await import("vue");
            const { default: component } = await import("./components/native-dom-events.js");

            Deno.test("native carriers keep identity, nullability and live payloads", async () => {
                const host = document.createElement("main");
                document.body.append(host);
                const seen = [], targets = [], stats = [], payloads = [], touches = [], errors = [];
                const app = createApp(component, {
                    Seen(event) { seen.push(event); },
                    Targets(first, current) { targets.push([first, current]); },
                    Stats(name, value) { stats.push([name, value]); },
                    Payload(data, files, items) { payloads.push([data, files, items]); },
                    Touches(list, copied, first) { touches.push([list, copied, first]); }
                });
                app.config.errorHandler = error => errors.push(error.message);
                app.mount(host);
                await nextTick();
                const mouseTarget = host.querySelector("#mouse");
                const input = host.querySelector("#focus");
                const mouse = new window.MouseEvent("click", { bubbles: true, cancelable: true, clientX: 12, clientY: 34 });
                // Happy DOM omits the standard x/y aliases; provide those native fields
                // without replacing the event that owns dispatch and CurrentTarget.
                Object.defineProperties(mouse, { x: { value: 12 }, y: { value: 34 } });
                mouseTarget.dispatchEvent(mouse);
                assert.ok(seen.at(-1) === mouse, "mouse carrier identity; observed=" + seen.length);
                assert.equal(mouse.defaultPrevented, true);
                assert.ok(targets.at(-1)?.[0] === mouseTarget && targets.at(-1)?.[1] === mouseTarget, "mouse targets");
                assert.equal(mouse.currentTarget, null, "CurrentTarget is transient after dispatch");
                assert.deepEqual(stats.at(-1), ["mouse", "12,34"]);

                const focus = new window.FocusEvent("focus", { relatedTarget: mouseTarget });
                input.dispatchEvent(focus);
                assert.ok(seen.at(-1) === focus, "focus carrier identity");
                assert.ok(targets.at(-1)?.[0] === mouseTarget && targets.at(-1)?.[1] === input, "focus targets");
                host.querySelector("#focus-button").click();
                assert.ok(document.activeElement === input, "ElementReference.NativeElement focuses the same DOM node");

                const keyboard = new window.KeyboardEvent("keydown", { key: "Enter", bubbles: true, detail: 2 });
                input.dispatchEvent(keyboard);
                assert.ok(seen.at(-1) === keyboard, "keyboard carrier identity");
                assert.ok(targets.at(-1)?.[0] === input && targets.at(-1)?.[1] === input, "keyboard targets");
                assert.deepEqual(stats.at(-1), ["keyboard", "2"]);

                const pointerTarget = host.querySelector("#pointer");
                const pointer = new window.PointerEvent("pointerdown", { bubbles: true, relatedTarget: input, tangentialPressure: 0.25, twist: 45 });
                pointerTarget.dispatchEvent(pointer);
                assert.ok(seen.at(-1) === pointer, "pointer carrier identity");
                assert.ok(targets.at(-1)?.[0] === input && targets.at(-1)?.[1] === pointerTarget, "pointer targets");
                assert.deepEqual(stats.at(-1), ["pointer", "0.25,45"]);

                const transfer = new window.DataTransfer();
                transfer.setData("text/plain", "pasted text");
                const file = new window.File(["abc"], "sample.txt", { type: "text/plain" });
                transfer.items.add(file);
                // Happy DOM exposes files as an array without FileList.item(). Shape
                // that missing collection contract while retaining its real File/items.
                const files = { 0: file, length: 1, item: index => index === 0 ? file : null };
                Object.defineProperty(transfer, "files", { value: files });
                const drop = new window.DragEvent("drop", { bubbles: true, cancelable: true, dataTransfer: transfer });
                // This DOM implementation omits the native payload initializer fields.
                Object.defineProperty(drop, "dataTransfer", { value: transfer });
                host.querySelector("#drop").dispatchEvent(drop);
                assert.deepEqual(errors, [], "drag callback errors");
                assert.ok(seen.at(-1) === drop, "drop carrier identity");
                assert.equal(drop.defaultPrevented, true);
                assert.ok(payloads.at(-1)[0] === transfer, "native transfer identity");
                // Check the FileList contract and original file identity.
                assert.equal(payloads.at(-1)[1].length, 1);
                assert.ok(payloads.at(-1)[2] === transfer.items, "native item list identity");
                assert.ok(payloads.at(-1)[1].item(0) === file, "native file identity");
                const emptyDrop = new window.DragEvent("drop", { dataTransfer: null });
                Object.defineProperty(emptyDrop, "dataTransfer", { value: null });
                host.querySelector("#drop").dispatchEvent(emptyDrop);
                assert.deepEqual(payloads.at(-1), [null, undefined, null]);

                const paste = new window.ClipboardEvent("paste", { clipboardData: transfer });
                Object.defineProperty(paste, "clipboardData", { value: transfer });
                host.querySelector("#clipboard").dispatchEvent(paste);
                assert.deepEqual(errors, [], "clipboard callback errors");
                assert.ok(seen.at(-1) === paste, "clipboard carrier identity");
                assert.ok(payloads.at(-1)[0] === transfer, "clipboard payload identity");
                assert.deepEqual(stats.at(-1), ["clipboard", "pasted text"]);
                const noClipboard = new window.ClipboardEvent("paste", { clipboardData: null });
                Object.defineProperty(noClipboard, "clipboardData", { value: null });
                host.querySelector("#clipboard").dispatchEvent(noClipboard);
                assert.deepEqual(stats.at(-1), ["clipboard", "none"]);

                // Happy DOM has no complete Touch/TouchList implementation. Use a real
                // dispatched Event with shaped payloads for the native-vs-array boundary.
                const touchTarget = host.querySelector("#touch");
                const point = { identifier: 1, target: touchTarget, radiusX: 4, force: 0.5 };
                const list = { 0: point, length: 1, item: index => index === 0 ? point : null };
                const touch = new window.Event("touchstart");
                Object.defineProperty(touch, "touches", { value: list });
                touchTarget.dispatchEvent(touch);
                assert.ok(seen.at(-1) === touch, "touch carrier identity");
                assert.ok(touches.at(-1)[0] === list, "native touch list identity");
                assert.ok(Array.isArray(touches.at(-1)[1]));
                assert.ok(touches.at(-1)[1] !== list);
                assert.ok(touches.at(-1)[1][0] === point, "copied touch point identity");
                assert.ok(touches.at(-1)[2] === point, "native touch identity");
                assert.deepEqual(stats.at(-1), ["touch", "4,0.5"]);
                const progress = new window.ProgressEvent("progress", { lengthComputable: true, loaded: 3, total: 10 });
                host.querySelector("#progress").dispatchEvent(progress);
                assert.deepEqual(errors, [], "progress arithmetic callback errors");
                assert.ok(seen.at(-1) === progress, "progress carrier identity");
                assert.deepEqual(stats.at(-2), ["progress", "4,7"]);
                assert.deepEqual(stats.at(-1), ["native-progress", "3"]);
                assert.deepEqual(errors, [], "native callback errors");
                app.unmount();
                window.happyDOM.abort();
            });
            """,
            vueModuleSpecifier: "npm:vue@3.5.42/dist/vue.runtime.esm-browser.prod.js",
            restoreNpmDependencies: true);
    }
}
