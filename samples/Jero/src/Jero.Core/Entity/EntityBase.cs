namespace Jero.Core;

/// <summary>
/// 框架实体基类Id
/// </summary>
public abstract class EntityBaseId
{
    /// <summary>
    /// 雪花Id
    /// </summary>
    [SugarColumn(ColumnName = "Id", ColumnDescription = "主键Id", IsPrimaryKey = true, IsIdentity = false)]
    public virtual long Id { get; set; }
}

/// <summary>
/// 框架实体基类
/// </summary>
[SugarIndex("index_{table}_CT", nameof(CreateTime), OrderByType.Asc)]
public abstract class EntityBase : EntityBaseId
{
    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnDescription = "创建时间", IsNullable = true, IsOnlyIgnoreUpdate = true)]
    public virtual DateTime CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [SugarColumn(ColumnDescription = "更新时间")]
    public virtual DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 创建者Id
    /// </summary>
    [OwnerUser]
    [SugarColumn(ColumnDescription = "创建者Id", IsOnlyIgnoreUpdate = true)]
    public virtual long? CreateUserId { get; set; }

    ///// <summary>
    ///// 创建者
    ///// </summary>
    //[System.Text.Json.Serialization.JsonIgnore]
    //[Navigate(NavigateType.OneToOne, nameof(CreateUserId))]
    //public virtual SysUser CreateUser { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者姓名", Length = 64, IsOnlyIgnoreUpdate = true)]
    public virtual string? CreateUserName { get; set; }

    /// <summary>
    /// 修改者Id
    /// </summary>
    [SugarColumn(ColumnDescription = "修改者Id")]
    public virtual long? UpdateUserId { get; set; }

    ///// <summary>
    ///// 修改者
    ///// </summary>
    //[System.Text.Json.Serialization.JsonIgnore]
    //[Navigate(NavigateType.OneToOne, nameof(UpdateUserId))]
    //public virtual SysUser UpdateUser { get; set; }

    /// <summary>
    /// 修改者姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "修改者姓名", Length = 64)]
    public virtual string? UpdateUserName { get; set; }
}

/// <summary>
/// 框架实体基类（删除标志）
/// </summary>
[SugarIndex("index_{table}_D", nameof(IsDelete), OrderByType.Asc)]
[SugarIndex("index_{table}_DT", nameof(DeleteTime), OrderByType.Asc)]
public abstract class EntityBaseDel : EntityBase, IDeletedFilter
{
    /// <summary>
    /// 软删除
    /// </summary>
    [SugarColumn(ColumnDescription = "软删除")]
    public virtual bool IsDelete { get; set; } = false;

    /// <summary>
    /// 软删除时间
    /// </summary>
    [SugarColumn(ColumnDescription = "软删除时间")]
    public virtual DateTime? DeleteTime { get; set; }
}

/// <summary>
/// 机构实体基类（数据权限）
/// </summary>
public abstract class EntityBaseOrg : EntityBase, IOrgIdFilter
{
    /// <summary>
    /// 机构Id
    /// </summary>
    [SugarColumn(ColumnDescription = "机构Id", IsNullable = true)]
    public virtual long OrgId { get; set; }

    ///// <summary>
    ///// 创建者部门Id
    ///// </summary>
    //[SugarColumn(ColumnDescription = "创建者部门Id", IsOnlyIgnoreUpdate = true)]
    //public virtual long? CreateOrgId { get; set; }

    ///// <summary>
    ///// 创建者部门
    ///// </summary>
    //[System.Text.Json.Serialization.JsonIgnore]
    //[Navigate(NavigateType.OneToOne, nameof(CreateOrgId))]
    //public virtual SysOrg CreateOrg { get; set; }

    ///// <summary>
    ///// 创建者部门名称
    ///// </summary>
    //[SugarColumn(ColumnDescription = "创建者部门名称", Length = 64, IsOnlyIgnoreUpdate = true)]
    //public virtual string? CreateOrgName { get; set; }
}

/// <summary>
/// 机构实体基类（数据权限、删除标志）
/// </summary>
public abstract class EntityBaseOrgDel : EntityBaseDel, IOrgIdFilter
{
    /// <summary>
    /// 机构Id
    /// </summary>
    [SugarColumn(ColumnDescription = "机构Id", IsNullable = true)]
    public virtual long OrgId { get; set; }
}

/// <summary>
/// 租户实体基类
/// </summary>
public abstract class EntityBaseTenant : EntityBase, ITenantIdFilter
{
    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id", IsOnlyIgnoreUpdate = true)]
    public virtual long? TenantId { get; set; }
}

/// <summary>
/// 租户实体基类（删除标志）
/// </summary>
public abstract class EntityBaseTenantDel : EntityBaseDel, ITenantIdFilter
{
    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id", IsOnlyIgnoreUpdate = true)]
    public virtual long? TenantId { get; set; }
}

/// <summary>
/// 租户实体基类Id
/// </summary>
public abstract class EntityBaseTenantId : EntityBaseId, ITenantIdFilter
{
    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id", IsOnlyIgnoreUpdate = true)]
    public virtual long? TenantId { get; set; }
}

/// <summary>
/// 租户机构实体基类（数据权限）
/// </summary>
public abstract class EntityBaseTenantOrg : EntityBaseOrg, ITenantIdFilter
{
    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id", IsOnlyIgnoreUpdate = true)]
    public virtual long? TenantId { get; set; }
}

/// <summary>
/// 租户机构实体基类（数据权限、删除标志）
/// </summary>
public abstract class EntityBaseTenantOrgDel : EntityBaseOrgDel, ITenantIdFilter
{
    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id", IsOnlyIgnoreUpdate = true)]
    public virtual long? TenantId { get; set; }
}