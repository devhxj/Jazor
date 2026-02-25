# CharModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：char.Char()</br>
**签名**：_920bd6d3d675c7b2</br>

**成员**：static char.IsAscii(char)</br>
**签名**：_39826354b8bd0f55</br>
**注释**：

```xml
<summary>Returns <see langword="true" /> if <paramref name="c" /> is an ASCII character ([ U+0000..U+007F ]).</summary>
<param name="c">The character to analyze.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is an ASCII character; <see langword="false" /> otherwise.</returns>
```

**成员**：override char.GetHashCode()</br>
**签名**：_5b81ebfb78d5415c</br>
**注释**：

```xml
<summary>Returns the hash code for this instance.</summary>
<returns>A 32-bit signed integer hash code.</returns>
```

**成员**：override char.Equals(object)</br>
**签名**：_3f176ca2992b307c</br>
**注释**：

```xml
<summary>Returns a value that indicates whether this instance is equal to a specified object.</summary>
<param name="obj">An object to compare with this instance or <see langword="null" />.</param>
<returns>  <see langword="true" /> if <paramref name="obj" /> is an instance of <see cref="T:System.Char" /> and equals the value of this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：char.Equals(char)</br>
**签名**：_632690bee0e71964</br>
**注释**：

```xml
<summary>Returns a value that indicates whether this instance is equal to the specified <see cref="T:System.Char" /> object.</summary>
<param name="obj">An object to compare to this instance.</param>
<returns>  <see langword="true" /> if the <paramref name="obj" /> parameter equals the value of this instance; otherwise, <see langword="false" />.</returns>
```

**成员**：char.CompareTo(object)</br>
**签名**：_ddf9c5affdc041df</br>
**注释**：

```xml
<summary>Compares this instance to a specified object and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified <see cref="T:System.Object" />.</summary>
<param name="value">An object to compare this instance to, or <see langword="null" />.</param>
<exception cref="T:System.ArgumentException">  <paramref name="value" /> is not a <see cref="T:System.Char" /> object.</exception>
<returns>A signed number indicating the position of this instance in the sort order in relation to the <paramref name="value" /> parameter. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance has the same position in the sort order as <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="value" />. -or- <paramref name="value" /> is <see langword="null" />.</description></item></list></returns>
```

**成员**：char.CompareTo(char)</br>
**签名**：_309d33b86c3815d8</br>
**注释**：

```xml
<summary>Compares this instance to a specified <see cref="T:System.Char" /> object and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified <see cref="T:System.Char" /> object.</summary>
<param name="value">A <see cref="T:System.Char" /> object to compare.</param>
<returns>A signed number indicating the position of this instance in the sort order in relation to the <paramref name="value" /> parameter. <list type="table"><listheader><term> Return Value</term><description> Description</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance has the same position in the sort order as <paramref name="value" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="value" />.</description></item></list></returns>
```

**成员**：override char.ToString()</br>
**签名**：_4861ba21870a2ec3</br>
**注释**：

```xml
<summary>Converts the value of this instance to its equivalent string representation.</summary>
<returns>The string representation of the value of this instance.</returns>
```

**成员**：char.ToString(System.IFormatProvider)</br>
**签名**：_fc3c2436fe7b6197</br>
**注释**：

```xml
<summary>Converts the value of this instance to its equivalent string representation using the specified culture-specific format information.</summary>
<param name="provider">(Reserved) An object that supplies culture-specific formatting information.</param>
<returns>The string representation of the value of this instance as specified by <paramref name="provider" />.</returns>
```

**成员**：static char.ToString(char)</br>
**签名**：_f59d4d8b2c441c53</br>
**注释**：

```xml
<summary>Converts the specified Unicode character to its equivalent string representation.</summary>
<param name="c">The Unicode character to convert.</param>
<returns>The string representation of the value of <paramref name="c" />.</returns>
```

**成员**：static char.Parse(string)</br>
**签名**：_d89999df761a6d2e</br>
**注释**：

```xml
<summary>Converts the value of the specified string to its equivalent Unicode character.</summary>
<param name="s">A string that contains a single character, or <see langword="null" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.FormatException">The length of <paramref name="s" /> is not 1.</exception>
<returns>A Unicode character equivalent to the sole character in <paramref name="s" />.</returns>
```

**成员**：static char.TryParse(string, out char)</br>
**签名**：_9450f84427428db0</br>
**注释**：

```xml
<summary>Converts the value of the specified string to its equivalent Unicode character. A return code indicates whether the conversion succeeded or failed.</summary>
<param name="s">A string that contains a single character, or <see langword="null" />.</param>
<param name="result">When this method returns, contains a Unicode character equivalent to the sole character in <paramref name="s" />, if the conversion succeeded, or an undefined value if the conversion failed. The conversion fails if the <paramref name="s" /> parameter is <see langword="null" /> or the length of <paramref name="s" /> is not 1. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if the <paramref name="s" /> parameter was converted successfully; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsAsciiLetter(char)</br>
**签名**：_1737fc6cbaca1038</br>
**注释**：

