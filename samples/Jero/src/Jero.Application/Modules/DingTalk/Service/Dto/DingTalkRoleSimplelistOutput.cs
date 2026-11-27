namespace Jero.Application.Modules.DingTalk;

public class DingTalkRoleSimplelistOutput
{
    /// <summary>
    /// 是否还有更多数据
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("hasMore")]
    public bool hasMore { get; set; }

    /// <summary>
    /// 角色组列表
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("list")]
    public List<DingTalkRoleSimplelistResult> list { get; set; } = [];
}
