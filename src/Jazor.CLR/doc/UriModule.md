# UriModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：static readonly System.Uri.UriSchemeFile</br>
**签名**：_2ba16bbf0be9c766</br>
**注释**：

```xml
<summary>Specifies that the URI is a pointer to a file. This field is read-only.</summary>
```

**成员**：static readonly System.Uri.UriSchemeFtp</br>
**签名**：_6e85d2817c15512f</br>
**注释**：

```xml
<summary>Specifies that the URI is accessed through the File Transfer Protocol (FTP). This field is read-only.</summary>
```

**成员**：static readonly System.Uri.UriSchemeSftp</br>
**签名**：_3a7a17ca3a3657dc</br>
**注释**：

```xml
<summary>Specifies that the URI is accessed through the SSH File Transfer Protocol (SFTP). This field is read-only.</summary>
```

**成员**：static readonly System.Uri.UriSchemeFtps</br>
**签名**：_f1e516eed4f4741f</br>
**注释**：

```xml
<summary>Specifies that the URI is accessed through the File Transfer Protocol Secure (FTPS). This field is read-only.</summary>
```

**成员**：static readonly System.Uri.UriSchemeGopher</br>
**签名**：_fea0853a2e29d1f6</br>
**注释**：

```xml
<summary>Specifies that the URI is accessed through the Gopher protocol. This field is read-only.</summary>
```

**成员**：static readonly System.Uri.UriSchemeHttp</br>
**签名**：_2e8f86d57961652d</br>
**注释**：

```xml
<summary>Specifies that the URI is accessed through the Hypertext Transfer Protocol (HTTP). This field is read-only.</summary>
```

**成员**：static readonly System.Uri.UriSchemeHttps</br>
**签名**：_bef172fefa666833</br>
**注释**：

```xml
<summary>Specifies that the URI is accessed through the Secure Hypertext Transfer Protocol (HTTPS). This field is read-only.</summary>
```

**成员**：static readonly System.Uri.UriSchemeWs</br>
**签名**：_54e8680bee83f6f1</br>
**注释**：

```xml
<summary>Specifies that the URI is accessed through the WebSocket protocol (WS). This field is read-only.</summary>
```

**成员**：static readonly System.Uri.UriSchemeWss</br>
**签名**：_02277693524fbfc6</br>
**注释**：

```xml
<summary>Specifies that the URI is accessed through the WebSocket Secure protocol (WSS). This field is read-only.</summary>
```

**成员**：static readonly System.Uri.UriSchemeMailto</br>
**签名**：_c73be57d0636f694</br>
**注释**：

```xml
<summary>Specifies that the URI is an email address and is accessed through the Simple Mail Transport Protocol (SMTP). This field is read-only.</summary>
```

**成员**：static readonly System.Uri.UriSchemeNews</br>
**签名**：_ec4dcf23315e1226</br>
**注释**：

```xml
<summary>Specifies that the URI is an Internet news group and is accessed through the Network News Transport Protocol (NNTP). This field is read-only.</summary>
```

**成员**：static readonly System.Uri.UriSchemeNntp</br>
**签名**：_bd4bcf513b622ec9</br>
**注释**：

```xml
<summary>Specifies that the URI is an Internet news group and is accessed through the Network News Transport Protocol (NNTP). This field is read-only.</summary>
```

**成员**：static readonly System.Uri.UriSchemeSsh</br>
**签名**：_8327dcd8a2fc6e21</br>
**注释**：

```xml
<summary>Specifies that the URI is accessed through the Secure Socket Shell protocol (SSH). This field is read-only.</summary>
```

**成员**：static readonly System.Uri.UriSchemeTelnet</br>
**签名**：_9bec36f8ec4350e2</br>
**注释**：

```xml
<summary>Specifies that the URI is accessed through the Telnet protocol. This field is read-only.</summary>
```

**成员**：static readonly System.Uri.UriSchemeNetTcp</br>
**签名**：_26f9af9f89b63ca0</br>
**注释**：

```xml
<summary>Specifies that the URI is accessed through the NetTcp scheme used by Windows Communication Foundation (WCF). This field is read-only.</summary>
```

**成员**：static readonly System.Uri.UriSchemeNetPipe</br>
**签名**：_c0177fdc20844a24</br>
**注释**：

```xml
<summary>Specifies that the URI is accessed through the NetPipe scheme used by Windows Communication Foundation (WCF). This field is read-only.</summary>
```

