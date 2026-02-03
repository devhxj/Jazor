# MathModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：static System.Math.Acos(double)</br>
**签名**：_473e58e8c04acfd3</br>
**注释**：

```xml
<summary>Returns the angle whose cosine is the specified number.</summary>
<param name="d">A number representing a cosine, where <paramref name="d" /> must be greater than or equal to -1, but less than or equal to 1.</param>
<returns>An angle, θ, measured in radians, such that 0 ≤ θ ≤ π. -or- <see cref="F:System.Double.NaN" /> if <paramref name="d" /> &lt; -1 or <paramref name="d" /> &gt; 1 or <paramref name="d" /> equals <see cref="F:System.Double.NaN" />.</returns>
```

**成员**：static System.Math.Acosh(double)</br>
**签名**：_46ecb0a75e5ba94e</br>
**注释**：

```xml
<summary>Returns the angle whose hyperbolic cosine is the specified number.</summary>
<param name="d">A number representing a hyperbolic cosine, where <paramref name="d" /> must be greater than or equal to 1, but less than or equal to <see cref="F:System.Double.PositiveInfinity" />.</param>
<returns>An angle, θ, measured in radians, such that 0 ≤ θ ≤ ∞. -or- <see cref="F:System.Double.NaN" /> if <paramref name="d" /> &lt; 1 or <paramref name="d" /> equals <see cref="F:System.Double.NaN" />.</returns>
```

**成员**：static System.Math.Asin(double)</br>
**签名**：_31a8579686d23c98</br>
**注释**：

```xml
<summary>Returns the angle whose sine is the specified number.</summary>
<param name="d">A number representing a sine, where <paramref name="d" /> must be greater than or equal to -1, but less than or equal to 1.</param>
<returns>An angle, θ, measured in radians, such that -π/2 ≤ θ ≤ π/2. -or- <see cref="F:System.Double.NaN" /> if <paramref name="d" /> &lt; -1 or <paramref name="d" /> &gt; 1 or <paramref name="d" /> equals <see cref="F:System.Double.NaN" />.</returns>
```

**成员**：static System.Math.Asinh(double)</br>
**签名**：_fac652d6d6a2503b</br>
**注释**：

```xml
<summary>Returns the angle whose hyperbolic sine is the specified number.</summary>
<param name="d">A number representing a hyperbolic sine, where <paramref name="d" /> must be greater than or equal to <see cref="F:System.Double.NegativeInfinity" />, but less than or equal to <see cref="F:System.Double.PositiveInfinity" />.</param>
<returns>An angle, θ, measured in radians. -or- <see cref="F:System.Double.NaN" /> if <paramref name="d" /> equals <see cref="F:System.Double.NaN" />.</returns>
```

**成员**：static System.Math.Atan(double)</br>
**签名**：_64bb4dcf5871842b</br>
**注释**：

```xml
<summary>Returns the angle whose tangent is the specified number.</summary>
<param name="d">A number representing a tangent.</param>
<returns>An angle, θ, measured in radians, such that -π/2 ≤ θ ≤ π/2. -or- <see cref="F:System.Double.NaN" /> if <paramref name="d" /> equals <see cref="F:System.Double.NaN" />, -π/2 rounded to double precision (-1.5707963267949) if <paramref name="d" /> equals <see cref="F:System.Double.NegativeInfinity" />, or π/2 rounded to double precision (1.5707963267949) if <paramref name="d" /> equals <see cref="F:System.Double.PositiveInfinity" />.</returns>
```

**成员**：static System.Math.Atanh(double)</br>
**签名**：_8093e8210867a45e</br>
**注释**：

```xml
<summary>Returns the angle whose hyperbolic tangent is the specified number.</summary>
<param name="d">A number representing a hyperbolic tangent, where <paramref name="d" /> must be greater than or equal to -1, but less than or equal to 1.</param>
<returns>An angle, θ, measured in radians, such that -∞ &lt; θ &lt; -1, or 1 &lt; θ &lt; ∞. -or- <see cref="F:System.Double.NaN" /> if <paramref name="d" /> &lt; -1 or <paramref name="d" /> &gt; 1 or <paramref name="d" /> equals <see cref="F:System.Double.NaN" />.</returns>
```

**成员**：static System.Math.Atan2(double, double)</br>
**签名**：_cc6b2bb857d27648</br>
**注释**：

```xml
<summary>Returns the angle whose tangent is the quotient of two specified numbers.</summary>
<param name="y">The y coordinate of a point.</param>
<param name="x">The x coordinate of a point.</param>
<returns>An angle, θ, measured in radians, such that tan(θ) = <paramref name="y" /> / <paramref name="x" />, where (<paramref name="x" />, <paramref name="y" />) is a point in the Cartesian plane. Observe the following:-   For (<paramref name="x" />, <paramref name="y" />) in quadrant 1, 0 &lt; θ &lt; π/2.-   For (<paramref name="x" />, <paramref name="y" />) in quadrant 2, π/2 &lt; θ ≤ π.-   For (<paramref name="x" />, <paramref name="y" />) in quadrant 3, -π ≤ θ &lt; -π/2.-   For (<paramref name="x" />, <paramref name="y" />) in quadrant 4, -π/2 &lt; θ &lt; 0. For points on the boundaries of the quadrants, the return value is the following:-   If y is 0 and x is not negative, θ = 0.-   If y is 0 and x is negative, θ = π.-   If y is positive and x is 0, θ = π/2.-   If y is negative and x is 0, θ = -π/2.-   If y is 0 and x is 0, θ = 0. If <paramref name="x" /> or <paramref name="y" /> is <see cref="F:System.Double.NaN" />, or if <paramref name="x" /> and <paramref name="y" /> are either <see cref="F:System.Double.PositiveInfinity" /> or <see cref="F:System.Double.NegativeInfinity" />, the method returns <see cref="F:System.Double.NaN" />.</returns>
```

**成员**：static System.Math.Cbrt(double)</br>
**签名**：_9369c8e8f81372b6</br>
**注释**：

```xml
<summary>Returns the cube root of a specified number.</summary>
<param name="d">The number whose cube root is to be found.</param>
<returns>The cube root of <paramref name="d" />. -or- <see cref="F:System.Double.NaN" /> if <paramref name="d" /> equals <see cref="F:System.Double.NaN" />.</returns>
```

**成员**：static System.Math.Ceiling(double)</br>
**签名**：_d7be7c95bfefd788</br>
**注释**：

```xml
<summary>Returns the smallest integral value that is greater than or equal to the specified double-precision floating-point number.</summary>
<param name="a">A double-precision floating-point number.</param>
<returns>The smallest integral value that is greater than or equal to <paramref name="a" />. If <paramref name="a" /> is equal to <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NegativeInfinity" />, or <see cref="F:System.Double.PositiveInfinity" />, that value is returned. Note that this method returns a <see cref="T:System.Double" /> instead of an integral type.</returns>
```

**成员**：static System.Math.Cos(double)</br>
**签名**：_b6b312cfcefe789c</br>
**注释**：

```xml
<summary>Returns the cosine of the specified angle.</summary>
<param name="d">An angle, measured in radians.</param>
<returns>The cosine of <paramref name="d" />. If <paramref name="d" /> is equal to <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NegativeInfinity" />, or <see cref="F:System.Double.PositiveInfinity" />, this method returns <see cref="F:System.Double.NaN" />.</returns>
```

**成员**：static System.Math.Cosh(double)</br>
**签名**：_c6f1b8664a086e13</br>
**注释**：

```xml
<summary>Returns the hyperbolic cosine of the specified angle.</summary>
<param name="value">An angle, measured in radians.</param>
<returns>The hyperbolic cosine of <paramref name="value" />. If <paramref name="value" /> is equal to <see cref="F:System.Double.NegativeInfinity" /> or <see cref="F:System.Double.PositiveInfinity" />, <see cref="F:System.Double.PositiveInfinity" /> is returned. If <paramref name="value" /> is equal to <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NaN" /> is returned.</returns>
```

**成员**：static System.Math.Exp(double)</br>
**签名**：_d5b39999cc90e482</br>
**注释**：

```xml
<summary>Returns <see langword="e" /> raised to the specified power.</summary>
<param name="d">A number specifying a power.</param>
<returns>The number <see langword="e" /> raised to the power <paramref name="d" />. If <paramref name="d" /> equals <see cref="F:System.Double.NaN" /> or <see cref="F:System.Double.PositiveInfinity" />, that value is returned. If <paramref name="d" /> equals <see cref="F:System.Double.NegativeInfinity" />, 0 is returned.</returns>
```

**成员**：static System.Math.Floor(double)</br>
**签名**：_a43200909dff4bc0</br>
**注释**：

