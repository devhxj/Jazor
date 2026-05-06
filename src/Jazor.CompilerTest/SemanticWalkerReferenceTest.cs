using Acornima.Ast;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections;
using System.Reflection;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class SemanticWalkerReferenceTest
{
	/// <summary>
	/// 编译代码并获取roslyn代码块
	/// </summary>
	/// <param name="code"></param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	private static IBlockOperation GetBlockOperation(string code, string assemblyName = "TestAssembly")
	{
		var usings = @"
        global using System;
        global using System.Collections.Generic;
        global using System.Linq;
        global using System.Numerics;
		global using ECMAScript;
		global using static ECMAScript.Global;";

		var references = Basic.Reference.Assemblies.Net100.References.All
			.Add(MetadataReference.CreateFromFile(typeof(Global).Assembly.Location));
		var compilation = CSharpCompilation.Create(
			assemblyName: assemblyName,
			syntaxTrees: [
			  CSharpSyntaxTree.ParseText(usings),
			  CSharpSyntaxTree.ParseText(code)
			],
			references: references,
			options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		// 输出编译诊断信息
		var diagnostics = compilation.GetDiagnostics();
		var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		if (errors.Count > 0)
		{
			var errorMessages = string.Join("\n", errors.Select(e => $"{e.Id}: {e.GetMessage()}"));
			throw new InvalidOperationException(errorMessages);
		}

		var syntaxTree = compilation.SyntaxTrees.Last();
		var semanticModel = compilation.GetSemanticModel(syntaxTree);
		var root = syntaxTree.GetRoot();

		// 优先定位约定的 TestMethod，并跳过接口/抽象等无方法体声明，避免误取辅助成员。
		var methodDeclaration = root.DescendantNodes()
			.OfType<MethodDeclarationSyntax>()
			.FirstOrDefault(static method => method.Identifier.ValueText == "TestMethod" && method.Body is not null)
			?? root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault(static method => method.Body is not null);
		if (methodDeclaration?.Body is not null)
		{
			var operation = semanticModel.GetOperation(methodDeclaration.Body) as IBlockOperation;
			if (operation is not null)
				return operation;
		}

		throw new InvalidOperationException("未找到可分析的操作");
	}

	// 锁定完整 JS 输出，避免弱断言漏掉引号、换行和调用形态回退。
	private static void AssertScriptEqual(string expected, string? actual)
		=> Assert.AreEqual(expected.ReplaceLineEndings("\n"), actual?.ReplaceLineEndings("\n"));

	private static void AssertJsNamingScriptEqual(string expected, string? actual)
		=> Assert.AreEqual(ExpectedJsNaming.Normalize(expected).ReplaceLineEndings("\n"), actual?.ReplaceLineEndings("\n"));

	private static TOperation GetFirstOperation<TOperation>(string code)
		where TOperation : class, IOperation
	{
		var block = GetBlockOperation(code);
		var operation = EnumerateOperations(block).OfType<TOperation>().FirstOrDefault();
		if (operation is null)
			throw new InvalidOperationException($"未找到可分析的操作 {typeof(TOperation).Name}");

		return operation;
	}

	private static IEnumerable<IOperation> EnumerateOperations(IOperation root)
	{
		yield return root;
		foreach (var child in root.ChildOperations)
		{
			foreach (var nested in EnumerateOperations(child))
				yield return nested;
		}
	}
	
	#region VisitLocalReference - 局部变量引用

	/// <summary>
	/// 测试 VisitLocalReference - 简单局部变量引用
	/// C# 示例：int x = 5; Console.WriteLine(x);
	/// 转换结果：let x = 5; Console.WriteLine(x);
	/// </summary>
	[TestMethod]
	public void Visit_LocalReference_Simple()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int x = 5;
                    int y = x;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = 5;
  let y = x;
}", script);
	}

	/// <summary>
	/// 测试 VisitLocalReference - 多个局部变量引用
	/// </summary>
	[TestMethod]
	public void Visit_LocalReference_Multiple()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 1;
                    int b = 2;
                    int c = a + b;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let a = 1;
  let b = 2;
  let c = a + b;
}", script);
	}

	#endregion

	#region VisitParameterReference - 参数引用

	[TestMethod]
	public void Visit_Reference_DescriptionBasedNamespaceAliasType_TranslatesToWebIdlNames()
	{
		var block = GetBlockOperation(@"
            using System;
            using System.ComponentModel;
            using console = ECMAScript.Console.Console;

            class TestClass
            {
                void TestMethod()
                {
                    console.Log(""hello"");
                }
            }

            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : Attribute
                {
                }
            }

            namespace ECMAScript.Console
            {
                [Description(""@#console"")]
                [ECMAScript.ECMAScript]
                public static class Console
                {
                    [Description(""@#log"")]
                    public static void Log(string value) { }
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  console.log(""hello"");
}".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
	}

	[TestMethod]
	public void Visit_Reference_ActualEcmascriptBindings_EmitRealisticBrowserShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var element = Global.Document.CreateElement(""button"", new ElementCreationOptions(Is: ""x-button""));
                    element.AddEventListener(""click"", null, new AddEventListenerOptions(Once: true));
                    var evt = new Event(""click"", new EventInit(Bubbles: true, Cancelable: true));
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let element = document.createElement(""button"", { is: ""x-button"" });
  element.addEventListener(""click"", null, { once: true });
  let evt = new Event(""click"", { bubbles: true, cancelable: true });
}".ReplaceLineEndings("\n"), script?.ReplaceLineEndings("\n"));
	}

	[TestMethod]
	public void Visit_Reference_StringBuilderAppendLine_UsesInlineSequence()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(string text)
                {
                    var sb = new System.Text.StringBuilder();
                    var appended = sb.AppendLine(text);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let sb = [];
  let appended = (sb.push(...(text ?? '').split('')), sb.push('\n'), sb);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_StringBuilderAppendLineWithoutValue_UsesInlineSequence()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var sb = new System.Text.StringBuilder();
                    var appended = sb.AppendLine();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let sb = [];
  let appended = (sb.push('\n'), sb);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_NumericInlineTemplates_DoNotFallBackToImportHelpers()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(float left, float right, double value, double sign)
                {
                    var cmp = left.CompareTo(right);
                    var eq = left.Equals(right);
                    var fcopy = float.CopySign(left, right);
                    var dcmp = value.CompareTo(sign);
                    var deq = value.Equals(sign);
                    var copy = double.CopySign(value, sign);
                    var mathCopy = System.Math.CopySign(value, sign);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

        AssertScriptEqual(@"{
  let cmp = isNaN(left) ? isNaN(right) ? 0 : -1 : isNaN(right) ? 1 : left < right ? -1 : left > right ? 1 : 0;
  let eq = isNaN(left) ? isNaN(right) : isNaN(right) ? false : !(left < right) && !(left > right);
  let fcopy = right < 0 || Object.is(right, -0) ? -Math.abs(left) : Math.abs(left);
  let dcmp = isNaN(value) ? isNaN(sign) ? 0 : -1 : isNaN(sign) ? 1 : value < sign ? -1 : value > sign ? 1 : 0;
  let deq = isNaN(value) ? isNaN(sign) : isNaN(sign) ? false : !(value < sign) && !(value > sign);
  let copy = sign < 0 || Object.is(sign, -0) ? -Math.abs(value) : Math.abs(value);
  let mathCopy = sign < 0 || Object.is(sign, -0) ? -Math.abs(value) : Math.abs(value);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_SingleMathIntrinsics_UseDirectMathShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(float value, float left, float right, float third)
                {
                    var log2 = float.Log2(value);
                    var expM1 = float.ExpM1(value);
                    var ceil = float.Ceiling(value);
                    var floor = float.Floor(value);
                    var round = float.Round(value);
                    var trunc = float.Truncate(value);
                    var atan2Pi = float.Atan2Pi(left, right);
                    var fused = float.FusedMultiplyAdd(left, right, third);
                    var ieee = float.Ieee754Remainder(left, right);
                    var lerp = float.Lerp(left, right, third);
                    var reciprocal = float.ReciprocalEstimate(value);
                    var acosh = float.Acosh(value);
                    var logBase = float.Log(left, right);
                    var clamp = float.Clamp(value, left, right);
                    var max = float.Max(left, right);
                    var abs = float.Abs(value);
                    var even = float.IsEvenInteger(value);
                    var integer = float.IsInteger(value);
                    var positive = float.IsPositive(value);
                    var real = float.IsRealNumber(value);
                    var pow = float.Pow(left, right);
                    var sqrt = float.Sqrt(value);
                    var acosPi = float.AcosPi(value);
                    var cosPi = float.CosPi(value);
                    var deg = float.DegreesToRadians(value);
                    var sin = float.Sin(value);
                    var tanPi = float.TanPi(value);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let log2 = Math.log2(value);
  let expM1 = Math.exp(value) - 1;
  let ceil = Math.ceil(value);
  let floor = Math.floor(value);
  let round = Math.round(value);
  let trunc = Math.trunc(value);
  let atan2Pi = Math.atan2(left, right) / Math.PI;
  let fused = left * right + third;
  let ieee = left - right * Math.round(left / right);
  let lerp = left + (right - left) * third;
  let reciprocal = 1 / value;
  let acosh = Math.acosh(value);
  let logBase = Math.log(left) / Math.log(right);
  let clamp = Math.max(left, Math.min(value, right));
  let max = Math.max(left, right);
  let abs = Math.abs(value);
  let even = value % 2 === 0;
  let integer = Number.isInteger(value);
  let positive = value > 0 || Object.is(value, 0);
  let real = !isNaN(value) && value !== Infinity && value !== -Infinity;
  let pow = Math.pow(left, right);
  let sqrt = Math.sqrt(value);
  let acosPi = Math.acos(value) / Math.PI;
  let cosPi = Math.cos(value * Math.PI);
  let deg = value * Math.PI / 180;
  let sin = Math.sin(value);
  let tanPi = Math.tan(value * Math.PI);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_DoubleMathIntrinsics_UseMathHostInsteadOfNumberHost()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(double value, double left, double right)
                {
                    var log2 = double.Log2(value);
                    var exp = double.Exp(value);
                    var max = double.Max(left, right);
                    var abs = double.Abs(value);
                    var pow = double.Pow(left, right);
                    var sqrt = double.Sqrt(value);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let log2 = Math.log2(value);
  let exp = Math.exp(value);
  let max = Math.max(left, right);
  let abs = Math.abs(value);
  let pow = Math.pow(left, right);
  let sqrt = Math.sqrt(value);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_FloatingPointMaxMinNumber_UseInlineNaNFallback()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(float fleft, float fright, double dleft, double dright)
                {
                    var fmax = float.MaxNumber(fleft, fright);
                    var fmin = float.MinNumber(fleft, fright);
                    var dmax = double.MaxNumber(dleft, dright);
                    var dmin = double.MinNumber(dleft, dright);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let fmax = isNaN(fleft) ? fright : isNaN(fright) ? fleft : Math.max(fleft, fright);
  let fmin = isNaN(fleft) ? fright : isNaN(fright) ? fleft : Math.min(fleft, fright);
  let dmax = isNaN(dleft) ? dright : isNaN(dright) ? dleft : Math.max(dleft, dright);
  let dmin = isNaN(dleft) ? dright : isNaN(dright) ? dleft : Math.min(dleft, dright);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_FloatingPointSignAndPow2_UseRuntimeHelpers()
	{
		var block = GetBlockOperation(@"
            using System;

            class TestClass
            {
                void TestMethod(float fvalue, double dvalue)
                {
                    var fsign = float.Sign(fvalue);
                    var fpow2 = float.IsPow2(fvalue);
                    var dsign = double.Sign(dvalue);
                    var dpow2 = double.IsPow2(dvalue);
                    var mathFSign = Math.Sign(fvalue);
                    var mathDSign = Math.Sign(dvalue);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let fsign = _323a6b94e62b2729(fvalue);
  let fpow2 = _0dcf89ab5d6bd60c(fvalue);
  let dsign = _eee146c74a9bc322(dvalue);
  let dpow2 = _0f9f49a802919a8f(dvalue);
  let mathFSign = _c0668680ba7ef96e(fvalue);
  let mathDSign = _9a554cfca79bdc59(dvalue);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_FloatingPointNormalClassification_UseInlineThresholdChecks()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(float fvalue, double dvalue)
                {
                    var fnormal = float.IsNormal(fvalue);
                    var fsubnormal = float.IsSubnormal(fvalue);
                    var dnormal = double.IsNormal(dvalue);
                    var dsubnormal = double.IsSubnormal(dvalue);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let fnormal = isFinite(fvalue) && fvalue !== 0 && Math.abs(fvalue) >= 1.17549435e-38;
  let fsubnormal = isFinite(fvalue) && fvalue !== 0 && Math.abs(fvalue) < 1.17549435e-38;
  let dnormal = isFinite(dvalue) && dvalue !== 0 && Math.abs(dvalue) >= 2.2250738585072014e-308;
  let dsubnormal = isFinite(dvalue) && dvalue !== 0 && Math.abs(dvalue) < 2.2250738585072014e-308;
}", script);
	}

	[TestMethod]
	public void Visit_Reference_SingleSinCos_UsesRuntimeHelpers()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(float value)
                {
                    var pair = float.SinCos(value);
                    var pairPi = float.SinCosPi(value);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let pair = _9905e3952bca67bc(value);
  let pairPi = _2c792a5d6ef88cd1(value);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_BooleanSimpleMembers_UseInlineShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(bool left, bool right)
                {
                    var text = left.ToString();
                    var cmp = left.CompareTo(right);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let text = left ? ""True"" : ""False"";
  let cmp = left === right ? 0 : left ? 1 : -1;
}", script);
	}

	[TestMethod]
	public void Visit_Reference_Int32Intrinsics_UseInlineShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(int value, int sign, int min, int max)
                {
                    var copy = int.CopySign(value, sign);
                    var clamp = int.Clamp(value, min, max);
                    var signum = int.Sign(value);
                    var abs = int.Abs(value);
                    var even = int.IsEvenInteger(value);
                    var negative = int.IsNegative(value);
                    var odd = int.IsOddInteger(value);
                    var positive = int.IsPositive(value);
                    var pow2 = int.IsPow2(value);
                    var log2 = int.Log2(value);
                    var leadingZeros = int.LeadingZeroCount(value);
                    var trailingZeros = int.TrailingZeroCount(value);
                    var maxValue = int.Max(min, max);
                    var minValue = int.Min(min, max);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let copy = sign < 0 ? -Math.abs(value) : Math.abs(value);
  let clamp = Math.min(Math.max(value, min), max);
  let signum = value > 0 ? 1 : value < 0 ? -1 : 0;
  let abs = Math.abs(value);
  let even = (value & 1) === 0;
  let negative = value < 0;
  let odd = (value & 1) !== 0;
  let positive = value > 0;
  let pow2 = value > 0 && (value & value - 1) === 0;
  let log2 = Math.floor(Math.log2(value));
  let leadingZeros = Math.clz32(value);
  let trailingZeros = value === 0 ? 32 : 31 - Math.clz32(value & -value);
  let maxValue = Math.max(min, max);
  let minValue = Math.min(min, max);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_Int16AndUInt16Intrinsics_UseInlineShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(short signedValue, short signedSign, short signedMin, short signedMax, ushort unsignedValue, ushort unsignedMin, ushort unsignedMax)
                {
                    var signedCopy = short.CopySign(signedValue, signedSign);
                    var signedClamp = short.Clamp(signedValue, signedMin, signedMax);
                    var signedSignum = short.Sign(signedValue);
                    var signedAbs = short.Abs(signedValue);
                    var signedEven = short.IsEvenInteger(signedValue);
                    var signedNegative = short.IsNegative(signedValue);
                    var signedOdd = short.IsOddInteger(signedValue);
                    var signedPositive = short.IsPositive(signedValue);
                    var signedPow2 = short.IsPow2(signedValue);
                    var signedLog2 = short.Log2(signedValue);
                    var signedLeadingZeros = short.LeadingZeroCount(signedValue);
                    var signedTrailingZeros = short.TrailingZeroCount(signedValue);
                    var signedRotateLeft = short.RotateLeft(signedValue, 3);
                    var signedRotateRight = short.RotateRight(signedValue, 5);
                    var signedMaxValue = short.Max(signedMin, signedMax);
                    var signedMinValue = short.Min(signedMin, signedMax);
                    var unsignedClamp = ushort.Clamp(unsignedValue, unsignedMin, unsignedMax);
                    var unsignedSignum = ushort.Sign(unsignedValue);
                    var unsignedEven = ushort.IsEvenInteger(unsignedValue);
                    var unsignedOdd = ushort.IsOddInteger(unsignedValue);
                    var unsignedPow2 = ushort.IsPow2(unsignedValue);
                    var unsignedLog2 = ushort.Log2(unsignedValue);
                    var unsignedLeadingZeros = ushort.LeadingZeroCount(unsignedValue);
                    var unsignedTrailingZeros = ushort.TrailingZeroCount(unsignedValue);
                    var unsignedRotateLeft = ushort.RotateLeft(unsignedValue, 3);
                    var unsignedRotateRight = ushort.RotateRight(unsignedValue, 5);
                    var unsignedMaxValue = ushort.Max(unsignedMin, unsignedMax);
                    var unsignedMinValue = ushort.Min(unsignedMin, unsignedMax);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let signedCopy = signedSign < 0 ? -Math.abs(signedValue) : Math.abs(signedValue);
  let signedClamp = Math.min(Math.max(signedValue, signedMin), signedMax);
  let signedSignum = signedValue > 0 ? 1 : signedValue < 0 ? -1 : 0;
  let signedAbs = Math.abs(signedValue);
  let signedEven = (signedValue & 1) === 0;
  let signedNegative = signedValue < 0;
  let signedOdd = (signedValue & 1) !== 0;
  let signedPositive = signedValue > 0;
  let signedPow2 = signedValue > 0 && (signedValue & signedValue - 1) === 0;
  let signedLog2 = Math.floor(Math.log2(signedValue));
  let signedLeadingZeros = signedValue === 0 ? 16 : Math.clz32(signedValue & 0xFFFF) - 16;
  let signedTrailingZeros = signedValue === 0 ? 16 : Math.floor(Math.log2(signedValue & 0xFFFF & -(signedValue & 0xFFFF)));
  let signedRotateLeft = (((signedValue & 0xFFFF) << (3 & 15) | (signedValue & 0xFFFF) >>> 16 - (3 & 15)) & 0xFFFF) << 16 >> 16;
  let signedRotateRight = (((signedValue & 0xFFFF) >>> (5 & 15) | (signedValue & 0xFFFF) << 16 - (5 & 15)) & 0xFFFF) << 16 >> 16;
  let signedMaxValue = Math.max(signedMin, signedMax);
  let signedMinValue = Math.min(signedMin, signedMax);
  let unsignedClamp = Math.min(Math.max(unsignedValue, unsignedMin), unsignedMax);
  let unsignedSignum = unsignedValue === 0 ? 0 : 1;
  let unsignedEven = (unsignedValue & 1) === 0;
  let unsignedOdd = (unsignedValue & 1) !== 0;
  let unsignedPow2 = unsignedValue > 0 && (unsignedValue & unsignedValue - 1) === 0;
  let unsignedLog2 = Math.floor(Math.log2(unsignedValue));
  let unsignedLeadingZeros = unsignedValue === 0 ? 16 : Math.clz32(unsignedValue & 0xFFFF) - 16;
  let unsignedTrailingZeros = unsignedValue === 0 ? 16 : Math.floor(Math.log2(unsignedValue & 0xFFFF & -(unsignedValue & 0xFFFF)));
  let unsignedRotateLeft = (unsignedValue << (3 & 15) | unsignedValue >>> 16 - (3 & 15)) & 0xFFFF;
  let unsignedRotateRight = (unsignedValue >>> (5 & 15) | unsignedValue << 16 - (5 & 15)) & 0xFFFF;
  let unsignedMaxValue = Math.max(unsignedMin, unsignedMax);
  let unsignedMinValue = Math.min(unsignedMin, unsignedMax);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_UInt16AndUInt32DivRem_UseImportHelpers()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(ushort ushortLeft, ushort ushortRight, uint uintLeft, uint uintRight)
                {
                    var ushortPair = ushort.DivRem(ushortLeft, ushortRight);
                    var ushortQuotient = ushortPair.Quotient;
                    var ushortRemainder = ushortPair.Remainder;
                    var uintPair = uint.DivRem(uintLeft, uintRight);
                    var uintQuotient = uintPair.Quotient;
                    var uintRemainder = uintPair.Remainder;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  let ushortPair = _80e78c0aa0b98fef(ushortLeft, ushortRight);
  let ushortQuotient = ushortPair.Quotient;
  let ushortRemainder = ushortPair.Remainder;
  let uintPair = _8a073d758132b5bb(uintLeft, uintRight);
  let uintQuotient = uintPair.Quotient;
  let uintRemainder = uintPair.Remainder;
}", script);
	}

	[TestMethod]
	public void Visit_Reference_Int16DivRemAndPopCount_UseImportHelpers()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(short left, short right, short value)
                {
                    var pair = short.DivRem(left, right);
                    var quotient = pair.Quotient;
                    var remainder = pair.Remainder;
                    var count = short.PopCount(value);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  let pair = _b2c1f15fae072110(left, right);
  let quotient = pair.Quotient;
  let remainder = pair.Remainder;
  let count = _1636c956519f95fa(value);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_UInt16AndUInt32PopCount_UseImportHelpers()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(ushort ushortValue, uint uintValue)
                {
                    var ushortCount = ushort.PopCount(ushortValue);
                    var uintCount = uint.PopCount(uintValue);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let ushortCount = _2ea0cab4f3f489d9(ushortValue);
  let uintCount = _96cd49e102b39e5b(uintValue);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_ByteIntrinsics_UseInlineShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(byte value, byte min, byte max)
                {
                    var clamp = byte.Clamp(value, min, max);
                    var signum = byte.Sign(value);
                    var even = byte.IsEvenInteger(value);
                    var odd = byte.IsOddInteger(value);
                    var pow2 = byte.IsPow2(value);
                    var log2 = byte.Log2(value);
                    var maxValue = byte.Max(min, max);
                    var minValue = byte.Min(min, max);
                    var leadingZeros = byte.LeadingZeroCount(value);
                    var popCount = byte.PopCount(value);
                    var trailingZeros = byte.TrailingZeroCount(value);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let clamp = Math.min(Math.max(value, min), max);
  let signum = value === 0 ? 0 : 1;
  let even = (value & 1) === 0;
  let odd = (value & 1) !== 0;
  let pow2 = value > 0 && (value & value - 1) === 0;
  let log2 = Math.floor(Math.log2(value));
  let maxValue = Math.max(min, max);
  let minValue = Math.min(min, max);
  let leadingZeros = value === 0 ? 8 : Math.clz32(value & 0xFF) - 24;
  let popCount = (value & 1) + (value >> 1 & 1) + (value >> 2 & 1) + (value >> 3 & 1) + (value >> 4 & 1) + (value >> 5 & 1) + (value >> 6 & 1) + (value >> 7 & 1);
  let trailingZeros = value === 0 ? 8 : Math.floor(Math.log2(value & 0xFF & -(value & 0xFF)));
}", script);
	}

	[TestMethod]
	public void Visit_Reference_SByteUInt32AndUInt64Intrinsics_UseInlineShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(
                    sbyte signedValue,
                    sbyte signedSign,
                    sbyte signedMin,
                    sbyte signedMax,
                    uint unsignedValue,
                    uint unsignedMin,
                    uint unsignedMax,
                    ulong ulongValue,
                    ulong ulongMin,
                    ulong ulongMax)
                {
                    var signedCopy = sbyte.CopySign(signedValue, signedSign);
                    var signedClamp = sbyte.Clamp(signedValue, signedMin, signedMax);
                    var signedSignum = sbyte.Sign(signedValue);
                    var signedAbs = sbyte.Abs(signedValue);
                    var signedEven = sbyte.IsEvenInteger(signedValue);
                    var signedNegative = sbyte.IsNegative(signedValue);
                    var signedOdd = sbyte.IsOddInteger(signedValue);
                    var signedPositive = sbyte.IsPositive(signedValue);
                    var signedPow2 = sbyte.IsPow2(signedValue);
                    var signedLog2 = sbyte.Log2(signedValue);
                    var signedMaxValue = sbyte.Max(signedMin, signedMax);
                    var signedMinValue = sbyte.Min(signedMin, signedMax);
                    var unsignedClamp = uint.Clamp(unsignedValue, unsignedMin, unsignedMax);
                    var unsignedSignum = uint.Sign(unsignedValue);
                    var unsignedEven = uint.IsEvenInteger(unsignedValue);
                    var unsignedOdd = uint.IsOddInteger(unsignedValue);
                    var unsignedPow2 = uint.IsPow2(unsignedValue);
                    var unsignedLog2 = uint.Log2(unsignedValue);
                    var unsignedLeadingZeros = uint.LeadingZeroCount(unsignedValue);
                    var unsignedTrailingZeros = uint.TrailingZeroCount(unsignedValue);
                    var unsignedRotateLeft = uint.RotateLeft(unsignedValue, 3);
                    var unsignedRotateRight = uint.RotateRight(unsignedValue, 5);
                    var unsignedMaxValue = uint.Max(unsignedMin, unsignedMax);
                    var unsignedMinValue = uint.Min(unsignedMin, unsignedMax);
                    var ulongClamp = ulong.Clamp(ulongValue, ulongMin, ulongMax);
                    var ulongSignum = ulong.Sign(ulongValue);
                    var ulongEven = ulong.IsEvenInteger(ulongValue);
                    var ulongOdd = ulong.IsOddInteger(ulongValue);
                    var ulongPow2 = ulong.IsPow2(ulongValue);
                    var ulongLog2 = ulong.Log2(ulongValue);
                    var ulongMaxValue = ulong.Max(ulongMin, ulongMax);
                    var ulongMinValue = ulong.Min(ulongMin, ulongMax);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let signedCopy = signedSign < 0 ? -Math.abs(signedValue) : Math.abs(signedValue);
  let signedClamp = Math.min(Math.max(signedValue, signedMin), signedMax);
  let signedSignum = signedValue > 0 ? 1 : signedValue < 0 ? -1 : 0;
  let signedAbs = Math.abs(signedValue);
  let signedEven = (signedValue & 1) === 0;
  let signedNegative = signedValue < 0;
  let signedOdd = (signedValue & 1) !== 0;
  let signedPositive = signedValue > 0;
  let signedPow2 = signedValue > 0 && (signedValue & signedValue - 1) === 0;
  let signedLog2 = Math.floor(Math.log2(signedValue));
  let signedMaxValue = Math.max(signedMin, signedMax);
  let signedMinValue = Math.min(signedMin, signedMax);
  let unsignedClamp = Math.min(Math.max(unsignedValue, unsignedMin), unsignedMax);
  let unsignedSignum = unsignedValue === 0 ? 0 : 1;
  let unsignedEven = (unsignedValue & 1) === 0;
  let unsignedOdd = (unsignedValue & 1) !== 0;
  let unsignedPow2 = unsignedValue > 0 && (unsignedValue & unsignedValue - 1) === 0;
  let unsignedLog2 = Math.floor(Math.log2(unsignedValue));
  let unsignedLeadingZeros = Math.clz32(unsignedValue);
  let unsignedTrailingZeros = unsignedValue === 0 ? 32 : 31 - Math.clz32(unsignedValue >>> 0 & -(unsignedValue >>> 0));
  let unsignedRotateLeft = (unsignedValue << (3 & 31) | unsignedValue >>> 32 - (3 & 31)) >>> 0;
  let unsignedRotateRight = (unsignedValue >>> (5 & 31) | unsignedValue << 32 - (5 & 31)) >>> 0;
  let unsignedMaxValue = Math.max(unsignedMin, unsignedMax);
  let unsignedMinValue = Math.min(unsignedMin, unsignedMax);
  let ulongClamp = ulongValue < ulongMin ? ulongMin : ulongValue > ulongMax ? ulongMax : ulongValue;
  let ulongSignum = ulongValue === 0n ? 0 : 1;
  let ulongEven = ulongValue % 2n === 0n;
  let ulongOdd = ulongValue % 2n !== 0n;
  let ulongPow2 = ulongValue > 0n && (ulongValue & ulongValue - 1n) === 0n;
  let ulongLog2 = ulongValue === 0n ? 0n : BigInt(ulongValue.toString(2).length - 1);
  let ulongMaxValue = ulongMin > ulongMax ? ulongMin : ulongMax;
  let ulongMinValue = ulongMin < ulongMax ? ulongMin : ulongMax;
}", script);
	}

	[TestMethod]
	public void Visit_Reference_Int64AndMathBigIntIntrinsics_UseInlineShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(long longValue, long longMin, long longMax, ulong ulongValue, ulong ulongMin, ulong ulongMax)
                {
                    var longClamp = long.Clamp(longValue, longMin, longMax);
                    var longSignum = long.Sign(longValue);
                    var longMaxValue = System.Math.Max(longMin, longMax);
                    var longMinValue = System.Math.Min(longMin, longMax);
                    var mathLongClamp = System.Math.Clamp(longValue, longMin, longMax);
                    var mathLongSignum = System.Math.Sign(longValue);
                    var ulongMaxValue = System.Math.Max(ulongMin, ulongMax);
                    var ulongMinValue = System.Math.Min(ulongMin, ulongMax);
                    var mathUlongClamp = System.Math.Clamp(ulongValue, ulongMin, ulongMax);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let longClamp = longValue < longMin ? longMin : longValue > longMax ? longMax : longValue;
  let longSignum = longValue > 0n ? 1 : longValue < 0n ? -1 : 0;
  let longMaxValue = longMin > longMax ? longMin : longMax;
  let longMinValue = longMin < longMax ? longMin : longMax;
  let mathLongClamp = longValue < longMin ? longMin : longValue > longMax ? longMax : longValue;
  let mathLongSignum = longValue > 0n ? 1 : longValue < 0n ? -1 : 0;
  let ulongMaxValue = ulongMin > ulongMax ? ulongMin : ulongMax;
  let ulongMinValue = ulongMin < ulongMax ? ulongMin : ulongMax;
  let mathUlongClamp = ulongValue < ulongMin ? ulongMin : ulongValue > ulongMax ? ulongMax : ulongValue;
}", script);
	}

	[TestMethod]
	public void Visit_Reference_BigIntegerSimpleIntrinsics_UseInlineShapes()
	{
		var block = GetBlockOperation(@"
            using System.Numerics;

            class TestClass
            {
                void TestMethod(BigInteger value, BigInteger sign, BigInteger left, BigInteger right)
                {
                    var abs = BigInteger.Abs(value);
                    var add = BigInteger.Add(left, right);
                    var copy = BigInteger.CopySign(value, sign);
                    var divide = BigInteger.Divide(left, right);
                    var max = BigInteger.Max(left, right);
                    var min = BigInteger.Min(left, right);
                    var multiply = BigInteger.Multiply(left, right);
                    var negate = BigInteger.Negate(value);
                    var even = BigInteger.IsEvenInteger(value);
                    var negative = BigInteger.IsNegative(value);
                    var odd = BigInteger.IsOddInteger(value);
                    var positive = BigInteger.IsPositive(value);
                    var remainder = BigInteger.Remainder(left, right);
                    var subtract = BigInteger.Subtract(left, right);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let abs = value < 0n ? -value : value;
  let add = left + right;
  let copy = sign < 0n ? value < 0n ? value : -value : value < 0n ? -value : value;
  let divide = left / right;
  let max = left > right ? left : right;
  let min = left < right ? left : right;
  let multiply = left * right;
  let negate = -value;
  let even = value % 2n === 0n;
  let negative = value < 0n;
  let odd = value % 2n !== 0n;
  let positive = value > 0n;
  let remainder = left % right;
  let subtract = left - right;
}", script);
	}

	[TestMethod]
	public void Visit_Reference_BigIntegerDivRemOut_UsesHelperReturnPacking()
	{
		var block = GetBlockOperation(@"
            using System.Numerics;

            class TestClass
            {
                void TestMethod(BigInteger left, BigInteger right)
                {
                    BigInteger remainder;
                    var quotient = BigInteger.DivRem(left, right, out remainder);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0;
  let remainder;
  let quotient = (v$0 = _598611fb2b8a064a(left, right, remainder), remainder = v$0[1], v$0[0]);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_BigIntegerMaxMinMagnitude_UsesRuntimeHelpers()
	{
		var block = GetBlockOperation(@"
            using System.Numerics;

            class TestClass
            {
                void TestMethod(BigInteger left, BigInteger right)
                {
                    var max = BigInteger.MaxMagnitude(left, right);
                    var min = BigInteger.MinMagnitude(left, right);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let max = _d305de2c64e85995(left, right);
  let min = _fef56ccd17b22e88(left, right);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_Int64MaxMinMagnitude_UsesRuntimeHelpers()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(long left, long right)
                {
                    var max = long.MaxMagnitude(left, right);
                    var min = long.MinMagnitude(left, right);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let max = _9618dc0d855ee729(left, right);
  let min = _bfad1ee52075b36e(left, right);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_SingleMagnitudeHelpers_UseRuntimeHelpers()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(float left, float right)
                {
                    var max = float.MaxMagnitude(left, right);
                    var maxNumber = float.MaxMagnitudeNumber(left, right);
                    var min = float.MinMagnitude(left, right);
                    var minNumber = float.MinMagnitudeNumber(left, right);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let max = _7c146ff0a50e958f(left, right);
  let maxNumber = _b7b1d7781578b7e0(left, right);
  let min = _e5a7b14f707c69f7(left, right);
  let minNumber = _4a2ec5d010e27cb1(left, right);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_DoubleMagnitudeNumberHelpers_UseRuntimeHelpers()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(double left, double right)
                {
                    var max = double.MaxMagnitude(left, right);
                    var maxNumber = double.MaxMagnitudeNumber(left, right);
                    var min = double.MinMagnitude(left, right);
                    var minNumber = double.MinMagnitudeNumber(left, right);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let max = _b6202851542d164c(left, right);
  let maxNumber = _7f7b38b043f3f42f(left, right);
  let min = _bb1daa880a2ad14e(left, right);
  let minNumber = _315c6cdfa11efcf2(left, right);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_Int16MaxMinMagnitude_UsesRuntimeHelpers()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(short left, short right)
                {
                    var max = short.MaxMagnitude(left, right);
                    var min = short.MinMagnitude(left, right);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let max = _ea75510d32bc8099(left, right);
  let min = _63d3d54252a49e29(left, right);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_BigIntegerCompareEqualsToString_UseWhiteListShapes()
	{
		var block = GetBlockOperation(@"
            using System.Numerics;

            class TestClass
            {
                void TestMethod(BigInteger left, BigInteger right, object obj)
                {
                    var cmp = BigInteger.Compare(left, right);
                    var typedCmp = left.CompareTo(right);
                    var typedEq = left.Equals(right);
                    var objectEq = left.Equals(obj);
                    var text = left.ToString();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let cmp = left < right ? -1 : left > right ? 1 : 0;
  let typedCmp = left < right ? -1 : left > right ? 1 : 0;
  let typedEq = left === right;
  let objectEq = left === obj;
  let text = left.toString();
}", script);
	}

	[TestMethod]
	public void Visit_Reference_PrimitiveCompareToSameType_UseInlineShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(byte b1, byte b2, sbyte sb1, sbyte sb2, short s1, short s2, int i1, int i2, char c1, char c2)
                {
                    var byteCmp = b1.CompareTo(b2);
                    var sbyteCmp = sb1.CompareTo(sb2);
                    var shortCmp = s1.CompareTo(s2);
                    var intCmp = i1.CompareTo(i2);
                    var charCmp = c1.CompareTo(c2);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let byteCmp = b1 < b2 ? -1 : b1 > b2 ? 1 : 0;
  let sbyteCmp = sb1 < sb2 ? -1 : sb1 > sb2 ? 1 : 0;
  let shortCmp = s1 < s2 ? -1 : s1 > s2 ? 1 : 0;
  let intCmp = i1 < i2 ? -1 : i1 > i2 ? 1 : 0;
  let charCmp = c1 < c2 ? -1 : c1 > c2 ? 1 : 0;
}", script);
	}

	[TestMethod]
	public void Visit_Reference_StringBuilderAppend_UsesInlineSequence()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(string text)
                {
                    var sb = new System.Text.StringBuilder();
                    var appended = sb.Append(text);
                    var value = sb.ToString();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let sb = [];
  let appended = (sb.push(...(text ?? '').split('')), sb);
  let value = sb.join('');
}", script);
	}

	[TestMethod]
	public void Visit_Reference_StringBuilderClearAndLength_UseInlineShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(string text)
                {
                    var sb = new System.Text.StringBuilder(text);
                    var length = sb.Length;
                    var cleared = sb.Clear();
                    var nextLength = sb.Length;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let sb = (text ?? '').split('');
  let length = sb.length;
  let cleared = (sb.length = 0, sb);
  let nextLength = sb.length;
}", script);
	}

	[TestMethod]
	public void Visit_Reference_NumericStaticPredicates_UseInlineShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(double value)
                {
                    var negativeZero = double.NegativeZero;
                    var isNegative = double.IsNegative(negativeZero);
                    var isInfinity = double.IsInfinity(value);
                    var isPositiveInfinity = double.IsPositiveInfinity(value);
                    var isNegativeInfinity = double.IsNegativeInfinity(value);
                    var isNaN = double.IsNaN(value);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		// 局部变量名与全局 isNaN 冲突时，当前实现会提升到 Number.isNaN 以避免遮蔽。
		AssertScriptEqual(@"{
  let negativeZero = -0;
  let isNegative = Object.is(negativeZero, -0) || negativeZero < 0;
  let isInfinity = value === Infinity || value === -Infinity;
  let isPositiveInfinity = value === Infinity;
  let isNegativeInfinity = value === -Infinity;
  let isNaN = Number.isNaN(value);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_IsFinite_WithShadowedGlobalName_UsesNumberHost()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(double value)
                {
                    bool isFinite = double.IsFinite(value);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let isFinite = Number.isFinite(value);
}", script);
	}

	/// <summary>
	/// 测试 VisitParameterReference - 简单参数引用
	/// C# 示例：void Method(int x) { Console.WriteLine(x); }
	/// 转换结果：function Method(x) { Console.WriteLine(x); }
	/// </summary>
	[TestMethod]
	public void Visit_ParameterReference_Simple()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(int param)
                {
                    int x = param;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = param;
}", script);
	}

	/// <summary>
	/// 测试 VisitParameterReference - 多个参数引用
	/// </summary>
	[TestMethod]
	public void Visit_ParameterReference_Multiple()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(int a, int b, int c)
                {
                    int sum = a + b + c;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let sum = a + b + c;
}", script);
	}

	#endregion

	#region VisitFieldReference - 字段引用

	/// <summary>
	/// 测试 VisitFieldReference - 静态常量字段 PositiveInfinity
	/// C# 示例：double x = double.PositiveInfinity;
	/// 转换结果：let x = Infinity;
	/// </summary>
	[TestMethod]
	public void Visit_FieldReference_StaticPositiveInfinity()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    double x = double.PositiveInfinity;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = Infinity;
}", script);
	}

	/// <summary>
	/// 测试 VisitFieldReference - 静态常量字段 NegativeInfinity
	/// C# 示例：double x = double.NegativeInfinity;
	/// 转换结果：let x = -Infinity;
	/// </summary>
	[TestMethod]
	public void Visit_FieldReference_StaticNegativeInfinity()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    double x = double.NegativeInfinity;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = -Infinity;
}", script);
	}

	/// <summary>
	/// 测试 VisitFieldReference - 静态常量字段 NaN
	/// C# 示例：double x = double.NaN;
	/// 转换结果：let x = NaN;
	/// </summary>
	[TestMethod]
	public void Visit_FieldReference_StaticNaN()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    double x = double.NaN;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = NaN;
}", script);
	}

	/// <summary>
	/// 测试 VisitFieldReference - 静态常量字段 Epsilon
	/// C# 示例：double x = double.Epsilon;
	/// 转换结果：let x = Number.MIN_VALUE;
	/// </summary>
	[TestMethod]
	public void Visit_FieldReference_StaticEpsilon()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    double x = double.Epsilon;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = Number.MIN_VALUE;
}", script);
	}

	/// <summary>
	/// 测试 VisitFieldReference - 静态常量字段 MaxValue
	/// C# 示例：double x = double.MaxValue;
	/// 转换结果：let x = Number.MAX_VALUE;
	/// </summary>
	[TestMethod]
	public void Visit_FieldReference_StaticMaxValue()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    double x = double.MaxValue;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = Number.MAX_VALUE;
}", script);
	}

	/// <summary>
	/// 测试 VisitFieldReference - 静态常量字段 MinValue
	/// C# 示例：double x = double.MinValue;
	/// 转换结果：let x = Number.MIN_VALUE;
	/// </summary>
	[TestMethod]
	public void Visit_FieldReference_StaticMinValue()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    double x = double.MinValue;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = -Number.MAX_VALUE;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试 VisitFieldReference - long.MaxValue
	/// C# 示例：long x = long.MaxValue;
	/// 转换结果：let x = Number.MAX_SAFE_INTEGER;
	/// </summary>
	[TestMethod]
	public void Visit_FieldReference_LongMaxValue()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    long x = long.MaxValue;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = 9223372036854775807n;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试 VisitFieldReference - long.MinValue
	/// C# 示例：long x = long.MinValue;
	/// 转换结果：let x = Number.MIN_SAFE_INTEGER;
	/// </summary>
	[TestMethod]
	public void Visit_FieldReference_LongMinValue()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    long x = long.MinValue;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = -9223372036854775808n;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试 VisitFieldReference - 实例字段访问
	/// C# 示例：obj.field
	/// 转换结果：obj.field
	/// </summary>
	[TestMethod]
	public void Visit_FieldReference_InstanceField()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public int Field;

                void TestMethod()
                {
                    TestClass obj = new TestClass();
                    int x = obj.Field;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  let obj = new TestClass;
  let x = obj.Field;
}", script);
	}

	[TestMethod]
	public void Visit_FieldReference_InstanceField_UsesConfiguredAlias()
	{
		var block = GetBlockOperation(@"
            using System.ComponentModel;

            class TestClass
            {
                [Description(""@#count"")]
                public int Value;

                void TestMethod()
                {
                    TestClass obj = new TestClass();
                    int x = obj.Value;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let obj = new TestClass;
  let x = obj.count;
}", script);
	}

	[TestMethod]
	public void Visit_FieldReference_UnsupportedExternalConstField_Throws()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var tab = System.CodeDom.Compiler.IndentedTextWriter.DefaultTabString;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		Assert.Throws<OperationTransformationException>(() =>
		{
			_ = walker.Visit(block, new());
		});
	}

	#endregion

	#region VisitPropertyReference - 属性引用

	/// <summary>
	/// 测试 VisitPropertyReference - 实例属性访问
	/// C# 示例：string str = "hello"; int len = str.Length;
	/// 转换结果：let str = "hello"; let len = str.Length;
	/// </summary>
	[TestMethod]
	public void Visit_PropertyReference_InstanceProperty()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string str = ""hello"";
                    int len = str.Length;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let str = ""hello"";
  let len = str.length;
}", script);

	}

	/// <summary>
	/// 测试 VisitPropertyReference - 静态属性访问
	/// C# 示例：DateTime.Now
	/// 转换结果：DateTime.Now
	/// </summary>
	[TestMethod]
	public void Visit_PropertyReference_StaticProperty()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    DateTime now = DateTime.Now;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let now = _ee9dd166a34a2fa5();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());

	}

	/// <summary>
	/// 测试 VisitPropertyReference - 链式属性访问
	/// C# 示例：obj.Prop1.Prop2
	/// 转换结果：obj.Prop1.Prop2
	/// </summary>
	[TestMethod]
	public void Visit_PropertyReference_Chained()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public TestClass Prop { get; set; }

                void TestMethod()
                {
                    TestClass obj = new TestClass();
                    var value = obj.Prop.Prop;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  let obj = new TestClass;
  let value = obj.Prop.Prop;
}", script);
	}

	#endregion

	#region VisitMethodReference - 方法引用

	[TestMethod]
	public void Visit_Invocation_ObjectPrototypeMethod_HasOwnProperty()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public int Value { get; set; }

                void TestMethod()
                {
                    var obj = new TestClass();
                    var hasValue = obj.HasOwnProperty(""Value"");
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let obj = new TestClass;
  let hasValue = obj.hasOwnProperty(""Value"");
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Invocation_ObjectStaticMethod_Keys()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var keys = Object.Keys(new TestClass());
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let keys = Object.keys(new TestClass);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_IObjectNumericIndexer_UsesJsIndexAccess()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    IObject obj = Object.Create(null)!;
                    var value = obj[1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let obj = Object.create(null);
  let value = obj[1];
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Invocation_ObjectStaticMethod_Is()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var same = Object.Is(1, 1);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let same = Object.is(1, 1);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Invocation_ObjectStaticMethod_HasOwn_UsesJsMemberName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public int Value { get; set; }

                void TestMethod()
                {
                    var hasValue = Object.HasOwn(new TestClass(), ""Value"");
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let hasValue = Object.hasOwn(new TestClass, ""Value"");
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试 VisitMethodReference - 静态方法引用
	/// C# 示例：Func<int, int> abs = Math.Abs;
	/// 转换结果：let abs = Math.Abs;
	/// </summary>
	[TestMethod]
	public void Visit_MethodReference_StaticMethod()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Func<int, int> abs = Math.Abs;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let abs = Math.abs;
}", script);

	}

	[TestMethod]
	public void Visit_MethodReference_TypedArrayStaticMethod_UsesConcreteRuntimeHost()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Func<byte[], Uint8Array> factory = Uint8Array.Of;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let factory = Uint8Array.of;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_TypedArrayAliasHost_UsesConcreteRuntimeHost()
	{
		var block = GetBlockOperation(@"
            using Bytes = ECMAScript.Uint8Array;

            class TestClass
            {
                void TestMethod()
                {
                    var bytes = Bytes.Of(1, 2, 3);
                    Number size = Bytes.BYTES_PER_ELEMENT;
                    Func<byte[], Uint8Array> factory = Bytes.Of;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let bytes = Uint8Array.of(1, 2, 3);
  let size = Uint8Array.BYTES_PER_ELEMENT;
  let factory = Uint8Array.of;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_TypedArrayNamespaceAliasHost_UsesConcreteRuntimeHost()
	{
		var block = GetBlockOperation(@"
            using E = ECMAScript;

            class TestClass
            {
                void TestMethod()
                {
                    var bytes = E.Uint8Array.Of(1, 2, 3);
                    Number size = E.Uint8Array.BYTES_PER_ELEMENT;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let bytes = Uint8Array.of(1, 2, 3);
  let size = Uint8Array.BYTES_PER_ELEMENT;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_TypedArrayGlobalAliasHost_UsesConcreteRuntimeHost()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var bytes = global::ECMAScript.Uint8Array.Of(1, 2, 3);
                    Number size = global::ECMAScript.Uint8Array.BYTES_PER_ELEMENT;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let bytes = Uint8Array.of(1, 2, 3);
  let size = Uint8Array.BYTES_PER_ELEMENT;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_ArrayAliasHost_UsesRuntimeHost()
	{
		var block = GetBlockOperation(@"
            using IntArray = ECMAScript.Array<int>;

            class TestClass
            {
                void TestMethod()
                {
                    var created = IntArray.Of(1, 2, 3);
                    var mapped = IntArray.From(new int[] { 1, 2, 3 }, (value, index) => value + index);
                    var isArray = IntArray.IsArray(new int[] { 1, 2, 3 });
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let created = Array.of(1, 2, 3);
  let mapped = Array.from([1, 2, 3], (value, index) => {
    return value + index;
  });
  let isArray = Array.isArray([1, 2, 3]);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_NestedStaticHostAlias_UsesRuntimeHost()
	{
		var block = GetBlockOperation(@"
            using NumberFormatter = ECMAScript.Intl.NumberFormat;
            using I18n = ECMAScript.Intl;

            class TestClass
            {
                void TestMethod()
                {
                    var locales = NumberFormatter.SupportedLocalesOf(""en-US"");
                    var locales2 = I18n.NumberFormat.SupportedLocalesOf(""zh-CN"");
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let locales = Intl.NumberFormat.supportedLocalesOf(""en-US"");
  let locales2 = Intl.NumberFormat.supportedLocalesOf(""zh-CN"");
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_NestedStaticHostGlobalAlias_UsesRuntimeHost()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var locales = global::ECMAScript.Intl.NumberFormat.SupportedLocalesOf(""en-US"");
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let locales = Intl.NumberFormat.supportedLocalesOf(""en-US"");
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_WebIdlNamespaceHostGlobalQualified_UsesRuntimeHost()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var supported = global::ECMAScript.CSS.CSS.Supports(""display"", ""grid"");
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let supported = CSS.supports(""display"", ""grid"");
}", script);
	}

	[TestMethod]
	public void Visit_Reference_WebIdlNamespaceHostAlias_UsesRuntimeHost()
	{
		var block = GetBlockOperation(@"
            global using CssHost = global::ECMAScript.CSS.CSS;

            class TestClass
            {
                void TestMethod()
                {
                    var supported = CssHost.Supports(""display"", ""grid"");
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let supported = CSS.supports(""display"", ""grid"");
}", script);
	}

	/// <summary>
	/// 测试 VisitMethodReference - 实例方法引用
	/// C# 示例：Action<string> write = Console.WriteLine;
	/// 转换结果：let write = Console.WriteLine;
	/// </summary>
	[TestMethod]
	public void Visit_MethodReference_InstanceMethod()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Action action = Console.WriteLine;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(
@"{
  let action = console.log;
}", script);

	}

	[TestMethod]
	public void Visit_MethodReference_InstanceMethod1()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var x = new BigInteger(100);
                    var y = BigInteger.Zero;
					var z = BigInteger.Parse(""33"");
					var w = y++;
                    var v = z * 33;
					var a = y.CompareTo(y);
					var b = z.ToString();
					var c = w.Equals(v);
                    Console.WriteLine(z);
                    TestMethod(x,y);
                }

				BigInteger TestMethod(BigInteger a,BigInteger b)
                {
                    return a + b;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(
@"{
  let x = BigInt(100);
  let y = 0n;
  let z = _155212572c9a3297(""33"");
  let w = y++;
  let v = z * BigInt(33);
  let a = y < y ? -1 : y > y ? 1 : 0;
  let b = z.toString();
  let c = w === v;
  console.log(z);
  this.TestMethod_86dba5f71d944ea3(x, y);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());

	}

	#endregion

	#region VisitInstanceReference - 实例引用

	/// <summary>
	/// 测试 VisitInstanceReference - this 引用
	/// C# 示例：this.Property
	/// 转换结果：this.Property
	/// </summary>
	[TestMethod]
	public void Visit_InstanceReference_This()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                private int _field;

                void TestMethod()
                {
                    int x = this._field;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = this._field;
}", script);
	}

	/// <summary>
	/// 测试 VisitInstanceReference - this 关键字
	/// C# 示例：int x = this._field;
	/// 转换结果：let x = this._field;
	/// </summary>
	[TestMethod]
	public void Visit_InstanceReference_ThisMethodCall()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                private int _field;

                void TestMethod()
                {
                    int x = this._field;
                    int y = this._field + 1;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let x = this._field;
  let y = this._field + 1;
}", script);
	}

	#endregion

	#region VisitArrayElementReference - 数组元素访问

	/// <summary>
	/// 测试 VisitArrayElementReference - 简单数组索引
	/// C# 示例：array[0]
	/// 转换结果：array[0]
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_SimpleIndex()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3];
                    int x = array[0];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3];
  let x = array[0];
}", script);
	}

	/// <summary>
	/// 测试 VisitArrayElementReference - 变量索引
	/// C# 示例：array[i]
	/// 转换结果：array[i]
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_VariableIndex()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3];
                    int i = 1;
                    int x = array[i];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3];
  let i = 1;
  let x = array[i];
}", script);
	}

	/// <summary>
	/// 测试 VisitArrayElementReference - 从末尾索引 (^1)
	/// C# 示例：array[^1]
	/// 转换结果：array[array.length - 1]
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_FromEnd()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3];
                    int x = array[^1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3];
  let x = array[array.length - 1];
}", script);
	}

	/// <summary>
	/// 测试 VisitArrayElementReference - 从末尾变量索引 (^n)
	/// C# 示例：array[^n]
	/// 转换结果：array[array.length - n]
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_FromEndVariable()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3];
                    int n = 2;
                    int x = array[^n];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3];
  let n = 2;
  let x = array[array.length - n];
}", script);
	}

	/// <summary>
	/// 测试 VisitArrayElementReference - 范围操作完整范围 [start..end]
	/// C# 示例：array[1..3]
	/// 转换结果：array.slice(1, 4)
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_RangeComplete()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    int[] slice = array[1..3];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let slice = array.slice(1, 3);
}", script);
	}

	/// <summary>
	/// 测试 VisitArrayElementReference - 范围操作只有起始 [start..]
	/// C# 示例：array[2..]
	/// 转换结果：array.slice(2)
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_RangeFromStart()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    int[] slice = array[2..];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let slice = array.slice(2);
}", script);
	}

	/// <summary>
	/// 测试 VisitArrayElementReference - 范围操作只有结束 [..end]
	/// C# 示例：array[..3]
	/// 转换结果：array.slice(0, 4)
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_RangeToEnd()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    int[] slice = array[..3];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let slice = array.slice(0, 3);
}", script);
	}

	/// <summary>
	/// 测试 VisitArrayElementReference - 范围操作全部 [..]
	/// C# 示例：array[..]
	/// 转换结果：array.slice()
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_RangeAll()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    int[] copy = array[..];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let copy = array.slice();
}", script);
	}

	/// <summary>
	/// 测试 VisitArrayElementReference - 范围操作从末尾 [^3..^1]
	/// C# 示例：array[^3..^1]
	/// 转换结果：array.slice(array.length - 3, array.length - 1)
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_RangeFromEnd()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    int[] slice = array[^3..^1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let slice = array.slice(array.length - 3, array.length - 1);
}", script);
	}

	/// <summary>
	/// 测试 VisitArrayElementReference - 表达式索引
	/// C# 示例：array[i + 1]
	/// 转换结果：array[i + 1]
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_ExpressionIndex()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    int i = 1;
                    int x = array[i + 1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let i = 1;
  let x = array[i + 1];
}", script);
	}

	[TestMethod]
	public void Visit_ArrayElementReference_NestedImplicitIndexer_PreservesOuterArrayTarget()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [10, 20, 30];
                    var indexes = new List<int> { 1, 2 };
                    int x = array[indexes[^1]];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let array = [10, 20, 30];
  let indexes = [1, 2];
  let x = array[_d389c31d59037b42(indexes, indexes.length - 1)];
}", script);
	}

	#endregion

	#region VisitImplicitIndexerReference - 隐式索引器引用

	/// <summary>
	/// 测试 VisitImplicitIndexerReference - 隐式索引器从末尾
	/// C# 示例：array[^1]
	/// 转换结果：array[array.length - 1]
	/// </summary>
	[TestMethod]
	public void Visit_ImplicitIndexerReference_FromEnd()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3];
                    int x = array[^1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3];
  let x = array[array.length - 1];
}", script);
	}

	[TestMethod]
	public void Visit_ImplicitIndexerReference_StringFromEnd_UsesWhitelistLengthAndIndexer()
	{
		var operation = GetFirstOperation<IImplicitIndexerReferenceOperation>(@"
            class TestClass
            {
                char M(string text)
                {
                    return text[^1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(operation, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"_5ad63706a889c294(text, text.length - 1)", script);
	}

	[TestMethod]
	public void Visit_Reference_CharIntrinsics_PreserveStringCarrierSemantics()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(string text)
                {
                    char last = text[^1];
                    char upper = char.ToUpperInvariant(last);
                    char lower = char.ToLowerInvariant(last);
                    bool letter = char.IsLetter(last);
                    bool whitespace = char.IsWhiteSpace(last);
                    double numeric = char.GetNumericValue(last);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let last = _5ad63706a889c294(text, text.length - 1);
  let upper = last.toUpperCase();
  let lower = last.toLowerCase();
  let letter = /[a-zA-Z]/.test(last);
  let whitespace = _16e351e6f7b127f7(last);
  let numeric = _d86c1e9964250116(last);
}", script);
	}

	[TestMethod]
	public void Visit_ImplicitIndexerReference_StringRange_UsesExclusiveEndSubstring()
	{
		var operation = GetFirstOperation<IImplicitIndexerReferenceOperation>(@"
            class TestClass
            {
                string M(string text)
                {
                    return text[1..^1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(operation, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"text.substring(1, 1 + (text.length - 1 - 1))", script);
	}

	[TestMethod]
	public void Visit_ImplicitIndexerReference_StringFromEnd_ComplexReceiver_EvaluatesReceiverOnce()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string text = ""abc"";
                    Func<string> next = () => text;
                    char last = next()[^1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0;
  let text = ""abc"";
  let next = () => {
    return text;
  };
  let last = (v$0 = next(), _5ad63706a889c294(v$0, v$0.length - 1));
}", script);
	}

	[TestMethod]
	public void Visit_ArrayElementReference_FromEndAndRange_ComplexReceiver_EvaluatesReceiverOnce()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] values = [1, 2, 3];
                    Func<int[]> next = () => values;
                    int last = next()[^1];
                    int[] middle = next()[1..^1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0, v$1;
  let values = [1, 2, 3];
  let next = () => {
    return values;
  };
  let last = (v$0 = next(), v$0[v$0.length - 1]);
  let middle = (v$1 = next(), v$1.slice(1, v$1.length - 1));
}", script);
	}

	[TestMethod]
	public void Visit_ArrayElementAssignment_FromEnd_ComplexReceiver_EvaluatesReceiverOnce()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] values = [1, 2, 3];
                    Func<int[]> next = () => values;
                    next()[^1] = 4;
                    next()[^1] += 5;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0, v$1;
  let values = [1, 2, 3];
  let next = () => {
    return values;
  };
  v$0 = next(), v$0[v$0.length - 1] = 4;
  v$1 = next(), v$1[v$1.length - 1] += 5;
}", script);
	}

	#endregion

	#region 综合测试

	/// <summary>
	/// 综合测试 - 字段和属性引用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_FieldAndPropertyCombined()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                private int _field;
                public int Property { get; set; }

                void TestMethod(int param)
                {
                    int local = 5;
                    int a = local;
                    int b = param;
                    int c = this._field;
                    int d = this.Property;
                    double e = double.MaxValue;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  let local = 5;
  let a = local;
  let b = param;
  let c = this._field;
  let d = this.Property;
  let e = Number.MAX_VALUE;
}", script);
	}

	/// <summary>
	/// 综合测试 - 数组操作
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ArrayOperationsCombined()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    int first = array[0];
                    int last = array[^1];
                    int[] middle = array[1..4];
                    int[] fromTwo = array[2..];
                    int[] uptoThree = array[..3];
                    int[] copy = array[..];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let first = array[0];
  let last = array[array.length - 1];
  let middle = array.slice(1, 4);
  let fromTwo = array.slice(2);
  let uptoThree = array.slice(0, 3);
  let copy = array.slice();
}", script);
	}

	/// <summary>
	/// 综合测试 - 特殊常量字段
	/// </summary>
	[TestMethod]
	public void Visit_Reference_SpecialConstants()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    double posInf = double.PositiveInfinity;
                    double negInf = double.NegativeInfinity;
                    double nan = double.NaN;
                    double epsilon = double.Epsilon;
                    double maxVal = double.MaxValue;
                    double minVal = double.MinValue;
                    long longMax = long.MaxValue;
                    long longMin = long.MinValue;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let posInf = Infinity;
  let negInf = -Infinity;
  let nan = NaN;
  let epsilon = Number.MIN_VALUE;
  let maxVal = Number.MAX_VALUE;
  let minVal = -Number.MAX_VALUE;
  let longMax = 9223372036854775807n;
  let longMin = -9223372036854775808n;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	#endregion

	#region 扩展测试用例 - 多维数组

	/// <summary>
	/// 测试二维数组元素访问
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_TwoDimensional()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[,] matrix = new int[2, 3];
                    int x = matrix[0, 1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let matrix = new Array(2).fill().map(() => new Array(3));
  let x = matrix[0][1];
}", script);
	}

	/// <summary>
	/// 测试交错数组元素访问
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_Jagged()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[][] jagged = new int[2][];
                    int x = jagged[0][1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let jagged = new Array(2);
  let x = jagged[0][1];
}", script);
	}

	/// <summary>
	/// 测试数组元素赋值
	/// </summary>
	[TestMethod]
	public void Visit_ArrayElementReference_Assignment()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = new int[5];
                    array[0] = 10;
                    array[1] = 20;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = new Array(5);
  array[0] = 10;
  array[1] = 20;
}", script);
	}

	#endregion

	#region 扩展测试用例 - 属性引用变体

	/// <summary>
	/// 测试只读属性访问
	/// </summary>
	[TestMethod]
	public void Visit_PropertyReference_ReadOnly()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public int ReadOnlyProp { get; }

                void TestMethod()
                {
                    TestClass obj = new TestClass();
                    int x = obj.ReadOnlyProp;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  let obj = new TestClass;
  let x = obj.ReadOnlyProp;
}", script);
	}

	/// <summary>
	/// 测试静态属性访问 DateTime.UtcNow
	/// </summary>
	[TestMethod]
	public void Visit_PropertyReference_DateTimeUtcNow()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    DateTime utcNow = DateTime.UtcNow;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let utcNow = _d4c39bdf47f391cf();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试 DateTime 日期属性
	/// </summary>
	[TestMethod]
	public void Visit_PropertyReference_DateTimeProperties()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    DateTime now = DateTime.Now;
                    int year = now.Year;
                    int month = now.Month;
                    int day = now.Day;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let now = _ee9dd166a34a2fa5();
  let year = _9d56b09432f81c05(now);
  let month = _a8a6b6e36a0ea736(now);
  let day = _3b9ecf5fd3c301db(now);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试数组长度属性
	/// </summary>
	[TestMethod]
	public void Visit_PropertyReference_ArrayLength()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] array = [1, 2, 3, 4, 5];
                    int len = array.Length;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [1, 2, 3, 4, 5];
  let len = array.length;
}", script);
	}

	/// <summary>
	/// 测试字符串长度属性
	/// </summary>
	[TestMethod]
	public void Visit_PropertyReference_StringLength()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string text = ""Hello"";
                    int len = text.Length;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let text = ""Hello"";
  let len = text.length;
}", script);
	}

	#endregion

	#region 扩展测试用例 - 方法引用变体

	/// <summary>
	/// 测试实例方法引用带绑定
	/// </summary>
	[TestMethod]
	public void Visit_MethodReference_BoundMethod()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    TestClass obj = new TestClass();
                    Action action = obj.DoSomething;
                }

                void DoSomething() { }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  let obj = new TestClass;
  let action = obj.DoSomething.bind(obj);
}", script);
	}

	/// <summary>
	/// 测试复杂 receiver 的实例方法引用只求值一次
	/// </summary>
	[TestMethod]
	public void Visit_MethodReference_BoundMethod_ComplexReceiver_EvaluatesReceiverOnce()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Action action = Create().DoSomething;
                }

                TestClass Create() => new TestClass();

                void DoSomething() { }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  let v$1;
  let action = (v$1 = this.Create(), v$1.DoSomething.bind(v$1));
}", script);
	}

	/// <summary>
	/// 测试带参数方法引用
	/// </summary>
	[TestMethod]
	public void Visit_MethodReference_WithParameters()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Func<int, int, int> add = Add;
                }

                int Add(int a, int b) => a + b;
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  let add = this.Add.bind(this);
}", script);
	}

	#endregion

	#region 扩展测试用例 - this引用变体

	/// <summary>
	/// 测试this传递给方法
	/// </summary>
	[TestMethod]
	public void Visit_InstanceReference_ThisAsArgument()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Process(this);
                }

                void Process(TestClass obj) { }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  this.Process(this);
}", script);
	}

	/// <summary>
	/// 测试this返回
	/// </summary>
	[TestMethod]
	public void Visit_InstanceReference_ReturnThis()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                TestClass TestMethod()
                {
                    return this;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  return this;
}", script);
	}

	#endregion

	#region 扩展测试用例 - 索引器引用

	/// <summary>
	/// 测试列表索引器访问
	/// </summary>
	[TestMethod]
	public void Visit_IndexerReference_List()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new List<int> { 1, 2, 3 };
                    int first = list[0];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let list = [1, 2, 3];
  let first = _d389c31d59037b42(list, 0);
}", script);
	}

	/// <summary>
	/// 测试字典索引器访问
	/// </summary>
	[TestMethod]
	public void Visit_IndexerReference_Dictionary()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var dict = new Dictionary<string, int>();
                    dict[""key""] = 42;
                    int value = dict[""key""];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let dict = new Map;
  dict.set(""key"", 42);
  let value = _e73dbdff85c46ddc(dict, ""key"");
}", script);
	}

	/// <summary>
	/// 测试字典索引器访问 - 匿名对象键
	/// </summary>
	[TestMethod]
	public void Visit_IndexerReference_Dictionary_AnonymousObjectKey()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var dict = new Dictionary<object, int>();
                    dict[new { Name = ""key"", Index = 1 }] = 42;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  let dict = new Map;
  dict.set({ Name: ""key"", Index: 1 }, 42);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试字典索引器访问 - new 表达式键
	/// </summary>
	[TestMethod]
	public void Visit_IndexerReference_Dictionary_NewExpressionKey()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var dict = new Dictionary<object, int>();
                    dict[new TestClass()] = 42;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let dict = new Map;
  dict.set(new TestClass, 42);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试字典索引器访问 - 可选链键
	/// </summary>
	[TestMethod]
	public void Visit_IndexerReference_Dictionary_ConditionalAccessKey()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                class Person
                {
                    public string Name { get; set; }
                }

                void TestMethod()
                {
                    Person person = null;
                    var dict = new Dictionary<string, int>();
                    dict[person?.Name] = 42;
                    int value = dict[person?.Name];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  let person = null;
  let dict = new Map;
  dict.set(person?.Name, 42);
  let value = _e73dbdff85c46ddc(dict, person?.Name);
}", script);
	}

	/// <summary>
	/// 测试多参数索引器在 JavaScript fallback 下不会静默误编译
	/// </summary>
	[TestMethod]
	public void Visit_IndexerReference_MultiParameterIndexer_Throws()
	{
		var block = GetBlockOperation(@"
            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            class Matrix
            {
                public int this[int row, int column]
                {
                    get => row + column;
                    set { }
                }
            }

            class TestClass
            {
                void TestMethod(Matrix matrix)
                {
                    int value = matrix[1, 2];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var exception = Assert.Throws<OperationTransformationException>(() =>
		{
			_ = walker.Visit(block, new());
		});
		Assert.AreEqual(OperationKind.PropertyReference, exception.Kind);
		StringAssert.Contains(exception.Message, "single translated index argument");
	}

	/// <summary>
	/// 测试多参数索引器赋值在 JavaScript fallback 下会明确拒绝
	/// </summary>
	[TestMethod]
	public void Visit_IndexerAssignment_MultiParameterIndexer_Throws()
	{
		var block = GetBlockOperation(@"
            namespace ECMAScript
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptAttribute : Attribute
                {
                }
            }

            [ECMAScript.ECMAScript]
            class Matrix
            {
                public int this[int row, int column]
                {
                    get => row + column;
                    set { }
                }
            }

            class TestClass
            {
                void TestMethod(Matrix matrix)
                {
                    matrix[1, 2] = 7;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var exception = Assert.Throws<OperationTransformationException>(() =>
		{
			_ = walker.Visit(block, new());
		});
		Assert.AreEqual(OperationKind.PropertyReference, exception.Kind);
		StringAssert.Contains(exception.Message, "single translated index argument");
	}

	/// <summary>
	/// 测试白名单运行时类型上未进入支持表的属性 getter 不会静默回退成普通 JS 访问
	/// </summary>
	[TestMethod]
	public void Visit_Reference_UnmappedRuntimePropertyGetter_Throws()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new List<int>();
                    int capacity = list.Capacity;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		Assert.Throws<OperationTransformationException>(() =>
		{
			_ = walker.Visit(block, new());
		});
	}

	/// <summary>
	/// 测试白名单运行时类型上未进入支持表的属性 setter 不会静默回退成普通 JS 赋值
	/// </summary>
	[TestMethod]
	public void Visit_SimpleAssignment_UnmappedRuntimePropertySetter_Throws()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new List<int>();
                    list.Capacity = 8;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		Assert.Throws<OperationTransformationException>(() =>
		{
			_ = walker.Visit(block, new());
		});
	}

	/// <summary>
	/// 测试白名单运行时类型上未进入支持表的方法不会静默回退成普通 JS 调用
	/// </summary>
	[TestMethod]
	public void Visit_Invocation_UnmappedRuntimeMethod_Throws()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new List<int>();
                    var enumerator = list.GetEnumerator();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		Assert.Throws<OperationTransformationException>(() =>
		{
			_ = walker.Visit(block, new());
		});
	}

	[TestMethod]
	public void Visit_Invocation_UnmappedArrayRuntimeMethod_Throws()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] values = [1, 2, 3];
                    var enumerator = values.GetEnumerator();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		Assert.Throws<OperationTransformationException>(() =>
		{
			_ = walker.Visit(block, new());
		});
	}

	[TestMethod]
	public void Visit_Invocation_InterfaceGetEnumerator_Throws()
	{
		var block = GetBlockOperation(@"
            using System.Collections.Generic;
            class TestClass
            {
                void TestMethod()
                {
                    IEnumerable<int> values = new List<int>();
                    var enumerator = values.GetEnumerator();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		Assert.Throws<OperationTransformationException>(() =>
		{
			_ = walker.Visit(block, new());
		});
	}

	[TestMethod]
	public void Visit_Invocation_NonGenericInterfaceGetEnumerator_Throws()
	{
		var block = GetBlockOperation(@"
            using System.Collections;
            using System.Collections.Generic;
            class TestClass
            {
                void TestMethod()
                {
                    IEnumerable values = new List<int>();
                    var enumerator = values.GetEnumerator();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		Assert.Throws<OperationTransformationException>(() =>
		{
			_ = walker.Visit(block, new());
		});
	}

	[TestMethod]
	public void Visit_Invocation_ICollectionContains_UsesArrayIncludesAlias()
	{
		var block = GetBlockOperation(@"
            using System.Collections.Generic;
            class TestClass
            {
                void TestMethod()
                {
                    ICollection<int> values = [1, 2, 3];
                    bool found = values.Contains(2);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let values = [1, 2, 3];
  let found = values.includes(2);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_ICollectionCount_UsesArrayLengthAlias()
	{
		var block = GetBlockOperation(@"
            using System.Collections.Generic;
            class TestClass
            {
                void TestMethod()
                {
                    ICollection<int> values = [1, 2, 3];
                    int count = values.Count;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let values = [1, 2, 3];
  let count = values.length;
}", script);
	}

	[TestMethod]
	public void Visit_Invocation_IListIndexOf_UsesArrayIndexOfAlias()
	{
		var block = GetBlockOperation(@"
            using System.Collections.Generic;
            class TestClass
            {
                void TestMethod()
                {
                    IList<int> values = [10, 20, 30];
                    int index = values.IndexOf(20);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let values = [10, 20, 30];
  let index = values.indexOf(20);
}", script);
	}

	[TestMethod]
	public void Visit_Invocation_ListIndexOfAndLastIndexOf_WithStartIndex_UseImportHelpers()
	{
		var block = GetBlockOperation(@"
            using System.Collections.Generic;
            class TestClass
            {
                void TestMethod()
                {
                    var values = new List<int> { 10, 20, 30, 20 };
                    int first = values.IndexOf(20, 1);
                    int last = values.LastIndexOf(20, 2);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let values = [10, 20, 30, 20];
  let first = _71ee35e0e260eb27(values, 20, 1);
  let last = _279befda6399cda5(values, 20, 2);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_IListIndexerGet_UsesImportHelper()
	{
		var block = GetBlockOperation(@"
            using System.Collections.Generic;
            class TestClass
            {
                void TestMethod()
                {
                    IList<int> values = [10, 20, 30];
                    int item = values[1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let values = [10, 20, 30];
  let item = _8b52bea1dfb9f9ba(values, 1);
}", script);
	}

	[TestMethod]
	public void Visit_Invocation_IDictionaryContainsKey_UsesMapHasAlias()
	{
		var block = GetBlockOperation(@"
            using System.Collections.Generic;
            class TestClass
            {
                void TestMethod()
                {
                    IDictionary<string, int> dict = new Dictionary<string, int>();
                    bool found = dict.ContainsKey(""key"");
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let dict = new Map;
  let found = dict.has(""key"");
}", script);
	}

	[TestMethod]
	public void Visit_Invocation_IDictionaryContainsKey_WithRecordKey_Throws()
	{
		var block = GetBlockOperation(@"
            using System.Collections.Generic;

            record Key(int Id);

            class TestClass
            {
                void TestMethod(IDictionary<Key, int> dict, Key key)
                {
                    bool found = dict.ContainsKey(key);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var exception = Assert.Throws<OperationTransformationException>(() =>
		{
			_ = walker.Visit(block, new());
		});

		StringAssert.Contains(exception.Message, "JS-stable default equality");
		StringAssert.Contains(exception.Message, "System.Collections.Generic.IDictionary<Key, int>");
		StringAssert.Contains(exception.Message, "Key");
	}

	[TestMethod]
	public void Visit_Invocation_ISetContains_WithRecordElement_Throws()
	{
		var block = GetBlockOperation(@"
            using System.Collections.Generic;

            record Key(int Id);

            class TestClass
            {
                void TestMethod(ISet<Key> set, Key key)
                {
                    bool found = set.Contains(key);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var exception = Assert.Throws<OperationTransformationException>(() =>
		{
			_ = walker.Visit(block, new());
		});

		StringAssert.Contains(exception.Message, "JS-stable default equality");
		StringAssert.Contains(exception.Message, "System.Collections.Generic.ISet<Key>");
		StringAssert.Contains(exception.Message, "Key");
	}

	[TestMethod]
	public void Visit_Invocation_IDictionaryContainsKey_WithPlainReferenceIdentityKey_Allows()
	{
		var block = GetBlockOperation(@"
            using System.Collections.Generic;

            class Key
            {
                public int Id { get; set; }
            }

            class TestClass
            {
                void TestMethod(IDictionary<Key, int> dict, Key key)
                {
                    bool found = dict.ContainsKey(key);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let found = dict.has(key);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_IDictionaryIndexerGetSet_UsesSetAndGetMappings()
	{
		var block = GetBlockOperation(@"
            using System.Collections.Generic;
            class TestClass
            {
                void TestMethod()
                {
                    IDictionary<string, int> dict = new Dictionary<string, int>();
                    dict[""key""] = 42;
                    int value = dict[""key""];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let dict = new Map;
  _f3b177bfce76ed5c(dict, ""key"", 42);
  let value = _371fad9265e864a1(dict, ""key"");
}", script);
	}

	/// <summary>
	/// 索引器 getter 的 key 实参必须只求值一次，不能重复执行副作用。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_IDictionaryIndexerGet_SideEffectingKey_EvaluatedOnce()
	{
		var block = GetBlockOperation(@"
            using System.Collections.Generic;
            class TestClass
            {
                int _keyCalls;

                string NextKey()
                {
                    _keyCalls++;
                    return ""key"";
                }

                void TestMethod()
                {
                    IDictionary<string, int> dict = new Dictionary<string, int>();
                    dict[""key""] = 42;
                    int value = dict[NextKey()];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  let dict = new Map;
  _f3b177bfce76ed5c(dict, ""key"", 42);
  let value = _371fad9265e864a1(dict, this.NextKey());
}", script);
	}

	/// <summary>
	/// 索引器 getter 的 receiver 实参必须只求值一次，不能重复执行副作用。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_DictionaryIndexerGet_SideEffectingReceiver_EvaluatedOnce()
	{
		var block = GetBlockOperation(@"
            using System.Collections.Generic;
            class TestClass
            {
                int _dictCalls;

                Dictionary<string, int> Pick(Dictionary<string, int> dict)
                {
                    _dictCalls++;
                    return dict;
                }

                void TestMethod()
                {
                    var dict = new Dictionary<string, int>();
                    dict[""key""] = 42;
                    int value = Pick(dict)[""key""];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  let dict = new Map;
  dict.set(""key"", 42);
  let value = _e73dbdff85c46ddc(this.Pick(dict), ""key"");
}", script);
	}

	/// <summary>
	/// 测试未标记且不在白名单的外部静态属性不会静默回退成普通 JS 访问
	/// </summary>
	[TestMethod]
	public void Visit_Reference_UnsupportedExternalStaticProperty_Throws()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var shared = Random.Shared;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		Assert.Throws<OperationTransformationException>(() =>
		{
			_ = walker.Visit(block, new());
		});
	}

	[TestMethod]
	public void Visit_Reference_WhitelistPropertyOnErasedUnsupportedGenericHost_Allows()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(List<Random> list)
                {
                    var count = list.Count;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

	Assert.AreEqual(@"{
  let count = list.length;
}", script);
	}

	/// <summary>
	/// 测试未标记且不在白名单的外部静态方法不会静默回退成普通 JS 调用
	/// </summary>
	[TestMethod]
	public void Visit_Invocation_UnsupportedExternalStaticMethod_Throws()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var total = GC.GetTotalMemory(false);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		Assert.Throws<OperationTransformationException>(() =>
		{
			_ = walker.Visit(block, new());
		});
	}

	[TestMethod]
	public void Visit_Invocation_FakeSupportMarkerOnExternalHost_Throws()
	{
		var block = GetBlockOperation(@"
            using System;

            class TestClass
            {
                void TestMethod()
                {
                    var value = Fake.Helper.DoWork();
                }
            }

            namespace Fake
            {
                [AttributeUsage(AttributeTargets.Class, Inherited = false)]
                public sealed class ECMAScriptModuleAttribute : Attribute
                {
                }

                [ECMAScriptModule]
                public static class Helper
                {
                    public static int DoWork() => 1;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		Assert.Throws<OperationTransformationException>(() =>
		{
			_ = walker.Visit(block, new());
		});
	}

	[TestMethod]
	public void Visit_Invocation_WhitelistStaticGenericMethodWithErasedUnsupportedTypeArgument_Allows()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var items = Array.Empty<Random>();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

	Assert.AreEqual(@"{
  let items = [];
}", script);
	}

	[TestMethod]
	public void Visit_Invocation_VoidMethodWithMultipleRefParameters_UsesDistinctPackedReturnIndexes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 1;
                    int b = 2;
                    Update(ref a, ref b);
                }

                void Update(ref int a, ref int b)
                {
                    a++;
                    b += 10;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

	AssertJsNamingScriptEqual(@"{
  let v$0;
  let a = 1;
  let b = 2;
  v$0 = this.Update(a, b), a = v$0[0], b = v$0[1];
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Invocation_NonVoidMethodWithMultipleRefParameters_UsesReturnThenDistinctRefIndexes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int a = 1;
                    int b = 2;
                    int result = Update(ref a, ref b);
                }

                int Update(ref int a, ref int b)
                {
                    a++;
                    b += 10;
                    return a + b;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  let v$0;
  let a = 1;
  let b = 2;
  let result = (v$0 = this.Update(a, b), a = v$0[1], b = v$0[2], v$0[0]);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试只读字典索引器 getter 必须保留运行时 helper 语义
	/// </summary>
	[TestMethod]
	public void Visit_IndexerReference_ReadOnlyDictionary_UsesRuntimeHelper()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var source = new Dictionary<string, int>();
                    source[""key""] = 42;
                    var dict = new System.Collections.ObjectModel.ReadOnlyDictionary<string, int>(source);
                    int value = dict[""key""];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let source = new Map;
  source.set(""key"", 42);
  let dict = _b22e987e1be225aa(source);
  let value = _ed4a7913b74bfd87(dict, ""key"");
}", script);
	}

	/// <summary>
	/// 只读字典经 IDictionary 写入口时必须走 helper 并保留只读约束。
	/// </summary>
	[TestMethod]
	public void Visit_IndexerReference_ReadOnlyDictionary_AsIDictionarySet_UsesReadOnlyGuardedHelper()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var source = new Dictionary<string, int>();
                    source[""key""] = 1;
                    var readOnly = new System.Collections.ObjectModel.ReadOnlyDictionary<string, int>(source);
                    System.Collections.Generic.IDictionary<string, int> dict = readOnly;
                    dict[""key""] = 2;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let source = new Map;
  source.set(""key"", 1);
  let readOnly = _b22e987e1be225aa(source);
  let dict = readOnly;
  _f3b177bfce76ed5c(dict, ""key"", 2);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_ReadOnlySet_ConstructionAndEmpty_UseRuntimeHelpers()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var source = new System.Collections.Generic.HashSet<int>();
                    source.Add(1);
                    var readOnly = new System.Collections.ObjectModel.ReadOnlySet<int>(source);
                    var empty = System.Collections.ObjectModel.ReadOnlySet<int>.Empty;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let source = new Set;
  _e1d2ba750a2788cb(source, 1);
  let readOnly = _aede400efbd05842(source);
  let empty = _843cd8664672a9f8();
}", script);
	}

	[TestMethod]
	public void Visit_Reference_ReadOnlySet_AsISetAdd_UsesGuardedISetHelper()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var source = new System.Collections.Generic.HashSet<int>();
                    source.Add(1);
                    var readOnly = new System.Collections.ObjectModel.ReadOnlySet<int>(source);
                    System.Collections.Generic.ISet<int> set = readOnly;
                    bool added = set.Add(2);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let source = new Set;
  _e1d2ba750a2788cb(source, 1);
  let readOnly = _aede400efbd05842(source);
  let set = readOnly;
  let added = _fa512a510bd763de(set, 2);
}", script);
	}

	/// <summary>
	/// 测试只读集合索引器 getter 必须保留运行时 helper 语义
	/// </summary>
	[TestMethod]
	public void Visit_IndexerReference_ReadOnlyCollection_UsesRuntimeHelper()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var source = new List<int> { 1, 2, 3 };
                    var list = new System.Collections.ObjectModel.ReadOnlyCollection<int>(source);
                    int value = list[1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let source = [1, 2, 3];
  let list = _d4e5f6a7b8c9d0e1(source);
  let value = _b8c9d0e1f2a3b4c5(list, 1);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_ReadOnlyCollection_Empty_UsesRuntimeHelper()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var empty = System.Collections.ObjectModel.ReadOnlyCollection<int>.Empty;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let empty = _e5f6a7b8c9d0e1f2();
}", script);
	}

	[TestMethod]
	public void Visit_Invocation_ListGetRange_UsesRuntimeHelper()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var values = new List<int> { 1, 2, 3 };
                    var segment = values.GetRange(1, 2);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let values = [1, 2, 3];
  let segment = _c35c9c99a23ff96a(values, 1, 2);
}", script);
	}

	/// <summary>
	/// 测试可选链 + 列表索引器 getter 不能丢掉短路语义
	/// </summary>
	[TestMethod]
	public void Visit_IndexerReference_List_ConditionalAccessReceiver_UsesNullishShortCircuit()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    List<int> list = null;
                    int? value = list?[0];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0;
  let list = null;
  let value = (v$0 = list, v$0 == null ? undefined : _d389c31d59037b42(v$0, 0));
}", script);
	}

	[TestMethod]
	public void Visit_ImplicitIndexerReference_ListFromEnd_ConditionalAccessReceiver_UsesNullishShortCircuit()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    List<int> list = null;
                    int? value = list?[^1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0;
  let list = null;
  let value = (v$0 = list, v$0 == null ? undefined : _d389c31d59037b42(v$0, v$0.length - 1));
}", script);
	}

	[TestMethod]
	public void Visit_ArrayElementReference_ConditionalAccessReceiver_UsesNullishShortCircuit()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] values = null;
                    int? first = values?[0];
                    int? last = values?[^1];
                    int[] middle = values?[1..^1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0, v$1, v$2;
  let values = null;
  let first = (v$0 = values, v$0 == null ? undefined : v$0[0]);
  let last = (v$1 = values, v$1 == null ? undefined : v$1[v$1.length - 1]);
  let middle = (v$2 = values, v$2 == null ? undefined : v$2.slice(1, v$2.length - 1));
}", script);
	}

	/// <summary>
	/// 测试可选链 + 字典索引器 getter 不能丢掉短路语义
	/// </summary>
	[TestMethod]
	public void Visit_IndexerReference_Dictionary_ConditionalAccessReceiver_UsesNullishShortCircuit()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Dictionary<string, int> dict = null;
                    int? value = dict?[""key""];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0;
  let dict = null;
  let value = (v$0 = dict, v$0 == null ? undefined : _e73dbdff85c46ddc(v$0, ""key""));
}", script);
	}

	/// <summary>
	/// 测试可选链 + helper 方法调用保留短路语义
	/// </summary>
	[TestMethod]
	public void Visit_Invocation_ListRemove_ConditionalAccessReceiver_UsesNullishShortCircuit()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    List<int> list = null;
                    bool? removed = list?.Remove(1);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0;
  let list = null;
  let removed = (v$0 = list, v$0 == null ? undefined : _562f832fd220e768(v$0, 1));
}", script);
	}

	/// <summary>
	/// 测试列表索引器复合赋值会保留 getter/setter helper 语义
	/// </summary>
	[TestMethod]
	public void Visit_CompoundAssignment_ListIndexer_UsesGetterAndSetterHelpers()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new List<int> { 1, 2, 3 };
                    list[0] += 5;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0;
  let list = [1, 2, 3];
  v$0 = _d389c31d59037b42(list, 0) + 5, _c16a7960302ea054(list, 0, v$0), v$0;
}", script);
	}

	[TestMethod]
	public void Visit_SimpleAssignment_ListImplicitIndexer_FromEnd_UsesSingleReceiverEvaluation()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new List<int> { 1, 2, 3 };
                    Func<List<int>> next = () => list;
                    next()[^1] = 4;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0;
  let list = [1, 2, 3];
  let next = () => {
    return list;
  };
  v$0 = next(), _c16a7960302ea054(v$0, v$0.length - 1, 4);
}", script);
	}

	[TestMethod]
	public void Visit_CompoundAssignment_ListImplicitIndexer_FromEnd_UsesGetterAndSetterHelpers()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new List<int> { 1, 2, 3 };
                    Func<List<int>> next = () => list;
                    next()[^1] += 5;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0, v$1, v$2;
  let list = [1, 2, 3];
  let next = () => {
    return list;
  };
  v$0 = next(), v$1 = v$0.length - 1, v$2 = _d389c31d59037b42(v$0, v$1) + 5, _c16a7960302ea054(v$0, v$1, v$2), v$2;
}", script);
	}

	/// <summary>
	/// 测试列表索引器后缀递增会保留 getter/setter helper 语义和返回旧值
	/// </summary>
	[TestMethod]
	public void Visit_IncrementOrDecrement_ListIndexer_Postfix_UsesGetterAndSetterHelpers()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new List<int> { 1, 2, 3 };
                    int before = list[0]++;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0;
  let list = [1, 2, 3];
  let before = (v$0 = _d389c31d59037b42(list, 0), _c16a7960302ea054(list, 0, v$0 + 1), v$0);
}", script);
	}

	[TestMethod]
	public void Visit_IncrementOrDecrement_ListImplicitIndexer_FromEnd_Postfix_UsesGetterAndSetterHelpers()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new List<int> { 1, 2, 3 };
                    Func<List<int>> next = () => list;
                    int before = next()[^1]++;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0, v$1, v$2;
  let list = [1, 2, 3];
  let next = () => {
    return list;
  };
  let before = (v$0 = next(), v$1 = v$0.length - 1, v$2 = _d389c31d59037b42(v$0, v$1), _c16a7960302ea054(v$0, v$1, v$2 + 1), v$2);
}", script);
	}

	/// <summary>
	/// 测试字典索引器复合赋值会保留 getter/setter helper 语义
	/// </summary>
	[TestMethod]
	public void Visit_CompoundAssignment_DictionaryIndexer_UsesGetterAndSetterHelpers()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var dict = new Dictionary<string, int>();
                    dict[""key""] = 1;
                    dict[""key""] += 5;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0;
  let dict = new Map;
  dict.set(""key"", 1);
  v$0 = _e73dbdff85c46ddc(dict, ""key"") + 5, dict.set(""key"", v$0), v$0;
}", script);
	}

	/// <summary>
	/// 测试列表索引器空合并赋值会保留 getter/setter helper 语义
	/// </summary>
	[TestMethod]
	public void Visit_CoalesceAssignment_ListIndexer_UsesGetterAndSetterHelpers()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new List<string?> { null };
                    string value = list[0] ??= ""fallback"";
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0;
  let list = [null];
  let value = (v$0 = _d389c31d59037b42(list, 0), v$0 == null ? (v$0 = ""fallback"", _c16a7960302ea054(list, 0, v$0), v$0) : v$0);
}", script);
	}

	[TestMethod]
	public void Visit_CoalesceAssignment_ListImplicitIndexer_FromEnd_UsesGetterAndSetterHelpers()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new List<string?> { ""a"", null };
                    Func<List<string?>> next = () => list;
                    string value = next()[^1] ??= ""fallback"";
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let v$0, v$1, v$2;
  let list = [""a"", null];
  let next = () => {
    return list;
  };
  let value = (v$0 = next(), v$1 = v$0.length - 1, v$2 = _d389c31d59037b42(v$0, v$1), v$2 == null ? (v$2 = ""fallback"", _c16a7960302ea054(v$0, v$1, v$2), v$2) : v$2);
}", script);
	}

	/// <summary>
	/// 测试列表索引器 read-modify-write 只计算一次索引表达式
	/// </summary>
	[TestMethod]
	public void Visit_CompoundAssignment_ListIndexer_EvaluatesIndexExpressionOnce()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int index = 0;
                    Func<int> nextIndex = () => index++;
                    var list = new List<int> { 1, 2, 3 };
                    list[nextIndex()] += 5;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript()?.ReplaceLineEndings("\n");

		AssertScriptEqual(@"{
  let v$0, v$1;
  let index = 0;
  let nextIndex = () => {
    return index++;
  };
  let list = [1, 2, 3];
  v$0 = nextIndex(), v$1 = _d389c31d59037b42(list, v$0) + 5, _c16a7960302ea054(list, v$0, v$1), v$1;
}", script);
	}

	#endregion

	#region 扩展测试用例 - 复杂引用场景

	/// <summary>
	/// 测试链式方法调用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ChainedMethodCalls()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string text = ""  Hello World  "";
                    string result = text.Trim().ToUpper().Substring(0, 5);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let text = ""  Hello World  "";
  let result = text.trim().toUpperCase().substring(0, 0 + 5);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试内联方法调用 - 数组字面量参数
	/// </summary>
	[TestMethod]
	public void Visit_Invocation_ArrayClear_ArrayLiteralArgument()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Array.Clear(new int[] { 1, 2, 3 });
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  [1, 2, 3].length = 0;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试嵌套属性访问
	/// </summary>
	[TestMethod]
	public void Visit_Reference_NestedPropertyAccess()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public InnerClass Inner { get; set; }

                public class InnerClass
                {
                    public int Value { get; set; }
                }

                void TestMethod()
                {
                    TestClass obj = new TestClass();
                    obj.Inner = new InnerClass();
                    int x = obj.Inner.Value;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  let obj = new TestClass;
  obj.Inner = new InnerClass;
  let x = obj.Inner.Value;
}", script);
	}

	/// <summary>
	/// 测试数组元素作为对象属性访问
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ArrayElementProperty()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var array = new[] { ""apple"", ""banana"", ""cherry"" };
                    int len = array[0].Length;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [""apple"", ""banana"", ""cherry""];
  let len = array[0].length;
}", script);
	}

	/// <summary>
	/// 测试数组元素方法调用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ArrayElementMethod()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var array = new[] { ""apple"", ""banana"", ""cherry"" };
                    string upper = array[0].ToUpper();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = [""apple"", ""banana"", ""cherry""];
  let upper = array[0].toUpperCase();
}", script);
	}

	#region 扩展测试用例 - 更多属性引用

	/// <summary>
	/// 测试静态属性引用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_StaticProperty()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var now = System.DateTime.Now;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let now = _ee9dd166a34a2fa5();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_RuntimeStaticProperty_UsesImplicitEcmascriptMemberName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var buffer = new ArrayBuffer(8);
                    Number byteLength = buffer.ByteLength;
                    var sliced = buffer.Slice(0, 4);
                    var view = new DataView(buffer, 1, 2);
                    Number byteOffset = view.ByteOffset;
                    Number bytesPerElement = Uint8Array.BYTES_PER_ELEMENT;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let buffer = new ArrayBuffer(8);
  let byteLength = buffer.byteLength;
  let sliced = buffer.slice(0, 4);
  let view = new DataView(buffer, 1, 2);
  let byteOffset = view.byteOffset;
  let bytesPerElement = Uint8Array.BYTES_PER_ELEMENT;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_RuntimeHostMembers_PreserveExplicitUppercaseJsNames()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Number pi = Math.PI;
                    Number ln10 = Math.LN10;
                    Number nan = Number.NaN;
                    Number utc = Date.UTC(2024, 0, 2, 3, 4, 5, 6);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let pi = Math.PI;
  let ln10 = Math.LN10;
  let nan = Number.NaN;
  let utc = Date.UTC(2024, 0, 2, 3, 4, 5, 6);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_DataViewApis_UseJsShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var buffer = new ArrayBuffer(8);
                    var view = new DataView(buffer, 0, 8);
                    byte first = view.GetUint8(0);
                    view.SetUint8(1, 255);
                    var big = view.GetBigInt64(0, true);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let buffer = new ArrayBuffer(8);
  let view = new DataView(buffer, 0, 8);
  let first = view.getUint8(0);
  view.setUint8(1, 255);
  let big = view.getBigInt64(0, true);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试只读属性
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ReadOnlyProperty()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public int ReadOnly => 42;

                void TestMethod()
                {
                    int value = ReadOnly;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  let value = this.ReadOnly;
}", script);
	}

	/// <summary>
	/// 测试计算属性
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ComputedProperty()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                private int _value = 10;
                public int Doubled => _value * 2;

                void TestMethod()
                {
                    int result = Doubled;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  let result = this.Doubled;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试索引器属性
	/// </summary>
	[TestMethod]
	public void Visit_Reference_IndexerProperty()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new System.Collections.Generic.List<int> { 1, 2, 3 };
                    int first = list[0];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let list = [1, 2, 3];
  let first = _d389c31d59037b42(list, 0);
}", script);
	}

	/// <summary>
	/// 测试字典索引器
	/// </summary>
	[TestMethod]
	public void Visit_Reference_DictionaryIndexer()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var dict = new System.Collections.Generic.Dictionary<string, int>();
                    dict[""key""] = 42;
                    int value = dict[""key""];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let dict = new Map;
  dict.set(""key"", 42);
  let value = _e73dbdff85c46ddc(dict, ""key"");
}", script);
	}

	#endregion

	#region 扩展测试用例 - 更多字段引用

	/// <summary>
	/// 测试静态字段引用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_StaticField()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public static int Counter = 0;

                void TestMethod()
                {
                    int count = Counter;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  let count = TestClass.Counter;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试只读字段
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ReadOnlyField()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public readonly int Value = 42;

                void TestMethod()
                {
                    int v = Value;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  let v = this.Value;
}", script);
	}

	/// <summary>
	/// 测试常量字段
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ConstField()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public const int MaxValue = 100;

                void TestMethod()
                {
                    int max = MaxValue;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let max = 100;
}", script);
	}

	#endregion

	#region 扩展测试用例 - 数组引用

	/// <summary>
	/// 测试二维数组引用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_2DArray()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[,] matrix = new int[3, 3];
                    matrix[0, 0] = 1;
                    int value = matrix[1, 2];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let matrix = new Array(3).fill().map(() => new Array(3));
  matrix[0][0] = 1;
  let value = matrix[1][2];
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试锯齿数组引用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_JaggedArray()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[][] jagged = new int[3][];
                    jagged[0] = new int[] { 1, 2, 3 };
                    int value = jagged[0][1];
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let jagged = new Array(3);
  jagged[0] = [1, 2, 3];
  let value = jagged[0][1];
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试数组 Length 属性
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ArrayLength()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] arr = new int[] { 1, 2, 3, 4, 5 };
                    int len = arr.Length;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let arr = [1, 2, 3, 4, 5];
  let len = arr.length;
}", script);
	}

	/// <summary>
	/// 测试数组 Rank 属性
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ArrayRank()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[,] matrix = new int[3, 4];
                    int rank = matrix.Rank;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let matrix = new Array(3).fill().map(() => new Array(4));
  let rank = 2;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	#endregion

	#region 扩展测试用例 - 方法引用

	/// <summary>
	/// 测试方法组引用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_MethodGroup()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    System.Func<int, int> func = Double;
                    int result = func(5);
                }

                int Double(int x) => x * 2;
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  let func = this.Double.bind(this);
  let result = func(5);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试静态方法引用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_StaticMethod()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int result = Add(1, 2);
                }

                static int Add(int a, int b) => a + b;
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

	AssertJsNamingScriptEqual(@"{
  let result = TestClass.Add(1, 2);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_NestedStaticTypeMembers_UseFlattenedRuntimeTypeName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int field = A.Field;
                    int property = A.Value;
                    int result = A.GetNumbers();
                }

                static class A
                {
                    public static int Field = 1;
                    public static int Value => 2;
                    public static int GetNumbers() => 3;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

	AssertJsNamingScriptEqual(@"{
  let field = A.Field;
  let property = A.Value;
  let result = A.GetNumbers();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_EcmascriptStaticNestedTypeMembers_PreserveFullNestedHostChain()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int result = Host.Middle.Inner.Value;
                }

                public static class Host
                {
                    public static class Middle
                    {
                        public static class Inner
                        {
                            public static int Value = 1;
                        }
                    }
                }
            }
        ", assemblyName: "ECMAScript");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let result = TestClass.Host.Middle.Inner.value;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_RuntimeInstanceMethod_UsesImplicitEcmascriptMemberName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var date = new Date(0);
                    var time = date.GetTime();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let date = new Date(0);
  let time = date.getTime();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_RuntimeStaticMethod_UsesImplicitEcmascriptMemberName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var isInteger = Number.IsInteger(1);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let isInteger = Number.isInteger(1);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_ECMAScriptEnumField_EmitsStringLiteral()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var matcher = Intl.LocaleMatcher.BestFit;
                    var twoDigit = Intl.NumericTwoDigit.TwoDigit;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let matcher = ""best fit"";
  let twoDigit = ""2-digit"";
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_ECMAScriptNumericEnumField_EmitsNumericLiteral()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var fractional = Intl.FractionalSecondDigits.Three;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let fractional = 3;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_IntlOptionRecords_ProjectStringEnumsToStringLiterals()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var displayOptions = new Intl.DisplayNamesOptions(
                        LocaleMatcher: Intl.LocaleMatcher.BestFit,
                        Style: Intl.LongShortNarrow.Narrow,
                        Type: Intl.DisplayNamesType.Language,
                        Fallback: Intl.DisplayNamesFallback.Code,
                        LanguageDisplay: Intl.DisplayNamesLanguageDisplay.Dialect);
                    var dateOptions = new Intl.DateTimeFormatOptions(
                        Year: Intl.NumericTwoDigit.TwoDigit,
                        FractionalSecondDigits: Intl.FractionalSecondDigits.Three,
                        FormatMatcher: Intl.FormatMatcher.BestFit,
                        DateStyle: Intl.DateTimeStyle.Short);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "localeMatcher: \"best fit\"");
		StringAssert.Contains(script, "style: \"narrow\"");
		StringAssert.Contains(script, "type: \"language\"");
		StringAssert.Contains(script, "fallback: \"code\"");
		StringAssert.Contains(script, "languageDisplay: \"dialect\"");
		StringAssert.Contains(script, "year: \"2-digit\"");
		StringAssert.Contains(script, "fractionalSecondDigits: 3");
		StringAssert.Contains(script, "formatMatcher: \"best fit\"");
		StringAssert.Contains(script, "dateStyle: \"short\"");
		Assert.IsFalse(script.Contains("type: 0", StringComparison.Ordinal), script);
		Assert.IsFalse(script.Contains("formatMatcher: 0", StringComparison.Ordinal), script);
		Assert.IsFalse(script.Contains("dateStyle: 0", StringComparison.Ordinal), script);
	}

	[TestMethod]
	public void Visit_Reference_IntlResolvedOptions_CompareUsingStringEnumLiterals()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var resolved = new Intl.DisplayNames(
                        new Intl.DisplayNamesOptions(Type: Intl.DisplayNamesType.Language))
                        .ResolvedOptions();
                    var isLanguage = resolved.Type == Intl.DisplayNamesType.Language;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "new Intl.DisplayNames({");
		StringAssert.Contains(script, "type: \"language\"");
		StringAssert.Contains(script, "let isLanguage = resolved.type === \"language\";");
	}

	[TestMethod]
	public void Visit_Reference_ReflectGet_UsesJsMemberName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                public int Value { get; set; }

                void TestMethod()
                {
                    var value = Reflect.Get(new TestClass(), ""Value"");
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let value = Reflect.get(new TestClass, ""Value"");
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_NumberAndIntlApis_UseAlignedCSharpNames()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Number value = 1;
                    var text = value.ToString();
                    var localized = value.ToLocaleString();
                    var localizedDate = new Date().ToLocaleDateString(new[] { ""en-US"", ""zh-CN"" });
                    var locales = Intl.NumberFormat.SupportedLocalesOf(""en-US"");
                    var options = new Intl.NumberFormat().ResolvedOptions();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

	Assert.AreEqual(@"{
  let value = 1;
  let text = value.toString();
  let localized = value.toLocaleString();
  let localizedDate = (new Date).toLocaleDateString([""en-US"", ""zh-CN""]);
  let locales = Intl.NumberFormat.supportedLocalesOf(""en-US"");
  let options = (new Intl.NumberFormat).resolvedOptions();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_IntlConstructors_UseJsRuntimeHosts()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var collator = new Intl.Collator(""en-US"");
                    var comparer = collator.Compare(""a"", ""b"");
                    var dtf = new Intl.DateTimeFormat(""en-US"");
                    var text = dtf.Format(new Date());
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let collator = new Intl.Collator(""en-US"");
  let comparer = collator.compare(""a"", ""b"");
  let dtf = new Intl.DateTimeFormat(""en-US"");
  let text = dtf.format(new Date);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_IntlNestedRuntimeTypes_PreserveIntlQualification()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(object provider)
                {
                    var usesNumberFormat = provider is Intl.NumberFormat;
                    var numberFormat = new Intl.NumberFormat();
                    var locale = new Intl.Locale(""zh-CN"");
                    var displayNames = new Intl.DisplayNames(new Intl.DisplayNamesOptions(Type: Intl.DisplayNamesType.Language));
                    var dateTimeFormat = new Intl.DateTimeFormat(""en-US"");
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "provider instanceof Intl.NumberFormat");
		StringAssert.Contains(script, "let numberFormat = new Intl.NumberFormat");
		StringAssert.Contains(script, "let locale = new Intl.Locale(\"zh-CN\")");
		StringAssert.Contains(script, "let displayNames = new Intl.DisplayNames({");
		StringAssert.Contains(script, "let dateTimeFormat = new Intl.DateTimeFormat(\"en-US\")");
		Assert.IsFalse(script.Contains("instanceof NumberFormat", StringComparison.Ordinal), script);
		Assert.IsFalse(script.Contains("new NumberFormat", StringComparison.Ordinal), script);
		Assert.IsFalse(script.Contains("new Locale", StringComparison.Ordinal), script);
		Assert.IsFalse(script.Contains("new DisplayNames", StringComparison.Ordinal), script);
		Assert.IsFalse(script.Contains("new DateTimeFormat", StringComparison.Ordinal), script);
	}

	[TestMethod]
	public void Visit_Reference_PropertyDescriptor_LowersToObjectLiteral()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Object.DefineProperty(this, Symbol.ToPrimitive, new ECMAScript.PropertyDescriptor
                    {
                        Value = (global::System.Func<string?, object>)ToPrimitive,
                        Configurable = true
                    });
                }

                object ToPrimitive(string? hint)
                    => """";
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "Object.defineProperty(this, Symbol.toPrimitive, {");
		StringAssert.Contains(script, "value: this.toPrimitive.bind(this),");
		StringAssert.Contains(script, "configurable: true");
		Assert.IsFalse(script.Contains("new PropertyDescriptor", StringComparison.Ordinal), script);
	}

	[TestMethod]
	public void Visit_Reference_RegExpProperties_UseJsMemberNames()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var regex = new RegExp(""a"", ""dguy"");
                    var flags = regex.Flags;
                    var sticky = regex.Sticky;
                    var unicode = regex.Unicode;
                    var dotAll = regex.DotAll;
                    var hasIndices = regex.HasIndices;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let regex = new RegExp(""a"", ""dguy"");
  let flags = regex.flags;
  let sticky = regex.sticky;
  let unicode = regex.unicode;
  let dotAll = regex.dotAll;
  let hasIndices = regex.hasIndices;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_RegExpExecResult_UsesJsResultShape()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var regex = new RegExp(""(?<word>a)"", ""g"");
                    var result = regex.Exec(""ab"")!;
                    var input = result.Input;
                    var index = result.Index;
                    var groups = result.Groups;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let regex = new RegExp(""(?<word>a)"", ""g"");
  let result = regex.exec(""ab"");
  let input = result.input;
  let index = result.index;
  let groups = result.groups;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_WeakCollections_RequireObjectTargets()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                class Box
                {
                }

                void TestMethod()
                {
                    var key = new Box();
                    var map = new WeakMap<Box, int>();
                    map.Set(key, 1);

                    var weak = new WeakRef<Box>(key);
                    var target = weak.Deref();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let key = new Box;
  let map = new WeakMap;
  map.set(key, 1);
  let weak = new WeakRef(key);
  let target = weak.deref();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_GlobalTypeOf_UsesCompileHook()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(object value)
                {
                    var kind = TypeOf(value);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let kind = typeof value;
}", script);
	}

	[TestMethod]
	public void Visit_Reference_GlobalRegExp_UsesOrdinaryStaticCall()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var regex = RegExp(""a"");
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let regex = RegExp(""a"");
}", script);
	}

	[TestMethod]
	public void Visit_Reference_BooleanGetTypeCode_UsesInlineConstant()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    bool flag = true;
                    var code = flag.GetTypeCode();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let flag = true;
  let code = 3;
}", script);
	}

	[TestMethod]
	public void Visit_Reference_EqualityComparerDefaultEquals_UsesClrModuleMapping()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(int left, int right)
                {
                    var same = System.Collections.Generic.EqualityComparer<int>.Default.Equals(left, right);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let same = _4614e5ce6b42a7ad(globalThis.__jazorEqualityComparerDefault ??= {}, left, right);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_IEqualityComparerEquals_UsesInterfaceMapping()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(int left, int right)
                {
                    System.Collections.Generic.IEqualityComparer<int> comparer = System.Collections.Generic.EqualityComparer<int>.Default;
                    var same = comparer.Equals(left, right);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let comparer = globalThis.__jazorEqualityComparerDefault ??= {};
  let same = _dae184550b995be1(comparer, left, right);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_EqualityComparerDefaultGetHashCode_UsesClrModuleMapping()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(int value)
                {
                    var hash = System.Collections.Generic.EqualityComparer<int>.Default.GetHashCode(value);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let hash = _2c3736bd7d205921(globalThis.__jazorEqualityComparerDefault ??= {}, value);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_IEqualityComparerGetHashCode_UsesInterfaceMapping()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(int value)
                {
                    System.Collections.Generic.IEqualityComparer<int> comparer = System.Collections.Generic.EqualityComparer<int>.Default;
                    var hash = comparer.GetHashCode(value);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let comparer = globalThis.__jazorEqualityComparerDefault ??= {};
  let hash = _f53ff8f6435182d7(comparer, value);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_IEqualityComparerNonGenericEquals_UsesInterfaceMapping()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(int left, int right)
                {
                    System.Collections.IEqualityComparer comparer = System.Collections.Generic.EqualityComparer<int>.Default;
                    var same = comparer.Equals(left, right);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let comparer = globalThis.__jazorEqualityComparerDefault ??= {};
  let same = _eb0a1792ad8b44b7(comparer, left, right);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_IEqualityComparerNonGenericGetHashCode_UsesInterfaceMapping()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(int value)
                {
                    System.Collections.IEqualityComparer comparer = System.Collections.Generic.EqualityComparer<int>.Default;
                    var hash = comparer.GetHashCode(value);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let comparer = globalThis.__jazorEqualityComparerDefault ??= {};
  let hash = _8f16da840d40722e(comparer, value);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_ComparerDefaultCompare_UsesClrModuleMapping()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(int left, int right)
                {
                    var order = System.Collections.Generic.Comparer<int>.Default.Compare(left, right);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let order = _a4222c99b516b861(globalThis.__jazorComparerDefault ??= {}, left, right);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_IComparerCompare_UsesInterfaceMapping()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(int left, int right)
                {
                    System.Collections.Generic.IComparer<int> comparer = System.Collections.Generic.Comparer<int>.Default;
                    var order = comparer.Compare(left, right);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let comparer = globalThis.__jazorComparerDefault ??= {};
  let order = _0289dcf579b8a65e(comparer, left, right);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_IComparerNonGenericCompare_UsesInterfaceMapping()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(object left, object right)
                {
                    System.Collections.IComparer comparer = System.Collections.Generic.Comparer<int>.Default;
                    var order = comparer.Compare(left, right);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let comparer = globalThis.__jazorComparerDefault ??= {};
  let order = _7dffdd7244581cc5(comparer, left, right);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_StringCompareToObject_UsesClrModuleMapping()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(string left, object right)
                {
                    var order = left.CompareTo(right);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let order = _629b0613344d82e7(left, right);
}", script);
	}

	[TestMethod]
	public void Visit_Reference_StringCompareToString_UsesClrModuleMapping()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(string left, string right)
                {
                    var order = left.CompareTo(right);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let order = _380e7c7649d703f0(left, right);
}", script);
	}

	[TestMethod]
	public void WhiteList_BooleanGetTypeCode_UsesInlineRule()
	{
		var membersField = typeof(SemanticWalker).Assembly
			.GetType("Jazor.Compiler.WhiteList", throwOnError: true)!
			.GetField("Members", BindingFlags.Public | BindingFlags.Static);
		var members = (IDictionary?)membersField?.GetValue(null);

		Assert.IsNotNull(members);
		Assert.IsTrue(members.Contains("bool.GetTypeCode()"));

		var value = members["bool.GetTypeCode()"];
		Assert.IsNotNull(value);

		var valueType = value.GetType();
		var op = valueType.GetProperty("Op", BindingFlags.Instance | BindingFlags.Public)?.GetValue(value)?.ToString();
		var template = (string?)valueType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)?.GetValue(value);

		Assert.AreEqual("Inline", op);
		Assert.AreEqual("3", template);
	}

	[TestMethod]
	public void Visit_Reference_PrimitiveGetTypeCode_UseInlineConstants()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod(byte b, char c, short s, int i, long l, sbyte sb, float f, double d, decimal m, ushort us, uint ui, ulong ul, string str, System.DateTime dt)
                {
                    var byteCode = b.GetTypeCode();
                    var charCode = c.GetTypeCode();
                    var shortCode = s.GetTypeCode();
                    var intCode = i.GetTypeCode();
                    var longCode = l.GetTypeCode();
                    var sbyteCode = sb.GetTypeCode();
                    var floatCode = f.GetTypeCode();
                    var doubleCode = d.GetTypeCode();
                    var decimalCode = m.GetTypeCode();
                    var ushortCode = us.GetTypeCode();
                    var uintCode = ui.GetTypeCode();
                    var ulongCode = ul.GetTypeCode();
                    var stringCode = str.GetTypeCode();
                    var dateTimeCode = dt.GetTypeCode();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let byteCode = 6;
  let charCode = 4;
  let shortCode = 7;
  let intCode = 9;
  let longCode = 11;
  let sbyteCode = 5;
  let floatCode = 13;
  let doubleCode = 14;
  let decimalCode = 15;
  let ushortCode = 8;
  let uintCode = 10;
  let ulongCode = 12;
  let stringCode = 18;
  let dateTimeCode = 16;
}", script);
	}

	[TestMethod]
	public void WhiteList_PrimitiveGetTypeCode_UseInlineConstants()
	{
		var expected = new Dictionary<string, string>
		{
			["byte.GetTypeCode()"] = "6",
			["char.GetTypeCode()"] = "4",
			["short.GetTypeCode()"] = "7",
			["int.GetTypeCode()"] = "9",
			["long.GetTypeCode()"] = "11",
			["sbyte.GetTypeCode()"] = "5",
			["float.GetTypeCode()"] = "13",
			["double.GetTypeCode()"] = "14",
			["decimal.GetTypeCode()"] = "15",
			["ushort.GetTypeCode()"] = "8",
			["uint.GetTypeCode()"] = "10",
			["ulong.GetTypeCode()"] = "12",
			["string.GetTypeCode()"] = "18",
			["System.DateTime.GetTypeCode()"] = "16",
		};

		var membersField = typeof(SemanticWalker).Assembly
			.GetType("Jazor.Compiler.WhiteList", throwOnError: true)!
			.GetField("Members", BindingFlags.Public | BindingFlags.Static);
		var members = (IDictionary?)membersField?.GetValue(null);

		Assert.IsNotNull(members);

		foreach (var pair in expected)
		{
			Assert.IsTrue(members.Contains(pair.Key), $"missing whitelist entry: {pair.Key}");

			var value = members[pair.Key];
			Assert.IsNotNull(value, $"missing whitelist value: {pair.Key}");

			var valueType = value.GetType();
			var op = valueType.GetProperty("Op", BindingFlags.Instance | BindingFlags.Public)?.GetValue(value)?.ToString();
			var template = (string?)valueType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)?.GetValue(value);

			Assert.AreEqual("Inline", op, $"unexpected op for {pair.Key}");
			Assert.AreEqual(pair.Value, template, $"unexpected inline template for {pair.Key}");
		}
	}

	[TestMethod]
	public void WhiteList_EqualityComparerDefaultAndEquals_AreMapped()
	{
		var membersField = typeof(SemanticWalker).Assembly
			.GetType("Jazor.Compiler.WhiteList", throwOnError: true)!
			.GetField("Members", BindingFlags.Public | BindingFlags.Static);
		var members = (IDictionary?)membersField?.GetValue(null);

		Assert.IsNotNull(members);
		Assert.IsTrue(members.Contains("static System.Collections.Generic.EqualityComparer<T>.Default.get"));
		Assert.IsTrue(members.Contains("virtual System.Collections.Generic.EqualityComparer<T>.Equals(T, T)"));
		Assert.IsTrue(members.Contains("virtual System.Collections.Generic.EqualityComparer<T>.GetHashCode(T)"));

		var defaultValue = members["static System.Collections.Generic.EqualityComparer<T>.Default.get"];
		Assert.IsNotNull(defaultValue);
		var defaultValueType = defaultValue.GetType();
		var defaultOp = defaultValueType.GetProperty("Op", BindingFlags.Instance | BindingFlags.Public)?.GetValue(defaultValue)?.ToString();
		var defaultTemplate = (string?)defaultValueType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)?.GetValue(defaultValue);
		Assert.AreEqual("Inline", defaultOp);
		Assert.AreEqual("(globalThis.__jazorEqualityComparerDefault ??= {})", defaultTemplate);

		var equalsValue = members["virtual System.Collections.Generic.EqualityComparer<T>.Equals(T, T)"];
		Assert.IsNotNull(equalsValue);
		var equalsValueType = equalsValue.GetType();
		var equalsOp = equalsValueType.GetProperty("Op", BindingFlags.Instance | BindingFlags.Public)?.GetValue(equalsValue)?.ToString();
		var equalsMethod = (string?)equalsValueType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)?.GetValue(equalsValue);
		var equalsPath = (string?)equalsValueType.GetProperty("Path", BindingFlags.Instance | BindingFlags.Public)?.GetValue(equalsValue);
		Assert.AreEqual("Import", equalsOp);
		Assert.AreEqual("_4614e5ce6b42a7ad", equalsMethod);
		Assert.AreEqual("System/Collections/Generic/EqualityComparerT1Module.js", equalsPath);

		var getHashCodeValue = members["virtual System.Collections.Generic.EqualityComparer<T>.GetHashCode(T)"];
		Assert.IsNotNull(getHashCodeValue);
		var getHashCodeValueType = getHashCodeValue.GetType();
		var getHashCodeOp = getHashCodeValueType.GetProperty("Op", BindingFlags.Instance | BindingFlags.Public)?.GetValue(getHashCodeValue)?.ToString();
		var getHashCodeMethod = (string?)getHashCodeValueType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)?.GetValue(getHashCodeValue);
		var getHashCodePath = (string?)getHashCodeValueType.GetProperty("Path", BindingFlags.Instance | BindingFlags.Public)?.GetValue(getHashCodeValue);
		Assert.AreEqual("Import", getHashCodeOp);
		Assert.AreEqual("_2c3736bd7d205921", getHashCodeMethod);
		Assert.AreEqual("System/Collections/Generic/EqualityComparerT1Module.js", getHashCodePath);
	}

	[TestMethod]
	public void WhiteList_IEqualityComparerEquals_IsMapped()
	{
		var membersField = typeof(SemanticWalker).Assembly
			.GetType("Jazor.Compiler.WhiteList", throwOnError: true)!
			.GetField("Members", BindingFlags.Public | BindingFlags.Static);
		var members = (IDictionary?)membersField?.GetValue(null);

		Assert.IsNotNull(members);
		Assert.IsTrue(members.Contains("System.Collections.Generic.IEqualityComparer<T>.Equals(T, T)"));

		var equalsValue = members["System.Collections.Generic.IEqualityComparer<T>.Equals(T, T)"];
		Assert.IsNotNull(equalsValue);
		var equalsValueType = equalsValue.GetType();
		var equalsOp = equalsValueType.GetProperty("Op", BindingFlags.Instance | BindingFlags.Public)?.GetValue(equalsValue)?.ToString();
		var equalsMethod = (string?)equalsValueType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)?.GetValue(equalsValue);
		var equalsPath = (string?)equalsValueType.GetProperty("Path", BindingFlags.Instance | BindingFlags.Public)?.GetValue(equalsValue);

		Assert.AreEqual("Import", equalsOp);
		Assert.AreEqual("_dae184550b995be1", equalsMethod);
		Assert.AreEqual("System/Collections/Generic/IEqualityComparerT1Module.js", equalsPath);
	}

	[TestMethod]
	public void WhiteList_IEqualityComparerGetHashCode_IsMapped()
	{
		var membersField = typeof(SemanticWalker).Assembly
			.GetType("Jazor.Compiler.WhiteList", throwOnError: true)!
			.GetField("Members", BindingFlags.Public | BindingFlags.Static);
		var members = (IDictionary?)membersField?.GetValue(null);

		Assert.IsNotNull(members);
		Assert.IsTrue(members.Contains("System.Collections.Generic.IEqualityComparer<T>.GetHashCode(T)"));

		var getHashCodeValue = members["System.Collections.Generic.IEqualityComparer<T>.GetHashCode(T)"];
		Assert.IsNotNull(getHashCodeValue);
		var getHashCodeValueType = getHashCodeValue.GetType();
		var getHashCodeOp = getHashCodeValueType.GetProperty("Op", BindingFlags.Instance | BindingFlags.Public)?.GetValue(getHashCodeValue)?.ToString();
		var getHashCodeMethod = (string?)getHashCodeValueType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)?.GetValue(getHashCodeValue);
		var getHashCodePath = (string?)getHashCodeValueType.GetProperty("Path", BindingFlags.Instance | BindingFlags.Public)?.GetValue(getHashCodeValue);

		Assert.AreEqual("Import", getHashCodeOp);
		Assert.AreEqual("_f53ff8f6435182d7", getHashCodeMethod);
		Assert.AreEqual("System/Collections/Generic/IEqualityComparerT1Module.js", getHashCodePath);
	}

	[TestMethod]
	public void WhiteList_IEqualityComparerNonGeneric_IsMapped()
	{
		var membersField = typeof(SemanticWalker).Assembly
			.GetType("Jazor.Compiler.WhiteList", throwOnError: true)!
			.GetField("Members", BindingFlags.Public | BindingFlags.Static);
		var members = (IDictionary?)membersField?.GetValue(null);

		Assert.IsNotNull(members);
		Assert.IsTrue(members.Contains("System.Collections.IEqualityComparer.Equals(object, object)"));
		Assert.IsTrue(members.Contains("System.Collections.IEqualityComparer.GetHashCode(object)"));

		var equalsValue = members["System.Collections.IEqualityComparer.Equals(object, object)"];
		Assert.IsNotNull(equalsValue);
		var equalsValueType = equalsValue.GetType();
		var equalsOp = equalsValueType.GetProperty("Op", BindingFlags.Instance | BindingFlags.Public)?.GetValue(equalsValue)?.ToString();
		var equalsMethod = (string?)equalsValueType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)?.GetValue(equalsValue);
		var equalsPath = (string?)equalsValueType.GetProperty("Path", BindingFlags.Instance | BindingFlags.Public)?.GetValue(equalsValue);
		Assert.AreEqual("Import", equalsOp);
		Assert.AreEqual("_eb0a1792ad8b44b7", equalsMethod);
		Assert.AreEqual("System/Collections/IEqualityComparerModule.js", equalsPath);

		var getHashCodeValue = members["System.Collections.IEqualityComparer.GetHashCode(object)"];
		Assert.IsNotNull(getHashCodeValue);
		var getHashCodeValueType = getHashCodeValue.GetType();
		var getHashCodeOp = getHashCodeValueType.GetProperty("Op", BindingFlags.Instance | BindingFlags.Public)?.GetValue(getHashCodeValue)?.ToString();
		var getHashCodeMethod = (string?)getHashCodeValueType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)?.GetValue(getHashCodeValue);
		var getHashCodePath = (string?)getHashCodeValueType.GetProperty("Path", BindingFlags.Instance | BindingFlags.Public)?.GetValue(getHashCodeValue);
		Assert.AreEqual("Import", getHashCodeOp);
		Assert.AreEqual("_8f16da840d40722e", getHashCodeMethod);
		Assert.AreEqual("System/Collections/IEqualityComparerModule.js", getHashCodePath);
	}

	[TestMethod]
	public void WhiteList_ComparerDefaultAndCompare_AreMapped()
	{
		var membersField = typeof(SemanticWalker).Assembly
			.GetType("Jazor.Compiler.WhiteList", throwOnError: true)!
			.GetField("Members", BindingFlags.Public | BindingFlags.Static);
		var members = (IDictionary?)membersField?.GetValue(null);

		Assert.IsNotNull(members);
		Assert.IsTrue(members.Contains("static System.Collections.Generic.Comparer<T>.Default.get"));
		Assert.IsTrue(members.Contains("virtual System.Collections.Generic.Comparer<T>.Compare(T, T)"));

		var defaultValue = members["static System.Collections.Generic.Comparer<T>.Default.get"];
		Assert.IsNotNull(defaultValue);
		var defaultValueType = defaultValue.GetType();
		var defaultOp = defaultValueType.GetProperty("Op", BindingFlags.Instance | BindingFlags.Public)?.GetValue(defaultValue)?.ToString();
		var defaultTemplate = (string?)defaultValueType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)?.GetValue(defaultValue);
		Assert.AreEqual("Inline", defaultOp);
		Assert.AreEqual("(globalThis.__jazorComparerDefault ??= {})", defaultTemplate);

		var compareValue = members["virtual System.Collections.Generic.Comparer<T>.Compare(T, T)"];
		Assert.IsNotNull(compareValue);
		var compareValueType = compareValue.GetType();
		var compareOp = compareValueType.GetProperty("Op", BindingFlags.Instance | BindingFlags.Public)?.GetValue(compareValue)?.ToString();
		var compareMethod = (string?)compareValueType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)?.GetValue(compareValue);
		var comparePath = (string?)compareValueType.GetProperty("Path", BindingFlags.Instance | BindingFlags.Public)?.GetValue(compareValue);
		Assert.AreEqual("Import", compareOp);
		Assert.AreEqual("_a4222c99b516b861", compareMethod);
		Assert.AreEqual("System/Collections/Generic/ComparerT1Module.js", comparePath);
	}

	[TestMethod]
	public void WhiteList_IComparerCompare_IsMapped()
	{
		var membersField = typeof(SemanticWalker).Assembly
			.GetType("Jazor.Compiler.WhiteList", throwOnError: true)!
			.GetField("Members", BindingFlags.Public | BindingFlags.Static);
		var members = (IDictionary?)membersField?.GetValue(null);

		Assert.IsNotNull(members);
		Assert.IsTrue(members.Contains("System.Collections.Generic.IComparer<T>.Compare(T, T)"));

		var compareValue = members["System.Collections.Generic.IComparer<T>.Compare(T, T)"];
		Assert.IsNotNull(compareValue);
		var compareValueType = compareValue.GetType();
		var compareOp = compareValueType.GetProperty("Op", BindingFlags.Instance | BindingFlags.Public)?.GetValue(compareValue)?.ToString();
		var compareMethod = (string?)compareValueType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)?.GetValue(compareValue);
		var comparePath = (string?)compareValueType.GetProperty("Path", BindingFlags.Instance | BindingFlags.Public)?.GetValue(compareValue);

		Assert.AreEqual("Import", compareOp);
		Assert.AreEqual("_0289dcf579b8a65e", compareMethod);
		Assert.AreEqual("System/Collections/Generic/IComparerT1Module.js", comparePath);
	}

	[TestMethod]
	public void WhiteList_IComparerNonGenericCompare_IsMapped()
	{
		var membersField = typeof(SemanticWalker).Assembly
			.GetType("Jazor.Compiler.WhiteList", throwOnError: true)!
			.GetField("Members", BindingFlags.Public | BindingFlags.Static);
		var members = (IDictionary?)membersField?.GetValue(null);

		Assert.IsNotNull(members);
		Assert.IsTrue(members.Contains("System.Collections.IComparer.Compare(object, object)"));

		var compareValue = members["System.Collections.IComparer.Compare(object, object)"];
		Assert.IsNotNull(compareValue);
		var compareValueType = compareValue.GetType();
		var compareOp = compareValueType.GetProperty("Op", BindingFlags.Instance | BindingFlags.Public)?.GetValue(compareValue)?.ToString();
		var compareMethod = (string?)compareValueType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)?.GetValue(compareValue);
		var comparePath = (string?)compareValueType.GetProperty("Path", BindingFlags.Instance | BindingFlags.Public)?.GetValue(compareValue);

		Assert.AreEqual("Import", compareOp);
		Assert.AreEqual("_7dffdd7244581cc5", compareMethod);
		Assert.AreEqual("System/Collections/IComparerModule.js", comparePath);
	}

	[TestMethod]
	public void WhiteList_StringCompareTo_IsMapped()
	{
		var membersField = typeof(SemanticWalker).Assembly
			.GetType("Jazor.Compiler.WhiteList", throwOnError: true)!
			.GetField("Members", BindingFlags.Public | BindingFlags.Static);
		var members = (IDictionary?)membersField?.GetValue(null);

		Assert.IsNotNull(members);
		Assert.IsTrue(members.Contains("string.CompareTo(object)"));
		Assert.IsTrue(members.Contains("string.CompareTo(string)"));

		var compareObjectValue = members["string.CompareTo(object)"];
		Assert.IsNotNull(compareObjectValue);
		var compareObjectValueType = compareObjectValue.GetType();
		var compareObjectOp = compareObjectValueType.GetProperty("Op", BindingFlags.Instance | BindingFlags.Public)?.GetValue(compareObjectValue)?.ToString();
		var compareObjectMethod = (string?)compareObjectValueType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)?.GetValue(compareObjectValue);
		var compareObjectPath = (string?)compareObjectValueType.GetProperty("Path", BindingFlags.Instance | BindingFlags.Public)?.GetValue(compareObjectValue);
		Assert.AreEqual("Import", compareObjectOp);
		Assert.AreEqual("_629b0613344d82e7", compareObjectMethod);
		Assert.AreEqual("System/StringModule.js", compareObjectPath);

		var compareStringValue = members["string.CompareTo(string)"];
		Assert.IsNotNull(compareStringValue);
		var compareStringValueType = compareStringValue.GetType();
		var compareStringOp = compareStringValueType.GetProperty("Op", BindingFlags.Instance | BindingFlags.Public)?.GetValue(compareStringValue)?.ToString();
		var compareStringMethod = (string?)compareStringValueType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)?.GetValue(compareStringValue);
		var compareStringPath = (string?)compareStringValueType.GetProperty("Path", BindingFlags.Instance | BindingFlags.Public)?.GetValue(compareStringValue);
		Assert.AreEqual("Import", compareStringOp);
		Assert.AreEqual("_380e7c7649d703f0", compareStringMethod);
		Assert.AreEqual("System/StringModule.js", compareStringPath);
	}

	[TestMethod]
	public void WhiteList_GenericSignatureEquivalence_MatchesDeclaredParametersOnly()
	{
		var whiteListLookupType = typeof(SemanticWalker).Assembly
			.GetType("Jazor.Compiler.WhiteListLookup", throwOnError: true);
		var method = whiteListLookupType?
			.GetMethods(BindingFlags.Public | BindingFlags.Static)
			.SingleOrDefault(static candidate =>
			{
				if (candidate.Name != "TryGetValue" || !candidate.IsGenericMethodDefinition)
					return false;

				var parameters = candidate.GetParameters();
				return parameters.Length == 4 &&
					   parameters[1].ParameterType == typeof(string);
			});

		Assert.IsNotNull(method);

		var genericMethod = method!.MakeGenericMethod(typeof(int));
		var mappings = new Dictionary<string, int>(StringComparer.Ordinal)
		{
			["System.Threading.Tasks.Task<TResult>.WaitAsync(System.Threading.CancellationToken)"] = 1,
			["static bool.Parse(System.ReadOnlySpan<char>)"] = 2,
		};

		var positiveArguments = new object?[]
		{
			mappings,
			"System.Threading.Tasks.Task<T>.WaitAsync(System.Threading.CancellationToken)",
			null,
			0,
		};
		var positiveMatched = (bool)(genericMethod.Invoke(null, positiveArguments) ?? false);
		Assert.IsTrue(positiveMatched);
		Assert.AreEqual(
			"System.Threading.Tasks.Task<TResult>.WaitAsync(System.Threading.CancellationToken)",
			positiveArguments[2]);
		Assert.AreEqual(1, positiveArguments[3]);

		var negativeArguments = new object?[]
		{
			mappings,
			"static bool.Parse(System.ReadOnlySpan<byte>)",
			null,
			0,
		};
		var negativeMatched = (bool)(genericMethod.Invoke(null, negativeArguments) ?? false);
		Assert.IsFalse(negativeMatched);
	}

	[TestMethod]
	public void Compile_GlobalTypeOf_InvalidHandler_Throws()
	{
		var walker = new SemanticWalker(true);

		Assert.Throws<InvalidOperationException>(() =>
			walker.Compile_27d71701fd254382(
				null!,
				SenseArgument.Default,
				new Identifier("host"),
				[new Identifier("value")],
				null));
	}
	
	[TestMethod]
	public void Visit_Reference_ConsoleTimeEnd_UsesJsMemberName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Console.Time(""work"");
                    Console.TimeEnd(""work"");
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  console.time(""work"");
  console.timeEnd(""work"");
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_DateNow_UsesJsMemberName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var now = Date.Now();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let now = Date.now();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_ArrayFromAsync_UsesJsMemberName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var mapped = Array<int>.From(new int[] { 1, 2, 3 }, (value, index) => value + index);
                    var items = Array<int>.FromAsync(new int[] { 1, 2, 3 });
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let mapped = Array.from([1, 2, 3], (value, index) => {
    return value + index;
  });
  let items = Array.fromAsync([1, 2, 3]);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_ArrayIsArray_UsesJsMemberName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var isArray = Array.IsArray(new int[] { 1, 2, 3 });
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let isArray = Array.isArray([1, 2, 3]);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_ArrayVariadicApis_UseJsShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var array = Array<int>.Of(1, 2, 3);
                    var combined = array.Concat(4, 5);
                    Number pushed = array.Push(4, 5);
                    Number unshifted = array.Unshift(0);
                    var removed = array.Splice(2, 1, 8, 9);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = Array.of(1, 2, 3);
  let combined = array.concat(4, 5);
  let pushed = array.push(4, 5);
  let unshifted = array.unshift(0);
  let removed = array.splice(2, 1, 8, 9);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_ArrayResize_UsesRefLowering()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int[] numbers = [1, 2, 3];
                    Array.Resize(ref numbers, 5);
                    var length = numbers.Length;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let v$0;
  let numbers = [1, 2, 3];
  v$0 = _127013d39cf5bff9(numbers, 5), numbers = v$0[0];
  let length = numbers.length;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_ArrayInstanceApis_UseJsShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var array = Array<int>.Of(1, 2, 3);
                    Number length = array.Length;
                    Number last = array.LastIndexOf(2);
                    var sliced = array.Slice(1, 2);
                    var mappedSimple = array.Map(value => value + 1);
                    array.ForEach((value, index) => { });
                    var every = array.Every(value => value > 0);
                    var filtered = array.Filter((value, index, self) => value > 1);
                    var found = array.Find((value, index, self) => value == 2);
                    var index = array.FindIndex(value => value == 3);
                    var filled = array.Fill(0, 1, 2);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let array = Array.of(1, 2, 3);
  let length = array.length;
  let last = array.lastIndexOf(2);
  let sliced = array.slice(1, 2);
  let mappedSimple = array.map(value => {
    return value + 1;
  });
  array.forEach((value, index) => {
    return;
  });
  let every = array.every(value => {
    return value > 0;
  });
  let filtered = array.filter((value, index, self) => {
    return value > 1;
  });
  let found = array.find((value, index, self) => {
    return value === 2;
  });
  let index = array.findIndex(value => {
    return value === 3;
  });
  let filled = array.fill(0, 1, 2);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_TypedArrayStaticApis_UseJsShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var bytes = Uint8Array.Of(1, 2, 3);
                    var mapped = Uint8Array.From(new int[] { 1, 2, 3 }, (value, index) => (byte)(value + index));
                    var signed = BigInt64Array.Of(BigInt.One, BigInt.Two);
                    var unsigned = BigUint64Array.From(new[] { BigInt.One, BigInt.Two });
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let bytes = Uint8Array.of(1, 2, 3);
  let mapped = Uint8Array.from([1, 2, 3], (value, index) => {
    return value + index;
  });
  let signed = BigInt64Array.of(1n, 2n);
  let unsigned = BigUint64Array.from([1n, 2n]);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_TypedArrayInstanceApis_UseJsShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var signed = Int8Array.Of(-1, 2, -1);
                    var bigSigned = BigInt64Array.Of(BigInt.One, BigInt.Two);
                    Number first = signed.IndexOf(-1);
                    Number last = signed.LastIndexOf(-1);
                    Number found = signed.FindIndex((value, index, array) => value < 0);
                    Number bigFound = bigSigned.IndexOf(BigInt.One);
                    var mapped = signed.Map((value, index, array) => value);
                    var sorted = signed.Sort((left, right) => left - right);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let signed = Int8Array.of(-1, 2, -1);
  let bigSigned = BigInt64Array.of(1n, 2n);
  let first = signed.indexOf(-1);
  let last = signed.lastIndexOf(-1);
  let found = signed.findIndex((value, index, array) => {
    return value < 0;
  });
  let bigFound = bigSigned.indexOf(1n);
  let mapped = signed.map((value, index, array) => {
    return value;
  });
  let sorted = signed.sort((left, right) => {
    return left - right;
  });
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_MapAndSetForEach_UseJsCallbackShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var set = new Set<int>();
                    set.Add(1);
                    set.ForEach((value, key, self) => { }, new { tag = 1 });

                    var map = new Map<string, int>();
                    map.Set(""one"", 1);
                    map.ForEach((value, key, self) => { }, new { tag = 2 });
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let set = new Set;
  set.add(1);
  set.forEach((value, key, self) => {
    return;
  }, { tag: 1 });
  let map = new Map;
  map.set(""one"", 1);
  map.forEach((value, key, self) => {
    return;
  }, { tag: 2 });
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_MapAndSetConstructors_UseJsIterableShapes()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Array<object?>[] entries = [[""one"", 1], [""two"", 2]];
                    int[] values = [1, 2, 3];

                    var map = new Map<string, int>(entries);
                    var set = new Set<int>(values);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let entries = [[""one"", 1], [""two"", 2]];
  let values = [1, 2, 3];
  let map = new Map(entries);
  let set = new Set(values);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_GlobalSymbolFactory_UsesJsHostName()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var token = SymbolFn(""value"");
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

	Assert.AreEqual(@"{
  let token = Symbol(""value"");
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_ErrorMembers_UseJsMemberNames()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var error = new Error(""boom"");
                    var message = error.Message;
                    var cause = error.Cause;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let error = new Error(""boom"");
  let message = error.message;
  let cause = error.cause;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_InvalidOperationExceptionMembers_FallbackToBaseErrorMappings()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var error = new InvalidOperationException(""boom"");
                    var message = error.Message;
                    var stack = error.StackTrace;
                    var text = error.ToString();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let error = new Error(""boom"");
  let message = error.message;
  let stack = error.stack;
  let text = error.toString();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_ArgumentNullExceptionMembers_FallbackToBaseErrorMappings()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var error = new ArgumentNullException(""arg"");
                    var message = error.Message;
                    var name = error.ParamName;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let error = new TypeError(""arg"");
  let message = error.message;
  let name = error.message;
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_ErrorOptionsConstructor_PreservesJsShape()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var error = new Error(""boom"", new ErrorOptions
                    {
                        Cause = ""root""
                    });
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let error = new Error(""boom"", { cause: ""root"" });
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	[TestMethod]
	public void Visit_Reference_TopLevelSiblingSourceStaticMethod_Throws()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var value = Helper.DoWork();
                }
            }

            class Helper
            {
                public static int DoWork() => 1;
            }
        ");

		var walker = new SemanticWalker(true);
		Assert.Throws<OperationTransformationException>(() =>
		{
			_ = walker.Visit(block, new());
		});
	}

	/// <summary>
	/// 测试扩展方法引用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ExtensionMethod()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var list = new System.Collections.Generic.List<int> { 1, 2, 3 };
                    var doubled = list.Select(x => x * 2).ToList();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "let list = [1, 2, 3];");
		StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: source is null\");");
		StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: selector is null\");");
		StringAssert.Contains(script, "return __src.map(__callback);");
	}

	[TestMethod]
	public void Visit_Reference_ExtensionMethod_OnCustomIList_UsesDirectArrayFastPath()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod<T>(T source) where T : System.Collections.Generic.IList<int>
                {
                    var filtered = source.Where(x => x > 1).ToList();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: source is null\");");
		StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: predicate is null\");");
		StringAssert.Contains(script, "return __src.filter(__callback);");
	}

	[TestMethod]
	public void Visit_Reference_ExtensionMethod_OnCustomIEnumerable_UsesArrayFromFastPath()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod<T>(T source) where T : System.Collections.Generic.IEnumerable<int>
                {
                    var filtered = source.Where(x => x > 1).ToList();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: source is null\");");
		StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: predicate is null\");");
		StringAssert.Contains(script, "return Array.from(__src).filter(__callback);");
	}

	[TestMethod]
	public void Visit_Reference_ExtensionMethod_OnCustomICollection_UsesArrayFromFastPath()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod<T>(T source) where T : System.Collections.Generic.ICollection<int>
                {
                    var filtered = source.Where(x => x > 1).ToList();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: source is null\");");
		StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: predicate is null\");");
		StringAssert.Contains(script, "return Array.from(__src).filter(__callback);");
	}

	[TestMethod]
	public void Visit_Reference_ExtensionMethod_OnHashSet_UsesArrayFromFastPath()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var source = new System.Collections.Generic.HashSet<int>();
                    var filtered = source.Where(x => x > 1).ToList();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "let source = new Set;");
		StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: source is null\");");
		StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: predicate is null\");");
		StringAssert.Contains(script, "return Array.from(__src).filter(__callback);");
	}

	[TestMethod]
	public void Visit_Reference_ExtensionMethod_OnDictionary_UsesArrayFromFastPath()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var source = new System.Collections.Generic.Dictionary<string, int>();
                    var filtered = source.Where(_ => true).ToList();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.IsNotNull(script);
		StringAssert.Contains(script, "let source = new Map;");
		StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: source is null\");");
		StringAssert.Contains(script, "throw new Error(\"ArgumentNullException: predicate is null\");");
		StringAssert.Contains(script, "return Array.from(__src).filter(__callback);");
	}

	#endregion

	#region 扩展测试用例 - 链式调用

	/// <summary>
	/// 测试链式方法调用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ChainedMethods()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    string result = ""hello world"".ToUpper().Trim().Substring(0, 5);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let result = ""hello world"".toUpperCase().trim().substring(0, 0 + 5);
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试链式属性访问
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ChainedProperties()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var date = System.DateTime.Now.Date;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let date = _d77d20d9d04e2b6b(_ee9dd166a34a2fa5());
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
	}

	/// <summary>
	/// 测试混合链式调用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_MixedChaining()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    int len = ""hello"".ToUpper().Length;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let len = ""hello"".toUpperCase().length;
}", script);
	}

	/// <summary>
	/// 测试 DateOnly.ToString 走运行时 carrier 的 toString，而不是退化成普通对象访问。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_DateOnlyToString()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var date = new System.DateOnly(2024, 1, 2);
                    var text = date.ToString();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let date = _8c5a25d777626c6c(2024, 1, 2);
  let text = date.toString();
}", script);
	}

	/// <summary>
	/// 测试 TimeOnly.ToString 绑定到 carrier 的 toString。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_TimeOnlyToString()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var time = new System.TimeOnly(12, 30, 0);
                    var text = time.ToString();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let time = _e9a3481b3456aad4(12, 30, 0);
  let text = time.toString();
}", script);
	}

	/// <summary>
	/// 测试 TimeSpan.ToString 绑定到 carrier 的 toString。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_TimeSpanToString()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var span = new System.TimeSpan(1, 2, 3);
                    var text = span.ToString();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let span = _6f22e268aec62fe7(1, 2, 3);
  let text = span.toString();
}", script);
	}

	/// <summary>
	/// 测试 DateTimeOffset.Date 不会误降级成 JS Date 的 date/getDate 访问，而是走专门 helper。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_DateTimeOffsetDate()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var dto = new System.DateTimeOffset(2024, 1, 1, 12, 0, 0, System.TimeSpan.Zero);
                    var date = dto.Date;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let dto = _d90dce0e1d2f06e4(2024, 1, 1, 12, 0, 0, _e5548fcde33957a6());
  let date = _d7098a1eabebc945(dto);
}", script);
	}

	/// <summary>
	/// 测试 DateTimeOffset.ToString 绑定到 carrier 的 toString。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_DateTimeOffsetToString()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var dto = new System.DateTimeOffset(2024, 1, 1, 12, 0, 0, System.TimeSpan.Zero);
                    var text = dto.ToString();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let dto = _d90dce0e1d2f06e4(2024, 1, 1, 12, 0, 0, _e5548fcde33957a6());
  let text = _2aaccc10061a3bb0(dto);
}", script);
	}

	/// <summary>
	/// 测试 CultureInfo 仍然按字符串 carrier 工作，Name 直接取值，ToString 走标准 helper。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_CultureInfoNameAndToString()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var culture = new System.Globalization.CultureInfo(""en-US"");
                    var name = culture.Name;
                    var text = culture.ToString();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let culture = _b7486264ae338f27(""en-US"");
  let name = culture;
  let text = _559b27327f84f1af(culture);
}", script);
	}

	/// <summary>
	/// 测试 InvariantCulture 保持字符串 carrier 语义。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_CultureInfoInvariantCulture()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var culture = System.Globalization.CultureInfo.InvariantCulture;
                    var name = culture.Name;
                    var text = culture.ToString();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let culture = _e4c4d53d69e72382();
  let name = culture;
  let text = _559b27327f84f1af(culture);
}", script);
	}

	/// <summary>
	/// 测试 CultureInfo.EnglishName / NativeName 绑定到专门 helper，而不是退化成字符串属性访问。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_CultureInfoLocalizedNames()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var culture = new System.Globalization.CultureInfo(""zh-CN"");
                    var englishName = culture.EnglishName;
                    var nativeName = culture.NativeName;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let culture = _b7486264ae338f27(""zh-CN"");
  let englishName = _97ad9637d1f75e7c(culture);
  let nativeName = _a4804f687bfc0013(culture);
}", script);
	}

	/// <summary>
	/// 测试 CultureInfo 的三字母语言代码属性绑定到模块 helper。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_CultureInfoThreeLetterLanguageNames()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var culture = new System.Globalization.CultureInfo(""en-US"");
                    var iso = culture.ThreeLetterISOLanguageName;
                    var windows = culture.ThreeLetterWindowsLanguageName;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let culture = _b7486264ae338f27(""en-US"");
  let iso = _285ede13a469ce7b(culture);
  let windows = _1f981ccac713f3d9(culture);
}", script);
	}

	/// <summary>
	/// 测试 GregorianCalendar 通过专门 helper 创建，并访问实例方法。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_GregorianCalendarGetYear()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var calendar = new System.Globalization.GregorianCalendar();
                    var year = calendar.GetYear(new System.DateTime(2024, 1, 2));
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let calendar = _23b9e8d671b5210e();
  let year = _fd5a2cde6fb4d6f5(calendar, _4cb33a818161a3e1(2024, 1, 2));
}", script);
	}

	/// <summary>
	/// 测试 GregorianCalendar 的实例状态不会丢失。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_GregorianCalendarStatefulMembers()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var calendar = new System.Globalization.GregorianCalendar(System.Globalization.GregorianCalendarTypes.USEnglish);
                    calendar.TwoDigitYearMax = 2099;
                    var year = calendar.ToFourDigitYear(30);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let calendar = _c043a86ee7a70c81(2);
  _9537b0490ec80689(calendar, 2099);
  let year = _cca1b99b56b6a322(calendar, 30);
}", script);
	}

	/// <summary>
	/// 测试 CultureInfo.Calendar 返回 GregorianCalendar wrapper，并继续走实例 helper。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_CultureInfoCalendarChain()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var culture = new System.Globalization.CultureInfo(""en-US"");
                    var calendar = culture.Calendar;
                    var year = calendar.ToFourDigitYear(30);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let culture = _b7486264ae338f27(""en-US"");
  let calendar = _2ab4f6aaba1be337(culture);
  let year = _8e7d51754b95ea42(calendar, 30);
}", script);
	}

	/// <summary>
	/// 测试 Guid.ToString 走规范化 helper，而不是直接调用原生 string.toString。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_GuidToString()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var guid = new System.Guid(""00112233-4455-6677-8899-aabbccddeeff"");
                    var text = guid.ToString();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let guid = _24e026ca196fe82b(""00112233-4455-6677-8899-aabbccddeeff"");
  let text = _055f1f857de6de37(guid);
}", script);
	}

	/// <summary>
	/// 测试 decimal.ToString 走 DecimalModule helper，而不是退化成 JS Number/String 的原生 toString。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_DecimalToString()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    decimal value = decimal.MaxValue;
                    var text = value.ToString();
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let value = _6a4e5f697d4fc607();
  let text = _65a0e4fe8ccdd829(value);
}", script);
	}

	/// <summary>
	/// 测试 DateOnly.Parse 绑定到 DateOnlyModule helper。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_DateOnlyParse()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var value = System.DateOnly.Parse(""2024-01-02"");
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let value = _e2640560d207afce(""2024-01-02"");
}", script);
	}

	/// <summary>
	/// 测试 DateOnly.ToString(string) 不会回退到默认 toString。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_DateOnlyToStringFormat()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var value = new System.DateOnly(2024, 1, 2);
                    var text = value.ToString(""O"");
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let value = _8c5a25d777626c6c(2024, 1, 2);
  let text = _5dd96e58e55f801c(value, ""O"");
}", script);
	}

	/// <summary>
	/// 测试 TimeOnly.Parse(string, provider, style) 绑定到完整 helper。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_TimeOnlyParseWithStyle()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var value = System.TimeOnly.Parse(""12:30:00"", null, System.Globalization.DateTimeStyles.None);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let value = _b10aeed232e37ce3(""12:30:00"", null, 0);
}", script);
	}

	/// <summary>
	/// 测试 TimeOnly.ToString(string, provider) 绑定到格式化 helper。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_TimeOnlyToStringFormat()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var value = new System.TimeOnly(12, 30, 0);
                    var text = value.ToString(""O"", null);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let value = _e9a3481b3456aad4(12, 30, 0);
  let text = _dd80539f727e11c1(value, ""O"", null);
}", script);
	}

	/// <summary>
	/// 测试 TimeSpan.Parse(string, provider) 绑定到 TimeSpanModule helper。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_TimeSpanParseWithProvider()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var value = System.TimeSpan.Parse(""01:02:03"", null);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let value = _55da737da6ee6a65(""01:02:03"", null);
}", script);
	}

	/// <summary>
	/// 测试 TimeSpan.ToString(string, provider) 绑定到格式化 helper。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_TimeSpanToStringFormat()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var value = new System.TimeSpan(1, 2, 3);
                    var text = value.ToString(""c"", null);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let value = _6f22e268aec62fe7(1, 2, 3);
  let text = _49fbba4d75df94f7(value, ""c"", null);
}", script);
	}

	/// <summary>
	/// 测试 DateTimeOffset.Parse(string, provider, style) 绑定到完整 helper。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_DateTimeOffsetParseWithStyle()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var value = System.DateTimeOffset.Parse(""2024-01-02T03:04:05+08:00"", null, System.Globalization.DateTimeStyles.None);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let value = _277a1a2c7845bcdc(""2024-01-02T03:04:05+08:00"", null, 0);
}", script);
	}

	/// <summary>
	/// 测试 DateTimeOffset.ToString(string, provider) 绑定到格式化 helper。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_DateTimeOffsetToStringFormat()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var value = new System.DateTimeOffset(2024, 1, 1, 12, 0, 0, System.TimeSpan.Zero);
                    var text = value.ToString(""O"", null);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let value = _d90dce0e1d2f06e4(2024, 1, 1, 12, 0, 0, _e5548fcde33957a6());
  let text = _e856edbfd7db0646(value, ""O"", null);
}", script);
	}

	/// <summary>
	/// 测试 decimal.Parse(string, provider) 绑定到 DecimalModule helper。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_DecimalParseWithProvider()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var value = decimal.Parse(""123.45"", null);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let value = _01be2a34fe2cda4e(""123.45"", null);
}", script);
	}

	/// <summary>
	/// 测试 decimal.ToString(string, provider) 绑定到 DecimalModule helper。
	/// </summary>
	[TestMethod]
	public void Visit_Reference_DecimalToStringFormat()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    var value = decimal.Parse(""123.45"", null);
                    var text = value.ToString(""G"", null);
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertScriptEqual(@"{
  let value = _01be2a34fe2cda4e(""123.45"", null);
  let text = _b1e6a06111674f0c(value, ""G"", null);
}", script);
	}

	#endregion

	#region 扩展测试用例 - this和base

	/// <summary>
	/// 测试 this 引用
	/// </summary>
	[TestMethod]
	public void Visit_Reference_This()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                private int _value;

                void TestMethod()
                {
                    this._value = 10;
                }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  this._value = 10;
}", script);
	}

	/// <summary>
	/// 测试 this 作为参数
	/// </summary>
	[TestMethod]
	public void Visit_Reference_ThisAsArgument()
	{
		var block = GetBlockOperation(@"
            class TestClass
            {
                void TestMethod()
                {
                    Process(this);
                }

                void Process(TestClass obj) { }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		AssertJsNamingScriptEqual(@"{
  this.Process(this);
}", script);
	}

	#endregion
}
#endregion