**成员**：static readonly System.Uri.UriSchemeData</br>
**签名**：_7176a60db1b2d8ff</br>

**成员**：static readonly System.Uri.SchemeDelimiter</br>
**签名**：_8f18e71b9b3655d0</br>
**注释**：

```xml
<summary>Specifies the characters that separate the communication protocol scheme from the address portion of the URI. This field is read-only.</summary>
```

**成员**：System.Uri.Uri(string)</br>
**签名**：_c69acf122e3679e8</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Uri" /> class with the specified URI.</summary>
<param name="uriString">A string that identifies the resource to be represented by the <see cref="T:System.Uri" /> instance. Note that an IPv6 address in string form must be enclosed within brackets. For example, "http://[2607:f8b0:400d:c06::69]".</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="uriString" /> is <see langword="null" />.</exception>
<exception cref="T:System.UriFormatException">Note: In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.FormatException" />, instead.<paramref name="uriString" /> is empty. -or- The scheme specified in <paramref name="uriString" /> is not correctly formed. See <see cref="M:System.Uri.CheckSchemeName(System.String)" />. -or- <paramref name="uriString" /> contains too many slashes. -or- The password specified in <paramref name="uriString" /> is not valid. -or- The host name specified in <paramref name="uriString" /> is not valid. -or- The file name specified in <paramref name="uriString" /> is not valid. -or- The user name specified in <paramref name="uriString" /> is not valid. -or- The host or authority name specified in <paramref name="uriString" /> cannot be terminated by backslashes. -or- The port number specified in <paramref name="uriString" /> is not valid or cannot be parsed. -or- The length of <paramref name="uriString" /> exceeds 65519 characters. -or- The length of the scheme specified in <paramref name="uriString" /> exceeds 1023 characters. -or- There is an invalid character sequence in <paramref name="uriString" />. -or- The MS-DOS path specified in <paramref name="uriString" /> must start with c:\\.</exception>
```

**成员**：System.Uri.Uri(string, bool)</br>
**签名**：_5bfdc0bfeb15d51a</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Uri" /> class with the specified URI, with explicit control of character escaping.</summary>
<param name="uriString">A string that identifies the resource to be represented by the <see cref="T:System.Uri" /> instance. Note that an IPv6 address in string form must be enclosed within brackets. For example, "http://[2607:f8b0:400d:c06::69]".</param>
<param name="dontEscape">  <see langword="true" /> if <paramref name="uriString" /> is completely escaped; otherwise, <see langword="false" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="uriString" /> is <see langword="null" />.</exception>
<exception cref="T:System.UriFormatException">  <paramref name="uriString" /> is empty or contains only spaces. -or- The scheme specified in <paramref name="uriString" /> is not valid. -or- <paramref name="uriString" /> contains too many slashes. -or- The password specified in <paramref name="uriString" /> is not valid. -or- The host name specified in <paramref name="uriString" /> is not valid. -or- The file name specified in <paramref name="uriString" /> is not valid. -or- The user name specified in <paramref name="uriString" /> is not valid. -or- The host or authority name specified in <paramref name="uriString" /> cannot be terminated by backslashes. -or- The port number specified in <paramref name="uriString" /> is not valid or cannot be parsed. -or- The length of <paramref name="uriString" /> exceeds 65519 characters. -or- The length of the scheme specified in <paramref name="uriString" /> exceeds 1023 characters. -or- There is an invalid character sequence in <paramref name="uriString" />. -or- The MS-DOS path specified in <paramref name="uriString" /> must start with c:\\.</exception>
```

