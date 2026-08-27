using Acornima;
using Acornima.Ast;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class BlazorClrMappingTests
{
    [TestMethod]
    public void WhiteList_MapsBlazorDomEventGettersToNativeCarriers()
    {
        var expectedAliases = new (string TypeName, string RuntimeName)[]
        {
            ("Microsoft.AspNetCore.Components.Web.MouseEventArgs", "MouseEvent"),
            ("Microsoft.AspNetCore.Components.Web.KeyboardEventArgs", "KeyboardEvent"),
            ("Microsoft.AspNetCore.Components.Web.FocusEventArgs", "FocusEvent"),
            ("Microsoft.AspNetCore.Components.Web.PointerEventArgs", "PointerEvent"),
            ("Microsoft.AspNetCore.Components.Web.WheelEventArgs", "WheelEvent"),
            ("Microsoft.AspNetCore.Components.Web.DragEventArgs", "DragEvent"),
            ("Microsoft.AspNetCore.Components.Web.DataTransfer", "DataTransfer"),
            ("Microsoft.AspNetCore.Components.Web.ClipboardEventArgs", "ClipboardEvent"),
            ("Microsoft.AspNetCore.Components.Web.TouchEventArgs", "TouchEvent"),
            ("Microsoft.AspNetCore.Components.Web.TouchPoint", "Touch"),
            ("Microsoft.AspNetCore.Components.Web.ErrorEventArgs", "ErrorEvent"),
            ("Microsoft.AspNetCore.Components.Web.ProgressEventArgs", "ProgressEvent"),
            ("Microsoft.AspNetCore.Components.ChangeEventArgs", "JazorEvent"),
            ("Microsoft.AspNetCore.Components.ElementReference", "HTMLElement")
        };
        var expectedMembers = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Microsoft.AspNetCore.Components.Web.MouseEventArgs.Detail.get"] = "__arg1.detail",
            ["Microsoft.AspNetCore.Components.Web.MouseEventArgs.ScreenX.get"] = "__arg1.screenX",
            ["Microsoft.AspNetCore.Components.Web.MouseEventArgs.ScreenY.get"] = "__arg1.screenY",
            ["Microsoft.AspNetCore.Components.Web.MouseEventArgs.ClientX.get"] = "__arg1.clientX",
            ["Microsoft.AspNetCore.Components.Web.MouseEventArgs.ClientY.get"] = "__arg1.clientY",
            ["Microsoft.AspNetCore.Components.Web.MouseEventArgs.OffsetX.get"] = "__arg1.offsetX",
            ["Microsoft.AspNetCore.Components.Web.MouseEventArgs.OffsetY.get"] = "__arg1.offsetY",
            ["Microsoft.AspNetCore.Components.Web.MouseEventArgs.PageX.get"] = "__arg1.pageX",
            ["Microsoft.AspNetCore.Components.Web.MouseEventArgs.PageY.get"] = "__arg1.pageY",
            ["Microsoft.AspNetCore.Components.Web.MouseEventArgs.MovementX.get"] = "__arg1.movementX",
            ["Microsoft.AspNetCore.Components.Web.MouseEventArgs.MovementY.get"] = "__arg1.movementY",
            ["Microsoft.AspNetCore.Components.Web.MouseEventArgs.Button.get"] = "__arg1.button",
            ["Microsoft.AspNetCore.Components.Web.MouseEventArgs.Buttons.get"] = "__arg1.buttons",
            ["Microsoft.AspNetCore.Components.Web.MouseEventArgs.CtrlKey.get"] = "__arg1.ctrlKey",
            ["Microsoft.AspNetCore.Components.Web.MouseEventArgs.ShiftKey.get"] = "__arg1.shiftKey",
            ["Microsoft.AspNetCore.Components.Web.MouseEventArgs.AltKey.get"] = "__arg1.altKey",
            ["Microsoft.AspNetCore.Components.Web.MouseEventArgs.MetaKey.get"] = "__arg1.metaKey",
            ["Microsoft.AspNetCore.Components.Web.MouseEventArgs.Type.get"] = "__arg1.type",
            ["Microsoft.AspNetCore.Components.Web.KeyboardEventArgs.Key.get"] = "__arg1.key",
            ["Microsoft.AspNetCore.Components.Web.KeyboardEventArgs.Code.get"] = "__arg1.code",
            ["Microsoft.AspNetCore.Components.Web.KeyboardEventArgs.Location.get"] = "__arg1.location",
            ["Microsoft.AspNetCore.Components.Web.KeyboardEventArgs.Repeat.get"] = "__arg1.repeat",
            ["Microsoft.AspNetCore.Components.Web.KeyboardEventArgs.CtrlKey.get"] = "__arg1.ctrlKey",
            ["Microsoft.AspNetCore.Components.Web.KeyboardEventArgs.ShiftKey.get"] = "__arg1.shiftKey",
            ["Microsoft.AspNetCore.Components.Web.KeyboardEventArgs.AltKey.get"] = "__arg1.altKey",
            ["Microsoft.AspNetCore.Components.Web.KeyboardEventArgs.MetaKey.get"] = "__arg1.metaKey",
            ["Microsoft.AspNetCore.Components.Web.KeyboardEventArgs.Type.get"] = "__arg1.type",
            ["Microsoft.AspNetCore.Components.Web.KeyboardEventArgs.IsComposing.get"] = "__arg1.isComposing",
            ["Microsoft.AspNetCore.Components.Web.FocusEventArgs.Type.get"] = "__arg1.type",
            ["Microsoft.AspNetCore.Components.Web.PointerEventArgs.PointerId.get"] = "__arg1.pointerId",
            ["Microsoft.AspNetCore.Components.Web.PointerEventArgs.Width.get"] = "__arg1.width",
            ["Microsoft.AspNetCore.Components.Web.PointerEventArgs.Height.get"] = "__arg1.height",
            ["Microsoft.AspNetCore.Components.Web.PointerEventArgs.Pressure.get"] = "__arg1.pressure",
            ["Microsoft.AspNetCore.Components.Web.PointerEventArgs.TiltX.get"] = "__arg1.tiltX",
            ["Microsoft.AspNetCore.Components.Web.PointerEventArgs.TiltY.get"] = "__arg1.tiltY",
            ["Microsoft.AspNetCore.Components.Web.PointerEventArgs.PointerType.get"] = "__arg1.pointerType",
            ["Microsoft.AspNetCore.Components.Web.PointerEventArgs.IsPrimary.get"] = "__arg1.isPrimary",
            ["Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaX.get"] = "__arg1.deltaX",
            ["Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaY.get"] = "__arg1.deltaY",
            ["Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaZ.get"] = "__arg1.deltaZ",
            ["Microsoft.AspNetCore.Components.Web.WheelEventArgs.DeltaMode.get"] = "__arg1.deltaMode",
            ["Microsoft.AspNetCore.Components.Web.DragEventArgs.DataTransfer.get"] = "__arg1.dataTransfer",
            ["Microsoft.AspNetCore.Components.Web.DataTransfer.DropEffect.get"] = "__arg1.dropEffect",
            ["Microsoft.AspNetCore.Components.Web.DataTransfer.EffectAllowed.get"] = "__arg1.effectAllowed",
            ["Microsoft.AspNetCore.Components.Web.DataTransfer.Types.get"] = "__arg1.types",
            ["Microsoft.AspNetCore.Components.Web.ClipboardEventArgs.Type.get"] = "__arg1.type",
            ["Microsoft.AspNetCore.Components.Web.TouchEventArgs.Detail.get"] = "__arg1.detail",
            ["Microsoft.AspNetCore.Components.Web.TouchEventArgs.Touches.get"] = "Array.from(__arg1.touches)",
            ["Microsoft.AspNetCore.Components.Web.TouchEventArgs.TargetTouches.get"] = "Array.from(__arg1.targetTouches)",
            ["Microsoft.AspNetCore.Components.Web.TouchEventArgs.ChangedTouches.get"] = "Array.from(__arg1.changedTouches)",
            ["Microsoft.AspNetCore.Components.Web.TouchEventArgs.CtrlKey.get"] = "__arg1.ctrlKey",
            ["Microsoft.AspNetCore.Components.Web.TouchEventArgs.ShiftKey.get"] = "__arg1.shiftKey",
            ["Microsoft.AspNetCore.Components.Web.TouchEventArgs.AltKey.get"] = "__arg1.altKey",
            ["Microsoft.AspNetCore.Components.Web.TouchEventArgs.MetaKey.get"] = "__arg1.metaKey",
            ["Microsoft.AspNetCore.Components.Web.TouchEventArgs.Type.get"] = "__arg1.type",
            ["Microsoft.AspNetCore.Components.Web.TouchPoint.Identifier.get"] = "__arg1.identifier",
            ["Microsoft.AspNetCore.Components.Web.TouchPoint.ScreenX.get"] = "__arg1.screenX",
            ["Microsoft.AspNetCore.Components.Web.TouchPoint.ScreenY.get"] = "__arg1.screenY",
            ["Microsoft.AspNetCore.Components.Web.TouchPoint.ClientX.get"] = "__arg1.clientX",
            ["Microsoft.AspNetCore.Components.Web.TouchPoint.ClientY.get"] = "__arg1.clientY",
            ["Microsoft.AspNetCore.Components.Web.TouchPoint.PageX.get"] = "__arg1.pageX",
            ["Microsoft.AspNetCore.Components.Web.TouchPoint.PageY.get"] = "__arg1.pageY",
            ["Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Message.get"] = "__arg1.message",
            ["Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Filename.get"] = "__arg1.filename",
            ["Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Lineno.get"] = "__arg1.lineno",
            ["Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Colno.get"] = "__arg1.colno",
            ["Microsoft.AspNetCore.Components.Web.ErrorEventArgs.Type.get"] = "__arg1.type",
            ["Microsoft.AspNetCore.Components.Web.ProgressEventArgs.LengthComputable.get"] = "__arg1.lengthComputable",
            ["Microsoft.AspNetCore.Components.Web.ProgressEventArgs.Loaded.get"] = "__arg1.loaded",
            ["Microsoft.AspNetCore.Components.Web.ProgressEventArgs.Total.get"] = "__arg1.total",
            ["Microsoft.AspNetCore.Components.Web.ProgressEventArgs.Type.get"] = "__arg1.type",
            ["static Microsoft.AspNetCore.Components.ElementReferenceExtensions.FocusAsync(Microsoft.AspNetCore.Components.ElementReference)"] = "Promise.resolve(__arg1.focus())",
            ["static Microsoft.AspNetCore.Components.ElementReferenceExtensions.FocusAsync(Microsoft.AspNetCore.Components.ElementReference, bool)"] = "Promise.resolve(__arg1.focus({ preventScroll: __arg2 }))"
        };
        var expectedImports = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Microsoft.AspNetCore.Components.ChangeEventArgs.captureChangeEvent"] = "captureChangeEvent",
            ["Microsoft.AspNetCore.Components.ChangeEventArgs.Value.get"] = "getChangeEventValue"
        };

        foreach (var (typeName, runtimeName) in expectedAliases)
        {
            AssertTypeAlias(typeName, runtimeName);

            // The first slice is a DOM-origin read projection. Discarded constructors
            // and setters stay in the CLR source as explicit rejection markers, but
            // are intentionally absent from the generated whitelist.
            var expectedKeys = expectedMembers.Keys
                .Concat(expectedImports.Keys)
                .Where(key => key.StartsWith(typeName + ".", StringComparison.Ordinal))
                .OrderBy(static key => key, StringComparer.Ordinal)
                .ToArray();
            var actualKeys = WhiteList.Members.Keys
                .Where(key => key.StartsWith(typeName + ".", StringComparison.Ordinal))
                .OrderBy(static key => key, StringComparer.Ordinal)
                .ToArray();
            CollectionAssert.AreEqual(expectedKeys, actualKeys, $"Unexpected mapped surface for {typeName}.");
        }

        foreach (var (memberName, template) in expectedMembers)
        {
            AssertInline(memberName, template);
        }

        foreach (var (memberName, exportName) in expectedImports)
        {
            AssertImport(memberName, exportName, "Microsoft/AspNetCore/Components/ChangeEventArgsModule.js");
        }
    }

    [TestMethod]
    public void SemanticWalker_BlazorDomEventGetters_ReadNativeEventProperties()
    {
        var block = GetBlockOperation(
            """
            using Microsoft.AspNetCore.Components.Web;
            using Microsoft.AspNetCore.Components;
            using System.Threading.Tasks;

            public static class BlazorEventScenario
            {
                public static string Evaluate(
                    MouseEventArgs mouse,
                    KeyboardEventArgs keyboard,
                    FocusEventArgs focus,
                    PointerEventArgs pointer,
                    WheelEventArgs wheel,
                    DragEventArgs drag,
                    ClipboardEventArgs clipboard,
                    TouchEventArgs touch,
                    ErrorEventArgs error,
                    ProgressEventArgs progress,
                    ChangeEventArgs change,
                    ElementReference element)
                {
                    var clientX = mouse.ClientX;
                    var key = keyboard.Key;
                    var pointerId = pointer.PointerId;
                    var pointerType = pointer.PointerType;
                    var deltaX = wheel.DeltaX;
                    var deltaMode = wheel.DeltaMode;
                    var transfer = drag.DataTransfer;
                    var dropEffect = transfer.DropEffect;
                    var effectAllowed = transfer.EffectAllowed;
                    var clipboardType = clipboard.Type;
                    var detail = touch.Detail;
                    var firstTouch = touch.ChangedTouches[0];
                    var touchX = firstTouch.ClientX;
                    var errorMessage = error.Message;
                    var errorLine = error.Lineno;
                    var errorColumn = error.Colno;
                    var errorFile = error.Filename;
                    var errorType = error.Type;
                    var progressLengthComputable = progress.LengthComputable;
                    var loaded = progress.Loaded;
                    var total = progress.Total;
                    var progressType = progress.Type;
                    _ = element.FocusAsync();
                    _ = element.FocusAsync(true);
                    var value = change.Value;
                    return focus.Type + key + clientX + pointerId + pointerType + deltaX + deltaMode + dropEffect + effectAllowed + clipboardType + (string)value!;
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers();
        Assert.HasCount(1, imports, body);
        Assert.AreEqual("Microsoft/AspNetCore/Components/ChangeEventArgsModule.js", imports[0].Key, body);
        var importSpecifier = imports[0].Value.OfType<ImportSpecifier>().Single();
        Assert.IsInstanceOfType<Identifier>(importSpecifier.Imported, body);
        Assert.AreEqual("getChangeEventValue", ((Identifier)importSpecifier.Imported).Name, body);
        StringAssert.Contains(body, "clientX", StringComparison.Ordinal);
        StringAssert.Contains(body, "keyboard.key", StringComparison.Ordinal);
        StringAssert.Contains(body, "focus.type", StringComparison.Ordinal);
        StringAssert.Contains(body, "pointer.pointerId", StringComparison.Ordinal);
        StringAssert.Contains(body, "pointer.pointerType", StringComparison.Ordinal);
        StringAssert.Contains(body, "wheel.deltaX", StringComparison.Ordinal);
        StringAssert.Contains(body, "wheel.deltaMode", StringComparison.Ordinal);
        StringAssert.Contains(body, "drag.dataTransfer", StringComparison.Ordinal);
        StringAssert.Contains(body, "transfer.dropEffect", StringComparison.Ordinal);
        StringAssert.Contains(body, "transfer.effectAllowed", StringComparison.Ordinal);
        StringAssert.Contains(body, "clipboard.type", StringComparison.Ordinal);
        StringAssert.Contains(body, "touch.detail", StringComparison.Ordinal);
        StringAssert.Contains(body, "Array.from(touch.changedTouches)", StringComparison.Ordinal);
        StringAssert.Contains(body, "firstTouch.clientX", StringComparison.Ordinal);
        StringAssert.Contains(body, "error.message", StringComparison.Ordinal);
        StringAssert.Contains(body, "error.lineno", StringComparison.Ordinal);
        StringAssert.Contains(body, "error.colno", StringComparison.Ordinal);
        StringAssert.Contains(body, "error.filename", StringComparison.Ordinal);
        StringAssert.Contains(body, "error.type", StringComparison.Ordinal);
        StringAssert.Contains(body, "progress.lengthComputable", StringComparison.Ordinal);
        StringAssert.Contains(body, "progress.loaded", StringComparison.Ordinal);
        StringAssert.Contains(body, "progress.total", StringComparison.Ordinal);
        StringAssert.Contains(body, "progress.type", StringComparison.Ordinal);
        StringAssert.Contains(body, "Promise.resolve(element.focus())", StringComparison.Ordinal);
        StringAssert.Contains(body, "Promise.resolve(element.focus({ preventScroll: true }))", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(mouse, keyboard, focus, pointer, wheel) " + body);
    }

    [TestMethod]
    public void SemanticWalker_BlazorDomEventSetter_IsRejectedAtTheUsageSite()
    {
        var block = GetBlockOperation(
            """
            using Microsoft.AspNetCore.Components.Web;

            public static class BlazorEventScenario
            {
                public static void Evaluate(PointerEventArgs pointer)
                {
                    pointer.PointerId = 4;
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "External member", StringComparison.Ordinal);
        StringAssert.Contains(
            exception.Message,
            "Microsoft.AspNetCore.Components.Web.PointerEventArgs.PointerId.set",
            StringComparison.Ordinal);
    }

    [TestMethod]
    public void SemanticWalker_BlazorDomEventConstruction_IsRejectedAtTheUsageSite()
    {
        var block = GetBlockOperation(
            """
            using Microsoft.AspNetCore.Components.Web;

            public static class BlazorEventScenario
            {
                public static PointerEventArgs Evaluate()
                {
                    return new PointerEventArgs();
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "PointerEventArgs", StringComparison.Ordinal);
        StringAssert.Contains(exception.Message, "not supported", StringComparison.Ordinal);
    }

    [TestMethod]
    public void SemanticWalker_BlazorDragDataTransferSetter_IsRejectedAtTheUsageSite()
    {
        var block = GetBlockOperation(
            """
            using Microsoft.AspNetCore.Components.Web;

            public static class BlazorEventScenario
            {
                public static void Evaluate(DragEventArgs drag)
                {
                    var transfer = drag.DataTransfer;
                    transfer.DropEffect = "copy";
                }
            }
            """);

        var exception = Assert.ThrowsExactly<OperationTransformationException>(() =>
            new SemanticWalker(true).Visit(block, new SenseArgument()));

        StringAssert.Contains(exception.Message, "External member", StringComparison.Ordinal);
        StringAssert.Contains(
            exception.Message,
            "Microsoft.AspNetCore.Components.Web.DataTransfer.DropEffect.set",
            StringComparison.Ordinal);
    }

    private static void AssertTypeAlias(string typeName, string runtimeName)
    {
        Assert.IsTrue(WhiteList.Types.TryGetValue(typeName, out var mapping), $"Missing Blazor type mapping: {typeName}");
        Assert.AreEqual(ECMAScript.Contract.Op.Alias, mapping.Op);
        Assert.AreEqual(runtimeName, mapping.Value);
        Assert.IsNull(mapping.RuntimeValueCarrier);
    }

    private static void AssertInline(string memberName, string template)
    {
        Assert.IsTrue(WhiteList.Members.TryGetValue(memberName, out var mapping), $"Missing Blazor member mapping: {memberName}");
        Assert.AreEqual(ECMAScript.Contract.Op.Inline, mapping.Op);
        Assert.AreEqual(template, mapping.Value);
    }

    private static void AssertImport(string memberName, string exportName, string modulePath)
    {
        Assert.IsTrue(WhiteList.Members.TryGetValue(memberName, out var mapping), $"Missing Blazor member mapping: {memberName}");
        Assert.AreEqual(ECMAScript.Contract.Op.Import, mapping.Op);
        Assert.AreEqual(exportName, mapping.Value);
        Assert.AreEqual(modulePath, mapping.Path);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var references = TestMetadataReferences.Net11
            .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(EventCallback).Assembly.Location))
            .Add(MetadataReference.CreateFromFile(typeof(MouseEventArgs).Assembly.Location));
        var compilation = CSharpCompilation.Create(
            "EcmaScriptBlazorMappingScenario",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "Evaluate");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
