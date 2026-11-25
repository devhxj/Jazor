namespace Jero.Core;

/// <summary>
/// 支付宝支付常量
/// </summary>
[SuppressSniffer]
public class AlipayConst
{
    /// <summary>
    /// 单笔无密转账【业务场景】固定值
    /// </summary>
    public const string BizScene = "DIRECT_TRANSFER";

    /// <summary>
    /// 单笔无密转账【销售产品码】固定值
    /// </summary>
    public const string ProductCode = "TRANS_ACCOUNT_NO_PWD";

    /// <summary>
    /// 交易状态参数名
    /// </summary>
    public const string TradeStatus = "trade_status";

    /// <summary>
    /// 交易成功标识
    /// </summary>
    public const string TradeSuccess = "TRADE_SUCCESS";

    /// <summary>
    /// 授权类型
    /// </summary>
    public const string GrantType = "authorization_code";
}