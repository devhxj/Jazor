using Jazor.Common;
using Jazor.CLR.Generator;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
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
	typeof(Task),
	typeof(ValueTask),
	typeof(Uri),
	typeof(CancellationTokenSource),
	typeof(CancellationToken),
	typeof(CancellationTokenRegistration),
	// Blazor authoring contracts are scaffolded from the real ASP.NET Core symbols,
	// then carried into Jazor.CLR as erased browser-facing signatures. The CLR project
	// itself must not reference ASP.NET Core; only this generator needs the packages.
	typeof(NavigationManager),
	typeof(NavigationOptions),
	typeof(Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs),
	typeof(Microsoft.AspNetCore.Components.Routing.NotFoundEventArgs),
	typeof(Microsoft.AspNetCore.Components.Routing.LocationChangingContext),
	typeof(NavigationManagerExtensions),

	// RazorVue product hooks own final Vue lowering, but their accepted CLR symbols
	// still enter the compiler only through generated Jazor.CLR modules.
	typeof(ComponentBase),
	typeof(EventCallback),
	typeof(EventCallback<>),
	typeof(EventCallbackFactory),
	typeof(RenderFragment),
	typeof(RenderFragment<>),
	typeof(MarkupString),
	typeof(ParameterView),
	typeof(RenderTreeBuilder),
	typeof(WebRenderTreeBuilderExtensions),

	// DOM-origin event carriers and controlled element operations.
	typeof(ChangeEventArgs),
	typeof(ElementReference),
	typeof(Microsoft.AspNetCore.Components.ElementReferenceExtensions),
	typeof(MouseEventArgs),
	typeof(KeyboardEventArgs),
	typeof(FocusEventArgs),
	typeof(PointerEventArgs),
	typeof(WheelEventArgs),
	typeof(DragEventArgs),
	typeof(DataTransfer),
	typeof(DataTransferItem),
	typeof(ClipboardEventArgs),
	typeof(TouchEventArgs),
	typeof(TouchPoint),
	typeof(Microsoft.AspNetCore.Components.Web.ErrorEventArgs),
	typeof(ProgressEventArgs),
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
	{typeof(Task),"Promise"},
	{typeof(ValueTask),"Promise"},
	{typeof(Uri),"URL"},
	{typeof(CancellationTokenSource),"AbortController"},
	{typeof(CancellationToken),"AbortSignal"},
	// registration 只是"如何解除订阅"的载体，浏览器没有对等类型，脚手架回落到 Object；
	// 真实 carrier 由 src/Jazor.CLR 侧的 adapter 签名声明。
	{typeof(CancellationTokenRegistration),"Object"},
	{typeof(NavigationManager),"Object"},
	{typeof(NavigationOptions),"Object"},
	{typeof(Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs),"Object"},
	{typeof(Microsoft.AspNetCore.Components.Routing.NotFoundEventArgs),"Object"},
	{typeof(Microsoft.AspNetCore.Components.Routing.LocationChangingContext),"Object"},
	{typeof(NavigationManagerExtensions),"Object"},
	{typeof(ComponentBase),"Object"},
	{typeof(EventCallback),"Object"},
	{typeof(EventCallback<>),"Object"},
	{typeof(EventCallbackFactory),"Object"},
	{typeof(RenderFragment),"Object"},
	{typeof(RenderFragment<>),"Object"},
	{typeof(MarkupString),"Object"},
	{typeof(ParameterView),"Object"},
	{typeof(RenderTreeBuilder),"Object"},
	{typeof(WebRenderTreeBuilderExtensions),"Object"},
	{typeof(ChangeEventArgs),"JazorEvent"},
	{typeof(ElementReference),"HTMLElement"},
	{typeof(Microsoft.AspNetCore.Components.ElementReferenceExtensions),"Object"},
	{typeof(MouseEventArgs),"MouseEvent"},
	{typeof(KeyboardEventArgs),"KeyboardEvent"},
	{typeof(FocusEventArgs),"FocusEvent"},
	{typeof(PointerEventArgs),"PointerEvent"},
	{typeof(WheelEventArgs),"WheelEvent"},
	{typeof(DragEventArgs),"DragEvent"},
	{typeof(DataTransfer),"DataTransfer"},
	{typeof(DataTransferItem),"DataTransferItem"},
	{typeof(ClipboardEventArgs),"ClipboardEvent"},
	{typeof(TouchEventArgs),"TouchEvent"},
	{typeof(TouchPoint),"Touch"},
	{typeof(Microsoft.AspNetCore.Components.Web.ErrorEventArgs),"ErrorEvent"},
	{typeof(ProgressEventArgs),"ProgressEvent"},
};
var blazorRuntimeCarrierMaps = typeMaps
	.Where(static entry => entry.Key.Namespace?.StartsWith("Microsoft.AspNetCore.Components", StringComparison.Ordinal) == true)
	.ToDictionary(
		static entry => entry.Key.FullName!,
		static entry => entry.Value,
		StringComparer.Ordinal);
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
	// System.Uri lives in System.Private.Uri, not CoreLib, but its doc comments ship in the
	// CoreLib documentation set.
	MetadataReference.CreateFromFile(typeof(Uri).Assembly.Location, documentation: coreLibXml),
	MetadataReference.CreateFromFile(typeof(NavigationManager).Assembly.Location),
	MetadataReference.CreateFromFile(typeof(MouseEventArgs).Assembly.Location),
]);
string ConvertTypeName(ITypeSymbol symbol)
{
	var display = symbol.ToDisplayString();
	var nullableSuffix = display.EndsWith("?", StringComparison.Ordinal) ? "?" : string.Empty;
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

	// The scaffold is copied into Jazor.CLR, which deliberately has no ASP.NET Core
	// reference. Preserve a known browser carrier when one was explicitly configured;
	// unknown Blazor-only types still erase to Object in the adapter declaration.
	if (TryGetBlazorRuntimeCarrier(symbol, out var runtimeCarrier))
		return $"{runtimeCarrier}{nullableSuffix}";

	if (UsesBlazorExternalType(symbol))
		return $"Object{nullableSuffix}";

	var key = display.TrimEnd('?');
	if (nameMaps.TryGetValue(key, out var mapName))
		return $"{mapName}{nullableSuffix}";

	// 未知 host 类型仍保留其 C# 类型。Op.Discard 只表示尚未 lower，
	// 不能把生成骨架的强类型契约悄悄降为 object。
	var nonNullable = symbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
	return $"{nonNullable.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}{nullableSuffix}";
}

