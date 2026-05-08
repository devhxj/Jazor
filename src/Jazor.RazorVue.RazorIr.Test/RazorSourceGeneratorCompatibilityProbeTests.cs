using Jazor.Analyzer.RazorVue.Generation;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorSourceGeneratorCompatibilityProbeTests
{
    [TestMethod]
    public void CollectCurrent_ReturnsExpectedRazorSourceGeneratorShape()
    {
        var result = RazorSourceGeneratorCompatibilityProbe.CollectCurrent();

        Assert.IsTrue(result.Success, result.Failure ?? "The Razor source generator compatibility probe did not succeed.");
        Assert.IsNotNull(result.Shape, "The compatibility probe did not return a shape snapshot.");

        var shape = result.Shape!;
        Assert.IsFalse(string.IsNullOrWhiteSpace(shape.AssemblyPath), "Assembly path was empty.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(shape.AssemblyVersion), "Assembly version was empty.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(shape.ModuleVersionId), "ModuleVersionId was empty.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(shape.InitializeMethodIlSha256), "Initialize IL SHA-256 was empty.");
        Assert.IsTrue(shape.InitializeMethodIlLength > 0, "Initialize method IL length must be positive.");
        Assert.AreEqual("Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator", shape.TypeFullName);
        Assert.AreEqual("Initialize", shape.InitializeMethodName);
        Assert.AreEqual("Microsoft.CodeAnalysis.IncrementalGeneratorInitializationContext", shape.InitializeContextParameterType);
        Assert.IsTrue(shape.ImplementsIncrementalGenerator, "The Razor source generator no longer implements IIncrementalGenerator.");
        var fileSha256 = RazorIrTestHost.ComputeFileSha256(shape.AssemblyPath);
        Assert.IsFalse(string.IsNullOrWhiteSpace(fileSha256), "File SHA-256 was empty.");

        TestContext.WriteLine("Assembly path: " + shape.AssemblyPath);
        TestContext.WriteLine("Assembly version: " + shape.AssemblyVersion);
        TestContext.WriteLine("ModuleVersionId: " + shape.ModuleVersionId);
        TestContext.WriteLine("File SHA-256: " + fileSha256);
        TestContext.WriteLine("Initialize IL length: " + shape.InitializeMethodIlLength);
        TestContext.WriteLine("Initialize IL SHA-256: " + shape.InitializeMethodIlSha256);
        TestContext.WriteLine("Declared methods:");
        foreach (var methodName in shape.DeclaredMethodNames)
        {
            TestContext.WriteLine("  " + methodName);
        }
    }

    public TestContext TestContext { get; set; } = default!;
}
