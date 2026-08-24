using Acornima;
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
public sealed class EcmaScriptBlazorMappingTests
{
    [TestMethod]
    public void WhiteList_MapsBlazorDomEventGettersToNativeCarriers()
    {
        var expectedAliases = new (string TypeName, string RuntimeName)[]
        {
            ("Microsoft.AspNetCore.Components.Web.MouseEventArgs", "MouseEvent"),
            ("Microsoft.AspNetCore.Components.Web.KeyboardEventArgs", "KeyboardEvent"),
            ("Microsoft.AspNetCore.Components.Web.FocusEventArgs", "FocusEvent")
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
            ["Microsoft.AspNetCore.Components.Web.FocusEventArgs.Type.get"] = "__arg1.type"
        };

        foreach (var (typeName, runtimeName) in expectedAliases)
        {
            AssertTypeAlias(typeName, runtimeName);

            // The first slice is a DOM-origin read projection. Any constructor or
            // setter entry would silently claim support for synthetic EventArgs values.
            var expectedKeys = expectedMembers.Keys
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
    }

    [TestMethod]
    public void SemanticWalker_BlazorDomEventGetters_ReadNativeEventProperties()
    {
        var block = GetBlockOperation(
            """
            using Microsoft.AspNetCore.Components.Web;

            public static class BlazorEventScenario
            {
                public static string Evaluate(MouseEventArgs mouse, KeyboardEventArgs keyboard, FocusEventArgs focus)
                {
                    var clientX = mouse.ClientX;
                    var key = keyboard.Key;
                    return focus.Type + key + clientX;
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        Assert.HasCount(0, argument.FlushImportSpecifiers(), body);
        StringAssert.Contains(body, "clientX", StringComparison.Ordinal);
        StringAssert.Contains(body, "keyboard.key", StringComparison.Ordinal);
        StringAssert.Contains(body, "focus.type", StringComparison.Ordinal);
        _ = new Parser().ParseScript("function verify(mouse, keyboard, focus) " + body);
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
