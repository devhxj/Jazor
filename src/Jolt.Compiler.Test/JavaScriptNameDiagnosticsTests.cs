namespace Jolt.Compiler.Test;

[TestClass]
public sealed class JavaScriptNameDiagnosticsTests
{
    [DataRow("Description", "\"@#\"")]
    [DataRow("ECMAScriptName", "\"\"")]
    [TestMethod]
    public void Analyze_EmptyJavaScriptName_MapsToAuthoredAttribute(string attribute, string argument)
    {
        string attributeText = attribute + "(" + argument + ")";
        string source = $$"""
            @using System.ComponentModel
            @using ECMAScript
            <div />
            @code {
                [{{attributeText}}]
                private string Caption => "caption";
            }
            """;
        string sharedSource = $$"""
            using System.ComponentModel;
            using ECMAScript;

            [{{attributeText}}]
            public static class Shared { }
            """;
        var options = new JoltCompileOptions { CSharpSources = [new("Shared.cs", sharedSource)] };

        var result = JoltProjectCompiler.Analyze([new("App.jazor", source)], options);
        var errors = result.Diagnostics.Where(diagnostic => diagnostic.Id == "JAZOR009")
            .ToDictionary(diagnostic => diagnostic.Location.GetLineSpan().Path, StringComparer.Ordinal);

        Assert.HasCount(2, errors, string.Join(Environment.NewLine, result.Diagnostics));
        foreach (var (path, text) in new[] { ("App.jazor", source), ("Shared.cs", sharedSource) })
        {
            var span = errors[path].Location.SourceSpan;
            int attributeStart = text.IndexOf(attributeText, StringComparison.Ordinal);
            Assert.IsGreaterThanOrEqualTo(attributeStart, span.Start);
            Assert.IsLessThanOrEqualTo(attributeStart + attributeText.Length, span.End);
        }
    }
}