```xml
<summary>Returns the largest integral value less than or equal to the specified double-precision floating-point number.</summary>
<param name="d">A double-precision floating-point number.</param>
<returns>The largest integral value less than or equal to <paramref name="d" />. If <paramref name="d" /> is equal to <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NegativeInfinity" />, or <see cref="F:System.Double.PositiveInfinity" />, that value is returned.</returns>
```

**成员**：static System.Math.FusedMultiplyAdd(double, double, double)</br>
**签名**：_52c95df2ad20c3bd</br>
**注释**：

```xml
<summary>Returns (x * y) + z, rounded as one ternary operation.</summary>
<param name="x">The number to be multiplied with <paramref name="y" />.</param>
<param name="y">The number to be multiplied with <paramref name="x" />.</param>
<param name="z">The number to be added to the result of <paramref name="x" /> multiplied by <paramref name="y" />.</param>
<returns>(x * y) + z, rounded as one ternary operation.</returns>
```

**成员**：static System.Math.Log(double)</br>
**签名**：_c65770c0fcbed4b6</br>
**注释**：

```xml
<summary>Returns the natural (base <see langword="e" />) logarithm of a specified number.</summary>
<param name="d">The number whose logarithm is to be found.</param>
<returns>One of the values in the following table. <list type="table"><listheader><term><paramref name="d" /> parameter</term><description> Return value</description></listheader><item><term> Positive</term><description> The natural logarithm of <paramref name="d" />; that is, ln <paramref name="d" />, or log e <paramref name="d" /></description></item><item><term> Zero</term><description><see cref="F:System.Double.NegativeInfinity" /></description></item><item><term> Negative</term><description><see cref="F:System.Double.NaN" /></description></item><item><term> Equal to <see cref="F:System.Double.NaN" /></term><description><see cref="F:System.Double.NaN" /></description></item><item><term> Equal to <see cref="F:System.Double.PositiveInfinity" /></term><description><see cref="F:System.Double.PositiveInfinity" /></description></item></list></returns>
```

**成员**：static System.Math.Log2(double)</br>
**签名**：_e622dc98a98720f4</br>
**注释**：

```xml
<summary>Returns the base 2 logarithm of a specified number.</summary>
<param name="x">A number whose logarithm is to be found.</param>
<returns>One of the values in the following table. <list type="table"><listheader><term><paramref name="x" /> parameter</term><description> Return value</description></listheader><item><term> Positive</term><description> The base 2 log of <paramref name="x" />; that is, log 2<paramref name="x" />.</description></item><item><term> Zero</term><description><see cref="F:System.Double.NegativeInfinity" /></description></item><item><term> Negative</term><description><see cref="F:System.Double.NaN" /></description></item><item><term> Equal to <see cref="F:System.Double.NaN" /></term><description><see cref="F:System.Double.NaN" /></description></item><item><term> Equal to <see cref="F:System.Double.PositiveInfinity" /></term><description><see cref="F:System.Double.PositiveInfinity" /></description></item></list></returns>
```

**成员**：static System.Math.Log10(double)</br>
**签名**：_a882de08086ccec9</br>
**注释**：

```xml
<summary>Returns the base 10 logarithm of a specified number.</summary>
<param name="d">A number whose logarithm is to be found.</param>
<returns>One of the values in the following table. <list type="table"><listheader><term><paramref name="d" /> parameter</term><description> Return value</description></listheader><item><term> Positive</term><description> The base 10 log of <paramref name="d" />; that is, log 10<paramref name="d" />.</description></item><item><term> Zero</term><description><see cref="F:System.Double.NegativeInfinity" /></description></item><item><term> Negative</term><description><see cref="F:System.Double.NaN" /></description></item><item><term> Equal to <see cref="F:System.Double.NaN" /></term><description><see cref="F:System.Double.NaN" /></description></item><item><term> Equal to <see cref="F:System.Double.PositiveInfinity" /></term><description><see cref="F:System.Double.PositiveInfinity" /></description></item></list></returns>
```

**成员**：static System.Math.Pow(double, double)</br>
**签名**：_fd439387b010bb99</br>
**注释**：

```xml
<summary>Returns a specified number raised to the specified power.</summary>
<param name="x">A double-precision floating-point number to be raised to a power.</param>
<param name="y">A double-precision floating-point number that specifies a power.</param>
<returns>The number <paramref name="x" /> raised to the power <paramref name="y" />.</returns>
```

**成员**：static System.Math.Sin(double)</br>
**签名**：_f1029100ea8114ab</br>
**注释**：

```xml
<summary>Returns the sine of the specified angle.</summary>
<param name="a">An angle, measured in radians.</param>
<returns>The sine of <paramref name="a" />. If <paramref name="a" /> is equal to <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NegativeInfinity" />, or <see cref="F:System.Double.PositiveInfinity" />, this method returns <see cref="F:System.Double.NaN" />.</returns>
```

**成员**：static System.Math.SinCos(double)</br>
**签名**：_4dcadff583296186</br>
**注释**：

```xml
<summary>Returns the sine and cosine of the specified angle.</summary>
<param name="x">An angle, measured in radians.</param>
<returns>The sine and cosine of <paramref name="x" />. If <paramref name="x" /> is equal to <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NegativeInfinity" />, or <see cref="F:System.Double.PositiveInfinity" />, this method returns <see cref="F:System.Double.NaN" />.</returns>
```

**成员**：static System.Math.Sinh(double)</br>
**签名**：_f48ae51bac192bdf</br>
**注释**：

```xml
<summary>Returns the hyperbolic sine of the specified angle.</summary>
<param name="value">An angle, measured in radians.</param>
<returns>The hyperbolic sine of <paramref name="value" />. If <paramref name="value" /> is equal to <see cref="F:System.Double.NegativeInfinity" />, <see cref="F:System.Double.PositiveInfinity" />, or <see cref="F:System.Double.NaN" />, this method returns a <see cref="T:System.Double" /> equal to <paramref name="value" />.</returns>
```

**成员**：static System.Math.Sqrt(double)</br>
**签名**：_b303f709d2b283f0</br>
**注释**：

```xml
<summary>Returns the square root of a specified number.</summary>
<param name="d">The number whose square root is to be found.</param>
<returns>One of the values in the following table. <list type="table"><listheader><term><paramref name="d" /> parameter</term><description> Return value</description></listheader><item><term> Zero or positive</term><description> The positive square root of <paramref name="d" />.</description></item><item><term> Negative</term><description><see cref="F:System.Double.NaN" /></description></item><item><term> Equals <see cref="F:System.Double.NaN" /></term><description><see cref="F:System.Double.NaN" /></description></item><item><term> Equals <see cref="F:System.Double.PositiveInfinity" /></term><description><see cref="F:System.Double.PositiveInfinity" /></description></item></list></returns>
```

**成员**：static System.Math.Tan(double)</br>
**签名**：_5f9763f3b0176663</br>
**注释**：

```xml
<summary>Returns the tangent of the specified angle.</summary>
<param name="a">An angle, measured in radians.</param>
<returns>The tangent of <paramref name="a" />. If <paramref name="a" /> is equal to <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NegativeInfinity" />, or <see cref="F:System.Double.PositiveInfinity" />, this method returns <see cref="F:System.Double.NaN" />.</returns>
```

**成员**：static System.Math.Tanh(double)</br>
**签名**：_d198ea5fec4f6c8a</br>
**注释**：

```xml
<summary>Returns the hyperbolic tangent of the specified angle.</summary>
<param name="value">An angle, measured in radians.</param>
<returns>The hyperbolic tangent of <paramref name="value" />. If <paramref name="value" /> is equal to <see cref="F:System.Double.NegativeInfinity" />, this method returns -1. If value is equal to <see cref="F:System.Double.PositiveInfinity" />, this method returns 1. If <paramref name="value" /> is equal to <see cref="F:System.Double.NaN" />, this method returns <see cref="F:System.Double.NaN" />.</returns>
```

**成员**：static System.Math.Abs(short)</br>
**签名**：_81a80e1bfb516bfb</br>
**注释**：

```xml
<summary>Returns the absolute value of a 16-bit signed integer.</summary>
<param name="value">A number that is greater than <see cref="F:System.Int16.MinValue">Int16.MinValue</see>, but less than or equal to <see cref="F:System.Int16.MaxValue">Int16.MaxValue</see>.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> equals <see cref="F:System.Int16.MinValue">Int16.MinValue</see>.</exception>
<returns>A 16-bit signed integer, x, such that 0 ≤ x ≤ <see cref="F:System.Int16.MaxValue">Int16.MaxValue</see>.</returns>
```