**成员**：System.Uri.Uri(System.Uri, string, bool)</br>
**签名**：_d51abfd7096feca6</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Uri" /> class based on the specified base and relative URIs, with explicit control of character escaping.</summary>
<param name="baseUri">The base URI.</param>
<param name="relativeUri">The relative URI to add to the base URI.</param>
<param name="dontEscape">  <see langword="true" /> if <paramref name="baseUri" /> and <paramref name="relativeUri" /> are completely escaped; otherwise, <see langword="false" />.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="baseUri" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="baseUri" /> is not an absolute <see cref="T:System.Uri" /> instance.</exception>
<exception cref="T:System.UriFormatException">The URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is empty or contains only spaces. -or- The scheme specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is not valid. -or- The URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> contains too many slashes. -or- The password specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is not valid. -or- The host name specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is not valid. -or- The file name specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is not valid. -or- The user name specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is not valid. -or- The host or authority name specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> cannot be terminated by backslashes. -or- The port number specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is not valid or cannot be parsed. -or- The length of the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> exceeds 65519 characters. -or- The length of the scheme specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> exceeds 1023 characters. -or- There is an invalid character sequence in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" />. -or- The MS-DOS path specified in <paramref name="baseUri" /> must start with c:\\.</exception>
```

**成员**：System.Uri.Uri(string, System.UriKind)</br>
**签名**：_6117c7498c2ced6a</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Uri" /> class with the specified URI. This constructor allows you to specify if the URI string is a relative URI, absolute URI, or is indeterminate.</summary>
<param name="uriString">A string that identifies the resource to be represented by the <see cref="T:System.Uri" /> instance. Note that an IPv6 address in string form must be enclosed within brackets. For example, "http://[2607:f8b0:400d:c06::69]".</param>
<param name="uriKind">Specifies whether the URI string is a relative URI, absolute URI, or is indeterminate.</param>
<exception cref="T:System.ArgumentException">  <paramref name="uriKind" /> is invalid.</exception>
<exception cref="T:System.ArgumentNullException">  <paramref name="uriString" /> is <see langword="null" />.</exception>
<exception cref="T:System.UriFormatException">Note: In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.FormatException" />, instead.<paramref name="uriString" /> contains a relative URI and <paramref name="uriKind" /> is <see cref="F:System.UriKind.Absolute" />. or <paramref name="uriString" /> contains an absolute URI and <paramref name="uriKind" /> is <see cref="F:System.UriKind.Relative" />. or <paramref name="uriString" /> is empty. -or- The scheme specified in <paramref name="uriString" /> is not correctly formed. See <see cref="M:System.Uri.CheckSchemeName(System.String)" />. -or- <paramref name="uriString" /> contains too many slashes. -or- The password specified in <paramref name="uriString" /> is not valid. -or- The host name specified in <paramref name="uriString" /> is not valid. -or- The file name specified in <paramref name="uriString" /> is not valid. -or- The user name specified in <paramref name="uriString" /> is not valid. -or- The host or authority name specified in <paramref name="uriString" /> cannot be terminated by backslashes. -or- The port number specified in <paramref name="uriString" /> is not valid or cannot be parsed. -or- The length of <paramref name="uriString" /> exceeds 65519 characters. -or- The length of the scheme specified in <paramref name="uriString" /> exceeds 1023 characters. -or- There is an invalid character sequence in <paramref name="uriString" />. -or- The MS-DOS path specified in <paramref name="uriString" /> must start with c:\\.</exception>
```

**成员**：System.Uri.Uri(string, in System.UriCreationOptions)</br>
**签名**：_b085b39dce013441</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Uri" /> class with the specified URI and additional <see cref="T:System.UriCreationOptions" />.</summary>
<param name="uriString">A string that identifies the resource to be represented by the <see cref="T:System.Uri" /> instance.</param>
<param name="creationOptions">Options that control how the <see cref="T:System.Uri" /> is created and behaves.</param>
```

**成员**：System.Uri.Uri(System.Uri, string)</br>
**签名**：_1018fc46d28f8d3a</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Uri" /> class based on the specified base URI and relative URI string.</summary>
<param name="baseUri">The base URI.</param>
<param name="relativeUri">The relative URI to add to the base URI.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="baseUri" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="baseUri" /> is not an absolute <see cref="T:System.Uri" /> instance.</exception>
<exception cref="T:System.UriFormatException">Note: In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.FormatException" />, instead. The URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is empty or contains only spaces. -or- The scheme specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is not valid. -or- The URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> contains too many slashes. -or- The password specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is not valid. -or- The host name specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is not valid. -or- The file name specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is not valid. -or- The user name specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is not valid. -or- The host or authority name specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> cannot be terminated by backslashes. -or- The port number specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is not valid or cannot be parsed. -or- The length of the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> exceeds 65519 characters. -or- The length of the scheme specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> exceeds 1023 characters. -or- There is an invalid character sequence in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" />. -or- The MS-DOS path specified in <paramref name="baseUri" /> must start with c:\\.</exception>
```

