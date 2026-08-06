// Implements dynamic organization-scoped resource-operation policies backed by persisted role grants.
// 实现基于持久化角色授权的动态组织范围资源-操作策略。
using JazorAdmin.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using JazorAdmin.Data;

namespace JazorAdmin.Authorization;

public sealed record ResourceOperationRequirement(string Resource, string Operation) : IAuthorizationRequirement;

public sealed class ResourceOperationAuthorizationPolicyProvider(
    IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public const string PolicyPrefix = "jazoradmin:";

    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!TryParse(policyName, out var requirement))
            return base.GetPolicyAsync(policyName);

        var policy = new AuthorizationPolicyBuilder(JazorAdminAuthentication.ApiScheme)
            .RequireAuthenticatedUser()
            .AddRequirements(requirement)
            .Build();
        return Task.FromResult<AuthorizationPolicy?>(policy);
    }

    public static string CreatePolicyName(string resource, string operation)
        => PolicyPrefix + resource + ":" + operation;

    private static bool TryParse(string policyName, out ResourceOperationRequirement requirement)
    {
        requirement = null!;
        if (!policyName.StartsWith(PolicyPrefix, StringComparison.Ordinal))
            return false;

        var separator = policyName.LastIndexOf(':');
        if (separator <= PolicyPrefix.Length || separator == policyName.Length - 1)
            return false;

        requirement = new ResourceOperationRequirement(
            policyName[PolicyPrefix.Length..separator],
            policyName[(separator + 1)..]);
        return true;
    }
}

public sealed class ResourceOperationAuthorizationHandler(
    JazorAdminDbContext database)
    : AuthorizationHandler<ResourceOperationRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOperationRequirement requirement)
    {
        if (context.User.IsInRole(JazorAdminRoles.PlatformAdministrator))
        {
            context.Succeed(requirement);
            return;
        }

        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? context.User.FindFirst(OpenIddict.Abstractions.OpenIddictConstants.Claims.Subject)?.Value;
        // Endpoint authorization supplies the current request as Resource. Do not reach through
        // IHttpContextAccessor here: the resource is the authorization pipeline's request-local source.
        // Endpoint 授权会将当前请求传入 Resource；此处直接使用它，避免间接上下文误取路由值。
        var requestContext = context.Resource as HttpContext;
        if (string.IsNullOrWhiteSpace(userId) || !TryGetOrganizationId(requestContext, out var organizationId))
            return;

        // Permission is queried on every protected request so removing a grant takes effect immediately.
        // 每个受保护请求都查询授权，角色权限被撤销后无需等待 token 过期即可生效。
        var isGranted = await (
            from membership in database.OrganizationMemberships
            join membershipRole in database.OrganizationMembershipRoles on membership.Id equals membershipRole.MembershipId
            join grant in database.ResourceOperationGrants on membershipRole.RoleId equals grant.RoleId
            where membership.OrganizationId == organizationId
                  && membership.UserId == userId
                  && membership.IsActive
                  && grant.ResourceKey == requirement.Resource
                  && grant.OperationKey == requirement.Operation
            select grant.RoleId).AnyAsync(requestContext?.RequestAborted ?? CancellationToken.None);

        if (isGranted)
            context.Succeed(requirement);
    }

    private static bool TryGetOrganizationId(HttpContext? context, out Guid organizationId)
    {
        organizationId = default;
        if (context is null)
            return false;

        var routeValue = context.Request.RouteValues["organizationId"]?.ToString();
        var requestValue = routeValue ?? context.Request.Headers["X-Jazor-Organization"].FirstOrDefault();
        return Guid.TryParse(requestValue, out organizationId);
    }
}

public static class ResourceOperationEndpointConventionBuilderExtensions
{
    public static TBuilder RequireResourceOperation<TBuilder>(
        this TBuilder builder,
        string resource,
        string operation)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.RequireAuthorization(ResourceOperationAuthorizationPolicyProvider.CreatePolicyName(resource, operation));
        return builder;
    }
}
