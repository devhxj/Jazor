using System;
using System.ComponentModel.DataAnnotations;

namespace ECMAScript;

/// <summary>
/// 类型模式
/// </summary>
public enum TypeScheme
{
	/// <summary>
	/// 不变
	/// </summary>
	Unchanged,
	/// <summary>
	/// 设为可空
	/// </summary>
	Partial,
	/// <summary>
	/// 设置为必需
	/// </summary>
	Required
}

/// <summary>
/// 指定在某些操作或处理期间要省略的类型属性。
/// </summary>
/// <typeparam name="TEntity">指定要省略属性的实体类型。</typeparam>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true)]
public class OmitAttribute<TEntity> : Attribute
{
	private readonly TypeScheme _scheme;
	private readonly string[] _properties;

	public OmitAttribute(params string[] prop) => _properties = prop;

	public OmitAttribute(TypeScheme scheme, params string[] prop) => (_scheme, _properties) = (scheme, prop);
}

/// <summary>
/// 指定在特定上下文中使用类型时要包含的属性子集。
/// </summary>
/// <typeparam name="TEntity">指定要选择属性的实体类型。</typeparam>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true)]
public class PickAttribute<TEntity> : Attribute
{
	private readonly TypeScheme _scheme;
	private readonly string[] _properties;

	public PickAttribute(params string[] prop) => _properties = prop;

	public PickAttribute(TypeScheme scheme, params string[] prop) => (_scheme, _properties) = (scheme, prop);
}

/// <summary>
/// 为类型注入新的属性。
/// </summary>
/// <typeparam name="TType">包含的属性的类型。</typeparam>
/// <param name="name">包含的属性的的名称。该值不能为空或空字符串。</param>
/// <param name="attributes">可选的属性字典，为包含的属性提供额外的元数据。如果未提供，则默认为 <see langword="null"/>。</param>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true)]
public class WithAttribute<TType>(string name, params string[] attributes) : Attribute
{
	private readonly string Name = name;

	private readonly object? Attributes = attributes;
}
