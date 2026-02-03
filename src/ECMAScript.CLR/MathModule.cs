using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Math", WhiteListOp.Allowed, null, "System/MathModule.js")]
public static class MathModule
{
	//System.Math.E = 2.718281828459045;

	//System.Math.PI = 3.141592653589793;

	//System.Math.Tau = 6.283185307179586;

	[WhiteList("static System.Math.Abs(decimal)", WhiteListOp.Replace, "abs")]
	public extern static String _eab3564b2663dff6(String value);

	[WhiteList("static System.Math.Abs(double)", WhiteListOp.Replace, "abs")]
	public extern static Number _6a0f94e87051cd5f(Number value);

	[WhiteList("static System.Math.Abs(short)", WhiteListOp.Replace, "abs")]
	public extern static Number _81a80e1bfb516bfb(Number value);

	[WhiteList("static System.Math.Abs(int)", WhiteListOp.Replace, "abs")]
	public extern static Number _0aaf1073fc70e405(Number value);

	[WhiteList("static System.Math.Abs(long)", WhiteListOp.Replace, "abs")]
	public extern static BigInt _2f5b0b713dde9501(BigInt value);

	[WhiteList("static System.Math.Abs(nint)", WhiteListOp.Replace, "abs")]
	public extern static nint _6de080191221a07d(object value);

	[WhiteList("static System.Math.Abs(sbyte)", WhiteListOp.Replace, "abs")]
	public extern static Number _6ed2ee0733ac7051(Number value);

	[WhiteList("static System.Math.Abs(float)", WhiteListOp.Replace, "abs")]
	public extern static Number _3e86488d0112bcd3(Number value);

	[WhiteList("static System.Math.Acos(double)", WhiteListOp.Replace, "acos")]
	public extern static Number _473e58e8c04acfd3(Number d);

	[WhiteList("static System.Math.Acosh(double)", WhiteListOp.Replace, "acos")]
	public extern static Number _46ecb0a75e5ba94e(Number d);

	[WhiteList("static System.Math.Asin(double)", WhiteListOp.Replace, "asin")]
	public extern static Number _31a8579686d23c98(Number d);

	[WhiteList("static System.Math.Asinh(double)", WhiteListOp.Replace, "asinh")]
	public extern static Number _fac652d6d6a2503b(Number d);

	[WhiteList("static System.Math.Atan(double)", WhiteListOp.Replace, "atan")]
	public extern static Number _64bb4dcf5871842b(Number d);

	[WhiteList("static System.Math.Atan2(double, double)", WhiteListOp.Replace, "atan2")]
	public extern static Number _cc6b2bb857d27648(Number y, Number x);

	[WhiteList("static System.Math.Atanh(double)", WhiteListOp.Replace, "atanh")]
	public extern static Number _8093e8210867a45e(Number d);

	[WhiteList("static System.Math.BigMul(int, int)", WhiteListOp.Discard)]
	public extern static BigInt _f8dfabc9cf61c7c8(Number a, Number b);

	[WhiteList("static System.Math.BigMul(long, long)", WhiteListOp.Discard)]
	public extern static BigInt _9eceeda3d33f938a(BigInt a, BigInt b);

	[WhiteList("static System.Math.BigMul(long, long, out long)", WhiteListOp.Discard)]
	public extern static BigInt _1f2b3fb549b0a774(BigInt a, BigInt b, OutValue<BigInt> low);

	[WhiteList("static System.Math.BigMul(uint, uint)", WhiteListOp.Discard)]
	public extern static BigInt _6683ad6f7ac7c14c(Number a, Number b);

	[WhiteList("static System.Math.BigMul(ulong, ulong)", WhiteListOp.Discard)]
	public extern static BigInt _d2fa7191b8139e97(BigInt a, BigInt b);

	[WhiteList("static System.Math.BigMul(ulong, ulong, out ulong)", WhiteListOp.Discard)]
	public extern static BigInt _99697fddb05f0646(BigInt a, BigInt b, OutValue<BigInt> low);

	[WhiteList("static System.Math.BitDecrement(double)", WhiteListOp.Discard)]
	public extern static Number _bc28ec82e8385202(Number x);

	[WhiteList("static System.Math.BitIncrement(double)", WhiteListOp.Discard)]
	public extern static Number _655bd4d428ca20ea(Number x);

