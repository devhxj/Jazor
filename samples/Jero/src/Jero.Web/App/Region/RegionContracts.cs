namespace Jero.Web.App.Region;

public sealed partial record RegionFlatView
{
    public required string id { get; init; }
    public string pid { get; init; } = "0";
    public required string name { get; init; }
    public required string code { get; init; }
    public required string cityCode { get; init; }
    public required int orderNo { get; init; }
    public required string remark { get; init; }
    public required int depth { get; init; }
    public bool hasChildren { get; init; }
    public bool childrenLoaded { get; init; }
    public string pathName { get; init; } = string.Empty;
    public string[] ancestorIds { get; init; } = [];
}