**成员**：static System.Math.Abs(int)</br>
**签名**：_0aaf1073fc70e405</br>
**注释**：

```xml
<summary>Returns the absolute value of a 32-bit signed integer.</summary>
<param name="value">A number that is greater than <see cref="F:System.Int32.MinValue">Int32.MinValue</see>, but less than or equal to <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> equals <see cref="F:System.Int32.MinValue">Int32.MinValue</see>.</exception>
<returns>A 32-bit signed integer, x, such that 0 ≤ x ≤ <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>.</returns>
```

**成员**：static System.Math.Abs(long)</br>
**签名**：_2f5b0b713dde9501</br>
**注释**：

```xml
<summary>Returns the absolute value of a 64-bit signed integer.</summary>
<param name="value">A number that is greater than <see cref="F:System.Int64.MinValue">Int64.MinValue</see>, but less than or equal to <see cref="F:System.Int64.MaxValue">Int64.MaxValue</see>.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> equals <see cref="F:System.Int64.MinValue">Int64.MinValue</see>.</exception>
<returns>A 64-bit signed integer, x, such that 0 ≤ x ≤ <see cref="F:System.Int64.MaxValue">Int64.MaxValue</see>.</returns>
```

**成员**：static System.Math.Abs(nint)</br>
**签名**：_6de080191221a07d</br>
**注释**：

```xml
<summary>Returns the absolute value of a native signed integer.</summary>
<param name="value">A number that is greater than <see cref="P:System.IntPtr.MinValue" />, but less than or equal to <see cref="P:System.IntPtr.MaxValue" />.</param>
<returns>A native signed integer, x, such that 0 ≤ x ≤ <see cref="P:System.IntPtr.MaxValue" />.</returns>
```

**成员**：static System.Math.Abs(sbyte)</br>
**签名**：_6ed2ee0733ac7051</br>
**注释**：

```xml
<summary>Returns the absolute value of an 8-bit signed integer.</summary>
<param name="value">A number that is greater than <see cref="F:System.SByte.MinValue">SByte.MinValue</see>, but less than or equal to <see cref="F:System.SByte.MaxValue">SByte.MaxValue</see>.</param>
<exception cref="T:System.OverflowException">  <paramref name="value" /> equals <see cref="F:System.SByte.MinValue">SByte.MinValue</see>.</exception>
<returns>An 8-bit signed integer, x, such that 0 ≤ x ≤ <see cref="F:System.SByte.MaxValue">SByte.MaxValue</see>.</returns>
```

**成员**：static System.Math.Abs(decimal)</br>
**签名**：_eab3564b2663dff6</br>
**注释**：

```xml
<summary>Returns the absolute value of a <see cref="T:System.Decimal" /> number.</summary>
<param name="value">A number that is greater than or equal to <see cref="F:System.Decimal.MinValue">Decimal.MinValue</see>, but less than or equal to <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</param>
<returns>A decimal number, x, such that 0 ≤ x ≤ <see cref="F:System.Decimal.MaxValue">Decimal.MaxValue</see>.</returns>
```

**成员**：static System.Math.Abs(double)</br>
**签名**：_6a0f94e87051cd5f</br>
**注释**：

```xml
<summary>Returns the absolute value of a double-precision floating-point number.</summary>
<param name="value">A number that is greater than or equal to <see cref="F:System.Double.MinValue">Double.MinValue</see>, but less than or equal to <see cref="F:System.Double.MaxValue">Double.MaxValue</see>.</param>
<returns>A double-precision floating-point number, x, such that 0 ≤ x ≤ <see cref="F:System.Double.MaxValue">Double.MaxValue</see>.</returns>
```

**成员**：static System.Math.Abs(float)</br>
**签名**：_3e86488d0112bcd3</br>
**注释**：

```xml
<summary>Returns the absolute value of a single-precision floating-point number.</summary>
<param name="value">A number that is greater than or equal to <see cref="F:System.Single.MinValue">Single.MinValue</see>, but less than or equal to <see cref="F:System.Single.MaxValue">Single.MaxValue</see>.</param>
<returns>A single-precision floating-point number, x, such that 0 ≤ x ≤ <see cref="F:System.Single.MaxValue">Single.MaxValue</see>.</returns>
```

**成员**：static System.Math.BigMul(uint, uint)</br>
**签名**：_6683ad6f7ac7c14c</br>
**注释**：

```xml
<summary>Produces the full product of two unsigned 32-bit numbers.</summary>
<param name="a">The first number to multiply.</param>
<param name="b">The second number to multiply.</param>
<returns>The number containing the product of the specified numbers.</returns>
```

**成员**：static System.Math.BigMul(int, int)</br>
**签名**：_f8dfabc9cf61c7c8</br>
**注释**：

```xml
<summary>Produces the full product of two 32-bit numbers.</summary>
<param name="a">The first number to multiply.</param>
<param name="b">The second number to multiply.</param>
<returns>The number containing the product of the specified numbers.</returns>
```

**成员**：static System.Math.BigMul(ulong, ulong, out ulong)</br>
**签名**：_99697fddb05f0646</br>
**注释**：

```xml
<summary>Produces the full product of two unsigned 64-bit numbers.</summary>
<param name="a">The first number to multiply.</param>
<param name="b">The second number to multiply.</param>
<param name="low">When this method returns, contains the low 64-bit of the product of the specified numbers.</param>
<returns>The high 64-bit of the product of the specified numbers.</returns>
```

**成员**：static System.Math.BigMul(long, long, out long)</br>
**签名**：_1f2b3fb549b0a774</br>
**注释**：

```xml
<summary>Produces the full product of two 64-bit numbers.</summary>
<param name="a">The first number to multiply.</param>
<param name="b">The second number to multiply.</param>
<param name="low">When this method returns, contains the low 64-bit of the product of the specified numbers.</param>
<returns>The high 64-bit of the product of the specified numbers.</returns>
```

**成员**：static System.Math.BigMul(ulong, ulong)</br>
**签名**：_d2fa7191b8139e97</br>
**注释**：

```xml
<summary>Produces the full product of two unsigned 64-bit numbers.</summary>
<param name="a">The first number to multiply.</param>
<param name="b">The second number to multiply.</param>
<returns>The full product of the specified numbers.</returns>
```

**成员**：static System.Math.BigMul(long, long)</br>
**签名**：_9eceeda3d33f938a</br>
**注释**：

```xml
<summary>Produces the full product of two 64-bit numbers.</summary>
<param name="a">The first number to multiply.</param>
<param name="b">The second number to multiply.</param>
<returns>The full product of the specified numbers.</returns>
```

**成员**：static System.Math.BitDecrement(double)</br>
**签名**：_bc28ec82e8385202</br>
**注释**：

```xml
<summary>Returns the largest value that compares less than a specified value.</summary>
<param name="x">The value to decrement.</param>
<returns>The largest value that compares less than <paramref name="x" />. -or- <see cref="F:System.Double.NegativeInfinity" /> if <paramref name="x" /> equals <see cref="F:System.Double.NegativeInfinity" />. -or- <see cref="F:System.Double.NaN" /> if <paramref name="x" /> equals <see cref="F:System.Double.NaN" />.</returns>
```

**成员**：static System.Math.BitIncrement(double)</br>
**签名**：_655bd4d428ca20ea</br>
**注释**：

```xml
<summary>Returns the smallest value that compares greater than a specified value.</summary>
<param name="x">The value to increment.</param>
<returns>The smallest value that compares greater than <paramref name="x" />. -or- <see cref="F:System.Double.PositiveInfinity" /> if <paramref name="x" /> equals <see cref="F:System.Double.PositiveInfinity" />. -or- <see cref="F:System.Double.NaN" /> if <paramref name="x" /> equals <see cref="F:System.Double.NaN" />.</returns>
```

**成员**：static System.Math.CopySign(double, double)</br>
**签名**：_f51bc6e5d8ce272b</br>
**注释**：

```xml
<summary>Returns a value with the magnitude of <paramref name="x" /> and the sign of <paramref name="y" />.</summary>
<param name="x">A number whose magnitude is used in the result.</param>
<param name="y">A number whose sign is the used in the result.</param>
<returns>A value with the magnitude of <paramref name="x" /> and the sign of <paramref name="y" />.</returns>
```

**成员**：static System.Math.DivRem(int, int, out int)</br>
**签名**：_2a90cb0f64781864</br>
**注释**：

```xml
<summary>Calculates the quotient of two 32-bit signed integers and also returns the remainder in an output parameter.</summary>
<param name="a">The dividend.</param>
<param name="b">The divisor.</param>
<param name="result">When this method returns, contains the remainder.</param>
<exception cref="T:System.DivideByZeroException">  <paramref name="b" /> is zero.</exception>
<returns>The quotient of the specified numbers.</returns>
```

