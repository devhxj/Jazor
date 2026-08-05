// Declares organization, membership, role, and grant DTOs without exposing persistence entities directly.
// 声明组织、成员、角色和授权 DTO，避免将持久化实体直接暴露到 HTTP API。
using JazorAdmin.Features.Identity;

namespace JazorAdmin.Features.Organizations;

public sealed record CreateOrganizationRequest(string Code, string DisplayName);

public sealed record OrganizationDetailResponse(
    string Id,
    string Code,
    string DisplayName,
    string? ParentId,
    OrganizationSummary[] Children);

public sealed record CreateOrganizationRoleRequest(string Code, string DisplayName);

public sealed record OrganizationRoleResponse(string Id, string Code, string DisplayName);

public sealed record ResourceOperationResponse(string Resource, string Operation, string DisplayName);

public sealed record UpdateRoleGrantsRequest(ResourceOperationSelection[] Grants);

public sealed record ResourceOperationSelection(string Resource, string Operation);

public sealed record OrganizationRoleGrantResponse(string Resource, string Operation);

public sealed record CreateOrganizationMemberRequest(string Email, string[] RoleIds);

public sealed record UpdateOrganizationMemberRolesRequest(string[] RoleIds);

public sealed record OrganizationMemberResponse(
    string MembershipId,
    string UserId,
    string Email,
    string DisplayName,
    OrganizationRoleResponse[] Roles);
