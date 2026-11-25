namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 后台模拟助手使用的隔离身份。
///
/// 模拟请求必须显式携带标识，并且只能由后台登录态发起；小程序登录态
/// 即使伪造该请求头也不会被当作模拟用户。这样可以复用助手的会话、检索
/// 和工具链，同时不会把测试消息写入真实 OpenId 的数据范围。
/// </summary>
public static class SaveLifeAgentSimulationIdentity
{
    public const string HeaderName = "X-SaveLife-Agent-Simulation";

    private const string EnabledValue = "1";
    private const string EnabledTextValue = "true";

    public static bool IsSimulation(HttpContext? context)
    {
        if (context == null)
            return false;

        var value = context.Request.Headers[HeaderName].FirstOrDefault()?.Trim();
        return string.Equals(value, EnabledValue, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, EnabledTextValue, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 获取当前请求对应的外部用户 Id。普通小程序请求仍使用 OpenId，
    /// 模拟请求则使用稳定的后台账号合成 Id。
    /// </summary>
    public static string ResolveExternalUserId(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (IsSimulation(context))
            return ResolveSimulationExternalUserId(context);

        var openId = context.User.FindFirst(ClaimConst.OpenId)?.Value?.Trim();
        return !string.IsNullOrWhiteSpace(openId)
            ? openId
            : throw Oops.Oh("小程序登录态无效，请重新登录").StatusCode(400);
    }

    /// <summary>
    /// 要求当前请求是经过后台认证的模拟请求。
    /// </summary>
    public static string RequireSimulationExternalUserId(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (!IsSimulation(context))
            throw Oops.Oh("模拟助手请求标识无效").StatusCode(400);

        return ResolveSimulationExternalUserId(context);
    }

    private static string ResolveSimulationExternalUserId(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            throw Oops.Oh("后台登录态无效，请重新登录").StatusCode(401);

        var loginMode = context.User.FindFirst(ClaimConst.LoginMode)?.Value?.Trim();
        if (string.Equals(loginMode, "APP", StringComparison.OrdinalIgnoreCase) ||
            loginMode == ((int)LoginModeEnum.APP).ToString())
        {
            throw Oops.Oh("小程序登录态不能使用后台模拟助手").StatusCode(403);
        }

        var accountTypeValue = context.User.FindFirst(ClaimConst.AccountType)?.Value?.Trim();
        if (!int.TryParse(accountTypeValue, out var accountType) ||
            accountType is not ((int)AccountTypeEnum.SuperAdmin) and not ((int)AccountTypeEnum.SysAdmin))
        {
            throw Oops.Oh("仅系统管理员可以使用后台模拟助手").StatusCode(403);
        }

        if (!long.TryParse(
                context.User.FindFirst(ClaimConst.UserId)?.Value?.Trim(),
                out var userId) ||
            userId <= 0)
        {
            throw Oops.Oh("后台登录态无效，请重新登录").StatusCode(401);
        }

        var tenantId = context.User.FindFirst(ClaimConst.TenantId)?.Value?.Trim();
        if (!long.TryParse(tenantId, out var parsedTenantId) || parsedTenantId <= 0)
            parsedTenantId = SqlSugarConst.DefaultTenantId;

        // 该前缀是模拟身份的命名空间，不能与正常小程序 OpenId 混用。
        return $"admin-sim:{parsedTenantId}:{userId}";
    }
}
