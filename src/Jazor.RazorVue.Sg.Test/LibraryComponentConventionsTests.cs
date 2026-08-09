using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class LibraryComponentConventionsTests
{
    [TestMethod]
    public void Naming_ResolvesExplicitModelDescriptorAndConventionalEventContracts()
    {
        var compilation = CreateCompilation(
            """
            #nullable enable
            using ECMAScript;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace Demo;

            [VueLibraryComponent("demo-components", "LibraryWidget")]
            [VueLibraryEmit(nameof(ExternalChanged), Name = "change:external")]
            [VueLibraryEmit(nameof(OnSave), Name = "  ")]
            [VueLibraryEmit("Other", Name = "ignored")]
            public sealed class LibraryWidget : ComponentBase
            {
                [Parameter, ECMAScriptName("data-title")]
                public string Title { get; set; } = string.Empty;

                [Parameter]
                public string Value { get; set; } = string.Empty;

                [Parameter]
                public EventCallback<string> ValueChanged { get; set; }

                [Parameter]
                public EventCallback OnSave { get; set; }

                [Parameter]
                public EventCallback ExternalChanged { get; set; }

                [Parameter, ECMAScriptName("onDismiss")]
                public EventCallback Dismissed { get; set; }

                [Parameter]
                public EventCallback ChangedWithoutModel { get; set; }
            }
            """);
        var component = GetNamedType(compilation, "Demo.LibraryWidget");

        var title = GetDeclaredProperty(component, "Title");
        var valueChanged = GetDeclaredProperty(component, "ValueChanged");
        var onSave = GetDeclaredProperty(component, "OnSave");
        var externalChanged = GetDeclaredProperty(component, "ExternalChanged");
        var dismissed = GetDeclaredProperty(component, "Dismissed");
        var changedWithoutModel = GetDeclaredProperty(component, "ChangedWithoutModel");

        Assert.AreEqual("data-title", LibraryComponentConventions.GetPropRuntimeName(title));
        Assert.AreEqual("onUpdate:value", LibraryComponentConventions.GetEventListenerRuntimeName(component, valueChanged));
        Assert.AreEqual("onSave", LibraryComponentConventions.GetEventListenerRuntimeName(component, onSave));
        Assert.AreEqual("onChange:external", LibraryComponentConventions.GetEventListenerRuntimeName(component, externalChanged));
        Assert.AreEqual("onDismiss", LibraryComponentConventions.GetEventListenerRuntimeName(component, dismissed));
        Assert.AreEqual("changedWithoutModel", LibraryComponentConventions.GetEventListenerRuntimeName(component, changedWithoutModel));

        Assert.AreEqual("update:value", LibraryComponentConventions.GetEmitRuntimeName(component, valueChanged));
        Assert.AreEqual("save", LibraryComponentConventions.GetEmitRuntimeName(component, onSave));
        Assert.AreEqual("change:external", LibraryComponentConventions.GetEmitRuntimeName(component, externalChanged));
        Assert.AreEqual("dismiss", LibraryComponentConventions.GetEmitRuntimeName(component, dismissed));
        Assert.AreEqual("changedWithoutModel", LibraryComponentConventions.GetEmitRuntimeName(component, changedWithoutModel));

        Assert.IsTrue(LibraryComponentConventions.TryGetModelUpdateEventName(component, valueChanged, out var modelEvent));
        Assert.AreEqual("update:value", modelEvent);
        Assert.IsFalse(LibraryComponentConventions.TryGetModelUpdateEventName(component, changedWithoutModel, out var missingModelEvent));
        Assert.AreEqual(string.Empty, missingModelEvent);
    }

    [TestMethod]
    public void Naming_UsesEffectiveInheritedParametersAndSeparatesPropAndSlotNameDomains()
    {
        var compilation = CreateCompilation(
            """
            #nullable enable
            using ECMAScript;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace Demo;

            public abstract class StandardBase : ComponentBase
            {
                [Parameter]
                public string Title { get; set; } = string.Empty;

                [Parameter]
                public RenderFragment? HeaderContent { get; set; }

                [Parameter]
                public RenderFragment? ChildContent { get; set; }
            }

            public sealed class StandardWidget : StandardBase
            {
                public new string Title { get; set; } = string.Empty;
            }

            [VueLibraryComponent("demo-components", "LibraryWidget")]
            public sealed class LibraryWidget : ComponentBase
            {
                [Parameter]
                public string Shared { get; set; } = string.Empty;

                [Parameter, ECMAScriptName("shared")]
                public RenderFragment? SharedContent { get; set; }

                [Parameter]
                public RenderFragment? HeaderContent { get; set; }

                [Parameter]
                public RenderFragment? ChildContent { get; set; }

                [Parameter]
                public RenderFragment? ApiURLContent { get; set; }
            }

            [VueLibraryComponent("demo-components", "DefaultSlotWidget")]
            public sealed class DefaultSlotWidget : ComponentBase
            {
                [Parameter]
                public RenderFragment? DefaultContent { get; set; }
            }
            """);
        var standard = GetNamedType(compilation, "Demo.StandardWidget");
        var library = GetNamedType(compilation, "Demo.LibraryWidget");
        var defaultSlot = GetNamedType(compilation, "Demo.DefaultSlotWidget");

        var effective = LibraryComponentConventions.GetEffectiveParameterProperties(standard);
        CollectionAssert.AreEquivalent(
            new[] { "Title", "HeaderContent", "ChildContent" },
            effective.Select(static property => property.Name).ToArray());
        Assert.AreEqual("StandardBase", effective.Single(static property => property.Name == "Title").ContainingType.Name);
        Assert.AreEqual(
            "headerContent",
            LibraryComponentConventions.GetSlotRuntimeName(
                standard,
                effective.Single(static property => property.Name == "HeaderContent")));

        Assert.AreEqual("header", LibraryComponentConventions.GetSlotRuntimeName(library, GetDeclaredProperty(library, "HeaderContent")));
        Assert.AreEqual("default", LibraryComponentConventions.GetSlotRuntimeName(library, GetDeclaredProperty(library, "ChildContent")));
        Assert.AreEqual("default", LibraryComponentConventions.GetSlotRuntimeName(defaultSlot, GetDeclaredProperty(defaultSlot, "DefaultContent")));
        Assert.AreEqual("api-url", LibraryComponentConventions.GetSlotRuntimeName(library, GetDeclaredProperty(library, "ApiURLContent")));

        var names = LibraryComponentConventions.BuildParameterRuntimeNameMap(library);
        Assert.AreEqual("shared", names["SharedContent"]);
        Assert.AreEqual("header", names["HeaderContent"]);
        Assert.AreEqual("default", names["ChildContent"]);
        Assert.AreEqual("api-url", names["ApiURLContent"]);
        Assert.IsFalse(names.ContainsKey("Shared"));

        var defaultSlotNames = LibraryComponentConventions.BuildParameterRuntimeNameMap(defaultSlot);
        Assert.AreEqual("default", defaultSlotNames["DefaultContent"]);
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var compilation = CSharpCompilation.Create(
            "RazorVue.LibraryComponentConventions.Tests",
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview))],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        return compilation;
    }

    private static INamedTypeSymbol GetNamedType(Compilation compilation, string metadataName)
    {
        var component = compilation.GetTypeByMetadataName(metadataName);
        Assert.IsNotNull(component, metadataName);
        return component!;
    }

    private static IPropertySymbol GetDeclaredProperty(INamedTypeSymbol component, string name)
        => component.GetMembers(name).OfType<IPropertySymbol>().Single();
}
