using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

var outTypes = new Type[]{
	// 基本类型
	typeof(void),
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
	typeof(Int128),
	typeof(UInt128),
	typeof(Half),
	typeof(Single),
	typeof(Double),
	typeof(Decimal),
	typeof(DateTime),
	typeof(DateOnly),
	typeof(TimeOnly),
	typeof(DateTimeOffset),
	typeof(TimeSpan),
	typeof(Index),
	typeof(Range),
	typeof(String),
	typeof(Exception),
	typeof(StringBuilder),
	typeof(Nullable),
	typeof(ValueTuple),
	typeof(WeakReference),
	typeof(List<>),
	typeof(Dictionary<,>),
	typeof(KeyValuePair<,>),
	typeof(HashSet<>),
	typeof(ReadOnlyCollection),
	typeof(ReadOnlyDictionary<,>),
	typeof(ReadOnlySet<>),
	typeof(ConditionalWeakTable<,>),
	typeof(GregorianCalendar),
	typeof(CultureInfo),
	typeof(Queue<>),
	typeof(Stack<>),
	typeof(Array),
	typeof(Guid),
	typeof(Task)
};
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
	{typeof(Half),"Number"},
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
	{typeof(StringBuilder),"String"},
	{typeof(Nullable),"Object"},
	{typeof(ValueTuple),"Object"},
	{typeof(WeakReference),"WeakRef"},
	//{typeof(Action),""},
	//{typeof(Func<>),""},
	{typeof(List<>),"Array<T>"},
	{typeof(Dictionary<,>),"Map<TKey,TValue>"},
	{typeof(KeyValuePair<,>),"Array"},
	{typeof(HashSet<>),"Set<T>"},
	{typeof(ReadOnlyCollection),"Array<T>"},
	{typeof(ReadOnlyDictionary<,>),"Map<TKey,TValue>"},
	{typeof(ReadOnlySet<>),"Set<T>"},
	{typeof(ConditionalWeakTable<,>),"WeakMap<TKey,TValue>"},
	{typeof(Calendar),"Date"},
	{typeof(GregorianCalendar),"Date"},
	{typeof(CultureInfo),"String"},
	{typeof(Console),"Object"},
	{typeof(Guid),"String"},
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
	{"System.Half","Number"},
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
	{"bool","bool"},

	{"System.DateTime","Date"},
	{"System.DateOnly","Date"},
	{"System.TimeOnly","Number"},
	{"System.DateTimeOffset","Date"},
	{"System.Console","object"},

	{"System.Span<char>","string"},
	{"System.ReadOnlySpan<char>","string"},
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
	{"System.ValueTuple","Object" },
	{"System.Collections.Generic.Dictionary<TKey, TValue>.KeyCollection","IArray<TKey>" },
	{"System.Collections.Generic.Dictionary<TKey, TValue>.ValueCollection","IArray<TValue>" },
	{"System.Collections.Generic.Dictionary<TKey, TValue>.AlternateLookup<TAlternateKey>","Dictionary<TKey, TValue>.AlternateLookup<TAlternateKey>" },
	{"System.Collections.Generic.IEnumerator<T>","Array<T>" },
	{"System.Globalization.CultureInfo","String"}
};

