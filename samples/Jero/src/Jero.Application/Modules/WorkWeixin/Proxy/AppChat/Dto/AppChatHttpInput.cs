using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Jero.Application.Modules.WorkWeixin.Proxy;

/// <summary>
/// 创建群聊会话输入参数
/// </summary>
public class CreatAppChatInput
{
    /// <summary>
    /// 群名称
    /// </summary>
    [JsonPropertyName("name")]
    [Required(ErrorMessage = "群名称不能为空"), MaxLength(50, ErrorMessage = "群名称最多不能超过50个字符")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 群主Id
    /// </summary>
    [JsonPropertyName("owner")]
    [Required(ErrorMessage = "群主Id不能为空")]
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// 群成员Id列表
    /// </summary>
    [JsonPropertyName("userlist")]
    [Core.NotEmpty(ErrorMessage = "群成员列表不能为空")]
    public List<string> UserList { get; set; } = [];

    /// <summary>
    /// 群Id
    /// </summary>
    [JsonPropertyName("chatid")]
    [Required(ErrorMessage = "群Id不能为空"), MaxLength(32, ErrorMessage = "群Id最多不能超过32个字符")]
    public string ChatId { get; set; } = string.Empty;
}

/// <summary>
/// 修改群聊会话输入参数
/// </summary>
public class UpdateAppChatInput
{
    /// <summary>
    /// 群Id
    /// </summary>
    [JsonPropertyName("chatid")]
    [Required(ErrorMessage = "群Id不能为空"), MaxLength(32, ErrorMessage = "群Id最多不能超过32个字符")]
    public string ChatId { get; set; } = string.Empty;

    /// <summary>
    /// 群名称
    /// </summary>
    [JsonPropertyName("name")]
    [Required(ErrorMessage = "群名称不能为空"), MaxLength(50, ErrorMessage = "群名称最多不能超过50个字符")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 群主Id
    /// </summary>
    [JsonPropertyName("owner")]
    [Required(ErrorMessage = "群主Id不能为空")]
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// 添加成员的id列表
    /// </summary>
    [JsonPropertyName("add_user_list")]
    public List<string> AddUserList { get; set; } = [];

    /// <summary>
    /// 踢出成员的id列表
    /// </summary>
    [JsonPropertyName("del_user_list")]
    public List<string> DelUserList { get; set; } = [];
}

/// <summary>
/// 应用消息推送输入基类参数
/// </summary>
public class SendBaseAppChatInput
{
    /// <summary>
    /// 群Id
    /// </summary>
    [JsonPropertyName("chatid")]
    [Required(ErrorMessage = "群Id不能为空"), MaxLength(32, ErrorMessage = "群Id最多不能超过32个字符")]
    public string ChatId { get; set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    /// <example>text：文本消息</example>
    /// <example>image：图片消息</example>
    /// <example>voice：图片消息</example>
    /// <example>video：视频消息</example>
    /// <example>file：文件消息</example>
    /// <example>textcard：文本卡片</example>
    /// <example>news：图文消息</example>
    /// <example>mpnews：图文消息（存储在企业微信）</example>
    /// <example>markdown：markdown消息</example>
    [JsonPropertyName("msgtype")]
    [Required(ErrorMessage = "消息类型不能为空")]
    protected string MsgType { get; set; }

    /// <summary>
    /// 是否是保密消息
    /// </summary>
    [JsonPropertyName("safe")]
    [Required(ErrorMessage = "消息类型不能为空")]
    public int Safe { get; set; }

    public SendBaseAppChatInput(string chatId, string msgType, bool safe = false)
    {
        ChatId = chatId;
        MsgType = msgType;
        Safe = safe ? 1 : 0;
    }
}

/// <summary>
/// 推送文本消息输入参数
/// </summary>
public class SendTextAppChatInput : SendBaseAppChatInput
{
    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("text")]
    public object Text { get; set; }

    /// <summary>
    /// 文本消息
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="content"></param>
    /// <param name="safe"></param>
    public SendTextAppChatInput(string chatId, string content, bool safe = false) : base(chatId, "text", safe)
    {
        Text = new { content };
    }
}

/// <summary>
/// 推送图片消息输入参数
/// </summary>
public class SendImageAppChatInput : SendBaseAppChatInput
{
    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("image")]
    public object Image { get; set; }

