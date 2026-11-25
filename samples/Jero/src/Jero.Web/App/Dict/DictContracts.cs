namespace Jero.Web.App.Dict;

public sealed partial record DictTypeView
{
    public required string id { get; init; }
    public required string name { get; init; }
    public required string code { get; init; }
    public required int orderNo { get; init; }
    public string? remark { get; init; }
    public required int status { get; init; }
    public required int sysFlag { get; init; }
    public required int isTenant { get; init; }
}

public sealed partial record DictDataView
{
    public required string id { get; init; }
    public string dictTypeId { get; init; } = string.Empty;
    public required string label { get; init; }
    public required string value { get; init; }
    public required string? code { get; init; }
    public string? name { get; init; }
    public required string? tagType { get; init; }
    public required string? styleSetting { get; init; }
    public required string? classSetting { get; init; }
    public required int orderNo { get; init; }
    public string? remark { get; init; }
    public required string? extData { get; init; }
    public required int status { get; init; }
}