```xml
<summary>Indicates whether a character is categorized as an ASCII letter.</summary>
<param name="c">The character to evaluate.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is an ASCII letter; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsAsciiLetterLower(char)</br>
**签名**：_d0f415a83ae10d8a</br>
**注释**：

```xml
<summary>Indicates whether a character is categorized as a lowercase ASCII letter.</summary>
<param name="c">The character to evaluate.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is a lowercase ASCII letter; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsAsciiLetterUpper(char)</br>
**签名**：_30f49ccd6f1f8b2d</br>
**注释**：

```xml
<summary>Indicates whether a character is categorized as an uppercase ASCII letter.</summary>
<param name="c">The character to evaluate.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is an uppercase ASCII letter; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsAsciiDigit(char)</br>
**签名**：_266ce5f0f0db2958</br>
**注释**：

```xml
<summary>Indicates whether a character is categorized as an ASCII digit.</summary>
<param name="c">The character to evaluate.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is an ASCII digit; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsAsciiLetterOrDigit(char)</br>
**签名**：_3f3a99864b7042e9</br>
**注释**：

```xml
<summary>Indicates whether a character is categorized as an ASCII letter or digit.</summary>
<param name="c">The character to evaluate.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is an ASCII letter or digit; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsAsciiHexDigit(char)</br>
**签名**：_8ebed700a57241d2</br>
**注释**：

```xml
<summary>Indicates whether a character is categorized as an ASCII hexademical digit.</summary>
<param name="c">The character to evaluate.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is a hexademical digit; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsAsciiHexDigitUpper(char)</br>
**签名**：_47cc49555e21ab3b</br>
**注释**：

```xml
<summary>Indicates whether a character is categorized as an ASCII upper-case hexademical digit.</summary>
<param name="c">The character to evaluate.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is a hexademical digit; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsAsciiHexDigitLower(char)</br>
**签名**：_c082c46f951a0c9f</br>
**注释**：

```xml
<summary>Indicates whether a character is categorized as an ASCII lower-case hexademical digit.</summary>
<param name="c">The character to evaluate.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is a lower-case hexademical digit; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsDigit(char)</br>
**签名**：_91a882221d295c32</br>
**注释**：

```xml
<summary>Indicates whether the specified Unicode character is categorized as a decimal digit.</summary>
<param name="c">The Unicode character to evaluate.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is a decimal digit; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsBetween(char, char, char)</br>
**签名**：_dfb76865a7840d43</br>
**注释**：

```xml
<summary>Indicates whether a character is within the specified inclusive range.</summary>
<param name="c">The character to evaluate.</param>
<param name="minInclusive">The lower bound, inclusive.</param>
<param name="maxInclusive">The upper bound, inclusive.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is within the specified range; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsLetter(char)</br>
**签名**：_38721338a529a8d7</br>
**注释**：

```xml
<summary>Indicates whether the specified Unicode character is categorized as a Unicode letter.</summary>
<param name="c">The Unicode character to evaluate.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is a letter; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsWhiteSpace(char)</br>
**签名**：_16e351e6f7b127f7</br>
**注释**：

```xml
<summary>Indicates whether the specified Unicode character is categorized as white space.</summary>
<param name="c">The Unicode character to evaluate.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is white space; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsUpper(char)</br>
**签名**：_7d70d8021ab255a8</br>
**注释**：

