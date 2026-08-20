using Acornima;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerUriMemberTests
{
    [TestMethod]
    public void Visit_UriMembers_LowerToBrowserUrlCarrier()
    {
        var block = GetBlockOperation(
            """
            using System;

            public static class UriMemberScenarios
            {
                public static string Evaluate(string baseUri, string relative)
                {
                    var root = new Uri(baseUri);
                    var target = new Uri(root, relative);
                    var port = target.Port;
                    var text = root.AbsoluteUri + "|" + target.ToString() + "|" + target.AbsolutePath +
                        "|" + target.Query + "|" + target.Fragment + "|" + target.Host + "|" +
                        target.Authority + "|" + target.Scheme + "|" + target.PathAndQuery;
                    return port > 0 ? text : text + "|none";
                }
            }
            """);

        var argument = new SenseArgument(UseImportAliases: true);
        var body = new SemanticWalker(true).Visit(block, argument)?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

        Assert.IsNotNull(body);

        // Only PathAndQuery and Port need helpers; every other member is an alias or a short inline
        // template, so the module contributes exactly two import specifiers.
        var imports = argument.FlushImportSpecifiers().ToDictionary(static pair => pair.Key, static pair => pair.Value);
        Assert.HasCount(1, imports, body);
        Assert.HasCount(2, imports["System/UriModule.js"], body);

        StringAssert.Contains(body, "new URL(baseUri)", StringComparison.Ordinal);
        StringAssert.Contains(body, "new URL(relative, root.href)", StringComparison.Ordinal);
        StringAssert.Contains(body, "root.href", StringComparison.Ordinal);
        StringAssert.Contains(body, "target.href", StringComparison.Ordinal);
        StringAssert.Contains(body, "target.pathname", StringComparison.Ordinal);
        StringAssert.Contains(body, "target.search", StringComparison.Ordinal);
        StringAssert.Contains(body, "target.hash", StringComparison.Ordinal);
        // Uri.Host drops the port while Uri.Authority keeps it, matching hostname vs host.
        StringAssert.Contains(body, "target.hostname", StringComparison.Ordinal);
        StringAssert.Contains(body, "target.host +", StringComparison.Ordinal);
        StringAssert.Contains(body, "target.protocol.slice(0, -1)", StringComparison.Ordinal);
        StringAssert.Contains(body, "getPathAndQuery(target)", StringComparison.Ordinal);
        StringAssert.Contains(body, "getPort(target)", StringComparison.Ordinal);

        _ = new Parser().ParseScript("function verify(baseUri, relative) " + body);
    }

    private static IBlockOperation GetBlockOperation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create(
            "UriMemberScenarios",
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
