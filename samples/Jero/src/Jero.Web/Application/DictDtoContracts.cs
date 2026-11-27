using static ECMAScript.Vue;

namespace Jero.Web.Application;

public sealed record DictTypeView : VueProps
{
    [ECMAScriptName("id")]
    public required string Id { get; init; }
    [ECMAScriptName("name")]
    public required string Name { get; init; }
    [ECMAScriptName("code")]
    public required string Code { get; init; }
    [ECMAScriptName("orderNo")]
    public required int OrderNo { get; init; }
    public string? Remark { get; init; }
    [ECMAScriptName("status")]
    public required int Status { get; init; }
    public required int SysFlag { get; init; }
    public required int IsTenant { get; init; }
}

public sealed record DictDataView : VueProps
{
    [ECMAScriptName("id")]
    public required string Id { get; init; }
    public string DictTypeId { get; init; } = string.Empty;
    public required string Label { get; init; }
    public required string Value { get; init; }
    public required string? Code { get; init; }
    public string? Name { get; init; }
    [ECMAScriptName("tagType")]
    public required string? TagType { get; init; }
    public required string? StyleSetting { get; init; }
    public required string? ClassSetting { get; init; }
    [ECMAScriptName("orderNo")]
    public required int OrderNo { get; init; }
    public string? Remark { get; init; }
    public required string? ExtData { get; init; }
    [ECMAScriptName("status")]
    public required int Status { get; init; }
}