	[WhiteList("static System.Math.Cbrt(double)", WhiteListOp.Replace, "cbrt")]
	public extern static Number _9369c8e8f81372b6(Number d);

	[WhiteList("static System.Math.Ceiling(decimal)", WhiteListOp.Replace, "ceil")]
	public extern static String _84cbc0eaf2d899af(String d);

	[WhiteList("static System.Math.Ceiling(double)", WhiteListOp.Replace, "ceil")]
	public extern static Number _d7be7c95bfefd788(Number a);

	[WhiteList("static System.Math.Clamp(byte, byte, byte)", WhiteListOp.Discard)]
	public extern static Number _8921213084b6685c(Number value, Number min, Number max);

	[WhiteList("static System.Math.Clamp(decimal, decimal, decimal)", WhiteListOp.Discard)]
	public extern static String _735e24a467fce432(String value, String min, String max);

	[WhiteList("static System.Math.Clamp(double, double, double)", WhiteListOp.Discard)]
	public extern static Number _a416f1414d77c0fa(Number value, Number min, Number max);

	[WhiteList("static System.Math.Clamp(short, short, short)", WhiteListOp.Discard)]
	public extern static Number _86bd53ebc62ad520(Number value, Number min, Number max);

	[WhiteList("static System.Math.Clamp(int, int, int)", WhiteListOp.Discard)]
	public extern static Number _ac5962f496c6acc0(Number value, Number min, Number max);

	[WhiteList("static System.Math.Clamp(long, long, long)", WhiteListOp.Discard)]
	public extern static BigInt _d74b585d391b448a(BigInt value, BigInt min, BigInt max);

	[WhiteList("static System.Math.Clamp(nint, nint, nint)", WhiteListOp.Discard)]
	public extern static nint _63803d1734456eee(object value, object min, object max);

	[WhiteList("static System.Math.Clamp(sbyte, sbyte, sbyte)", WhiteListOp.Discard)]
	public extern static Number _f2a0d82587b4e02a(Number value, Number min, Number max);

	[WhiteList("static System.Math.Clamp(float, float, float)", WhiteListOp.Discard)]
	public extern static Number _751a0e2d62df6aff(Number value, Number min, Number max);

	[WhiteList("static System.Math.Clamp(ushort, ushort, ushort)", WhiteListOp.Discard)]
	public extern static Number _74d6735122ecb151(Number value, Number min, Number max);

	[WhiteList("static System.Math.Clamp(uint, uint, uint)", WhiteListOp.Discard)]
	public extern static Number _8322034639d6a05c(Number value, Number min, Number max);

	[WhiteList("static System.Math.Clamp(ulong, ulong, ulong)", WhiteListOp.Discard)]
	public extern static BigInt _f1743d6e0c7a2101(BigInt value, BigInt min, BigInt max);

	[WhiteList("static System.Math.Clamp(nuint, nuint, nuint)", WhiteListOp.Discard)]
	public extern static nuint _25b262a1a57d5d06(object value, object min, object max);

	[WhiteList("static System.Math.CopySign(double, double)", WhiteListOp.Discard)]
	public extern static Number _f51bc6e5d8ce272b(Number x, Number y);

	[WhiteList("static System.Math.Cos(double)", WhiteListOp.Replace, "cos")]
	public extern static Number _b6b312cfcefe789c(Number d);

	[WhiteList("static System.Math.Cosh(double)", WhiteListOp.Replace, "cosh")]
	public extern static Number _c6f1b8664a086e13(Number value);

	[WhiteList("static System.Math.DivRem(byte, byte)", WhiteListOp.Discard)]
	public extern static (byte Quotient, byte Remainder) _09ec2eababe53085(Number left, Number right);

	[WhiteList("static System.Math.DivRem(short, short)", WhiteListOp.Discard)]
	public extern static (short Quotient, short Remainder) _f6eb115003bc623f(Number left, Number right);

	[WhiteList("static System.Math.DivRem(int, int)", WhiteListOp.Discard)]
	public extern static (int Quotient, int Remainder) _45a4ab35fd8b6be8(Number left, Number right);

	[WhiteList("static System.Math.DivRem(int, int, out int)", WhiteListOp.Discard)]
	public extern static Number _2a90cb0f64781864(Number a, Number b, OutValue<Number> result);