//var xmlDir = @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\10.0.2\ref\net10.0";
var coreLibXml = XmlDocumentationProvider.CreateFromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "System.Private.CoreLib.xml"));
var numericsXml = XmlDocumentationProvider.CreateFromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "System.Runtime.Numerics.xml"));
var compilation = CSharpCompilation.Create("Jazor", references: [
	MetadataReference.CreateFromFile(typeof(object).Assembly.Location, documentation: coreLibXml),
	MetadataReference.CreateFromFile(typeof(Stack<>).Assembly.Location, documentation: coreLibXml),
	MetadataReference.CreateFromFile(typeof(Console).Assembly.Location, documentation: coreLibXml),
	MetadataReference.CreateFromFile(typeof(Math).Assembly.Location, documentation: coreLibXml),
	MetadataReference.CreateFromFile(typeof(BigInteger).Assembly.Location, documentation: numericsXml),
]);
string ConvertTypeName(ITypeSymbol symbol)
{
	var display = symbol.ToDisplayString();
	var nullableSuffix = display.EndsWith("?", StringComparison.Ordinal) ? "?" : string.Empty;
	var key = display.TrimEnd('?');
	if (nameMaps.TryGetValue(key, out var mapName))
		return $"{mapName}{nullableSuffix}";

	if (symbol is ITypeParameterSymbol typeParameter)
		return $"{EscapeIdentifier(typeParameter.Name)}{nullableSuffix}";

	if (symbol is IArrayTypeSymbol array)
		return $"Array<{ConvertTypeName(array.ElementType)}>{nullableSuffix}";

	if (symbol is INamedTypeSymbol { IsTupleType: true } tuple)
	{
		// C# tuple syntax only exists for arity >= 2. ValueTuple<T1> is a normal
		// generic struct and must stay that way in the generated scaffold.
		if (tuple.TupleElements.Length == 1)
			return tuple.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

		var elements = tuple.TupleElements.Select(element =>
			$"{ConvertTypeName(element.Type)} {EscapeIdentifier(element.Name)}");
		return $"({string.Join(", ", elements)}){nullableSuffix}";
	}

	if (symbol is INamedTypeSymbol nullable &&
		nullable.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
		return $"{ConvertTypeName(nullable.TypeArguments[0])}?";

	// 未知 host 类型仍保留其 C# 类型。Op.Discard 只表示尚未 lower，
	// 不能把生成骨架的强类型契约悄悄降为 object。
	var nonNullable = symbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
	return $"{nonNullable.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}{nullableSuffix}";
}

string FormatParameter(IParameterSymbol symbol)
	=> $"{ConvertTypeName(symbol.Type)} {EscapeIdentifier(symbol.Name)}";

string EscapeIdentifier(string name)
	=> SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ||
		SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None
		? $"@{name}"
		: name;

string FormatConstraints(IEnumerable<ITypeParameterSymbol> typeParameters, string indentation)
{
	var result = new StringBuilder();
	foreach (var typeParameter in typeParameters)
	{
		var constraints = new List<string>();
		if (typeParameter.HasUnmanagedTypeConstraint)
			constraints.Add("unmanaged");
		else if (typeParameter.HasValueTypeConstraint)
			constraints.Add("struct");
		else if (typeParameter.HasReferenceTypeConstraint)
			constraints.Add(typeParameter.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated
				? "class?"
				: "class");
		else if (typeParameter.HasNotNullConstraint)
			constraints.Add("notnull");

		constraints.AddRange(typeParameter.ConstraintTypes.Select(constraint =>
			constraint.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
		if (typeParameter.HasConstructorConstraint)
			constraints.Add("new()");
		if (typeParameter.AllowsRefLikeType)
			constraints.Add("allows ref struct");

		if (constraints.Count > 0)
		{
			result
				.AppendLine()
				.Append(indentation)
				.Append("where ")
				.Append(EscapeIdentifier(typeParameter.Name))
				.Append(" : ")
				.Append(string.Join(", ", constraints));
		}
	}

	return result.ToString();
}

if (args.Length > 1)
{
	Console.Error.WriteLine("Usage: Jazor.CLR.Generator [output-directory]");
	return 2;
}

var dir = args.Length == 1
	? Path.GetFullPath(args[0])
	: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "generate");
var doc = Path.Combine(dir, "doc");
var module = Path.Combine(dir, "module");

if (!Directory.Exists(dir))
	Directory.CreateDirectory(dir);

if (!Directory.Exists(doc))
	Directory.CreateDirectory(doc);

if (!Directory.Exists(module))
	Directory.CreateDirectory(module);

foreach (var type in outTypes)
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
	var typeConstraints = FormatConstraints(symbol.OriginalDefinition.TypeParameters, "\t");

	if (!typeMaps.TryGetValue(type, out var mapName))
		mapName = fullName;

	coder.Append(
$@"namespace Jazor.CLR;

[ECMAScriptModule(""{type.FullName?.Split('`')[0].Replace('.', '/')}Module.js"")]
[Jazor(Op.Alias, ""{fullName}"", ""{mapName}"")]
public static class {typeName}Module{(typeGenerics.Length > 0 ? typeGenerics : "")}{typeConstraints}
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
			var methodConstraints = string.Empty;
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
				// Pointer/function-pointer 没有可用的 ECMAScript carrier，骨架中直接略过。
				if (method.ReturnType.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer ||
					method.Parameters.Any(parameter => parameter.Type.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer) ||
					method.MethodKind == MethodKind.Destructor)
					continue;

				returnType = method.MethodKind == MethodKind.Constructor
					? mapName
					: ConvertTypeName(method.ReturnType);

				var parameters = method.Parameters.Select(FormatParameter).ToList();
				if (!method.IsStatic && method.MethodKind != MethodKind.Constructor)
					parameters.Insert(0, $"{mapName} instance");
				para = string.Join(", ", parameters);

				// ref/out 在 runtime 侧使用 [returnValue, out1, ...] 协议，不保留 CLR 地址语义。
				if (method.Parameters.Any(x => x.RefKind is RefKind.Ref or RefKind.Out))
					returnType = "Array<object?>";

				var methodGenericNames = method.OriginalDefinition.TypeParameters
					.Select(x => x.Name)
					.Except(typeGenericNames!)
					.ToArray();
				generics = methodGenericNames.Length > 0
					? $"<{string.Join(", ", methodGenericNames)}>"
					: string.Empty;
				methodConstraints = FormatConstraints(method.OriginalDefinition.TypeParameters, "\t\t");

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
	public extern static {returnType} {hash}{generics}({para}){methodConstraints};
");

			noter.Append(
$@"**成员**：{display}</br>
**签名**：{hash}</br>{comment}

");
		}
	}

	coder.AppendLine("}");
	File.WriteAllText(Path.Combine(module, $"{typeName}Module.cs"), coder.ToString());
	File.WriteAllText(Path.Combine(doc, $"{typeName}Module.md"), noter.ToString().TrimEnd() + Environment.NewLine);
	Console.WriteLine(typeName);
}

return 0;


static string? GetComment(ISymbol? symbol, out string? summary)
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
