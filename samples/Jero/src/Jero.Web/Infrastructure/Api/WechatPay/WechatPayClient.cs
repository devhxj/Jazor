using Jero.Web.Application;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.WechatPay;

/// <summary>查询微信支付订单和退款，发起扫码支付或退款，并同步订单状态。</summary>
[ECMAScriptModule("components/wechat-pay-client.js")]
public static class WechatPayClient
{
    public static IPromise<ApiOutcome> Page(WechatPayPageInput input)
        => BrowserApiClient.Post("/api/sysWechatPay/page", input);

    public static IPromise<ApiOutcome> CreateNative(WechatPayCreateInput input)
        => BrowserApiClient.Post("/api/sysWechatPay/payTransactionNative", input);

    public static IPromise<ApiOutcome> Refunds(string transactionId)
        => BrowserApiClient.Post("/api/sysWechatPay/listRefund", transactionId);

    public static IPromise<ApiOutcome> Refund(WechatPayRefundInput input)
        => BrowserApiClient.Post("/api/sysWechatPay/refundDomestic", input);

    public static IPromise<ApiOutcome> SyncFromWechat(string outTradeNumber)
        => BrowserApiClient.Get("/api/sysWechatPay/payInfoFromWechat/" + EncodeURIComponent(outTradeNumber));

    public static WechatPayView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new WechatPayView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadPay(values[index]!);
        return result;
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static WechatRefundView[] ReadRefunds(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new WechatRefundView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadRefund(values[index]!);
        return result;
    }

    public static WechatPayCreateResult ReadCreateResult(object? data)
        => data is null
            ? new WechatPayCreateResult()
            : new WechatPayCreateResult
            {
                OutTradeNumber = ReadString(data, "outTradeNumber"),
                QrcodeUrl = ReadString(data, "qrcodeUrl")
            };

    public static WechatPayView? ReadPayDetail(object? data)
        => data is null ? null : ReadPay(data);

    private static WechatPayView ReadPay(object value)
        => new()
        {
            Id = ReadString(value, "id"),
            MerchantId = ReadString(value, "merchantId"),
            AppId = ReadString(value, "appId"),
            SubMerchantId = ReadString(value, "subMerchantId"),
            SubAppId = ReadString(value, "subAppId"),
            OutTradeNumber = ReadString(value, "outTradeNumber"),
            TransactionId = ReadString(value, "transactionId"),
            TradeType = ReadString(value, "tradeType"),
            TradeState = ReadString(value, "tradeState"),
            TradeStateDescription = ReadString(value, "tradeStateDescription"),
            BankType = ReadString(value, "bankType"),
            Total = ReadInt(value, "total", 0),
            PayerTotal = ReadNullableInt(value, "payerTotal"),
            SuccessTime = ReadString(value, "successTime"),
            ExpireTime = ReadString(value, "expireTime"),
            Description = ReadString(value, "description"),
            Scene = ReadString(value, "scene"),
            Attachment = ReadString(value, "attachment"),
            GoodsTag = ReadString(value, "goodsTag"),
            Settlement = ReadString(value, "settlement"),
            NotifyUrl = ReadString(value, "notifyUrl"),
            Remark = ReadString(value, "remark"),
            OpenId = ReadString(value, "openId"),
            SubOpenId = ReadString(value, "subOpenId"),
            Tags = ReadString(value, "tags"),
            BusinessId = ReadString(value, "businessId"),
            QrcodeContent = ReadString(value, "qrcodeContent"),
            CreateTime = ReadString(value, "createTime"),
            CreateUserId = ReadString(value, "createUserId"),
            CreateUserName = ReadString(value, "createUserName"),
            UpdateTime = ReadString(value, "updateTime"),
            UpdateUserId = ReadString(value, "updateUserId"),
            UpdateUserName = ReadString(value, "updateUserName")
        };

    private static WechatRefundView ReadRefund(object value)
        => new()
        {
            Id = ReadString(value, "id"),
            WechatPayId = ReadString(value, "wechatPayId"),
            OutRefundNumber = ReadString(value, "outRefundNumber"),
            TransactionId = ReadString(value, "transactionId"),
            Reason = ReadString(value, "reason"),
            Channel = ReadString(value, "channel"),
            UserReceivedAccount = ReadString(value, "userReceivedAccount"),
            TradeState = ReadString(value, "tradeState"),
            TradeStateDescription = ReadString(value, "tradeStateDescription"),
            Refund = ReadInt(value, "refund", 0),
            SuccessTime = ReadString(value, "successTime"),
            NotifyUrl = ReadString(value, "notifyUrl"),
            Remark = ReadString(value, "remark"),
            CreateTime = ReadString(value, "createTime"),
            CreateUserId = ReadString(value, "createUserId"),
            CreateUserName = ReadString(value, "createUserName"),
            UpdateTime = ReadString(value, "updateTime"),
            UpdateUserId = ReadString(value, "updateUserId"),
            UpdateUserName = ReadString(value, "updateUserName")
        };

    private static string ReadString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        if (value is null)
            return string.Empty;
        var result = StringValue(value);
        return result == "null" || result == "undefined" ? string.Empty : result;
    }

    private static int ReadInt(object data, string key, int fallback)
    {
        var value = Reflect.Get(data, key);
        return value is null ? fallback : (int)NumberValue(value);
    }

    private static int? ReadNullableInt(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is null ? null : (int)NumberValue(value);
    }
}

public sealed record WechatPayPageInput
{
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("keyword")]
    public string Keyword { get; init; } = string.Empty;
    [ECMAScriptName("createTimeRange")]
    public string[] CreateTimeRange { get; init; } = [];
}

/// <summary>微信 Native 支付请求。businessId 在浏览器 JSON 中保留为十进制字符串。</summary>
public sealed record WechatPayCreateInput
{
    [ECMAScriptName("openId")]
    public string OpenId { get; init; } = string.Empty;
    [ECMAScriptName("total")]
    public int Total { get; init; }
    [ECMAScriptName("description")]
    public string Description { get; init; } = string.Empty;
    [ECMAScriptName("attachment")]
    public string Attachment { get; init; } = string.Empty;
    [ECMAScriptName("goodsTag")]
    public string GoodsTag { get; init; } = string.Empty;
    [ECMAScriptName("tags")]
    public string Tags { get; init; } = string.Empty;
    [ECMAScriptName("businessId")]
    public string BusinessId { get; init; } = "0";
}

public sealed record WechatPayRefundInput
{
    [ECMAScriptName("tradeId")]
    public string TradeId { get; init; } = string.Empty;
    [ECMAScriptName("reason")]
    public string Reason { get; init; } = string.Empty;
    [ECMAScriptName("refund")]
    public int Refund { get; init; }
    [ECMAScriptName("total")]
    public int Total { get; init; }
}

public sealed record WechatPayCreateResult
{
    public string OutTradeNumber { get; init; } = string.Empty;
    public string QrcodeUrl { get; init; } = string.Empty;
}