	[WhiteList("static System.Math.DivRem(long, long)", WhiteListOp.Discard)]
	public extern static (long Quotient, long Remainder) _96f1b2c20bd2e40b(BigInt left, BigInt right);

	[WhiteList("static System.Math.DivRem(long, long, out long)", WhiteListOp.Discard)]
	public extern static BigInt _1961d3558bd76ea4(BigInt a, BigInt b, OutValue<BigInt> result);

	[WhiteList("static System.Math.DivRem(nint, nint)", WhiteListOp.Discard)]
	public extern static (nint Quotient, nint Remainder) _98ac53eebed8e823(object left, object right);

	[WhiteList("static System.Math.DivRem(sbyte, sbyte)", WhiteListOp.Discard)]
	public extern static (sbyte Quotient, sbyte Remainder) _e0661118fd9ce98d(Number left, Number right);

	[WhiteList("static System.Math.DivRem(ushort, ushort)", WhiteListOp.Discard)]
	public extern static (ushort Quotient, ushort Remainder) _267e04d7693208d4(Number left, Number right);

	[WhiteList("static System.Math.DivRem(uint, uint)", WhiteListOp.Discard)]
	public extern static (uint Quotient, uint Remainder) _c8e57fe110813408(Number left, Number right);

	[WhiteList("static System.Math.DivRem(ulong, ulong)", WhiteListOp.Discard)]
	public extern static (ulong Quotient, ulong Remainder) _4d9536a1220a7365(BigInt left, BigInt right);

	[WhiteList("static System.Math.DivRem(nuint, nuint)", WhiteListOp.Discard)]
	public extern static (nuint Quotient, nuint Remainder) _1b2439f6e0d31865(object left, object right);

	[WhiteList("static System.Math.Exp(double)", WhiteListOp.Replace, "exp")]
	public extern static Number _d5b39999cc90e482(Number d);

	[WhiteList("static System.Math.Floor(decimal)", WhiteListOp.Replace, "floor")]
	public extern static String _b12193a7b6647a82(String d);

	[WhiteList("static System.Math.Floor(double)", WhiteListOp.Replace, "floor")]
	public extern static Number _a43200909dff4bc0(Number d);

	[WhiteList("static System.Math.FusedMultiplyAdd(double, double, double)", WhiteListOp.Discard)]
	public extern static Number _52c95df2ad20c3bd(Number x, Number y, Number z);

	[WhiteList("static System.Math.IEEERemainder(double, double)", WhiteListOp.Discard)]
	public extern static Number _288c181b5d9cf968(Number x, Number y);

	[WhiteList("static System.Math.ILogB(double)", WhiteListOp.Discard)]
	public extern static Number _51e4d6005e6e11ef(Number x);

	[WhiteList("static System.Math.Log(double)", WhiteListOp.Replace, "log")]
	public extern static Number _c65770c0fcbed4b6(Number d);

	[WhiteList("static System.Math.Log(double, double)", WhiteListOp.Replace, "log")]
	public extern static Number _da091a35a0d7bc64(Number a, Number newBase);

	[WhiteList("static System.Math.Log10(double)", WhiteListOp.Replace, "log10")]
	public extern static Number _a882de08086ccec9(Number d);

	[WhiteList("static System.Math.Log2(double)", WhiteListOp.Replace, "log2")]
	public extern static Number _e622dc98a98720f4(Number x);

	[WhiteList("static System.Math.Max(byte, byte)", WhiteListOp.Replace, "max")]
	public extern static Number _a26e415f31a1dd41(Number val1, Number val2);

	[WhiteList("static System.Math.Max(decimal, decimal)", WhiteListOp.Replace, "max")]
	public extern static String _68326de2fcd99278(String val1, String val2);

	[WhiteList("static System.Math.Max(double, double)", WhiteListOp.Replace, "max")]
	public extern static Number _1bcd36ee2d1a5261(Number val1, Number val2);

	[WhiteList("static System.Math.Max(short, short)", WhiteListOp.Replace, "max")]
	public extern static Number _52a2dcd88692950d(Number val1, Number val2);

	[WhiteList("static System.Math.Max(int, int)", WhiteListOp.Replace, "max")]
	public extern static Number _c89f0321e6ece69a(Number val1, Number val2);

	[WhiteList("static System.Math.Max(long, long)", WhiteListOp.Replace, "max")]
	public extern static BigInt _1513b88bb1abfff1(BigInt val1, BigInt val2);