**成员**：System.Uri.Uri(System.Uri, System.Uri)</br>
**签名**：_e160df002d8288c4</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Uri" /> class based on the combination of a specified base <see cref="T:System.Uri" /> instance and a relative <see cref="T:System.Uri" /> instance.</summary>
<param name="baseUri">An absolute <see cref="T:System.Uri" /> that is the base for the new <see cref="T:System.Uri" /> instance.</param>
<param name="relativeUri">A relative <see cref="T:System.Uri" /> instance that is combined with <paramref name="baseUri" />.</param>
<exception cref="T:System.ArgumentException">  <paramref name="baseUri" /> is not an absolute <see cref="T:System.Uri" /> instance.</exception>
<exception cref="T:System.ArgumentNullException">  <paramref name="baseUri" /> is <see langword="null" />.</exception>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="baseUri" /> is not an absolute <see cref="T:System.Uri" /> instance.</exception>
<exception cref="T:System.UriFormatException">Note: In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.FormatException" />, instead.The URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is empty or contains only spaces. -or- The scheme specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is not valid. -or- The URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> contains too many slashes. -or- The password specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is not valid. -or- The host name specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is not valid. -or- The file name specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is not valid. -or- The user name specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is not valid. -or- The host or authority name specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> cannot be terminated by backslashes. -or- The port number specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> is not valid or cannot be parsed. -or- The length of the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> exceeds 65519 characters. -or- The length of the scheme specified in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" /> exceeds 1023 characters. -or- There is an invalid character sequence in the URI formed by combining <paramref name="baseUri" /> and <paramref name="relativeUri" />. -or- The MS-DOS path specified in <paramref name="baseUri" /> must start with c:\\.</exception>
```

**成员**：System.Uri.AbsolutePath.get</br>
**签名**：_3f2baa0c5c2071a3</br>

**成员**：System.Uri.AbsoluteUri.get</br>
**签名**：_8dd50aa0b90b1213</br>

**成员**：System.Uri.LocalPath.get</br>
**签名**：_e7e565809f507adf</br>

**成员**：System.Uri.Authority.get</br>
**签名**：_593ae4a9895276c1</br>

**成员**：System.Uri.HostNameType.get</br>
**签名**：_96948be49c895a89</br>

**成员**：System.Uri.IsDefaultPort.get</br>
**签名**：_1baacf675bb3fb95</br>

**成员**：System.Uri.IsFile.get</br>
**签名**：_faa00427645f805c</br>

**成员**：System.Uri.IsLoopback.get</br>
**签名**：_bd9f067200257f8b</br>

**成员**：System.Uri.PathAndQuery.get</br>
**签名**：_a239992828cb65fe</br>

**成员**：System.Uri.Segments.get</br>
**签名**：_f58a297dde499995</br>

**成员**：System.Uri.IsUnc.get</br>
**签名**：_592ac2011277a2a8</br>

**成员**：System.Uri.Host.get</br>
**签名**：_0bf33ac48e4b1418</br>

**成员**：System.Uri.Port.get</br>
**签名**：_6c33876665ad9277</br>

**成员**：System.Uri.Query.get</br>
**签名**：_2f4402ceaba26b6a</br>

**成员**：System.Uri.Fragment.get</br>
**签名**：_1f4ba98ea2f15f79</br>

**成员**：System.Uri.Scheme.get</br>
**签名**：_3e0f742131d7d14c</br>

**成员**：System.Uri.OriginalString.get</br>
**签名**：_e79e114400371f50</br>

**成员**：System.Uri.DnsSafeHost.get</br>
**签名**：_035f59f486887f90</br>

**成员**：System.Uri.IdnHost.get</br>
**签名**：_91e7e401791152a5</br>

**成员**：System.Uri.IsAbsoluteUri.get</br>
**签名**：_238d60be3ca3fe79</br>

**成员**：System.Uri.UserEscaped.get</br>
**签名**：_07d13be47bc558eb</br>

**成员**：System.Uri.UserInfo.get</br>
**签名**：_c2821111b3996886</br>

**成员**：static System.Uri.CheckHostName(string)</br>
**签名**：_be7fb8f462373216</br>
**注释**：

```xml
<summary>Determines whether the specified host name is a valid DNS name.</summary>
<param name="name">The host name to validate. This can be an IPv4 or IPv6 address or an Internet host name.</param>
<returns>The type of the host name. If the type of the host name cannot be determined or if the host name is <see langword="null" /> or a zero-length string, this method returns <see cref="F:System.UriHostNameType.Unknown" />.</returns>
```

**成员**：System.Uri.GetLeftPart(System.UriPartial)</br>
**签名**：_b220d23969edcd04</br>
**注释**：

```xml
<summary>Gets the specified portion of a <see cref="T:System.Uri" /> instance.</summary>
<param name="part">One of the enumeration values that specifies the end of the URI portion to return.</param>
<exception cref="T:System.InvalidOperationException">The current <see cref="T:System.Uri" /> instance is not an absolute instance.</exception>
<exception cref="T:System.ArgumentException">The specified <paramref name="part" /> is not valid.</exception>
<returns>The specified portion of the <see cref="T:System.Uri" /> instance.</returns>
```

**成员**：static System.Uri.HexEscape(char)</br>
**签名**：_4fb3f2958a4e2a4c</br>
**注释**：

```xml
<summary>Converts a specified character into its hexadecimal equivalent.</summary>
<param name="character">The character to convert to hexadecimal representation.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="character" /> is greater than 255.</exception>
<returns>The hexadecimal representation of the specified character.</returns>
```

**成员**：static System.Uri.HexUnescape(string, ref int)</br>
**签名**：_841cac4a5e117221</br>
**注释**：

```xml
<summary>Converts a specified hexadecimal representation of a character to the character.</summary>
<param name="pattern">The hexadecimal representation of a character.</param>
<param name="index">The location in <paramref name="pattern" /> where the hexadecimal representation of a character begins.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="index" /> is less than 0 or greater than or equal to the number of characters in <paramref name="pattern" />.</exception>
<returns>The character represented by the hexadecimal encoding at position <paramref name="index" />. If the character at <paramref name="index" /> is not hexadecimal encoded, the character at <paramref name="index" /> is returned. The value of <paramref name="index" /> is incremented to point to the character following the one returned.</returns>
```

**成员**：static System.Uri.IsHexEncoding(string, int)</br>
**签名**：_8d41c6f05edd1db4</br>
**注释**：

```xml
<summary>Determines whether a character in a string is hexadecimal encoded.</summary>
<param name="pattern">The string to check.</param>
<param name="index">The location in <paramref name="pattern" /> to check for hexadecimal encoding.</param>
<returns>  <see langword="true" /> if <paramref name="pattern" /> is hexadecimal encoded at the specified location; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Uri.CheckSchemeName(string)</br>
**签名**：_1424b7645b530be5</br>
**注释**：

