using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JazorAdmin.Data.Migrations;

/// <inheritdoc />
public partial class _20260805081749_InitialIdentityAndAuthorization : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AspNetRoles",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoles", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", nullable: false),
                DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUsers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AuthorizationResources",
            columns: table => new
            {
                Key = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuthorizationResources", x => x.Key);
            });

        migrationBuilder.CreateTable(
            name: "OpenIddictApplications",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", nullable: false),
                ApplicationType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                ClientId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                ClientSecret = table.Column<string>(type: "TEXT", nullable: true),
                ClientType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                ConcurrencyToken = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                ConsentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                DisplayNames = table.Column<string>(type: "TEXT", nullable: true),
                JsonWebKeySet = table.Column<string>(type: "TEXT", nullable: true),
                Permissions = table.Column<string>(type: "TEXT", nullable: true),
                PostLogoutRedirectUris = table.Column<string>(type: "TEXT", nullable: true),
                Properties = table.Column<string>(type: "TEXT", nullable: true),
                RedirectUris = table.Column<string>(type: "TEXT", nullable: true),
                Requirements = table.Column<string>(type: "TEXT", nullable: true),
                Settings = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OpenIddictApplications", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "OpenIddictScopes",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", nullable: false),
                ConcurrencyToken = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                Description = table.Column<string>(type: "TEXT", nullable: true),
                Descriptions = table.Column<string>(type: "TEXT", nullable: true),
                DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                DisplayNames = table.Column<string>(type: "TEXT", nullable: true),
                Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                Properties = table.Column<string>(type: "TEXT", nullable: true),
                Resources = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OpenIddictScopes", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Organizations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                ParentId = table.Column<Guid>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Organizations", x => x.Id);
                table.ForeignKey(
                    name: "FK_Organizations_Organizations_ParentId",
                    column: x => x.ParentId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "AspNetRoleClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                RoleId = table.Column<string>(type: "TEXT", nullable: false),
                ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                UserId = table.Column<string>(type: "TEXT", nullable: false),
                ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserLogins",
            columns: table => new
            {
                LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                UserId = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(
                    name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserRoles",
            columns: table => new
            {
                UserId = table.Column<string>(type: "TEXT", nullable: false),
                RoleId = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserTokens",
            columns: table => new
            {
                UserId = table.Column<string>(type: "TEXT", nullable: false),
                LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false),
                Value = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AuthorizationOperations",
            columns: table => new
            {
                ResourceKey = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Key = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuthorizationOperations", x => new { x.ResourceKey, x.Key });
                table.ForeignKey(
                    name: "FK_AuthorizationOperations_AuthorizationResources_ResourceKey",
                    column: x => x.ResourceKey,
                    principalTable: "AuthorizationResources",
                    principalColumn: "Key",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "OpenIddictAuthorizations",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", nullable: false),
                ApplicationId = table.Column<string>(type: "TEXT", nullable: true),
                ConcurrencyToken = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                CreationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                Properties = table.Column<string>(type: "TEXT", nullable: true),
                Scopes = table.Column<string>(type: "TEXT", nullable: true),
                Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                Subject = table.Column<string>(type: "TEXT", maxLength: 400, nullable: true),
                Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OpenIddictAuthorizations", x => x.Id);
                table.ForeignKey(
                    name: "FK_OpenIddictAuthorizations_OpenIddictApplications_ApplicationId",
                    column: x => x.ApplicationId,
                    principalTable: "OpenIddictApplications",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "OrganizationMemberships",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                OrganizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                UserId = table.Column<string>(type: "TEXT", nullable: false),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrganizationMemberships", x => x.Id);
                table.ForeignKey(
                    name: "FK_OrganizationMemberships_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_OrganizationMemberships_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "OrganizationRoles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                OrganizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrganizationRoles", x => x.Id);
                table.ForeignKey(
                    name: "FK_OrganizationRoles_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "OpenIddictTokens",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", nullable: false),
                ApplicationId = table.Column<string>(type: "TEXT", nullable: true),
                AuthorizationId = table.Column<string>(type: "TEXT", nullable: true),
                ConcurrencyToken = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                CreationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                ExpirationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                Payload = table.Column<string>(type: "TEXT", nullable: true),
                Properties = table.Column<string>(type: "TEXT", nullable: true),
                RedemptionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                ReferenceId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                Subject = table.Column<string>(type: "TEXT", maxLength: 400, nullable: true),
                Type = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OpenIddictTokens", x => x.Id);
                table.ForeignKey(
                    name: "FK_OpenIddictTokens_OpenIddictApplications_ApplicationId",
                    column: x => x.ApplicationId,
                    principalTable: "OpenIddictApplications",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_OpenIddictTokens_OpenIddictAuthorizations_AuthorizationId",
                    column: x => x.AuthorizationId,
                    principalTable: "OpenIddictAuthorizations",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "OrganizationMembershipRoles",
            columns: table => new
            {
                MembershipId = table.Column<Guid>(type: "TEXT", nullable: false),
                RoleId = table.Column<Guid>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrganizationMembershipRoles", x => new { x.MembershipId, x.RoleId });
                table.ForeignKey(
                    name: "FK_OrganizationMembershipRoles_OrganizationMemberships_MembershipId",
                    column: x => x.MembershipId,
                    principalTable: "OrganizationMemberships",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_OrganizationMembershipRoles_OrganizationRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "OrganizationRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ResourceOperationGrants",
            columns: table => new
            {
                RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                ResourceKey = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                OperationKey = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ResourceOperationGrants", x => new { x.RoleId, x.ResourceKey, x.OperationKey });
                table.ForeignKey(
                    name: "FK_ResourceOperationGrants_AuthorizationOperations_ResourceKey_OperationKey",
                    columns: x => new { x.ResourceKey, x.OperationKey },
                    principalTable: "AuthorizationOperations",
                    principalColumns: new[] { "ResourceKey", "Key" },
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ResourceOperationGrants_OrganizationRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "OrganizationRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AspNetRoleClaims_RoleId",
            table: "AspNetRoleClaims",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "AspNetRoles",
            column: "NormalizedName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserClaims_UserId",
            table: "AspNetUserClaims",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserLogins_UserId",
            table: "AspNetUserLogins",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserRoles_RoleId",
            table: "AspNetUserRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "AspNetUsers",
            column: "NormalizedEmail");

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            table: "AspNetUsers",
            column: "NormalizedUserName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_OpenIddictApplications_ClientId",
            table: "OpenIddictApplications",
            column: "ClientId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_OpenIddictAuthorizations_ApplicationId_Status_Subject_Type",
            table: "OpenIddictAuthorizations",
            columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_OpenIddictScopes_Name",
            table: "OpenIddictScopes",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_OpenIddictTokens_ApplicationId_Status_Subject_Type",
            table: "OpenIddictTokens",
            columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_OpenIddictTokens_AuthorizationId",
            table: "OpenIddictTokens",
            column: "AuthorizationId");

        migrationBuilder.CreateIndex(
            name: "IX_OpenIddictTokens_ReferenceId",
            table: "OpenIddictTokens",
            column: "ReferenceId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_OrganizationMembershipRoles_RoleId",
            table: "OrganizationMembershipRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "IX_OrganizationMemberships_OrganizationId_UserId",
            table: "OrganizationMemberships",
            columns: new[] { "OrganizationId", "UserId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_OrganizationMemberships_UserId",
            table: "OrganizationMemberships",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_OrganizationRoles_OrganizationId_Code",
            table: "OrganizationRoles",
            columns: new[] { "OrganizationId", "Code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Organizations_Code",
            table: "Organizations",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Organizations_ParentId",
            table: "Organizations",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_ResourceOperationGrants_ResourceKey_OperationKey",
            table: "ResourceOperationGrants",
            columns: new[] { "ResourceKey", "OperationKey" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AspNetRoleClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserLogins");

        migrationBuilder.DropTable(
            name: "AspNetUserRoles");

        migrationBuilder.DropTable(
            name: "AspNetUserTokens");

        migrationBuilder.DropTable(
            name: "OpenIddictScopes");

        migrationBuilder.DropTable(
            name: "OpenIddictTokens");

        migrationBuilder.DropTable(
            name: "OrganizationMembershipRoles");

        migrationBuilder.DropTable(
            name: "ResourceOperationGrants");

        migrationBuilder.DropTable(
            name: "AspNetRoles");

        migrationBuilder.DropTable(
            name: "OpenIddictAuthorizations");

        migrationBuilder.DropTable(
            name: "OrganizationMemberships");

        migrationBuilder.DropTable(
            name: "AuthorizationOperations");

        migrationBuilder.DropTable(
            name: "OrganizationRoles");

        migrationBuilder.DropTable(
            name: "OpenIddictApplications");

        migrationBuilder.DropTable(
            name: "AspNetUsers");

        migrationBuilder.DropTable(
            name: "AuthorizationResources");

        migrationBuilder.DropTable(
            name: "Organizations");
    }
}
