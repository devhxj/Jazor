using System.Reflection;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class VueModuleBuilderPrivateContractTests
{
    [TestMethod]
    public void NamingHelpers_KeepSourceCandidatesAndReservedMethodRulesStable()
    {
        var compilation = CreateCompilation();
        var shapes = GetNamedType(compilation, "PrivateContracts.Shapes");
        var nested = GetNamedType(compilation, "PrivateContracts.Shapes+Nested");
        var autoProperty = GetProperty(shapes, "Auto");
        var ordinaryMethod = GetMethod(shapes, "Ordinary");
        var staticConstructor = shapes.StaticConstructors.Single();
        var finalizer = shapes.GetMembers().OfType<IMethodSymbol>().Single(method => method.MethodKind == MethodKind.Destructor);
        var backingField = shapes.GetMembers().OfType<IFieldSymbol>()
            .Single(field => SymbolEqualityComparer.Default.Equals(field.AssociatedSymbol, autoProperty));
        var normalField = shapes.GetMembers("Field").OfType<IFieldSymbol>().Single();

        Assert.AreEqual("Field", Invoke<string>("GetSourceDeclaredNameCandidate", normalField));
        Assert.IsNull(Invoke<string?>("GetSourceDeclaredNameCandidate", backingField));
        Assert.AreEqual("Auto", Invoke<string>("GetSourceDeclaredNameCandidate", autoProperty.GetMethod!));
        Assert.AreEqual("Ordinary", Invoke<string>("GetSourceDeclaredNameCandidate", ordinaryMethod));
        Assert.AreEqual("Nested", Invoke<string>("GetSourceDeclaredNameCandidate", nested));
        Assert.AreEqual(
            "PrivateContracts",
            Invoke<string>(
                "GetSourceDeclaredNameCandidate",
                compilation.GlobalNamespace.GetNamespaceMembers().Single(@namespace => @namespace.Name == "PrivateContracts")));

        Assert.IsFalse(Invoke<bool>("ShouldReserveModuleMethodName", staticConstructor));
        Assert.IsFalse(Invoke<bool>("ShouldReserveModuleMethodName", autoProperty.SetMethod!));
        Assert.IsTrue(Invoke<bool>("ShouldReserveModuleMethodName", ordinaryMethod));
        Assert.IsFalse(Invoke<bool>("ShouldReserveModuleMethodName", finalizer));
    }

    [TestMethod]
    public void NamingAndPathHelpers_NormalizeIdentifierAndModuleShapesDeterministically()
    {
        var compilation = CreateCompilation();
        var marked = GetNamedType(compilation, "PrivateContracts.Marked");
        var blank = GetNamedType(compilation, "PrivateContracts.Blank");
        var plain = GetNamedType(compilation, "PrivateContracts.Plain");
        var record = GetNamedType(compilation, "PrivateContracts.RecordShape");
        var value = GetNamedType(compilation, "PrivateContracts.ValueShape");

        Assert.AreEqual("fallback", Invoke<string>("SanitizeJavaScriptIdentifierPart", string.Empty, "fallback"));
        Assert.AreEqual("fallback9_value", Invoke<string>("SanitizeJavaScriptIdentifierPart", "9-value", "fallback"));
        Assert.AreEqual("ready$value", Invoke<string>("SanitizeJavaScriptIdentifierPart", "ready$value", "fallback"));

        Assert.AreEqual("components/marked.mjs", Invoke<string>("GetRelativePath", marked));
        Assert.AreEqual("RazorVue.PrivateContracts/PrivateContracts/Blank.mjs", Invoke<string>("GetRelativePath", blank));
        Assert.AreEqual("RazorVue.PrivateContracts/PrivateContracts/Plain.mjs", Invoke<string>("GetRelativePath", plain));

        Assert.IsTrue(Invoke<bool>("IsRuntimeMemberClass", plain));
        Assert.IsFalse(Invoke<bool>("IsRuntimeMemberClass", record));
        Assert.IsFalse(Invoke<bool>("IsRuntimeMemberClass", value));
    }

    private static T Invoke<T>(string methodName, params object?[] arguments)
    {
        var method = typeof(VueModuleBuilder)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length);
        return (T)method.Invoke(null, arguments)!;
    }

    private static CSharpCompilation CreateCompilation()
    {
        var source = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;

            namespace PrivateContracts;

            public sealed class Shapes
            {
                private static readonly int Seed = 1;
                public int Field;
                public int Auto { get; init; }
                public static void Ordinary() { }
                ~Shapes() { }
                public sealed class Nested { }
            }

            [ECMAScriptModule("./components/marked")]
            public sealed class Marked { }

            [ECMAScriptModule("")]
            public sealed class Blank { }

            public sealed class Plain { }
            public sealed record RecordShape;
            public readonly struct ValueShape;
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "PrivateContracts.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.PrivateContracts",
            [source],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        return compilation;
    }

    private static INamedTypeSymbol GetNamedType(Compilation compilation, string metadataName)
    {
        var type = compilation.GetTypeByMetadataName(metadataName);
        Assert.IsNotNull(type, metadataName);
        return type!;
    }

    private static IPropertySymbol GetProperty(INamedTypeSymbol type, string name)
        => type.GetMembers(name).OfType<IPropertySymbol>().Single();

    private static IMethodSymbol GetMethod(INamedTypeSymbol type, string name)
        => type.GetMembers(name).OfType<IMethodSymbol>().Single();
}
