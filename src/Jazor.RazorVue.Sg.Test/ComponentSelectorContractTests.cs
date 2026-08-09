using System.Reflection;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class ComponentSelectorContractTests
{
    [TestMethod]
    public void DiscoverCurrentComponents_RejectsNullAndReturnsEmptyWhenRequiredMetadataIsUnavailable()
    {
        var nullCompilation = Assert.Throws<ArgumentNullException>(() => ComponentSelector.DiscoverCurrentComponents(null!));
        Assert.AreEqual("compilation", nullCompilation.ParamName);

        var noMetadata = CSharpCompilation.Create(
            "RazorVue.ComponentSelector.NoMetadata",
            [CSharpSyntaxTree.ParseText("namespace Demo; public sealed class PlainComponent { }")],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.IsTrue(ComponentSelector.DiscoverCurrentComponents(noMetadata).IsDefaultOrEmpty);

        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;

            namespace Demo;

            [ECMAScriptModule("./components/no-vue-marker")]
            public sealed class ModuleOnlyComponent
            {
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview));
        var noMarker = CSharpCompilation.Create(
            "RazorVue.ComponentSelector.NoVueMarker",
            [sourceTree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(noMarker);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        Assert.IsTrue(ComponentSelector.DiscoverCurrentComponents(noMarker).IsDefaultOrEmpty);
    }

    [TestMethod]
    public void FindBuildRenderTreeMethod_RequiresSourceInstanceRenderTreeSignatureAndSkipsGeneratedMethods()
    {
        var compilation = CreateCompilation(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo;

            internal static class StaticCandidate
            {
                public static void BuildRenderTree(RenderTreeBuilder builder)
                {
                }
            }

            internal sealed class WrongSignatureCandidate
            {
                public void BuildRenderTree(string content)
                {
                }
            }

            [ECMAScriptModule("./components/designer")]
            public sealed class DesignerComponent : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "designer");
                }
            }
            """,
            "Candidates/DesignerComponent.designer.cs");
        var staticCandidate = GetNamedType(compilation, "Demo.StaticCandidate");
        var wrongSignatureCandidate = GetNamedType(compilation, "Demo.WrongSignatureCandidate");
        var designerComponent = GetNamedType(compilation, "Demo.DesignerComponent");

        Assert.IsNull(ComponentSelector.FindBuildRenderTreeMethod(staticCandidate));
        Assert.IsNull(ComponentSelector.FindBuildRenderTreeMethod(wrongSignatureCandidate));
        Assert.IsNotNull(ComponentSelector.FindBuildRenderTreeMethod(designerComponent));
        Assert.IsNull(ComponentSelector.FindHandwrittenBuildRenderTreeMethod(designerComponent));

        Assert.AreEqual(1, ComponentSelector.DiscoverCurrentComponents(compilation).Length);
        Assert.IsTrue(ComponentSelector.DiscoverHandwrittenComponents(compilation).IsDefaultOrEmpty);
        Assert.AreEqual(1, ComponentSelector.DiscoverTailOutputComponents(compilation).Length);
    }

    [TestMethod]
    public void TrySelect_RejectsNullBinding()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => ComponentSelector.TrySelect(null!, out _, out _));
        Assert.AreEqual("binding", exception.ParamName);
    }

    [TestMethod]
    public void SourceIdentityHelpers_ClassifyRazorAndGeneratedPathShapes()
    {
        var razorCompilation = CreateCompilation(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo;

            [ECMAScriptModule("./components/razor")]
            public sealed class RazorIdentity : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "razor");
                }
            }

            public sealed class PlainIdentity
            {
                public void Render() { }
            }
            """,
            "Pages/RazorIdentity.razor");
        var generatedCompilation = CreateCompilation(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo;

            [ECMAScriptModule("./components/generated")]
            public sealed class GeneratedIdentity : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder) { }
            }
            """,
            "obj/GeneratedIdentity.razor.g.cs");
        var razor = GetNamedType(razorCompilation, "Demo.RazorIdentity");
        var plain = GetNamedType(razorCompilation, "Demo.PlainIdentity");
        var generated = GetNamedType(generatedCompilation, "Demo.GeneratedIdentity");
        var moduleAttribute = razorCompilation.GetTypeByMetadataName("ECMAScript.ECMAScriptModuleAttribute");
        Assert.IsNotNull(moduleAttribute);

        Assert.IsTrue(InvokePrivate<bool>("HasCurrentCompilationSource", razor));
        Assert.IsFalse(InvokePrivate<bool>("HasCurrentCompilationSource", razorCompilation.GetSpecialType(SpecialType.System_String)));
        Assert.IsTrue(InvokePrivate<bool>("HasECMAScriptModuleAttribute", razor, moduleAttribute!));
        Assert.IsFalse(InvokePrivate<bool>("HasECMAScriptModuleAttribute", plain, moduleAttribute!));
        Assert.IsTrue(InvokePrivate<bool>("HasRazorSourceIdentity", razor));
        Assert.IsFalse(InvokePrivate<bool>(
            "HasRazorSourceIdentity",
            razorCompilation.GetSpecialType(SpecialType.System_String)));
        Assert.IsTrue(InvokePrivate<bool>("HasRazorSourceIdentity", GetBuildMethod(razor)));
        Assert.IsTrue(InvokePrivate<bool>("IsGeneratedSourcePath", "obj/Counter.razor.g.cs"));
        Assert.IsTrue(InvokePrivate<bool>("IsGeneratedSourcePath", "obj/Counter.generated.cs"));
        Assert.IsFalse(InvokePrivate<bool>("IsGeneratedSourcePath", "Pages/Counter.razor"));
        Assert.IsFalse(InvokePrivate<bool>("IsGeneratedSourcePath", string.Empty));
        Assert.IsTrue(InvokePrivate<bool>("HasRazorSourcePath", "Pages/Counter.razor"));
        Assert.IsTrue(InvokePrivate<bool>("HasRazorSourcePath", "Pages/Counter.razor.cs"));
        Assert.IsFalse(InvokePrivate<bool>("HasRazorSourcePath", "Pages/Counter.cs"));
        Assert.IsNull(ComponentSelector.FindHandwrittenBuildRenderTreeMethod(generated));
    }

    [TestMethod]
    public void SourceIdentityHelpers_RecognizeLineMappedRazorOrigins()
    {
        var compilation = CreateCompilation(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo;

            #line 1 "Pages/LineMappedIdentity.razor"
            [ECMAScriptModule("./components/line-mapped")]
            public sealed class LineMappedIdentity : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "line-mapped");
                }
            }
            #line default
            """,
            "Generated/LineMappedIdentity.cs");
        var component = GetNamedType(compilation, "Demo.LineMappedIdentity");
        var buildRenderTree = GetBuildMethod(component);

        Assert.IsTrue(InvokePrivate<bool>("HasRazorSourceIdentity", component));
        Assert.IsTrue(InvokePrivate<bool>("HasRazorSourceIdentity", buildRenderTree));
        Assert.IsTrue(InvokePrivate<bool>("IsLikelyRazorAuthored", component));
        Assert.IsNull(ComponentSelector.FindHandwrittenBuildRenderTreeMethod(component));
    }

    [TestMethod]
    public void SourceIdentityHelpers_RejectNonRazorSourcePathsAndNullPathInputs()
    {
        var compilation = CreateCompilation(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo;

            [ECMAScriptModule("./components/plain-source")]
            public sealed class PlainSourceComponent : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "plain-source");
                }
            }
            """,
            "Pages/PlainSourceComponent.cs");
        var component = GetNamedType(compilation, "Demo.PlainSourceComponent");
        var buildRenderTree = GetBuildMethod(component);

        Assert.IsFalse(InvokePrivate<bool>("HasRazorSourceIdentity", component));
        Assert.IsFalse(InvokePrivate<bool>("HasRazorSourceIdentity", buildRenderTree));
        Assert.IsFalse(InvokePrivate<bool>(
            "HasMappedRazorPath",
            buildRenderTree.DeclaringSyntaxReferences.Single().GetSyntax()));
        Assert.IsFalse(InvokePrivate<bool>("IsGeneratedSourcePath", (object?)null));
        Assert.IsFalse(InvokePrivate<bool>("HasRazorSourcePath", (object?)null));
        Assert.IsNotNull(ComponentSelector.FindHandwrittenBuildRenderTreeMethod(component));
        Assert.IsTrue(ComponentSelector.DiscoverTailRequiredComponents(compilation).IsDefaultOrEmpty);
    }

    private static IMethodSymbol GetBuildMethod(INamedTypeSymbol type)
        => type.GetMembers("BuildRenderTree").OfType<IMethodSymbol>().Single();

    private static T InvokePrivate<T>(string methodName, params object?[] arguments)
    {
        var method = typeof(ComponentSelector)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(candidate =>
                candidate.Name == methodName &&
                candidate.GetParameters().Length == arguments.Length &&
                candidate.GetParameters()
                    .Zip(arguments, static (parameter, argument) =>
                        argument is null
                            ? !parameter.ParameterType.IsValueType || Nullable.GetUnderlyingType(parameter.ParameterType) is not null
                            : parameter.ParameterType.IsInstanceOfType(argument))
                    .All(static matches => matches));
        return (T)method.Invoke(null, arguments)!;
    }

    private static CSharpCompilation CreateCompilation(string source, string path)
    {
        var compilation = CSharpCompilation.Create(
            "RazorVue.ComponentSelector.Contract.Tests",
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), path)],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        return compilation;
    }

    private static INamedTypeSymbol GetNamedType(Compilation compilation, string metadataName)
    {
        var symbol = compilation.GetTypeByMetadataName(metadataName);
        Assert.IsNotNull(symbol, metadataName);
        return symbol!;
    }
}
