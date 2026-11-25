namespace Jero.Core.Service;

public class DbTableInput
{
    public string ConfigId { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<DbColumnInput> DbColumnInfoList { get; set; } = [];
}

public class UpdateDbTableInput
{
    public string ConfigId { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string OldTableName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class DeleteDbTableInput
{
    public string ConfigId { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
}