	[WhiteList("static System.Math.Max(nint, nint)", WhiteListOp.Replace, "max")]
	public extern static nint _c03baee2a94d0113(object val1, object val2);

	[WhiteList("static System.Math.Max(sbyte, sbyte)", WhiteListOp.Replace, "max")]
	public extern static Number _cb1537d45a143e0d(Number val1, Number val2);

	[WhiteList("static System.Math.Max(float, float)", WhiteListOp.Replace, "max")]
	public extern static Number _5acf698f9a9ada61(Number val1, Number val2);

	[WhiteList("static System.Math.Max(ushort, ushort)", WhiteListOp.Replace, "max")]
	public extern static Number _07de56d6927ee6af(Number val1, Number val2);

	[WhiteList("static System.Math.Max(uint, uint)", WhiteListOp.Replace, "max")]
	public extern static Number _6638c647001d2908(Number val1, Number val2);

	[WhiteList("static System.Math.Max(ulong, ulong)", WhiteListOp.Replace, "max")]
	public extern static BigInt _3ac884b966eeb605(BigInt val1, BigInt val2);

	[WhiteList("static System.Math.Max(nuint, nuint)", WhiteListOp.Replace, "max")]
	public extern static nuint _7f3becc9b24d51d3(object val1, object val2);

	[WhiteList("static System.Math.MaxMagnitude(double, double)", WhiteListOp.Discard)]
	public extern static Number _7922e74207558715(Number x, Number y);

	[WhiteList("static System.Math.Min(byte, byte)", WhiteListOp.Replace, "min")]
	public extern static Number _f8806316e956dbb8(Number val1, Number val2);

	[WhiteList("static System.Math.Min(decimal, decimal)", WhiteListOp.Replace, "min")]
	public extern static String _87f14d6593efd87f(String val1, String val2);

	[WhiteList("static System.Math.Min(double, double)", WhiteListOp.Replace, "min")]
	public extern static Number _d0d428d1a1f7d899(Number val1, Number val2);

	[WhiteList("static System.Math.Min(short, short)", WhiteListOp.Replace, "min")]
	public extern static Number _d7a779b3283b34dc(Number val1, Number val2);

	[WhiteList("static System.Math.Min(int, int)", WhiteListOp.Replace, "min")]
	public extern static Number _7fb229bda6fa1941(Number val1, Number val2);

	[WhiteList("static System.Math.Min(long, long)", WhiteListOp.Replace, "min")]
	public extern static BigInt _b98fea9bd3e4ce52(BigInt val1, BigInt val2);

	[WhiteList("static System.Math.Min(nint, nint)", WhiteListOp.Replace, "min")]
	public extern static nint _e3cdc59c4e2b3f04(object val1, object val2);

	[WhiteList("static System.Math.Min(sbyte, sbyte)", WhiteListOp.Replace, "min")]
	public extern static Number _0f8bf59fee331622(Number val1, Number val2);

	[WhiteList("static System.Math.Min(float, float)", WhiteListOp.Replace, "min")]
	public extern static Number _2c1e93a158a72838(Number val1, Number val2);

	[WhiteList("static System.Math.Min(ushort, ushort)", WhiteListOp.Replace, "min")]
	public extern static Number _3e853af2da5fd862(Number val1, Number val2);

	[WhiteList("static System.Math.Min(uint, uint)", WhiteListOp.Replace, "min")]
	public extern static Number _849b5d874239b92c(Number val1, Number val2);

	[WhiteList("static System.Math.Min(ulong, ulong)", WhiteListOp.Replace, "min")]
	public extern static BigInt _d468e999912e1120(BigInt val1, BigInt val2);

	[WhiteList("static System.Math.Min(nuint, nuint)", WhiteListOp.Replace, "min")]
	public extern static nuint _c03fe2f175939d3a(object val1, object val2);

	[WhiteList("static System.Math.MinMagnitude(double, double)", WhiteListOp.Discard)]
	public extern static Number _44776725ec896ede(Number x, Number y);

	[WhiteList("static System.Math.Pow(double, double)", WhiteListOp.Replace, "pow")]
	public extern static Number _fd439387b010bb99(Number x, Number y);

	[WhiteList("static System.Math.ReciprocalEstimate(double)", WhiteListOp.Discard)]
	public extern static Number _63ae085718e46139(Number d);

