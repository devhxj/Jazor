using ECMAScript.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

static string? GetComment(ISymbol? symbol,out string? summary)
{
	summary = null;
	var xml = symbol?.GetDocumentationCommentXml();
	if (string.IsNullOrEmpty(xml))
		return null;

	var member = XElement.Parse(xml);
	var builder = new StringBuilder();
	foreach (var node in member.Nodes())
	{
		var value = node.ToString().Replace(Environment.NewLine, "");
		if (value.StartsWith("<summary>"))
			summary = value;

		builder.AppendLine(value);
	}
	return builder.ToString().TrimEnd('\n');
}

//var xmlDir = @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\10.0.2\ref\net10.0";
var coreLibXml = XmlDocumentationProvider.CreateFromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "System.Private.CoreLib.xml"));
var numericsXml = XmlDocumentationProvider.CreateFromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "System.Runtime.Numerics.xml"));
var compilation = CSharpCompilation.Create("Jazor", references: [
	MetadataReference.CreateFromFile(typeof(object).Assembly.Location, documentation:coreLibXml),
	MetadataReference.CreateFromFile(typeof(BigInteger).Assembly.Location,documentation:numericsXml),
]);


var operatorNames = new Dictionary<string, string>
{
	{ "op_Addition", "+" },
	{ "op_Subtraction", "-" },
	{ "op_Multiply", "*" },
	{ "op_Division", "/" },
	{ "op_Modulus", "%" },
	{ "op_BitwiseAnd", "&" },
	{ "op_BitwiseOr", "|" },
	{ "op_ExclusiveOr", "^" },
	{ "op_LogicalNot", "!" },
	{ "op_OnesComplement", "~" },
	{ "op_LeftShift", "<<" },
	{ "op_RightShift", ">>" },
	{ "op_Equality", "==" },
	{ "op_Inequality", "!=" },
	{ "op_LessThan", "<" },
	{ "op_LessThanOrEqual", "<=" },
	{ "op_GreaterThan", ">" },
	{ "op_GreaterThanOrEqual", ">=" },
	{ "op_Increment", "++" },
	{ "op_Decrement", "--" },
	{ "op_UnaryPlus", "+" },
	{ "op_UnaryNegation", "-" },
	{ "op_True", "true" },
	{ "op_False", "false" }
};