**成员**：static System.Math.DivRem(long, long, out long)</br>
**签名**：_1961d3558bd76ea4</br>
**注释**：

```xml
<summary>Calculates the quotient of two 64-bit signed integers and also returns the remainder in an output parameter.</summary>
<param name="a">The dividend.</param>
<param name="b">The divisor.</param>
<param name="result">When this method returns, contains the remainder.</param>
<exception cref="T:System.DivideByZeroException">  <paramref name="b" /> is zero.</exception>
<returns>The quotient of the specified numbers.</returns>
```

**成员**：static System.Math.DivRem(sbyte, sbyte)</br>
**签名**：_e0661118fd9ce98d</br>
**注释**：

```xml
<summary>Produces the quotient and the remainder of two signed 8-bit numbers.</summary>
<param name="left">The dividend.</param>
<param name="right">The divisor.</param>
<returns>The quotient and the remainder of the specified numbers.</returns>
```

**成员**：static System.Math.DivRem(byte, byte)</br>
**签名**：_09ec2eababe53085</br>
**注释**：

```xml
<summary>Produces the quotient and the remainder of two unsigned 8-bit numbers.</summary>
<param name="left">The dividend.</param>
<param name="right">The divisor.</param>
<returns>The quotient and the remainder of the specified numbers.</returns>
```

**成员**：static System.Math.DivRem(short, short)</br>
**签名**：_f6eb115003bc623f</br>
**注释**：

```xml
<summary>Produces the quotient and the remainder of two signed 16-bit numbers.</summary>
<param name="left">The dividend.</param>
<param name="right">The divisor.</param>
<returns>The quotient and the remainder of the specified numbers.</returns>
```

**成员**：static System.Math.DivRem(ushort, ushort)</br>
**签名**：_267e04d7693208d4</br>
**注释**：

```xml
<summary>Produces the quotient and the remainder of two unsigned 16-bit numbers.</summary>
<param name="left">The dividend.</param>
<param name="right">The divisor.</param>
<returns>The quotient and the remainder of the specified numbers.</returns>
```

**成员**：static System.Math.DivRem(int, int)</br>
**签名**：_45a4ab35fd8b6be8</br>
**注释**：

```xml
<summary>Produces the quotient and the remainder of two signed 32-bit numbers.</summary>
<param name="left">The dividend.</param>
<param name="right">The divisor.</param>
<returns>The quotient and the remainder of the specified numbers.</returns>
```

**成员**：static System.Math.DivRem(uint, uint)</br>
**签名**：_c8e57fe110813408</br>
**注释**：

```xml
<summary>Produces the quotient and the remainder of two unsigned 32-bit numbers.</summary>
<param name="left">The dividend.</param>
<param name="right">The divisor.</param>
<returns>The quotient and the remainder of the specified numbers.</returns>
```

**成员**：static System.Math.DivRem(long, long)</br>
**签名**：_96f1b2c20bd2e40b</br>
**注释**：

```xml
<summary>Produces the quotient and the remainder of two signed 64-bit numbers.</summary>
<param name="left">The dividend.</param>
<param name="right">The divisor.</param>
<returns>The quotient and the remainder of the specified numbers.</returns>
```

**成员**：static System.Math.DivRem(ulong, ulong)</br>
**签名**：_4d9536a1220a7365</br>
**注释**：

```xml
<summary>Produces the quotient and the remainder of two unsigned 64-bit numbers.</summary>
<param name="left">The dividend.</param>
<param name="right">The divisor.</param>
<returns>The quotient and the remainder of the specified numbers.</returns>
```

**成员**：static System.Math.DivRem(nint, nint)</br>
**签名**：_98ac53eebed8e823</br>
**注释**：

```xml
<summary>Produces the quotient and the remainder of two signed native-size numbers.</summary>
<param name="left">The dividend.</param>
<param name="right">The divisor.</param>
<returns>The quotient and the remainder of the specified numbers.</returns>
```

**成员**：static System.Math.DivRem(nuint, nuint)</br>
**签名**：_1b2439f6e0d31865</br>
**注释**：

```xml
<summary>Produces the quotient and the remainder of two unsigned native-size numbers.</summary>
<param name="left">The dividend.</param>
<param name="right">The divisor.</param>
<returns>The quotient and the remainder of the specified numbers.</returns>
```

**成员**：static System.Math.Ceiling(decimal)</br>
**签名**：_84cbc0eaf2d899af</br>
**注释**：

```xml
<summary>Returns the smallest integral value that is greater than or equal to the specified decimal number.</summary>
<param name="d">A decimal number.</param>
<returns>The smallest integral value that is greater than or equal to <paramref name="d" />. Note that this method returns a <see cref="T:System.Decimal" /> instead of an integral type.</returns>
```

**成员**：static System.Math.Clamp(byte, byte, byte)</br>
**签名**：_8921213084b6685c</br>
**注释**：

```xml
<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
<param name="value">The value to be clamped.</param>
<param name="min">The lower bound of the result.</param>
<param name="max">The upper bound of the result.</param>
<returns>  <para>    <paramref name="value" /> if <paramref name="min" /> ≤ <paramref name="value" /> ≤ <paramref name="max" />.</para>  <para>-or-</para>  <para>    <paramref name="min" /> if <paramref name="value" /> &lt; <paramref name="min" />.</para>  <para>-or-</para>  <para>    <paramref name="max" /> if <paramref name="max" /> &lt; <paramref name="value" />.</para></returns>
```

**成员**：static System.Math.Clamp(decimal, decimal, decimal)</br>
**签名**：_735e24a467fce432</br>
**注释**：

```xml
<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
<param name="value">The value to be clamped.</param>
<param name="min">The lower bound of the result.</param>
<param name="max">The upper bound of the result.</param>
<returns>  <paramref name="value" /> if <paramref name="min" /> ≤ <paramref name="value" /> ≤ <paramref name="max" />. -or- <paramref name="min" /> if <paramref name="value" /> &lt;<paramref name="min" />. -or- <paramref name="max" /> if <paramref name="max" /> &lt; <paramref name="value" />.</returns>
```

**成员**：static System.Math.Clamp(double, double, double)</br>
**签名**：_a416f1414d77c0fa</br>
**注释**：

```xml
<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
<param name="value">The value to be clamped.</param>
<param name="min">The lower bound of the result.</param>
<param name="max">The upper bound of the result.</param>
<returns>  <paramref name="value" /> if <paramref name="min" /> ≤ <paramref name="value" /> ≤ <paramref name="max" />. -or- <paramref name="min" /> if <paramref name="value" /> &lt; <paramref name="min" />. -or- <paramref name="max" /> if <paramref name="max" /> &lt; <paramref name="value" />. -or-  <see cref="F:System.Double.NaN" /> if <paramref name="value" /> equals <see cref="F:System.Double.NaN" />.</returns>
```

**成员**：static System.Math.Clamp(short, short, short)</br>
**签名**：_86bd53ebc62ad520</br>
**注释**：

```xml
<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
<param name="value">The value to be clamped.</param>
<param name="min">The lower bound of the result.</param>
<param name="max">The upper bound of the result.</param>
<returns>  <paramref name="value" /> if <paramref name="min" /> ≤ <paramref name="value" /> ≤ <paramref name="max" />. -or- <paramref name="min" /> if <paramref name="value" /> &lt; <paramref name="min" />. -or- <paramref name="max" /> if <paramref name="max" /> &lt; <paramref name="value" />.</returns>
```

**成员**：static System.Math.Clamp(int, int, int)</br>
**签名**：_ac5962f496c6acc0</br>
**注释**：

```xml
<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
<param name="value">The value to be clamped.</param>
<param name="min">The lower bound of the result.</param>
<param name="max">The upper bound of the result.</param>
<returns>  <paramref name="value" /> if <paramref name="min" /> ≤ <paramref name="value" /> ≤ <paramref name="max" />. -or- <paramref name="min" /> if <paramref name="value" /> &lt; <paramref name="min" />. -or- <paramref name="max" /> if <paramref name="max" /> &lt; <paramref name="value" />.</returns>
```

**成员**：static System.Math.Clamp(long, long, long)</br>
**签名**：_d74b585d391b448a</br>
**注释**：

```xml
<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
<param name="value">The value to be clamped.</param>
<param name="min">The lower bound of the result.</param>
<param name="max">The upper bound of the result.</param>
<returns>  <paramref name="value" /> if <paramref name="min" /> ≤ <paramref name="value" /> ≤ <paramref name="max" />. -or- <paramref name="min" /> if <paramref name="value" /> &lt; <paramref name="min" />. -or- <paramref name="max" /> if <paramref name="max" /> &lt; <paramref name="value" />.</returns>
```

