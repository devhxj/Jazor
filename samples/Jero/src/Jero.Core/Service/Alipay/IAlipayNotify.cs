


using Aop.Api.Response;
namespace Jero.Core.Service;

/// <summary>
/// 支付宝回调接口
/// </summary>
public abstract class IAlipayNotify
{
    /// <summary>
    /// 充值回调方法
    /// </summary>
    /// <param name="type">交易类型</param>
    /// <param name="tradeNo">交易id</param>
    public abstract bool TopUpCallback(long type, long tradeNo);

    /// <summary>
    /// 扫码回调
    /// </summary>
    /// <param name="type"></param>
    /// <param name="userId"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    public abstract bool ScanCallback(long type, long userId, AlipayUserInfoShareResponse response);
}