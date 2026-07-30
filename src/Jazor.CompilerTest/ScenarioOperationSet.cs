using ECMAScript;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

internal sealed class ScenarioOperationSet
{
    private readonly IReadOnlyDictionary<string, IBlockOperation> _blocks;

    private ScenarioOperationSet(IReadOnlyDictionary<string, IBlockOperation> blocks)
        => _blocks = blocks;

    public IBlockOperation GetBlock(string scenarioId)
        => _blocks.TryGetValue(scenarioId, out var block)
            ? block
            : throw new InvalidOperationException($"Scenario block '{scenarioId}' was not available.");

    public static ScenarioOperationSet Create(
        string assemblyName,
        string className,
        IReadOnlyList<ScenarioOperationSource> scenarios)
    {
        var source = BuildSource(className, scenarios);
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: [syntaxTree],
            references: TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var model = compilation.GetSemanticModel(syntaxTree);
        var methods = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(static declaration => declaration.Identifier.ValueText.StartsWith("Scenario", StringComparison.Ordinal))
            .ToDictionary(static declaration => declaration.Identifier.ValueText, StringComparer.Ordinal);
        var blocks = new Dictionary<string, IBlockOperation>(scenarios.Count, StringComparer.Ordinal);
        foreach (var scenario in scenarios)
        {
            if (!methods.TryGetValue(scenario.MethodName, out var method))
                throw new InvalidOperationException($"Scenario method '{scenario.MethodName}' was not available.");

            blocks.Add(
                scenario.Id,
                Assert.IsInstanceOfType<IBlockOperation>(model.GetOperation(method.Body!)));
        }

        return new ScenarioOperationSet(blocks);
    }

    private static string BuildSource(string className, IReadOnlyList<ScenarioOperationSource> scenarios)
    {
        var methods = string.Join(
            Environment.NewLine,
            scenarios.Select(static scenario => $$"""
                    public void {{scenario.MethodName}}()
                    {
                {{scenario.Body}}
                    }
                """));
        return $$"""
            using System;
            using System.Collections.Generic;
            using ECMAScript;
            using static ECMAScript.Global;

            public sealed class {{className}}
            {
                private static void Consume<T>(T value) { }

            {{methods}}
            }
            """;
    }
}

internal sealed record ScenarioOperationSource(string Id, string MethodName, string Body);