**成员**：static System.Math.Clamp(nint, nint, nint)</br>
**签名**：_63803d1734456eee</br>
**注释**：

```xml
<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
<param name="value">The value to be clamped.</param>
<param name="min">The lower bound of the result.</param>
<param name="max">The upper bound of the result.</param>
<returns>  <para>    <paramref name="value" /> if <paramref name="min" /> ≤ <paramref name="value" /> ≤ <paramref name="max" />.</para>  <para>-or-</para>  <para>    <paramref name="min" /> if <paramref name="value" /> &lt; <paramref name="min" />.</para>  <para>-or-</para>  <para>    <paramref name="max" /> if <paramref name="max" /> &lt; <paramref name="value" />.</para></returns>
```

**成员**：static System.Math.Clamp(sbyte, sbyte, sbyte)</br>
**签名**：_f2a0d82587b4e02a</br>
**注释**：

```xml
<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
<param name="value">The value to be clamped.</param>
<param name="min">The lower bound of the result.</param>
<param name="max">The upper bound of the result.</param>
<returns>  <paramref name="value" /> if <paramref name="min" /> ≤ <paramref name="value" /> ≤ <paramref name="max" />. -or- <paramref name="min" /> if <paramref name="value" /> &lt; <paramref name="min" />. -or- <paramref name="max" /> if <paramref name="max" /> &lt; <paramref name="value" />.</returns>
```

**成员**：static System.Math.Clamp(float, float, float)</br>
**签名**：_751a0e2d62df6aff</br>
**注释**：

```xml
<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
<param name="value">The value to be clamped.</param>
<param name="min">The lower bound of the result.</param>
<param name="max">The upper bound of the result.</param>
<returns>  <paramref name="value" /> if <paramref name="min" /> ≤ <paramref name="value" /> ≤ <paramref name="max" />. -or- <paramref name="min" /> if <paramref name="value" /> &lt; <paramref name="min" />. -or- <paramref name="max" /> if <paramref name="max" /> &lt; <paramref name="value" />. -or-  <see cref="F:System.Single.NaN" /> if <paramref name="value" /> equals <see cref="F:System.Single.NaN" />.</returns>
```

**成员**：static System.Math.Clamp(ushort, ushort, ushort)</br>
**签名**：_74d6735122ecb151</br>
**注释**：

```xml
<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
<param name="value">The value to be clamped.</param>
<param name="min">The lower bound of the result.</param>
<param name="max">The upper bound of the result.</param>
<returns>  <paramref name="value" /> if <paramref name="min" /> ≤ <paramref name="value" /> ≤ <paramref name="max" />. -or- <paramref name="min" /> if <paramref name="value" /> &lt; <paramref name="min" />. -or- <paramref name="max" /> if <paramref name="max" /> &lt; <paramref name="value" />.</returns>
```

**成员**：static System.Math.Clamp(uint, uint, uint)</br>
**签名**：_8322034639d6a05c</br>
**注释**：

```xml
<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
<param name="value">The value to be clamped.</param>
<param name="min">The lower bound of the result.</param>
<param name="max">The upper bound of the result.</param>
<returns>  <paramref name="value" /> if <paramref name="min" /> ≤ <paramref name="value" /> ≤ <paramref name="max" />. -or- <paramref name="min" /> if <paramref name="value" /> &lt; <paramref name="min" />. -or- <paramref name="max" /> if <paramref name="max" /> &lt; <paramref name="value" />.</returns>
```

**成员**：static System.Math.Clamp(ulong, ulong, ulong)</br>
**签名**：_f1743d6e0c7a2101</br>
**注释**：

```xml
<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
<param name="value">The value to be clamped.</param>
<param name="min">The lower bound of the result.</param>
<param name="max">The upper bound of the result.</param>
<returns>  <paramref name="value" /> if <paramref name="min" /> ≤ <paramref name="value" /> ≤ <paramref name="max" />. -or- <paramref name="min" /> if <paramref name="value" /> &lt; <paramref name="min" />. -or- <paramref name="max" /> if <paramref name="max" /> &lt; <paramref name="value" />.</returns>
```

**成员**：static System.Math.Clamp(nuint, nuint, nuint)</br>
**签名**：_25b262a1a57d5d06</br>
**注释**：

```xml
<summary>Returns <paramref name="value" /> clamped to the inclusive range of <paramref name="min" /> and <paramref name="max" />.</summary>
<param name="value">The value to be clamped.</param>
<param name="min">The lower bound of the result.</param>
<param name="max">The upper bound of the result.</param>
<returns>  <para>    <paramref name="value" /> if <paramref name="min" /> ≤ <paramref name="value" /> ≤ <paramref name="max" />.</para>  <para>-or-</para>  <para>    <paramref name="min" /> if <paramref name="value" /> &lt; <paramref name="min" />.</para>  <para>-or-</para>  <para>    <paramref name="max" /> if <paramref name="max" /> &lt; <paramref name="value" />.</para></returns>
```

**成员**：static System.Math.Floor(decimal)</br>
**签名**：_b12193a7b6647a82</br>
**注释**：

```xml
<summary>Returns the largest integral value less than or equal to the specified decimal number.</summary>
<param name="d">A decimal number.</param>
<returns>The largest integral value less than or equal to <paramref name="d" />.  Note that the method returns an integral value of type <see cref="T:System.Decimal" />.</returns>
```

**成员**：static System.Math.IEEERemainder(double, double)</br>
**签名**：_288c181b5d9cf968</br>
**注释**：

```xml
<summary>Returns the remainder resulting from the division of a specified number by another specified number.</summary>
<param name="x">A dividend.</param>
<param name="y">A divisor.</param>
<returns>A number equal to <paramref name="x" /> - (<paramref name="y" /> Q), where Q is the quotient of <paramref name="x" /> / <paramref name="y" /> rounded to the nearest integer (if <paramref name="x" /> / <paramref name="y" /> falls halfway between two integers, the even integer is returned). If <paramref name="x" /> - (<paramref name="y" /> Q) is zero, the value +0 is returned if <paramref name="x" /> is positive, or -0 if <paramref name="x" /> is negative. If <paramref name="y" /> = 0, <see cref="F:System.Double.NaN" /> is returned.</returns>
```

**成员**：static System.Math.ILogB(double)</br>
**签名**：_51e4d6005e6e11ef</br>
**注释**：

```xml
<summary>Returns the base 2 integer logarithm of a specified number.</summary>
<param name="x">The number whose logarithm is to be found.</param>
<returns>One of the values in the following table. <list type="table"><listheader><term><paramref name="x" /> parameter</term><description> Return value</description></listheader><item><term> Default</term><description> The base 2 integer log of <paramref name="x" />; that is, (int)log2(<paramref name="x" />).</description></item><item><term> Zero</term><description><see cref="F:System.Int32.MinValue">Int32.MinValue</see></description></item><item><term> Equal to <see cref="F:System.Double.NaN" /> or <see cref="F:System.Double.PositiveInfinity" /> or <see cref="F:System.Double.NegativeInfinity" /></term><description><see cref="F:System.Int32.MaxValue">Int32.MaxValue</see></description></item></list></returns>
```

**成员**：static System.Math.Log(double, double)</br>
**签名**：_da091a35a0d7bc64</br>
**注释**：

```xml
<summary>Returns the logarithm of a specified number in a specified base.</summary>
<param name="a">The number whose logarithm is to be found.</param>
<param name="newBase">The base of the logarithm.</param>
<returns>One of the values in the following table. (+Infinity denotes <see cref="F:System.Double.PositiveInfinity" />, -Infinity denotes <see cref="F:System.Double.NegativeInfinity" />, and NaN denotes <see cref="F:System.Double.NaN" />.) <list type="table"><listheader><term><paramref name="a" /></term><description><paramref name="newBase" /></description><description> Return value</description></listheader><item><term><paramref name="a" />&gt; 0</term><description> (0 &lt;<paramref name="newBase" />&lt; 1) -or- (<paramref name="newBase" />&gt; 1)</description><description> lognewBase(a)</description></item><item><term><paramref name="a" />&lt; 0</term><description> (any value)</description><description> NaN</description></item><item><term> (any value)</term><description><paramref name="newBase" />&lt; 0</description><description> NaN</description></item><item><term><paramref name="a" /> != 1</term><description><paramref name="newBase" /> = 0</description><description> NaN</description></item><item><term><paramref name="a" /> != 1</term><description><paramref name="newBase" /> = +Infinity</description><description> NaN</description></item><item><term><paramref name="a" /> = NaN</term><description> (any value)</description><description> NaN</description></item><item><term> (any value)</term><description><paramref name="newBase" /> = NaN</description><description> NaN</description></item><item><term> (any value)</term><description><paramref name="newBase" /> = 1</description><description> NaN</description></item><item><term><paramref name="a" /> = 0</term><description> 0 &lt;<paramref name="newBase" />&lt; 1</description><description> +Infinity</description></item><item><term><paramref name="a" /> = 0</term><description><paramref name="newBase" />&gt; 1</description><description> -Infinity</description></item><item><term><paramref name="a" /> =  +Infinity</term><description> 0 &lt;<paramref name="newBase" />&lt; 1</description><description> -Infinity</description></item><item><term><paramref name="a" /> =  +Infinity</term><description><paramref name="newBase" />&gt; 1</description><description> +Infinity</description></item><item><term><paramref name="a" /> = 1</term><description><paramref name="newBase" /> = 0</description><description> 0</description></item><item><term><paramref name="a" /> = 1</term><description><paramref name="newBase" /> = +Infinity</description><description> 0</description></item></list></returns>
```

