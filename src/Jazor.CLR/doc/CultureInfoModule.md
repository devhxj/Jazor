# CultureInfoModule.cs

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：System.Globalization.CultureInfo.CultureInfo(string)</br>
**签名**：_b7486264ae338f27</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureInfo" /> class based on the culture specified by name.</summary>
<param name="name">A predefined <see cref="T:System.Globalization.CultureInfo" /> name, <see cref="P:System.Globalization.CultureInfo.Name" /> of an existing <see cref="T:System.Globalization.CultureInfo" />, or Windows-only culture name. <paramref name="name" /> is not case-sensitive.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="name" /> is null.</exception>
<exception cref="T:System.Globalization.CultureNotFoundException">  <paramref name="name" /> is not a valid culture name. For more information, see the Notes to Callers section.</exception>
```

**成员**：System.Globalization.CultureInfo.CultureInfo(string, bool)</br>
**签名**：_df21a93fd9f84197</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureInfo" /> class based on the culture specified by name and on a value that specifies whether to use the user-selected culture settings from Windows.</summary>
<param name="name">A predefined <see cref="T:System.Globalization.CultureInfo" /> name, <see cref="P:System.Globalization.CultureInfo.Name" /> of an existing <see cref="T:System.Globalization.CultureInfo" />, or Windows-only culture name. <paramref name="name" /> is not case-sensitive.</param>
<param name="useUserOverride">  <see langword="true" /> to use the user-selected culture settings (Windows only); <see langword="false" /> to use the default culture settings.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="name" /> is null.</exception>
<exception cref="T:System.Globalization.CultureNotFoundException">  <paramref name="name" /> is not a valid culture name. See the Notes to Callers section for more information.</exception>
```

