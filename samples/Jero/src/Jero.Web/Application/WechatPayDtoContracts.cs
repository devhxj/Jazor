using static ECMAScript.Vue;

namespace Jero.Web.Application;

public sealed record WechatPayView : VueProps
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    public string MerchantId { get; init; } = string.Empty;
    public string AppId { get; init; } = string.Empty;
    public string SubMerchantId { get; init; } = string.Empty;
    public string SubAppId { get; init; } = string.Empty;
    [ECMAScriptName("outTradeNumber")]
    public string OutTradeNumber { get; init; } = string.Empty;
    [ECMAScriptName("transactionId")]
    public string TransactionId { get; init; } = string.Empty;
    public string TradeType { get; init; } = string.Empty;
    [ECMAScriptName("tradeState")]
    public string TradeState { get; init; } = string.Empty;
    [ECMAScriptName("tradeStateDescription")]
    public string TradeStateDescription { get; init; } = string.Empty;
    public string BankType { get; init; } = string.Empty;
    public int Total { get; init; }
    public int? PayerTotal { get; init; }
    [ECMAScriptName("successTime")]
    public string SuccessTime { get; init; } = string.Empty;
    public string ExpireTime { get; init; } = string.Empty;
    [ECMAScriptName("description")]
    public string Description { get; init; } = string.Empty;
    public string Scene { get; init; } = string.Empty;
    public string Attachment { get; init; } = string.Empty;
    public string GoodsTag { get; init; } = string.Empty;
    public string Settlement { get; init; } = string.Empty;
    public string NotifyUrl { get; init; } = string.Empty;
    public string Remark { get; init; } = string.Empty;
    public string OpenId { get; init; } = string.Empty;
    public string SubOpenId { get; init; } = string.Empty;
    [ECMAScriptName("tags")]
    public string Tags { get; init; } = string.Empty;
    [ECMAScriptName("businessId")]
    public string BusinessId { get; init; } = string.Empty;
    public string QrcodeContent { get; init; } = string.Empty;
    public string CreateTime { get; init; } = string.Empty;
    public string CreateUserId { get; init; } = string.Empty;
    public string CreateUserName { get; init; } = string.Empty;
    public string UpdateTime { get; init; } = string.Empty;
    public string UpdateUserId { get; init; } = string.Empty;
    public string UpdateUserName { get; init; } = string.Empty;
}

public sealed record WechatRefundView : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string WechatPayId { get; init; } = string.Empty;
    public string OutRefundNumber { get; init; } = string.Empty;
    public string TransactionId { get; init; } = string.Empty;
    [ECMAScriptName("reason")]
    public string Reason { get; init; } = string.Empty;
    [ECMAScriptName("channel")]
    public string Channel { get; init; } = string.Empty;
    [ECMAScriptName("userReceivedAccount")]
    public string UserReceivedAccount { get; init; } = string.Empty;
    public string TradeState { get; init; } = string.Empty;
    [ECMAScriptName("tradeStateDescription")]
    public string TradeStateDescription { get; init; } = string.Empty;
    public int Refund { get; init; }
    [ECMAScriptName("successTime")]
    public string SuccessTime { get; init; } = string.Empty;
    public string NotifyUrl { get; init; } = string.Empty;
    public string Remark { get; init; } = string.Empty;
    public string CreateTime { get; init; } = string.Empty;
    public string CreateUserId { get; init; } = string.Empty;
    public string CreateUserName { get; init; } = string.Empty;
    public string UpdateTime { get; init; } = string.Empty;
    public string UpdateUserId { get; init; } = string.Empty;
    public string UpdateUserName { get; init; } = string.Empty;
}
