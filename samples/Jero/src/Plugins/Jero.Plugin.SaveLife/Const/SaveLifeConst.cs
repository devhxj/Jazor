namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机业务模块相关常量
/// </summary>
[Const("医线生机业务模块相关常量")]
public class SaveLifeConst
{
    /// <summary>
    /// API 分组名称
    /// </summary>
    public const string GroupName = "SaveLife";

    /// <summary>
    /// 药物临床试验登记平台 HTTP 客户端名称
    /// </summary>
    public const string ClinicalTrialHttpClientName = "SaveLifeClinicalTrial";

    /// <summary>
    /// 病例材料多模态向量 HTTP 客户端名称
    /// </summary>
    public const string MaterialEmbeddingHttpClientName = "SaveLifeMaterialEmbedding";

    /// <summary>
    /// 病例资料多模态对话 HTTP 客户端名称
    /// </summary>
    public const string AgentChatHttpClientName = "SaveLifeAgentChat";

    /// <summary>
    /// 微信小程序 API HTTP 客户端名称
    /// </summary>
    public const string MiniAppHttpClientName = "SaveLifeMiniApp";

    /// <summary>
    /// 药物临床试验登记平台地址
    /// </summary>
    public const string ClinicalTrialBaseUrl = "https://www.chinadrugtrials.org.cn/";
}
