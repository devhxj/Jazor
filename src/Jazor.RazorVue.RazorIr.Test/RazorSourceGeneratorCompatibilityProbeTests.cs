using Jazor.Analyzer.RazorVue.Generation;
using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorSourceGeneratorCompatibilityProbeTests
{
    [TestMethod]
    public void CollectCurrent_ReturnsSupportedRazorSourceGeneratorAbi()
    {
        var result = RazorSourceGeneratorCompatibilityProbe.CollectCurrent();

        Assert.IsTrue(result.Success, result.Failure ?? "The Razor source generator compatibility probe did not succeed.");
        Assert.IsNotNull(result.Shape, "The compatibility probe did not return a shape snapshot.");

        var shape = result.Shape!;
        Assert.IsFalse(string.IsNullOrWhiteSpace(shape.AssemblyPath), "Assembly path was empty.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(shape.AssemblyVersion), "Assembly version was empty.");
        Assert.AreEqual("Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator", shape.TypeFullName);
        Assert.AreEqual("Initialize", shape.InitializeMethodName);
        Assert.AreEqual("Microsoft.CodeAnalysis.IncrementalGeneratorInitializationContext", shape.InitializeContextParameterType);
        Assert.AreEqual("System.Void", shape.InitializeMethodReturnType);
        Assert.IsTrue(shape.InitializeMethodIsPublic, "Initialize must remain public for RazorVue to integrate with the SDK generator entry point.");
        Assert.IsFalse(shape.InitializeMethodIsStatic, "Initialize must remain an instance method for RazorVue to integrate with the SDK generator entry point.");
        Assert.IsTrue(shape.ImplementsIncrementalGenerator, "The Razor source generator no longer implements IIncrementalGenerator.");
        Assert.IsTrue(shape.DeclaredMethodNames.Contains("Initialize"), "Declared methods must include Initialize.");
        if (shape.InitializeMethodIlFingerprintAvailable)
        {
            Assert.IsTrue(shape.InitializeMethodIlLength > 0, "Initialize method IL length must be positive when the fingerprint is available.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(shape.InitializeMethodIlSha256), "Initialize method IL SHA-256 must be present when the fingerprint is available.");
        }

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
    public void ValidateAssemblyForPatch_ForNonIncrementalGeneratorShape_FailsBeforePatch()
    {
        var assembly = CreateUnsupportedRazorCompilerAssembly();

        var validation = RazorSourceGeneratorInitializeHookInstaller.ValidateAssemblyForPatch(assembly);

        Assert.IsFalse(validation.Success, "Unsupported Razor SG shape must not be accepted for tail injection patching.");
        StringAssert.Contains(validation.Failure ?? string.Empty, "no longer implements Microsoft.CodeAnalysis.IIncrementalGenerator");
    }

    [TestMethod]
    public void Validate_ForSupportedAbiWithUnknownInitializeIlHash_Succeeds()
    {
        var shape = new RazorSourceGeneratorCompatibilityShape(
            AssemblyPath: "ignored",
            AssemblyVersion: "11.0.0.0",
            ModuleVersionId: "ignored",
            TypeFullName: RazorSourceGeneratorCompatibilityGuard.RazorSourceGeneratorTypeName,
            ImplementsIncrementalGenerator: true,
            InitializeMethodName: "Initialize",
            InitializeContextParameterType: RazorSourceGeneratorCompatibilityGuard.IncrementalGeneratorInitializationContextTypeName,
            InitializeMethodReturnType: RazorSourceGeneratorCompatibilityGuard.VoidTypeName,
            InitializeMethodIsPublic: true,
            InitializeMethodIsStatic: false,
            InitializeMethodIlLength: 1,
            InitializeMethodIlSha256: "UNKNOWN_PREVIEW_HASH",
            DeclaredMethodNames: ["Initialize"]);

        var validation = RazorSourceGeneratorCompatibilityGuard.Validate(shape);

        Assert.IsTrue(validation.Success, validation.Failure ?? "Supported Razor SG ABI must not be rejected because the method body hash changed.");
    }

    [TestMethod]
    public void Validate_ForSupportedAbiWithUnavailableInitializeIlFingerprint_Succeeds()
    {
        var shape = new RazorSourceGeneratorCompatibilityShape(
            AssemblyPath: "ignored",
            AssemblyVersion: "11.0.0.0",
            ModuleVersionId: "ignored",
            TypeFullName: RazorSourceGeneratorCompatibilityGuard.RazorSourceGeneratorTypeName,
            ImplementsIncrementalGenerator: true,
            InitializeMethodName: "Initialize",
            InitializeContextParameterType: RazorSourceGeneratorCompatibilityGuard.IncrementalGeneratorInitializationContextTypeName,
            InitializeMethodReturnType: RazorSourceGeneratorCompatibilityGuard.VoidTypeName,
            InitializeMethodIsPublic: true,
            InitializeMethodIsStatic: false,
            InitializeMethodIlLength: 0,
            InitializeMethodIlSha256: string.Empty,
            DeclaredMethodNames: ["Initialize"]);

        var validation = RazorSourceGeneratorCompatibilityGuard.Validate(shape);

        Assert.IsTrue(validation.Success, validation.Failure ?? "Supported Razor SG ABI must not be rejected because the optional method body fingerprint is unavailable.");
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