```xml
<summary>Determines whether the specified scheme name is valid.</summary>
<param name="schemeName">The scheme name to validate.</param>
<returns>  <see langword="true" /> if the scheme name is valid; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Uri.IsHexDigit(char)</br>
**签名**：_66ddaab13b45c161</br>
**注释**：

```xml
<summary>Determines whether a specified character is a valid hexadecimal digit.</summary>
<param name="character">The character to validate.</param>
<returns>  <see langword="true" /> if the character is a valid hexadecimal digit; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Uri.FromHex(char)</br>
**签名**：_28262bda12df3885</br>
**注释**：

```xml
<summary>Gets the decimal value of a hexadecimal digit.</summary>
<param name="digit">The hexadecimal digit (0-9, a-f, A-F) to convert.</param>
<exception cref="T:System.ArgumentException">  <paramref name="digit" /> is not a valid hexadecimal digit (0-9, a-f, A-F).</exception>
<returns>A number from 0 to 15 that corresponds to the specified hexadecimal digit.</returns>
```

**成员**：override System.Uri.GetHashCode()</br>
**签名**：_a68401b6d6678489</br>
**注释**：

```xml
<summary>Gets the hash code for the URI.</summary>
<returns>The hash value generated for this URI.</returns>
```

**成员**：override System.Uri.ToString()</br>
**签名**：_833680ed5ab9dcdd</br>
**注释**：

```xml
<summary>Gets a canonical string representation for the specified <see cref="T:System.Uri" /> instance.</summary>
<returns>The unescaped canonical representation of the <see cref="T:System.Uri" /> instance. All characters are unescaped except #, ?, and %.</returns>
```

**成员**：System.Uri.TryFormat(System.Span<char>, out int)</br>
**签名**：_ffb7c681439c53db</br>
**注释**：

```xml
<summary>Attempts to format a canonical string representation for the <see cref="T:System.Uri" /> instance into the specified span.</summary>
<param name="destination">The span into which to write this instance's value formatted as a span of characters.</param>
<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Uri.operator ==(System.Uri, System.Uri)</br>
**签名**：_be6b025ad012bc33</br>
**注释**：

```xml
<summary>Determines whether two <see cref="T:System.Uri" /> instances have the same value.</summary>
<param name="uri1">A URI to compare with <paramref name="uri2" />.</param>
<param name="uri2">A URI to compare with <paramref name="uri1" />.</param>
<returns>  <see langword="true" /> if the <see cref="T:System.Uri" /> instances are equivalent; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Uri.operator !=(System.Uri, System.Uri)</br>
**签名**：_d9d982bd81667405</br>
**注释**：

