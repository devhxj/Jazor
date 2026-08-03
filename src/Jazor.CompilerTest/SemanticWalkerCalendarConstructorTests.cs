using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerCalendarConstructorTests
{
    [TestMethod]
    public void Visit_GregorianCalendarConstructorOverloads_UseDateRuntimeImports()
    {
        var block = GetBlockOperation(
            """
            using System;
            using System.Globalization;

            public static class CalendarConstructorScenarios
            {
                public static DateTimeOffset Evaluate(Calendar calendar, TimeSpan offset)
                {
                    var date = new DateTime(2024, 2, 29, calendar);
                    date = new DateTime(2024, 2, 29, 3, 4, 5, calendar);
                    date = new DateTime(2024, 2, 29, 3, 4, 5, 6, calendar);
                    date = new DateTime(2024, 2, 29, 3, 4, 5, 6, calendar, DateTimeKind.Utc);
                    date = new DateTime(2024, 2, 29, 3, 4, 5, 6, 7, calendar);
                    date = new DateTime(2024, 2, 29, 3, 4, 5, 6, 7, calendar, DateTimeKind.Local);
                    _ = date;
                    _ = new DateTimeOffset(2024, 2, 29, 3, 4, 5, 6, calendar, offset);
                    return new DateTimeOffset(2024, 2, 29, 3, 4, 5, 6, 7, calendar, offset);
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(2, imports, body);
        Assert.HasCount(6, imports["System/DateTimeModule.js"], body);
        Assert.HasCount(2, imports["System/DateTimeOffsetModule.js"], body);
        StringAssert.Contains(body, "_a515b8bb82ad96b7(2024, 2, 29, calendar)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_29bb943b21806bd9(2024, 2, 29, 3, 4, 5, calendar)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_8a4d2d51b716bb36(2024, 2, 29, 3, 4, 5, 6, calendar)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_bd2c430e6327a2cc(2024, 2, 29, 3, 4, 5, 6, calendar, 1)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_bd13792ce57e1964(2024, 2, 29, 3, 4, 5, 6, 7, calendar)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_cd0b8f2bce1e09ed(2024, 2, 29, 3, 4, 5, 6, 7, calendar, 2)", StringComparison.Ordinal);
        StringAssert.Contains(body, "_61ea80919619bab9(2024, 2, 29, 3, 4, 5, 6, calendar, offset)", StringComparison.Ordinal);
        StringAssert.Contains(body, "return _d027561c1f6af451(2024, 2, 29, 3, 4, 5, 6, 7, calendar, offset);", StringComparison.Ordinal);

        _ = new Parser().ParseScript("function verify(calendar, offset) " + body);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "CalendarConstructorScenarios",
            [syntaxTree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors.Select(static error => error.ToString())));

        var method = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static candidate => candidate.Identifier.ValueText == "Evaluate");
        return Assert.IsInstanceOfType<IBlockOperation>(compilation.GetSemanticModel(syntaxTree).GetOperation(method.Body!));
    }
}