```xml
<summary>Indicates whether the specified Unicode character is categorized as an uppercase letter.</summary>
<param name="c">The Unicode character to evaluate.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is an uppercase letter; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsLower(char)</br>
**签名**：_b344d14ce0e33570</br>
**注释**：

```xml
<summary>Indicates whether the specified Unicode character is categorized as a lowercase letter.</summary>
<param name="c">The Unicode character to evaluate.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is a lowercase letter; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsPunctuation(char)</br>
**签名**：_ce3de1c060963041</br>
**注释**：

```xml
<summary>Indicates whether the specified Unicode character is categorized as a punctuation mark.</summary>
<param name="c">The Unicode character to evaluate.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is a punctuation mark; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsLetterOrDigit(char)</br>
**签名**：_49432dd2165d98f0</br>
**注释**：

```xml
<summary>Indicates whether the specified Unicode character is categorized as a letter or a decimal digit.</summary>
<param name="c">The Unicode character to evaluate.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is a letter or a decimal digit; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.ToUpper(char, System.Globalization.CultureInfo)</br>
**签名**：_dd41639bb00c83ab</br>
**注释**：

```xml
<summary>Converts the value of a specified Unicode character to its uppercase equivalent using specified culture-specific formatting information.</summary>
<param name="c">The Unicode character to convert.</param>
<param name="culture">An object that supplies culture-specific casing rules.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="culture" /> is <see langword="null" />.</exception>
<returns>The uppercase equivalent of <paramref name="c" />, modified according to <paramref name="culture" />, or the unchanged value of <paramref name="c" /> if <paramref name="c" /> is already uppercase, has no uppercase equivalent, or is not alphabetic.</returns>
```

**成员**：static char.ToUpper(char)</br>
**签名**：_2713512e6f5a9312</br>
**注释**：

```xml
<summary>Converts the value of a Unicode character to its uppercase equivalent.</summary>
<param name="c">The Unicode character to convert.</param>
<returns>The uppercase equivalent of <paramref name="c" />, or the unchanged value of <paramref name="c" /> if <paramref name="c" /> is already uppercase, has no uppercase equivalent, or is not alphabetic.</returns>
```

**成员**：static char.ToUpperInvariant(char)</br>
**签名**：_b0c91aa30cd2a5f7</br>
**注释**：

```xml
<summary>Converts the value of a Unicode character to its uppercase equivalent using the casing rules of the invariant culture.</summary>
<param name="c">The Unicode character to convert.</param>
<returns>The uppercase equivalent of the <paramref name="c" /> parameter, or the unchanged value of <paramref name="c" />, if <paramref name="c" /> is already uppercase or not alphabetic.</returns>
```

**成员**：static char.ToLower(char, System.Globalization.CultureInfo)</br>
**签名**：_b81ddeb8c6240b72</br>
**注释**：

```xml
<summary>Converts the value of a specified Unicode character to its lowercase equivalent using specified culture-specific formatting information.</summary>
<param name="c">The Unicode character to convert.</param>
<param name="culture">An object that supplies culture-specific casing rules.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="culture" /> is <see langword="null" />.</exception>
<returns>The lowercase equivalent of <paramref name="c" />, modified according to <paramref name="culture" />, or the unchanged value of <paramref name="c" />, if <paramref name="c" /> is already lowercase or not alphabetic.</returns>
```

**成员**：static char.ToLower(char)</br>
**签名**：_b91d21a936e68017</br>
**注释**：

```xml
<summary>Converts the value of a Unicode character to its lowercase equivalent.</summary>
<param name="c">The Unicode character to convert.</param>
<returns>The lowercase equivalent of <paramref name="c" />, or the unchanged value of <paramref name="c" />, if <paramref name="c" /> is already lowercase or not alphabetic.</returns>
```

**成员**：static char.ToLowerInvariant(char)</br>
**签名**：_76274ed9d45c0127</br>
**注释**：

```xml
<summary>Converts the value of a Unicode character to its lowercase equivalent using the casing rules of the invariant culture.</summary>
<param name="c">The Unicode character to convert.</param>
<returns>The lowercase equivalent of the <paramref name="c" /> parameter, or the unchanged value of <paramref name="c" />, if <paramref name="c" /> is already lowercase or not alphabetic.</returns>
```

