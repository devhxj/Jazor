using static ECMAScript.Vue;

namespace Jero.Web.Application;

public sealed record RegionFlatView : VueProps
{
    [ECMAScriptName("id")]
    public required string Id { get; init; }
    public string Pid { get; init; } = "0";
    [ECMAScriptName("name")]
    public required string Name { get; init; }
    public required string Code { get; init; }
    public required string CityCode { get; init; }
    public required int OrderNo { get; init; }
    public required string Remark { get; init; }
    [ECMAScriptName("depth")]
    public required int Depth { get; init; }
    public bool HasChildren { get; init; }
    public bool ChildrenLoaded { get; init; }
    public string PathName { get; init; } = string.Empty;
    public string[] AncestorIds { get; init; } = [];
}
