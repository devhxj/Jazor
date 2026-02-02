using ECMAScript;
using static ECMAScript.Global;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1;

class Point
{
	public int X { get; set; }
	public int Y { get; set; }

	public Point(int x, int y)
	{
		X = x;
		Y = y;
	}

	public void Deconstruct(out int x, out int y)
	{
		x = X;
		y = Y;
	}
}


class TestClass
{
	void TestMethod()
	{
		var a = Math.PI * Math.SQRT1_2;
		var b = TypeOf(a);
		var c = RegExp("a");
		Console.Log("Hello, World!");
	}

	class A
	{
		public B? A1 { get; set; }
		public string? A2 { get; set; }
	}

	class B
	{
		public string? B1 { get; set; }
		public C? B2 { get; set; }
	}

	class C
	{
		public string? C1 { get; set; }
		public int C2 { get; set; }
	}
}