namespace Jero.Core;

/// <summary>
/// 参与方的证件类型枚举
/// </summary>
[SuppressSniffer]
[Description("参与方的证件类型枚举")]
public enum AlipayCertTypeEnum
{
    [Description("身份证")]
    IDENTITY_CARD,

    [Description("护照")]
    PASSPORT
}