using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Jero.Core;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public sealed class SaveLifeAgentSimulationIdentityTests
{
    [Fact]
    public void ResolveExternalUserId_UsesStableTenantAndAdminIdentity()
    {
        var context = CreateContext(
            AccountTypeEnum.SuperAdmin,
            userId: 1300000000101,
            tenantId: 1300000000001,
            simulation: true);

        var externalUserId = SaveLifeAgentSimulationIdentity.ResolveExternalUserId(context);

        Assert.Equal("admin-sim:1300000000001:1300000000101", externalUserId);
    }

    [Theory]
    [InlineData(AccountTypeEnum.NormalUser)]
    [InlineData(AccountTypeEnum.Member)]
    public void RequireSimulationExternalUserId_RejectsNonAdmin(AccountTypeEnum accountType)
    {
        var context = CreateContext(
            accountType,
            userId: 1300000000115,
            tenantId: 1300000000001,
            simulation: true);

        var exception = Assert.ThrowsAny<Exception>(
            () => SaveLifeAgentSimulationIdentity.RequireSimulationExternalUserId(context));
        Assert.NotNull(exception);
    }

    [Fact]
    public void RequireSimulationExternalUserId_RejectsMiniAppLogin()
    {
        var context = CreateContext(
            AccountTypeEnum.SuperAdmin,
            userId: 1300000000101,
            tenantId: 1300000000001,
            simulation: true,
            loginMode: LoginModeEnum.APP);

        var exception = Assert.ThrowsAny<Exception>(
            () => SaveLifeAgentSimulationIdentity.RequireSimulationExternalUserId(context));
        Assert.NotNull(exception);
    }

    [Fact]
    public void ResolveExternalUserId_LeavesNormalMiniAppOpenIdUnchanged()
    {
        var context = CreateContext(
            AccountTypeEnum.Member,
            userId: 1300000000115,
            tenantId: 1300000000001,
            simulation: false,
            openId: "wx-open-id");

        Assert.Equal("wx-open-id", SaveLifeAgentSimulationIdentity.ResolveExternalUserId(context));
    }

    private static DefaultHttpContext CreateContext(
        AccountTypeEnum accountType,
        long userId,
        long tenantId,
        bool simulation,
        LoginModeEnum? loginMode = null,
        string? openId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimConst.UserId, userId.ToString()),
            new(ClaimConst.TenantId, tenantId.ToString()),
            new(ClaimConst.AccountType, ((int)accountType).ToString()),
        };
        if (loginMode.HasValue)
            claims.Add(new Claim(ClaimConst.LoginMode, ((int)loginMode.Value).ToString()));
        if (!string.IsNullOrWhiteSpace(openId))
            claims.Add(new Claim(ClaimConst.OpenId, openId));

        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
        };
        if (simulation)
            context.Request.Headers[SaveLifeAgentSimulationIdentity.HeaderName] = "1";
        return context;
    }
}
