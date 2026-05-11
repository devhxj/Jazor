using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class JazorAnalyzerSurfaceTests
{
    [TestMethod]
    public void AnalysisOperationKinds_MatchAnalysisOperationActionSwitchCases()
    {
        var analyzerSourcePath = FindRepositoryFile(Path.Combine("src", "Jazor.Analyzer", "Analyzer.cs"));
        var root = CSharpSyntaxTree.ParseText(File.ReadAllText(analyzerSourcePath)).GetRoot();
        var analyzerType = root
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Single(static declaration => declaration.Identifier.ValueText == "Analyzer");

        var registeredKinds = Jazor.Analyzer.Analyzer.AnalysisOperationKinds
            .Distinct()
            .OrderBy(static kind => (int)kind)
            .ToArray();

        var operationSwitch = analyzerType
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(static method => method.Identifier.ValueText == "AnalysisOperationAction")
            .DescendantNodes()
            .OfType<SwitchStatementSyntax>()
            .Single(static statement => statement.Expression.ToString() == "ctx.Operation.Kind");

        var handledKinds = operationSwitch
            .Sections
            .SelectMany(static section => section.Labels.OfType<CaseSwitchLabelSyntax>())
            .Select(static label => label.Value)
            .OfType<MemberAccessExpressionSyntax>()
            .Where(static memberAccess => memberAccess.Expression.ToString() == nameof(OperationKind))
            .Select(static memberAccess => memberAccess.Name.Identifier.ValueText)
            .Select(static name => Enum.Parse<OperationKind>(name))
            .Distinct()
            .OrderBy(static kind => (int)kind)
            .ToArray();

        AssertSameOperationKinds(
            expected: handledKinds,
            actual: registeredKinds,
            "Analyzer operation registration must stay aligned with AnalysisOperationAction switch cases.");
    }

    [TestMethod]
    public void AnalysisOperationKinds_HaveNoDuplicateRegistrations()
    {
        var duplicates = Jazor.Analyzer.Analyzer.AnalysisOperationKinds
            .GroupBy(static kind => kind)
            .Where(static group => group.Count() > 1)
            .Select(static group => group.Key.ToString())
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.AreEqual(0, duplicates.Length, "Duplicate analyzer operation registrations: " + string.Join(", ", duplicates));
    }

    private static string FindRepositoryFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository file: " + relativePath);
    }

    private static void AssertSameOperationKinds(
        OperationKind[] expected,
        OperationKind[] actual,
        string message)
    {
        var missing = expected.Except(actual).OrderBy(static kind => (int)kind).Select(static kind => kind.ToString()).ToArray();
        var unexpected = actual.Except(expected).OrderBy(static kind => (int)kind).Select(static kind => kind.ToString()).ToArray();

        Assert.IsTrue(
            missing.Length == 0 && unexpected.Length == 0,
            message +
            Environment.NewLine +
            "Missing registrations: " + string.Join(", ", missing) +
            Environment.NewLine +
            "Unexpected registrations: " + string.Join(", ", unexpected));
    }
}
