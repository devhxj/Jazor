


using Microsoft.AspNetCore.SignalR;
namespace Jero.Core.Service;

/// <summary>
/// 系统消息发送服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 370)]
public class SysMessageService : IDynamicApiController, ITransient
{
    private readonly SysCacheService _sysCacheService;
    private readonly IHubContext<OnlineUserHub, IOnlineUserHub> _chatHubContext;

    public SysMessageService(SysCacheService sysCacheService,
        IHubContext<OnlineUserHub, IOnlineUserHub> chatHubContext)
    {
        _sysCacheService = sysCacheService;
        _chatHubContext = chatHubContext;
    }

    /// <summary>
    /// 发送消息给所有人 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("发送消息给所有人")]
    public async Task SendAllUser(MessageInput input)
    {
        await _chatHubContext.Clients.All.ReceiveMessage(input);
    }

    /// <summary>
    /// 发送消息给除了发送人的其他人 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("发送消息给除了发送人的其他人")]
    public async Task SendOtherUser(MessageInput input)
    {
        var hashKey = _sysCacheService.HashGetAll<SysOnlineUser>(CacheConst.KeyUserOnline);
        var exceptReceiveUsers = hashKey.Where(u => u.Value.UserId == input.ReceiveUserId).Select(u => u.Value).ToList();
        var excludedConnectionIds = exceptReceiveUsers
            .Select(t => t.ConnectionId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Cast<string>()
            .ToList();
        await _chatHubContext.Clients.AllExcept(excludedConnectionIds).ReceiveMessage(input);
    }

    /// <summary>
    /// 发送消息给某个人 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("发送消息给某个人")]
    public async Task SendUser(MessageInput input)
    {
        var hashKey = _sysCacheService.HashGetAll<SysOnlineUser>(CacheConst.KeyUserOnline);
        var receiveUsers = hashKey.Where(u => u.Value.UserId == input.ReceiveUserId).Select(u => u.Value).ToList();
        await receiveUsers.ForEachAsync(u => _chatHubContext.Clients.Client(u.ConnectionId ?? "").ReceiveMessage(input));
    }

    /// <summary>
    /// 发送消息给某些人 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("发送消息给某些人")]
    public async Task SendUsers(MessageInput input)
    {
        var hashKey = _sysCacheService.HashGetAll<SysOnlineUser>(CacheConst.KeyUserOnline);
        var receiveUsers = hashKey.Where(u => input.UserIds.Any(a => a == u.Value.UserId)).Select(u => u.Value).ToList();
        await receiveUsers.ForEachAsync(u => _chatHubContext.Clients.Client(u.ConnectionId ?? "").ReceiveMessage(input));
    }
}
