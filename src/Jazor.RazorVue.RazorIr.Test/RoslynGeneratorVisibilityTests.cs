using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RoslynGeneratorVisibilityTests
{
    [TestMethod]
    public void GeneratorDriver_SameRun_ConsumerCompilation_DoesNotSeeProducerPartialAttribute()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CreateCompilation(parseOptions);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators:
            [
                new ProducerPartialAttributeGenerator().AsSourceGenerator(),
                new ConsumerObservationGenerator().AsSourceGenerator()
            ],
            parseOptions: parseOptions);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
        var runResult = driver.GetRunResult();
        var observationSource = GetGeneratedSource(runResult, "Consumer.Observation.g.cs");
        var outputSymbol = outputCompilation.GetTypeByMetadataName("Demo.Counter");
        var outputErrors = RazorIrTestHost.GetCompilationErrors(outputCompilation);

        Assert.AreEqual(0, diagnostics.Length, string.Join(Environment.NewLine, diagnostics.Select(static item => item.ToString())));
        Assert.AreEqual(0, outputErrors.Length, string.Join(Environment.NewLine, outputErrors));
        StringAssert.Contains(
            observationSource,
            "internal const bool CarrierVisibleInConsumerCompilation = false;",
            "The consumer generator unexpectedly observed the producer partial in the same driver run.");
        Assert.IsNotNull(outputSymbol, "The final output compilation did not contain Demo.Counter.");
        Assert.IsTrue(
            outputSymbol!.GetAttributes().Any(static attribute =>
                string.Equals(attribute.AttributeClass?.ToDisplayString(), "System.ObsoleteAttribute", StringComparison.Ordinal)),
            "The final output compilation should include the producer partial attribute, proving the visibility gap is between generator input and final merged output.");
    }

    [TestMethod]
    public void GeneratorDriver_SecondRun_ConsumerCompilation_SeesProducerPartialAttribute()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CreateCompilation(parseOptions);

        GeneratorDriver producerDriver = CSharpGeneratorDriver.Create(
            generators: [new ProducerPartialAttributeGenerator().AsSourceGenerator()],
            parseOptions: parseOptions);
        producerDriver = producerDriver.RunGeneratorsAndUpdateCompilation(compilation, out var producerOutputCompilation, out var producerDiagnostics);

        GeneratorDriver consumerDriver = CSharpGeneratorDriver.Create(
            generators: [new ConsumerObservationGenerator().AsSourceGenerator()],
            parseOptions: parseOptions);
        consumerDriver = consumerDriver.RunGeneratorsAndUpdateCompilation(producerOutputCompilation, out var consumerOutputCompilation, out var consumerDiagnostics);

        var observationSource = GetGeneratedSource(consumerDriver.GetRunResult(), "Consumer.Observation.g.cs");
        var consumerErrors = RazorIrTestHost.GetCompilationErrors(consumerOutputCompilation);

        Assert.AreEqual(0, producerDiagnostics.Length, string.Join(Environment.NewLine, producerDiagnostics.Select(static item => item.ToString())));
        Assert.AreEqual(0, consumerDiagnostics.Length, string.Join(Environment.NewLine, consumerDiagnostics.Select(static item => item.ToString())));
        Assert.AreEqual(0, consumerErrors.Length, string.Join(Environment.NewLine, consumerErrors));
        StringAssert.Contains(
            observationSource,
            "internal const bool CarrierVisibleInConsumerCompilation = true;",
            "Once the producer output is part of the next compilation input, the consumer generator should observe the partial attribute.");
    }

    private static CSharpCompilation CreateCompilation(CSharpParseOptions parseOptions)
        => CSharpCompilation.Create(
            assemblyName: "RazorVue.RoslynGeneratorVisibility",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
                    namespace Demo;

                    public partial class Counter
                    {
                    }
                    """,
                    options: parseOptions,
                    path: "Counter.cs")
            ],
            references: RazorIrTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static string GetGeneratedSource(GeneratorDriverRunResult runResult, string hintName)
    {
        foreach (var result in runResult.Results)
        {
            foreach (var source in result.GeneratedSources)
            {
                if (string.Equals(source.HintName, hintName, StringComparison.Ordinal))
                {
                    return source.SourceText.ToString();
                }
            }
        }

        Assert.Fail("Generated source was not found: " + hintName);
        return string.Empty;
    }

    [Generator]
    private sealed class ProducerPartialAttributeGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(
                context.CompilationProvider,
                static (outputContext, _) =>
                {
                    outputContext.AddSource(
                        "Producer.Carrier.g.cs",
                        SourceText.From(
                            """
                            namespace Demo;

                            [global::System.Obsolete("generated carrier")]
                            public partial class Counter
                            {
                            }
                            """,
                            Encoding.UTF8));
                });
        }
    }

    [Generator]
    private sealed class ConsumerObservationGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var carrierVisibility = context.CompilationProvider.Select(static (compilation, _) =>
            {
                var symbol = compilation.GetTypeByMetadataName("Demo.Counter");
                return symbol?.GetAttributes().Any(static attribute =>
                    string.Equals(attribute.AttributeClass?.ToDisplayString(), "System.ObsoleteAttribute", StringComparison.Ordinal)) == true;
            });

            context.RegisterSourceOutput(
                carrierVisibility,
                static (outputContext, isVisible) =>
                {
                    outputContext.AddSource(
                        "Consumer.Observation.g.cs",
                        SourceText.From(
                            """
                            namespace Demo;

                            internal static class ConsumerObservation
                            {
                                internal const bool CarrierVisibleInConsumerCompilation = PLACEHOLDER;
                            }
                            """.Replace("PLACEHOLDER", isVisible ? "true" : "false", StringComparison.Ordinal),
                            Encoding.UTF8));
                });
        }
    }
}
