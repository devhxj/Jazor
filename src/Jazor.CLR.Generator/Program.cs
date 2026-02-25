using Jazor.Name;
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
			summary = value
				.Replace("<summary>        ", "<summary>")
				.Replace("      </summary>", "</summary>");

		builder.AppendLine(value);
	}
	var result = builder.ToString().TrimEnd('\n').Trim();

	if (string.IsNullOrEmpty(result))
		return null;

	return $@"
**注释**：

```xml
{result}
```";
}

//var xmlDir = @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\10.0.2\ref\net10.0";
var coreLibXml = XmlDocumentationProvider.CreateFromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "System.Private.CoreLib.xml"));
var numericsXml = XmlDocumentationProvider.CreateFromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "System.Runtime.Numerics.xml"));
var compilation = CSharpCompilation.Create("Jazor", references: [
	MetadataReference.CreateFromFile(typeof(object).Assembly.Location, documentation: coreLibXml),
	MetadataReference.CreateFromFile(typeof(Console).Assembly.Location, documentation: coreLibXml),
	MetadataReference.CreateFromFile(typeof(Math).Assembly.Location, documentation: coreLibXml),
	MetadataReference.CreateFromFile(typeof(BigInteger).Assembly.Location, documentation: numericsXml),
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
	typeof(Console),
	typeof(Math),
	typeof(BigInteger),
	typeof(Object),
	typeof(Boolean),
	typeof(Char),
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
	typeof(Exception),
	typeof(StringBuilder),
	typeof(Nullable),
	typeof(ValueTuple),
	typeof(WeakReference),
	typeof(List<>),
	typeof(Dictionary<,>),
	typeof(HashSet<>),
	typeof(ReadOnlyCollection),
	typeof(ReadOnlyDictionary<,>),
	typeof(ReadOnlySet<>),
	typeof(ConditionalWeakTable<,>),
	typeof(GregorianCalendar),
	typeof(CultureInfo),
	typeof(Array)
};
var typeMaps = new Dictionary<Type, string>()
{
	//{typeof(void),"void"},
	{typeof(Object),"object"},
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
	{typeof(Decimal),"string"},
	{typeof(String),"string"},
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
	{typeof(CultureInfo),"string"},
	{typeof(Console),"object"},
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
	{"decimal","string"},
	{"object","object"},
	{"string","string"},

	{"System.DateTime","Date"},
	{"System.DateOnly","Date"},
	{"System.TimeOnly","Number"},
	{"System.DateTimeOffset","Date"},
	{"System.Console","object"},

	{"System.Span<char>","Uint32Array"},
	{"System.ReadOnlySpan<char>","Uint32Array"},
	{"System.Span<byte>","Uint8Array"},
	{"System.ReadOnlySpan<byte>","Uint8Array"},
	{"System.Collections.ObjectModel.ReadOnlyCollection<T>","Array<T>" },
	{"System.Collections.Generic.List<T>","Array<T>" },
	{"T[]","Array<T>"},
	{"System.Predicate<T>","Predicate<T>" },
	{"System.Collections.Generic.ICollection<T>","Array<T>" },
	{"System.Collections.Generic.IList","Array<object?>" },
	{"System.Collections.Generic.IList<T>","Array<T>" },
	{"System.Collections.Generic.IEnumerable<T>","Array<T>" },
	{"System.Collections.Generic.IComparer<T>","IComparer<T>" },
	{"System.Comparison<T>","Comparison<T>" },
	{"System.Collections.Generic.ISet<T>","Set<T>" },
	{"System.Globalization.Calendar","GregorianCalendar" },
	{"System.Globalization.GregorianCalendar","GregorianCalendar" },
	{"System.Collections.Generic.Dictionary<TKey, TValue>.KeyCollection","IArray<TKey>" },
	{"System.Collections.Generic.Dictionary<TKey, TValue>.ValueCollection","IArray<TValue>" },
	{"System.Collections.Generic.Dictionary<TKey, TValue>.AlternateLookup<TAlternateKey>","Dictionary<TKey, TValue>.AlternateLookup<TAlternateKey>" },
	{"System.Collections.Generic.IEnumerator<T>","Array<T>" },
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

	var r = name.Replace(key, newValue);
	if (symbol.IsParams)
		r = r.Replace("params", "");
	return r;
}

//var directory = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName, "generate");
var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "generate");
var doc = Path.Combine(dir, "doc");
var module = Path.Combine(dir, "module");

