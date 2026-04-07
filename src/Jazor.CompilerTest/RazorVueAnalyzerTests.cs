using System.Collections.Immutable;
using Basic.Reference.Assemblies;
using Jazor.Analyzer;
using Jazor.Razor;
using Jazor.RazorVue;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class RazorVueAnalyzerTests
{
    [TestMethod]
    public async Task RazorVue_Entry_ValidVueComponent_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using Jazor.RazorVue;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class ValidComponent : VueComponent
            {
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE001", "JAZORVUE002", "JAZORVUE004", "JAZORVUE005", "JAZORVUE006", "JAZOR001");
    }

    [TestMethod]
    public async Task RazorVue_Entry_ComponentBaseOnly_ReportsJAZORVUE002()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : ComponentBase
            {
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE002");
        AssertNoDiagnostic(diagnostics, "JAZOR001");
    }

    [TestMethod]
    public async Task RazorVue_Entry_StaticModule_RemainsOnLegacyPath()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public static class InvalidModule
            {
                public static Version Value = new();
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZOR001");
        AssertNoDiagnostic(diagnostics, "JAZORVUE001", "JAZORVUE002", "JAZORVUE004", "JAZORVUE005", "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_StateHasChanged_ReportsJAZORVUE004()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using Jazor.RazorVue;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : VueComponent
            {
                public void Trigger()
                {
                    StateHasChanged();
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE004");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_ShouldRender_ReportsJAZORVUE005()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using Jazor.RazorVue;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : VueComponent
            {
                protected override bool ShouldRender()
                {
                    return true;
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE005");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_BaseOnlySetParametersAsync_IsAccepted()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : VueComponent
            {
                public override Task SetParametersAsync(ParameterView parameters)
                {
                    return base.SetParametersAsync(parameters);
                }
            }
            """);

        AssertNoDiagnostic(diagnostics, "JAZORVUE006");
    }

    [TestMethod]
    public async Task RazorVue_Misuse_NonTrivialSetParametersAsync_ReportsJAZORVUE006()
    {
        var diagnostics = await GetAnalyzerDiagnosticsAsync(
            """
            using System;
            using System.Threading.Tasks;
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components;

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                    public ECMAScriptModuleAttribute() { }
                    public ECMAScriptModuleAttribute(string import) { }
                }
            }

            [ECMAScript.ECMAScriptModule]
            public class InvalidComponent : VueComponent
            {
                [Parameter]
                public int Value { get; set; }

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    Value++;
                }
            }
            """);

        AssertHasDiagnostic(diagnostics, "JAZORVUE006");
    }

    private static async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(string source)
    {
        var compilation = CreateCompilation(source);
        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
            new Jazor.Analyzer.Analyzer(),
            new RazorVueEntryAnalyzer(),
            new RazorVueMisuseAnalyzer());

        var compileErrors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, compileErrors.Length, string.Join(Environment.NewLine, compileErrors.Select(static x => x.ToString())));

        return await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync();
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var references = Net100.References.All
            .Cast<MetadataReference>()
            .ToList();
        // Roslyn test compilations need both the Razor substrate assembly and the
        // Vue-facing assembly because metadata references do not bring transitive
        // project references along automatically.
        references.Add(MetadataReference.CreateFromFile(typeof(JazorComponent).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(VueComponent).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(JazorComponent).BaseType!.Assembly.Location));

        return CSharpCompilation.Create(
            assemblyName: "RazorVue.Analyzer.Tests",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source)],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static void AssertHasDiagnostic(IEnumerable<Diagnostic> diagnostics, string id)
        => Assert.IsTrue(
            diagnostics.Any(diagnostic => diagnostic.Id == id),
            $"Expected diagnostic {id}, actual: {string.Join(Environment.NewLine, diagnostics.Select(static x => x.ToString()))}");

    private static void AssertNoDiagnostic(IEnumerable<Diagnostic> diagnostics, params string[] ids)
    {
        var unexpected = diagnostics
            .Where(diagnostic => ids.Contains(diagnostic.Id, StringComparer.Ordinal))
            .ToArray();

        Assert.AreEqual(0, unexpected.Length, string.Join(Environment.NewLine, unexpected.Select(static x => x.ToString())));
    }
}
