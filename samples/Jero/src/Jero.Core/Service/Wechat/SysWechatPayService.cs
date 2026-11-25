


using System.Text.Json;
namespace Jero.Core.Service;

/// <summary>
/// 微信支付服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 210)]
public class SysWechatPayService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SysWechatPay> _sysWechatPayRep;
    private readonly SqlSugarRepository<SysWechatRefund> _sysWechatRefundRep;

    private readonly WechatPayOptions _wechatPayOptions;
    private readonly PayCallBackOptions _payCallBackOptions;

    private readonly WechatTenpayClient _wechatTenpayClient;

    public SysWechatPayService(SqlSugarRepository<SysWechatPay> sysWechatPayUserRep,
        SqlSugarRepository<SysWechatRefund> sysWechatRefundRep,
        IOptions<WechatPayOptions> wechatPayOptions,
        IOptions<PayCallBackOptions> payCallBackOptions)
    {
        _sysWechatPayRep = sysWechatPayUserRep;
        _sysWechatRefundRep = sysWechatRefundRep;
        _wechatPayOptions = wechatPayOptions.Value;
        _payCallBackOptions = payCallBackOptions.Value;

        _wechatTenpayClient = CreateTenpayClient();
    }

    /// <summary>
    /// 初始化微信支付客户端
    /// </summary>
    /// <returns></returns>
    private WechatTenpayClient CreateTenpayClient()
    {
        var privateKeyPath = _wechatPayOptions.MerchantCertificatePrivateKey ?? string.Empty;
        var cerFilePath = Path.Combine(App.WebHostEnvironment.ContentRootPath, privateKeyPath.Replace("/", Path.DirectorySeparatorChar.ToString()));

        var tenpayClientOptions = new WechatTenpayClientOptions()
        {
            MerchantId = _wechatPayOptions.MerchantId,
            MerchantV3Secret = _wechatPayOptions.MerchantV3Secret,
            MerchantCertificateSerialNumber = _wechatPayOptions.MerchantCertificateSerialNumber,
            MerchantCertificatePrivateKey = File.Exists(cerFilePath) ? File.ReadAllText(cerFilePath) : "",
            PlatformCertificateManager = new InMemoryCertificateManager()
        };
        return new WechatTenpayClient(tenpayClientOptions);
    }

    private static DateTime? GetWechatSuccessTime(DateTimeOffset? successTime, string? tradeState, string context)
    {
        if (successTime.HasValue)
            return successTime.Value.DateTime;

        if (string.Equals(tradeState, "SUCCESS", StringComparison.OrdinalIgnoreCase))
            throw Oops.Oh($"{context}状态为SUCCESS但缺少success_time");

        return null;
    }

    /// <summary>
    /// 分页查询支付列表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<SysWechatPay>> Page(WechatPayPageInput input)
    {
        var startTime = input.CreateTimeRange is { Count: > 0 } ? input.CreateTimeRange[0] : null;
        var endTime = input.CreateTimeRange is { Count: > 1 } ? input.CreateTimeRange[1] : null;
        var hasStartTime = startTime.HasValue;
        var hasEndTime = endTime.HasValue;
        var startTimeValue = startTime.GetValueOrDefault();
        var endTimeExclusive = endTime.GetValueOrDefault().AddDays(1);

        var query = _sysWechatPayRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.OutTradeNumber == input.Keyword || u.TransactionId == input.Keyword)
            .WhereIF(hasStartTime, x => x.CreateTime >= startTimeValue)
            .WhereIF(hasEndTime, x => x.CreateTime < endTimeExclusive);
        return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 查询退款信息列表
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost]
    [DisplayName("根据支付id获取退款信息列表")]
    public async Task<List<SysWechatRefund>> ListRefund([FromBody] string id)
    {
        var query = _sysWechatRefundRep.AsQueryable()
            .Where(u => u.TransactionId == id);
        return await query.ToListAsync();
    }

    /// <summary>
    /// 生成JSAPI调起支付所需参数 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("生成JSAPI调起支付所需参数")]
    public WechatPayParaOutput GenerateParametersForJsapiPay(WechatPayParaInput input)
    {
        var data = _wechatTenpayClient.GenerateParametersForJsapiPayRequest(_wechatPayOptions.AppId, input.PrepayId);
        return new WechatPayParaOutput()
        {
            AppId = data["appId"],
            TimeStamp = data["timeStamp"],
            NonceStr = data["nonceStr"],
            Package = data["package"],
            SignType = data["signType"],
            PaySign = data["paySign"]
        };
    }

    /// <summary>
    /// 微信支付下单(商户直连) 🔖
    /// </summary>
    [DisplayName("微信支付下单(商户直连)")]
    public async Task<WechatPayTransactionOutput> CreatePayTransaction([FromBody] WechatPayTransactionInput input)
    {
        var request = new CreatePayTransactionJsapiRequest()
        {
            OutTradeNumber = DateTimeOffset.Now.ToString("yyyyMMddHHmmssfff") + (new Random()).Next(100, 1000), // 订单号
            AppId = _wechatPayOptions.AppId,
            Description = input.Description,
            Attachment = input.Attachment,
            GoodsTag = input.GoodsTag,
            ExpireTime = DateTimeOffset.Now.AddMinutes(10),
            NotifyUrl = _payCallBackOptions.WechatPayUrl,
            Amount = new CreatePayTransactionJsapiRequest.Types.Amount() { Total = input.Total },
            Payer = new CreatePayTransactionJsapiRequest.Types.Payer() { OpenId = input.OpenId }
        };
        var response = await _wechatTenpayClient.ExecuteCreatePayTransactionJsapiAsync(request);
        if (!response.IsSuccessful())
            throw Oops.Oh(response.ErrorMessage);

        var prepayId = response.PrepayId ?? throw Oops.Oh("微信支付下单响应缺少预支付单号");
        var singInfo = this.GenerateParametersForJsapiPay(new WechatPayParaInput() { PrepayId = prepayId });
        // 保存订单信息
        var wechatPay = new SysWechatPay()
        {
            AppId = _wechatPayOptions.AppId,
            MerchantId = _wechatPayOptions.MerchantId,
            OutTradeNumber = request.OutTradeNumber,
            Description = input.Description,
            Attachment = input.Attachment,
            GoodsTag = input.GoodsTag,
            Total = input.Total,
            OpenId = input.OpenId,
            TransactionId = "",
            Tags = input.Tags,
            BusinessId = input.BusinessId,
        };
        await _sysWechatPayRep.InsertAsync(wechatPay);

        return new WechatPayTransactionOutput()
        {
            PrepayId = prepayId,
            OutTradeNumber = request.OutTradeNumber,
            SingInfo = singInfo
        };
    }

    /// <summary>
    /// 微信支付下单(商户直连)Native
    /// </summary>
    [DisplayName("微信支付下单(商户直连)Native")]
    public async Task<dynamic> CreatePayTransactionNative([FromBody] WechatPayTransactionInput input)
    {
        var request = new CreatePayTransactionNativeRequest()
        {
            OutTradeNumber = DateTimeOffset.Now.ToString("yyyyMMddHHmmssfff") + (new Random()).Next(100, 1000), // 订单号
            AppId = _wechatPayOptions.AppId,
            Description = input.Description,
            Attachment = input.Attachment,
            GoodsTag = input.GoodsTag,
            ExpireTime = DateTimeOffset.Now.AddMinutes(10),
            NotifyUrl = _payCallBackOptions.WechatPayUrl,
            Amount = new CreatePayTransactionNativeRequest.Types.Amount() { Total = input.Total },
            //Payer = new CreatePayTransactionNativeRequest.Types.Payer() { OpenId = input.OpenId }
            Scene = new CreatePayTransactionNativeRequest.Types.Scene() { ClientIp = "127.0.0.1" }
        };
        var response = await _wechatTenpayClient.ExecuteCreatePayTransactionNativeAsync(request);
        if (!response.IsSuccessful())
        {
            Log.Information("{Response}", JsonSerializer.Serialize(response, SystemTextJsonSerializer.GetDefaultSerializerOptions()));
            throw Oops.Oh(response.ErrorMessage);
        }
        // 保存订单信息
        var wechatPay = new SysWechatPay()
        {
            AppId = _wechatPayOptions.AppId,
            MerchantId = _wechatPayOptions.MerchantId,
            OutTradeNumber = request.OutTradeNumber,
            Description = input.Description,
            Attachment = input.Attachment,
            GoodsTag = input.GoodsTag,
            Total = input.Total,
            //OpenId = input.OpenId,
            TransactionId = "",
            QrcodeContent = response.QrcodeUrl,
            Tags = input.Tags,
            BusinessId = input.BusinessId,
        };
        await _sysWechatPayRep.InsertAsync(wechatPay);
        return new
        {
            request.OutTradeNumber,
            response.QrcodeUrl
        };
    }

    /// <summary>
    /// 微信支付下单(服务商模式) 🔖
    /// </summary>
    [DisplayName("微信支付下单(服务商模式)")]
    public async Task<dynamic> CreatePayPartnerTransaction([FromBody] WechatPayTransactionInput input)
    {
        var request = new CreatePayPartnerTransactionJsapiRequest()
        {
            OutTradeNumber = DateTimeOffset.Now.ToString("yyyyMMddHHmmssfff") + (new Random()).Next(100, 1000), // 订单号
            AppId = _wechatPayOptions.AppId,
            MerchantId = _wechatPayOptions.MerchantId,
            SubAppId = _wechatPayOptions.AppId,
            SubMerchantId = _wechatPayOptions.MerchantId,
            Description = input.Description,
            Attachment = input.Attachment,
            GoodsTag = input.GoodsTag,
            ExpireTime = DateTimeOffset.Now.AddMinutes(10),
            NotifyUrl = _payCallBackOptions.WechatPayUrl,
            Amount = new CreatePayPartnerTransactionJsapiRequest.Types.Amount() { Total = input.Total },
            Payer = new CreatePayPartnerTransactionJsapiRequest.Types.Payer() { OpenId = input.OpenId }
        };
        var response = await _wechatTenpayClient.ExecuteCreatePayPartnerTransactionJsapiAsync(request);
        if (!response.IsSuccessful())
            throw Oops.Oh(response.ErrorMessage);
        var prepayId = response.PrepayId ?? throw Oops.Oh("微信支付下单响应缺少预支付单号");
        var singInfo = this.GenerateParametersForJsapiPay(new WechatPayParaInput() { PrepayId = prepayId });
        // 保存订单信息
        var wechatPay = new SysWechatPay()
        {
            AppId = _wechatPayOptions.AppId,
            MerchantId = _wechatPayOptions.MerchantId,
            SubAppId = _wechatPayOptions.AppId,
            SubMerchantId = _wechatPayOptions.MerchantId,
            OutTradeNumber = request.OutTradeNumber,
            Description = input.Description,
            Attachment = input.Attachment,
            GoodsTag = input.GoodsTag,
            Total = input.Total,
            OpenId = input.OpenId,
            TransactionId = ""
        };
        await _sysWechatPayRep.InsertAsync(wechatPay);

        return new
        {
            PrepayId = prepayId,
            request.OutTradeNumber,
            singInfo
        };
    }

    /// <summary>
    /// 获取支付订单详情(本地库) 🔖
    /// </summary>
    /// <param name="tradeId"></param>
    /// <returns></returns>
    [DisplayName("获取支付订单详情(本地库)")]
    public async Task<SysWechatPay> GetPayInfo(string tradeId)
    {
        return await _sysWechatPayRep.GetFirstAsync(u => u.OutTradeNumber == tradeId) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
    }

    /// <summary>
    /// 获取支付订单详情(微信接口) 🔖
    /// </summary>
    /// <param name="tradeId"></param>
    /// <returns></returns>
    [DisplayName("获取支付订单详情(微信接口)")]
    public async Task<SysWechatPay> GetPayInfoFromWechat(string tradeId)
    {
        var request = new GetPayTransactionByOutTradeNumberRequest();
        request.OutTradeNumber = tradeId;
        var response = await _wechatTenpayClient.ExecuteGetPayTransactionByOutTradeNumberAsync(request);
        var amount = response.Amount ?? throw Oops.Oh("微信支付订单金额信息为空");
        var successTime = GetWechatSuccessTime(response.SuccessTime, response.TradeState, $"支付订单[{tradeId}]");

        // 修改订单支付状态
        var wechatPay = await _sysWechatPayRep.GetFirstAsync(u => u.OutTradeNumber == response.OutTradeNumber
            && u.MerchantId == response.MerchantId);
        // 如果状态不一致就更新数据库中的记录
        if (wechatPay != null && wechatPay.TradeState != response.TradeState)
        {
            wechatPay.OpenId = response.Payer?.OpenId ?? string.Empty;
            wechatPay.TransactionId = response.TransactionId ?? string.Empty; // 支付订单号
            wechatPay.TradeType = response.TradeType ?? string.Empty; // 交易类型
            wechatPay.TradeState = response.TradeState ?? string.Empty; // 交易状态
            wechatPay.TradeStateDescription = response.TradeStateDescription ?? string.Empty; // 交易状态描述
            wechatPay.BankType = response.BankType ?? string.Empty; // 付款银行类型
            wechatPay.Total = amount.Total; // 订单总金额
            wechatPay.PayerTotal = amount.PayerTotal; // 用户支付金额
            wechatPay.SuccessTime = successTime; // 支付完成时间
            await _sysWechatPayRep.AsUpdateable(wechatPay).IgnoreColumns(true).ExecuteCommandAsync();
        }
        wechatPay = new SysWechatPay()
        {
            AppId = _wechatPayOptions.AppId,
            MerchantId = _wechatPayOptions.MerchantId,
            SubAppId = _wechatPayOptions.AppId,
            SubMerchantId = _wechatPayOptions.MerchantId,
            OutTradeNumber = request.OutTradeNumber,
            Attachment = response.Attachment ?? string.Empty,
            Total = amount.Total, // 订单总金额
            TransactionId = response.TransactionId ?? string.Empty,
            TradeType = response.TradeType ?? string.Empty, // 交易类型
            TradeState = response.TradeState ?? string.Empty, // 交易状态
            TradeStateDescription = response.TradeStateDescription ?? string.Empty, // 交易状态描述
            BankType = response.BankType ?? string.Empty, // 付款银行类型
            PayerTotal = amount.PayerTotal, // 用户支付金额
            SuccessTime = successTime // 支付完成时间
        };
        return wechatPay;
    }

    /// <summary>
    /// 退款申请
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("退款申请")]
    [HttpPost]
    public async Task<dynamic> CreateRefundDomestic([FromBody] WechatPayRefundDomesticInput input)
    {
        // refund/domestic/refunds
        var request = new CreateRefundDomesticRefundRequest()
        {
            Amount = new CreateRefundDomesticRefundRequest.Types.Amount()
            {
                Refund = input.Refund,
                Total = input.Total,
                Currency = "CNY"
            },

            OutTradeNumber = input.TradeId,
            OutRefundNumber = "R" + DateTimeOffset.Now.ToString("yyyyMMddHHmmssfff") + (new Random()).Next(100, 1000), // 订单号
            NotifyUrl = _payCallBackOptions.WechatRefundUrl, // 应采用WechatRefundUrl参数，如与WechatPayUrl入口相同，也应分开设置参数
            Reason = input.Reason,
        };
        var response = await _wechatTenpayClient.ExecuteCreateRefundDomesticRefundAsync(request);
        if (string.IsNullOrEmpty(response.ErrorCode))
        {
            // 成功了，这里应该保存退款订单信息
            var wechatPay = await _sysWechatPayRep.GetFirstAsync(u => u.OutTradeNumber == response.OutTradeNumber);
            // 保存订单信息
            if (wechatPay != null)
            {
                var wechatRefund = new SysWechatRefund()
                {
                    WechatPayId = wechatPay.Id,
                    TransactionId = response.TransactionId ?? throw Oops.Oh("微信退款响应缺少支付订单号"),
                    Refund = input.Refund,
                    Reason = input.Reason,
                    OutRefundNumber = request.OutRefundNumber,
                    Channel = response.Channel,
                    UserReceivedAccount = response.UserReceivedAccount,
                };
                await _sysWechatRefundRep.InsertAsync(wechatRefund);
            }
        }
        else
        {
            throw Oops.Bah($"[{response.ErrorCode}]{response.ErrorMessage}");
        }
        return response;
    }

    /// <summary>
    /// 获取退款订单详情(微信接口)
    /// </summary>
    /// <param name="refundId"></param>
    /// <returns></returns>
    [DisplayName("获取退款订单详情(微信接口)")]
    public async Task<SysWechatRefund> GetRefundInfoFromWechat(string refundId)
    {
        var request = new GetRefundDomesticRefundByOutRefundNumberRequest();
        request.OutRefundNumber = refundId;
        var response = await _wechatTenpayClient.ExecuteGetRefundDomesticRefundByOutRefundNumberAsync(request);
        var amount = response.Amount ?? throw Oops.Oh("微信退款订单金额信息为空");
        var successTime = GetWechatSuccessTime(response.SuccessTime, response.Status, $"退款订单[{refundId}]");

        // 修改订单支付状态
        var wechatRefund = await _sysWechatRefundRep.GetFirstAsync(u => u.OutRefundNumber == refundId);
        // 如果状态不一致就更新数据库中的记录
        if (wechatRefund != null && wechatRefund.TradeState != response.Status)
        {
            wechatRefund.TransactionId = response.TransactionId ?? string.Empty; // 支付订单号
            wechatRefund.TradeState = response.Status ?? string.Empty; // 交易状态
            wechatRefund.SuccessTime = successTime; // 支付完成时间
            await _sysWechatRefundRep.AsUpdateable(wechatRefund).IgnoreColumns(true).ExecuteCommandAsync();
            // 有退款，刷新一下订单状态
            var wechatPay = await _sysWechatPayRep.GetFirstAsync(u => u.Id == wechatRefund.WechatPayId);
            if (wechatPay != null)
                await GetPayInfoFromWechat(wechatPay.OutTradeNumber);
        }
        wechatRefund = new SysWechatRefund()
        {
            TransactionId = response.TransactionId ?? string.Empty,
            Refund = amount.Refund,
            OutRefundNumber = request.OutRefundNumber,
            Channel = response.Channel ?? string.Empty,
            UserReceivedAccount = response.UserReceivedAccount ?? string.Empty,
            TradeState = response.Status ?? string.Empty, // 交易状态
            SuccessTime = successTime, // 支付完成时间
        };
        return wechatRefund;
    }

    /// <summary>
    /// 微信支付成功回调(商户直连)
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("微信支付成功回调(商户直连)")]
    public async Task<WechatPayOutput?> PayCallBack()
    {
        using var ms = new MemoryStream();
        await App.HttpContext.Request.Body.CopyToAsync(ms);
        var b = ms.ToArray();
        var callbackJson = Encoding.UTF8.GetString(b);

        var callbackModel = _wechatTenpayClient.DeserializeEvent(callbackJson);
        if ("TRANSACTION.SUCCESS".Equals(callbackModel.EventType))
        {
            try
            {
                var callbackPayResource = _wechatTenpayClient.DecryptEventResource<TransactionResource>(callbackModel);
                var callbackAmount = callbackPayResource.Amount ?? throw Oops.Oh("微信支付回调金额信息为空");

                // 修改订单支付状态
                var wechatPay = await _sysWechatPayRep.GetFirstAsync(u => u.OutTradeNumber == callbackPayResource.OutTradeNumber
                    && u.MerchantId == callbackPayResource.MerchantId);
                if (wechatPay == null) return null;
                wechatPay.OpenId = callbackPayResource.Payer?.OpenId ?? string.Empty; // 支付者标识
                //wechatPay.MerchantId = callbackResource.MerchantId; // 微信商户号
                //wechatPay.OutTradeNumber = callbackResource.OutTradeNumber; // 商户订单号
                wechatPay.TransactionId = callbackPayResource.TransactionId ?? string.Empty; // 支付订单号
                wechatPay.TradeType = callbackPayResource.TradeType ?? string.Empty; // 交易类型
                wechatPay.TradeState = callbackPayResource.TradeState ?? string.Empty; // 交易状态
                wechatPay.TradeStateDescription = callbackPayResource.TradeStateDescription ?? string.Empty; // 交易状态描述
                wechatPay.BankType = callbackPayResource.BankType ?? string.Empty; // 付款银行类型
                wechatPay.Total = callbackAmount.Total; // 订单总金额
                wechatPay.PayerTotal = callbackAmount.PayerTotal; // 用户支付金额
                wechatPay.SuccessTime = callbackPayResource.SuccessTime.DateTime; // 支付完成时间

                await _sysWechatPayRep.AsUpdateable(wechatPay).IgnoreColumns(true).ExecuteCommandAsync();

                return new WechatPayOutput()
                {
                    Total = wechatPay.Total,
                    Attachment = wechatPay.Attachment ?? string.Empty,
                    GoodsTag = wechatPay.GoodsTag ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                Log.Error("微信支付回调时出错：", ex);
            }
        }
        else if ("REFUND.SUCCESS".Equals(callbackModel.EventType))
        {
            //参考：https://pay.weixin.qq.com/docs/merchant/apis/jsapi-payment/refund-result-notice.html
            try
            {
                var callbackRefundResource = _wechatTenpayClient.DecryptEventResource<RefundResource>(callbackModel);
                // 修改订单支付状态
                var wechatRefund = await _sysWechatRefundRep.GetFirstAsync(u => u.OutRefundNumber == callbackRefundResource.OutRefundNumber);
                if (wechatRefund == null) return null;
                wechatRefund.TradeState = callbackRefundResource.RefundStatus ?? string.Empty; // 交易状态
                wechatRefund.SuccessTime = GetWechatSuccessTime(callbackRefundResource.SuccessTime, callbackRefundResource.RefundStatus, $"退款回调[{callbackRefundResource.OutRefundNumber}]"); // 支付完成时间

                await _sysWechatRefundRep.AsUpdateable(wechatRefund).IgnoreColumns(true).ExecuteCommandAsync();
                // 有退款，刷新一下订单状态
                var tradeId = callbackRefundResource.OutTradeNumber ?? throw Oops.Oh("微信退款回调缺少商户订单号");
                await GetPayInfoFromWechat(tradeId);
            }
            catch (Exception ex)
            {
                Log.Error("微信退款回调时出错：", ex);
            }
        }
        else
        {
            Log.Information("{EventType}", callbackModel.EventType);
        }

        return null;
    }

    /// <summary>
    /// 微信支付成功回调(服务商模式) 🔖
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("微信支付成功回调(服务商模式)")]
    public async Task PayPartnerCallBack()
    {
        using var ms = new MemoryStream();
        await App.HttpContext.Request.Body.CopyToAsync(ms);
        var b = ms.ToArray();
        var callbackJson = Encoding.UTF8.GetString(b);

        var callbackModel = _wechatTenpayClient.DeserializeEvent(callbackJson);
        if ("TRANSACTION.SUCCESS".Equals(callbackModel.EventType))
        {
            var callbackResource = _wechatTenpayClient.DecryptEventResource<PartnerTransactionResource>(callbackModel);
            var callbackAmount = callbackResource.Amount ?? throw Oops.Oh("微信支付回调金额信息为空");

            // 修改订单支付状态
            var wechatPay = await _sysWechatPayRep.GetFirstAsync(u => u.OutTradeNumber == callbackResource.OutTradeNumber
                && u.MerchantId == callbackResource.MerchantId);
            if (wechatPay == null) return;
            //wechatPay.OpenId = callbackResource.Payer.OpenId; // 支付者标识
            //wechatPay.MerchantId = callbackResource.MerchantId; // 微信商户号
            //wechatPay.OutTradeNumber = callbackResource.OutTradeNumber; // 商户订单号
            wechatPay.TransactionId = callbackResource.TransactionId ?? throw Oops.Oh("微信支付回调缺少支付订单号");
            wechatPay.TradeType = callbackResource.TradeType; // 交易类型
            wechatPay.TradeState = callbackResource.TradeState; // 交易状态
            wechatPay.TradeStateDescription = callbackResource.TradeStateDescription; // 交易状态描述
            wechatPay.BankType = callbackResource.BankType; // 付款银行类型
            wechatPay.Total = callbackAmount.Total; // 订单总金额
            wechatPay.PayerTotal = callbackAmount.PayerTotal; // 用户支付金额
            wechatPay.SuccessTime = callbackResource.SuccessTime.DateTime; // 支付完成时间

            await _sysWechatPayRep.AsUpdateable(wechatPay).IgnoreColumns(true).ExecuteCommandAsync();
        }
    }
}
