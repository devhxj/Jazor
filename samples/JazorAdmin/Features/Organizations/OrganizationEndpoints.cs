// Maps organization administration APIs and keeps all state-changing operations behind persisted grants.
// 映射组织机构管理 API，并将所有变更操作置于持久化授权校验之后。
using JazorAdmin.Authorization;
using JazorAdmin.Data;
using JazorAdmin.Features.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JazorAdmin.Features.Organizations;

public static class OrganizationEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationEndpoints(this IEndpointRouteBuilder app)
    {
        var organizations = app.MapGroup("/api/organizations")
            .WithTags("Organizations")
            .RequireAuthorization();

        organizations.MapGet("/", ListOrganizationsAsync);
        organizations.MapPost("/", CreateRootOrganizationAsync)
            .RequireAuthorization(PolicyKeys.PlatformAdministrator);
        organizations.MapGet("/{organizationId:guid}", GetOrganizationAsync)
            .RequireResourceOperation(ResourceKeys.Organizations, OperationKeys.Read);
        organizations.MapPost("/{organizationId:guid}/children", CreateChildOrganizationAsync)
            .RequireResourceOperation(ResourceKeys.Organizations, OperationKeys.Manage);

        organizations.MapGet("/{organizationId:guid}/roles", ListRolesAsync)
            .RequireResourceOperation(ResourceKeys.Authorization, OperationKeys.Manage);
        organizations.MapPost("/{organizationId:guid}/roles", CreateRoleAsync)
            .RequireResourceOperation(ResourceKeys.Authorization, OperationKeys.Manage);
        organizations.MapGet("/{organizationId:guid}/roles/{roleId:guid}/grants", ListRoleGrantsAsync)
            .RequireResourceOperation(ResourceKeys.Authorization, OperationKeys.Manage);
        organizations.MapPut("/{organizationId:guid}/roles/{roleId:guid}/grants", ReplaceRoleGrantsAsync)
            .RequireResourceOperation(ResourceKeys.Authorization, OperationKeys.Manage);
        organizations.MapGet("/{organizationId:guid}/authorization-resources", ListAuthorizationResourcesAsync)
            .RequireResourceOperation(ResourceKeys.Authorization, OperationKeys.Manage);

        organizations.MapGet("/{organizationId:guid}/members", ListMembersAsync)
            .RequireResourceOperation(ResourceKeys.Organizations, OperationKeys.Manage);
        organizations.MapPost("/{organizationId:guid}/members", CreateMemberAsync)
            .RequireResourceOperation(ResourceKeys.Organizations, OperationKeys.Manage);
        organizations.MapPut("/{organizationId:guid}/members/{membershipId:guid}/roles", ReplaceMemberRolesAsync)
            .RequireResourceOperation(ResourceKeys.Organizations, OperationKeys.Manage);
        return app;
    }

    private static async Task<IResult> ListOrganizationsAsync(
        HttpContext context,
        UserManager<AdminUser> users,
        AdminDbContext database)
    {
        var user = await users.GetUserAsync(context.User);
        if (user is null)
            return Results.Unauthorized();

        var organizations = await database.OrganizationMemberships
            .AsNoTracking()
            .Where(membership => membership.UserId == user.Id && membership.IsActive)
            .OrderBy(membership => membership.Organization.DisplayName)
            .Select(membership => new OrganizationSummary(
                membership.OrganizationId.ToString(),
                membership.Organization.Code,
                membership.Organization.DisplayName))
            .ToArrayAsync(context.RequestAborted);
        return Results.Ok(organizations);
    }

    private static async Task<IResult> CreateRootOrganizationAsync(
        CreateOrganizationRequest request,
        AdminDbContext database,
        HttpContext context)
    {
        if (!TryValidateOrganization(request, out var errors))
            return Results.ValidationProblem(errors);

        if (await database.Organizations.AnyAsync(organization => organization.Code == request.Code, context.RequestAborted))
            return Results.Conflict(new { message = "An organization with this code already exists." });

        var organization = new Organization { Code = request.Code, DisplayName = request.DisplayName };
        database.Organizations.Add(organization);
        await database.SaveChangesAsync(context.RequestAborted);
        return Results.Created("/api/organizations/" + organization.Id, ToDetail(organization, []));
    }

    private static async Task<IResult> GetOrganizationAsync(
        Guid organizationId,
        AdminDbContext database,
        HttpContext context)
    {
        var organization = await database.Organizations
            .AsNoTracking()
            .Include(item => item.Children)
            .SingleOrDefaultAsync(item => item.Id == organizationId, context.RequestAborted);
        return organization is null
            ? Results.NotFound()
            : Results.Ok(ToDetail(organization, organization.Children));
    }

    private static async Task<IResult> CreateChildOrganizationAsync(
        Guid organizationId,
        CreateOrganizationRequest request,
        AdminDbContext database,
        HttpContext context)
    {
        if (!TryValidateOrganization(request, out var errors))
            return Results.ValidationProblem(errors);

        if (!await database.Organizations.AnyAsync(organization => organization.Id == organizationId, context.RequestAborted))
            return Results.NotFound();
        if (await database.Organizations.AnyAsync(organization => organization.Code == request.Code, context.RequestAborted))
            return Results.Conflict(new { message = "An organization with this code already exists." });

        var organization = new Organization
        {
            Code = request.Code,
            DisplayName = request.DisplayName,
            ParentId = organizationId
        };
        database.Organizations.Add(organization);
        await database.SaveChangesAsync(context.RequestAborted);
        return Results.Created("/api/organizations/" + organization.Id, ToDetail(organization, []));
    }

    private static async Task<IResult> ListRolesAsync(Guid organizationId, AdminDbContext database, HttpContext context)
    {
        var roles = await database.OrganizationRoles
            .AsNoTracking()
            .Where(role => role.OrganizationId == organizationId)
            .OrderBy(role => role.Code)
            .Select(role => new OrganizationRoleResponse(role.Id.ToString(), role.Code, role.DisplayName))
            .ToArrayAsync(context.RequestAborted);
        return Results.Ok(roles);
    }

    private static async Task<IResult> CreateRoleAsync(
        Guid organizationId,
        CreateOrganizationRoleRequest request,
        AdminDbContext database,
        HttpContext context)
    {
        if (!TryValidateRole(request, out var errors))
            return Results.ValidationProblem(errors);
        if (!await database.Organizations.AnyAsync(organization => organization.Id == organizationId, context.RequestAborted))
            return Results.NotFound();
        if (await database.OrganizationRoles.AnyAsync(
                role => role.OrganizationId == organizationId && role.Code == request.Code,
                context.RequestAborted))
        {
            return Results.Conflict(new { message = "A role with this code already exists in the organization." });
        }

        var role = new OrganizationRole
        {
            OrganizationId = organizationId,
            Code = request.Code,
            DisplayName = request.DisplayName
        };
        database.OrganizationRoles.Add(role);
        await database.SaveChangesAsync(context.RequestAborted);
        return Results.Created(
            "/api/organizations/" + organizationId + "/roles/" + role.Id,
            new OrganizationRoleResponse(role.Id.ToString(), role.Code, role.DisplayName));
    }

    private static async Task<IResult> ListRoleGrantsAsync(
        Guid organizationId,
        Guid roleId,
        AdminDbContext database,
        HttpContext context)
    {
        var grants = await database.ResourceOperationGrants
            .AsNoTracking()
            .Where(grant => grant.RoleId == roleId && grant.Role.OrganizationId == organizationId)
            .OrderBy(grant => grant.ResourceKey)
            .ThenBy(grant => grant.OperationKey)
            .Select(grant => new OrganizationRoleGrantResponse(grant.ResourceKey, grant.OperationKey))
            .ToArrayAsync(context.RequestAborted);
        return Results.Ok(grants);
    }

    private static async Task<IResult> ReplaceRoleGrantsAsync(
        Guid organizationId,
        Guid roleId,
        UpdateRoleGrantsRequest request,
        AdminDbContext database,
        HttpContext context)
    {
        var role = await database.OrganizationRoles
            .SingleOrDefaultAsync(item => item.Id == roleId && item.OrganizationId == organizationId, context.RequestAborted);
        if (role is null)
            return Results.NotFound();

        var selected = request.Grants
            .Where(grant => !string.IsNullOrWhiteSpace(grant.Resource) && !string.IsNullOrWhiteSpace(grant.Operation))
            .DistinctBy(grant => (grant.Resource, grant.Operation))
            .ToArray();
        if (selected.Length != request.Grants.Length)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["grants"] = ["Each grant requires a unique resource and operation."]
            });
        }

        var selectedResources = selected.Select(grant => grant.Resource).Distinct().ToArray();
        var knownOperations = await database.AuthorizationOperations
            .AsNoTracking()
            .Where(operation => selectedResources.Contains(operation.ResourceKey))
            .Select(operation => new { operation.ResourceKey, operation.Key })
            .ToArrayAsync(context.RequestAborted);
        if (selected.Any(grant => !knownOperations.Any(operation =>
                operation.ResourceKey == grant.Resource && operation.Key == grant.Operation)))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["grants"] = ["The request contains an unknown resource operation."]
            });
        }

        var existing = await database.ResourceOperationGrants
            .Where(grant => grant.RoleId == role.Id)
            .ToArrayAsync(context.RequestAborted);
        database.ResourceOperationGrants.RemoveRange(existing);
        database.ResourceOperationGrants.AddRange(selected.Select(grant => new ResourceOperationGrant
        {
            RoleId = role.Id,
            ResourceKey = grant.Resource,
            OperationKey = grant.Operation
        }));
        await database.SaveChangesAsync(context.RequestAborted);
        return Results.NoContent();
    }

    private static async Task<IResult> ListAuthorizationResourcesAsync(AdminDbContext database, HttpContext context)
    {
        var operations = await database.AuthorizationOperations
            .AsNoTracking()
            .OrderBy(operation => operation.ResourceKey)
            .ThenBy(operation => operation.Key)
            .Select(operation => new ResourceOperationResponse(
                operation.ResourceKey,
                operation.Key,
                operation.DisplayName))
            .ToArrayAsync(context.RequestAborted);
        return Results.Ok(operations);
    }

    private static async Task<IResult> ListMembersAsync(Guid organizationId, AdminDbContext database, HttpContext context)
    {
        var memberships = await database.OrganizationMemberships
            .AsNoTracking()
            .Include(membership => membership.User)
            .Include(membership => membership.Roles)
                .ThenInclude(membershipRole => membershipRole.Role)
            .Where(membership => membership.OrganizationId == organizationId)
            .OrderBy(membership => membership.User.Email)
            .ToArrayAsync(context.RequestAborted);
        return Results.Ok(memberships.Select(ToMemberResponse).ToArray());
    }

    private static async Task<IResult> CreateMemberAsync(
        Guid organizationId,
        CreateOrganizationMemberRequest request,
        UserManager<AdminUser> users,
        AdminDbContext database,
        HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["email"] = ["Email is required."] });

        var user = await users.FindByEmailAsync(request.Email);
        if (user is null)
            return Results.NotFound(new { message = "The account does not exist." });
        if (await database.OrganizationMemberships.AnyAsync(
                membership => membership.OrganizationId == organizationId && membership.UserId == user.Id,
                context.RequestAborted))
        {
            return Results.Conflict(new { message = "The account is already a member of the organization." });
        }

        var roles = await ResolveOrganizationRolesAsync(organizationId, request.RoleIds, database, context.RequestAborted);
        if (roles is null)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["roleIds"] = ["Every role must belong to the organization."] });

        var membership = new OrganizationMembership { OrganizationId = organizationId, UserId = user.Id };
        foreach (var role in roles)
            membership.Roles.Add(new OrganizationMembershipRole { RoleId = role.Id });

        database.OrganizationMemberships.Add(membership);
        await database.SaveChangesAsync(context.RequestAborted);
        return Results.Created(
            "/api/organizations/" + organizationId + "/members/" + membership.Id,
            new OrganizationMemberResponse(
                membership.Id.ToString(),
                user.Id,
                user.Email ?? string.Empty,
                user.DisplayName,
                roles.Select(role => new OrganizationRoleResponse(role.Id.ToString(), role.Code, role.DisplayName)).ToArray()));
    }

    private static async Task<IResult> ReplaceMemberRolesAsync(
        Guid organizationId,
        Guid membershipId,
        UpdateOrganizationMemberRolesRequest request,
        AdminDbContext database,
        HttpContext context)
    {
        var membership = await database.OrganizationMemberships
            .Include(item => item.Roles)
            .SingleOrDefaultAsync(
                item => item.Id == membershipId && item.OrganizationId == organizationId,
                context.RequestAborted);
        if (membership is null)
            return Results.NotFound();

        var roles = await ResolveOrganizationRolesAsync(organizationId, request.RoleIds, database, context.RequestAborted);
        if (roles is null)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["roleIds"] = ["Every role must belong to the organization."] });

        database.OrganizationMembershipRoles.RemoveRange(membership.Roles);
        foreach (var role in roles)
            membership.Roles.Add(new OrganizationMembershipRole { MembershipId = membership.Id, RoleId = role.Id });
        await database.SaveChangesAsync(context.RequestAborted);
        return Results.NoContent();
    }

    private static async Task<OrganizationRole[]?> ResolveOrganizationRolesAsync(
        Guid organizationId,
        string[] roleIds,
        AdminDbContext database,
        CancellationToken cancellationToken)
    {
        var parsedRoleIds = new Guid[roleIds.Length];
        for (var index = 0; index < roleIds.Length; index++)
        {
            if (!Guid.TryParse(roleIds[index], out parsedRoleIds[index]))
                return null;
        }

        var distinctRoleIds = parsedRoleIds.Distinct().ToArray();
        var roles = await database.OrganizationRoles
            .Where(role => role.OrganizationId == organizationId && distinctRoleIds.Contains(role.Id))
            .OrderBy(role => role.Code)
            .ToArrayAsync(cancellationToken);
        return roles.Length == distinctRoleIds.Length ? roles : null;
    }

    private static bool TryValidateOrganization(
        CreateOrganizationRequest request,
        out Dictionary<string, string[]> errors)
    {
        errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Code) || request.Code.Length > 64)
            errors["code"] = ["Code is required and must be at most 64 characters."];
        if (string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Length > 200)
            errors["displayName"] = ["Display name is required and must be at most 200 characters."];
        return errors.Count == 0;
    }

    private static bool TryValidateRole(
        CreateOrganizationRoleRequest request,
        out Dictionary<string, string[]> errors)
    {
        errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Code) || request.Code.Length > 64)
            errors["code"] = ["Code is required and must be at most 64 characters."];
        if (string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Length > 200)
            errors["displayName"] = ["Display name is required and must be at most 200 characters."];
        return errors.Count == 0;
    }

    private static OrganizationDetailResponse ToDetail(Organization organization, IEnumerable<Organization> children)
        => new(
            organization.Id.ToString(),
            organization.Code,
            organization.DisplayName,
            organization.ParentId?.ToString(),
            children
                .OrderBy(child => child.DisplayName)
                .Select(child => new OrganizationSummary(child.Id.ToString(), child.Code, child.DisplayName))
                .ToArray());

    private static OrganizationMemberResponse ToMemberResponse(OrganizationMembership membership)
        => new(
            membership.Id.ToString(),
            membership.UserId,
            membership.User.Email ?? string.Empty,
            membership.User.DisplayName,
            membership.Roles
                .OrderBy(item => item.Role.Code)
                .Select(item => new OrganizationRoleResponse(item.RoleId.ToString(), item.Role.Code, item.Role.DisplayName))
                .ToArray());
}
