namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialExtendedDomEventRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorPointerAndWheelHandlers_ReadNativeEventCarriersOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ExtendedDomEvents.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <div data-pointer-id="@LastPointerId"
                 data-pointer-type="@LastPointerType"
                 data-primary="@LastPrimary"
                 data-delta-x="@LastDeltaX"
                 data-delta-mode="@LastDeltaMode"
                 @onpointerdown="HandlePointer"
                 @onwheel="HandleWheel"></div>
            """,
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components.Web;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/extended-dom-events")]
            public partial class ExtendedDomEvents : ComponentBase, IVueComponent
            {
                private long LastPointerId { get; set; }
                private string LastPointerType { get; set; } = "none";
                private bool LastPrimary { get; set; }
                private double LastDeltaX { get; set; }
                private long LastDeltaMode { get; set; }

                private void HandlePointer(PointerEventArgs args)
                {
                    LastPointerId = args.PointerId;
                    LastPointerType = args.PointerType;
                    LastPrimary = args.IsPrimary;
                }

                private void HandleWheel(WheelEventArgs args)
                {
                    LastDeltaX = args.DeltaX;
                    LastDeltaMode = args.DeltaMode;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ExtendedDomEvents");

        StringAssert.Contains(
            observation.GeneratedCSharp,
            "EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.Web.PointerEventArgs>",
            StringComparison.Ordinal);
        StringAssert.Contains(
            observation.GeneratedCSharp,
            "EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.Web.WheelEventArgs>",
            StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "onPointerdown", StringComparison.Ordinal);
        StringAssert.Contains(script, "onWheel", StringComparison.Ordinal);
        StringAssert.Contains(script, "pointerId", StringComparison.Ordinal);
        StringAssert.Contains(script, "pointerType", StringComparison.Ordinal);
        StringAssert.Contains(script, "isPrimary", StringComparison.Ordinal);
        StringAssert.Contains(script, "deltaX", StringComparison.Ordinal);
        StringAssert.Contains(script, "deltaMode", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("PointerEventArgsModule.js", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("WheelEventArgsModule.js", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/extended-dom-events.mjs",
            script,
            "official-extended-dom-events-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/extended-dom-events.mjs";

            test("native pointer and wheel event objects reach typed Blazor handlers", async () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();

                assert.equal(typeof initial.props.onPointerdown, "function");
                assert.equal(typeof initial.props.onWheel, "function");
                assert.equal(initial.props["data-pointer-id"], 0n);
                assert.equal(initial.props["data-pointer-type"], "none");
                assert.equal(initial.props["data-primary"], false);
                assert.equal(initial.props["data-delta-x"], 0);
                assert.equal(initial.props["data-delta-mode"], 0n);

                await Promise.resolve(initial.props.onPointerdown({
                    pointerId: 17,
                    pointerType: "pen",
                    isPrimary: true
                }));
                const afterPointer = render();
                assert.equal(afterPointer.props["data-pointer-id"], 17);
                assert.equal(afterPointer.props["data-pointer-type"], "pen");
                assert.equal(afterPointer.props["data-primary"], true);

                await Promise.resolve(afterPointer.props.onWheel({
                    deltaX: 2.5,
                    deltaMode: 1
                }));
                const afterWheel = render();
                assert.equal(afterWheel.props["data-delta-x"], 2.5);
                assert.equal(afterWheel.props["data-delta-mode"], 1);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorDragAndClipboardHandlers_ReadNativeEventCarriersOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/DragClipboardEvents.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <div draggable="true"
                 data-drop-effect="@DropEffect"
                 data-effect-allowed="@EffectAllowed"
                 data-clipboard-type="@ClipboardType"
                 @ondragstart="HandleDrag"
                 @onpaste="HandleClipboard"></div>
            """,
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components.Web;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/drag-clipboard-events")]
            public partial class DragClipboardEvents : ComponentBase, IVueComponent
            {
                private string DropEffect { get; set; } = "none";
                private string EffectAllowed { get; set; } = "none";
                private string ClipboardType { get; set; } = "none";

                private void HandleDrag(DragEventArgs args)
                {
                    var transfer = args.DataTransfer;
                    DropEffect = transfer.DropEffect;
                    EffectAllowed = transfer.EffectAllowed;
                }

                private void HandleClipboard(ClipboardEventArgs args)
                    => ClipboardType = args.Type;
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.DragClipboardEvents");

        StringAssert.Contains(
            observation.GeneratedCSharp,
            "EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.Web.DragEventArgs>",
            StringComparison.Ordinal);
        StringAssert.Contains(
            observation.GeneratedCSharp,
            "EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.Web.ClipboardEventArgs>",
            StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "onDragstart", StringComparison.Ordinal);
        StringAssert.Contains(script, "onPaste", StringComparison.Ordinal);
        StringAssert.Contains(script, "dataTransfer", StringComparison.Ordinal);
        StringAssert.Contains(script, "dropEffect", StringComparison.Ordinal);
        StringAssert.Contains(script, "effectAllowed", StringComparison.Ordinal);
        StringAssert.Contains(script, "ClipboardType", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("DragEventArgsModule.js", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("DataTransferModule.js", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("ClipboardEventArgsModule.js", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/drag-clipboard-events.mjs",
            script,
            "official-drag-clipboard-events-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/drag-clipboard-events.mjs";

            test("native drag and clipboard event objects reach typed Blazor handlers", async () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();

                assert.equal(typeof initial.props.onDragstart, "function");
                assert.equal(typeof initial.props.onPaste, "function");
                assert.equal(initial.props["data-drop-effect"], "none");
                assert.equal(initial.props["data-effect-allowed"], "none");
                assert.equal(initial.props["data-clipboard-type"], "none");

                await Promise.resolve(initial.props.onDragstart({
                    dataTransfer: {
                        dropEffect: "copy",
                        effectAllowed: "copyMove",
                        types: ["text/plain"]
                    }
                }));
                const afterDrag = render();
                assert.equal(afterDrag.props["data-drop-effect"], "copy");
                assert.equal(afterDrag.props["data-effect-allowed"], "copyMove");

                await Promise.resolve(afterDrag.props.onPaste({ type: "paste" }));
                const afterClipboard = render();
                assert.equal(afterClipboard.props["data-clipboard-type"], "paste");
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorTouchErrorAndProgressHandlers_ReadNativeEventCarriersOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/TouchErrorProgressEvents.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <div data-touch-detail="@TouchDetail"
                 data-touch-x="@TouchX"
                 data-touch-ctrl="@TouchCtrl"
                 data-error-message="@ErrorMessage"
                 data-error-line="@ErrorLine"
                 data-error-column="@ErrorColumn"
                 data-error-file="@ErrorFile"
                 data-error-type="@ErrorType"
                 data-progress-computable="@ProgressComputable"
                 data-progress-loaded="@ProgressLoaded"
                 data-progress-total="@ProgressTotal"
                 data-progress-type="@ProgressType"
                 @ontouchstart="HandleTouch"
                 @onerror="HandleError"
                 @onprogress="HandleProgress"></div>
            """,
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components.Web;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/touch-error-progress-events")]
            public partial class TouchErrorProgressEvents : ComponentBase, IVueComponent
            {
                private long TouchDetail { get; set; }
                private double TouchX { get; set; }
                private bool TouchCtrl { get; set; }
                private string ErrorMessage { get; set; } = "none";
                private int ErrorLine { get; set; }
                private int ErrorColumn { get; set; }
                private string ErrorFile { get; set; } = "none";
                private string ErrorType { get; set; } = "none";
                private bool ProgressComputable { get; set; }
                private long ProgressLoaded { get; set; }
                private long ProgressTotal { get; set; }
                private string ProgressType { get; set; } = "none";

                private void HandleTouch(TouchEventArgs args)
                {
                    TouchDetail = args.Detail;
                    TouchCtrl = args.CtrlKey;
                    TouchX = args.ChangedTouches[0].ClientX;
                }

                private void HandleError(ErrorEventArgs args)
                {
                    ErrorMessage = args.Message ?? "none";
                    ErrorLine = args.Lineno;
                    ErrorColumn = args.Colno;
                    ErrorFile = args.Filename ?? "none";
                    ErrorType = args.Type ?? "none";
                }

                private void HandleProgress(ProgressEventArgs args)
                {
                    ProgressComputable = args.LengthComputable;
                    ProgressLoaded = args.Loaded;
                    ProgressTotal = args.Total;
                    ProgressType = args.Type;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TouchErrorProgressEvents");

        StringAssert.Contains(
            observation.GeneratedCSharp,
            "EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.Web.TouchEventArgs>",
            StringComparison.Ordinal);
        StringAssert.Contains(
            observation.GeneratedCSharp,
            "EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.Web.ErrorEventArgs>",
            StringComparison.Ordinal);
        StringAssert.Contains(
            observation.GeneratedCSharp,
            "EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.Web.ProgressEventArgs>",
            StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "onTouchstart", StringComparison.Ordinal);
        StringAssert.Contains(script, "onError", StringComparison.Ordinal);
        StringAssert.Contains(script, "onProgress", StringComparison.Ordinal);
        StringAssert.Contains(script, "Array.from", StringComparison.Ordinal);
        StringAssert.Contains(script, "changedTouches", StringComparison.Ordinal);
        StringAssert.Contains(script, "clientX", StringComparison.Ordinal);
        StringAssert.Contains(script, "lineno", StringComparison.Ordinal);
        StringAssert.Contains(script, "colno", StringComparison.Ordinal);
        StringAssert.Contains(script, "lengthComputable", StringComparison.Ordinal);
        StringAssert.Contains(script, "loaded", StringComparison.Ordinal);
        StringAssert.Contains(script, "total", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("TouchEventArgsModule.js", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("TouchPointModule.js", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("ErrorEventArgsModule.js", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("ProgressEventArgsModule.js", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/touch-error-progress-events.mjs",
            script,
            "official-touch-error-progress-events-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/touch-error-progress-events.mjs";

            test("native touch, error, and progress events reach typed Blazor handlers", async () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();

                assert.equal(typeof initial.props.onTouchstart, "function");
                assert.equal(typeof initial.props.onError, "function");
                assert.equal(typeof initial.props.onProgress, "function");
                assert.equal(initial.props["data-touch-detail"], 0n);
                assert.equal(initial.props["data-touch-x"], 0);
                assert.equal(initial.props["data-touch-ctrl"], false);
                assert.equal(initial.props["data-error-message"], "none");
                assert.equal(initial.props["data-error-line"], 0);
                assert.equal(initial.props["data-error-column"], 0);
                assert.equal(initial.props["data-error-file"], "none");
                assert.equal(initial.props["data-error-type"], "none");
                assert.equal(initial.props["data-progress-computable"], false);
                assert.equal(initial.props["data-progress-loaded"], 0n);
                assert.equal(initial.props["data-progress-total"], 0n);
                assert.equal(initial.props["data-progress-type"], "none");

                await Promise.resolve(initial.props.onTouchstart({
                    detail: 3,
                    ctrlKey: true,
                    changedTouches: [{ clientX: 12.5 }]
                }));
                const afterTouch = render();
                assert.equal(afterTouch.props["data-touch-detail"], 3);
                assert.equal(afterTouch.props["data-touch-x"], 12.5);
                assert.equal(afterTouch.props["data-touch-ctrl"], true);

                await Promise.resolve(afterTouch.props.onError({
                    message: "boom",
                    filename: "app.js",
                    lineno: 7,
                    colno: 2,
                    type: "error"
                }));
                const afterError = render();
                assert.equal(afterError.props["data-error-message"], "boom");
                assert.equal(afterError.props["data-error-line"], 7);
                assert.equal(afterError.props["data-error-column"], 2);
                assert.equal(afterError.props["data-error-file"], "app.js");
                assert.equal(afterError.props["data-error-type"], "error");

                await Promise.resolve(afterError.props.onProgress({
                    lengthComputable: true,
                    loaded: 42,
                    total: 100,
                    type: "progress"
                }));
                const afterProgress = render();
                assert.equal(afterProgress.props["data-progress-computable"], true);
                assert.equal(afterProgress.props["data-progress-loaded"], 42);
                assert.equal(afterProgress.props["data-progress-total"], 100);
                assert.equal(afterProgress.props["data-progress-type"], "progress");
            });
            """);
    }
}
