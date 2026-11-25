


using System.Text.Json.Serialization;
namespace Jero.Plugin.ApprovalFlow.Service;

public class ApprovalFormItem
{
    [JsonPropertyName("configId")]
    public string ConfigId { get; set; } = string.Empty;

    [JsonPropertyName("tableName")]
    public string TableName { get; set; } = string.Empty;

    [JsonPropertyName("entityName")]
    public string EntityName { get; set; } = string.Empty;

    [JsonPropertyName("typeName")]
    public string TypeName { get; set; } = string.Empty;

    [JsonPropertyName("route")]
    public string Route
    {
        get
        {
            if (string.IsNullOrWhiteSpace(EntityName)) return string.Empty;
            return EntityName[..1].ToLower() + EntityName[1..] + "/" + TypeName;
        }
    }
}
