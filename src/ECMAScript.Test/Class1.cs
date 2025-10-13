using Microsoft.CodeAnalysis.Operations;
using System.Collections;
using System.Collections.Frozen;
using System.ComponentModel;
using System.Numerics;

namespace ECMAScript.Test;

internal class Class1<T>
{
	public int Test(T value)
	{
		//GregorianCalendar a = 1;
		var a = 1;
		//var b = new bool();

		BigInteger b = BigInteger.One;
		var c = new string("");
		return -a;
	}
}

/// <summary>
/// 
/// </summary>
public class CSharpToJsConverter : OperationWalker
{

}


[ECMAScriptModule]
internal class Class1
{
	public string Property { get; set; } = "Hello, World!";

	public Dictionary<int, int> b;

	public void A1() { }

	public void Method()
	{
		var cc = new EventTarget();
		cc.AddEventListener(EventType.MouseDown, new HandleEventCallback(e =>
		{

		}));

		var b = new Action<int>(x => { });
		b.Invoke(1);

		var h = new {
			a=1
		};

		h.ToString();

		var a7 = new Class1<int>();

		a7.Test(9);

		//var cc = window.Caches
		var a = document.GetElementsByName("");
		var c = a.Select(b => b.TextContent).ToList();
		foreach (var x in a)
		{

		}
		//var a = new StylePropertyMapReadOnly();
		//foreach (var (c,b) in a)
		//{

		//}
		//var cc = new StringBuilder();
		TimeOnly hh;
		hh = new TimeOnly(12, 30, 45);
		//var b = new DateTime();
		//var d = int.Parse("2");
		//b.AddSeconds(1);
	}
}

public static class Test1
{
	public static int a, b;
	public static int A => a;

	public static int A1() => a;

	public class C1
	{

	}

	public enum C2
	{

	}
}


public record User(
	[Description("")]string Username,
	string Password,
	DateTime BirthDate,
	decimal Salary);

[Omit<User>("Password","Salary")]
[Pick<User>("Password", "Salary")]
[With<int?>("ClassId")]
public partial interface ISecureUser;

public class a {
	public FrozenSet<int> MyProperty1 { get; set; }
	public BigInteger MyProperty { get; set; }
}

public record A1 (int B1,int C1);
// Fix for CS1729: “A1”不包含采用 0 个参数的构造函数  
// Fix for CS8907: 参数“B1”未读。是否忘记通过它来使用该名称初始化属性?  
// Fix for CS8907: 参数“C1”未读。是否忘记通过它来使用该名称初始化属性?  

public record D1(int B1, int C1) : A1(B1, C1);