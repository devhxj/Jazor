namespace Jero.Core;

/// <summary>
/// 参与方的标识类型枚举
/// </summary>
[SuppressSniffer]
[Description("参与方的标识类型枚举")]
public enum AlipayIdentityTypeEnum
{
    [Description("支付宝用户UID")]
    ALIPAY_USER_ID,

    [Description("支付宝登录号")]
    ALIPAY_LOGON_ID
}