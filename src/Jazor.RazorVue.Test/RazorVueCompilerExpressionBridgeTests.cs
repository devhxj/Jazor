using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RenderTree;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Test;

[TestClass]
public sealed class RazorVueCompilerExpressionBridgeTests
{
    [TestMethod]
    public void RazorVue_Pipeline_LowersWhitelistedTemplateExpressions_UsingCompilerSemantics()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

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
                [ECMAScript.ECMAScriptModule("./components/expression-card")]
                public class ExpressionCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public string RawDate { get; set; } = "2024-01-02";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Math.Abs(Value));
                        builder.AddContent(2, DateOnly.Parse(RawDate).ToString());
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "Math.abs(props.value)");
        StringAssert.Contains(artifact.ModuleCode, "from \"System/DateOnlyModule.js\";");
        StringAssert.Contains(artifact.ModuleCode, "(props.rawDate)");
        CollectionAssert.Contains(artifact.Imports.ToArray(), "System/DateOnlyModule.js");
    }

    [TestMethod]
    public void RazorVue_SfcPipeline_CarriesCompilerImports_ForTemplateExpressions()
    {
        var context = CreateContext(
            """
            using System;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

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
                [ECMAScript.ECMAScriptModule("./components/expression-card")]
                public class ExpressionCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public string RawDate { get; set; } = "2024-01-02";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, Math.Abs(Value));
                        builder.AddContent(2, DateOnly.Parse(RawDate).ToString());
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVueSfcPipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ScriptSetupText, "from \"System/DateOnlyModule.js\";");
        StringAssert.Contains(artifact.ScriptSetupText, "Math.abs(props.value)");
        StringAssert.Contains(artifact.ScriptSetupText, "(props.rawDate)");
        CollectionAssert.Contains(artifact.Imports.ToArray(), "System/DateOnlyModule.js");
    }

    [TestMethod]
    public void RazorVue_Pipeline_LowersStaticObjectEquals_InLifecyclePayload_UsingCompilerSemantics()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

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
                [ECMAScript.ECMAScriptModule("./components/expression-card")]
                public class ExpressionCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(object.Equals(firstRender, true));
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ModuleCode, "await emit(\"readyChanged\", currentFirstRender === true);");
    }

    [TestMethod]
    public void RazorVue_SfcPipeline_LowersLocalAliasFirstRender_InLifecyclePayload_UsingCompilerSemantics()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

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
                [ECMAScript.ECMAScriptModule("./components/expression-card")]
                public class ExpressionCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        var alias = firstRender;
                        return ReadyChanged.InvokeAsync(alias);
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVueSfcPipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ScriptSetupText, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ScriptSetupText, "firstRender = false;");
        StringAssert.Contains(artifact.ScriptSetupText, "await emit(\"readyChanged\", currentFirstRender);");
    }

    [TestMethod]
    public void RazorVue_SfcPipeline_LowersNullableBoolAliasCoalescedFirstRender_InLifecyclePayload_UsingCompilerSemantics()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

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
                [ECMAScript.ECMAScriptModule("./components/expression-card")]
                public class ExpressionCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        bool? alias = firstRender;
                        return ReadyChanged.InvokeAsync(alias ?? false);
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVueSfcPipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ScriptSetupText, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ScriptSetupText, "firstRender = false;");
        StringAssert.Contains(artifact.ScriptSetupText, "await emit(\"readyChanged\", currentFirstRender ?? false);");
    }

    [TestMethod]
    public void RazorVue_SfcPipeline_LowersDeclarationPatternFirstRender_InLifecyclePayload_UsingCompilerSemantics()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

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
                [ECMAScript.ECMAScriptModule("./components/expression-card")]
                public class ExpressionCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(firstRender is bool ready && ready);
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVueSfcPipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ScriptSetupText, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ScriptSetupText, "firstRender = false;");
        StringAssert.Contains(artifact.ScriptSetupText, "let ready;");
        StringAssert.Contains(artifact.ScriptSetupText, "typeof currentFirstRender === \"boolean\"");
        StringAssert.Contains(artifact.ScriptSetupText, "(ready = currentFirstRender, true)");
        StringAssert.Contains(artifact.ScriptSetupText, "&& ready");
        StringAssert.Contains(artifact.ScriptSetupText, "await emit(\"readyChanged\", (() => {");
    }

    [TestMethod]
    public void RazorVue_SfcPipeline_LowersArrayIndexerFirstRenderCarrier_InLifecyclePayload_UsingCompilerSemantics()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

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
                [ECMAScript.ECMAScriptModule("./components/expression-card")]
                public class ExpressionCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        var readyStates = new[] { false, firstRender };
                        return ReadyChanged.InvokeAsync(readyStates[1]);
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVueSfcPipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ScriptSetupText, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ScriptSetupText, "firstRender = false;");
        StringAssert.Contains(artifact.ScriptSetupText, "const __jazorLifecycleLocal");
        StringAssert.Contains(artifact.ScriptSetupText, "[false, currentFirstRender]");
        StringAssert.Contains(artifact.ScriptSetupText, "await emit(\"readyChanged\", __jazorLifecycleLocal");
    }

    [TestMethod]
    public void RazorVue_SfcPipeline_LowersArrayPatternFirstRenderCarrier_InLifecyclePayload_UsingCompilerSemantics()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

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
                [ECMAScript.ECMAScriptModule("./components/expression-card")]
                public class ExpressionCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        var readyStates = new[] { false, firstRender };
                        var payload = readyStates is [_, var ready] ? ready : false;
                        return ReadyChanged.InvokeAsync(payload);
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVueSfcPipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ScriptSetupText, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ScriptSetupText, "firstRender = false;");
        StringAssert.Contains(artifact.ScriptSetupText, "const __jazorLifecycleLocal");
        StringAssert.Contains(artifact.ScriptSetupText, "Array.isArray(__jazorLifecycleLocal");
        StringAssert.Contains(artifact.ScriptSetupText, "await emit(\"readyChanged\", __jazorLifecycleLocal");
    }

    [TestMethod]
    public void RazorVue_SfcPipeline_LowersSwitchExpressionFirstRender_InLifecyclePayload_UsingCompilerSemantics()
    {
        var context = CreateContext(
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

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
                [ECMAScript.ECMAScriptModule("./components/expression-card")]
                public class ExpressionCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(firstRender switch
                        {
                            true => true,
                            false => false,
                        });
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """);

        var artifact = new RazorVueSfcPipeline(BuildRenderTreeTemplateFrontend.Instance)
            .Execute(context)
            .Artifacts
            .Single();

        StringAssert.Contains(artifact.ScriptSetupText, "const currentFirstRender = firstRender;");
        StringAssert.Contains(artifact.ScriptSetupText, "firstRender = false;");
        StringAssert.Contains(artifact.ScriptSetupText, "const __swexpr$");
        StringAssert.Contains(artifact.ScriptSetupText, "=== true");
        StringAssert.Contains(artifact.ScriptSetupText, "=== false");
        StringAssert.Contains(artifact.ScriptSetupText, "await emit(\"readyChanged\", (() => {");
    }

    [TestMethod]
    public void RazorVue_ExpressionEmitter_EmitSetupExpression_LowersStaticObjectEquals_UsingCompilerSemantics()
    {
        const string source =
            """
            using System;
            using System.Threading.Tasks;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

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
                [ECMAScript.ECMAScriptModule("./components/expression-card")]
                public class ExpressionCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public EventCallback<bool> ReadyChanged { get; set; }

                    protected override Task OnAfterRenderAsync(bool firstRender)
                    {
                        return ReadyChanged.InvokeAsync(object.Equals(firstRender, true));
                    }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "section");
                        builder.AddContent(1, "ready");
                        builder.CloseElement();
                    }
                }
            }
            """;

        var context = CreateContext(source);
        var snapshot = context.CreateSemanticSnapshots().Single();
        var syntaxTree = context.Compilation.SyntaxTrees.Last();
        var semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
        var invocation = syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Single(static node => string.Equals(node.ToString(), "object.Equals(firstRender, true)", StringComparison.Ordinal));
        var operation = semanticModel.GetOperation(invocation) as IInvocationOperation;

        Assert.IsNotNull(operation);
        var emitter = new RazorVueExpressionEmitter(snapshot);
        var expression = emitter.EmitSetupExpression(operation!);

        Assert.AreEqual("firstRender === true", expression);
    }

    private static RazorVueCompilationContext CreateContext(string source)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorVue.ExpressionBridge.Tests",
            syntaxTrees: RazorVueMetadataReferences.CreateSyntaxTrees(source),
            references: RazorVueMetadataReferences.Create(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static diagnostic => diagnostic.ToString())));

        var context = RazorVueCompilationContext.TryCreate(compilation);
        Assert.IsNotNull(context);
        return context;
    }
}
