namespace Jero.Core.Service;

public class WechatPayTransactionInput
{
    /// <summary>
    /// OpenId
    /// </summary>
    public string OpenId { get; set; } = string.Empty;
    /// <summary>
    /// 订单金额
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 商品描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// 附加数据
    /// </summary>
    public string Attachment { get; set; } = string.Empty;
    /// <summary>
    /// 优惠标记
    /// </summary>
    public string GoodsTag { get; set; } = string.Empty;
    /// <summary>
    /// 业务标签，用来区分做什么业务
    /// </summary>
    public string Tags { get; set; } = string.Empty;
    /// <summary>
    /// 对应业务的主键
    /// </summary>
    public long BusinessId { get; set; }
}

public class WechatPayParaInput
{
    /// <summary>
    /// 订单Id
    /// </summary>
    public string PrepayId { get; set; } = string.Empty;
}

public class WechatPayRefundDomesticInput
{
    /// <summary>
    /// 商户端生成的业务流水号
    /// </summary>
    [Required]
    public string TradeId { get; set; } = string.Empty;
    /// <summary>
    /// 退款原因
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    /// <summary>
    /// 退款金额
    /// </summary>
    [Required]
    public int Refund { get; set; }

    /// <summary>
    /// 原订单金额
    /// </summary>
    [Required]
    public int Total { get; set; }
}

public class WechatPayPageInput : BasePageInput
{
    /// <summary>
    /// 添加时间范围
    /// </summary>
    public List<DateTime?> CreateTimeRange { get; set; } = [];
}