**成员**：static System.Math.Max(byte, byte)</br>
**签名**：_a26e415f31a1dd41</br>
**注释**：

```xml
<summary>Returns the larger of two 8-bit unsigned integers.</summary>
<param name="val1">The first of two 8-bit unsigned integers to compare.</param>
<param name="val2">The second of two 8-bit unsigned integers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is larger.</returns>
```

**成员**：static System.Math.Max(decimal, decimal)</br>
**签名**：_68326de2fcd99278</br>
**注释**：

```xml
<summary>Returns the larger of two decimal numbers.</summary>
<param name="val1">The first of two decimal numbers to compare.</param>
<param name="val2">The second of two decimal numbers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is larger.</returns>
```

**成员**：static System.Math.Max(double, double)</br>
**签名**：_1bcd36ee2d1a5261</br>
**注释**：

```xml
<summary>Returns the larger of two double-precision floating-point numbers.</summary>
<param name="val1">The first of two double-precision floating-point numbers to compare.</param>
<param name="val2">The second of two double-precision floating-point numbers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is larger. If <paramref name="val1" />, <paramref name="val2" />, or both <paramref name="val1" /> and <paramref name="val2" /> are equal to <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NaN" /> is returned.</returns>
```

**成员**：static System.Math.Max(short, short)</br>
**签名**：_52a2dcd88692950d</br>
**注释**：

```xml
<summary>Returns the larger of two 16-bit signed integers.</summary>
<param name="val1">The first of two 16-bit signed integers to compare.</param>
<param name="val2">The second of two 16-bit signed integers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is larger.</returns>
```

**成员**：static System.Math.Max(int, int)</br>
**签名**：_c89f0321e6ece69a</br>
**注释**：

```xml
<summary>Returns the larger of two 32-bit signed integers.</summary>
<param name="val1">The first of two 32-bit signed integers to compare.</param>
<param name="val2">The second of two 32-bit signed integers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is larger.</returns>
```

**成员**：static System.Math.Max(long, long)</br>
**签名**：_1513b88bb1abfff1</br>
**注释**：

```xml
<summary>Returns the larger of two 64-bit signed integers.</summary>
<param name="val1">The first of two 64-bit signed integers to compare.</param>
<param name="val2">The second of two 64-bit signed integers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is larger.</returns>
```

**成员**：static System.Math.Max(nint, nint)</br>
**签名**：_c03baee2a94d0113</br>
**注释**：

```xml
<summary>Returns the larger of two native signed integers.</summary>
<param name="val1">The first of two native signed integers to compare.</param>
<param name="val2">The second of two native signed integers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is larger.</returns>
```

**成员**：static System.Math.Max(sbyte, sbyte)</br>
**签名**：_cb1537d45a143e0d</br>
**注释**：

```xml
<summary>Returns the larger of two 8-bit signed integers.</summary>
<param name="val1">The first of two 8-bit signed integers to compare.</param>
<param name="val2">The second of two 8-bit signed integers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is larger.</returns>
```

**成员**：static System.Math.Max(float, float)</br>
**签名**：_5acf698f9a9ada61</br>
**注释**：

```xml
<summary>Returns the larger of two single-precision floating-point numbers.</summary>
<param name="val1">The first of two single-precision floating-point numbers to compare.</param>
<param name="val2">The second of two single-precision floating-point numbers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is larger. If <paramref name="val1" />, or <paramref name="val2" />, or both <paramref name="val1" /> and <paramref name="val2" /> are equal to <see cref="F:System.Single.NaN" />, <see cref="F:System.Single.NaN" /> is returned.</returns>
```

**成员**：static System.Math.Max(ushort, ushort)</br>
**签名**：_07de56d6927ee6af</br>
**注释**：

```xml
<summary>Returns the larger of two 16-bit unsigned integers.</summary>
<param name="val1">The first of two 16-bit unsigned integers to compare.</param>
<param name="val2">The second of two 16-bit unsigned integers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is larger.</returns>
```

**成员**：static System.Math.Max(uint, uint)</br>
**签名**：_6638c647001d2908</br>
**注释**：

```xml
<summary>Returns the larger of two 32-bit unsigned integers.</summary>
<param name="val1">The first of two 32-bit unsigned integers to compare.</param>
<param name="val2">The second of two 32-bit unsigned integers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is larger.</returns>
```

**成员**：static System.Math.Max(ulong, ulong)</br>
**签名**：_3ac884b966eeb605</br>
**注释**：

```xml
<summary>Returns the larger of two 64-bit unsigned integers.</summary>
<param name="val1">The first of two 64-bit unsigned integers to compare.</param>
<param name="val2">The second of two 64-bit unsigned integers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is larger.</returns>
```

**成员**：static System.Math.Max(nuint, nuint)</br>
**签名**：_7f3becc9b24d51d3</br>
**注释**：

```xml
<summary>Returns the larger of two native unsigned integers.</summary>
<param name="val1">The first of two native unsigned integers to compare.</param>
<param name="val2">The second of two native unsigned integers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is larger.</returns>
```

**成员**：static System.Math.MaxMagnitude(double, double)</br>
**签名**：_7922e74207558715</br>
**注释**：

```xml
<summary>Returns the larger magnitude of two double-precision floating-point numbers.</summary>
<param name="x">The first of two double-precision floating-point numbers to compare.</param>
<param name="y">The second of two double-precision floating-point numbers to compare.</param>
<returns>Parameter <paramref name="x" /> or <paramref name="y" />, whichever has the larger magnitude. If <paramref name="x" />, or <paramref name="y" />, or both <paramref name="x" /> and <paramref name="y" /> are equal to <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NaN" /> is returned.</returns>
```

**成员**：static System.Math.Min(byte, byte)</br>
**签名**：_f8806316e956dbb8</br>
**注释**：

```xml
<summary>Returns the smaller of two 8-bit unsigned integers.</summary>
<param name="val1">The first of two 8-bit unsigned integers to compare.</param>
<param name="val2">The second of two 8-bit unsigned integers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is smaller.</returns>
```

**成员**：static System.Math.Min(decimal, decimal)</br>
**签名**：_87f14d6593efd87f</br>
**注释**：

```xml
<summary>Returns the smaller of two decimal numbers.</summary>
<param name="val1">The first of two decimal numbers to compare.</param>
<param name="val2">The second of two decimal numbers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is smaller.</returns>
```

**成员**：static System.Math.Min(double, double)</br>
**签名**：_d0d428d1a1f7d899</br>
**注释**：

```xml
<summary>Returns the smaller of two double-precision floating-point numbers.</summary>
<param name="val1">The first of two double-precision floating-point numbers to compare.</param>
<param name="val2">The second of two double-precision floating-point numbers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is smaller. If <paramref name="val1" />, <paramref name="val2" />, or both <paramref name="val1" /> and <paramref name="val2" /> are equal to <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NaN" /> is returned.</returns>
```

**成员**：static System.Math.Min(short, short)</br>
**签名**：_d7a779b3283b34dc</br>
**注释**：

```xml
<summary>Returns the smaller of two 16-bit signed integers.</summary>
<param name="val1">The first of two 16-bit signed integers to compare.</param>
<param name="val2">The second of two 16-bit signed integers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is smaller.</returns>
```

**成员**：static System.Math.Min(int, int)</br>
**签名**：_7fb229bda6fa1941</br>
**注释**：

```xml
<summary>Returns the smaller of two 32-bit signed integers.</summary>
<param name="val1">The first of two 32-bit signed integers to compare.</param>
<param name="val2">The second of two 32-bit signed integers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is smaller.</returns>
```

**成员**：static System.Math.Min(long, long)</br>
**签名**：_b98fea9bd3e4ce52</br>
**注释**：