```xml
<summary>Determines whether two <see cref="T:System.Uri" /> instances do not have the same value.</summary>
<param name="uri1">A URI to compare with <paramref name="uri2" />.</param>
<param name="uri2">A URI to compare with <paramref name="uri1" />.</param>
<returns>  <see langword="true" /> if the two <see cref="T:System.Uri" /> instances are not equal; otherwise, <see langword="false" />. If either parameter is <see langword="null" />, this method returns <see langword="true" />.</returns>
```

**成员**：override System.Uri.Equals(object)</br>
**签名**：_d94fe0c95141df5b</br>
**注释**：

```xml
<summary>Compares two <see cref="T:System.Uri" /> instances for equality.</summary>
<param name="comparand">The URI or a URI identifier to compare with the current instance.</param>
<returns>  <see langword="true" /> if the two instances represent the same URI; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Uri.Equals(System.Uri)</br>
**签名**：_11c1d4c51e31190b</br>
**注释**：

```xml
<summary>Compares two <see cref="T:System.Uri" /> instances for equality.</summary>
<param name="other">The <see cref="T:System.Uri" /> to compare to this instance.</param>
<returns>  <see langword="true" /> if the two instances represent the same URI; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Uri.MakeRelativeUri(System.Uri)</br>
**签名**：_6496de9263f7b27c</br>
**注释**：

```xml
<summary>Determines the difference between two <see cref="T:System.Uri" /> instances.</summary>
<param name="uri">The URI to compare to the current URI.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="uri" /> is <see langword="null" />.</exception>
<exception cref="T:System.InvalidOperationException">This instance represents a relative URI, and this property is valid only for absolute URIs.</exception>
<returns>If the hostname and scheme of this URI instance and <paramref name="uri" /> are the same, then this method returns a relative <see cref="T:System.Uri" /> that, when appended to the current URI instance, yields <paramref name="uri" />. If the hostname or scheme is different, then this method returns a <see cref="T:System.Uri" /> that represents the <paramref name="uri" /> parameter.</returns>
```

**成员**：System.Uri.MakeRelative(System.Uri)</br>
**签名**：_7614a1b9096feecb</br>
**注释**：

```xml
<summary>Determines the difference between two <see cref="T:System.Uri" /> instances.</summary>
<param name="toUri">The URI to compare to the current URI.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="toUri" /> is <see langword="null" />.</exception>
<exception cref="T:System.InvalidOperationException">This instance represents a relative URI, and this method is valid only for absolute URIs.</exception>
<returns>If the hostname and scheme of this URI instance and <paramref name="toUri" /> are the same, then this method returns a <see cref="T:System.String" /> that represents a relative URI that, when appended to the current URI instance, yields the <paramref name="toUri" /> parameter. If the hostname or scheme is different, then this method returns a <see cref="T:System.String" /> that represents the <paramref name="toUri" /> parameter.</returns>
```

**成员**：static System.Uri.TryCreate(string, System.UriKind, out System.Uri)</br>
**签名**：_decf8e6cc59a22c8</br>
**注释**：

```xml
<summary>Creates a new <see cref="T:System.Uri" /> using the specified <see cref="T:System.String" /> instance and a <see cref="T:System.UriKind" />.</summary>
<param name="uriString">The string representation of the <see cref="T:System.Uri" />.</param>
<param name="uriKind">The type of the Uri.</param>
<param name="result">When this method returns, contains the constructed <see cref="T:System.Uri" />.</param>
<returns>  <see langword="true" /> if the <see cref="T:System.Uri" /> was successfully created; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Uri.TryCreate(string, in System.UriCreationOptions, out System.Uri)</br>
**签名**：_4b6400aec40b008a</br>
**注释**：

```xml
<summary>Creates a new <see cref="T:System.Uri" /> using the specified <see cref="T:System.String" /> instance and <see cref="T:System.UriCreationOptions" />.</summary>
<param name="uriString">The string representation of the <see cref="T:System.Uri" />.</param>
<param name="creationOptions">Options that control how the <see cref="T:System.Uri" /> is created and behaves.</param>
<param name="result">When this method returns, contains the constructed <see cref="T:System.Uri" />.</param>
<returns>  <see langword="true" /> if the <see cref="T:System.Uri" /> was successfully created; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Uri.TryCreate(System.Uri, string, out System.Uri)</br>
**签名**：_727803a29648406f</br>
**注释**：