var types = new Type[]{
	// 基本类型
	//typeof(void),
	typeof(BigInteger),
	//typeof(Object),
	//typeof(Boolean),
	//typeof(Char),
	//typeof(SByte),
	//typeof(Byte),
	//typeof(Int16),
	//typeof(UInt16),
	//typeof(Int32),
	//typeof(UInt32),
	//typeof(Int64),
	//typeof(UInt64),
	//typeof(Single),
	//typeof(Double),
	//typeof(Decimal),
	//typeof(DateTime),
	//typeof(DateOnly),
	//typeof(TimeOnly),
	//typeof(DateTimeOffset),
	//typeof(TimeSpan),
	typeof(String),
	//typeof(Exception),
	//typeof(StringBuilder),
	//typeof(Nullable),
	//typeof(ValueTuple),
	//typeof(WeakReference),
	//typeof(List<>),
	//typeof(Dictionary<,>),
	//typeof(HashSet<>),
	//typeof(ReadOnlyCollection),
	//typeof(ReadOnlyDictionary<,>),
	//typeof(ReadOnlySet<>),
	//typeof(ConditionalWeakTable<,>),
	//typeof(GregorianCalendar),
	//typeof(CultureInfo)
};
var typeMaps = new Dictionary<Type, string>()
{
	//{typeof(void),"void"},
	{typeof(Object),"Object"},
	{typeof(Boolean),"Boolean"},
	{typeof(Char),"Number"},
	{typeof(SByte),"Number"},
	{typeof(Byte),"Number"},
	{typeof(Int16),"Number"},
	{typeof(UInt16),"Number"},
	{typeof(Int32),"Number"},
	{typeof(UInt32),"Number"},
	{typeof(Single),"Number"},
	{typeof(Double),"Number"},
	{typeof(TimeOnly),"Number"},
	{typeof(DateOnly),"Date"},
	{typeof(DateTime),"Date"},
	{typeof(DateTimeOffset),"Date"},
	{typeof(Int64),"BigInt"},
	{typeof(UInt64),"BigInt"},
	{typeof(Int128),"BigInt"},
	{typeof(UInt128),"BigInt"},
	{typeof(TimeSpan),"BigInt"},
	{typeof(BigInteger),"BigInt"},
	{typeof(Decimal),"String"},
	{typeof(String),"String"},
	{typeof(Exception),"Error"},
	//{typeof(StringBuilder),""},
	//{typeof(Nullable),"null"},
	//{typeof(ValueTuple),""},
	{typeof(WeakReference),"WeakRef"},
	//{typeof(Action),""},
	//{typeof(Func<>),""},
	{typeof(List<>),"Array<T>"},
	{typeof(Dictionary<,>),"Map<TKey,TValue>"},
	{typeof(HashSet<>),"Set<T>"},
	{typeof(ReadOnlyCollection),"Array<T>"},
	{typeof(ReadOnlyDictionary<,>),"Map<TKey,TValue>"},
	{typeof(ReadOnlySet<>),"Set<T>"},
	{typeof(ConditionalWeakTable<,>),"WeakMap<TKey,TValue>"},
	{typeof(Calendar),"GregorianCalendar"},
	{typeof(CultureInfo),"String"},
};
var nameMaps = new Dictionary<string, string>()
{
	{"System.IFormatProvider","Intl.NumberFormat"},
	{"long","BigInt"},
	{"ulong","BigInt"},
	{"System.Int128","BigInt"},
	{"System.UInt128","BigInt"},
	{"System.TimeSpan","BigInt"},
	{"System.Numerics.BigInteger","BigInt"},

	{"System.Byte","Number"},
	{"System.SByte","Number"},
	{"System.Int16","Number"},
	{"System.UInt16","Number"},
	{"System.Int32","Number"},
	{"System.UInt32","Number"},
	{"System.Char","Number"},
	{"System.Single","Number"},
	{"System.Double","Number"},
	{"byte","Number"},
	{"sbyte","Number"},
	{"short","Number"},
	{"ushort","Number"},
	{"char","Number"},
	{"int","Number"},
	{"uint","Number"},
	{"float","Number"},
	{"double","Number"},
	{"decimal","String"},
	{"object","Object"},

	{"System.DateTime","Date"},
	{"System.DateOnly","Date"},
	{"System.TimeOnly","Number"},
	{"System.DateTimeOffset","Date"},

	{"System.Span<char>","Uint32Array"},
	{"System.ReadOnlySpan<char>","Uint32Array"},
	{"System.Span<byte>","Uint8Array"},
	{"System.ReadOnlySpan<byte>","Uint8Array"},
	{"System.Collections.ObjectModel.ReadOnlyCollection<T>","Array<T>" },
	{"T[]","Array<T>"},
	{"System.Predicate<T>","Predicate<T>" },
	{"System.Collections.Generic.IEnumerable<T>","IEnumerable<T>" },
	{"System.Collections.Generic.IComparer<T>","IComparer<T>" },
	{"System.Comparison<T>","Comparison<T>" },
	{"System.Collections.Generic.ISet<T>","ISet<T>" },
	{"System.Globalization.Calendar","GregorianCalendar" },
	{"System.Globalization.GregorianCalendar","GregorianCalendar" },
	{"System.Collections.Generic.Dictionary<TKey, TValue>.KeyCollection","IArray<TKey>" },
	{"System.Collections.Generic.Dictionary<TKey, TValue>.ValueCollection","IArray<TValue>" },
	{"System.Collections.Generic.Dictionary<TKey, TValue>.AlternateLookup<TAlternateKey>","Dictionary<TKey, TValue>.AlternateLookup<TAlternateKey>" },
	{"System.Collections.Generic.IEnumerator<T>","IEnumerator<T>" },
	{"System.Globalization.CultureInfo","String"}
};

string ConvertParamaterName(IParameterSymbol symbol)
{
	var name = symbol.ToDisplayString();
	var key = symbol.Type.ToDisplayString();
	var newValue = key;
	if (nameMaps.TryGetValue(key.TrimEnd('?'), out var mapName))
		newValue = $"{mapName}{(key.EndsWith('?') ? "?" : "")}";
	else
		newValue = "object";

	if (symbol.RefKind == RefKind.Ref)
	{
		key = $"ref {key}";
		newValue = $"RefValue<{newValue}>";
	}
	else if (symbol.RefKind == RefKind.Out)
	{
		key = $"out {key}";
		newValue = $"OutValue<{newValue}>";
	}


	var r = name.Replace(key, newValue);
	if (symbol.IsParams)
		r = r.Replace("params", "");
	return r;
}

//var directory = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName, "generate");
var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "generate");

if (!Directory.Exists(directory))
	Directory.CreateDirectory(directory);

