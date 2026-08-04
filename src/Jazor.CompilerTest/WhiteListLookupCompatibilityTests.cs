using Jazor.Common;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class WhiteListLookupCompatibilityTests
{
    [TestMethod]
    public void WhiteListLookup_PartialMemberImplementations_UseDefinitionKeys()
    {
        const string source = """
            partial class Host
            {
                public partial int Value { get; }
                public partial int this[int index] { get; }
                partial void Apply(int value);
            }

            partial class Host
            {
                public partial int Value => 1;
                public partial int this[int index] => index;
                partial void Apply(int value) { }
            }
            """;
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: "partial-members.cs");
        var compilation = CSharpCompilation.Create(
            "WhiteListLookup.PartialMembers",
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var model = compilation.GetSemanticModel(syntaxTree);
        var implementationMembers = syntaxTree.GetRoot().DescendantNodes()
            .Select(node => node switch
            {
                PropertyDeclarationSyntax property => (ISymbol?)model.GetDeclaredSymbol(property),
                IndexerDeclarationSyntax indexer => (ISymbol?)model.GetDeclaredSymbol(indexer),
                MethodDeclarationSyntax method => (ISymbol?)model.GetDeclaredSymbol(method),
                _ => null
            })
            .Where(static symbol => symbol is IMethodSymbol { PartialDefinitionPart: not null } or
                IPropertySymbol { PartialDefinitionPart: not null })
            .Cast<ISymbol>()
            .ToArray();
        Assert.HasCount(3, implementationMembers);

        foreach (var implementation in implementationMembers)
        {
            ISymbol definition = implementation switch
            {
                IMethodSymbol method => method.PartialDefinitionPart!,
                IPropertySymbol property => property.PartialDefinitionPart!,
                _ => throw new AssertFailedException($"Unexpected partial member kind '{implementation.Kind}'.")
            };
            var definitionKey = definition.OriginalDefinition.ToDisplayString(Format.NameFormat);
            var mappings = new Dictionary<string, string> { [definitionKey] = "mapped" };

            Assert.IsTrue(WhiteListLookup.TryGetValue(
                mappings,
                implementation,
                out var matchedKey,
                out var matchedValue));
            Assert.AreEqual(definitionKey, matchedKey);
            Assert.AreEqual("mapped", matchedValue);
        }
    }

    [TestMethod]
    public void WhiteListLookup_GenericParameterNormalization_MatchesEquivalentDeclaredParameterNames()
    {
        const string candidateKey = "LookupTests.Host<T>.Use(T)";
        const string lookupKey = "LookupTests.Host<TValue>.Use(TValue)";
        var mappings = new Dictionary<string, string>
        {
            [candidateKey] = "allowed"
        };

        var result = WhiteListLookup.TryGetValue(
            mappings,
            lookupKey,
            out var matchedKey,
            out var matchedValue);

        Assert.IsTrue(result);
        Assert.AreEqual(candidateKey, matchedKey);
        Assert.AreEqual("allowed", matchedValue);

        mappings[candidateKey] = "updated";
        Assert.IsTrue(WhiteListLookup.TryGetValue(
            mappings,
            lookupKey,
            out matchedKey,
            out matchedValue));
        Assert.AreEqual(candidateKey, matchedKey);
        Assert.AreEqual("updated", matchedValue);
    }

    [TestMethod]
    public void WhiteListLookup_GenericParameterIndex_IsScopedToMapping()
    {
        const string lookupKey = "LookupTests.Host<TValue>.Use(TValue)";
        var firstMappings = new Dictionary<string, string>
        {
            ["LookupTests.Host<T>.Use(T)"] = "first"
        };
        var secondMappings = new Dictionary<string, string>
        {
            ["LookupTests.Host<TItem>.Use(TItem)"] = "second"
        };
        Assert.IsTrue(WhiteListLookup.TryGetValue(
            firstMappings,
            lookupKey,
            out var firstMatchedKey,
            out var firstMatchedValue));
        Assert.IsTrue(WhiteListLookup.TryGetValue(
            secondMappings,
            lookupKey,
            out var secondMatchedKey,
            out var secondMatchedValue));

        Assert.AreEqual("LookupTests.Host<T>.Use(T)", firstMatchedKey);
        Assert.AreEqual("first", firstMatchedValue);
        Assert.AreEqual("LookupTests.Host<TItem>.Use(TItem)", secondMatchedKey);
        Assert.AreEqual("second", secondMatchedValue);
    }

    [TestMethod]
    public void WhiteListLookup_GenericParameterIndex_PreservesFirstEquivalentMapping()
    {
        const string firstKey = "LookupTests.Host<T>.Use(T)";
        var mappings = new Dictionary<string, string>
        {
            [firstKey] = "first",
            ["LookupTests.Host<TItem>.Use(TItem)"] = "second"
        };

        var result = WhiteListLookup.TryGetValue(
            mappings,
            "LookupTests.Host<TValue>.Use(TValue)",
            out var matchedKey,
            out var matchedValue);

        Assert.IsTrue(result);
        Assert.AreEqual(firstKey, matchedKey);
        Assert.AreEqual("first", matchedValue);
    }

    [TestMethod]
    public void WhiteListLookup_GenericParameterNormalization_DoesNotRewriteQualifiedConcreteTypeNames()
    {
        const string candidateKey = "LookupTests.Host<T>.Use(LookupTests.Types.T)";
        const string lookupKey = "LookupTests.Host<U>.Use(LookupTests.Types.U)";
        var mappings = new Dictionary<string, string>
        {
            [candidateKey] = "allowed"
        };

        var result = WhiteListLookup.TryGetValue(
            mappings,
            lookupKey,
            out var matchedKey,
            out var matchedValue);

        Assert.IsFalse(result, $"Unexpected whitelist match: key={matchedKey}, value={matchedValue}");
    }

    [TestMethod]
    public void WhiteListLookup_SourceModifierSymbols_ResolveCanonicalConsumerKeys()
    {
        const string source = """
            namespace LookupTests;

            public readonly struct ReadonlyHost
            {
                public readonly int Value;
            }

            public static class ConstHost
            {
                public const int Value = 1;
            }

            public class VirtualHost
            {
                public virtual void Read() { }
            }

            public abstract class AbstractHost
            {
                public abstract void Read();
            }

            public interface IStaticHost
            {
                static abstract void Create();
                static virtual void Reset() { }
            }

            public static class ExternHost
            {
                public static extern void Read();
            }
            """;
        var compilation = CreateCompilation(source, "WhiteListLookup.SourceModifiers");
        var cases = new (ISymbol Symbol, string CanonicalKey)[]
        {
            (GetMember(compilation, "LookupTests.ReadonlyHost", "Value"), "LookupTests.ReadonlyHost.Value"),
            (GetMember(compilation, "LookupTests.ConstHost", "Value"), "static LookupTests.ConstHost.Value"),
            (GetMember(compilation, "LookupTests.VirtualHost", "Read"), "LookupTests.VirtualHost.Read()"),
            (GetMember(compilation, "LookupTests.AbstractHost", "Read"), "LookupTests.AbstractHost.Read()"),
            (GetMember(compilation, "LookupTests.IStaticHost", "Create"), "static LookupTests.IStaticHost.Create()"),
            (GetMember(compilation, "LookupTests.IStaticHost", "Reset"), "static LookupTests.IStaticHost.Reset()"),
            (GetMember(compilation, "LookupTests.ExternHost", "Read"), "static LookupTests.ExternHost.Read()")
        };

        foreach (var testCase in cases)
        {
            var mappings = new Dictionary<string, string> { [testCase.CanonicalKey] = "allowed" };

            Assert.IsTrue(WhiteListLookup.TryGetValue(
                mappings,
                testCase.Symbol,
                out var matchedKey,
                out var matchedValue),
                testCase.Symbol.ToDisplayString(Format.NameFormat));
            Assert.AreEqual(testCase.CanonicalKey, matchedKey);
            Assert.AreEqual("allowed", matchedValue);
        }
    }

    [TestMethod]
    public void WhiteListLookup_OverriddenMembers_ResolveBaseDefinitionKeys()
    {
        const string source = """
            using System;

            namespace LookupTests;

            public class BaseHost
            {
                public virtual int Value { get; }
                public virtual void Apply() { }
                public virtual event Action? Changed { add { } remove { } }
            }

            public sealed class DerivedHost : BaseHost
            {
                public override int Value => 1;
                public override void Apply() { }
                public override event Action? Changed { add { } remove { } }
            }
            """;
        var compilation = CreateCompilation(source, "WhiteListLookup.OverrideFallback");
        var baseType = compilation.GetTypeByMetadataName("LookupTests.BaseHost")!;
        var derivedType = compilation.GetTypeByMetadataName("LookupTests.DerivedHost")!;

        foreach (var memberName in new[] { "Value", "Apply", "Changed" })
        {
            var baseMember = baseType.GetMembers(memberName).Single();
            var derivedMember = derivedType.GetMembers(memberName).Single();
            var baseKey = baseMember.OriginalDefinition.ToDisplayString(Format.NameFormat);
            var mappings = new Dictionary<string, string> { [baseKey] = memberName };

            Assert.IsTrue(WhiteListLookup.TryGetValue(
                mappings,
                derivedMember,
                out var matchedKey,
                out var matchedValue),
                memberName);
            Assert.AreEqual(baseKey, matchedKey, memberName);
            Assert.AreEqual(memberName, matchedValue, memberName);
        }
    }

    [TestMethod]
    public void WhiteListLookup_ReducedExtensionInvocation_ResolvesStaticExtensionKey()
    {
        const string source = """
            namespace LookupTests;

            public static class TextExtensions
            {
                public static int Measure(this string value, in int offset)
                    => value.Length + offset;
            }

            public sealed class Consumer
            {
                public int Read(string value)
                {
                    var offset = 2;
                    return value.Measure(in offset);
                }
            }
            """;
        var compilation = CreateCompilation(source, "WhiteListLookup.ReducedExtension");
        var syntaxTree = compilation.SyntaxTrees.Single();
        var model = compilation.GetSemanticModel(syntaxTree);
        var invocation = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Single();
        var reducedMethod = Assert.IsInstanceOfType<IMethodSymbol>(model.GetSymbolInfo(invocation).Symbol);
        var extensionDefinition = reducedMethod.ReducedFrom!.OriginalDefinition;
        var staticKey = extensionDefinition.ToDisplayString(Format.StaticExtensionNameFormat);
        var mappings = new Dictionary<string, string> { [staticKey] = "measure" };

        Assert.IsTrue(WhiteListLookup.TryGetValue(
            mappings,
            reducedMethod,
            out var matchedKey,
            out var matchedValue));
        Assert.AreEqual(staticKey, matchedKey);
        Assert.AreEqual("measure", matchedValue);
    }

    [TestMethod]
    public void WhiteListLookup_FallbackSymbols_FollowRoslynDefinitionRelationships()
    {
        const string source = """
            using System;

            namespace LookupTests;

            public static class TextExtensions
            {
                public static int Measure(this string value) => value.Length;
            }

            public partial class PartialHost
            {
                partial void Apply(int value);
                public partial int Value { get; }
            }

            public partial class PartialHost
            {
                partial void Apply(int value) { }
                public partial int Value => 1;
            }

            public static class ReadOnlyArguments
            {
                public static int Read(in int value) => value;
            }

            public class BaseHost
            {
                public virtual int Value { get; }
                public virtual void Apply() { }
                public virtual event Action? Changed { add { } remove { } }
            }

            public sealed class DerivedHost : BaseHost
            {
                public override int Value => 1;
                public override void Apply() { }
                public override event Action? Changed { add { } remove { } }
            }

            public sealed class Consumer
            {
                public int Read(string value) => value.Measure();
                public void Plain() { }
            }
            """;
        var compilation = CreateCompilation(source, "WhiteListLookup.FallbackSymbols");
        var syntaxTree = compilation.SyntaxTrees.Single();
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var reducedInvocation = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Single();
        var reducedMethod = Assert.IsInstanceOfType<IMethodSymbol>(semanticModel.GetSymbolInfo(reducedInvocation).Symbol);
        var partialImplementationSyntax = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static method =>
                method.Identifier.ValueText == "Apply" &&
                method.Body is not null &&
                method.Parent is ClassDeclarationSyntax { Identifier.ValueText: "PartialHost" });
        var partialImplementation = Assert.IsInstanceOfType<IMethodSymbol>(
            semanticModel.GetDeclaredSymbol(partialImplementationSyntax));
        var derivedType = compilation.GetTypeByMetadataName("LookupTests.DerivedHost")!;
        var consumer = compilation.GetTypeByMetadataName("LookupTests.Consumer")!;

        Assert.AreEqual(
            reducedMethod.ReducedFrom!.OriginalDefinition,
            WhiteListLookup.GetFallbackSymbol(reducedMethod));
        Assert.AreEqual(
            partialImplementation.PartialDefinitionPart!.OriginalDefinition,
            WhiteListLookup.GetFallbackSymbol(partialImplementation));
        var partialPropertyDefinition = Assert.IsInstanceOfType<IPropertySymbol>(
            compilation.GetTypeByMetadataName("LookupTests.PartialHost")!
                .GetMembers("Value")
                .Single());
        var partialProperty = partialPropertyDefinition.PartialImplementationPart!;
        Assert.AreEqual(
            partialPropertyDefinition.OriginalDefinition,
            WhiteListLookup.GetFallbackSymbol(partialProperty));
        Assert.AreEqual(
            ((IMethodSymbol)derivedType.GetMembers("Apply").Single()).OverriddenMethod!.OriginalDefinition,
            WhiteListLookup.GetFallbackSymbol(derivedType.GetMembers("Apply").Single()));
        Assert.AreEqual(
            ((IPropertySymbol)derivedType.GetMembers("Value").Single()).OverriddenProperty!.OriginalDefinition,
            WhiteListLookup.GetFallbackSymbol(derivedType.GetMembers("Value").Single()));
        Assert.AreEqual(
            ((IEventSymbol)derivedType.GetMembers("Changed").Single()).OverriddenEvent!.OriginalDefinition,
            WhiteListLookup.GetFallbackSymbol(derivedType.GetMembers("Changed").Single()));
        Assert.IsNull(WhiteListLookup.GetFallbackSymbol(consumer.GetMembers("Plain").Single()));

        var inParameterMethod = compilation.GetTypeByMetadataName("LookupTests.ReadOnlyArguments")!
            .GetMembers("Read")
            .OfType<IMethodSymbol>()
            .Single();
        Assert.IsFalse(WhiteListLookup.TryGetValue(
            new Dictionary<string, string>(),
            inParameterMethod,
            out var missingDisplay,
            out _));
        StringAssert.Contains(missingDisplay, "in int", StringComparison.Ordinal);
    }

    private static CSharpCompilation CreateCompilation(string source, string assemblyName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            TestMetadataReferences.PreviewParseOptions,
            path: $"{assemblyName}.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName,
            [syntaxTree],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        return compilation;
    }

    private static ISymbol GetMember(CSharpCompilation compilation, string typeName, string memberName)
        => compilation.GetTypeByMetadataName(typeName)!.GetMembers(memberName).Single();
}