if (!Directory.Exists(dir))
	Directory.CreateDirectory(dir);

if (!Directory.Exists(doc))
	Directory.CreateDirectory(doc);

if (!Directory.Exists(module))
	Directory.CreateDirectory(module);

foreach (var type in types)
{
	var coder = new StringBuilder();
	var noter = new StringBuilder();
	var symbol = compilation.GetTypeByMetadataName(type.FullName!)!;
	var typeName = type.Name.Split('`')[0];
	var fullName = symbol.ToDisplayString(Format.NameFormat);

	var typeGenericNames = symbol
		.OriginalDefinition
		.TypeParameters
		.Select(x => x.Name)
		.ToArray()!;
	var typeGenerics = typeGenericNames?.Length > 0
		? $"<{string.Join(", ", typeGenericNames)}>"
		: string.Empty;

	if (!typeMaps.TryGetValue(type, out var mapName))
		mapName = fullName;

	coder.Append(
$@"namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, ""{fullName}"",""{type.FullName?.Split('`')[0].Replace('.', '/')}Module.js"")]
public static class {typeName}Module{(typeGenerics.Length > 0 ? typeGenerics : "")}
{{");

	noter
		.AppendLine($"# {typeName}Module.cs")
		.AppendLine()
		.AppendLine(@"> ⚠️ **注意**：签名= _+ SHA256Hash(成员)")
		.AppendLine();

	var keys = new Dictionary<string,string>();
	var members = symbol.GetMembers();
	foreach (var member in members)
	{
		if (member.DeclaredAccessibility.HasFlag(Accessibility.Public))
		{
			var display = member.ToDisplayString(Format.NameFormat);
			var hash = Format.HashName(display);
			var comment = GetComment(member,out var summary);
			var generics = string.Empty;
			var para = string.Empty;
			var wlop = "Op.Discard";
			string? value = null;
			var returnType = string.Empty;
			keys.Add(hash, display);

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

					if (method.Parameters.Any(x => x.RefKind == RefKind.Ref || x.RefKind == RefKind.Out))
						returnType = "Array<object?>";
				}

				var methodGenericNames = method.OriginalDefinition.TypeParameters
					.Select(x => x.Name)
					.Except(typeGenericNames!)
					.ToArray();
				generics = methodGenericNames.Length > 0
					? $"<{string.Join(", ", methodGenericNames)}>"
					: string.Empty;

				if (method.Name.StartsWith("op_", StringComparison.InvariantCulture))
				{
					if (operatorNames.TryGetValue(method.Name, out var @operator))
					{
						if (method.Parameters.Length == 1)
						{
							wlop = "Op.Allowed";
						}
						else if (method.Parameters.Length == 2)
						{
							wlop = "Op.Allowed";
						}
					}
				}
			}
			else continue;

			coder.Append(
$@"{(summary is not null?$"\r\n\t///{summary}":"")}
	[Jazor({wlop} ,""{display}""{(value is null ? string.Empty : $", \"{value}\"")})]
	public extern static {returnType} {hash}{generics}({para});
");

			noter.Append(
$@"**成员**：{display}</br>
**签名**：{hash}</br>{comment}

");
		}
	}

	coder.AppendLine("}");
	File.WriteAllText(Path.Combine(module, $"{typeName}Module.cs"), coder.ToString());
	File.WriteAllText(Path.Combine(doc, $"{typeName}Module.md"), noter.ToString());
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
	.Select(type => compilation!.GetTypeByMetadataName(type.FullName!)!.ToDisplayString(Format.NameFormat))
	.Select(type => $"\"{type}\"");

var whiteListTypeName = string.Join($", \n", whiteListTypeNames);

var name = compilation!.GetTypeByMetadataName(typeof(List<>).FullName!)!
	.Construct(compilation.GetSpecialType(SpecialType.System_Int32))
	.ToDisplayString(Format.NameFormat);

Console.WriteLine(whiteListTypeName);
Console.ReadLine();