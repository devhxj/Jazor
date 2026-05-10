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
    public void Collect_ForInitializeWithWrongContextParameter_FailsBeforeValidation()
    {
        var assembly = CreateRazorCompilerAssemblyWithPublicInitialize(typeof(object));

        var result = RazorSourceGeneratorCompatibilityProbe.Collect(assembly);

        Assert.IsFalse(result.Success, "Razor SG probing must match Initialize(IncrementalGeneratorInitializationContext), not only the Initialize method name.");
        StringAssert.Contains(result.Failure ?? string.Empty, "Initialize(IncrementalGeneratorInitializationContext) was not found");
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

    [TestMethod]
    public void Validate_ForWrongGeneratorTypeName_Fails()
    {
        var shape = CreateSupportedShape() with
        {
            TypeFullName = "Microsoft.NET.Sdk.Razor.SourceGenerators.RenamedRazorSourceGenerator"
        };

        var validation = RazorSourceGeneratorCompatibilityGuard.Validate(shape);

        Assert.IsFalse(validation.Success, "Razor SG patching must be gated by the exact SDK generator type.");
        StringAssert.Contains(validation.Failure ?? string.Empty, "Razor source generator type mismatch");
    }

    [TestMethod]
    public void Validate_ForNonIncrementalGeneratorShape_Fails()
    {
        var shape = CreateSupportedShape() with
        {
            ImplementsIncrementalGenerator = false
        };

        var validation = RazorSourceGeneratorCompatibilityGuard.Validate(shape);

        Assert.IsFalse(validation.Success, "Razor SG patching must reject generators that no longer implement IIncrementalGenerator.");
        StringAssert.Contains(validation.Failure ?? string.Empty, "no longer implements Microsoft.CodeAnalysis.IIncrementalGenerator");
    }

    [TestMethod]
    public void Validate_ForInitializeParameterMismatch_Fails()
    {
        var shape = CreateSupportedShape() with
        {
            InitializeContextParameterType = "Microsoft.CodeAnalysis.GeneratorInitializationContext"
        };

        var validation = RazorSourceGeneratorCompatibilityGuard.Validate(shape);

        Assert.IsFalse(validation.Success, "Razor SG patching must be gated by Initialize(IncrementalGeneratorInitializationContext).");
        StringAssert.Contains(validation.Failure ?? string.Empty, "Initialize parameter mismatch");
    }

    [TestMethod]
    public void Validate_ForInitializeReturnTypeMismatch_Fails()
    {
        var shape = CreateSupportedShape() with
        {
            InitializeMethodReturnType = "System.Boolean"
        };

        var validation = RazorSourceGeneratorCompatibilityGuard.Validate(shape);

        Assert.IsFalse(validation.Success, "Razor SG patching must require Initialize to keep returning void.");
        StringAssert.Contains(validation.Failure ?? string.Empty, "Initialize return type mismatch");
    }

    [TestMethod]
    public void Validate_ForNonPublicInitialize_Fails()
    {
        var shape = CreateSupportedShape() with
        {
            InitializeMethodIsPublic = false
        };

        var validation = RazorSourceGeneratorCompatibilityGuard.Validate(shape);

        Assert.IsFalse(validation.Success, "Razor SG patching must reject a non-public Initialize entry point.");
        StringAssert.Contains(validation.Failure ?? string.Empty, "Initialize is no longer public");
    }

    [TestMethod]
    public void Validate_ForStaticInitialize_Fails()
    {
        var shape = CreateSupportedShape() with
        {
            InitializeMethodIsStatic = true
        };

        var validation = RazorSourceGeneratorCompatibilityGuard.Validate(shape);

        Assert.IsFalse(validation.Success, "Razor SG patching must reject a static Initialize entry point.");
        StringAssert.Contains(validation.Failure ?? string.Empty, "Initialize is static");
    }

    [TestMethod]
    public void Validate_ForMissingDeclaredInitializeSurface_Fails()
    {
        var shape = CreateSupportedShape() with
        {
            DeclaredMethodNames = ["InitializeCore"]
        };

        var validation = RazorSourceGeneratorCompatibilityGuard.Validate(shape);

        Assert.IsFalse(validation.Success, "Razor SG patching must require the declared public Initialize surface to remain present.");
        StringAssert.Contains(validation.Failure ?? string.Empty, "Declared method surface no longer contains Initialize");
    }

    public TestContext TestContext { get; set; } = default!;

    private static RazorSourceGeneratorCompatibilityShape CreateSupportedShape()
        => new(
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
            InitializeMethodIlSha256: "diagnostic-only",
            DeclaredMethodNames: ["Initialize"]);

    private static Assembly CreateUnsupportedRazorCompilerAssembly()
        => CreateRazorCompilerAssemblyWithPublicInitialize(
            typeof(IncrementalGeneratorInitializationContext));

    private static Assembly CreateRazorCompilerAssemblyWithPublicInitialize(Type parameterType)
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
            [parameterType]);
        initialize.GetILGenerator().Emit(OpCodes.Ret);
        return typeBuilder.CreateTypeInfo()!.Assembly;
    }
}