**成员**：char.GetTypeCode()</br>
**签名**：_84932c09c59d9b51</br>
**注释**：

```xml
<summary>Returns the <see cref="T:System.TypeCode" /> for value type <see cref="T:System.Char" />.</summary>
<returns>The enumerated constant, <see cref="F:System.TypeCode.Char" />.</returns>
```

**成员**：static char.IsControl(char)</br>
**签名**：_c12d0a40e2ed8650</br>
**注释**：

```xml
<summary>Indicates whether the specified Unicode character is categorized as a control character.</summary>
<param name="c">The Unicode character to evaluate.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is a control character; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsControl(string, int)</br>
**签名**：_68e189abbb5497dc</br>
**注释**：

```xml
<summary>Indicates whether the character at the specified position in a specified string is categorized as a control character.</summary>
<param name="s">A string.</param>
<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is a control character; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsDigit(string, int)</br>
**签名**：_52eb020022da112b</br>
**注释**：

```xml
<summary>Indicates whether the character at the specified position in a specified string is categorized as a decimal digit.</summary>
<param name="s">A string.</param>
<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is a decimal digit; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsLetter(string, int)</br>
**签名**：_e7ee64c732d21cd5</br>
**注释**：

```xml
<summary>Indicates whether the character at the specified position in a specified string is categorized as a Unicode letter.</summary>
<param name="s">A string.</param>
<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is a letter; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsLetterOrDigit(string, int)</br>
**签名**：_d752ce4eaadf7612</br>
**注释**：

```xml
<summary>Indicates whether the character at the specified position in a specified string is categorized as a letter or a decimal digit.</summary>
<param name="s">A string.</param>
<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is a letter or a decimal digit; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsLower(string, int)</br>
**签名**：_6ebe08db86ea37a2</br>
**注释**：

```xml
<summary>Indicates whether the character at the specified position in a specified string is categorized as a lowercase letter.</summary>
<param name="s">A string.</param>
<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is a lowercase letter; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsNumber(char)</br>
**签名**：_77e97c648607e65e</br>
**注释**：

```xml
<summary>Indicates whether the specified Unicode character is categorized as a number.</summary>
<param name="c">The Unicode character to evaluate.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is a number; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsNumber(string, int)</br>
**签名**：_5180e5acb1d4bcb0</br>
**注释**：

```xml
<summary>Indicates whether the character at the specified position in a specified string is categorized as a number.</summary>
<param name="s">A string.</param>
<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is a number; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsPunctuation(string, int)</br>
**签名**：_5f7e394ed1d09372</br>
**注释**：

```xml
<summary>Indicates whether the character at the specified position in a specified string is categorized as a punctuation mark.</summary>
<param name="s">A string.</param>
<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is a punctuation mark; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsSeparator(char)</br>
**签名**：_066fc76a18dc824f</br>
**注释**：

```xml
<summary>Indicates whether the specified Unicode character is categorized as a separator character.</summary>
<param name="c">The Unicode character to evaluate.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is a separator character; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsSeparator(string, int)</br>
**签名**：_3d391ade47da71a6</br>
**注释**：

```xml
<summary>Indicates whether the character at the specified position in a specified string is categorized as a separator character.</summary>
<param name="s">A string.</param>
<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is a separator character; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsSurrogate(char)</br>
**签名**：_e5949fe4a1738a38</br>
**注释**：

```xml
<summary>Indicates whether the specified character has a surrogate code unit.</summary>
<param name="c">The Unicode character to evaluate.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is either a high surrogate or a low surrogate; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsSurrogate(string, int)</br>
**签名**：_bca1b50c85e48723</br>
**注释**：

```xml
<summary>Indicates whether the character at the specified position in a specified string has a surrogate code unit.</summary>
<param name="s">A string.</param>
<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is a either a high surrogate or a low surrogate; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsSymbol(char)</br>
**签名**：_0f18b1b6d2524322</br>
**注释**：

```xml
<summary>Indicates whether the specified Unicode character is categorized as a symbol character.</summary>
<param name="c">The Unicode character to evaluate.</param>
<returns>  <see langword="true" /> if <paramref name="c" /> is a symbol character; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsSymbol(string, int)</br>
**签名**：_16587492d280e91d</br>
**注释**：

