import { GetHashCodeCore } from "System/Collections/Generic/EqualityComparerT1Module.js";
import { _e1d2ba750a2788cb, createDefault } from "System/Collections/Generic/HashSetT1Module.js";
import { GetStringRepresentation, MaterializeReadOnlyCharSpan } from "System/RuntimeModule.js";
function EnsureNonNegativeWholeNumber(value, parameterName) {
  if (isNaN(value) || Math.floor(value) !== value || value < 0)
    throw new Error(`ArgumentOutOfRangeException: ${parameterName ?? ""} must be a non-negative whole number.`);
}
function JoinCharacters(value, startIndex, length) {
  let parts = new Array;
  for (let index = startIndex; index < startIndex + length; index++)
    parts.push(value[index]);
  return parts.join("");
}
function EnsureStringIndex(value, index, parameterName) {
  EnsureNonNegativeWholeNumber(index, parameterName);
  if (index > value.length)
    throw new Error(`ArgumentOutOfRangeException: ${parameterName ?? ""} is outside the string.`);
}
function CompareOrdinalRange(strA, indexA, strB, indexB, length) {
  if (strA === null)
    return strB === null ? 0 : -1;
  if (strB === null)
    return 1;
  EnsureStringIndex(strA, indexA, "indexA");
  EnsureStringIndex(strB, indexB, "indexB");
  EnsureNonNegativeWholeNumber(length, "length");
  let availableA = strA.length - indexA;
  let availableB = strB.length - indexB;
  let countA = length < availableA ? length : availableA;
  let countB = length < availableB ? length : availableB;
  let sharedCount = countA < countB ? countA : countB;
  for (let offset = 0; offset < sharedCount; offset++) {
    let difference = strA.charCodeAt(indexA + offset) - strB.charCodeAt(indexB + offset);
    if (difference !== 0)
      return difference;
  }
  return countA - countB;
}
function CopyCharacters(instance, sourceIndex, destination, destinationIndex, count) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  EnsureStringIndex(instance, sourceIndex, "sourceIndex");
  if (destination === null)
    throw new Error("ArgumentNullException: destination is null.");
  EnsureNonNegativeWholeNumber(destinationIndex, "destinationIndex");
  EnsureNonNegativeWholeNumber(count, "count");
  if (sourceIndex > instance.length - count)
    throw new Error("ArgumentOutOfRangeException: sourceIndex and count exceed the string.");
  if (destinationIndex > destination.length - count)
    throw new Error("ArgumentException: destination array is too small.");
  for (let offset = 0; offset < count; offset++)
    destination[destinationIndex + offset] = _5ad63706a889c294(instance, sourceIndex + offset);
}
function ConcatStrings_3b3f354fc6bc40c2(values, separator, parameterName) {
  if (values === null)
    throw new Error(`ArgumentNullException: ${parameterName ?? ""} is null.`);
  let parts = new Array;
  let first = true;
  for (let value of values) {
    if (!first)
      parts.push(separator);
    parts.push(value ?? "");
    first = false;
  }
  return parts.join("");
}
function ConcatStrings_12b3733691d99e1e(values, separator, parameterName) {
  if (values === null)
    throw new Error(`ArgumentNullException: ${parameterName ?? ""} is null.`);
  let parts = new Array;
  for (let index = 0; index < values.length; index++) {
    if (index !== 0)
      parts.push(separator);
    parts.push(values[index] ?? "");
  }
  return parts.join("");
}
function ConcatValues_0d1ceed252aad03f(values, separator, parameterName) {
  if (values === null)
    throw new Error(`ArgumentNullException: ${parameterName ?? ""} is null.`);
  let parts = new Array;
  let first = true;
  for (let value of values) {
    if (!first)
      parts.push(separator);
    parts.push(GetStringRepresentation(value));
    first = false;
  }
  return parts.join("");
}
function ConcatValues_865ce1c5554d72f2(values, separator, parameterName) {
  if (values === null)
    throw new Error(`ArgumentNullException: ${parameterName ?? ""} is null.`);
  let parts = new Array;
  for (let index = 0; index < values.length; index++) {
    if (index !== 0)
      parts.push(separator);
    parts.push(GetStringRepresentation(values[index]));
  }
  return parts.join("");
}
function JoinRange(separator, value, startIndex, count) {
  if (value === null)
    throw new Error("ArgumentNullException: value is null.");
  EnsureNonNegativeWholeNumber(startIndex, "startIndex");
  EnsureNonNegativeWholeNumber(count, "count");
  if (startIndex > value.length - count)
    throw new Error("ArgumentOutOfRangeException: startIndex and count must identify a valid range.");
  let parts = new Array;
  for (let index = startIndex; index < startIndex + count; index++) {
    if (index !== startIndex)
      parts.push(separator);
    parts.push(value[index] ?? "");
  }
  return parts.join("");
}
function GetNormalizationForm(normalizationForm) {
  if (normalizationForm === 1)
    return "NFC";
  if (normalizationForm === 2)
    return "NFD";
  if (normalizationForm === 5)
    return "NFKC";
  if (normalizationForm === 6)
    return "NFKD";
  throw new Error("ArgumentException: Invalid normalization form.");
}
function ReplaceLineEndingsCore(instance, replacementText) {
  if (replacementText === null)
    throw new Error("ArgumentNullException: replacementText is null.");
  let parts = new Array;
  let segmentStart = 0;
  for (let index = 0; index < instance.length; index++) {
    let codeUnit = instance.charCodeAt(index);
    let isCarriageReturn = codeUnit === 13;
    let isLineEnding = isCarriageReturn || codeUnit === 10 || codeUnit === 12 || codeUnit === 133 || codeUnit === 8232 || codeUnit === 8233;
    if (!isLineEnding)
      continue;
    parts.push(instance.substring(segmentStart, segmentStart + (index - segmentStart)));
    parts.push(replacementText);
    if (isCarriageReturn && index + 1 < instance.length && instance.charCodeAt(index + 1) === 10)
      index++;
    segmentStart = index + 1;
  }
  if (segmentStart === 0)
    return instance;
  parts.push(instance.substring(segmentStart));
  return parts.join("");
}
function TrimReadOnlyCharacterSpan(instance, trimChars, trimStart, trimEnd) {
  let characters = MaterializeReadOnlyCharSpan(trimChars);
  if (characters.length === 0)
    return instance;
  return TrimCharacterSet(instance, NormalizeCharSet(characters), trimStart, trimEnd);
}
function GetInternedString(value) {
  if (value === null)
    throw new Error("ArgumentNullException: str is null.");
  return value;
}
/*jazor:clr-member static string.Intern(string)*/
export function _1234444e218b96c3(str) {
  return GetInternedString(str);
}
/*jazor:clr-member static string.Compare(string, string)*/
export function _e16eea9fe3891a62(strA, strB) {
  if (strA === null && strB === null)
    return 0;
  if (strA === null)
    return -1;
  if (strB === null)
    return 1;
  if (strA < strB)
    return -1;
  if (strA > strB)
    return 1;
  return 0;
}
/*jazor:clr-member static string.Compare(string, string, bool)*/
export function _20874c0b43640318(strA, strB, ignoreCase) {
  if (strA === null && strB === null)
    return 0;
  if (strA === null)
    return -1;
  if (strB === null)
    return 1;
  let a = ignoreCase ? strA.toLowerCase() : strA;
  let b = ignoreCase ? strB.toLowerCase() : strB;
  if (a < b)
    return -1;
  if (a > b)
    return 1;
  return 0;
}
/*jazor:clr-member static string.Compare(string, string, System.StringComparison)*/
export function _9d940114ace1198f(strA, strB, comparisonType) {
  return _20874c0b43640318(strA, strB, IsOrdinalIgnoreCase(comparisonType));
}
/*jazor:clr-member static string.Compare(string, int, string, int, int, System.StringComparison)*/
export function _d78fb9d76fca75e4(strA, indexA, strB, indexB, length, comparisonType) {
  let sliceA = SliceOrEmpty(strA, indexA, length);
  let sliceB = SliceOrEmpty(strB, indexB, length);
  return _20874c0b43640318(sliceA, sliceB, IsOrdinalIgnoreCase(comparisonType));
}
/*jazor:clr-member static string.CompareOrdinal(string, string)*/
export function _a55d307de6e31c7b(strA, strB) {
  return _e16eea9fe3891a62(strA, strB);
}
/*jazor:clr-member static string.CompareOrdinal(string, int, string, int, int)*/
export function _dc789454b6ef6bcb(strA, indexA, strB, indexB, length) {
  return CompareOrdinalRange(strA, indexA, strB, indexB, length);
}
/*jazor:clr-member string.CompareTo(object)*/
export function _629b0613344d82e7(instance, value) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  if (value === null)
    return 1;
  if (typeof value !== "string")
    throw new Error("ArgumentException: Object must be of type String.");
  return _380e7c7649d703f0(instance, value);
}
/*jazor:clr-member string.CompareTo(string)*/
export function _380e7c7649d703f0(instance, strB) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  if (strB === null)
    return 1;
  if (instance < strB)
    return -1;
  if (instance > strB)
    return 1;
  return 0;
}
/*jazor:clr-member string.EndsWith(string, System.StringComparison)*/
export function _946b7129a48c8114(instance, value, comparisonType) {
  return IsOrdinalIgnoreCase(comparisonType) ? instance.toLowerCase().endsWith(value.toLowerCase()) : instance.endsWith(value);
}
/*jazor:clr-member string.Equals(string, System.StringComparison)*/
export function _f8e1e01e8c17e8bb(instance, value, comparisonType) {
  let __cacc$2da9859b7aebd15387b7dce4, __cacc$c8f06dd595aa65f98847bb10;
  return IsOrdinalIgnoreCase(comparisonType) ? (__cacc$2da9859b7aebd15387b7dce4 = instance, __cacc$2da9859b7aebd15387b7dce4 == null ? undefined : __cacc$2da9859b7aebd15387b7dce4.toLowerCase()) === (__cacc$c8f06dd595aa65f98847bb10 = value, __cacc$c8f06dd595aa65f98847bb10 == null ? undefined : __cacc$c8f06dd595aa65f98847bb10.toLowerCase()) : instance === value;
}
/*jazor:clr-member static string.Equals(string, string, System.StringComparison)*/
export function _b7c36408f0f172e9(a, b, comparisonType) {
  let __cacc$1a0d72065ce8db1d27f6f1f2, __cacc$8e7af1ee27a2ea99cd05a56d;
  return IsOrdinalIgnoreCase(comparisonType) ? (__cacc$1a0d72065ce8db1d27f6f1f2 = a, __cacc$1a0d72065ce8db1d27f6f1f2 == null ? undefined : __cacc$1a0d72065ce8db1d27f6f1f2.toLowerCase()) === (__cacc$8e7af1ee27a2ea99cd05a56d = b, __cacc$8e7af1ee27a2ea99cd05a56d == null ? undefined : __cacc$8e7af1ee27a2ea99cd05a56d.toLowerCase()) : a === b;
}
/*jazor:clr-member override string.GetHashCode()*/
export function _bccdd3f386a6fbbc(instance) {
  return GetHashCodeCore(instance);
}
/*jazor:clr-member string.GetHashCode(System.StringComparison)*/
export function _04edfc3090710ca7(instance, comparisonType) {
  EnsureOrdinalHashComparison(comparisonType);
  return GetHashCodeCore(instance);
}
/*jazor:clr-member static string.GetHashCode(System.ReadOnlySpan<char>)*/
export function _4598a18be32f839d(value) {
  return GetHashCodeCore(MaterializeReadOnlyCharSpan(value));
}
/*jazor:clr-member static string.GetHashCode(System.ReadOnlySpan<char>, System.StringComparison)*/
export function _d123047f69d911f5(value, comparisonType) {
  EnsureOrdinalHashComparison(comparisonType);
  return GetHashCodeCore(MaterializeReadOnlyCharSpan(value));
}
function IsOrdinalIgnoreCase(comparisonType) {
  let value;
  return typeof comparisonType === "number" && (value = comparisonType, true) && value === 5;
}
function EnsureOrdinalHashComparison(comparisonType) {
  if (comparisonType === 4)
    return;
  if (comparisonType >= 0 && comparisonType <= 5)
    throw new Error("NotSupportedException: string hash comparison currently supports only StringComparison.Ordinal.");
  throw new Error("ArgumentException: comparisonType is not a valid StringComparison value.");
}
function SliceOrEmpty(value, start, length) {
  if (!value)
    return value ?? "";
  if (start >= value.length || length <= 0)
    return "";
  let available = value.length - start;
  let take = length < available ? length : available;
  return value.substring(start, start + take);
}
/*jazor:clr-member string.StartsWith(string, System.StringComparison)*/
export function _0333a0fd5f67d8a0(instance, value, comparisonType) {
  return IsOrdinalIgnoreCase(comparisonType) ? instance.toLowerCase().startsWith(value.toLowerCase()) : instance.startsWith(value);
}
/*jazor:clr-member string.String(char[])*/
export function _6651b0a853e8e991(value) {
  if (value === null)
    throw new Error("ArgumentNullException: value is null.");
  return JoinCharacters(value, 0, value.length);
}
/*jazor:clr-member string.String(char[], int, int)*/
export function _ddce1a944159fc8b(value, startIndex, length) {
  if (value === null)
    throw new Error("ArgumentNullException: value is null.");
  EnsureNonNegativeWholeNumber(startIndex, "startIndex");
  EnsureNonNegativeWholeNumber(length, "length");
  if (startIndex > value.length - length)
    throw new Error("ArgumentOutOfRangeException: startIndex and length must identify a valid range.");
  return JoinCharacters(value, startIndex, length);
}
/*jazor:clr-member string.String(char, int)*/
export function _0ce0d88e18c041c8(c, count) {
  EnsureNonNegativeWholeNumber(count, "count");
  return c.repeat(count);
}
/*jazor:clr-member string.String(System.ReadOnlySpan<char>)*/
export function _009fee2e166a416d(value) {
  return MaterializeReadOnlyCharSpan(value);
}
/*jazor:clr-member static string.implicit operator System.ReadOnlySpan<char>(string)*/
export function _5ff800b094791eb0(value) {
  return value ?? "";
}
/*jazor:clr-member static string.Copy(string)*/
export function _0dc0a16fd99401f8(str) {
  if (str === null)
    throw new Error("ArgumentNullException: str is null.");
  return str;
}
/*jazor:clr-member string.CopyTo(int, char[], int, int)*/
export function _45bb6097c28a2f1e(instance, sourceIndex, destination, destinationIndex, count) {
  CopyCharacters(instance, sourceIndex, destination, destinationIndex, count);
}
/*jazor:clr-member string.IsNormalized()*/
export function _f645a0207f41fd4a(instance) {
  return instance === instance.normalize();
}
/*jazor:clr-member string.IsNormalized(System.Text.NormalizationForm)*/
export function _30d0ce62702ae938(instance, normalizationForm) {
  return instance === instance.normalize(GetNormalizationForm(normalizationForm));
}
/*jazor:clr-member string.Normalize()*/
export function _967ef647d59f3e39(instance) {
  return instance.normalize();
}
/*jazor:clr-member string.Normalize(System.Text.NormalizationForm)*/
export function _59b116010f03241b(instance, normalizationForm) {
  return instance.normalize(GetNormalizationForm(normalizationForm));
}
/*jazor:clr-member string.this[int].get*/
export function _5ad63706a889c294(instance, index) {
  if (index < 0 || index >= instance.length)
    throw new Error("IndexOutOfRangeException: index is out of range.");
  return instance.charAt(index);
}
/*jazor:clr-member static string.Concat(object)*/
export function _db938b9c2eb90d32(arg0) {
  return GetStringRepresentation(arg0);
}
/*jazor:clr-member static string.Concat(object, object)*/
export function _d330ca25546acf36(arg0, arg1) {
  return GetStringRepresentation(arg0) + GetStringRepresentation(arg1);
}
/*jazor:clr-member static string.Concat(object, object, object)*/
export function _dab9155adbef8f67(arg0, arg1, arg2) {
  return GetStringRepresentation(arg0) + GetStringRepresentation(arg1) + GetStringRepresentation(arg2);
}
/*jazor:clr-member static string.Concat(params object[])*/
export function _e102498b82e5b869(args) {
  return ConcatValues_865ce1c5554d72f2(args, "", "args");
}
/*jazor:clr-member static string.Concat(params System.ReadOnlySpan<object>)*/
export function _2d6a291b64a11ba3(args) {
  return ConcatValues_865ce1c5554d72f2(args, "", "args");
}
/*jazor:clr-member static string.Concat<T>(System.Collections.Generic.IEnumerable<T>)*/
export function _68574aee669f440f(values) {
  return ConcatValues_0d1ceed252aad03f(values, "", "values");
}
/*jazor:clr-member static string.Concat(System.Collections.Generic.IEnumerable<string>)*/
export function _a2a66aa54427416c(values) {
  return ConcatStrings_3b3f354fc6bc40c2(values, "", "values");
}
/*jazor:clr-member static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)*/
export function _a6102c27abe1ff18(str0, str1) {
  return MaterializeReadOnlyCharSpan(str0) + MaterializeReadOnlyCharSpan(str1);
}
/*jazor:clr-member static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)*/
export function _7de0cfb062a343ee(str0, str1, str2) {
  return MaterializeReadOnlyCharSpan(str0) + MaterializeReadOnlyCharSpan(str1) + MaterializeReadOnlyCharSpan(str2);
}
/*jazor:clr-member static string.Concat(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.ReadOnlySpan<char>)*/
export function _5177ae056c5ca775(str0, str1, str2, str3) {
  return MaterializeReadOnlyCharSpan(str0) + MaterializeReadOnlyCharSpan(str1) + MaterializeReadOnlyCharSpan(str2) + MaterializeReadOnlyCharSpan(str3);
}
/*jazor:clr-member static string.Concat(params string[])*/
export function _0f681227152a171b(values) {
  return ConcatStrings_12b3733691d99e1e(values, "", "values");
}
/*jazor:clr-member static string.Concat(params System.ReadOnlySpan<string>)*/
export function _22098d7fa5ce7a81(values) {
  return ConcatStrings_12b3733691d99e1e(values, "", "values");
}
/*jazor:clr-member static string.Format(string, object)*/
export function _980dff69bc3b8afa(format, arg0) {
  let __cacc$395d2e230d4fd7fca9c80836;
  if (format === null)
    throw new Error("ArgumentNullException: Format string cannot be null.");
  return format.replaceAll("{0}", (__cacc$395d2e230d4fd7fca9c80836 = arg0, __cacc$395d2e230d4fd7fca9c80836 == null ? undefined : __cacc$395d2e230d4fd7fca9c80836.toString()) ?? "");
}
/*jazor:clr-member static string.Format(string, object, object)*/
export function _8606f3cc36d1f8ed(format, arg0, arg1) {
  let __cacc$db3f58b3e92e4728c273593b, __cacc$bb303a86e839b2b3676c9874;
  if (format === null)
    throw new Error("ArgumentNullException: Format string cannot be null.");
  return format.replaceAll("{0}", (__cacc$db3f58b3e92e4728c273593b = arg0, __cacc$db3f58b3e92e4728c273593b == null ? undefined : __cacc$db3f58b3e92e4728c273593b.toString()) ?? "").replaceAll("{1}", (__cacc$bb303a86e839b2b3676c9874 = arg1, __cacc$bb303a86e839b2b3676c9874 == null ? undefined : __cacc$bb303a86e839b2b3676c9874.toString()) ?? "");
}
/*jazor:clr-member static string.Format(string, object, object, object)*/
export function _cda0978188193522(format, arg0, arg1, arg2) {
  let __cacc$44b0a6e5791917a29c4e538c, __cacc$780204a2b7a1d978e7335bb2, __cacc$80ebaa8df0b48ba4c27476b5;
  if (format === null)
    throw new Error("ArgumentNullException: Format string cannot be null.");
  return format.replaceAll("{0}", (__cacc$44b0a6e5791917a29c4e538c = arg0, __cacc$44b0a6e5791917a29c4e538c == null ? undefined : __cacc$44b0a6e5791917a29c4e538c.toString()) ?? "").replaceAll("{1}", (__cacc$780204a2b7a1d978e7335bb2 = arg1, __cacc$780204a2b7a1d978e7335bb2 == null ? undefined : __cacc$780204a2b7a1d978e7335bb2.toString()) ?? "").replaceAll("{2}", (__cacc$80ebaa8df0b48ba4c27476b5 = arg2, __cacc$80ebaa8df0b48ba4c27476b5 == null ? undefined : __cacc$80ebaa8df0b48ba4c27476b5.toString()) ?? "");
}
/*jazor:clr-member static string.Format(string, params object[])*/
export function _99b8bed2ce27774c(format, args) {
  if (format === null)
    throw new Error("ArgumentNullException: Format string cannot be null.");
  let result = format;
  for (let i = 0; i < args.length; i++) {
    let __cacc$9c599237b1c6729184068285;
    result = result.replaceAll("{" + i + "}", (__cacc$9c599237b1c6729184068285 = args[i], __cacc$9c599237b1c6729184068285 == null ? undefined : __cacc$9c599237b1c6729184068285.toString()) ?? "");
  }
  return result;
}
/*jazor:clr-member static string.Join(char, params string[])*/
export function _14ec7ebbb72b7d13(separator, value) {
  return ConcatStrings_12b3733691d99e1e(value, separator, "value");
}
/*jazor:clr-member static string.Join(char, params System.ReadOnlySpan<string>)*/
export function _9f939553178c2ca6(separator, value) {
  return ConcatStrings_12b3733691d99e1e(value, separator, "value");
}
/*jazor:clr-member static string.Join(string, params string[])*/
export function _f269cd27a4bbd549(separator, value) {
  return ConcatStrings_12b3733691d99e1e(value, separator ?? "", "value");
}
/*jazor:clr-member static string.Join(string, params System.ReadOnlySpan<string>)*/
export function _224682d778b9facf(separator, value) {
  return ConcatStrings_12b3733691d99e1e(value, separator ?? "", "value");
}
/*jazor:clr-member static string.Join(char, string[], int, int)*/
export function _f461a3c632706317(separator, value, startIndex, count) {
  return JoinRange(separator, value, startIndex, count);
}
/*jazor:clr-member static string.Join(string, string[], int, int)*/
export function _f1ad756b7baec84b(separator, value, startIndex, count) {
  return JoinRange(separator ?? "", value, startIndex, count);
}
/*jazor:clr-member static string.Join(string, System.Collections.Generic.IEnumerable<string>)*/
export function _d8814705c8078096(separator, values) {
  return ConcatStrings_3b3f354fc6bc40c2(values, separator ?? "", "values");
}
/*jazor:clr-member static string.Join(char, params object[])*/
export function _5ac0762c6816a423(separator, values) {
  return ConcatValues_865ce1c5554d72f2(values, separator, "values");
}
/*jazor:clr-member static string.Join(char, params System.ReadOnlySpan<object>)*/
export function _477a1f45d63f93c2(separator, values) {
  return ConcatValues_865ce1c5554d72f2(values, separator, "values");
}
/*jazor:clr-member static string.Join(string, params object[])*/
export function _c69ae51b8f3b72f0(separator, values) {
  return ConcatValues_865ce1c5554d72f2(values, separator ?? "", "values");
}
/*jazor:clr-member static string.Join(string, params System.ReadOnlySpan<object>)*/
export function _f8903c473c9e5f05(separator, values) {
  return ConcatValues_865ce1c5554d72f2(values, separator ?? "", "values");
}
/*jazor:clr-member static string.Join<T>(char, System.Collections.Generic.IEnumerable<T>)*/
export function _1c599eccbbc8f2b8(separator, values) {
  return ConcatValues_0d1ceed252aad03f(values, separator, "values");
}
/*jazor:clr-member static string.Join<T>(string, System.Collections.Generic.IEnumerable<T>)*/
export function _c78854b22e947a4f(separator, values) {
  return ConcatValues_0d1ceed252aad03f(values, separator ?? "", "values");
}
/*jazor:clr-member string.PadLeft(int)*/
export function _26620c4bafb4f435(instance, totalWidth) {
  EnsureNonNegativeWholeNumber(totalWidth, "totalWidth");
  return instance.padStart(totalWidth, " ");
}
/*jazor:clr-member string.PadLeft(int, char)*/
export function _7894e0294f780eb5(instance, totalWidth, paddingChar) {
  EnsureNonNegativeWholeNumber(totalWidth, "totalWidth");
  return instance.padStart(totalWidth, paddingChar);
}
/*jazor:clr-member string.PadRight(int)*/
export function _0e8f0a28fc1de8c2(instance, totalWidth) {
  EnsureNonNegativeWholeNumber(totalWidth, "totalWidth");
  return instance.padEnd(totalWidth, " ");
}
/*jazor:clr-member string.PadRight(int, char)*/
export function _685227781124d327(instance, totalWidth, paddingChar) {
  EnsureNonNegativeWholeNumber(totalWidth, "totalWidth");
  return instance.padEnd(totalWidth, paddingChar);
}
/*jazor:clr-member string.Replace(string, string, System.StringComparison)*/
export function _8a7510653022a974(instance, oldValue, newValue, comparisonType) {
  return IsOrdinalIgnoreCase(comparisonType) ? ReplaceAllIgnoreCase(instance, oldValue, newValue ?? "") : instance.replaceAll(oldValue, newValue ?? "");
}
/*jazor:clr-member string.Replace(char, char)*/
export function _7d7cb13bbbbb83c8(instance, oldChar, newChar) {
  return instance.replaceAll(oldChar.toString(), newChar.toString());
}
/*jazor:clr-member string.ReplaceLineEndings()*/
export function _3720e4de26fa4c1b(instance) {
  return ReplaceLineEndingsCore(instance, "\n");
}
/*jazor:clr-member string.ReplaceLineEndings(string)*/
export function _35041c0250b36108(instance, replacementText) {
  return ReplaceLineEndingsCore(instance, replacementText);
}
/*jazor:clr-member string.Split(char, System.StringSplitOptions)*/
export function _d8080c573d45b4b4(instance, separator, options) {
  return ApplySplitOptions(_96eb0a23afa7fdfb(instance, separator.toString(), Number(instance.length + 1), 0), options);
}
/*jazor:clr-member string.Split(char, int, System.StringSplitOptions)*/
export function _aaa73a4811837ec7(instance, separator, count, options) {
  let splitOptions;
  if (count <= 0)
    return [];
  if (count === 1)
    return ApplySplitOptions([instance], options);
  let trimEntries = false;
  let removeEmptyEntries = false;
  if (typeof options === "number" && (splitOptions = options, true)) {
    trimEntries = (splitOptions & 2) !== 0;
    removeEmptyEntries = (splitOptions & 1) !== 0;
  }
  let token = separator.toString();
  let result = new Array;
  let start = 0;
  while (result.length < count - 1) {
    let index = instance.indexOf(token, start);
    if (index < 0)
      break;
    let part = instance.substring(start, start + (index - start));
    part = trimEntries ? part.trim() : part;
    if (!removeEmptyEntries || part.length !== 0)
      result.push(part);
    start = index + token.length;
  }
  let tail = instance.substring(start);
  tail = trimEntries ? tail.trim() : tail;
  if (!removeEmptyEntries || tail.length !== 0)
    result.push(tail);
  return result;
}
/*jazor:clr-member string.Split(params char[])*/
export function _62c8810ea13dba45(instance, separator) {
  let singleSeparator, separators;
  if (separator == null)
    return SplitByWhitespace(instance);
  if (typeof separator === "string" && (singleSeparator = separator, true))
    return instance.split(RegExp(BuildSplitCharClassPattern_5b3f3896f3870422(singleSeparator)));
  if (Array.isArray(separator) && (separators = separator, true))
    return instance.split(RegExp(BuildSplitCharClassPattern_b468b5e7dbe28377(separators)));
  return SplitByWhitespace(instance);
}
function BuildSplitCharClassPattern_5b3f3896f3870422(separator) {
  if (separator.length === 0)
    return "\\s";
  let pattern = "[";
  for (let i = 0; i < separator.length; i++)
    pattern += EscapeRegexCharClassChar(separator.substring(i, i + 1));
  return pattern + "]";
}
function BuildSplitCharClassPattern_b468b5e7dbe28377(separators) {
  if (separators.length === 0)
    return "\\s";
  let pattern = "[";
  let hasSeparator = false;
  for (let i = 0; i < separators.length; i++) {
    let separator = separators[i];
    if (separator == null || separator.length === 0)
      continue;
    hasSeparator = true;
    for (let j = 0; j < separator.length; j++)
      pattern += EscapeRegexCharClassChar(separator.substring(j, j + 1));
  }
  return hasSeparator ? pattern + "]" : "\\s";
}
function IsWhitespaceCharacter(value) {
  if (value.length !== 1)
    return false;
  let code = value.charCodeAt(0);
  return code >= 9 && code <= 13 || code === 32 || code === 133 || code === 160 || code === 5760 || code >= 8192 && code <= 8202 || code === 8232 || code === 8233 || code === 8239 || code === 8287 || code === 12288;
}
function SplitByWhitespace(instance) {
  let result = new Array;
  let start = 0;
  for (let index = 0; index < instance.length; index++) {
    if (!IsWhitespaceCharacter(_5ad63706a889c294(instance, index)))
      continue;
    result.push(instance.substring(start, start + (index - start)));
    start = index + 1;
  }
  result.push(instance.substring(start));
  return result;
}
function EscapeRegexCharClassChar(ch) {
  return (() => {
    const __swexpr$48d52555b9226738624dc7e3 = ch;
    if (__swexpr$48d52555b9226738624dc7e3 === "\\")
      return "\\\\";
    if (__swexpr$48d52555b9226738624dc7e3 === "]")
      return "\\]";
    if (__swexpr$48d52555b9226738624dc7e3 === "^")
      return "\\^";
    if (__swexpr$48d52555b9226738624dc7e3 === "-")
      return "\\-";
    return ch;
  })();
}
function ApplySplitOptions(parts, options) {
  let splitOptions;
  let trimEntries = false;
  let removeEmptyEntries = false;
  if (typeof options === "number" && (splitOptions = options, true)) {
    trimEntries = (splitOptions & 2) !== 0;
    removeEmptyEntries = (splitOptions & 1) !== 0;
  }
  let result = new Array;
  for (let part of parts) {
    let current = trimEntries ? part.trim() : part;
    if (removeEmptyEntries && current.length === 0)
      continue;
    result.push(current);
  }
  return result;
}
function SplitByCharSetWithLimitAndOptions(instance, separator, count, options) {
  let splitOptions;
  if (count <= 0)
    return [];
  let trimEntries = false;
  let removeEmptyEntries = false;
  if (typeof options === "number" && (splitOptions = options, true)) {
    trimEntries = (splitOptions & 2) !== 0;
    removeEmptyEntries = (splitOptions & 1) !== 0;
  }
  if (count === 1)
    return ApplySplitOptions([instance], options);
  let any = NormalizeCharSet(separator);
  let useWhitespace = any.size === 0;
  let result = new Array;
  let start = 0;
  for (let i = 0; i < instance.length && result.length < count - 1; i++) {
    let current = _5ad63706a889c294(instance, i);
    if (useWhitespace ? !IsWhitespaceCharacter(current) : !any.has(current))
      continue;
    let part = instance.substring(start, start + (i - start));
    part = trimEntries ? part.trim() : part;
    if (!removeEmptyEntries || part.length !== 0)
      result.push(part);
    start = i + 1;
  }
  let tail = instance.substring(start);
  tail = trimEntries ? tail.trim() : tail;
  if (!removeEmptyEntries || tail.length !== 0)
    result.push(tail);
  return result;
}
function SplitByStringsWithLimitAndOptions(instance, separator, count, options) {
  let splitOptions;
  if (count <= 0)
    return [];
  let trimEntries = false;
  let removeEmptyEntries = false;
  if (typeof options === "number" && (splitOptions = options, true)) {
    trimEntries = (splitOptions & 2) !== 0;
    removeEmptyEntries = (splitOptions & 1) !== 0;
  }
  if (count === 1)
    return ApplySplitOptions([instance], options);
  let separators = NormalizeStringSeparators(separator);
  let result = new Array;
  let start = 0;
  while (result.length < count - 1) {
    let bestIndex = -1;
    let bestSeparator = null;
    for (let i = 0; i < separators.length; i++) {
      let item = separators[i];
      if (item == null)
        continue;
      let index = instance.indexOf(item, start);
      if (index < 0)
        continue;
      if (bestIndex < 0 || index < bestIndex) {
        bestIndex = index;
        bestSeparator = item;
      }
    }
    if (bestIndex < 0 || bestSeparator == null)
      break;
    let part = instance.substring(start, start + (bestIndex - start));
    part = trimEntries ? part.trim() : part;
    if (!removeEmptyEntries || part.length !== 0)
      result.push(part);
    start = bestIndex + bestSeparator.length;
  }
  let tail = instance.substring(start);
  tail = trimEntries ? tail.trim() : tail;
  if (!removeEmptyEntries || tail.length !== 0)
    result.push(tail);
  return result;
}
function ReplaceAllIgnoreCase(instance, oldValue, newValue) {
  if (oldValue.length === 0)
    return instance;
  let source = instance.toLowerCase();
  let target = oldValue.toLowerCase();
  let result = "";
  let start = 0;
  while (true) {
    let index = source.indexOf(target, start);
    if (index < 0)
      break;
    result += instance.substring(start, start + (index - start));
    result += newValue;
    start = index + oldValue.length;
  }
  return start === 0 ? instance : result + instance.substring(start);
}
function NormalizeStringSeparators(separator) {
  let result = new Array;
  (() => {
    let single, many;
    const __swpat$855907b63c1542dd7f0128ec = separator;
    if (typeof __swpat$855907b63c1542dd7f0128ec === "string" && (single = __swpat$855907b63c1542dd7f0128ec, true) && single.length !== 0) {
      result.push(single);
      return;
    }
    if (Array.isArray(__swpat$855907b63c1542dd7f0128ec) && (many = __swpat$855907b63c1542dd7f0128ec, true)) {
      for (let i = 0; i < many.length; i++) {
        let item = many[i];
        if (!(item == null) && item.length !== 0)
          result.push(item);
      }
      return;
    }
  })();
  return result;
}
/*jazor:clr-member string.Split(params System.ReadOnlySpan<char>)*/
export function _5417a93b3075813a(instance, separator) {
  return _62c8810ea13dba45(instance, separator);
}
/*jazor:clr-member string.Split(char[], int)*/
export function _d03d120228c0c4ed(instance, separator, count) {
  return SplitByCharSetWithLimitAndOptions(instance, separator, count, 0);
}
/*jazor:clr-member string.Split(char[], System.StringSplitOptions)*/
export function _25c1f15b0ed2cb6e(instance, separator, options) {
  return ApplySplitOptions(_62c8810ea13dba45(instance, separator), options);
}
/*jazor:clr-member string.Split(char[], int, System.StringSplitOptions)*/
export function _c8e5ceed33c6c638(instance, separator, count, options) {
  return SplitByCharSetWithLimitAndOptions(instance, separator, count, options);
}
/*jazor:clr-member string.Split(string, System.StringSplitOptions)*/
export function _189761f781df8770(instance, separator, options) {
  if (separator === null)
    return ApplySplitOptions([instance], options);
  return ApplySplitOptions(_96eb0a23afa7fdfb(instance, separator, Number(instance.length + 1), 0), options);
}
/*jazor:clr-member string.Split(string, int, System.StringSplitOptions)*/
export function _96eb0a23afa7fdfb(instance, separator, count, options) {
  let splitOptions;
  if (count <= 0)
    return [];
  if (count === 1)
    return ApplySplitOptions([instance], options);
  if (!separator)
    return ApplySplitOptions([instance], options);
  let trimEntries = false;
  let removeEmptyEntries = false;
  if (typeof options === "number" && (splitOptions = options, true)) {
    trimEntries = (splitOptions & 2) !== 0;
    removeEmptyEntries = (splitOptions & 1) !== 0;
  }
  let result = new Array;
  let start = 0;
  while (result.length < count - 1) {
    let index = instance.indexOf(separator, start);
    if (index < 0)
      break;
    let part = instance.substring(start, start + (index - start));
    part = trimEntries ? part.trim() : part;
    if (!removeEmptyEntries || part.length !== 0)
      result.push(part);
    start = index + separator.length;
  }
  let tail = instance.substring(start);
  tail = trimEntries ? tail.trim() : tail;
  if (!removeEmptyEntries || tail.length !== 0)
    result.push(tail);
  return result;
}
/*jazor:clr-member string.Split(string[], System.StringSplitOptions)*/
export function _fff99c96206a241e(instance, separator, options) {
  return SplitByStringsWithLimitAndOptions(instance, separator, instance.length + 1, options);
}
/*jazor:clr-member string.Split(string[], int, System.StringSplitOptions)*/
export function _f3c7edcc7cc89a4a(instance, separator, count, options) {
  return SplitByStringsWithLimitAndOptions(instance, separator, count, options);
}
/*jazor:clr-member string.Trim(char)*/
export function _5d7e005b9dcb67de(instance, trimChar) {
  let token = trimChar.toString() ?? "";
  if (token.length === 0)
    return instance;
  let start = 0;
  let end = instance.length - 1;
  while (start <= end && _5ad63706a889c294(instance, start) === token)
    start++;
  while (end >= start && _5ad63706a889c294(instance, end) === token)
    end--;
  return start > end ? "" : instance.substring(start, start + (end - start + 1));
}
/*jazor:clr-member string.Trim(params char[])*/
export function _c6c444b4e71e14f7(instance, trimChars) {
  if (trimChars === null || trimChars.length === 0)
    return instance.trim();
  return TrimCharacterSet(instance, NormalizeCharSet(trimChars), true, true);
}
/*jazor:clr-member string.Trim(params System.ReadOnlySpan<char>)*/
export function _0e8e4169883e5222(instance, trimChars) {
  return TrimReadOnlyCharacterSpan(instance, trimChars, true, true);
}
/*jazor:clr-member string.TrimStart(char)*/
export function _561fe737e62cf332(instance, trimChar) {
  let token = trimChar.toString() ?? "";
  if (token.length === 0)
    return instance;
  let start = 0;
  while (start < instance.length && _5ad63706a889c294(instance, start) === token)
    start++;
  return start === 0 ? instance : instance.substring(start);
}
/*jazor:clr-member string.TrimStart(params char[])*/
export function _98731360726c6976(instance, trimChars) {
  if (trimChars === null || trimChars.length === 0)
    return instance.trimStart();
  return TrimCharacterSet(instance, NormalizeCharSet(trimChars), true, false);
}
/*jazor:clr-member string.TrimStart(params System.ReadOnlySpan<char>)*/
export function _f0473806a2e03bb6(instance, trimChars) {
  return TrimReadOnlyCharacterSpan(instance, trimChars, true, false);
}
/*jazor:clr-member string.TrimEnd(char)*/
export function _eb362a090d734099(instance, trimChar) {
  let token = trimChar.toString() ?? "";
  if (token.length === 0)
    return instance;
  let end = instance.length - 1;
  while (end >= 0 && _5ad63706a889c294(instance, end) === token)
    end--;
  return end === instance.length - 1 ? instance : end < 0 ? "" : instance.substring(0, 0 + (end + 1));
}
/*jazor:clr-member string.TrimEnd(params char[])*/
export function _a62862c1fbaa21c3(instance, trimChars) {
  if (trimChars === null || trimChars.length === 0)
    return instance.trimEnd();
  return TrimCharacterSet(instance, NormalizeCharSet(trimChars), false, true);
}
/*jazor:clr-member string.TrimEnd(params System.ReadOnlySpan<char>)*/
export function _4f8d256566de4b17(instance, trimChars) {
  return TrimReadOnlyCharacterSpan(instance, trimChars, false, true);
}
/*jazor:clr-member string.Contains(string, System.StringComparison)*/
export function _d52d7114d5c1b839(instance, value, comparisonType) {
  return IsOrdinalIgnoreCase(comparisonType) ? instance.toLowerCase().includes(value.toLowerCase()) : instance.includes(value);
}
/*jazor:clr-member string.Contains(char, System.StringComparison)*/
export function _16d4b2b4de019fb2(instance, value, comparisonType) {
  let token = value.toString();
  return IsOrdinalIgnoreCase(comparisonType) ? instance.toLowerCase().includes(token.toLowerCase()) : instance.includes(token);
}
/*jazor:clr-member string.IndexOf(char, System.StringComparison)*/
export function _5331447e2c855a66(instance, value, comparisonType) {
  let token = value.toString();
  return IsOrdinalIgnoreCase(comparisonType) ? instance.toLowerCase().indexOf(token.toLowerCase()) : instance.indexOf(token);
}
/*jazor:clr-member string.IndexOf(char, int, int)*/
export function _d2873e605fbed764(instance, value, startIndex, count) {
  let target = value.toString();
  let end = startIndex + count;
  for (let i = startIndex; i < end && i < instance.length; i++) {
    if (_5ad63706a889c294(instance, i) === target)
      return i;
  }
  return -1;
}
/*jazor:clr-member string.IndexOfAny(char[])*/
export function _69b749a1c6cbae78(instance, anyOf) {
  let any = NormalizeCharSet(anyOf);
  for (let i = 0; i < instance.length; i++) {
    let current = _5ad63706a889c294(instance, i);
    if (any.has(current))
      return i;
  }
  return -1;
}
/*jazor:clr-member string.IndexOfAny(char[], int)*/
export function _63633a5f3b85c5a9(instance, anyOf, startIndex) {
  let any = NormalizeCharSet(anyOf);
  for (let i = startIndex; i < instance.length; i++) {
    let current = _5ad63706a889c294(instance, i);
    if (any.has(current))
      return i;
  }
  return -1;
}
/*jazor:clr-member string.IndexOfAny(char[], int, int)*/
export function _cb863079aae72451(instance, anyOf, startIndex, count) {
  let any = NormalizeCharSet(anyOf);
  let end = startIndex + count;
  for (let i = startIndex; i < end && i < instance.length; i++) {
    let current = _5ad63706a889c294(instance, i);
    if (any.has(current))
      return i;
  }
  return -1;
}
/*jazor:clr-member string.IndexOf(string, int, int)*/
export function _ff549d811898fb56(instance, value, startIndex, count) {
  let end = startIndex + count - value.length;
  for (let i = startIndex; i <= end && i + value.length <= instance.length; i++) {
    if (instance.substring(i, i + value.length) === value)
      return i;
  }
  return -1;
}
/*jazor:clr-member string.IndexOf(string, System.StringComparison)*/
export function _3ae4900da2b07b27(instance, value, comparisonType) {
  return IsOrdinalIgnoreCase(comparisonType) ? instance.toLowerCase().indexOf(value.toLowerCase()) : instance.indexOf(value);
}
/*jazor:clr-member string.IndexOf(string, int, System.StringComparison)*/
export function _2fabe2b831abe71e(instance, value, startIndex, comparisonType) {
  return IsOrdinalIgnoreCase(comparisonType) ? instance.toLowerCase().indexOf(value.toLowerCase(), startIndex) : instance.indexOf(value, startIndex);
}
/*jazor:clr-member string.IndexOf(string, int, int, System.StringComparison)*/
export function _ab22561fc42166db(instance, value, startIndex, count, comparisonType) {
  return IsOrdinalIgnoreCase(comparisonType) ? _ff549d811898fb56(instance.toLowerCase(), value.toLowerCase(), startIndex, count) : _ff549d811898fb56(instance, value, startIndex, count);
}
/*jazor:clr-member string.LastIndexOf(char, int, int)*/
export function _dbdd57f8d259ce66(instance, value, startIndex, count) {
  let target = value.toString();
  let end = startIndex >= instance.length ? Number(instance.length - 1) : startIndex;
  let begin = end - count + 1;
  if (begin < 0)
    begin = 0;
  for (let i = end; i >= begin; i--) {
    if (_5ad63706a889c294(instance, i) === target)
      return i;
  }
  return -1;
}
/*jazor:clr-member string.LastIndexOfAny(char[])*/
export function _c0212f4213a99019(instance, anyOf) {
  let any = NormalizeCharSet(anyOf);
  for (let i = instance.length - 1; i >= 0; i--) {
    let current = _5ad63706a889c294(instance, i);
    if (any.has(current))
      return i;
  }
  return -1;
}
function NormalizeCharSet(anyOf) {
  let set = createDefault();
  (() => {
    let single, many;
    const __swpat$9ae49210dec1797e58550092 = anyOf;
    if (typeof __swpat$9ae49210dec1797e58550092 === "string" && (single = __swpat$9ae49210dec1797e58550092, true)) {
      for (let i = 0; i < single.length; i++)
        _e1d2ba750a2788cb(set, _5ad63706a889c294(single, i));
      return;
    }
    if (Array.isArray(__swpat$9ae49210dec1797e58550092) && (many = __swpat$9ae49210dec1797e58550092, true)) {
      for (let i = 0; i < many.length; i++) {
        let item = many[i];
        if (!item)
          continue;
        for (let j = 0; j < item.length; j++)
          _e1d2ba750a2788cb(set, _5ad63706a889c294(item, j));
      }
      return;
    }
  })();
  return set;
}
function TrimCharacterSet(instance, characters, trimStart, trimEnd) {
  let start = 0;
  let end = instance.length - 1;
  if (trimStart) {
    while (start <= end && characters.has(_5ad63706a889c294(instance, start)))
      start++;
  }
  if (trimEnd) {
    while (end >= start && characters.has(_5ad63706a889c294(instance, end)))
      end--;
  }
  if (start === 0 && end === instance.length - 1)
    return instance;
  return start > end ? "" : instance.substring(start, start + (end - start + 1));
}
/*jazor:clr-member string.LastIndexOfAny(char[], int)*/
export function _c401e64318e768c4(instance, anyOf, startIndex) {
  let any = NormalizeCharSet(anyOf);
  let index = startIndex >= instance.length ? Number(instance.length - 1) : startIndex;
  for (let i = index; i >= 0; i--) {
    let current = _5ad63706a889c294(instance, i);
    if (any.has(current))
      return i;
  }
  return -1;
}
/*jazor:clr-member string.LastIndexOfAny(char[], int, int)*/
export function _3c17fcef5615e7a3(instance, anyOf, startIndex, count) {
  let any = NormalizeCharSet(anyOf);
  let end = startIndex >= instance.length ? Number(instance.length - 1) : startIndex;
  let begin = end - count + 1;
  if (begin < 0)
    begin = 0;
  for (let i = end; i >= begin; i--) {
    let current = _5ad63706a889c294(instance, i);
    if (any.has(current))
      return i;
  }
  return -1;
}
/*jazor:clr-member string.LastIndexOf(string, int, int)*/
export function _c4ee024d06ee238c(instance, value, startIndex, count) {
  let end = startIndex >= instance.length ? Number(instance.length - 1) : startIndex;
  let begin = end - count + 1;
  if (begin < 0)
    begin = 0;
  let maxStart = end - value.length + 1;
  for (let i = maxStart; i >= begin; i--) {
    if (i >= 0 && i + value.length <= instance.length && instance.substring(i, i + value.length) === value)
      return i;
  }
  return -1;
}
/*jazor:clr-member string.LastIndexOf(string, System.StringComparison)*/
export function _78449c135e18c4bc(instance, value, comparisonType) {
  return IsOrdinalIgnoreCase(comparisonType) ? instance.toLowerCase().lastIndexOf(value.toLowerCase()) : instance.lastIndexOf(value);
}
/*jazor:clr-member string.LastIndexOf(string, int, System.StringComparison)*/
export function _359dbce44ce4a4da(instance, value, startIndex, comparisonType) {
  return IsOrdinalIgnoreCase(comparisonType) ? instance.toLowerCase().lastIndexOf(value.toLowerCase(), startIndex) : instance.lastIndexOf(value, startIndex);
}
/*jazor:clr-member string.LastIndexOf(string, int, int, System.StringComparison)*/
export function _c911a06f021bd138(instance, value, startIndex, count, comparisonType) {
  return IsOrdinalIgnoreCase(comparisonType) ? _c4ee024d06ee238c(instance.toLowerCase(), value.toLowerCase(), startIndex, count) : _c4ee024d06ee238c(instance, value, startIndex, count);
}
