


using Microsoft.AspNetCore.Authentication;
namespace Jero.Core;

/// <summary>
/// Signature 身份验证选项
/// </summary>
public class SignatureAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// 请求时间允许的偏差范围
    /// </summary>
    public TimeSpan AllowedDateDrift { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Signature 身份验证事件
    /// </summary>
    public new SignatureAuthenticationEvent Events
    {
        get
        {
            if (base.Events is SignatureAuthenticationEvent events) return events;

            events = new SignatureAuthenticationEvent();
            base.Events = events;
            return events;
        }
        set => base.Events = value ?? throw new ArgumentNullException(nameof(value));
    }
}
