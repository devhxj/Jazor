


using Furion.EventBus;
namespace Jero.Application;

/// <summary>
/// 系统用户操作事件订阅
/// </summary>
public class SysUserEventSubscriber : IEventSubscriber, ISingleton, IDisposable
{
    public SysUserEventSubscriber()
    {
    }

    /// <summary>
    /// 增加系统用户
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe(SysUserEventTypeEnum.Add)]
    public Task AddUser(EventHandlerExecutingContext context)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 注册用户
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe(SysUserEventTypeEnum.Register)]
    public Task RegisterUser(EventHandlerExecutingContext context)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 更新系统用户
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe(SysUserEventTypeEnum.Update)]
    public Task UpdateUser(EventHandlerExecutingContext context)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 删除系统用户
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe(SysUserEventTypeEnum.Delete)]
    public Task DeleteUser(EventHandlerExecutingContext context)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 设置系统用户状态
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe(SysUserEventTypeEnum.SetStatus)]
    public Task SetUserStatus(EventHandlerExecutingContext context)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 授权用户角色
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe(SysUserEventTypeEnum.UpdateRole)]
    public Task UpdateUserRole(EventHandlerExecutingContext context)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 解除登录锁定
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe(SysUserEventTypeEnum.UnlockLogin)]
    public Task UnlockUserLogin(EventHandlerExecutingContext context)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 释放服务作用域
    /// </summary>
    public void Dispose()
    {
    }
}