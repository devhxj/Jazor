using Jazor.Analyzer.RazorVue.Generation;
using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;

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
        Assert.IsTrue(shape.InitializeMethodIlLength > 0, "Initialize method IL length must be positive.");
        Assert.AreEqual("Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator", shape.TypeFullName);
        Assert.AreEqual("Initialize", shape.InitializeMethodName);
        Assert.AreEqual("Microsoft.CodeAnalysis.IncrementalGeneratorInitializationContext", shape.InitializeContextParameterType);
        Assert.IsTrue(shape.ImplementsIncrementalGenerator, "The Razor source generator no longer implements IIncrementalGenerator.");
        Assert.AreEqual(RazorSourceGeneratorCompatibilityGuard.ExpectedInitializeMethodIlSha256, shape.InitializeMethodIlSha256);
        CollectionAssert.AreEqual(
            new[]
            {
                "ComputeRazorSourceGeneratorOptions",
                "Initialize"
            },
            shape.DeclaredMethodNames.ToArray());

        var validation = RazorSourceGeneratorCompatibilityGuard.Validate(result);
        Assert.IsTrue(validation.Success, validation.Failure ?? "The Razor source generator compatibility validation did not succeed.");

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

    [TestMethod]
    public void ValidateAssemblyForPatch_ForUnsupportedRazorSourceGeneratorShape_FailsBeforePatch()
    {
        var assembly = CreateUnsupportedRazorCompilerAssembly();

        var validation = RazorSourceGeneratorInitializeHookInstaller.ValidateAssemblyForPatch(assembly);

        Assert.IsFalse(validation.Success, "Unsupported Razor SG shape must not be accepted for tail injection patching.");
        StringAssert.Contains(validation.Failure ?? string.Empty, "Initialize(...) IL SHA-256 mismatch");
    }

    public TestContext TestContext { get; set; } = default!;

    private static Assembly CreateUnsupportedRazorCompilerAssembly()
    {
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName("Microsoft.CodeAnalysis.Razor.Compiler"),
            AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("FakeRazorCompiler");
        var typeBuilder = moduleBuilder.DefineType(
            "Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator",
            TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class);
        var initialize = typeBuilder.DefineMethod(
            "Initialize",
            MethodAttributes.Public,
            typeof(void),
            [typeof(IncrementalGeneratorInitializationContext)]);
        initialize.GetILGenerator().Emit(OpCodes.Ret);
        return typeBuilder.CreateTypeInfo()!.Assembly;
    }
}