	[WhiteList("static System.Math.ReciprocalSqrtEstimate(double)", WhiteListOp.Discard)]
	public extern static Number _5ab45aaeb89fbf4c(Number d);

	[WhiteList("static System.Math.Round(decimal)", WhiteListOp.Replace, "round")]
	public extern static String _257741f3e4260d82(String d);

	[WhiteList("static System.Math.Round(decimal, int)", WhiteListOp.Replace, "round")]
	public extern static String _10e883cf6d89b70c(String d, Number decimals);

	[WhiteList("static System.Math.Round(decimal, int, System.MidpointRounding)", WhiteListOp.Discard)]
	public extern static String _b955eff4c2d1fa63(String d, Number decimals, object mode);

	[WhiteList("static System.Math.Round(decimal, System.MidpointRounding)", WhiteListOp.Discard)]
	public extern static String _584a7b2219b578fa(String d, object mode);

	[WhiteList("static System.Math.Round(double)", WhiteListOp.Replace, "round")]
	public extern static Number _6cd7f67f98eae0bc(Number a);

	[WhiteList("static System.Math.Round(double, int)", WhiteListOp.Replace, "round")]
	public extern static Number _dab059b61a5b7428(Number value, Number digits);

	[WhiteList("static System.Math.Round(double, int, System.MidpointRounding)", WhiteListOp.Discard)]
	public extern static Number _ef441dda2abcc022(Number value, Number digits, object mode);

	[WhiteList("static System.Math.Round(double, System.MidpointRounding)", WhiteListOp.Discard)]
	public extern static Number _a7f99c51d0db12b5(Number value, object mode);

	[WhiteList("static System.Math.ScaleB(double, int)", WhiteListOp.Discard)]
	public extern static Number _11ce4194425195ad(Number x, Number n);

	[WhiteList("static System.Math.Sign(decimal)", WhiteListOp.Discard)]
	public extern static Number _8d626104a531d041(String value);

	[WhiteList("static System.Math.Sign(double)", WhiteListOp.Replace, "sign")]
	public extern static Number _9a554cfca79bdc59(Number value);

	[WhiteList("static System.Math.Sign(short)", WhiteListOp.Replace, "sign")]
	public extern static Number _f8eefd9c948ed90a(Number value);

	[WhiteList("static System.Math.Sign(int)", WhiteListOp.Replace, "sign")]
	public extern static Number _cfeb8757509066b2(Number value);

	[WhiteList("static System.Math.Sign(long)", WhiteListOp.Replace, "sign")]
	public extern static Number _5354f93121b296ff(BigInt value);

	[WhiteList("static System.Math.Sign(nint)", WhiteListOp.Replace, "sign")]
	public extern static Number _e5d5397dfe870f94(object value);

	[WhiteList("static System.Math.Sign(sbyte)", WhiteListOp.Replace, "sign")]
	public extern static Number _88575fe160876695(Number value);

	[WhiteList("static System.Math.Sign(float)", WhiteListOp.Replace, "sign")]
	public extern static Number _c0668680ba7ef96e(Number value);

	[WhiteList("static System.Math.Sin(double)", WhiteListOp.Replace, "sin")]
	public extern static Number _f1029100ea8114ab(Number a);

	[WhiteList("static System.Math.SinCos(double)", WhiteListOp.Discard)]
	public extern static (double Sin, double Cos) _4dcadff583296186(Number x);

	[WhiteList("static System.Math.Sinh(double)", WhiteListOp.Replace, "sinh")]
	public extern static Number _f48ae51bac192bdf(Number value);

	[WhiteList("static System.Math.Sqrt(double)", WhiteListOp.Replace, "sqrt")]
	public extern static Number _b303f709d2b283f0(Number d);

	[WhiteList("static System.Math.Tan(double)", WhiteListOp.Replace, "tan")]
	public extern static Number _5f9763f3b0176663(Number a);

	[WhiteList("static System.Math.Tanh(double)", WhiteListOp.Replace, "tanh")]
	public extern static Number _d198ea5fec4f6c8a(Number value);

	[WhiteList("static System.Math.Truncate(decimal)", WhiteListOp.Replace, "trunc")]
	public extern static String _abd9211e1e7514b4(String d);

	[WhiteList("static System.Math.Truncate(double)", WhiteListOp.Replace, "trunc")]
	public extern static Number _b74eaf879a3b5fd7(Number d);
}
