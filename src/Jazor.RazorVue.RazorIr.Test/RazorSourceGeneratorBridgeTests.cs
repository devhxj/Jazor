using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorSourceGeneratorBridgeTests
{
    [TestMethod]
    public void RazorSourceGeneratorBridge_CanExtractDocumentAndImportsFromCodeDocument()
    {
        const string projectDirectory = @"D:\repo\Demo";
        const string documentPath = @"D:\repo\Demo\Pages\Counter.razor";
        const string documentText =
            """
            @page "/counter"
            <h1>Hello</h1>
            """;

        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.RazorIr.SdkSourceGenerator.Bridge",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
                    internal static class EntryPoint
                    {
                    }
                    """,
                    options: parseOptions,
                    path: "EntryPoint.cs")
            ],
            references: RazorIrTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var bridgeType = typeof(Jazor.Analyzer.RazorVue.Generation.RazorSourceGeneratorBridge);
        var method = bridgeType.GetMethod(
            "TryCreateCarrier",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        Assert.IsNotNull(method, "TryCreateCarrier bridge method was not found.");

        var args = new object?[] { compilation, documentPath, documentText, projectDirectory, null, null };
        var success = (bool)method!.Invoke(null, args)!;
        Assert.IsTrue(success, args[5] as string ?? "Bridge invocation failed.");

        Assert.IsNotNull(args[4], "Bridge carrier result was null.");
        var carrier = args[4]!;
        var carrierType = carrier.GetType();
        var extractedDocumentPath = carrierType.GetProperty("DocumentPath")?.GetValue(carrier) as string;
        var extractedDocumentText = carrierType.GetProperty("DocumentText")?.GetValue(carrier) as string;

        Assert.AreEqual(documentPath, extractedDocumentPath);
        TestContext.WriteLine("Extracted document text length: " + (extractedDocumentText?.Length ?? -1));
        Assert.AreEqual(documentText, extractedDocumentText);
    }

    [TestMethod]
    public void RazorSourceGeneratorBridge_ProbeRazorSourceDocumentRuntimeSurface()
    {
        const string projectDirectory = @"D:\repo\Demo";
        const string documentPath = @"D:\repo\Demo\Pages\Counter.razor";
        const string documentText =
            """
            @page "/counter"
            <h1>Hello</h1>
            """;

        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.RazorIr.SdkSourceGenerator.Bridge.Probe",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
                    internal static class EntryPoint
                    {
                    }
                    """,
                    options: parseOptions,
                    path: "EntryPoint.cs")
            ],
            references: RazorIrTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var bridgeType = typeof(Jazor.Analyzer.RazorVue.Generation.RazorSourceGeneratorBridge);
        var method = bridgeType.GetMethod(
            "TryCreateCarrier",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        Assert.IsNotNull(method);

        var args = new object?[] { compilation, documentPath, documentText, projectDirectory, null, null };
        var success = (bool)method!.Invoke(null, args)!;
        Assert.IsTrue(success, args[5] as string ?? "Bridge invocation failed.");

        var rawMethod = bridgeType.GetMethod(
            "TryCreateCodeDocument",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        Assert.IsNotNull(rawMethod);

        var rawArgs = new object?[] { compilation, documentPath, documentText, projectDirectory, null, null };
        var rawSuccess = (bool)rawMethod!.Invoke(null, rawArgs)!;
        Assert.IsTrue(rawSuccess, rawArgs[5] as string ?? "Raw code document bridge invocation failed.");

        var codeDocument = rawArgs[4];
        Assert.IsNotNull(codeDocument);
        var source = codeDocument!.GetType().GetProperty("Source")?.GetValue(codeDocument);
        Assert.IsNotNull(source);

        var sourceType = source!.GetType();
        TestContext.WriteLine("Source runtime type: " + sourceType.FullName);
        foreach (var property in sourceType.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
        {
            object? value;
            try
            {
                value = property.GetIndexParameters().Length == 0 ? property.GetValue(source) : "<indexed>";
            }
            catch (Exception ex)
            {
                value = "<" + ex.GetType().Name + ">";
            }

            TestContext.WriteLine(property.Name + " = " + (value?.ToString() ?? "<null>"));
        }

        foreach (var field in sourceType.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
        {
            object? value;
            try
            {
                value = field.GetValue(source);
            }
            catch (Exception ex)
            {
                value = "<" + ex.GetType().Name + ">";
            }

            TestContext.WriteLine("field " + field.Name + " = " + (value?.ToString() ?? "<null>"));
        }
    }

    public TestContext TestContext { get; set; } = default!;
}
