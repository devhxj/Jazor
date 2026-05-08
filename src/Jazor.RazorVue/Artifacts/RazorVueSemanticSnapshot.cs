using System.Collections.Immutable;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;

namespace Jazor.RazorVue.Artifacts;

/// <summary>
/// RazorVue 编译主链路使用的最小语义快照。
///
/// 这里同时保留两层信息：
/// 1. descriptor/flag 视图（VueLifecycleDescriptor）供 HMR/hash/诊断等阶段使用，不依赖 Roslyn symbol。
/// 2. lifecycle method symbols，供 lowering 阶段在 setup() 中生成 Vue hooks（onMounted/onUpdated/watch 等），
///    避免 lowering 阶段再次重新发现。当方法体是 no-op 或 emit 调用时，
///    lowering 会展开实际的 Vue hook 表达式；否则明确抛异常（不做静默降级）。
/// </summary>
internal sealed record RazorVueSemanticSnapshot(
    Compilation Compilation,
    INamedTypeSymbol ComponentSymbol,
    IMethodSymbol? BuildRenderTreeMethod,
    RazorVueRazorIrCarrier? RazorIrCarrier,
    RazorVueRazorSourceGeneratorDocument? RazorSourceGeneratorDocument,
    VueLifecycleDescriptor Lifecycle,
    VueLogicDescriptor Logic,
    VueComponentDescriptor Descriptor,
    ImmutableArray<RazorVueSourceOrigin> Origins,
    ImmutableArray<string> ImportedNamespaces,
    // ---- Lifecycle method symbol carriers (for lowering) ----
    // 只携带当前 lowering 支持的安全子集；其他 lifecycle 以 bool flag 为准。
    IMethodSymbol? OnInitializedMethod = null,
    IMethodSymbol? OnInitializedAsyncMethod = null,
    IMethodSymbol? OnParametersSetMethod = null,
    IMethodSymbol? OnParametersSetAsyncMethod = null,
    IMethodSymbol? ShouldRenderMethod = null,
    IMethodSymbol? SetParametersAsyncMethod = null,
    IMethodSymbol? OnAfterRenderMethod = null,
    IMethodSymbol? OnAfterRenderAsyncMethod = null,
    IMethodSymbol? DisposeMethod = null,
    IMethodSymbol? DisposeAsyncMethod = null);
