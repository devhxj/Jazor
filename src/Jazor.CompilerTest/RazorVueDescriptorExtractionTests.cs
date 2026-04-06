using Basic.Reference.Assemblies;
using Jazor.RazorVue;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.Razor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class RazorVueDescriptorExtractionTests
{
    [TestMethod]
    public void RazorVue_Context_DiscoversVueComponentCandidates()
    {
        var context = CreateContext(
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
            public static class LegacyModule
            {
            }

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule]
                public partial class Counter : VueComponent
                {
                    protected void BuildRenderTree(object builder)
                    {
                    }
                }
            }
            """);

        var candidates = context.DiscoverComponentCandidates();
        Assert.HasCount(1, candidates);
        Assert.AreEqual("Counter", candidates[0].ComponentSymbol.Name);
        Assert.AreEqual(RazorVueEntryKind.RazorVueComponent, candidates[0].EntryKind);
        Assert.IsNotNull(candidates[0].BuildRenderTreeMethod);
    }

    [TestMethod]
    public void RazorVue_Snapshot_ParameterEventCallbackAndSlots_AreProjectedIntoDescriptor()
    {
        var snapshot = CreateSingleSnapshot(
            """
            using System;
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

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/counter")]
                public class Counter : VueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }

                    [Parameter]
                    public EventCallback<int> OnSave { get; set; }

                    [Parameter]
                    public RenderFragment? ChildContent { get; set; }

                    [Parameter]
                    public RenderFragment<string>? Header { get; set; }
                }
            }
            """);

        var descriptor = snapshot.Descriptor;
        Assert.AreEqual("Counter", descriptor.Name);
        Assert.AreEqual("Demo.Components.Counter", descriptor.FullName);
        Assert.AreEqual(VueComponentSourceKind.UserComponent, descriptor.SourceKind);
        Assert.AreEqual("Demo.Components", descriptor.ResolutionNamespace);
        Assert.AreEqual("./components/counter.mjs", descriptor.ImportSpecifier);
        Assert.AreEqual("default", descriptor.ExportName);

        var titleProp = descriptor.Props.Single(prop => prop.PublicName == "Title");
        Assert.AreEqual("title", titleProp.Name);
        Assert.AreEqual("string?", titleProp.TypeName);
        Assert.IsFalse(titleProp.AcceptsBinding);
        Assert.AreEqual(VuePropKind.Normal, titleProp.Kind);

        var valueProp = descriptor.Props.Single(prop => prop.PublicName == "Value");
        Assert.AreEqual("value", valueProp.Name);
        Assert.IsTrue(valueProp.AcceptsBinding);
        Assert.AreEqual(VuePropKind.Model, valueProp.Kind);

        var onSaveEmit = descriptor.Emits.Single(emit => emit.RazorAlias == "OnSave");
        Assert.AreEqual("save", onSaveEmit.Name);
        Assert.AreEqual("int", onSaveEmit.PayloadTypeName);
        Assert.AreEqual(VueEmitKind.Normal, onSaveEmit.Kind);

        var valueChangedEmit = descriptor.Emits.Single(emit => emit.RazorAlias == "ValueChanged");
        Assert.AreEqual("update:value", valueChangedEmit.Name);
        Assert.AreEqual("int", valueChangedEmit.PayloadTypeName);
        Assert.AreEqual(VueEmitKind.ModelUpdate, valueChangedEmit.Kind);

        var defaultSlot = descriptor.Slots.Single(slot => slot.IsDefault);
        Assert.AreEqual("default", defaultSlot.Name);
        Assert.IsEmpty(defaultSlot.Parameters);

        var headerSlot = descriptor.Slots.Single(slot => slot.Name == "header");
        Assert.IsFalse(headerSlot.IsDefault);
        Assert.HasCount(1, headerSlot.Parameters);
        Assert.AreEqual("context", headerSlot.Parameters[0].Name);
        Assert.AreEqual("string", headerSlot.Parameters[0].TypeName);

        Assert.AreEqual("Counter", snapshot.ComponentSymbol.Name);
        Assert.HasCount(1, snapshot.Origins);
        Assert.AreEqual(RazorVueOriginKind.Component, snapshot.Origins[0].OriginKind);
        Assert.AreEqual(RazorVueMappingQuality.MappedFromGenerated, snapshot.Origins[0].MappingQuality);
        Assert.AreEqual(RazorVueOriginProvenance.GeneratedSyntaxLocation, snapshot.Origins[0].Provenance);
    }

    [TestMethod]
    public void RazorVue_Candidate_ExtractsLifecycleAndLogicMethods()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
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

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : VueComponent, IDisposable, IAsyncDisposable
                {
                    protected override void OnInitialized()
                    {
                    }

                    protected override void OnParametersSet()
                    {
                    }

                    protected override void OnAfterRender(bool firstRender)
                    {
                    }

                    public void Dispose()
                    {
                    }

                    public ValueTask DisposeAsync()
                        => ValueTask.CompletedTask;

                    public int Calculate(int value)
                        => value + 1;

                    public async Task RefreshAsync()
                    {
                        await Task.CompletedTask;
                    }
                }
            }
            """);

        var candidate = context.DiscoverComponentCandidates().Single();

        Assert.IsNotNull(candidate.OnInitializedMethod);
        Assert.IsNotNull(candidate.OnParametersSetMethod);
        Assert.IsNotNull(candidate.OnAfterRenderMethod);
        Assert.IsNotNull(candidate.DisposeMethod);
        Assert.IsNotNull(candidate.DisposeAsyncMethod);

        var logicNames = candidate.LogicMethods.Select(static method => method.Name).ToArray();
        CollectionAssert.Contains(logicNames, "Calculate");
        CollectionAssert.Contains(logicNames, "RefreshAsync");
    }

    [TestMethod]
    public void RazorVue_Snapshot_ContainsLifecycleAndLogicDescriptors()
    {
        var snapshot = CreateSingleSnapshot(
            """
            using System;
            using System.Threading.Tasks;
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

            namespace Demo.Components
            {
                [ECMAScript.ECMAScriptModule("./components/lifecycle-card")]
                public class LifecycleCard : VueComponent, IDisposable, IAsyncDisposable
                {
                    protected override void OnInitialized()
                    {
                    }

                    protected override void OnParametersSet()
                    {
                    }

                    protected override void OnAfterRender(bool firstRender)
                    {
                    }

                    public void Dispose()
                    {
                    }

                    public ValueTask DisposeAsync()
                        => ValueTask.CompletedTask;

                    public int Calculate(int value)
                        => value + 1;

                    public async Task RefreshAsync()
                    {
                        await Task.CompletedTask;
                    }
                }
            }
            """);

        Assert.IsTrue(snapshot.Lifecycle.HasOnInitialized);
        Assert.IsTrue(snapshot.Lifecycle.HasOnParametersSet);
        Assert.IsTrue(snapshot.Lifecycle.HasOnAfterRender);
        Assert.IsTrue(snapshot.Lifecycle.HasDispose);
        Assert.IsTrue(snapshot.Lifecycle.HasDisposeAsync);
        Assert.IsTrue(snapshot.Lifecycle.HasAnyHook);

        var calculate = snapshot.Logic.Methods.Single(method => method.Name == "Calculate");
        Assert.AreEqual(1, calculate.Arity);
        Assert.IsFalse(calculate.IsAsync);

        var refresh = snapshot.Logic.Methods.Single(method => method.Name == "RefreshAsync");
        Assert.AreEqual(0, refresh.Arity);
        Assert.IsTrue(refresh.IsAsync);
    }

    private static RazorVueCompilationContext CreateContext(string source)
    {
        var compilation = CreateCompilation(source);
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));

        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);
        return context;
    }

    private static RazorVueSemanticSnapshot CreateSingleSnapshot(string source)
    {
        var context = CreateContext(source);
        var candidates = context.DiscoverComponentCandidates();
        Assert.HasCount(1, candidates);
        return context.CreateSemanticSnapshot(candidates[0]);
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var references = Net100.References.All
            .Cast<MetadataReference>()
            .ToList();
        references.Add(MetadataReference.CreateFromFile(typeof(JazorComponent).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(VueComponent).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(JazorComponent).BaseType!.Assembly.Location));

        return CSharpCompilation.Create(
            assemblyName: "RazorVue.Descriptor.Tests",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source)],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}

