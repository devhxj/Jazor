using System.Reflection;
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

        var result = InvokeStringLookup(
            typeof(SemanticWalker).Assembly.GetType("Jazor.Compiler.WhiteListLookup")
            ?? throw new InvalidOperationException("Cannot locate Jazor.Compiler.WhiteListLookup."),
            "TryGetValue",
            mappings,
            lookupKey,
            out var matchedKey,
            out var matchedValue);

        Assert.IsTrue(result);
        Assert.AreEqual(candidateKey, matchedKey);
        Assert.AreEqual("allowed", matchedValue);

        mappings[candidateKey] = "updated";
        Assert.IsTrue(InvokeStringLookup(
            typeof(SemanticWalker).Assembly.GetType("Jazor.Compiler.WhiteListLookup")
            ?? throw new InvalidOperationException("Cannot locate Jazor.Compiler.WhiteListLookup."),
            "TryGetValue",
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
        var lookupType = typeof(SemanticWalker).Assembly.GetType("Jazor.Compiler.WhiteListLookup")
            ?? throw new InvalidOperationException("Cannot locate Jazor.Compiler.WhiteListLookup.");

        Assert.IsTrue(InvokeStringLookup(
            lookupType,
            "TryGetValue",
            firstMappings,
            lookupKey,
            out var firstMatchedKey,
            out var firstMatchedValue));
        Assert.IsTrue(InvokeStringLookup(
            lookupType,
            "TryGetValue",
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

        var result = InvokeStringLookup(
            typeof(SemanticWalker).Assembly.GetType("Jazor.Compiler.WhiteListLookup")
            ?? throw new InvalidOperationException("Cannot locate Jazor.Compiler.WhiteListLookup."),
            "TryGetValue",
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

        var result = InvokeStringLookup(
            typeof(SemanticWalker).Assembly.GetType("Jazor.Compiler.WhiteListLookup")
            ?? throw new InvalidOperationException("Cannot locate Jazor.Compiler.WhiteListLookup."),
            "TryGetValue",
            mappings,
            lookupKey,
            out var matchedKey,
            out var matchedValue);

        Assert.IsFalse(result, $"Unexpected whitelist match: key={matchedKey}, value={matchedValue}");
    }

    [TestMethod]
    public void SemanticWalkerLookup_GenericParameterNormalization_DoesNotRewriteQualifiedConcreteTypeNames()
    {
        const string candidateKey = "LookupTests.Host<T>.Use(LookupTests.Types.T)";
        const string lookupKey = "LookupTests.Host<U>.Use(LookupTests.Types.U)";
        var mappings = new Dictionary<string, string>
        {
            [candidateKey] = "allowed"
        };

        var result = InvokeStringLookup(
            typeof(SemanticWalker),
            "TryGetWhiteListValue",
            mappings,
            lookupKey,
            out var matchedKey,
            out var matchedValue);

        Assert.IsFalse(result, $"Unexpected whitelist match: key={matchedKey}, value={matchedValue}");
    }

    private static bool InvokeStringLookup(
        Type lookupType,
        string methodName,
        Dictionary<string, string> mappings,
        string lookupKey,
        out string? matchedKey,
        out string? matchedValue)
    {
        var method = lookupType
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            .Single(candidate =>
            {
                if (candidate.Name != methodName || !candidate.IsGenericMethodDefinition)
                    return false;

                var parameters = candidate.GetParameters();
                return parameters.Length == 4 &&
                    parameters[0].ParameterType.IsGenericType &&
                    parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
                    parameters[1].ParameterType == typeof(string);
            })
            .MakeGenericMethod(typeof(string));

        var args = new object?[] { mappings, lookupKey, null, null };
        var result = (bool)(method.Invoke(null, args)
            ?? throw new InvalidOperationException($"Lookup method {lookupType.FullName}.{methodName} returned null."));
        matchedKey = args[2] as string;
        matchedValue = args[3] as string;
        return result;
    }
}