```xml
<summary>Creates a new <see cref="T:System.Uri" /> using the specified base and relative <see cref="T:System.String" /> instances.</summary>
<param name="baseUri">The base URI.</param>
<param name="relativeUri">The string representation of the relative URI to add to the base <see cref="T:System.Uri" />.</param>
<param name="result">When this method returns, contains a <see cref="T:System.Uri" /> constructed from <paramref name="baseUri" /> and <paramref name="relativeUri" />. This parameter is passed uninitialized.</param>
<returns>  <see langword="true" /> if the <see cref="T:System.Uri" /> was successfully created; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Uri.TryCreate(System.Uri, System.Uri, out System.Uri)</br>
**签名**：_ea835c1722497d5a</br>
**注释**：

```xml
<summary>Creates a new <see cref="T:System.Uri" /> using the specified base and relative <see cref="T:System.Uri" /> instances.</summary>
<param name="baseUri">The base URI.</param>
<param name="relativeUri">The relative URI to add to the base <see cref="T:System.Uri" />.</param>
<param name="result">When this method returns, contains a <see cref="T:System.Uri" /> constructed from <paramref name="baseUri" /> and <paramref name="relativeUri" />. This parameter is passed uninitialized.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="baseUri" /> is <see langword="null" />.</exception>
<returns>  <see langword="true" /> if the <see cref="T:System.Uri" /> was successfully created; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Uri.GetComponents(System.UriComponents, System.UriFormat)</br>
**签名**：_2f8a2b702985a27d</br>
**注释**：

```xml
<summary>Gets the specified components of the current instance using the specified escaping for special characters.</summary>
<param name="components">A bitwise combination of the <see cref="T:System.UriComponents" /> values that specifies which parts of the current instance to return to the caller.</param>
<param name="format">One of the enumeration values that controls how special characters are escaped.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="components" /> is not a combination of valid <see cref="T:System.UriComponents" /> values.</exception>
<exception cref="T:System.InvalidOperationException">The current <see cref="T:System.Uri" /> is not an absolute URI. Relative URIs cannot be used with this method.</exception>
<returns>The components of the current instance.</returns>
```

**成员**：static System.Uri.Compare(System.Uri, System.Uri, System.UriComponents, System.UriFormat, System.StringComparison)</br>
**签名**：_e5a72adb727a6498</br>
**注释**：

```xml
<summary>Compares the specified parts of two URIs using the specified comparison rules.</summary>
<param name="uri1">The first URI.</param>
<param name="uri2">The second URI.</param>
<param name="partsToCompare">A bitwise combination of the <see cref="T:System.UriComponents" /> values that specifies the parts of <paramref name="uri1" /> and <paramref name="uri2" /> to compare.</param>
<param name="compareFormat">One of the enumeration values that specifies the character escaping used when the URI components are compared.</param>
<param name="comparisonType">One of the enumeration values that specifies the culture, case, and sort rules for the comparison.</param>
<exception cref="T:System.ArgumentException">  <paramref name="comparisonType" /> is not a valid <see cref="T:System.StringComparison" /> value.</exception>
<returns>A value that indicates the lexical relationship between the compared <see cref="T:System.Uri" /> components. <list type="table"><listheader><term> Value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description><paramref name="uri1" /> is less than <paramref name="uri2" />.</description></item><item><term> Zero</term><description><paramref name="uri1" /> equals <paramref name="uri2" />.</description></item><item><term> Greater than zero</term><description><paramref name="uri1" /> is greater than <paramref name="uri2" />.</description></item></list></returns>
```

**成员**：System.Uri.IsWellFormedOriginalString()</br>
**签名**：_79b58cf4d4ee0163</br>
**注释**：

```xml
<summary>Indicates whether the string used to construct this <see cref="T:System.Uri" /> was well-formed and does not require further escaping.</summary>
<returns>  <see langword="true" /> if the string was well-formed; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Uri.IsWellFormedUriString(string, System.UriKind)</br>
**签名**：_fb8f84af7d4c2fb5</br>
**注释**：

```xml
<summary>Indicates whether the string is well-formed by attempting to construct a URI with the string and ensures that the string does not require further escaping.</summary>
<param name="uriString">The string used to attempt to construct a <see cref="T:System.Uri" />.</param>
<param name="uriKind">The type of the <see cref="T:System.Uri" /> in <paramref name="uriString" />.</param>
<returns>  <see langword="true" /> if the string was well-formed; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Uri.UnescapeDataString(string)</br>
**签名**：_5fc501940cb47432</br>
**注释**：

