namespace Jero.Core.Service;

/// <summary>
/// 代码生成参数类
/// </summary>
public class CodeGenInput : BasePageInput
{
    /// <summary>
    /// 作者姓名
    /// </summary>
    public virtual string AuthorName { get; set; } = string.Empty;
    /// <summary>
    /// 类名
    /// </summary>
    public virtual string ClassName { get; set; } = string.Empty;
    /// <summary>
    /// 是否移除表前缀
    /// </summary>
    public virtual string TablePrefix { get; set; } = string.Empty;
    /// <summary>
    /// 库定位器名
    /// </summary>
    public virtual string ConfigId { get; set; } = string.Empty;
    /// <summary>
    /// 数据库名(保留字段)
    /// </summary>
    public virtual string DbName { get; set; } = string.Empty;
    /// <summary>
    /// 数据库类型
    /// </summary>
    public virtual string DbType { get; set; } = string.Empty;
    /// <summary>
    /// 数据库链接
    /// </summary>
    public virtual string ConnectionString { get; set; } = string.Empty;
    /// <summary>
    /// 生成方式
    /// </summary>
    public virtual string GenerateType { get; set; } = string.Empty;
    /// <summary>
    /// 数据库表名
    /// </summary>
    public virtual string TableName { get; set; } = string.Empty;
    /// <summary>
    /// 命名空间
    /// </summary>
    public virtual string NameSpace { get; set; } = string.Empty;
    /// <summary>
    /// 业务名（业务代码包名称）
    /// </summary>
    public virtual string BusName { get; set; } = string.Empty;
    /// <summary>
    /// 功能名（数据库表名称）
    /// </summary>
    public virtual string TableComment { get; set; } = string.Empty;
    /// <summary>
    /// 表唯一字段
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual string TableUniqueConfig { get; set; } = string.Empty;
    /// <summary>
    /// 表唯一字段列表
    /// </summary>
    public virtual List<TableUniqueConfigItem> TableUniqueList { get; set; } = [];
    /// <summary>
    /// 菜单应用分类（应用编码）
    /// </summary>
    public virtual string MenuApplication { get; set; } = string.Empty;
    /// <summary>
    /// 是否生成菜单
    /// </summary>
    public virtual bool GenerateMenu { get; set; }

    /// <summary>
    /// 菜单父级
    /// </summary>
    public virtual long? MenuPid { get; set; }

    /// <summary>
    /// 菜单图标
    /// </summary>
    public virtual string MenuIcon { get; set; } = string.Empty;
    /// <summary>
    /// 页面目录
    /// </summary>
    public virtual string PagePath { get; set; } = string.Empty;
    /// <summary>
    /// 支持打印类型
    /// </summary>
    public virtual string PrintType { get; set; } = string.Empty;
    /// <summary>
    /// 打印模版名称
    /// </summary>
    public virtual string PrintName { get; set; } = string.Empty;
}

public class AddCodeGenInput : CodeGenInput
{
    /// <summary>
    /// 数据库表名
    /// </summary>
    [Required(ErrorMessage = "数据库表名不能为空")]
    public override string TableName { get; set; } = string.Empty;
    /// <summary>
    /// 业务名（业务代码包名称）
    /// </summary>
    [Required(ErrorMessage = "业务名不能为空")]
    public override string BusName { get; set; } = string.Empty;
    /// <summary>
    /// 命名空间
    /// </summary>
    [Required(ErrorMessage = "命名空间不能为空")]
    public override string NameSpace { get; set; } = string.Empty;
    /// <summary>
    /// 作者姓名
    /// </summary>
    [Required(ErrorMessage = "作者姓名不能为空")]
    public override string AuthorName { get; set; } = string.Empty;
    ///// <summary>
    ///// 类名
    ///// </summary>
    //[Required(ErrorMessage = "类名不能为空")]
    //public override string ClassName { get; set; }

    ///// <summary>
    ///// 是否移除表前缀
    ///// </summary>
    //[Required(ErrorMessage = "是否移除表前缀不能为空")]
    //public override string TablePrefix { get; set; }

    /// <summary>
    /// 生成方式
    /// </summary>
    [Required(ErrorMessage = "生成方式不能为空")]
    public override string GenerateType { get; set; } = string.Empty;
    ///// <summary>
    ///// 功能名（数据库表名称）
    ///// </summary>
    //[Required(ErrorMessage = "数据库表名不能为空")]
    //public override string TableComment { get; set; }

    /// <summary>
    /// 是否生成菜单
    /// </summary>
    [Required(ErrorMessage = "是否生成菜单不能为空")]
    public override bool GenerateMenu { get; set; }
}

public class DeleteCodeGenInput
{
    /// <summary>
    /// 代码生成器Id
    /// </summary>
    [Required(ErrorMessage = "代码生成器Id不能为空")]
    public long Id { get; set; }
}

public class UpdateCodeGenInput : AddCodeGenInput
{
    /// <summary>
    /// 代码生成器Id
    /// </summary>
    [Required(ErrorMessage = "代码生成器Id不能为空")]
    public long Id { get; set; }
}

public class QueryCodeGenInput : DeleteCodeGenInput
{
}