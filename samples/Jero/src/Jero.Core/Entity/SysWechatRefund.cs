namespace Jero.Core;

/// <summary>
/// 系统微信支付退款表
/// </summary>
[SugarTable(null, "系统微信支付退款表")]
[SysTable]
[SugarIndex("index_{table}_W", nameof(WechatPayId), OrderByType.Asc)]
public partial class SysWechatRefund : EntityBase
{
    /// <summary>
    /// 定单主键
    /// </summary>
    [SugarColumn(ColumnDescription = "定单主键")]
    public long WechatPayId { get; set; }

    /// <summary>
    /// 商户退款号
    /// </summary>
    [SugarColumn(ColumnDescription = "商户退款号")]
    [Required]
    public virtual string OutRefundNumber { get; set; } = string.Empty;
    /// <summary>
    /// 退款订单号
    /// </summary>
    [SugarColumn(ColumnDescription = "退款订单号")]
    [Required]
    public virtual string TransactionId { get; set; } = string.Empty;
    /// <summary>
    /// 退款原因
    /// </summary>
    [SugarColumn(ColumnDescription = "退款原因")]
    public string? Reason { get; set; }

    /// <summary>
    /// 退款渠道
    /// </summary>
    [SugarColumn(ColumnDescription = "退款渠道")]
    public string? Channel { get; set; }

    /// <summary>
    /// 退款入账账户
    /// </summary>
    /// <remarks>
    /// 取当前退款单的退款入账方，有以下几种情况：
    /// 1）退回银行卡：{银行名称}{卡类型}{ 卡尾号}
    /// 2）退回支付用户零钱: 支付用户零钱
    /// 3）退还商户: 商户基本账户商户结算银行账户
    /// 4）退回支付用户零钱通: 支付用户零钱通
    /// </remarks>
    [SugarColumn(ColumnDescription = "退款入账账户")]
    public string? UserReceivedAccount { get; set; }

    /// <summary>
    /// 退款状态
    /// </summary>
    [SugarColumn(ColumnDescription = "退款状态")]
    public string? TradeState { get; set; }

    /// <summary>
    /// 交易状态描述
    /// </summary>
    [SugarColumn(ColumnDescription = "交易状态描述")]
    public string? TradeStateDescription { get; set; }

    /// <summary>
    /// 订单总金额
    /// </summary>
    [SugarColumn(ColumnDescription = "退款金额")]
    public int Refund { get; set; }

    /// <summary>
    /// 支完成时间
    /// </summary>
    [SugarColumn(ColumnDescription = "完成时间")]
    public DateTime? SuccessTime { get; set; }

    /// <summary>
    /// 回调通知地址
    /// </summary>
    [SugarColumn(ColumnDescription = "回调通知地址")]
    public string? NotifyUrl { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注")]
    public string? Remark { get; set; }
}