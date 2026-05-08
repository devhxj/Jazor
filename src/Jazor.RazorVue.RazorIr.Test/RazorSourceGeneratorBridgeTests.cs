using Microsoft.AspNetCore.Razor.Language;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorSourceGeneratorBridgeTests
{
    [TestMethod]
    public void RazorSourceGeneratorBridge_CanExtractDocumentAndImportsFromOfficialCodeDocument()
    {
        const string documentPath = @"D:\repo\Demo\Pages\Counter.razor";
        const string documentText =
            """
            @page "/counter"
            <h1>Hello</h1>
            """;
        const string importPath = @"D:\repo\Demo\Pages\_Imports.razor";
        const string importText = "@using System";

        var codeDocument = RazorIrTestHost.CreateCodeDocument(
            documentPath,
            documentText,
            [RazorSourceDocument.Create(importText, importPath)],
            tagHelpers: null);

        var bridgeType = typeof(Jazor.Analyzer.RazorVue.Generation.RazorSourceGeneratorBridge);
        var method = bridgeType.GetMethod(
            "TryReadCarrier",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        Assert.IsNotNull(method, "TryReadCarrier bridge method was not found.");

        var args = new object?[] { codeDocument, null, null };
        var success = (bool)method!.Invoke(null, args)!;
        Assert.IsTrue(success, args[2] as string ?? "Bridge invocation failed.");

        Assert.IsNotNull(args[1], "Bridge carrier result was null.");
        var carrier = args[1]!;
        var carrierType = carrier.GetType();
        var extractedDocumentPath = carrierType.GetProperty("DocumentPath")?.GetValue(carrier) as string;
        var extractedDocumentText = carrierType.GetProperty("DocumentText")?.GetValue(carrier) as string;
        var imports = carrierType.GetProperty("Imports")?.GetValue(carrier) as System.Collections.IEnumerable;

        Assert.AreEqual(documentPath, extractedDocumentPath);
        TestContext.WriteLine("Extracted document text length: " + (extractedDocumentText?.Length ?? -1));
        Assert.AreEqual(documentText, extractedDocumentText);
        Assert.IsNotNull(imports, "Bridge carrier imports were not readable.");
        Assert.AreEqual(1, imports!.Cast<object>().Count());
    }

    [TestMethod]
    public void RazorSourceGeneratorBridge_ProbeRazorSourceDocumentRuntimeSurface()
    {
        const string documentPath = @"D:\repo\Demo\Pages\Counter.razor";
        const string documentText =
            """
            @page "/counter"
            <h1>Hello</h1>
            """;

        var codeDocument = RazorIrTestHost.CreateCodeDocument(documentPath, documentText);
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
