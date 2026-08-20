using System.Reflection;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class LibraryComponentConventionsTests
{
    [TestMethod]
    public void Naming_UsesExplicitMemberMetadataWithoutVueInference()
    {
        var compilation = CreateCompilation(
            """
            #nullable enable
            using ECMAScript;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace Demo;

            [VueLibraryComponent("demo-components", "LibraryWidget")]
            public sealed class LibraryWidget : ComponentBase
            {
                [Parameter, ECMAScriptName("data-title")]
                public string Title { get; set; } = string.Empty;

                [Parameter]
                public string Value { get; set; } = string.Empty;

                [Parameter, ECMAScriptName("onUpdate:value")]
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
        var value = GetDeclaredProperty(component, "Value");
        var valueChanged = GetDeclaredProperty(component, "ValueChanged");
        var onSave = GetDeclaredProperty(component, "OnSave");
        var externalChanged = GetDeclaredProperty(component, "ExternalChanged");
        var dismissed = GetDeclaredProperty(component, "Dismissed");
        var changedWithoutModel = GetDeclaredProperty(component, "ChangedWithoutModel");

        Assert.AreEqual("data-title", LibraryComponentConventions.GetPropRuntimeName(title));
        Assert.AreEqual("Value", LibraryComponentConventions.GetPropRuntimeName(value));
        Assert.AreEqual("onUpdate:value", LibraryComponentConventions.GetEventListenerRuntimeName(component, valueChanged));
        Assert.AreEqual("OnSave", LibraryComponentConventions.GetEventListenerRuntimeName(component, onSave));
        Assert.AreEqual("ExternalChanged", LibraryComponentConventions.GetEventListenerRuntimeName(component, externalChanged));
        Assert.AreEqual("onDismiss", LibraryComponentConventions.GetEventListenerRuntimeName(component, dismissed));
        Assert.AreEqual("ChangedWithoutModel", LibraryComponentConventions.GetEventListenerRuntimeName(component, changedWithoutModel));

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
                [Parameter, ECMAScriptName("default")]
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
            "HeaderContent",
            LibraryComponentConventions.GetSlotRuntimeName(
                standard,
                effective.Single(static property => property.Name == "HeaderContent")));

        Assert.AreEqual("HeaderContent", LibraryComponentConventions.GetSlotRuntimeName(library, GetDeclaredProperty(library, "HeaderContent")));
        Assert.AreEqual("ChildContent", LibraryComponentConventions.GetSlotRuntimeName(library, GetDeclaredProperty(library, "ChildContent")));
        Assert.AreEqual("default", LibraryComponentConventions.GetSlotRuntimeName(defaultSlot, GetDeclaredProperty(defaultSlot, "DefaultContent")));
        Assert.AreEqual("ApiURLContent", LibraryComponentConventions.GetSlotRuntimeName(library, GetDeclaredProperty(library, "ApiURLContent")));

        var names = LibraryComponentConventions.BuildParameterRuntimeNameMap(library);
        Assert.AreEqual("shared", names["SharedContent"]);
        Assert.IsFalse(names.ContainsKey("HeaderContent"));
        Assert.IsFalse(names.ContainsKey("ChildContent"));
        Assert.IsFalse(names.ContainsKey("ApiURLContent"));
        Assert.IsFalse(names.ContainsKey("Shared"));

        var defaultSlotNames = LibraryComponentConventions.BuildParameterRuntimeNameMap(defaultSlot);
        Assert.AreEqual("default", defaultSlotNames["DefaultContent"]);
    }

    [TestMethod]
    public void MetadataHelpers_ClassifyParameterShapes()
    {
        var compilation = CreateCompilation(
            """
            #nullable enable
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;

            namespace Demo;

            [VueLibraryComponent("demo-components", "LibraryWidget")]
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
        var ready = GetDeclaredProperty(library, "Ready");
        var plain = GetDeclaredProperty(standard, "Plain");
        var standardValue = GetDeclaredProperty(standard, "Value");
        var standardReady = GetDeclaredProperty(standard, "Ready");
        var header = GetDeclaredProperty(standard, "HeaderContent");
        var localCallback = GetDeclaredProperty(standard, "LocalCallback");
        var localFragment = GetDeclaredProperty(standard, "LocalFragment");

        Assert.AreEqual("HeaderContent", LibraryComponentConventions.GetSlotRuntimeName(standard, header));

        Assert.IsFalse(LibraryComponentConventions.IsParameterProperty(plain));
        Assert.IsTrue(LibraryComponentConventions.IsParameterProperty(standardValue));
        Assert.IsTrue(InvokePrivate<bool>("IsEventCallback", ready.Type));
        Assert.IsFalse(InvokePrivate<bool>("IsEventCallback", localCallback.Type));
        Assert.IsFalse(InvokePrivate<bool>("IsEventCallback", genericOwner.TypeParameters.Single()));
        Assert.IsTrue(InvokePrivate<bool>("IsRenderFragment", header.Type));
        Assert.IsFalse(InvokePrivate<bool>("IsRenderFragment", localFragment.Type));
        Assert.IsFalse(InvokePrivate<bool>("IsRenderFragment", genericOwner.TypeParameters.Single()));

    }

    [TestMethod]
    public void MetadataHelpers_ScanDecoratedMetadataAndRejectWrongHostTypes()
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

            public sealed class EventCallback;
            public sealed class RenderFragment;
            """);
        var component = GetNamedType(compilation, "Demo.DecoratedWidget");
        var onOpen = GetDeclaredProperty(component, "OnOpen");
        var lookalikeEventCallback = GetNamedType(compilation, "Demo.EventCallback");
        var lookalikeRenderFragment = GetNamedType(compilation, "Demo.RenderFragment");
        Assert.IsTrue(LibraryComponentConventions.IsParameterProperty(onOpen));

        var stringType = compilation.GetSpecialType(SpecialType.System_String);
        Assert.IsFalse(InvokePrivate<bool>("IsEventCallback", stringType));
        Assert.IsFalse(InvokePrivate<bool>("IsRenderFragment", stringType));
        Assert.IsFalse(InvokePrivate<bool>("IsEventCallback", lookalikeEventCallback));
        Assert.IsFalse(InvokePrivate<bool>("IsRenderFragment", lookalikeRenderFragment));
    }

    [TestMethod]
    public void ActivationAndCascadeContracts_HandleNullsInheritanceAndNamedMetadata()
    {
        var compilation = CreateCompilation(
            """
            #nullable enable
            using System.Collections.Generic;
            using Microsoft.AspNetCore.Components;

            namespace Demo;

            public abstract class BaseComponent : ComponentBase
            {
                [Inject] public string InheritedService { get; set; } = string.Empty;
                [CascadingParameter(Name = "named")] public int NamedCascade { get; set; }
                [CascadingParameter(Name = " ")] public string BlankCascade { get; set; } = string.Empty;
            }

            public sealed class ChildComponent : BaseComponent
            {
                [Inject] public new string InheritedService { get; set; } = string.Empty;
                [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? Attributes { get; set; }
                [Parameter(CaptureUnmatchedValues = false)] public string? Normal { get; set; }
                [Parameter] public string? Unspecified { get; set; }
                public string Plain { get; set; } = string.Empty;
            }
            """);
        var component = GetNamedType(compilation, "Demo.ChildComponent");
        var attributes = GetDeclaredProperty(component, "Attributes");
        var normal = GetDeclaredProperty(component, "Normal");
        var unspecified = GetDeclaredProperty(component, "Unspecified");
        var plain = GetDeclaredProperty(component, "Plain");
        var inheritedInject = LibraryComponentConventions.GetEffectiveInjectProperties(component);
        var cascades = LibraryComponentConventions.GetEffectiveCascadingParameterProperties(component);

        Assert.IsTrue(LibraryComponentConventions.CapturesUnmatchedValues(attributes));
        Assert.IsFalse(LibraryComponentConventions.CapturesUnmatchedValues(normal));
        Assert.IsFalse(LibraryComponentConventions.CapturesUnmatchedValues(unspecified));
        Assert.IsFalse(LibraryComponentConventions.CapturesUnmatchedValues(plain));
        Assert.ThrowsExactly<ArgumentNullException>(() => LibraryComponentConventions.CapturesUnmatchedValues(null!));

        Assert.HasCount(1, inheritedInject);
        Assert.AreEqual("ChildComponent", inheritedInject[0].ContainingType.Name);
        Assert.IsTrue(LibraryComponentConventions.IsInjectProperty(inheritedInject[0]));
        Assert.IsFalse(LibraryComponentConventions.IsInjectProperty(plain));

        Assert.HasCount(2, cascades);
        var named = cascades.Single(static property => property.Name == "NamedCascade");
        var blank = cascades.Single(static property => property.Name == "BlankCascade");
        Assert.IsTrue(LibraryComponentConventions.IsCascadingParameterProperty(named));
        Assert.IsFalse(LibraryComponentConventions.IsCascadingParameterProperty(plain));
        Assert.AreEqual("named", LibraryComponentConventions.GetCascadingParameterName(named));
        Assert.IsNull(LibraryComponentConventions.GetCascadingParameterName(blank));
        Assert.IsNull(LibraryComponentConventions.GetCascadingParameterName(plain));
        Assert.AreEqual(
            "jazor:cascade:int:named",
            LibraryComponentConventions.GetCascadingServiceKey(named));
        Assert.AreEqual(
            "jazor:cascade:string:",
            LibraryComponentConventions.GetCascadingServiceKey(plain.Type));
        Assert.AreEqual("string", LibraryComponentConventions.GetCascadingTypeKey(plain.Type));
        Assert.ThrowsExactly<ArgumentNullException>(() => LibraryComponentConventions.GetCascadingServiceKey(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => LibraryComponentConventions.GetCascadingTypeKey(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => LibraryComponentConventions.GetInjectServiceKey(null!));
        Assert.AreEqual("jazor:service:string", LibraryComponentConventions.GetInjectServiceKey(inheritedInject[0]));
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