bool TryGetBlazorRuntimeCarrier(ITypeSymbol symbol, out string runtimeCarrier)
{
	if (symbol is INamedTypeSymbol named &&
		blazorRuntimeCarrierMaps.TryGetValue(GetMetadataName(named.OriginalDefinition), out runtimeCarrier!))
	{
		return true;
	}

	runtimeCarrier = null!;
	return false;
}

string GetMetadataName(INamedTypeSymbol symbol)
{
	var typeNames = new Stack<string>();
	for (var current = symbol; current is not null; current = current.ContainingType)
		typeNames.Push(current.MetadataName);

	var namespaceName = symbol.ContainingNamespace.IsGlobalNamespace
		? string.Empty
		: symbol.ContainingNamespace.ToDisplayString();
	return string.IsNullOrEmpty(namespaceName)
		? string.Join("+", typeNames)
		: $"{namespaceName}.{string.Join("+", typeNames)}";
}

bool UsesBlazorExternalType(ITypeSymbol symbol)
{
	if (symbol is INamedTypeSymbol named &&
		(named.ContainingNamespace?.ToDisplayString().StartsWith("Microsoft.AspNetCore.Components", StringComparison.Ordinal) == true ||
		 named.TypeArguments.Any(UsesBlazorExternalType)))
		return true;

	if (symbol is IArrayTypeSymbol array)
		return UsesBlazorExternalType(array.ElementType);

	return false;
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

		// Jazor.CLR intentionally has no ASP.NET Core reference. A Blazor-only
		// constraint is authoring metadata, not a runtime carrier requirement for
		// the erased adapter, so omit it instead of generating an uncompilable
		// reference such as `where T : Microsoft.AspNetCore.Components.IComponent`.
		constraints.AddRange(typeParameter.ConstraintTypes
			.Where(constraint => !UsesBlazorExternalType(constraint))
			.Select(constraint => constraint.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
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

bool IsConsumerAccessible(ISymbol member)
	=> member.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal;

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

var outputNames = outTypes
	.Select(type => (Type: type, ModuleName: ModuleOutputNaming.GetModuleName(type)))
	.ToArray();
var duplicateOutputNames = outputNames
	.GroupBy(static output => output.ModuleName, StringComparer.Ordinal)
	.Where(static group => group.Count() > 1)
	.ToArray();
if (duplicateOutputNames.Length > 0)
{
	var collisions = string.Join(
		Environment.NewLine,
		duplicateOutputNames.Select(group =>
			$"{group.Key}: {string.Join(", ", group.Select(entry => entry.Type.FullName))}"));
	throw new InvalidOperationException($"CLR generator module output names must be unique.{Environment.NewLine}{collisions}");
}

foreach (var type in outTypes)
{
	var coder = new StringBuilder();
	var noter = new StringBuilder();
	var symbol = compilation.GetTypeByMetadataName(type.FullName!)!;
	var moduleName = ModuleOutputNaming.GetModuleName(type);
	var modulePath = ModuleOutputNaming.GetModulePath(type);
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

[ECMAScriptModule(""{modulePath}"")]
[Jazor(Op.Alias, ""{fullName}"", ""{mapName}"")]
public static class {moduleName}{(typeGenerics.Length > 0 ? typeGenerics : "")}{typeConstraints}
{{");

	noter
		.AppendLine($"# {moduleName}.cs")
		.AppendLine()
		.AppendLine(@"> ⚠️ **注意**：签名= _+ SHA256Hash(成员)")
		.AppendLine();

	var keys = new Dictionary<string,string>();
	var members = symbol.GetMembers();
	foreach (var member in members)
	{
		if (IsConsumerAccessible(member))
		{
			var display = member is IMethodSymbol { IsExtensionMethod: true } extensionMethod
				? extensionMethod.OriginalDefinition.ToDisplayString(Format.StaticExtensionNameFormat)
				: member.ToDisplayString(Format.NameFormat);
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
	File.WriteAllText(Path.Combine(module, $"{moduleName}.cs"), coder.ToString());
	File.WriteAllText(Path.Combine(doc, $"{moduleName}.md"), noter.ToString().TrimEnd() + Environment.NewLine);
	Console.WriteLine(moduleName);
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
