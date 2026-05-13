using System.Collections.Generic;

namespace Jazor.Emit;

internal sealed record RazorVueSfcCatalogRecord(
    string AssemblyName,
    IReadOnlyList<RazorVueEmitSfcArtifactRecord> Artifacts);

internal sealed record RazorVueEmitSfcArtifactRecord(
    string ComponentName,
    string RelativeSfcPath,
    string SfcText,
    RazorVueEmitSfcTemplateBlockRecord TemplateBlock,
    RazorVueEmitSfcScriptSetupBlockRecord ScriptSetupBlock,
    IReadOnlyList<RazorVueEmitSfcStyleBlockRecord> StyleBlocks,
    IReadOnlyList<RazorVueEmitSfcCustomBlockRecord> CustomBlocks,
    IReadOnlyList<string> RouteTemplates,
    IReadOnlyList<string> Imports,
    IReadOnlyList<string> Styles,
    IReadOnlyList<string> PluginRequirements,
    RazorVueEmitSfcArtifactIdentity Identity,
    RazorVueEmitRuntimeHints Hints,
    IReadOnlyList<RazorVueEmitSfcSourceOriginRecord> SourceOrigins)
{
    public string TemplateText => TemplateBlock.Text;

    public string ScriptSetupText => ScriptSetupBlock.Text;
}

internal sealed record RazorVueEmitSfcArtifactIdentity(
    string ComponentId,
    string ModuleId,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    string StyleHash,
    RazorVueHmrBoundaryKind HmrBoundaryKind);

internal sealed record RazorVueEmitSfcTemplateBlockRecord(
    string Text,
    IReadOnlyList<RazorVueEmitSfcSourceOriginRecord> SourceOrigins);

internal sealed record RazorVueEmitSfcScriptSetupBlockRecord(
    string Text,
    string? Language,
    IReadOnlyList<RazorVueEmitSfcSourceOriginRecord> SourceOrigins);

internal sealed record RazorVueEmitSfcStyleBlockRecord(
    string Text,
    bool IsScoped,
    string? ModuleName,
    string? Language,
    string? SourceFilePath,
    IReadOnlyList<RazorVueEmitSfcSourceOriginRecord> SourceOrigins);

internal sealed record RazorVueEmitSfcCustomBlockRecord(
    string Name,
    string Text,
    string? Language,
    IReadOnlyList<RazorVueEmitSfcAttributeRecord> Attributes,
    string? SourceFilePath,
    IReadOnlyList<RazorVueEmitSfcSourceOriginRecord> SourceOrigins);

internal sealed record RazorVueEmitSfcAttributeRecord(
    string Name,
    string? Value);

internal sealed record RazorVueEmitSfcSourceOriginRecord(
    RazorVueSfcOriginKindRecord OriginKind,
    string SourceFilePath,
    int SourceSpanStart,
    int SourceSpanLength,
    string? GeneratedFilePath,
    int? GeneratedSpanStart,
    int? GeneratedSpanLength,
    int StartLine,
    int StartColumn,
    RazorVueMappingQualityRecord MappingQuality,
    RazorVueOriginProvenanceRecord Provenance);

internal enum RazorVueSfcOriginKindRecord
{
    Component,
    Descriptor,
    Template,
    Logic,
    GeneratedRender,
    Style,
    CustomBlock
}
