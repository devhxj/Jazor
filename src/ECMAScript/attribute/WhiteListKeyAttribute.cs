using System;

namespace ECMAScript;

internal enum WhiteListOp
{
    NameReplace = 0,
	ObjectCreation2CallExpression = 1,
	FieldReference2CallExpression = 2,
	PropertyReference2CallExpression = 3,
	PropertyReference2Literal = 4,
	Invocation2CallExpression = 5,
}

[AttributeUsage(
	AttributeTargets.Class |
	AttributeTargets.Constructor |
	//AttributeTargets.Field |
	//AttributeTargets.Property |
	AttributeTargets.Method,
	AllowMultiple = true,
	Inherited = false)]
public sealed class WhiteListAttribute(string key) : Attribute
{
	public string Key { get; } = key;
}

//[AttributeUsage(
//    AttributeTargets.Class |
//    AttributeTargets.Constructor |
//    AttributeTargets.Field |
//    AttributeTargets.Property |
//    AttributeTargets.Method,
//    Inherited = false)]
//internal sealed class WhiteListAttribute(string key, 
//    WhiteListOp kind = WhiteListOp.NameReplace,
//    string? code = null) : Attribute
//{
//    public string Key { get; } = key;

//    public WhiteListOp Kind { get; } = kind;

//	public string? Code { get; } = code;
//}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal sealed class WhiteListRenameAttribute(string name, params string[] key) : Attribute
{
    public string Name { get; } = name;
    public string[] Key { get; } = key;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
internal sealed class WhiteListInlineAttribute(string code, WhiteListOp kind, params string[] key) : Attribute
{
    public string[] Key { get; } = key;
    public string Code { get; } = code;
    public WhiteListOp Kind { get; } = kind;
}
