using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.WechatPay;

/// <summary>Typed browser boundary for WeChat payment orders and refunds.</summary>
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
                outTradeNumber = ReadString(data, "outTradeNumber"),
                qrcodeUrl = ReadString(data, "qrcodeUrl")
            };

    public static WechatPayView? ReadPayDetail(object? data)
        => data is null ? null : ReadPay(data);

    private static WechatPayView ReadPay(object value)
        => new()
        {
            id = ReadString(value, "id"),
            merchantId = ReadString(value, "merchantId"),
            appId = ReadString(value, "appId"),
            subMerchantId = ReadString(value, "subMerchantId"),
            subAppId = ReadString(value, "subAppId"),
            outTradeNumber = ReadString(value, "outTradeNumber"),
            transactionId = ReadString(value, "transactionId"),
            tradeType = ReadString(value, "tradeType"),
            tradeState = ReadString(value, "tradeState"),
            tradeStateDescription = ReadString(value, "tradeStateDescription"),
            bankType = ReadString(value, "bankType"),
            total = ReadInt(value, "total", 0),
            payerTotal = ReadNullableInt(value, "payerTotal"),
            successTime = ReadString(value, "successTime"),
            expireTime = ReadString(value, "expireTime"),
            description = ReadString(value, "description"),
            scene = ReadString(value, "scene"),
            attachment = ReadString(value, "attachment"),
            goodsTag = ReadString(value, "goodsTag"),
            settlement = ReadString(value, "settlement"),
            notifyUrl = ReadString(value, "notifyUrl"),
            remark = ReadString(value, "remark"),
            openId = ReadString(value, "openId"),
            subOpenId = ReadString(value, "subOpenId"),
            tags = ReadString(value, "tags"),
            businessId = ReadString(value, "businessId"),
            qrcodeContent = ReadString(value, "qrcodeContent"),
            createTime = ReadString(value, "createTime"),
            createUserId = ReadString(value, "createUserId"),
            createUserName = ReadString(value, "createUserName"),
            updateTime = ReadString(value, "updateTime"),
            updateUserId = ReadString(value, "updateUserId"),
            updateUserName = ReadString(value, "updateUserName")
        };

    private static WechatRefundView ReadRefund(object value)
        => new()
        {
            id = ReadString(value, "id"),
            wechatPayId = ReadString(value, "wechatPayId"),
            outRefundNumber = ReadString(value, "outRefundNumber"),
            transactionId = ReadString(value, "transactionId"),
            reason = ReadString(value, "reason"),
            channel = ReadString(value, "channel"),
            userReceivedAccount = ReadString(value, "userReceivedAccount"),
            tradeState = ReadString(value, "tradeState"),
            tradeStateDescription = ReadString(value, "tradeStateDescription"),
            refund = ReadInt(value, "refund", 0),
            successTime = ReadString(value, "successTime"),
            notifyUrl = ReadString(value, "notifyUrl"),
            remark = ReadString(value, "remark"),
            createTime = ReadString(value, "createTime"),
            createUserId = ReadString(value, "createUserId"),
            createUserName = ReadString(value, "createUserName"),
            updateTime = ReadString(value, "updateTime"),
            updateUserId = ReadString(value, "updateUserId"),
            updateUserName = ReadString(value, "updateUserName")
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
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string keyword { get; init; } = string.Empty;
    public string[] createTimeRange { get; init; } = [];
}

/// <summary>Native payment payload; businessId stays decimal text across the JS JSON boundary.</summary>
public sealed record WechatPayCreateInput
{
    public string openId { get; init; } = string.Empty;
    public int total { get; init; }
    public string description { get; init; } = string.Empty;
    public string attachment { get; init; } = string.Empty;
    public string goodsTag { get; init; } = string.Empty;
    public string tags { get; init; } = string.Empty;
    public string businessId { get; init; } = "0";
}

public sealed record WechatPayRefundInput
{
    public string tradeId { get; init; } = string.Empty;
    public string reason { get; init; } = string.Empty;
    public int refund { get; init; }
    public int total { get; init; }
}

public sealed record WechatPayCreateResult
{
    public string outTradeNumber { get; init; } = string.Empty;
    public string qrcodeUrl { get; init; } = string.Empty;
}
