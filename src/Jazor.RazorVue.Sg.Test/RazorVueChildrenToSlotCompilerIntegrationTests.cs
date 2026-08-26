using Jazor.Compiler;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorVueChildrenToSlotCompilerIntegrationTests
{
    [TestMethod]
    public async Task Convert_ClassUsingVueBuiltInComponents_GeneratesPlainVueImportsAndProps()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue;

            namespace Demo
            {
                [ECMAScriptModule("components/builtins.mjs")]
                public static class BuiltInsModule
                {
                    public static IVNode Render()
                    {
                        var content = H("main", "content");
                        var transitioned = H(Vue.Transition, new VueTransitionProps
                        {
                            Name = "fade",
                            Mode = VueTransitionMode.OutIn,
                            Appear = true
                        }, content);
                        var teleported = H(Vue.Teleport, new VueTeleportProps
                        {
                            To = "#modal",
                            Disabled = false,
                            Defer = true
                        }, transitioned);
                        var kept = H(Vue.KeepAlive, new VueKeepAliveProps
                        {
                            Include = "Panel",
                            Max = 2
                        }, teleported);
                        return H(Vue.Suspense, new VueSuspenseProps
                        {
                            Timeout = 1000,
                            OnFallback = OnFallback
                        }, new VueSuspenseSlots
                        {
                            Default = () => kept,
                            Fallback = Loading
                        });
                    }

                    private static IVNode Loading()
                        => H("span", "loading");

                    private static void OnFallback()
                    {
                    }
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "BuiltInsModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "BuiltInsModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { KeepAlive, Suspense, Teleport, Transition, h } from ""vue"";
export function Render() {
  let content = h(""main"", ""content"");
  let transitioned = ((__component, __props, __slot0) => h(__component, __props, { default: () => __slot0 }))(Transition, {
    name: ""fade"",
    mode: ""out-in"",
    appear: true
  }, content);
  let teleported = ((__component, __props, __slot0) => h(__component, __props, { default: () => __slot0 }))(Teleport, {
    to: ""#modal"",
    disabled: false,
    defer: true
  }, transitioned);
  let kept = ((__component, __props, __slot0) => h(__component, __props, { default: () => __slot0 }))(KeepAlive, { include: ""Panel"", max: 2 }, teleported);
  return h(Suspense, { timeout: 1000, onFallback: OnFallback }, { default: () => {
    return kept;
  }, fallback: Loading });
}
function Loading() {
  return h(""span"", ""loading"");
}
function OnFallback() { }
", script);
    }


    [TestMethod]
    public async Task Convert_ClassUsingVueHComponentSingleVNodeChild_GeneratesDefaultSlotSugar()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue;

            namespace Demo
            {
                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback Default { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueSlotComponent<ChildSlots> Child = Vue.DefineComponent(new VueSlotComponentOptions<ChildSlots>
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render(IVNode child)
                        => H(Child, child);
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""vue"";
export let Child = defineComponent({ name: ""ChildView"" });
export function Render(child) {
  return ((__component, __slot0) => h(__component, { default: () => __slot0 }))(Child, child);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }


    [TestMethod]
    public async Task Convert_ClassUsingVueHComponentPropsAndSingleVNodeChild_GeneratesDefaultSlotSugar()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue;

            namespace Demo
            {
                public sealed record ChildProps : VueProps
                {
                    [Description("@#title")]
                    public string? Title { get; init; }
                }

                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback Default { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent<ChildProps, ChildSlots> Child = Vue.DefineComponent(new VueComponentOptions<ChildProps, ChildSlots>
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render(IVNode child)
                        => H(Child, new ChildProps { Title = "Welcome" }, child);
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""vue"";
export let Child = defineComponent({ name: ""ChildView"" });
export function Render(child) {
  return ((__component, __props, __slot0) => h(__component, __props, { default: () => __slot0 }))(Child, { title: ""Welcome"" }, child);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }


    [TestMethod]
    public async Task Convert_ClassUsingVueHComponentInvocationArguments_PreservesSingleEvaluationOrder()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue;

            namespace Demo
            {
                public sealed record ChildProps : VueProps
                {
                    [Description("@#title")]
                    public string? Title { get; init; }
                }

                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback Default { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVNode Render()
                        => H(GetChild(), CreateProps(), RenderChild());

                    private static IVueComponent<ChildProps, ChildSlots> GetChild()
                        => Vue.DefineComponent(new VueComponentOptions<ChildProps, ChildSlots>
                        {
                            Name = "ChildView"
                        });

                    private static ChildProps CreateProps()
                        => new ChildProps
                        {
                            Title = "Welcome"
                        };

                    private static IVNode RenderChild()
                        => H("span", "body");
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""vue"";
export function Render() {
  return ((__component, __props, __slot0) => h(__component, __props, { default: () => __slot0 }))(GetChild(), CreateProps(), RenderChild());
}
function GetChild() {
  return defineComponent({ name: ""ChildView"" });
}
function CreateProps() {
  return { title: ""Welcome"" };
}
function RenderChild() {
  return h(""span"", ""body"");
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }


    [TestMethod]
    public async Task Convert_ClassUsingVueHComponentTextChild_GeneratesDefaultSlotSugar()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue;

            namespace Demo
            {
                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback Default { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueSlotComponent<ChildSlots> Child = Vue.DefineComponent(new VueSlotComponentOptions<ChildSlots>
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render(string child)
                        => H(Child, child);
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""vue"";
export let Child = defineComponent({ name: ""ChildView"" });
export function Render(child) {
  return ((__component, __slot0) => h(__component, { default: () => __slot0 }))(Child, child);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }


    [TestMethod]
    public async Task Convert_ClassUsingVueHComponentLiteralTextChild_GeneratesDirectDefaultSlotObject()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue;

            namespace Demo
            {
                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback Default { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueSlotComponent<ChildSlots> Child = Vue.DefineComponent(new VueSlotComponentOptions<ChildSlots>
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render()
                        => H(Child, "body");
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""vue"";
export let Child = defineComponent({ name: ""ChildView"" });
export function Render() {
  return h(Child, { default: () => ""body"" });
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }


    [TestMethod]
    public async Task Convert_ClassUsingVueHComponentLiteralBoolChild_GeneratesDirectDefaultSlotObject()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue;

            namespace Demo
            {
                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback Default { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueSlotComponent<ChildSlots> Child = Vue.DefineComponent(new VueSlotComponentOptions<ChildSlots>
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render()
                        => H(Child, true);
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""vue"";
export let Child = defineComponent({ name: ""ChildView"" });
export function Render() {
  return h(Child, { default: () => true });
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }


    [TestMethod]
    public async Task Convert_ClassUsingVueHComponentBoolChild_GeneratesDefaultSlotSugar()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue;

            namespace Demo
            {
                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback Default { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueSlotComponent<ChildSlots> Child = Vue.DefineComponent(new VueSlotComponentOptions<ChildSlots>
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render(bool child)
                        => H(Child, child);
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""vue"";
export let Child = defineComponent({ name: ""ChildView"" });
export function Render(child) {
  return ((__component, __slot0) => h(__component, { default: () => __slot0 }))(Child, child);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }


    [TestMethod]
    public async Task Convert_ClassUsingVueHComponentIntChild_GeneratesDefaultSlotSugar()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue;

            namespace Demo
            {
                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback Default { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueSlotComponent<ChildSlots> Child = Vue.DefineComponent(new VueSlotComponentOptions<ChildSlots>
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render(int child)
                        => H(Child, child);
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""vue"";
export let Child = defineComponent({ name: ""ChildView"" });
export function Render(child) {
  return ((__component, __slot0) => h(__component, { default: () => __slot0 }))(Child, child);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }


    [TestMethod]
    public async Task Convert_ClassUsingVueHComponentPropsAndArrayChildren_GeneratesDefaultSlotSugar()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue;

            namespace Demo
            {
                public sealed record ChildProps : VueProps
                {
                    [Description("@#title")]
                    public string? Title { get; init; }
                }

                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback Default { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent<ChildProps, ChildSlots> Child = Vue.DefineComponent(new VueComponentOptions<ChildProps, ChildSlots>
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render()
                        => H(Child, new ChildProps { Title = "Welcome" }, new IVNode[]
                        {
                            H("span", "a"),
                            H("span", "b")
                        });
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""vue"";
export let Child = defineComponent({ name: ""ChildView"" });
export function Render() {
  return ((__component, __props, __slot0) => h(__component, __props, { default: () => __slot0 }))(Child, { title: ""Welcome"" }, [h(""span"", ""a""), h(""span"", ""b"")]);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }


    [TestMethod]
    public async Task Convert_ClassUsingTypedVueSlotOnlyComponentSingleVNodeChild_GeneratesDefaultSlotSugar()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue;

            namespace Demo
            {
                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback ChildContent { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueSlotComponent<ChildSlots> Child = Vue.DefineComponent(new VueSlotComponentOptions<ChildSlots>
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render(IVNode child)
                        => H(Child, child);
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""vue"";
export let Child = defineComponent({ name: ""ChildView"" });
export function Render(child) {
  return ((__component, __slot0) => h(__component, { default: () => __slot0 }))(Child, child);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }


    [TestMethod]
    public async Task Convert_ClassUsingVueLikeHostHContract_GeneratesDefaultSlotSugarWithoutVueRuntimeTypeName()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;

            namespace Demo
            {
                [ECMAScript("vue")]
                [Description("@#")]
                public static class VueLikeHost
                {
                    [ECMAScript]
                    [Description("@#")]
                    public interface IVNode;

                    [ECMAScript]
                    [Description("@#")]
                    public interface IVueSlotComponent<TSlots>
                        where TSlots : VueSlots;

                    [ECMAScript]
                    [Description("@#")]
                    public abstract record VueSlots;

                    public delegate IVNode VueSlotCallback();

                    [Description("@#h")]
                    public static extern IVNode H<TSlots>(IVueSlotComponent<TSlots> component, IVNode child)
                        where TSlots : VueSlots;
                }

                public sealed record ChildSlots : VueLikeHost.VueSlots
                {
                    [Description("@#default")]
                    public VueLikeHost.VueSlotCallback ChildContent { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static VueLikeHost.IVNode Render(VueLikeHost.IVueSlotComponent<ChildSlots> component, VueLikeHost.IVNode child)
                        => VueLikeHost.H(component, child);
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { h } from ""vue"";
export function Render(component, child) {
  return ((__component, __slot0) => h(__component, { default: () => __slot0 }))(component, child);
}
", script);
    }


    [TestMethod]
    public async Task Convert_ClassUsingVueLikeHostTypedComponentHContract_GeneratesPropsDefaultSlotSugarWithoutVueRuntimeTypeName()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;

            namespace Demo
            {
                [ECMAScript("vue")]
                [Description("@#")]
                public static class VueLikeHost
                {
                    [ECMAScript]
                    [Description("@#")]
                    public interface IVNode;

                    [ECMAScript]
                    [Description("@#")]
                    public interface IVueComponent<TProps, TSlots>
                        where TProps : VueProps
                        where TSlots : VueSlots;

                    [ECMAScript]
                    [Description("@#")]
                    public abstract record VueProps;

                    [ECMAScript]
                    [Description("@#")]
                    public abstract record VueSlots;

                    public delegate IVNode VueSlotCallback();

                    [Description("@#h")]
                    public static extern IVNode H<TProps, TSlots>(IVueComponent<TProps, TSlots> component, TProps props, IVNode child)
                        where TProps : VueProps
                        where TSlots : VueSlots;
                }

                public sealed record ChildProps : VueLikeHost.VueProps
                {
                    [Description("@#title")]
                    public string? Title { get; init; }
                }

                public sealed record ChildSlots : VueLikeHost.VueSlots
                {
                    [Description("@#default")]
                    public VueLikeHost.VueSlotCallback ChildContent { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static VueLikeHost.IVNode Render(
                        VueLikeHost.IVueComponent<ChildProps, ChildSlots> component,
                        VueLikeHost.IVNode child)
                        => VueLikeHost.H(component, new ChildProps { Title = "Welcome" }, child);
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { h } from ""vue"";
export function Render(component, child) {
  return ((__component, __props, __slot0) => h(__component, __props, { default: () => __slot0 }))(component, { title: ""Welcome"" }, child);
}
", script);
    }


    [TestMethod]
    public async Task Convert_ClassUsingVueLikeHostDefaultSlotDelegateContract_DoesNotRequireVueSlotCallbackName()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;

            namespace Demo
            {
                [ECMAScript("vue")]
                [Description("@#")]
                public static class VueLikeHost
                {
                    [ECMAScript]
                    [Description("@#")]
                    public interface IVNode;

                    [ECMAScript]
                    [Description("@#")]
                    public interface IVueSlotComponent<TSlots>
                        where TSlots : VueSlots;

                    [ECMAScript]
                    [Description("@#")]
                    public abstract record VueSlots;

                    public delegate IVNode RenderSlot();

                    [Description("@#h")]
                    public static extern IVNode H<TSlots>(IVueSlotComponent<TSlots> component, IVNode child)
                        where TSlots : VueSlots;
                }

                public sealed record ChildSlots : VueLikeHost.VueSlots
                {
                    [Description("@#default")]
                    public VueLikeHost.RenderSlot ChildContent { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static VueLikeHost.IVNode Render(VueLikeHost.IVueSlotComponent<ChildSlots> component, VueLikeHost.IVNode child)
                        => VueLikeHost.H(component, child);
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        AssertScriptEqual(
@"import { h } from ""vue"";
export function Render(component, child) {
  return ((__component, __slot0) => h(__component, { default: () => __slot0 }))(component, child);
}
", script);
    }


    [TestMethod]
    public async Task Convert_ClassUsingTypedVueSlotOnlyComponentStringChild_GeneratesDefaultSlotSugar()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue;

            namespace Demo
            {
                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback ChildContent { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueSlotComponent<ChildSlots> Child = Vue.DefineComponent(new VueSlotComponentOptions<ChildSlots>
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render(string child)
                        => H(Child, child);
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""vue"";
export let Child = defineComponent({ name: ""ChildView"" });
export function Render(child) {
  return ((__component, __slot0) => h(__component, { default: () => __slot0 }))(Child, child);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }


    [TestMethod]
    public async Task Convert_ClassUsingTypedVueComponentPropsAndBoolChild_GeneratesDefaultSlotSugar()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue;

            namespace Demo
            {
                public sealed record ChildProps : VueProps
                {
                    [Description("@#title")]
                    public string? Title { get; init; }
                }

                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback ChildContent { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent<ChildProps, ChildSlots> Child = Vue.DefineComponent(new VueComponentOptions<ChildProps, ChildSlots>
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render(bool child)
                        => H(Child, new ChildProps { Title = "Welcome" }, child);
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""vue"";
export let Child = defineComponent({ name: ""ChildView"" });
export function Render(child) {
  return ((__component, __props, __slot0) => h(__component, __props, { default: () => __slot0 }))(Child, { title: ""Welcome"" }, child);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }


    [TestMethod]
    public async Task Convert_ClassUsingTypedVueComponentSingleVNodeChild_GeneratesDefaultSlotSugar()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue;

            namespace Demo
            {
                public sealed record ChildProps : VueProps
                {
                    [Description("@#title")]
                    public string? Title { get; init; }
                }

                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback ChildContent { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent<ChildProps, ChildSlots> Child = Vue.DefineComponent(new VueComponentOptions<ChildProps, ChildSlots>
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render(IVNode child)
                        => H(Child, new ChildProps { Title = "Welcome" }, child);
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""vue"";
export let Child = defineComponent({ name: ""ChildView"" });
export function Render(child) {
  return ((__component, __props, __slot0) => h(__component, __props, { default: () => __slot0 }))(Child, { title: ""Welcome"" }, child);
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }


    [TestMethod]
    public async Task Convert_ClassUsingTypedVueSlotOnlyComponentSingleVNodeChildWithoutDefaultSlot_ThrowsOperationTransformationException()
    {
        var code = """
            using ECMAScript;
            using static ECMAScript.Vue;

            namespace Demo
            {
                public sealed record ChildSlots : VueSlots
                {
                    public VueSlotCallback Header { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueSlotComponent<ChildSlots> Child = Vue.DefineComponent(new VueSlotComponentOptions<ChildSlots>
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render(IVNode child)
                        => H(Child, child);
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var exception = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);
        StringAssert.Contains(exception.Message, "does not declare an explicit default slot");
        StringAssert.Contains(exception.Message, "Description(\"@#default\")");
    }


    [TestMethod]
    public async Task Convert_ClassUsingTypedVueSlotOnlyComponentSingleVNodeChildWithScopedDefaultSlot_ThrowsOperationTransformationException()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue;

            namespace Demo
            {
                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback<string> ChildContent { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueSlotComponent<ChildSlots> Child = Vue.DefineComponent(new VueSlotComponentOptions<ChildSlots>
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render(IVNode child)
                        => H(Child, child);
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var exception = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);
        StringAssert.Contains(exception.Message, "expects slot scope");
        StringAssert.Contains(exception.Message, "explicit slot callback");
    }


    [TestMethod]
    public async Task Convert_ClassUsingTypedVueSlotOnlyComponentSingleVNodeChildWithDuplicateDefaultSlot_ThrowsOperationTransformationException()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue;

            namespace Demo
            {
                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback Default { get; init; } = default!;

                    [Description("@#default")]
                    public VueSlotCallback ChildContent { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueSlotComponent<ChildSlots> Child = Vue.DefineComponent(new VueSlotComponentOptions<ChildSlots>
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render(IVNode child)
                        => H(Child, child);
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var exception = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);
        StringAssert.Contains(exception.Message, "more than one explicit default slot");
        StringAssert.Contains(exception.Message, "Only one property may map to 'default'");
    }


    [TestMethod]
    public async Task Convert_ClassUsingTypedVueSlotOnlyComponentSingleVNodeChildWithNonVNodeDefaultSlotDelegate_ThrowsOperationTransformationException()
    {
        var code = """
            using System;
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue;

            namespace Demo
            {
                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public Func<string> ChildContent { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueSlotComponent<ChildSlots> Child = Vue.DefineComponent(new VueSlotComponentOptions<ChildSlots>
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render(IVNode child)
                        => H(Child, child);
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var exception = await Assert.ThrowsAsync<OperationTransformationException>(converter.Convert);
        StringAssert.Contains(exception.Message, "must be a delegate returning the host IVNode type");
        StringAssert.Contains(exception.Message, "Default slot member");
    }


    [TestMethod]
    public async Task Convert_ClassUsingTypedVueComponentPropsAndLiteralIntChild_GeneratesDirectDefaultSlotObject()
    {
        var code = """
            using System.ComponentModel;
            using ECMAScript;
            using static ECMAScript.Vue;

            namespace Demo
            {
                public sealed record ChildProps : VueProps
                {
                    [Description("@#title")]
                    public string? Title { get; init; }
                }

                public sealed record ChildSlots : VueSlots
                {
                    [Description("@#default")]
                    public VueSlotCallback ChildContent { get; init; } = default!;
                }

                [ECMAScriptModule("components/panel.mjs")]
                public static class PanelModule
                {
                    public static IVueComponent<ChildProps, ChildSlots> Child = Vue.DefineComponent(new VueComponentOptions<ChildProps, ChildSlots>
                    {
                        Name = "ChildView"
                    });

                    public static IVNode Render()
                        => H(Child, new ChildProps { Title = "Welcome" }, 1);
                }
            }
            """;

        var (_, semanticModel) = CompileAndGetSymbol(
            code,
            "PanelModule",
            MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Contract.IUIComponent).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ECMAScript.Vue).Assembly.Location));
        var moduleSymbol = semanticModel.SyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(static x => x.Identifier.Text == "PanelModule")
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .OfType<INamedTypeSymbol>()
            .Single();

        var converter = CreateChildrenToSlotConverter(moduleSymbol, semanticModel);
        var module = await converter.Convert();
        var script = module?.ToKnRECMAScript();

        Assert.AreEqual(
@"import { defineComponent, h } from ""vue"";
export let Child = defineComponent({ name: ""ChildView"" });
export function Render() {
  return h(Child, { title: ""Welcome"" }, { default: () => 1 });
}
".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
    }

    private static AstConverter CreateChildrenToSlotConverter(
        INamedTypeSymbol moduleSymbol,
        SemanticModel semanticModel)
        => new(
            moduleSymbol,
            semanticModel,
            new AstConverterOptions(
                AstConverterProfile.Standard,
                Host: ChildrenToSlotSemanticWalkerHost.Instance));

    private static void AssertScriptEqual(string expected, string? actual)
        => Assert.AreEqual(expected.ReplaceLineEndings("\\n"), actual?.ReplaceLineEndings("\\n"));

    private static (INamedTypeSymbol, SemanticModel) CompileAndGetSymbol(
        string code,
        string className,
        params MetadataReference[] additionalReferences)
    {
        var references = TestMetadataReferences.Net11.AddRange(additionalReferences).ToList();
        if (references.Any(static reference =>
                string.Equals(reference.Display, typeof(ECMAScript.Vue).Assembly.Location, StringComparison.OrdinalIgnoreCase)))
        {
            references.Add(MetadataReference.CreateFromFile(
                typeof(ECMAScript.ECMAScriptAttribute).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(
                typeof(ECMAScript.VueContract.VueInjectAttribute).Assembly.Location));
            references.Add(MetadataReference.CreateFromFile(
                typeof(ECMAScript.Vue.IVueComponent).Assembly.Location));
        }

        var syntaxTrees = new List<SyntaxTree>
        {
            CSharpSyntaxTree.ParseText(
                "global using ECMAScript.VueContract;",
                TestMetadataReferences.PreviewParseOptions,
                path: "__TestGlobalUsings.cs"),
            CSharpSyntaxTree.ParseText(code, TestMetadataReferences.PreviewParseOptions)
        };
        var compilation = CSharpCompilation.Create(
            "RazorVue.ChildrenToSlot.Tests",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var diagnostics = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.IsFalse(
            diagnostics.Length > 0,
            string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString())));

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var classDeclaration = syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(candidate => candidate.Identifier.ValueText == className);
            if (classDeclaration is null)
                continue;

            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
            Assert.IsNotNull(classSymbol);
            return (classSymbol, semanticModel);
        }

        throw new InvalidOperationException($"Class '{className}' was not found.");
    }
}