```xml
<summary>Indicates whether the character at the specified position in a specified string is categorized as a symbol character.</summary>
<param name="s">A string.</param>
<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is a symbol character; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsUpper(string, int)</br>
**签名**：_1ae24de44f4b499e</br>
**注释**：

```xml
<summary>Indicates whether the character at the specified position in a specified string is categorized as an uppercase letter.</summary>
<param name="s">A string.</param>
<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is an uppercase letter; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsWhiteSpace(string, int)</br>
**签名**：_a21dd6de62be7b75</br>
**注释**：

```xml
<summary>Indicates whether the character at the specified position in a specified string is categorized as white space.</summary>
<param name="s">A string.</param>
<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
<returns>  <see langword="true" /> if the character at position <paramref name="index" /> in <paramref name="s" /> is white space; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.GetUnicodeCategory(char)</br>
**签名**：_226cc4ffd552fcf9</br>
**注释**：

```xml
<summary>Categorizes a specified Unicode character into a group identified by one of the <see cref="T:System.Globalization.UnicodeCategory" /> values.</summary>
<param name="c">The Unicode character to categorize.</param>
<returns>A <see cref="T:System.Globalization.UnicodeCategory" /> value that identifies the group that contains <paramref name="c" />.</returns>
```

**成员**：static char.GetUnicodeCategory(string, int)</br>
**签名**：_e41ad686bd01aff1</br>
**注释**：

```xml
<summary>Categorizes the character at the specified position in a specified string into a group identified by one of the <see cref="T:System.Globalization.UnicodeCategory" /> values.</summary>
<param name="s">A <see cref="T:System.String" />.</param>
<param name="index">The character position in <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
<returns>A <see cref="T:System.Globalization.UnicodeCategory" /> enumerated constant that identifies the group that contains the character at position <paramref name="index" /> in <paramref name="s" />.</returns>
```

**成员**：static char.GetNumericValue(char)</br>
**签名**：_d86c1e9964250116</br>
**注释**：

```xml
<summary>Converts the specified numeric Unicode character to a double-precision floating point number.</summary>
<param name="c">The Unicode character to convert.</param>
<returns>The numeric value of <paramref name="c" /> if that character represents a number; otherwise, -1.0.</returns>
```

**成员**：static char.GetNumericValue(string, int)</br>
**签名**：_938251f1b1fc7bc8</br>
**注释**：

```xml
<summary>Converts the numeric Unicode character at the specified position in a specified string to a double-precision floating point number.</summary>
<param name="s">A <see cref="T:System.String" />.</param>
<param name="index">The character position in <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than zero or greater than the last position in <paramref name="s" />.</exception>
<returns>The numeric value of the character at position <paramref name="index" /> in <paramref name="s" /> if that character represents a number; otherwise, -1.</returns>
```

**成员**：static char.IsHighSurrogate(char)</br>
**签名**：_4c066834beda061c</br>
**注释**：

```xml
<summary>Indicates whether the specified <see cref="T:System.Char" /> object is a high surrogate.</summary>
<param name="c">The Unicode character to evaluate.</param>
<returns>  <see langword="true" /> if the numeric value of the <paramref name="c" /> parameter ranges from U+D800 through U+DBFF; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsHighSurrogate(string, int)</br>
**签名**：_311485d1745ce294</br>
**注释**：

```xml
<summary>Indicates whether the <see cref="T:System.Char" /> object at the specified position in a string is a high surrogate.</summary>
<param name="s">A string.</param>
<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is not a position within <paramref name="s" />.</exception>
<returns>  <see langword="true" /> if the numeric value of the specified character in the <paramref name="s" /> parameter ranges from U+D800 through U+DBFF; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsLowSurrogate(char)</br>
**签名**：_7761ca7b99042e8a</br>
**注释**：

```xml
<summary>Indicates whether the specified <see cref="T:System.Char" /> object is a low surrogate.</summary>
<param name="c">The character to evaluate.</param>
<returns>  <see langword="true" /> if the numeric value of the <paramref name="c" /> parameter ranges from U+DC00 through U+DFFF; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsLowSurrogate(string, int)</br>
**签名**：_1d56cdc9a261e948</br>
**注释**：

