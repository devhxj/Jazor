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