```xml
<summary>Converts a string to its unescaped representation.</summary>
<param name="stringToUnescape">The string to unescape.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="stringToUnescape" /> is <see langword="null" />.</exception>
<returns>The unescaped representation of <paramref name="stringToUnescape" />.</returns>
```

**成员**：static System.Uri.UnescapeDataString(System.ReadOnlySpan<char>)</br>
**签名**：_163a2890feba4ab7</br>
**注释**：

```xml
<summary>Converts a span to its unescaped representation.</summary>
<param name="charsToUnescape">The span to unescape.</param>
<returns>The unescaped representation of <paramref name="charsToUnescape" />.</returns>
```

**成员**：static System.Uri.TryUnescapeDataString(System.ReadOnlySpan<char>, System.Span<char>, out int)</br>
**签名**：_e734a8707999cd87</br>
**注释**：

```xml
<summary>Attempts to convert a span to its unescaped representation.</summary>
<param name="charsToUnescape">The span to unescape.</param>
<param name="destination">The output span that contains the unescaped result of the operation.</param>
<param name="charsWritten">When this method returns, contains the number of chars that were written into <paramref name="destination" />.</param>
<returns>  <see langword="true" /> if the <paramref name="destination" /> was large enough to hold the entire result; otherwise, <see langword="false" />.</returns>
```

**成员**：static System.Uri.EscapeUriString(string)</br>
**签名**：_4cfca0b97aa1f937</br>
**注释**：

```xml
<summary>Converts a URI string to its escaped representation.</summary>
<param name="stringToEscape">The string to escape.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="stringToEscape" /> is <see langword="null" />.</exception>
<exception cref="T:System.UriFormatException">The length of <paramref name="stringToEscape" /> exceeds 32766 characters.        Note: In .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.FormatException" />, instead.</exception>
<returns>The escaped representation of <paramref name="stringToEscape" />.</returns>
```

**成员**：static System.Uri.EscapeDataString(string)</br>
**签名**：_0ee9999fc98d77d8</br>
**注释**：

```xml
<summary>Converts a string to its escaped representation.</summary>
<param name="stringToEscape">The string to escape.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="stringToEscape" /> is <see langword="null" />.</exception>
<exception cref="T:System.UriFormatException">Note: In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.FormatException" />, instead. The length of <paramref name="stringToEscape" /> exceeds 32766 characters.</exception>
<returns>The escaped representation of <paramref name="stringToEscape" />.</returns>
```

**成员**：static System.Uri.EscapeDataString(System.ReadOnlySpan<char>)</br>
**签名**：_981c6f695c23eff5</br>
**注释**：

```xml
<summary>Converts a span to its escaped representation.</summary>
<param name="charsToEscape">The span to escape.</param>
<returns>The escaped representation of <paramref name="charsToEscape" />.</returns>
```

**成员**：static System.Uri.TryEscapeDataString(System.ReadOnlySpan<char>, System.Span<char>, out int)</br>
**签名**：_8edf9b05bef27d8d</br>
**注释**：

```xml
<summary>Attempts to convert a span to its escaped representation.</summary>
<param name="charsToEscape">The span to escape.</param>
<param name="destination">The output span that contains the escaped result of the operation.</param>
<param name="charsWritten">When this method returns, contains the number of chars that were written into <paramref name="destination" />.</param>
<returns>  <see langword="true" /> if the <paramref name="destination" /> was large enough to hold the entire result; otherwise, <see langword="false" />.</returns>
```

**成员**：System.Uri.IsBaseOf(System.Uri)</br>
**签名**：_7146053eca342c63</br>
**注释**：

```xml
<summary>Determines whether the current <see cref="T:System.Uri" /> instance is a base of the specified <see cref="T:System.Uri" /> instance.</summary>
<param name="uri">The specified URI to test.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="uri" /> is <see langword="null" />.</exception>
<returns>  <see langword="true" /> if the current <see cref="T:System.Uri" /> instance is a base of <paramref name="uri" />; otherwise, <see langword="false" />.</returns>
```
