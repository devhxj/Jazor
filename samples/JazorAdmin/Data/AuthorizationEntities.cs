// Defines organization membership, role, resource, and operation entities persisted by the authorization model.
// 定义授权模型持久化的组织成员、角色、资源和操作实体。
namespace JazorAdmin.Data;

public sealed class Organization
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Code { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public Guid? ParentId { get; set; }

    public Organization? Parent { get; set; }

    public ICollection<Organization> Children { get; } = new List<Organization>();

    public ICollection<OrganizationMembership> Memberships { get; } = new List<OrganizationMembership>();

    public ICollection<OrganizationRole> Roles { get; } = new List<OrganizationRole>();
}

public sealed class OrganizationMembership
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;

    public JazorAdminUser User { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public ICollection<OrganizationMembershipRole> Roles { get; } = new List<OrganizationMembershipRole>();
}

public sealed class OrganizationRole
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public string Code { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public ICollection<OrganizationMembershipRole> Memberships { get; } = new List<OrganizationMembershipRole>();

    public ICollection<ResourceOperationGrant> Grants { get; } = new List<ResourceOperationGrant>();
}

public sealed class OrganizationMembershipRole
{
    public Guid MembershipId { get; set; }

    public OrganizationMembership Membership { get; set; } = null!;

    public Guid RoleId { get; set; }

    public OrganizationRole Role { get; set; } = null!;
}

public sealed class AuthorizationResource
{
    public string Key { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public ICollection<AuthorizationOperation> Operations { get; } = new List<AuthorizationOperation>();
}

public sealed class AuthorizationOperation
{
    public string ResourceKey { get; set; } = string.Empty;

    public AuthorizationResource Resource { get; set; } = null!;

    public string Key { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public ICollection<ResourceOperationGrant> Grants { get; } = new List<ResourceOperationGrant>();
}

public sealed class ResourceOperationGrant
{
    public Guid RoleId { get; set; }

    public OrganizationRole Role { get; set; } = null!;

    public string ResourceKey { get; set; } = string.Empty;

    public string OperationKey { get; set; } = string.Empty;

    public AuthorizationOperation Operation { get; set; } = null!;
}
