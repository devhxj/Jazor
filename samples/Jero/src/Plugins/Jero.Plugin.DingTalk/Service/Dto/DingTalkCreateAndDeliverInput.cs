namespace Jero.Plugin.DingTalk;

public class DingTalkCreateAndDeliverInput
{
    /// <summary>
    /// 卡片创建者的userId
    /// </summary>
    public string? userId { get; set; }

    /// <summary>
    /// 卡片内容模板ID
    /// </summary>
    [Required]
    public string cardTemplateId { get; set; } = string.Empty;

    /// <summary>
    /// 外部卡片实例Id
    /// </summary>
    [Required]
    public string outTrackId { get; set; } = string.Empty;

    /// <summary>
    /// 卡片回调的类型：STREAM：stream模式 HTTP：http模式
    /// </summary>
    public string? callbackType { get; set; }

    /// <summary>
    /// 卡片回调HTTP模式时的路由 Key，用于查询注册的 callbackUrl。
    /// </summary>
    public string? callbackRouteKey { get; set; }

    /// <summary>
    /// 卡片数据
    /// </summary>
    [Required]
    public DingTalk_CardData? cardData { get; set; }

    /// <summary>
    /// 用户的私有数据
    /// </summary>
    public PrivateData? crivateData { get; set; }

    /// <summary>
    /// 动态数据源配置
    /// </summary>
    public OpenDynamicDataConfig? openDynamicDataConfig { get; set; }

    /// <summary>
    /// IM单聊酷应用场域信息
    /// </summary>
    public OpenSpaceModel? imSingleOpenSpaceModel { get; set; }

    /// <summary>
    /// IM群聊场域信息。
    /// </summary>
    public OpenSpaceModel? imGroupOpenSpaceModel { get; set; }

    /// <summary>
    /// IM机器人单聊场域信息。
    /// </summary>
    public OpenSpaceModel? imRobotOpenSpaceModel { get; set; }

    /// <summary>
    /// 协作场域信息
    /// </summary>
    public OpenSpaceModel? coFeedOpenSpaceModel { get; set; }

    /// <summary>
    /// 吊顶场域信息
    /// </summary>
    public OpenSpaceModel? topOpenSpaceModel { get; set; }

    /// <summary>
    /// 表示场域及其场域id
    /// </summary>
    /// <remarks>
    /// 其格式为dtv1.card//spaceType1.spaceId1;spaceType2.spaceId2_1;spaceType2.spaceId2_2;spaceType3.spaceId3
    /// </remarks>
    [Required]
    public string openSpaceId { get; set; } = string.Empty;

    /// <summary>
    /// 单聊酷应用场域投放参数。
    /// </summary>
    public DingTalkOpenDeliverModel? imSingleOpenDeliverModel { get; set; }

    /// <summary>
    /// 群聊投放参数。
    /// </summary>
    public DingTalkOpenDeliverModel? imGroupOpenDeliverModel { get; set; }

    /// <summary>
    /// IM机器人单聊投放参数。
    /// </summary>
    public DingTalkOpenDeliverModel? imRobotOpenDeliverModel { get; set; }

    /// <summary>
    /// 吊顶投放参数。
    /// </summary>
    public DingTalkOpenDeliverModel? topOpenDeliverModel { get; set; }

    /// <summary>
    /// 协作投放参数。
    /// </summary>
    public DingTalkOpenDeliverModel? coFeedOpenDeliverModel { get; set; }

    /// <summary>
    /// 文档投放参数
    /// </summary>
    public DingTalkOpenDeliverModel? docOpenDeliverModel { get; set; }

    /// <summary>
    /// 用户userId类型:1（默认）：userId模式 2：unionId模式
    /// </summary>
    public int UserIdType { get; set; }
}

public class DingTalk_CardData
{
    public DingTalk_CardParamMap cardParamMap { get; set; } = new();
}

/// <summary>
/// 卡片模板内容替换参数
/// </summary>
public class DingTalk_CardParamMap
{
    /// <summary>
    /// 片模板内容替换参数
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("sys_full_json_obj")]
    public string sysFullJsonObj { get; set; } = string.Empty;
}

public class PrivateData
{
    public Dictionary<string, DingTalk_CardParamMap> key { get; set; } = new Dictionary<string, DingTalk_CardParamMap>();
}

public class OpenDynamicDataConfig
{
    /// <summary>
    /// 动态数据源配置列表。
    /// </summary>
    public List<DynamicDataSourceConfig>? dynamicDataSourceConfigs { get; set; }
}

