namespace Jero.Core.Service;

public class WechatPayOutput
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
    /// 附加数据
    /// </summary>
    public string Attachment { get; set; } = string.Empty;
    /// <summary>
    /// 优惠标记
    /// </summary>
    public string GoodsTag { get; set; } = string.Empty;
}

public class WechatPayTransactionOutput
{
    public string PrepayId { get; set; } = string.Empty;
    public string OutTradeNumber { get; set; } = string.Empty;
    public WechatPayParaOutput SingInfo { get; set; } = new();
}

public class WechatPayParaOutput
{
    public string AppId { get; set; } = string.Empty;
    public string TimeStamp { get; set; } = string.Empty;
    public string NonceStr { get; set; } = string.Empty;
    public string Package { get; set; } = string.Empty;
    public string SignType { get; set; } = string.Empty;
    public string PaySign { get; set; } = string.Empty;
}
