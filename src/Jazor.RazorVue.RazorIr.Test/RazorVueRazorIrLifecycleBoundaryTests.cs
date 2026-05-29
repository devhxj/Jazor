using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RazorSdk;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorVueRazorIrLifecycleBoundaryTests
{
    [TestMethod]
    public void RazorVuePipeline_WithSourceStablePrivateLifecycleHelper_LowersWithoutRuntimeHook()
    {
        var artifact = LowerPipeline(
            """
            <section>@Ready</section>

            @code {
                protected override void OnInitialized()
                {
                    var ready = Normalize(Ready);
                    return;
                }

                private bool Normalize(bool ready)
                    => ready;
            }
            """);

        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
        Assert.IsFalse(artifact.ModuleCode.Contains("onMounted", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("function normalize", StringComparison.Ordinal), artifact.ModuleCode);
        StringAssert.Contains(artifact.ModuleCode, "props.ready");
    }

    [TestMethod]
    public void RazorVuePipeline_WithLifecycleHelperValueTaskReturn_ThrowsUnsupportedLifecycleLowering()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            LowerPipeline(
                """
                <section>@Ready</section>

                @code {
                    protected override void OnInitialized()
                    {
                        Touch();
                    }

                    private void Touch() => GetOperation();

                    private ValueTask GetOperation()
                    {
                        return ValueTask.CompletedTask;
                    }
                }
                """));

        AssertUnsupportedLifecycle(exception, "OnInitialized");
    }

    [TestMethod]
    public void RazorVuePipeline_WithLifecycleHelperInParameter_LowersWithoutRuntimeHook()
    {
        var artifact = LowerPipeline(
            """
            <section>@Ready</section>

            @code {
                protected override void OnInitialized()
                {
                    var ready = Ready;
                    Touch(in ready);
                }

                private void Touch(in bool ready)
                {
                    var label = ready;
                }
            }
            """);

        Assert.AreEqual(HmrBoundaryKind.TemplateOnly, artifact.Identity.HmrBoundaryKind);
        Assert.IsFalse(artifact.ModuleCode.Contains("onMounted", StringComparison.Ordinal), artifact.ModuleCode);
        Assert.IsFalse(artifact.ModuleCode.Contains("function touch", StringComparison.Ordinal), artifact.ModuleCode);
        StringAssert.Contains(artifact.ModuleCode, "props.ready");
    }

    [TestMethod]
    public void RazorVuePipeline_WithSetParametersAsyncMutationAfterBase_RequiresFullReload()
    {
        var artifact = LowerPipeline(
            """
            <section>@Value</section>

            @code {
                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    Value++;
                }
            }
            """);

        Assert.AreEqual(HmrBoundaryKind.FullReloadRequired, artifact.Identity.HmrBoundaryKind);
        Assert.IsFalse(artifact.ModuleCode.Contains("watch(", StringComparison.Ordinal), artifact.ModuleCode);
    }

    [TestMethod]
    public void RazorVuePipeline_WithShouldRenderDelegateEscape_RequiresFullReload()
    {
        var artifact = LowerPipeline(
            """
            <section>@Value</section>

            @code {
                private Func<int, bool>? _cached;

                protected override bool ShouldRender()
                {
                    Func<int, bool> ready = value => value > 0;
                    _cached = ready;
                    return ready(Value);
                }
            }
            """);

        Assert.AreEqual(HmrBoundaryKind.FullReloadRequired, artifact.Identity.HmrBoundaryKind);
        Assert.IsFalse(artifact.ModuleCode.Contains("__jazorShouldRenderGate", StringComparison.Ordinal), artifact.ModuleCode);
    }

    [TestMethod]
    public void RazorVuePipeline_WithSupportedSetParametersAsyncEmit_LowersWatch()
    {
        var artifact = LowerPipeline(
            """
            <section>@Value</section>

            @code {
                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                    await ValueChanged.InvokeAsync(Value);
                }
            }
            """);

        Assert.AreEqual(HmrBoundaryKind.LogicSafe, artifact.Identity.HmrBoundaryKind);
        StringAssert.Contains(artifact.ModuleCode, "watch(() => [props.ready, props.value], async () => {");
        StringAssert.Contains(artifact.ModuleCode, "await emit(\"update:value\", props.value);");
    }

    private static VueCompiledArtifact LowerPipeline(string documentText)
    {
        var (context, snapshot) = CreateContext(documentText);
        return RazorVueRazorIrTestContextFactory.CreateSgPipeline(snapshot)
            .Execute(context)
            .Artifacts
            .Single();
    }

    private static void AssertUnsupportedLifecycle(RazorVueCompilationIssueException exception, string lifecycleName)
    {
        Assert.AreEqual(RazorVueIssueCode.UnsupportedLifecycleLowering, exception.Issue.Code);
        StringAssert.Contains(exception.Message, lifecycleName);
        Assert.AreEqual("Demo.Pages.TodoApp", exception.OwnerComponentFullName);
    }

    private static (RazorVueCompilationContext Context, RazorVueSemanticSnapshot Snapshot) CreateContext(
        string documentText)
        => RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.LifecycleBoundary.Tests",
            @"D:\repo\Demo\Pages\TodoApp.razor",
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool Ready { get; set; }

                    [Parameter]
                    public int Value { get; set; }

                    [Parameter]
                    public EventCallback<int> ValueChanged { get; set; }
                }
            }
            """,
            """
            @using Demo.Pages
            @using System
            @using System.Threading.Tasks
            """);
}