public class DynamicDataSourceConfig
{
    /// <summary>
    /// 数据源的唯一 ID, 调用方指定。
    /// </summary>
    public string? dynamicDataSourceId { get; set; }

    /// <summary>
    /// 回调数据源时回传的固定参数。 示例
    /// </summary>
    public Dictionary<string, string>? constParams { get; set; }

    /// <summary>
    /// 数据源拉取配置。
    /// </summary>
    public PullConfig? pullConfig { get; set; }
}

public class PullConfig
{
    /// <summary>
    /// 拉取策略，可选值：NONE：不拉取，无动态数据  INTERVAL：间隔拉取ONCE：只拉取一次
    /// </summary>
    public string pullStrategy { get; set; } = string.Empty;

    /// <summary>
    /// 拉取的间隔时间。
    /// </summary>
    public int interval { get; set; }

    /// <summary>
    /// 拉取的间隔时间的单位， 可选值：SECONDS：秒 MINUTES：分钟 HOURS：小时 DAYS：天
    /// </summary>
    public string timeUnit { get; set; } = string.Empty;
}

public class OpenSpaceModel
{
    /// <summary>
    /// 吊顶场域属性，通过增加spaeType使卡片支持吊顶场域。
    /// </summary>
    public string? spaceType { get; set; }

    /// <summary>
    /// 卡片标题。
    /// </summary>
    public string? title { get; set; }

    /// <summary>
    /// 酷应用编码。
    /// </summary>
    public string? coolAppCode { get; set; }

    /// <summary>
    /// 是否支持转发, 默认false。
    /// </summary>
    public bool? supportForward { get; set; }

    /// <summary>
    /// 支持国际化的LastMessage。
    /// </summary>
    public Dictionary<string, string>? lastMessageI18n { get; set; }

    /// <summary>
    /// 支持卡片消息可被搜索字段。
    /// </summary>
    public SearchSupport? searchSupport { get; set; }

    /// <summary>
    /// 通知信息。
    /// </summary>
    public Notification? notification { get; set; }
}

public class SearchSupport
{
    /// <summary>
    /// 类型的icon，供搜索展示使用。
    /// </summary>
    public string searchIcon { get; set; } = string.Empty;

    /// <summary>
    /// 卡片类型名。
    /// </summary>
    public string searchTypeName { get; set; } = string.Empty;

    /// <summary>
    /// 供消息展示与搜索的字段。
    /// </summary>
    public string searchDesc { get; set; } = string.Empty;
}

public class Notification
{
    /// <summary>
    /// 供消息展示与搜索的字段。
    /// </summary>
    public string alertContent { get; set; } = string.Empty;

    /// <summary>
    /// 是否关闭推送通知：true：关闭 false：不关闭
    /// </summary>
    public bool notificationOff { get; set; }
}

public class DingTalkOpenDeliverModel
{
    /// <summary>
    /// 用于发送卡片的机器人编码。
    /// </summary>
    public string robotCode { get; set; } = string.Empty;

    /// <summary>
    /// 消息@人。格式：{"key":"value"}。key：用户的userId value：用户名
    /// </summary>
    public Dictionary<string, string> atUserIds { get; set; } = [];

    /// <summary>
    /// 指定接收人的userId。
    /// </summary>
    public List<string> recipients { get; set; } = [];

    /// <summary>
    /// 扩展字段，示例如下：{"key":"value"}
    /// </summary>
    public Dictionary<string, string> extension { get; set; } = [];

    /// <summary>
    /// IM机器人单聊若未设置其他投放属性，需设置spaeType为IM_ROBOT。
    /// </summary>
    public string spaceType { get; set; } = string.Empty;

    /// <summary>
    /// 过期时间戳。若使用topOpenDeliverModel对象，则该字段必填。
    /// </summary>
    public long expiredTimeMillis { get; set; }

    /// <summary>
    /// 可以查看该吊顶卡片的userId。
    /// </summary>
    public List<string> userIds { get; set; } = [];

    /// <summary>
    /// 可以查看该吊顶卡片的设备：android｜ios｜win｜mac。
    /// </summary>
    public List<string> platforms { get; set; } = [];

    /// <summary>
    /// 业务标识。
    /// </summary>
    public string bizTag { get; set; } = string.Empty;

    /// <summary>
    /// 协作场域下的排序时间。
    /// </summary>
    public long gmtTimeLine { get; set; }

    /// <summary>
    /// 员工userId信息
    /// </summary>
    public string userId { get; set; } = string.Empty;
}
