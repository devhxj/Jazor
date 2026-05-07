using System.Collections.Immutable;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Canonical;
using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue;

namespace Jazor.RazorVue.Sfc;

internal sealed record RazorVueSfcSemanticModel(
    string ComponentName,
    string ComponentFullName,
    string RelativeSfcPath,
    VueComponentDescriptor Descriptor,
    ImmutableArray<string> Imports,
    ImmutableArray<RazorVueCompilerImportBinding> CompilerImports,
    ImmutableArray<RazorVueSfcComponentImport> ComponentImports,
    ImmutableArray<string> Styles,
    ImmutableArray<string> PluginRequirements,
    VueRuntimeHints Hints,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins,
    RazorVueSfcTemplateBlockModel TemplateBlock,
    RazorVueSfcScriptSetupBlockModel ScriptSetupBlock,
    ImmutableArray<RazorVueSfcStyleBlockModel> StyleBlocks,
    ImmutableArray<RazorVueSfcCustomBlockModel> CustomBlocks);

internal sealed record RazorVueSfcTemplateBlockModel(
    RazorVueCanonicalTemplateFragment Template,
    ImmutableArray<RazorVueSfcTemplateBindingSite> BindingSites,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins);

internal sealed record RazorVueSfcScriptSetupBlockModel(
    RazorVueCanonicalSetupModel Setup,
    ImmutableArray<RazorVueSfcSetupBinding> LiftedBindings,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins);

internal sealed record RazorVueSfcStyleBlockModel(
    string Text,
    bool IsScoped,
    string? ModuleName,
    string? Language,
    string? SourceFilePath,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins);

internal sealed record RazorVueSfcCustomBlockModel(
    string Name,
    string Text,
    string? Language,
    ImmutableArray<VueSfcAttribute> Attributes,
    string? SourceFilePath,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins);

internal sealed record RazorVueSfcSetupBinding(
    string Name,
    string ExpressionText,
    RazorVueSfcSetupBindingKind BindingKind,
    string TemplateExpressionText,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins);

internal sealed record RazorVueSfcTemplateBindingSite(
    string SitePath,
    string BindingName);

internal sealed record RazorVueSfcComponentImport(
    string ComponentKey,
    string TemplateTagName,
    string LocalBindingName,
    string? ImportSpecifier,
    string? ExportName,
    RazorVueSfcComponentImportKind ImportKind);

internal enum RazorVueSfcSetupBindingKind
{
    Helper,
    Computed,
    Method,
    LocalAlias
}

internal enum RazorVueSfcComponentImportKind
{
    None,
    Default,
    Named
}
