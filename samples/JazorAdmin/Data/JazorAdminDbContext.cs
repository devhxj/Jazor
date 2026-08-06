// Configures the shared SQLite store for Identity, OpenIddict, organizations, and operation grants.
// 配置由 Identity、OpenIddict、组织机构和操作授权共同使用的 SQLite 存储模型。
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JazorAdmin.Data;

public sealed class JazorAdminDbContext(DbContextOptions<JazorAdminDbContext> options)
    : IdentityDbContext<JazorAdminUser>(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<OrganizationMembership> OrganizationMemberships => Set<OrganizationMembership>();

    public DbSet<OrganizationRole> OrganizationRoles => Set<OrganizationRole>();

    public DbSet<OrganizationMembershipRole> OrganizationMembershipRoles => Set<OrganizationMembershipRole>();

    public DbSet<AuthorizationResource> AuthorizationResources => Set<AuthorizationResource>();

    public DbSet<AuthorizationOperation> AuthorizationOperations => Set<AuthorizationOperation>();

    public DbSet<ResourceOperationGrant> ResourceOperationGrants => Set<ResourceOperationGrant>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Organization>(entity =>
        {
            entity.Property(organization => organization.Code).HasMaxLength(64);
            entity.Property(organization => organization.DisplayName).HasMaxLength(200);
            entity.HasIndex(organization => organization.Code).IsUnique();
            entity.HasOne(organization => organization.Parent)
                .WithMany(organization => organization.Children)
                .HasForeignKey(organization => organization.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<OrganizationMembership>(entity =>
        {
            entity.HasIndex(membership => new { membership.OrganizationId, membership.UserId }).IsUnique();
            entity.HasOne(membership => membership.User)
                .WithMany()
                .HasForeignKey(membership => membership.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<OrganizationRole>(entity =>
        {
            entity.Property(role => role.Code).HasMaxLength(64);
            entity.Property(role => role.DisplayName).HasMaxLength(200);
            entity.HasIndex(role => new { role.OrganizationId, role.Code }).IsUnique();
        });

        builder.Entity<OrganizationMembershipRole>(entity =>
        {
            entity.HasKey(role => new { role.MembershipId, role.RoleId });
            entity.HasOne(role => role.Membership)
                .WithMany(membership => membership.Roles)
                .HasForeignKey(role => role.MembershipId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(role => role.Role)
                .WithMany(organizationRole => organizationRole.Memberships)
                .HasForeignKey(role => role.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AuthorizationResource>(entity =>
        {
            entity.HasKey(resource => resource.Key);
            entity.Property(resource => resource.Key).HasMaxLength(64);
            entity.Property(resource => resource.DisplayName).HasMaxLength(200);
        });

        builder.Entity<AuthorizationOperation>(entity =>
        {
            entity.HasKey(operation => new { operation.ResourceKey, operation.Key });
            entity.Property(operation => operation.ResourceKey).HasMaxLength(64);
            entity.Property(operation => operation.Key).HasMaxLength(64);
            entity.Property(operation => operation.DisplayName).HasMaxLength(200);
            entity.HasOne(operation => operation.Resource)
                .WithMany(resource => resource.Operations)
                .HasForeignKey(operation => operation.ResourceKey)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ResourceOperationGrant>(entity =>
        {
            entity.HasKey(grant => new { grant.RoleId, grant.ResourceKey, grant.OperationKey });
            entity.Property(grant => grant.ResourceKey).HasMaxLength(64);
            entity.Property(grant => grant.OperationKey).HasMaxLength(64);
            entity.HasOne(grant => grant.Role)
                .WithMany(role => role.Grants)
                .HasForeignKey(grant => grant.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(grant => grant.Operation)
                .WithMany(operation => operation.Grants)
                .HasForeignKey(grant => new { grant.ResourceKey, grant.OperationKey })
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OpenIddict adds its canonical application, authorization, scope, and token mappings to this model.
        // OpenIddict 在此统一加入标准 application、authorization、scope 和 token 映射。
        builder.UseOpenIddict();
    }
}
