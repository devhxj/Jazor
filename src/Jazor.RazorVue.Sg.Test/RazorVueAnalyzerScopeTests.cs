using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorVueAnalyzerScopeTests
{
    [TestMethod]
    public async Task ComponentSurface_DoesNotTriggerGenericWhitelistAnalyzer()
    {
        var diagnostics = await AnalyzeAsync(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using static ECMAScript.Vue;

            [ECMAScriptModule("./components/counter")]
            public sealed class Counter : ComponentBase, IVueComponent
            {
            }
            """);

        Assert.IsFalse(diagnostics.Any(static diagnostic => diagnostic.Id == "JAZOR001"));
    }

    [TestMethod]
    public async Task ComponentWithUnsupportedConcreteUsage_RemainsSubjectToGenericWhitelistAnalyzer()
    {
        var diagnostics = await AnalyzeAsync(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using static ECMAScript.Vue;

            [ECMAScriptModule("./components/counter")]
            public sealed class Counter : ComponentBase, IVueComponent
            {
                private readonly System.IO.FileInfo _file = new("counter.txt");
            }
            """);

        Assert.IsTrue(diagnostics.Any(static diagnostic => diagnostic.Id == "JAZOR001"));
    }

    [TestMethod]
    public async Task ModuleClassWithoutVueMarker_RemainsSubjectToGenericWhitelistAnalyzer()
    {
        var diagnostics = await AnalyzeAsync(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;

            [ECMAScriptModule("./components/incomplete")]
            public sealed class IncompleteComponent : ComponentBase
            {
                private readonly System.IO.FileInfo _file = new("incomplete.txt");
            }
            """);

        Assert.IsTrue(diagnostics.Any(static diagnostic => diagnostic.Id == "JAZOR001"));
    }

    [TestMethod]
    public async Task VueMemberNameMetadata_DoesNotTriggerRuntimeWhitelistDiagnostics()
    {
        var diagnostics = await AnalyzeAsync(
            """
            using ECMAScript;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using static ECMAScript.Vue;

            [ECMAScriptModule("./components/counter")]
            public sealed class Counter : ComponentBase, IVueComponent
            {
                [Parameter]
                [ECMAScriptName("runtimeTitle")]
                public string Title { get; set; } = string.Empty;
            }
            """);

        Assert.IsFalse(diagnostics.Any(static diagnostic => diagnostic.Id == "JAZOR001"),
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task VueLibraryComponentTypeArgument_IsAcceptedAsHostComponent()
    {
        var diagnostics = await AnalyzeAsync(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue;

            [ECMAScriptModule("./components/counter")]
            public sealed class Counter : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VueRouterLink>(0);
                    builder.AddComponentParameter(1, nameof(VueRouterLink.To), (RouteLocationRaw)"/");
                    builder.CloseComponent();
                }
            }
            """);

        Assert.IsFalse(diagnostics.Any(static diagnostic => diagnostic.Id == "JAZOR001"),
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task RuntimeMemberFieldLikeEvent_IsAcceptedByTheSharedCompilerProtocol()
    {
        var diagnostics = await AnalyzeAsync(
            """
            using System;
            using ECMAScript;

            [ECMAScriptModule("./events")]
            public static class EventsModule
            {
                public sealed class Emitter
                {
                    public event Action? Changed;

                    public void Subscribe(Action handler) => Changed += handler;

                    public void Raise() => Changed?.Invoke();
                }
            }
            """);

        Assert.IsFalse(diagnostics.Any(static diagnostic => diagnostic.Id == "JAZOR001"),
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task UnsupportedFieldLikeEventShapes_RemainRejectedByTheSharedCompilerProtocol()
    {
        var diagnostics = await AnalyzeAsync(
            """
            using System;
            using ECMAScript;

            [ECMAScriptModule("./events")]
            public static class EventsModule
            {
                public sealed class Emitter
                {
                    private Action? _changed;

                    public static event Action? GlobalChanged;

                    public event Action? Changed
                    {
                        add => _changed += value;
                        remove => _changed -= value;
                    }
                }

                public class VirtualEmitter
                {
                    public virtual event Action? Changed;
                }

                public struct ValueEmitter
                {
                    public event Action? Changed;
                }
            }
            """);

        Assert.IsTrue(diagnostics.Count(static diagnostic => diagnostic.Id == "JAZOR001") >= 4,
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task AllowedGenericCarriers_RejectUnsupportedConcreteTypeArguments()
    {
        var diagnostics = await AnalyzeAsync(
            """
            using ECMAScript;

            [ECMAScriptModule("./generic-carriers")]
            public static class GenericCarriers
            {
                private static System.Collections.Generic.IEnumerable<System.IO.FileInfo>? _interface;
                private static System.Func<System.IO.FileInfo>? _delegate;
                private static System.Collections.Generic.Comparer<System.IO.FileInfo>? _abstract;
                private static Payload<System.IO.FileInfo>? _record;

                public sealed record Payload<T>(T Value);
            }
            """);

        var unsupportedTypeDiagnostics = diagnostics
            .Where(static diagnostic => diagnostic.Id == "JAZOR001")
            .Where(static diagnostic => diagnostic.GetMessage().Contains("System.IO.FileInfo", StringComparison.Ordinal))
            .ToArray();

        Assert.HasCount(4, unsupportedTypeDiagnostics, string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task GenericInterfaceWithTypeParameter_RemainsAccepted()
    {
        var diagnostics = await AnalyzeAsync(
            """
            using ECMAScript;

            [ECMAScriptModule("./generic-carriers")]
            public static class GenericCarriers
            {
                public static System.Collections.Generic.IEnumerable<T> Keep<T>(
                    System.Collections.Generic.IEnumerable<T> values)
                    => values;
            }
            """);

        Assert.IsFalse(diagnostics.Any(static diagnostic => diagnostic.Id == "JAZOR001"),
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task SupportedRuntimeEvent_RejectsUnsupportedDelegatePayload()
    {
        var diagnostics = await AnalyzeAsync(
            """
            using System;
            using ECMAScript;

            [ECMAScriptModule("./events")]
            public static class EventsModule
            {
                public sealed class Emitter
                {
                    public event Action<System.IO.FileInfo>? Changed;
                }
            }
            """);

        AssertReportsUnsupportedFileInfo(diagnostics);
    }

    [TestMethod]
    public async Task IndexerParameter_RejectsUnsupportedConcreteType()
    {
        var diagnostics = await AnalyzeAsync(
            """
            using ECMAScript;

            [ECMAScriptModule("./indexer")]
            public static class IndexerModule
            {
                public sealed class Entry
                {
                    public string this[System.IO.FileInfo file] => string.Empty;
                }
            }
            """);

        AssertReportsUnsupportedFileInfo(diagnostics);
    }

    [TestMethod]
    public async Task ECMAScriptInterfaceSignature_RejectsUnsupportedConcreteType()
    {
        var diagnostics = await AnalyzeAsync(
            """
            using ECMAScript;

            [ECMAScript]
            public interface IFileContract
            {
                System.IO.FileInfo Read();
            }
            """);

        AssertReportsUnsupportedFileInfo(diagnostics);
    }

    [TestMethod]
    public async Task ECMAScriptDelegateSignature_RejectsUnsupportedConcreteType()
    {
        var diagnostics = await AnalyzeAsync(
            """
            using ECMAScript;

            [ECMAScript]
            public delegate System.IO.FileInfo FileCallback(System.IO.FileInfo value);
            """);

        AssertReportsUnsupportedFileInfo(diagnostics);
    }

    [TestMethod]
    public async Task ECMAScriptGenericContractSignatures_AllowTypeParameters()
    {
        var diagnostics = await AnalyzeAsync(
            """
            using ECMAScript;

            [ECMAScript]
            public interface IValueContract<T>
            {
                T Read(T fallback);
            }

            [ECMAScript]
            public delegate T ValueCallback<T>(T value);
            """);

        Assert.IsFalse(diagnostics.Any(static diagnostic => diagnostic.Id == "JAZOR001"),
            string.Join(Environment.NewLine, diagnostics));
    }

    [TestMethod]
    public async Task RuntimeTypeFilters_RejectUnsupportedConcreteTypes()
    {
        var diagnostics = await AnalyzeAsync(
            """
            using ECMAScript;

            [ECMAScriptModule("./type-filters")]
            public static class TypeFilters
            {
                public static bool IsType(object value) => value is System.IO.FileInfo;

                public static bool IsPattern(object value) => value is System.IO.FileNotFoundException _;

                public static bool IsRecursivePattern(object value) => value is System.IO.FileStream { };

                public static bool IsParenthesizedPattern(object value) => value is (System.IO.StringReader _);

                public static bool SwitchStatement(object value)
                {
                    switch (value)
                    {
                        case System.IO.DirectoryInfo _:
                            return true;
                        default:
                            return false;
                    }
                }

                public static bool SwitchExpression(object value) => value switch
                {
                    System.IO.DriveInfo _ => true,
                    _ => false
                };

                public static void CatchException()
                {
                    try
                    {
                    }
                    catch (System.UnauthorizedAccessException)
                    {
                    }
                }
            }
            """);

        AssertReportsUnsupportedTypes(
            diagnostics,
            "System.IO.FileInfo",
            "System.IO.FileNotFoundException",
            "System.IO.FileStream",
            "System.IO.StringReader",
            "System.IO.DirectoryInfo",
            "System.IO.DriveInfo",
            "System.UnauthorizedAccessException");
    }

    private static void AssertReportsUnsupportedFileInfo(ImmutableArray<Diagnostic> diagnostics)
    {
        AssertReportsUnsupportedTypes(diagnostics, "System.IO.FileInfo");
    }

    private static void AssertReportsUnsupportedTypes(
        ImmutableArray<Diagnostic> diagnostics,
        params string[] typeNames)
    {
        foreach (var typeName in typeNames)
        {
            Assert.IsTrue(
                diagnostics.Any(diagnostic =>
                    diagnostic.Id == "JAZOR001" &&
                    diagnostic.GetMessage().Contains(typeName, StringComparison.Ordinal)),
                string.Join(Environment.NewLine, diagnostics));
        }
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.AnalyzerScope.Tests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    source,
                    new CSharpParseOptions(LanguageVersion.Preview),
                    path: "Component.razor.cs")
            ],
            references: RazorSgTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return await compilation
            .WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new Jazor.Analyzer.Analyzer()))
            .GetAnalyzerDiagnosticsAsync();
    }
}
