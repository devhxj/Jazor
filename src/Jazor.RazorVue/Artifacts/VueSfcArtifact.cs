using System.Collections.Immutable;

namespace Jazor.RazorVue.Artifacts;

internal sealed record VueSfcArtifact(
    string ComponentName,
    string RelativeSfcPath,
    string SfcText,
    VueSfcTemplateBlock TemplateBlock,
    VueSfcScriptSetupBlock ScriptSetupBlock,
    VueSfcScriptBlock ScriptBlock,
    VueSfcArtifactRenderMode RenderMode,
    ImmutableArray<VueSfcStyleBlock> StyleBlocks,
    ImmutableArray<VueSfcCustomBlock> CustomBlocks,
    ImmutableArray<string> RouteTemplates,
    ImmutableArray<string> Imports,
    ImmutableArray<string> Styles,
    ImmutableArray<string> PluginRequirements,
    VueSfcArtifactIdentity Identity,
    VueRuntimeHints Hints,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins)
{
    public string TemplateText => TemplateBlock.Text;

    public string ScriptSetupText => ScriptSetupBlock.Text;

    public string ScriptText => ScriptBlock.Text;

    public bool HasTemplateBlock => !string.IsNullOrEmpty(TemplateBlock.Text);

    public bool UsesScriptSetup => !string.IsNullOrEmpty(ScriptSetupBlock.Text);
}

internal sealed record VueSfcArtifactIdentity(
    string ComponentId,
    string ModuleId,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    string StyleHash,
    HmrBoundaryKind HmrBoundaryKind);

internal sealed record VueSfcTemplateBlock(
    string Text,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins);

internal sealed record VueSfcScriptSetupBlock(
    string Text,
    string? Language,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins);

internal sealed record VueSfcScriptBlock(
    string Text,
    string? Language,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins);

internal enum VueSfcArtifactRenderMode
{
    Template,
    RenderFunction
}

internal sealed record VueSfcStyleBlock(
    string Text,
    bool IsScoped,
    string? ModuleName,
    string? Language,
    string? SourceFilePath,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins);

internal sealed record VueSfcCustomBlock(
    string Name,
    string Text,
    string? Language,
    ImmutableArray<VueSfcAttribute> Attributes,
    string? SourceFilePath,
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins);

internal sealed record VueSfcAttribute(
    string Name,
    string? Value);