    /// <summary>
    /// 图片消息
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="mediaId"></param>
    /// <param name="safe"></param>
    public SendImageAppChatInput(string chatId, string mediaId, bool safe = false) : base(chatId, "image", safe)
    {
        Image = new { media_id = mediaId };
    }
}

/// <summary>
/// 推送语音消息输入参数
/// </summary>
public class SendVoiceAppChatInput : SendBaseAppChatInput
{
    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("voice")]
    public object Voice { get; set; }

    /// <summary>
    /// 语音消息
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="mediaId"></param>
    /// <param name="safe"></param>
    public SendVoiceAppChatInput(string chatId, string mediaId, bool safe = false) : base(chatId, "voice", safe)
    {
        Voice = new { media_id = mediaId };
    }
}

/// <summary>
/// 推送视频消息输入参数
/// </summary>
public class SendVideoAppChatInput : SendBaseAppChatInput
{
    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("video")]
    public object Video { get; set; }

    /// <summary>
    /// 视频消息
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <param name="mediaId"></param>
    /// <param name="safe"></param>
    public SendVideoAppChatInput(string chatId, string title, string description, string mediaId, bool safe = false) : base(chatId, "video", safe)
    {
        Video = new
        {
            media_id = mediaId,
            description,
            title
        };
    }
}

/// <summary>
/// 推送视频消息输入参数
/// </summary>
public class SendFileAppChatInput : SendBaseAppChatInput
{
    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("file")]
    public object File { get; set; }

    /// <summary>
    /// 文件消息
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="mediaId"></param>
    /// <param name="safe"></param>
    public SendFileAppChatInput(string chatId, string mediaId, bool safe = false) : base(chatId, "video", safe)
    {
        File = new { media_id = mediaId };
    }
}

/// <summary>
/// 推送文本卡片消息输入参数
/// </summary>
public class SendTextCardAppChatInput : SendBaseAppChatInput
{
    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("textcard")]
    public object TextCard { get; set; }

    /// <summary>
    /// 文本卡片消息
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="title">标题</param>
    /// <param name="description">描述</param>
    /// <param name="url">点击后跳转的链接</param>
    /// <param name="btnTxt">按钮文字</param>
    /// <param name="safe"></param>
    public SendTextCardAppChatInput(string chatId, string title, string description, string url, string btnTxt, bool safe = false) : base(chatId, "textcard", safe)
    {
        TextCard = new
        {
            title,
            description,
            url,
            btntxt = btnTxt
        };
    }
}

/// <summary>
/// 图文消息项
/// </summary>
public class SendNewsItem
{
    /// <summary>
    /// 标题
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 图文消息的图片链接（推荐大图1068 * 455，小图150 * 150）
    /// </summary>
    [JsonPropertyName("picurl")]
    public string PicUrl { get; set; } = string.Empty;
}

/// <summary>
/// 推送图文消息输入参数
/// </summary>
public class SendNewsAppChatInput : SendBaseAppChatInput
{
    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("news")]
    public object News { get; set; }

    /// <summary>
    /// 图文消息
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="newsList">图文消息列表</param>
    /// <param name="safe"></param>
    public SendNewsAppChatInput(string chatId, List<SendNewsItem> newsList, bool safe = false) : base(chatId, "news", safe)
    {
        News = new { articles = newsList };
    }
}

/// <summary>
/// 图文消息项
/// </summary>
public class SendMpNewsItem
{
    /// <summary>
    /// 标题
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 缩略图media_id
    /// </summary>
    [JsonPropertyName("thumb_media_id")]
    public string ThumbMediaId { get; set; } = string.Empty;

    /// <summary>
    /// 作者
    /// </summary>
    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// 点击“阅读原文”之后的页面链接
    /// </summary>
    [JsonPropertyName("content_source_url")]
    public string ContentSourceUrl { get; set; } = string.Empty;

    /// <summary>
    /// 图文消息的内容
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 图文消息的描述
    /// </summary>
    [JsonPropertyName("digest")]
    public string Digest { get; set; } = string.Empty;
}

/// <summary>
/// 推送图文消息(存储在企业微信)输入参数
/// </summary>
public class SendMpNewsAppChatInput : SendBaseAppChatInput
{
    /// <summary>
    /// 消息内容
    /// </summary>
    [JsonPropertyName("mpnews")]
    public object MpNews { get; set; }

    /// <summary>
    /// 图文消息
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="mpNewsList">图文消息列表</param>
    /// <param name="safe"></param>
    public SendMpNewsAppChatInput(string chatId, List<SendMpNewsItem> mpNewsList, bool safe = false) : base(chatId, "mpnews", safe)
    {
        MpNews = new { articles = mpNewsList };
    }
}