```xml
<summary>Returns the smaller of two 64-bit signed integers.</summary>
<param name="val1">The first of two 64-bit signed integers to compare.</param>
<param name="val2">The second of two 64-bit signed integers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is smaller.</returns>
```

**成员**：static System.Math.Min(nint, nint)</br>
**签名**：_e3cdc59c4e2b3f04</br>
**注释**：

```xml
<summary>Returns the smaller of two native signed integers.</summary>
<param name="val1">The first of two native signed integers to compare.</param>
<param name="val2">The second of two native signed integers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is smaller.</returns>
```

**成员**：static System.Math.Min(sbyte, sbyte)</br>
**签名**：_0f8bf59fee331622</br>
**注释**：

```xml
<summary>Returns the smaller of two 8-bit signed integers.</summary>
<param name="val1">The first of two 8-bit signed integers to compare.</param>
<param name="val2">The second of two 8-bit signed integers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is smaller.</returns>
```

**成员**：static System.Math.Min(float, float)</br>
**签名**：_2c1e93a158a72838</br>
**注释**：

```xml
<summary>Returns the smaller of two single-precision floating-point numbers.</summary>
<param name="val1">The first of two single-precision floating-point numbers to compare.</param>
<param name="val2">The second of two single-precision floating-point numbers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is smaller. If <paramref name="val1" />, <paramref name="val2" />, or both <paramref name="val1" /> and <paramref name="val2" /> are equal to <see cref="F:System.Single.NaN" />, <see cref="F:System.Single.NaN" /> is returned.</returns>
```

**成员**：static System.Math.Min(ushort, ushort)</br>
**签名**：_3e853af2da5fd862</br>
**注释**：

```xml
<summary>Returns the smaller of two 16-bit unsigned integers.</summary>
<param name="val1">The first of two 16-bit unsigned integers to compare.</param>
<param name="val2">The second of two 16-bit unsigned integers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is smaller.</returns>
```

**成员**：static System.Math.Min(uint, uint)</br>
**签名**：_849b5d874239b92c</br>
**注释**：

```xml
<summary>Returns the smaller of two 32-bit unsigned integers.</summary>
<param name="val1">The first of two 32-bit unsigned integers to compare.</param>
<param name="val2">The second of two 32-bit unsigned integers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is smaller.</returns>
```

**成员**：static System.Math.Min(ulong, ulong)</br>
**签名**：_d468e999912e1120</br>
**注释**：

```xml
<summary>Returns the smaller of two 64-bit unsigned integers.</summary>
<param name="val1">The first of two 64-bit unsigned integers to compare.</param>
<param name="val2">The second of two 64-bit unsigned integers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is smaller.</returns>
```

**成员**：static System.Math.Min(nuint, nuint)</br>
**签名**：_c03fe2f175939d3a</br>
**注释**：

```xml
<summary>Returns the smaller of two native unsigned integers.</summary>
<param name="val1">The first of two native unsigned integers to compare.</param>
<param name="val2">The second of two native unsigned integers to compare.</param>
<returns>Parameter <paramref name="val1" /> or <paramref name="val2" />, whichever is smaller.</returns>
```

**成员**：static System.Math.MinMagnitude(double, double)</br>
**签名**：_44776725ec896ede</br>
**注释**：

```xml
<summary>Returns the smaller magnitude of two double-precision floating-point numbers.</summary>
<param name="x">The first of two double-precision floating-point numbers to compare.</param>
<param name="y">The second of two double-precision floating-point numbers to compare.</param>
<returns>Parameter <paramref name="x" /> or <paramref name="y" />, whichever has the smaller magnitude. If <paramref name="x" />, or <paramref name="y" />, or both <paramref name="x" /> and <paramref name="y" /> are equal to <see cref="F:System.Double.NaN" />, <see cref="F:System.Double.NaN" /> is returned.</returns>
```

**成员**：static System.Math.ReciprocalEstimate(double)</br>
**签名**：_63ae085718e46139</br>
**注释**：

```xml
<summary>Returns an estimate of the reciprocal of a specified number.</summary>
<param name="d">The number whose reciprocal is to be estimated.</param>
<returns>An estimate of the reciprocal of <paramref name="d" />.</returns>
```

**成员**：static System.Math.ReciprocalSqrtEstimate(double)</br>
**签名**：_5ab45aaeb89fbf4c</br>
**注释**：

```xml
<summary>Returns an estimate of the reciprocal square root of a specified number.</summary>
<param name="d">The number whose reciprocal square root is to be estimated.</param>
<returns>An estimate of the reciprocal square root <paramref name="d" />.</returns>
```

**成员**：static System.Math.Round(decimal)</br>
**签名**：_257741f3e4260d82</br>
**注释**：

```xml
<summary>Rounds a decimal value to the nearest integral value, and rounds midpoint values to the nearest even number.</summary>
<param name="d">A decimal number to be rounded.</param>
<exception cref="T:System.OverflowException">The result is outside the range of a <see cref="T:System.Decimal" />.</exception>
<returns>The integer nearest the <paramref name="d" /> parameter. If the fractional component of <paramref name="d" /> is halfway between two integers, one of which is even and the other odd, the even number is returned. Note that this method returns a <see cref="T:System.Decimal" /> instead of an integral type.</returns>
```

**成员**：static System.Math.Round(decimal, int)</br>
**签名**：_10e883cf6d89b70c</br>
**注释**：

```xml
<summary>Rounds a decimal value to a specified number of fractional digits, and rounds midpoint values to the nearest even number.</summary>
<param name="d">A decimal number to be rounded.</param>
<param name="decimals">The number of decimal places in the return value.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="decimals" /> is less than 0 or greater than 28.</exception>
<exception cref="T:System.OverflowException">The result is outside the range of a <see cref="T:System.Decimal" />.</exception>
<returns>The number nearest to <paramref name="d" /> that contains a number of fractional digits equal to <paramref name="decimals" />.</returns>
```

**成员**：static System.Math.Round(decimal, System.MidpointRounding)</br>
**签名**：_584a7b2219b578fa</br>
**注释**：

```xml
<summary>Rounds a decimal value an integer using the specified rounding convention.</summary>
<param name="d">A decimal number to be rounded.</param>
<param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>
<exception cref="T:System.ArgumentException">  <paramref name="mode" /> is not a valid value of <see cref="T:System.MidpointRounding" />.</exception>
<exception cref="T:System.OverflowException">The result is outside the range of a <see cref="T:System.Decimal" />.</exception>
<returns>The integer that <paramref name="d" /> is rounded to. This method returns a <see cref="T:System.Decimal" /> instead of an integral type.</returns>
```

**成员**：static System.Math.Round(decimal, int, System.MidpointRounding)</br>
**签名**：_b955eff4c2d1fa63</br>
**注释**：

```xml
<summary>Rounds a decimal value to a specified number of fractional digits using the specified rounding convention.</summary>
<param name="d">A decimal number to be rounded.</param>
<param name="decimals">The number of decimal places in the return value.</param>
<param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="decimals" /> is less than 0 or greater than 28.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="mode" /> is not a valid value of <see cref="T:System.MidpointRounding" />.</exception>
<exception cref="T:System.OverflowException">The result is outside the range of a <see cref="T:System.Decimal" />.</exception>
<returns>The number with <paramref name="decimals" /> fractional digits that <paramref name="d" /> is rounded to. If <paramref name="d" /> has fewer fractional digits than <paramref name="decimals" />, <paramref name="d" /> is returned unchanged.</returns>
```

**成员**：static System.Math.Round(double)</br>
**签名**：_6cd7f67f98eae0bc</br>
**注释**：

```xml
<summary>Rounds a double-precision floating-point value to the nearest integral value, and rounds midpoint values to the nearest even number.</summary>
<param name="a">A double-precision floating-point number to be rounded.</param>
<returns>The integer nearest <paramref name="a" />. If the fractional component of <paramref name="a" /> is halfway between two integers, one of which is even and the other odd, then the even number is returned. Note that this method returns a <see cref="T:System.Double" /> instead of an integral type.</returns>
```

**成员**：static System.Math.Round(double, int)</br>
**签名**：_dab059b61a5b7428</br>
**注释**：

```xml
<summary>Rounds a double-precision floating-point value to a specified number of fractional digits, and rounds midpoint values to the nearest even number.</summary>
<param name="value">A double-precision floating-point number to be rounded.</param>
<param name="digits">The number of fractional digits in the return value.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="digits" /> is less than 0 or greater than 15.</exception>
<returns>The number nearest to <paramref name="value" /> that contains a number of fractional digits equal to <paramref name="digits" />.</returns>
```

