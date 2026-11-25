namespace Jero.Core.Service;

public class DictDataOutput
{
    public long DictDataId { get; set; }
    public string TypeCode { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string TagType { get; set; } = string.Empty;
    public string StyleSetting { get; set; } = string.Empty;
    public string ClassSetting { get; set; } = string.Empty;
    public string ExtData { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public int OrderNo { get; set; }
    public StatusEnum Status { get; set; }
}