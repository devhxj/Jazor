using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class BindingAuthoringDocumentationTests
{
    private const string Source = """
        using System;
        namespace Demo;
        public class ParameterAttribute : Attribute { }
        public readonly union Scalar(double, string)
        {
            public double? AsNumber => Value is double value ? value : null;
            public string? AsString => Value as string;
        }
        public readonly union Nested(Scalar, Scalar[])
        {
            public Scalar? AsSingle => Value is Scalar value ? value : null;
            public Scalar[]? AsMultiple => Value as Scalar[];
        }
        public readonly union Third(Nested, bool)
        {
            public Nested? AsNested => Value is Nested value ? value : null;
        }
        public class UnrelatedName
        {
            /// <summary>Existing upstream description.</summary>
            [Parameter]
            public Scalar? Extent { get; set; }
            [Demo.Parameter]
            public Third? Selection { get; set; }
        }
        """;

    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public void Annotate_NewNamesAndThreeLevels_ProducesCompilableStableHintsAndProjections(string newline)
    {
        var source = Source.ReplaceLineEndings(newline);
        var documentation = new BindingAuthoringDocumentation([source]);
        var updated = documentation.Annotate(source);
        Assert.AreEqual(updated, documentation.Annotate(updated), "Regeneration must not add whitespace or depend on prior output.");
        Assert.AreEqual(updated, new BindingAuthoringDocumentation([updated]).Annotate(updated));
        StringAssert.Contains(updated, "public double? AsNumber => AsSingle?.AsNumber;");
        StringAssert.Contains(updated, "public double? AsNumber => AsNested?.AsSingle?.AsNumber;");
        StringAssert.Contains(updated, "Extent=\"@(32)\"");
        StringAssert.Contains(updated, "value?.AsNumber");
        StringAssert.Contains(updated, "Existing upstream description.");
        var tree = CSharpSyntaxTree.ParseText(updated, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create("GeneratedAuthoring", [tree], TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.IsEmpty(compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
    }

    [TestMethod]
    public void Annotate_NumberBranch_GeneratesOnlyMissingStronglyTypedNumericConversions()
    {
        const string source = """
            namespace ECMAScript
            {
                public readonly struct Number
                {
                    public static implicit operator Number(int value) => default;
                    public static implicit operator Number(double value) => default;
                }
                public readonly union Choice(Number, string)
                {
                    public Number? AsNumber => Value is Number value ? value : null;
                }
                public class Consumer
                {
                    public Choice Literal = 32;
                    public Choice? Nullable = 32;
                    public Choice Variable(int value) => value;
                    public Choice Fraction = 1.5;
                }
            }
            """;
        var documentation = new BindingAuthoringDocumentation([source]);
        var updated = documentation.Annotate(source);
        StringAssert.Contains(updated, "implicit operator Choice(int value)");
        Assert.AreEqual(updated, documentation.Annotate(updated));
        var tree = CSharpSyntaxTree.ParseText(updated, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create("NumericBridge", [tree], TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.IsEmpty(compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
    }

    [TestMethod]
    public void Describe_QualifiedDuplicateNamesAndClosedGenerics_UsesActualDeclaredDomain()
    {
        var documentation = new BindingAuthoringDocumentation([
            "namespace First; public readonly union Choice(double,string);",
            "namespace Second; public readonly union Choice(bool,string);",
            "namespace Generic; public readonly union Choice<T>(T,T[]);"
        ]);
        Assert.IsNull(documentation.Describe("Value", "Choice?"), "Ambiguous unqualified names must not get a guessed type hint.");
        StringAssert.Contains(documentation.Describe("Value", "First.Choice?")!, "double | string");
        Assert.IsFalse(documentation.Describe("Value", "Second.Choice?")!.Contains("@(32)", StringComparison.Ordinal));
        StringAssert.Contains(documentation.Describe("Value", "Generic.Choice<string>?")!, "string | string[]");
        StringAssert.Contains(documentation.Describe("Changed", "Microsoft.AspNetCore.Components.EventCallback<First.Choice?>")!, "C# 回调参数为 First.Choice?");
        StringAssert.Contains(documentation.Describe("Changed", "Microsoft.AspNetCore.Components.EventCallback<Generic.Choice<string>?>")!, "C# 回调参数为 Generic.Choice<string>?");
    }

    [TestMethod]
    public void Annotate_TwoScalarPathsWithSameName_PreservesExplicitPaths()
    {
        var source = Source + """

            public readonly union Another(double, bool)
            {
                public double? AsNumber => Value is double value ? value : null;
            }
            public readonly union Ambiguous(Scalar, Another)
            {
                public Scalar? AsFirst => Value is Scalar value ? value : null;
                public Another? AsSecond => Value is Another value ? value : null;
            }
            """;
        var updated = new BindingAuthoringDocumentation([source]).Annotate(source);
        var last = updated[updated.IndexOf("public readonly union Ambiguous", StringComparison.Ordinal)..];
        Assert.IsFalse(last.Contains("public double? AsNumber", StringComparison.Ordinal));
    }
}
