import { _5ad63706a889c294 } from "System/StringModule.js";
const EmptyGuid = "00000000-0000-0000-0000-000000000000";
function GetHexValue(c) {
  if (c.charCodeAt(0) >= "0".charCodeAt(0) && c.charCodeAt(0) <= "9".charCodeAt(0))
    return c.charCodeAt(0) - "0".charCodeAt(0);
  if (c.charCodeAt(0) >= "a".charCodeAt(0) && c.charCodeAt(0) <= "f".charCodeAt(0))
    return c.charCodeAt(0) - "a".charCodeAt(0) + 10;
  if (c.charCodeAt(0) >= "A".charCodeAt(0) && c.charCodeAt(0) <= "F".charCodeAt(0))
    return c.charCodeAt(0) - "A".charCodeAt(0) + 10;
  throw new Error("FormatException: Guid contained a non-hexadecimal character.");
}
function IsHexDigit(c) {
  return c.charCodeAt(0) >= "0".charCodeAt(0) && c.charCodeAt(0) <= "9".charCodeAt(0) || c.charCodeAt(0) >= "a".charCodeAt(0) && c.charCodeAt(0) <= "f".charCodeAt(0) || c.charCodeAt(0) >= "A".charCodeAt(0) && c.charCodeAt(0) <= "F".charCodeAt(0);
}
function ParseHexByte(text, start) {
  return GetHexValue(_5ad63706a889c294(text, start)) << 4 | GetHexValue(_5ad63706a889c294(text, start + 1));
}
function TryNormalizeGuid(input, normalized) {
  normalized = "00000000-0000-0000-0000-000000000000";
  let text = input.trim();
  if (text.length === 0)
    return [false, normalized];
  if (_5ad63706a889c294(text, 0).charCodeAt(0) === "{".charCodeAt(0) && _5ad63706a889c294(text, text.length - 1).charCodeAt(0) === "}".charCodeAt(0) || _5ad63706a889c294(text, 0).charCodeAt(0) === "(".charCodeAt(0) && _5ad63706a889c294(text, text.length - 1).charCodeAt(0) === ")".charCodeAt(0))
    text = text.substring(1, 1 + (text.length - 2));
  if (text.length === 32) {
    for (let i = 0; i < text.length; i++) {
      if (!IsHexDigit(_5ad63706a889c294(text, i)))
        return [false, normalized];
    }
    text = text.substring(0, 0 + 8) + "-" + text.substring(8, 8 + 4) + "-" + text.substring(12, 12 + 4) + "-" + text.substring(16, 16 + 4) + "-" + text.substring(20, 20 + 12);
  }
  else if (text.length !== 36) {
    return [false, normalized];
  }
  let lower = "";
  for (let i = 0; i < text.length; i++) {
    if (i === 8 || i === 13 || i === 18 || i === 23) {
      if (_5ad63706a889c294(text, i).charCodeAt(0) !== "-".charCodeAt(0))
        return [false, normalized];
      lower += "-";
      continue;
    }
    if (!IsHexDigit(_5ad63706a889c294(text, i)))
      return [false, normalized];
    let c = _5ad63706a889c294(text, i);
    lower += c.charCodeAt(0) >= "A".charCodeAt(0) && c.charCodeAt(0) <= "F".charCodeAt(0) ? String.fromCharCode(c.charCodeAt(0) + 32) : c;
  }
  normalized = lower;
  return [true, normalized];
}
/*jazor:clr-member System.Guid.Guid()*/
export function _0e58e51018e846d2() {
  return "00000000-0000-0000-0000-000000000000";
}
/*jazor:clr-member System.Guid.Guid(string)*/
export function _24e026ca196fe82b(g) {
  let normalized, __ref$89a586b6e78adaf3d696ff3b;
  if (!(__ref$89a586b6e78adaf3d696ff3b = TryNormalizeGuid(g, undefined), normalized = __ref$89a586b6e78adaf3d696ff3b[1], __ref$89a586b6e78adaf3d696ff3b[0]))
    throw new Error(`FormatException: Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx). Value was '${g ?? ""}'.`);
  return normalized;
}
/*jazor:clr-member static readonly System.Guid.Empty*/
export function _b4f8dd2bd0561d7e() {
  return "00000000-0000-0000-0000-000000000000";
}
/*jazor:clr-member static System.Guid.Parse(string)*/
export function _085f2911d59439cb(input) {
  return _24e026ca196fe82b(input);
}
/*jazor:clr-member static System.Guid.Parse(System.ReadOnlySpan<char>)*/
export function _352ce05083173561(input) {
  return _24e026ca196fe82b(input);
}
/*jazor:clr-member static System.Guid.TryParse(string, out System.Guid)*/
export function _a7f2670ff5b9fe61(input, result) {
  let normalized, __ref$5d73f7b8f3271e5ed2f07684;
  if (input !== null && (__ref$5d73f7b8f3271e5ed2f07684 = TryNormalizeGuid(input, undefined), normalized = __ref$5d73f7b8f3271e5ed2f07684[1], __ref$5d73f7b8f3271e5ed2f07684[0]))
    return [true, normalized];
  return [false, "00000000-0000-0000-0000-000000000000"];
}
/*jazor:clr-member static System.Guid.TryParse(System.ReadOnlySpan<char>, out System.Guid)*/
export function _f886f69cda12fbc3(input, result) {
  return _a7f2670ff5b9fe61(input, result);
}
/*jazor:clr-member override System.Guid.ToString()*/
export function _055f1f857de6de37(instance) {
  return _24e026ca196fe82b(instance);
}
/*jazor:clr-member System.Guid.ToString(string)*/
export function _a79f651902f5c771(instance, format) {
  let normalized = _24e026ca196fe82b(instance);
  if (format === null || format.length === 0)
    return normalized;
  if (format.length !== 1)
    throw new Error("FormatException: Format string can be only 'N', 'D', 'B', or 'P'.");
  let specifier = _5ad63706a889c294(format, 0);
  if (specifier.charCodeAt(0) >= "a".charCodeAt(0) && specifier.charCodeAt(0) <= "z".charCodeAt(0))
    specifier = String.fromCharCode(specifier.charCodeAt(0) - 32);
  if (specifier.charCodeAt(0) === "D".charCodeAt(0))
    return normalized;
  if (specifier.charCodeAt(0) === "N".charCodeAt(0))
    return normalized.replaceAll("-", "");
  if (specifier.charCodeAt(0) === "B".charCodeAt(0))
    return "{" + normalized + "}";
  if (specifier.charCodeAt(0) === "P".charCodeAt(0))
    return "(" + normalized + ")";
  throw new Error("FormatException: Format string can be only 'N', 'D', 'B', or 'P'.");
}
/*jazor:clr-member System.Guid.ToString(string, System.IFormatProvider)*/
export function _dfe41e7b4ff05614(instance, format, provider) {
  return _a79f651902f5c771(instance, format);
}
/*jazor:clr-member override System.Guid.Equals(object)*/
export function _7883fdaac79384d5(instance, value) {
  let normalizedOther, __ref$d5b4dacca3aa9f9359c8a8e0;
  let other = typeof value === "string" ? value : null;
  if (other === null)
    return false;
  if (!(__ref$d5b4dacca3aa9f9359c8a8e0 = TryNormalizeGuid(other, undefined), normalizedOther = __ref$d5b4dacca3aa9f9359c8a8e0[1], __ref$d5b4dacca3aa9f9359c8a8e0[0]))
    return false;
  return _24e026ca196fe82b(instance) === normalizedOther;
}
/*jazor:clr-member System.Guid.Equals(System.Guid)*/
export function _79ee6ab0f29f29dd(instance, value) {
  return _24e026ca196fe82b(instance) === _24e026ca196fe82b(value);
}
/*jazor:clr-member override System.Guid.GetHashCode()*/
export function _6237dbaa794d5c98(instance) {
  let normalized = _24e026ca196fe82b(instance);
  let b0 = ParseHexByte(normalized, 6);
  let b1 = ParseHexByte(normalized, 4);
  let b2 = ParseHexByte(normalized, 2);
  let b3 = ParseHexByte(normalized, 0);
  let b4 = ParseHexByte(normalized, 11);
  let b5 = ParseHexByte(normalized, 9);
  let b6 = ParseHexByte(normalized, 16);
  let b7 = ParseHexByte(normalized, 14);
  let b8 = ParseHexByte(normalized, 19);
  let b9 = ParseHexByte(normalized, 21);
  let b10 = ParseHexByte(normalized, 24);
  let b11 = ParseHexByte(normalized, 26);
  let b12 = ParseHexByte(normalized, 28);
  let b13 = ParseHexByte(normalized, 30);
  let b14 = ParseHexByte(normalized, 32);
  let b15 = ParseHexByte(normalized, 34);
  let i0 = b0 | b1 << 8 | b2 << 16 | b3 << 24;
  let i1 = b4 | b5 << 8 | b6 << 16 | b7 << 24;
  let i2 = b8 | b9 << 8 | b10 << 16 | b11 << 24;
  let i3 = b12 | b13 << 8 | b14 << 16 | b15 << 24;
  let hash = i0 ^ i1 ^ i2 ^ i3;
  return hash | 0;
}
