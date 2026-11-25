namespace Jero.Core;

/// <summary>
/// 系统微信支付表
/// </summary>
[SugarTable(null, "系统微信支付表")]
[SysTable]
[SugarIndex("index_{table}_BU", nameof(BusinessId), OrderByType.Asc)]
[SugarIndex("index_{table}_TR", nameof(TradeState), OrderByType.Asc)]
[SugarIndex("index_{table}_TA", nameof(Tags), OrderByType.Asc)]
public partial class SysWechatPay : EntityBase
{
    /// <summary>
    /// 微信商户号
    /// </summary>
    [SugarColumn(ColumnDescription = "微信商户号")]
    [Required]
    public virtual string MerchantId { get; set; } = string.Empty;
    /// <summary>
    /// 服务商AppId
    /// </summary>
    [SugarColumn(ColumnDescription = "服务商AppId")]
    [Required]
    public virtual string AppId { get; set; } = string.Empty;
    /// <summary>
    /// 商户订单号
    /// </summary>
    [SugarColumn(ColumnDescription = "商户订单号")]
    [Required]
    public virtual string OutTradeNumber { get; set; } = string.Empty;
    /// <summary>
    /// 支付订单号
    /// </summary>
    [SugarColumn(ColumnDescription = "支付订单号")]
    [Required]
    public virtual string TransactionId { get; set; } = string.Empty;
    /// <summary>
    /// 交易类型
    /// </summary>
    [SugarColumn(ColumnDescription = "交易类型")]
    public string? TradeType { get; set; }

    /// <summary>
    /// 交易状态
    /// </summary>
    [SugarColumn(ColumnDescription = "交易状态")]
    public string? TradeState { get; set; }

    /// <summary>
    /// 交易状态描述
    /// </summary>
    [SugarColumn(ColumnDescription = "交易状态描述")]
    public string? TradeStateDescription { get; set; }

    /// <summary>
    /// 付款银行类型
    /// </summary>
    [SugarColumn(ColumnDescription = "付款银行类型")]
    public string? BankType { get; set; }

    /// <summary>
    /// 订单总金额
    /// </summary>
    [SugarColumn(ColumnDescription = "订单总金额")]
    public int Total { get; set; }

    /// <summary>
    /// 用户支付金额
    /// </summary>
    [SugarColumn(ColumnDescription = "用户支付金额")]
    public int? PayerTotal { get; set; }

    /// <summary>
    /// 支付完成时间
    /// </summary>
    [SugarColumn(ColumnDescription = "支付完成时间")]
    public DateTime? SuccessTime { get; set; }

    /// <summary>
    /// 交易结束时间
    /// </summary>
    [SugarColumn(ColumnDescription = "交易结束时间")]
    public DateTime? ExpireTime { get; set; }

    /// <summary>
    /// 商品描述
    /// </summary>
    [SugarColumn(ColumnDescription = "商品描述")]
    public string? Description { get; set; }

    /// <summary>
    /// 场景信息
    /// </summary>
    [SugarColumn(ColumnDescription = "场景信息")]
    public string? Scene { get; set; }

    /// <summary>
    /// 附加数据
    /// </summary>
    [SugarColumn(ColumnDescription = "附加数据")]
    public string? Attachment { get; set; }

    /// <summary>
    /// 优惠标记
    /// </summary>
    [SugarColumn(ColumnDescription = "优惠标记")]
    public string? GoodsTag { get; set; }

    /// <summary>
    /// 结算信息
    /// </summary>
    [SugarColumn(ColumnDescription = "结算信息")]
    public string? Settlement { get; set; }

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

    /// <summary>
    /// 微信OpenId标识
    /// </summary>
    [SugarColumn(ColumnDescription = "微信OpenId标识")]
    public string? OpenId { get; set; }

    /// <summary>
    /// 业务标签，用来区分做什么业务
    /// </summary>
    /// <remarks>
    /// Tags标识用来区分这个支付记录对应什么业务从而确定相关联的表名，
    /// 再结合BusinessId保存了对应的业务数据的ID，就可以确定这个支付
    /// 记录与哪一条业务数据相关联
    /// </remarks>
    [SugarColumn(ColumnDescription = "业务标签，用来区分做什么业务", Length = 64)]
    public string? Tags { get; set; }

    /// <summary>
    /// 对应业务的主键
    /// </summary>
    [SugarColumn(ColumnDescription = "对应业务的主键")]
    public long BusinessId { get; set; }

    /// <summary>
    /// 付款二维码内容
    /// </summary>
    [SugarColumn(ColumnDescription = "付款二维码内容")]
    public string? QrcodeContent { get; set; }

    /// <summary>
    /// 关联微信用户
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Navigate(NavigateType.OneToOne, nameof(OpenId))]
    public SysWechatUser? SysWechatUser { get; set; }

    /// <summary>
    /// 子商户号
    /// </summary>
    [SugarColumn(ColumnDescription = "子商户号")]
    public string? SubMerchantId { get; set; }

    /// <summary>
    /// 子商户AppId
    /// </summary>
    [SugarColumn(ColumnDescription = "回调通知地址")]
    public string? SubAppId { get; set; }

    /// <summary>
    /// 子商户唯一标识
    /// </summary>
    [SugarColumn(ColumnDescription = "子商户唯一标识")]
    public string? SubOpenId { get; set; }
}