```xml
<summary>Indicates whether the <see cref="T:System.Char" /> object at the specified position in a string is a low surrogate.</summary>
<param name="s">A string.</param>
<param name="index">The position of the character to evaluate in <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is not a position within <paramref name="s" />.</exception>
<returns>  <see langword="true" /> if the numeric value of the specified character in the <paramref name="s" /> parameter ranges from U+DC00 through U+DFFF; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsSurrogatePair(string, int)</br>
**签名**：_27c9fca9c829cc5e</br>
**注释**：

```xml
<summary>Indicates whether two adjacent <see cref="T:System.Char" /> objects at a specified position in a string form a surrogate pair.</summary>
<param name="s">A string.</param>
<param name="index">The starting position of the pair of characters to evaluate within <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is not a position within <paramref name="s" />.</exception>
<returns>  <see langword="true" /> if the <paramref name="s" /> parameter includes adjacent characters at positions <paramref name="index" /> and <paramref name="index" /> + 1, and the numeric value of the character at position <paramref name="index" /> ranges from U+D800 through U+DBFF, and the numeric value of the character at position <paramref name="index" />+1 ranges from U+DC00 through U+DFFF; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.IsSurrogatePair(char, char)</br>
**签名**：_efe9c9b601517069</br>
**注释**：

```xml
<summary>Indicates whether the two specified <see cref="T:System.Char" /> objects form a surrogate pair.</summary>
<param name="highSurrogate">The character to evaluate as the high surrogate of a surrogate pair.</param>
<param name="lowSurrogate">The character to evaluate as the low surrogate of a surrogate pair.</param>
<returns>  <see langword="true" /> if the numeric value of the <paramref name="highSurrogate" /> parameter ranges from U+D800 through U+DBFF, and the numeric value of the <paramref name="lowSurrogate" /> parameter ranges from U+DC00 through U+DFFF; otherwise, <see langword="false" />.</returns>
```

**成员**：static char.ConvertFromUtf32(int)</br>
**签名**：_fdcbb676a7d83aab</br>
**注释**：

```xml
<summary>Converts the specified Unicode code point into a UTF-16 encoded string.</summary>
<param name="utf32">A 21-bit Unicode code point.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="utf32" /> is not a valid 21-bit Unicode code point ranging from U+0 through U+10FFFF, excluding the surrogate pair range from U+D800 through U+DFFF.</exception>
<returns>A string consisting of one <see cref="T:System.Char" /> object or a surrogate pair of <see cref="T:System.Char" /> objects equivalent to the code point specified by the <paramref name="utf32" /> parameter.</returns>
```

**成员**：static char.ConvertToUtf32(char, char)</br>
**签名**：_f842e9b2f7fea133</br>
**注释**：

```xml
<summary>Converts the value of a UTF-16 encoded surrogate pair into a Unicode code point.</summary>
<param name="highSurrogate">A high surrogate code unit (that is, a code unit ranging from U+D800 through U+DBFF).</param>
<param name="lowSurrogate">A low surrogate code unit (that is, a code unit ranging from U+DC00 through U+DFFF).</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="highSurrogate" /> is not in the range U+D800 through U+DBFF, or <paramref name="lowSurrogate" /> is not in the range U+DC00 through U+DFFF.</exception>
<returns>The 21-bit Unicode code point represented by the <paramref name="highSurrogate" /> and <paramref name="lowSurrogate" /> parameters.</returns>
```

**成员**：static char.ConvertToUtf32(string, int)</br>
**签名**：_d9f7c3c03ea64580</br>
**注释**：

```xml
<summary>Converts the value of a UTF-16 encoded character or surrogate pair at a specified position in a string into a Unicode code point.</summary>
<param name="s">A string that contains a character or surrogate pair.</param>
<param name="index">The index position of the character or surrogate pair in <paramref name="s" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is not a position within <paramref name="s" />.</exception>
<exception cref="T:System.ArgumentException">The specified index position contains a surrogate pair, and either the first character in the pair is not a valid high surrogate or the second character in the pair is not a valid low surrogate.</exception>
<returns>The 21-bit Unicode code point represented by the character or surrogate pair at the position in the <paramref name="s" /> parameter specified by the <paramref name="index" /> parameter.</returns>
```