**成员**：static System.Math.Round(double, System.MidpointRounding)</br>
**签名**：_a7f99c51d0db12b5</br>
**注释**：

```xml
<summary>Rounds a double-precision floating-point value to an integer using the specified rounding convention.</summary>
<param name="value">A double-precision floating-point number to be rounded.</param>
<param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>
<exception cref="T:System.ArgumentException">  <paramref name="mode" /> is not a valid value of <see cref="T:System.MidpointRounding" />.</exception>
<returns>The integer that <paramref name="value" /> is rounded to. This method returns a <see cref="T:System.Double" /> instead of an integral type.</returns>
```

**成员**：static System.Math.Round(double, int, System.MidpointRounding)</br>
**签名**：_ef441dda2abcc022</br>
**注释**：

```xml
<summary>Rounds a double-precision floating-point value to a specified number of fractional digits using the specified rounding convention.</summary>
<param name="value">A double-precision floating-point number to be rounded.</param>
<param name="digits">The number of fractional digits in the return value.</param>
<param name="mode">One of the enumeration values that specifies which rounding strategy to use.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="digits" /> is less than 0 or greater than 15.</exception>
<exception cref="T:System.ArgumentException">  <paramref name="mode" /> is not a valid value of <see cref="T:System.MidpointRounding" />.</exception>
<returns>The number that has <paramref name="digits" /> fractional digits that <paramref name="value" /> is rounded to. If <paramref name="value" /> has fewer fractional digits than <paramref name="digits" />, <paramref name="value" /> is returned unchanged.</returns>
```

**成员**：static System.Math.Sign(decimal)</br>
**签名**：_8d626104a531d041</br>
**注释**：

```xml
<summary>Returns an integer that indicates the sign of a decimal number.</summary>
<param name="value">A signed decimal number.</param>
<returns>A number that indicates the sign of <paramref name="value" />, as shown in the following table. <list type="table"><listheader><term> Return value</term><description> Meaning</description></listheader><item><term> -1</term><description><paramref name="value" /> is less than zero.</description></item><item><term> 0</term><description><paramref name="value" /> is equal to zero.</description></item><item><term> 1</term><description><paramref name="value" /> is greater than zero.</description></item></list></returns>
```

**成员**：static System.Math.Sign(double)</br>
**签名**：_9a554cfca79bdc59</br>
**注释**：

```xml
<summary>Returns an integer that indicates the sign of a double-precision floating-point number.</summary>
<param name="value">A signed number.</param>
<exception cref="T:System.ArithmeticException">  <paramref name="value" /> is equal to <see cref="F:System.Double.NaN" />.</exception>
<returns>A number that indicates the sign of <paramref name="value" />, as shown in the following table. <list type="table"><listheader><term> Return value</term><description> Meaning</description></listheader><item><term> -1</term><description><paramref name="value" /> is less than zero.</description></item><item><term> 0</term><description><paramref name="value" /> is equal to zero.</description></item><item><term> 1</term><description><paramref name="value" /> is greater than zero.</description></item></list></returns>
```

**成员**：static System.Math.Sign(short)</br>
**签名**：_f8eefd9c948ed90a</br>
**注释**：

```xml
<summary>Returns an integer that indicates the sign of a 16-bit signed integer.</summary>
<param name="value">A signed number.</param>
<returns>A number that indicates the sign of <paramref name="value" />, as shown in the following table. <list type="table"><listheader><term> Return value</term><description> Meaning</description></listheader><item><term> -1</term><description><paramref name="value" /> is less than zero.</description></item><item><term> 0</term><description><paramref name="value" /> is equal to zero.</description></item><item><term> 1</term><description><paramref name="value" /> is greater than zero.</description></item></list></returns>
```

**成员**：static System.Math.Sign(int)</br>
**签名**：_cfeb8757509066b2</br>
**注释**：

```xml
<summary>Returns an integer that indicates the sign of a 32-bit signed integer.</summary>
<param name="value">A signed number.</param>
<returns>A number that indicates the sign of <paramref name="value" />, as shown in the following table. <list type="table"><listheader><term> Return value</term><description> Meaning</description></listheader><item><term> -1</term><description><paramref name="value" /> is less than zero.</description></item><item><term> 0</term><description><paramref name="value" /> is equal to zero.</description></item><item><term> 1</term><description><paramref name="value" /> is greater than zero.</description></item></list></returns>
```

**成员**：static System.Math.Sign(long)</br>
**签名**：_5354f93121b296ff</br>
**注释**：

```xml
<summary>Returns an integer that indicates the sign of a 64-bit signed integer.</summary>
<param name="value">A signed number.</param>
<returns>A number that indicates the sign of <paramref name="value" />, as shown in the following table. <list type="table"><listheader><term> Return value</term><description> Meaning</description></listheader><item><term> -1</term><description><paramref name="value" /> is less than zero.</description></item><item><term> 0</term><description><paramref name="value" /> is equal to zero.</description></item><item><term> 1</term><description><paramref name="value" /> is greater than zero.</description></item></list></returns>
```

**成员**：static System.Math.Sign(nint)</br>
**签名**：_e5d5397dfe870f94</br>
**注释**：

```xml
<summary>Returns an integer that indicates the sign of a native sized signed integer.</summary>
<param name="value">A signed number.</param>
<returns>A number that indicates the sign of <paramref name="value" />, as shown in the following table. <list type="table"><listheader><term> Return value</term><description> Meaning</description></listheader><item><term> -1</term><description><paramref name="value" /> is less than zero.</description></item><item><term> 0</term><description><paramref name="value" /> is equal to zero.</description></item><item><term> 1</term><description><paramref name="value" /> is greater than zero.</description></item></list></returns>
```

**成员**：static System.Math.Sign(sbyte)</br>
**签名**：_88575fe160876695</br>
**注释**：

```xml
<summary>Returns an integer that indicates the sign of an 8-bit signed integer.</summary>
<param name="value">A signed number.</param>
<returns>A number that indicates the sign of <paramref name="value" />, as shown in the following table. <list type="table"><listheader><term> Return value</term><description> Meaning</description></listheader><item><term> -1</term><description><paramref name="value" /> is less than zero.</description></item><item><term> 0</term><description><paramref name="value" /> is equal to zero.</description></item><item><term> 1</term><description><paramref name="value" /> is greater than zero.</description></item></list></returns>
```

**成员**：static System.Math.Sign(float)</br>
**签名**：_c0668680ba7ef96e</br>
**注释**：

```xml
<summary>Returns an integer that indicates the sign of a single-precision floating-point number.</summary>
<param name="value">A signed number.</param>
<exception cref="T:System.ArithmeticException">  <paramref name="value" /> is equal to <see cref="F:System.Single.NaN" />.</exception>
<returns>A number that indicates the sign of <paramref name="value" />, as shown in the following table. <list type="table"><listheader><term> Return value</term><description> Meaning</description></listheader><item><term> -1</term><description><paramref name="value" /> is less than zero.</description></item><item><term> 0</term><description><paramref name="value" /> is equal to zero.</description></item><item><term> 1</term><description><paramref name="value" /> is greater than zero.</description></item></list></returns>
```

**成员**：static System.Math.Truncate(decimal)</br>
**签名**：_abd9211e1e7514b4</br>
**注释**：

```xml
<summary>Calculates the integral part of a specified decimal number.</summary>
<param name="d">A number to truncate.</param>
<returns>The integral part of <paramref name="d" />; that is, the number that remains after any fractional digits have been discarded.</returns>
```

**成员**：static System.Math.Truncate(double)</br>
**签名**：_b74eaf879a3b5fd7</br>
**注释**：

```xml
<summary>Calculates the integral part of a specified double-precision floating-point number.</summary>
<param name="d">A number to truncate.</param>
<returns>The integral part of <paramref name="d" />; that is, the number that remains after any fractional digits have been discarded, or one of the values listed in the following table. <list type="table"><listheader><term><paramref name="d" /></term><description> Return value</description></listheader><item><term><see cref="F:System.Double.NaN" /></term><description><see cref="F:System.Double.NaN" /></description></item><item><term><see cref="F:System.Double.NegativeInfinity" /></term><description><see cref="F:System.Double.NegativeInfinity" /></description></item><item><term><see cref="F:System.Double.PositiveInfinity" /></term><description><see cref="F:System.Double.PositiveInfinity" /></description></item></list></returns>
```

**成员**：static System.Math.ScaleB(double, int)</br>
**签名**：_11ce4194425195ad</br>
**注释**：

```xml
<summary>Returns x * 2^n computed efficiently.</summary>
<param name="x">A double-precision floating-point number that specifies the base value.</param>
<param name="n">A 32-bit integer that specifies the power.</param>
<returns>x * 2^n computed efficiently.</returns>
```