**成员**：System.Globalization.CultureInfo.CultureInfo(int)</br>
**签名**：_22aaac09e253b1f9</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureInfo" /> class based on the culture specified by the culture identifier.</summary>
<param name="culture">A predefined <see cref="T:System.Globalization.CultureInfo" /> identifier, <see cref="P:System.Globalization.CultureInfo.LCID" /> property of an existing <see cref="T:System.Globalization.CultureInfo" /> object, or Windows-only culture identifier.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="culture" /> is less than zero.</exception>
<exception cref="T:System.Globalization.CultureNotFoundException">  <paramref name="culture" /> is not a valid culture identifier. See the Notes to Callers section for more information.</exception>
```

**成员**：System.Globalization.CultureInfo.CultureInfo(int, bool)</br>
**签名**：_d0948ef9f698ec85</br>
**注释**：

```xml
<summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureInfo" /> class based on the culture specified by the culture identifier and on a value that specifies whether to use the user-selected culture settings from Windows.</summary>
<param name="culture">A predefined <see cref="T:System.Globalization.CultureInfo" /> identifier, <see cref="P:System.Globalization.CultureInfo.LCID" /> property of an existing <see cref="T:System.Globalization.CultureInfo" /> object, or Windows-only culture identifier.</param>
<param name="useUserOverride">  <see langword="true" /> to use the user-selected culture settings (Windows only); <see langword="false" /> to use the default culture settings.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="culture" /> is less than zero.</exception>
<exception cref="T:System.Globalization.CultureNotFoundException">  <paramref name="culture" /> is not a valid culture identifier. See the Notes to Callers section for more information.</exception>
```

**成员**：static System.Globalization.CultureInfo.CreateSpecificCulture(string)</br>
**签名**：_a078d5ccbbf2345a</br>
**注释**：

```xml
<summary>Creates a <see cref="T:System.Globalization.CultureInfo" /> that represents the specific culture that is associated with the specified name.</summary>
<param name="name">A predefined <see cref="T:System.Globalization.CultureInfo" /> name or the name of an existing <see cref="T:System.Globalization.CultureInfo" /> object. <paramref name="name" /> is not case-sensitive.</param>
<exception cref="T:System.Globalization.CultureNotFoundException">  <paramref name="name" /> is not a valid culture name. -or- The culture specified by <paramref name="name" /> does not have a specific culture associated with it.</exception>
<exception cref="T:System.NullReferenceException">  <paramref name="name" /> is null.</exception>
<returns>A <see cref="T:System.Globalization.CultureInfo" /> object that represents: The invariant culture, if <paramref name="name" /> is an empty string (""). -or- The specific culture associated with <paramref name="name" />, if <paramref name="name" /> is a neutral culture. -or- The culture specified by <paramref name="name" />, if <paramref name="name" /> is already a specific culture.</returns>
```

**成员**：static System.Globalization.CultureInfo.CurrentCulture.get</br>
**签名**：_1a26e2e2e4e0ca1d</br>

**成员**：static System.Globalization.CultureInfo.CurrentCulture.set</br>
**签名**：_82cfca57d721204e</br>

**成员**：static System.Globalization.CultureInfo.CurrentUICulture.get</br>
**签名**：_eca32c250ead7de9</br>

**成员**：static System.Globalization.CultureInfo.CurrentUICulture.set</br>
**签名**：_7e355a1a63351619</br>

**成员**：static System.Globalization.CultureInfo.InstalledUICulture.get</br>
**签名**：_98e743867688a06d</br>

**成员**：static System.Globalization.CultureInfo.DefaultThreadCurrentCulture.get</br>
**签名**：_3c1fdac9ccc43427</br>

**成员**：static System.Globalization.CultureInfo.DefaultThreadCurrentCulture.set</br>
**签名**：_96d14148886217cb</br>

**成员**：static System.Globalization.CultureInfo.DefaultThreadCurrentUICulture.get</br>
**签名**：_abdb5d2bfd934cfc</br>

**成员**：static System.Globalization.CultureInfo.DefaultThreadCurrentUICulture.set</br>
**签名**：_12da8bfb928d7414</br>

**成员**：static System.Globalization.CultureInfo.InvariantCulture.get</br>
**签名**：_e4c4d53d69e72382</br>

**成员**：virtual System.Globalization.CultureInfo.Parent.get</br>
**签名**：_cd29576576563da3</br>

**成员**：virtual System.Globalization.CultureInfo.LCID.get</br>
**签名**：_9152aa33e0560712</br>

**成员**：virtual System.Globalization.CultureInfo.KeyboardLayoutId.get</br>
**签名**：_13b0607d8916da7b</br>

**成员**：static System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes)</br>
**签名**：_40087650ec4f5285</br>
**注释**：

```xml
<summary>Gets the list of supported cultures filtered by the specified <see cref="T:System.Globalization.CultureTypes" /> parameter.</summary>
<param name="types">A bitwise combination of the enumeration values that filter the cultures to retrieve.</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="types" /> specifies an invalid combination of <see cref="T:System.Globalization.CultureTypes" /> values.</exception>
<returns>An array that contains the cultures specified by the <paramref name="types" /> parameter. The array of cultures is unsorted.</returns>
```

**成员**：virtual System.Globalization.CultureInfo.Name.get</br>
**签名**：_822a986168c7c539</br>

**成员**：System.Globalization.CultureInfo.IetfLanguageTag.get</br>
**签名**：_9c9f6e469362911e</br>

**成员**：virtual System.Globalization.CultureInfo.DisplayName.get</br>
**签名**：_59b041331098ad55</br>

**成员**：virtual System.Globalization.CultureInfo.NativeName.get</br>
**签名**：_a4804f687bfc0013</br>

**成员**：virtual System.Globalization.CultureInfo.EnglishName.get</br>
**签名**：_97ad9637d1f75e7c</br>

**成员**：virtual System.Globalization.CultureInfo.TwoLetterISOLanguageName.get</br>
**签名**：_112fba1dc945fa1a</br>

**成员**：virtual System.Globalization.CultureInfo.ThreeLetterISOLanguageName.get</br>
**签名**：_285ede13a469ce7b</br>

**成员**：virtual System.Globalization.CultureInfo.ThreeLetterWindowsLanguageName.get</br>
**签名**：_1f981ccac713f3d9</br>

**成员**：virtual System.Globalization.CultureInfo.CompareInfo.get</br>
**签名**：_90f3bc0ef0b5d452</br>

**成员**：virtual System.Globalization.CultureInfo.TextInfo.get</br>
**签名**：_e82427b8b3bb35c4</br>

**成员**：override System.Globalization.CultureInfo.Equals(object)</br>
**签名**：_dfe1a8cc1c9e5e52</br>
**注释**：

```xml
<summary>Determines whether the specified object is the same culture as the current <see cref="T:System.Globalization.CultureInfo" />.</summary>
<param name="value">The object to compare with the current <see cref="T:System.Globalization.CultureInfo" />.</param>
<returns>  <see langword="true" /> if <paramref name="value" /> is the same culture as the current <see cref="T:System.Globalization.CultureInfo" />; otherwise, <see langword="false" />.</returns>
```

**成员**：override System.Globalization.CultureInfo.GetHashCode()</br>
**签名**：_b3aae6e43cf38d8a</br>
**注释**：

```xml
<summary>Serves as a hash function for the current <see cref="T:System.Globalization.CultureInfo" />, suitable for hashing algorithms and data structures, such as a hash table.</summary>
<returns>A hash code for the current <see cref="T:System.Globalization.CultureInfo" />.</returns>
```

**成员**：override System.Globalization.CultureInfo.ToString()</br>
**签名**：_559b27327f84f1af</br>
**注释**：

```xml
<summary>Returns a string containing the name of the current <see cref="T:System.Globalization.CultureInfo" /> in the format languagecode2-country/regioncode2.</summary>
<returns>A string containing the name of the current <see cref="T:System.Globalization.CultureInfo" />.</returns>
```

**成员**：virtual System.Globalization.CultureInfo.GetFormat(System.Type)</br>
**签名**：_f8c5b22a1e711ffe</br>
**注释**：

```xml
<summary>Gets an object that defines how to format the specified type.</summary>
<param name="formatType">The <see cref="T:System.Type" /> for which to get a formatting object. This method only supports the <see cref="T:System.Globalization.NumberFormatInfo" /> and <see cref="T:System.Globalization.DateTimeFormatInfo" /> types.</param>
<returns>The value of the <see cref="P:System.Globalization.CultureInfo.NumberFormat" /> property, which is a <see cref="T:System.Globalization.NumberFormatInfo" /> containing the default number format information for the current <see cref="T:System.Globalization.CultureInfo" />, if <paramref name="formatType" /> is the <see cref="T:System.Type" /> object for the <see cref="T:System.Globalization.NumberFormatInfo" /> class. -or- The value of the <see cref="P:System.Globalization.CultureInfo.DateTimeFormat" /> property, which is a <see cref="T:System.Globalization.DateTimeFormatInfo" /> containing the default date and time format information for the current <see cref="T:System.Globalization.CultureInfo" />, if <paramref name="formatType" /> is the <see cref="T:System.Type" /> object for the <see cref="T:System.Globalization.DateTimeFormatInfo" /> class. -or- null, if <paramref name="formatType" /> is any other object.</returns>
```

**成员**：virtual System.Globalization.CultureInfo.IsNeutralCulture.get</br>
**签名**：_0bedb111138c14ed</br>

**成员**：System.Globalization.CultureInfo.CultureTypes.get</br>
**签名**：_7309acaa147028c6</br>

**成员**：virtual System.Globalization.CultureInfo.NumberFormat.get</br>
**签名**：_7472734ec9a97b33</br>

**成员**：virtual System.Globalization.CultureInfo.NumberFormat.set</br>
**签名**：_5943bc5946aadc23</br>

**成员**：virtual System.Globalization.CultureInfo.DateTimeFormat.get</br>
**签名**：_3084f61a73019848</br>

**成员**：virtual System.Globalization.CultureInfo.DateTimeFormat.set</br>
**签名**：_a72ad1794743a630</br>

**成员**：System.Globalization.CultureInfo.ClearCachedData()</br>
**签名**：_73e163fe0d6f4c41</br>
**注释**：

```xml
<summary>Refreshes cached culture-related information.</summary>
```

**成员**：virtual System.Globalization.CultureInfo.Calendar.get</br>
**签名**：_2ab4f6aaba1be337</br>

**成员**：virtual System.Globalization.CultureInfo.OptionalCalendars.get</br>
**签名**：_5031598284c711b5</br>

**成员**：System.Globalization.CultureInfo.UseUserOverride.get</br>
**签名**：_4b6ab04957c3b1d8</br>

**成员**：System.Globalization.CultureInfo.GetConsoleFallbackUICulture()</br>
**签名**：_e746a9049464da41</br>
**注释**：

```xml
<summary>Gets an alternate user interface culture suitable for console applications when the default graphic user interface culture is unsuitable.</summary>
<returns>An alternate culture that is used to read and display text on the console.</returns>
```

**成员**：virtual System.Globalization.CultureInfo.Clone()</br>
**签名**：_52d3a5ff068445a1</br>
**注释**：

```xml
<summary>Creates a copy of the current <see cref="T:System.Globalization.CultureInfo" />.</summary>
<returns>A copy of the current <see cref="T:System.Globalization.CultureInfo" />.</returns>
```

**成员**：static System.Globalization.CultureInfo.ReadOnly(System.Globalization.CultureInfo)</br>
**签名**：_f3218a923929edaf</br>
**注释**：

```xml
<summary>Returns a read-only wrapper around the specified <see cref="T:System.Globalization.CultureInfo" /> object.</summary>
<param name="ci">The <see cref="T:System.Globalization.CultureInfo" /> object to wrap.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="ci" /> is null.</exception>
<returns>A read-only <see cref="T:System.Globalization.CultureInfo" /> wrapper around <paramref name="ci" />.</returns>
```

**成员**：System.Globalization.CultureInfo.IsReadOnly.get</br>
**签名**：_1a2fc3e83feec6fd</br>

**成员**：static System.Globalization.CultureInfo.GetCultureInfo(int)</br>
**签名**：_be269d85f3085630</br>
**注释**：

```xml
<summary>Retrieves a cached, read-only instance of a culture by using the specified culture identifier.</summary>
<param name="culture">A locale identifier (LCID).</param>
<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="culture" /> is less than zero.</exception>
<exception cref="T:System.Globalization.CultureNotFoundException">  <paramref name="culture" /> specifies a culture that is not supported. See the Notes to Caller section for more information.</exception>
<returns>A read-only <see cref="T:System.Globalization.CultureInfo" /> object.</returns>
```

**成员**：static System.Globalization.CultureInfo.GetCultureInfo(string)</br>
**签名**：_a536c354b66082b9</br>
**注释**：

```xml
<summary>Retrieves a cached, read-only instance of a culture using the specified culture name.</summary>
<param name="name">The name of a culture. <paramref name="name" /> is not case-sensitive.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="name" /> is null.</exception>
<exception cref="T:System.Globalization.CultureNotFoundException">  <paramref name="name" /> specifies a culture that is not supported. See the Notes to Callers section for more information.</exception>
<returns>A read-only <see cref="T:System.Globalization.CultureInfo" /> object.</returns>
```

**成员**：static System.Globalization.CultureInfo.GetCultureInfo(string, string)</br>
**签名**：_e17d240a4c1653be</br>
**注释**：

```xml
<summary>Retrieves a cached, read-only instance of a culture. Parameters specify a culture that is initialized with the <see cref="T:System.Globalization.TextInfo" /> and <see cref="T:System.Globalization.CompareInfo" /> objects specified by another culture.</summary>
<param name="name">The name of a culture. <paramref name="name" /> is not case-sensitive.</param>
<param name="altName">The name of a culture that supplies the <see cref="T:System.Globalization.TextInfo" /> and <see cref="T:System.Globalization.CompareInfo" /> objects used to initialize <paramref name="name" />. <paramref name="altName" /> is not case-sensitive.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="name" /> or <paramref name="altName" /> is null.</exception>
<exception cref="T:System.Globalization.CultureNotFoundException">  <paramref name="name" /> or <paramref name="altName" /> specifies a culture that is not supported. See the Notes to Callers section for more information.</exception>
<returns>A read-only <see cref="T:System.Globalization.CultureInfo" /> object.</returns>
```

**成员**：static System.Globalization.CultureInfo.GetCultureInfo(string, bool)</br>
**签名**：_a43a2bb07ef29293</br>
**注释**：

```xml
<summary>Retrieves a cached, read-only instance of a culture.</summary>
<param name="name">The name of a culture. It is not case-sensitive.</param>
<param name="predefinedOnly">  <see langword="true" /> if requesting to create an instance of a culture that is known by the platform. <see langword="false" /> if it is ok to retreive a made-up culture even if the platform does not carry data for it.</param>
<returns>A read-only instance of a culture.</returns>
```

**成员**：static System.Globalization.CultureInfo.GetCultureInfoByIetfLanguageTag(string)</br>
**签名**：_1d57f4ce6dee8a81</br>
**注释**：

```xml
<summary>Deprecated. Retrieves a read-only <see cref="T:System.Globalization.CultureInfo" /> object having linguistic characteristics that are identified by the specified RFC 4646 language tag.</summary>
<param name="name">The name of a language as specified by the RFC 4646 standard.</param>
<exception cref="T:System.ArgumentNullException">  <paramref name="name" /> is null.</exception>
<exception cref="T:System.Globalization.CultureNotFoundException">  <paramref name="name" /> does not correspond to a supported culture.</exception>
<returns>A read-only <see cref="T:System.Globalization.CultureInfo" /> object.</returns>
```

