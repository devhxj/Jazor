using System.Reflection;
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

    [TestMethod]
    public void PrivateConventionHelpers_ClassifyNonLibraryAndIncompleteDescriptorShapes()
    {
        var compilation = CreateCompilation(
            """
            #nullable enable
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace Demo;

            [VueLibraryComponent("demo-components", "LibraryWidget")]
            [VueLibraryEmit(nameof(Ready))]
            [VueLibraryEmit("Other", Name = "  ")]
            public sealed class LibraryWidget : ComponentBase
            {
                [Parameter] public string Value { get; set; } = string.Empty;
                [Parameter] public global::Microsoft.AspNetCore.Components.EventCallback Ready { get; set; }
                [Parameter] public global::Microsoft.AspNetCore.Components.EventCallback ValueChanged { get; set; }
            }

            public sealed class StandardWidget : ComponentBase
            {
                public string Plain { get; set; } = string.Empty;
                [Parameter] public string Value { get; set; } = string.Empty;
                [Parameter] public global::Microsoft.AspNetCore.Components.EventCallback Ready { get; set; }
                [Parameter] public global::Microsoft.AspNetCore.Components.RenderFragment? HeaderContent { get; set; }
                public EventCallback LocalCallback { get; set; }
                public RenderFragment? LocalFragment { get; set; }
            }

            public sealed class EventCallback;
            public sealed class RenderFragment;

            public sealed class GenericOwner<T>;
            """);
        var library = GetNamedType(compilation, "Demo.LibraryWidget");
        var standard = GetNamedType(compilation, "Demo.StandardWidget");
        var genericOwner = GetNamedType(compilation, "Demo.GenericOwner`1");
        var value = GetDeclaredProperty(library, "Value");
        var ready = GetDeclaredProperty(library, "Ready");
        var valueChanged = GetDeclaredProperty(library, "ValueChanged");
        var plain = GetDeclaredProperty(standard, "Plain");
        var standardValue = GetDeclaredProperty(standard, "Value");
        var standardReady = GetDeclaredProperty(standard, "Ready");
        var header = GetDeclaredProperty(standard, "HeaderContent");
        var localCallback = GetDeclaredProperty(standard, "LocalCallback");
        var localFragment = GetDeclaredProperty(standard, "LocalFragment");

        Assert.IsFalse(LibraryComponentConventions.TryGetModelUpdateEventName(library, value, out _));
        Assert.IsFalse(LibraryComponentConventions.TryGetModelUpdateEventName(library, ready, out _));
        Assert.IsTrue(LibraryComponentConventions.TryGetModelUpdateEventName(library, valueChanged, out var updateName));
        Assert.AreEqual("update:value", updateName);
        Assert.AreEqual("ready", LibraryComponentConventions.GetEmitRuntimeName(library, ready));
        Assert.AreEqual("headerContent", LibraryComponentConventions.GetSlotRuntimeName(standard, header));

        Assert.IsTrue(InvokePrivate<bool>("IsVueLibraryComponent", library));
        Assert.IsFalse(InvokePrivate<bool>("IsVueLibraryComponent", standard));
        Assert.IsFalse(LibraryComponentConventions.IsParameterProperty(plain));
        Assert.IsTrue(LibraryComponentConventions.IsParameterProperty(standardValue));
        Assert.AreEqual(string.Empty, InvokePrivate<string>("ToDefaultRuntimeName", string.Empty));
        Assert.AreEqual("value", InvokePrivate<string>("ToDefaultRuntimeName", "Value"));
        Assert.IsTrue(InvokePrivate<bool>("IsEventCallback", ready.Type));
        Assert.IsFalse(InvokePrivate<bool>("IsEventCallback", localCallback.Type));
        Assert.IsFalse(InvokePrivate<bool>("IsEventCallback", genericOwner.TypeParameters.Single()));
        Assert.IsTrue(InvokePrivate<bool>("IsRenderFragment", header.Type));
        Assert.IsFalse(InvokePrivate<bool>("IsRenderFragment", localFragment.Type));
        Assert.IsFalse(InvokePrivate<bool>("IsRenderFragment", genericOwner.TypeParameters.Single()));

        var attributes = library.GetAttributes();
        var noName = attributes.Single(attribute => attribute.ConstructorArguments[0].Value as string == "Ready");
        var whitespaceName = attributes.Single(attribute => attribute.ConstructorArguments[0].Value as string == "Other");
        Assert.IsNull(InvokePrivate<string?>("GetNamedString", noName, "Name"));
        Assert.IsNull(InvokePrivate<string?>("GetNamedString", whitespaceName, "Name"));
        Assert.IsFalse(InvokePrivate<bool>(
            "IsDescriptorFor",
            whitespaceName,
            "ECMAScript.VueContract.VueLibraryEmitAttribute",
            "Ready"));
    }

    [TestMethod]
    public void PrivateConventionHelpers_ScanDecoratedMetadataAndRejectWrongHostTypes()
    {
        var compilation = CreateCompilation(
            """
            #nullable enable
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace Demo;

            [AttributeUsage(AttributeTargets.Class)]
            public sealed class CustomMetadataAttribute : Attribute
            {
                public bool Flag { get; set; }
            }

            [Obsolete("container metadata")]
            [CustomMetadata(Flag = true)]
            [VueLibraryComponent("demo-components", "DecoratedWidget")]
            public sealed class DecoratedWidget : ComponentBase
            {
                [Obsolete("listener metadata"), Parameter]
                public EventCallback OnOpen { get; set; }
            }
            """);
        var component = GetNamedType(compilation, "Demo.DecoratedWidget");
        var onOpen = GetDeclaredProperty(component, "OnOpen");
        var obsolete = component.GetAttributes().Single(attribute =>
            string.Equals(attribute.AttributeClass?.ToDisplayString(), "System.ObsoleteAttribute", StringComparison.Ordinal));
        var customMetadata = component.GetAttributes().Single(attribute =>
            string.Equals(attribute.AttributeClass?.ToDisplayString(), "Demo.CustomMetadataAttribute", StringComparison.Ordinal));

        Assert.IsTrue(LibraryComponentConventions.IsParameterProperty(onOpen));
        Assert.AreEqual("open", LibraryComponentConventions.GetEmitRuntimeName(component, onOpen));
        Assert.IsTrue(InvokePrivate<bool>("IsVueLibraryComponent", component));
        Assert.IsFalse(InvokePrivate<bool>(
            "IsDescriptorFor",
            obsolete,
            "ECMAScript.VueContract.VueLibraryEmitAttribute",
            "OnOpen"));
        Assert.IsNull(InvokePrivate<string?>("GetNamedString", customMetadata, "Name"));
        Assert.IsNull(InvokePrivate<string?>("GetNamedString", customMetadata, "Flag"));

        var stringType = compilation.GetSpecialType(SpecialType.System_String);
        Assert.IsFalse(InvokePrivate<bool>("IsEventCallback", stringType));
        Assert.IsFalse(InvokePrivate<bool>("IsRenderFragment", stringType));
    }

    private static T InvokePrivate<T>(string methodName, params object?[] arguments)
    {
        var method = typeof(LibraryComponentConventions)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(candidate => candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length);
        return (T)method.Invoke(null, arguments)!;
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
