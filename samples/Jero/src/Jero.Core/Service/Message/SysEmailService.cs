


using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
namespace Jero.Core.Service;

/// <summary>
/// 系统邮件发送服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 370)]
public class SysEmailService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SysTenant> _sysTenantRep;
    private readonly EmailOptions _emailOptions;

    public SysEmailService(IOptions<EmailOptions> emailOptions, SqlSugarRepository<SysTenant> sysTenantRep)
    {
        _emailOptions = emailOptions.Value;
        _sysTenantRep = sysTenantRep;
    }

    /// <summary>
    /// 发送邮件 📧
    /// </summary>
    /// <param name="content"></param>
    /// <param name="title"></param>
    /// <param name="toEmail"></param>
    /// <returns></returns>
    [DisplayName("发送邮件")]
    public async Task SendEmail([Required] string content, string title = "", string toEmail = "")
    {
        long.TryParse(App.User?.FindFirst(ClaimConst.TenantId)?.Value ?? SqlSugarConst.DefaultTenantId.ToString(), out var tenantId);
        var webTitle = (await _sysTenantRep.GetFirstAsync(u => u.Id == tenantId))?.Title;
        title = string.IsNullOrWhiteSpace(title) ? $"{webTitle} 系统邮件" : title;
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_emailOptions.DefaultFromEmail, _emailOptions.DefaultFromEmail));

        message.To.Add(string.IsNullOrWhiteSpace(toEmail)
            ? new MailboxAddress(_emailOptions.DefaultToEmail, _emailOptions.DefaultToEmail)
            : new MailboxAddress(toEmail, toEmail));

        message.Subject = title;
        message.Body = new TextPart("html")
        {
            Text = content
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(_emailOptions.Host, _emailOptions.Port, _emailOptions.EnableSsl);
        await client.AuthenticateAsync(_emailOptions.UserName, _emailOptions.Password);
        await client.SendAsync(new SendRequest(message));
        await client.DisconnectAsync(true);
    }
}
