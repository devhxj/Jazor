using Acornima.Ast;
using ECMAScript;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

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
	private static IBlockOperation GetBlockOperation(string code)
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
			assemblyName: "TestAssembly",
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

		// 查找第一个方法体
		var methodDeclaration = root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
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
            using System.ComponentModel;
            using console = ECMAScript.Console.Console;

            class TestClass
            {
                void TestMethod()
                {
                    console.Log(""hello"");
                }
            }

            namespace ECMAScript.Console
            {
                [Description(""@#console"")]
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
                    var signedMaxValue = short.Max(signedMin, signedMax);
                    var signedMinValue = short.Min(signedMin, signedMax);
                    var unsignedClamp = ushort.Clamp(unsignedValue, unsignedMin, unsignedMax);
                    var unsignedSignum = ushort.Sign(unsignedValue);
                    var unsignedEven = ushort.IsEvenInteger(unsignedValue);
                    var unsignedOdd = ushort.IsOddInteger(unsignedValue);
                    var unsignedPow2 = ushort.IsPow2(unsignedValue);
                    var unsignedLog2 = ushort.Log2(unsignedValue);
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
  let signedMaxValue = Math.max(signedMin, signedMax);
  let signedMinValue = Math.min(signedMin, signedMax);
  let unsignedClamp = Math.min(Math.max(unsignedValue, unsignedMin), unsignedMax);
  let unsignedSignum = unsignedValue === 0 ? 0 : 1;
  let unsignedEven = (unsignedValue & 1) === 0;
  let unsignedOdd = (unsignedValue & 1) !== 0;
  let unsignedPow2 = unsignedValue > 0 && (unsignedValue & unsignedValue - 1) === 0;
  let unsignedLog2 = Math.floor(Math.log2(unsignedValue));
  let unsignedMaxValue = Math.max(unsignedMin, unsignedMax);
  let unsignedMinValue = Math.min(unsignedMin, unsignedMax);
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
	/// 转换结果：let x = Number.EPSILON;
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
  let x = Number.EPSILON;
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

		Assert.AreEqual(@"{
  let obj = new TestClass;
  let x = obj.Field;
}", script);
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

		Assert.AreEqual(@"{
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

		Assert.AreEqual(
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
  let slice = array.slice(1, 3 + 1);
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
  let slice = array.slice(0, 3 + 1);
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
	/// 转换结果：array.slice(array.length - 3, array.length - 1 + 1)
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
  let slice = array.slice(array.length - 3, array.length - 1 + 1);
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

		Assert.AreEqual(@"{
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
  let middle = array.slice(1, 4 + 1);
  let fromTwo = array.slice(2);
  let uptoThree = array.slice(0, 3 + 1);
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
  let epsilon = Number.EPSILON;
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

		Assert.AreEqual(@"{
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

		Assert.AreEqual(@"{
  let obj = new TestClass;
  let action = obj.DoSomething.bind(obj);
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

		Assert.AreEqual(@"{
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

		Assert.AreEqual(@"{
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

		Assert.AreEqual(@"{
  let list = [1, 2, 3];
  let first = list[0];
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
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

		Assert.AreEqual(@"{
  let dict = new Map;
  dict.set(""key"", 42);
  let value = dict[""key""];
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
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

		Assert.AreEqual(@"{
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

		Assert.AreEqual(@"{
  let person = null;
  let dict = new Map;
  dict.set(person?.Name, 42);
  let value = dict[person?.Name];
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
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

                void TestMethod()
                {
                    TestClass obj = new TestClass();
                    obj.Inner = new InnerClass();
                    int x = obj.Inner.Value;
                }
            }

            class InnerClass
            {
                public int Value { get; set; }
            }
        ");

		var walker = new SemanticWalker(true);
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
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

		Assert.AreEqual(@"{
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

		Assert.AreEqual(@"{
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

		Assert.AreEqual(@"{
  let list = [1, 2, 3];
  let first = list[0];
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
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

		Assert.AreEqual(@"{
  let dict = new Map;
  dict.set(""key"", 42);
  let value = dict[""key""];
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
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

		Assert.AreEqual(@"{
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

		Assert.AreEqual(@"{
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

		Assert.AreEqual(@"{
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

	Assert.AreEqual(@"{
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

		Assert.AreEqual(@"{
  let field = A.Field;
  let property = A.Value;
  let result = A.GetNumbers();
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
            class Box
            {
            }

            class TestClass
            {
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
	public void Visit_Reference_BooleanGetTypeCode_UsesCompileHook()
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
	public void Compile_GlobalTypeOf_InvalidHandler_Throws()
	{
		var walker = new SemanticWalker(true);

		Assert.Throws<InvalidOperationException>(() =>
			walker.Compile_b58c68bda64ad0f8(
				new Identifier("host"),
				[new Identifier("value")]));
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
                    var token = Symbol_(""value"");
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
	public void Visit_Reference_UserStaticMethod_DoesNotUseImplicitEcmascriptMemberName()
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
		var node = walker.Visit(block, new());
		var script = node?.ToKnRECMAScript();

		Assert.AreEqual(@"{
  let value = Helper.DoWork();
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
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

		Assert.AreEqual(@"{
  let list = [1, 2, 3];
  let doubled = Array.from(Array.from(list).map(x => {
    return x * 2;
  }));
}".ReplaceLineEndings(), script?.ReplaceLineEndings());
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
  let text = dto.toString();
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
	/// 测试 GregorianCalendar 作为字符串 carrier 创建，并通过专门 helper 访问实例方法。
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
  let value = 79228162514264337593543950335;
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

		Assert.AreEqual(@"{
  this.Process(this);
}", script);
	}

	#endregion
}
#endregion