foreach (var type in types)
{
	var coder = new StringBuilder();
	var texter = new StringBuilder();
	var symbol = compilation.GetTypeByMetadataName(type.FullName!)!;
	var typeName = type.Name.Split('`')[0];
	var fullName = symbol.ToDisplayString(Util.NameFormat);

	if (!typeMaps.TryGetValue(type, out var mapName))
		mapName = fullName;
	
	coder.Append(
$@"using System.Collections;
using ECMAScript.Common;
using static ECMAScript.CLRModule;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList(""{fullName}"",""{fullName}"",WhiteListOp.Allowed)]
public static class {typeName}Module
{{");

	texter.AppendLine(@"签名= _+ SHA256Hash(成员)").AppendLine();

	var keys = new Dictionary<string,string>();
	var members = symbol.GetMembers();
	foreach (var member in members)
	{
		if (member.DeclaredAccessibility.HasFlag(Accessibility.Public))
		{
			var display = member.ToDisplayString(Util.NameFormat);
			var key = Util.HashName(display);
			var comment = GetComment(member,out var summary);
			var generics = string.Empty;
			var para = string.Empty;
			var wlop = "WhiteListOp.Discard";
			string? value = null;
			var returnType = string.Empty;
			keys.Add(key, display);

			if (member is IFieldSymbol field)
			{
				if (field.IsConst)
				{
					coder.AppendLine($@"{Environment.NewLine}	//{field.ToDisplayString()} = {field.ConstantValue};");
					continue;
				}
				else
				{
					returnType = mapName;
					para = field.IsStatic ? string.Empty : $"{mapName} instance";
				}
			}
			else if (member is IMethodSymbol method)
			{
				returnType = method.ReturnType.ToDisplayString();
				if (nameMaps.TryGetValue(returnType.TrimEnd('?'), out var nullMapName))
					returnType = $"{nullMapName}{(returnType.EndsWith('?') ? "?" : "")}";

				// 不支持指针
				if (method.Parameters.Any(x => x.Type?.TypeKind == TypeKind.Pointer))
					continue;

				if (method.MethodKind == MethodKind.PropertyGet || method.MethodKind == MethodKind.PropertySet)
				{
					para = method.Parameters.Length > 0
						? $"{mapName} instance, {string.Join(", ", method.Parameters.Select(ConvertParamaterName))}"
						: $"{mapName} instance";
				}
				else if (method.MethodKind != MethodKind.Destructor && method.MethodKind != MethodKind.Conversion)
				{
					if (method.MethodKind == MethodKind.Constructor)
						returnType = mapName;

					para = method.IsStatic || method.MethodKind == MethodKind.Constructor
						? string.Join(", ", method.Parameters.Select(ConvertParamaterName))
						: $"{mapName} instance{(method.Parameters.Length > 0 ? ", " : "")}{string.Join(", ", method.Parameters.Select(ConvertParamaterName))}";

					generics = (method.IsGenericMethod || method.ContainingType.TypeParameters.Length > 0)
						? $"<{string.Join(", ", method.ContainingType.TypeParameters
							.Concat(method.TypeParameters)
							.Select(x => x.Name))}>"
						: string.Empty;
				}
				if (method.Name.StartsWith("op_", StringComparison.InvariantCulture))
				{
					if (operatorNames.TryGetValue(method.Name, out var @operator))
					{
						if (method.Parameters.Length == 1)
						{
							wlop = "WhiteListOp.Literal";
							value = $", \"{@operator}{{0}}\"";
						}
						else if (method.Parameters.Length == 2)
						{
							wlop = "WhiteListOp.Literal";
							value = $", \"{{0}} {@operator} {{1}}\"";
						}
					}
				}
			}
			else continue;

			coder.Append(
$@"{(summary is not null?$"\r\n\t///{summary}":"")}
	[WhiteList(""{key}"", ""{display}"", {wlop}{value})]
	public extern static {returnType} {key}{generics}({para});
");

			texter.Append(
$@"签名: {key}
成员:{display}
注释：
{comment}
");
		}
	}

	coder.AppendLine("}");
	File.WriteAllText(Path.Combine(directory, $"{typeName}Module.cs"), coder.ToString());
	File.WriteAllText(Path.Combine(directory, $"{typeName}Module.note.txt"), texter.ToString());
	Console.WriteLine(typeName);
}

Type[] whiteListTypes = [
		// 基本类型
		typeof(void),
		typeof(Object),
		typeof(Boolean),
		typeof(Char),
		typeof(IntPtr),
		typeof(UIntPtr),
		typeof(SByte),
		typeof(Byte),
		typeof(Int16),
		typeof(UInt16),
		typeof(Int32),
		typeof(UInt32),
		typeof(Int64),
		typeof(UInt64),
		typeof(Single),
		typeof(Double),
		typeof(Decimal),
		typeof(DateTime),
		typeof(DateOnly),
		typeof(TimeOnly),
		typeof(DateTimeOffset),
		typeof(TimeSpan),
		typeof(String),
		typeof(BigInteger),
		typeof(Exception),
		typeof(StringBuilder),
		// 泛型或其他类型
		typeof(Nullable),
		typeof(ValueTuple),
		typeof(WeakReference),
		typeof(Action),
		typeof(Func<>),
		typeof(List<>),
		typeof(Dictionary<,>),
		typeof(HashSet<>),
		typeof(ReadOnlyCollection),
		typeof(ReadOnlyDictionary<,>),
		typeof(ReadOnlySet<>),
		typeof(ConditionalWeakTable<,>)
	];
var whiteListTypeNames = whiteListTypes
	.Select(type => compilation!.GetTypeByMetadataName(type.FullName!)!.ToDisplayString(Util.NameFormat))
	.Select(type => $"\"{type}\"");

var whiteListTypeName = string.Join($", \n", whiteListTypeNames);

var name = compilation!.GetTypeByMetadataName(typeof(List<>).FullName!)!
	.Construct(compilation.GetSpecialType(SpecialType.System_Int32))
	.ToDisplayString(Util.NameFormat);

Console.WriteLine(whiteListTypeName);
Console.ReadLine();