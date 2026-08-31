import { _5ad63706a889c294 } from "System/StringModule.js";
let LetterPattern = new RegExp("^\\p{L}$", "u");
let NumberPattern = new RegExp("^\\p{N}$", "u");
let PunctuationPattern = new RegExp("^\\p{P}$", "u");
let SeparatorPattern = new RegExp("^\\p{Z}$", "u");
let SymbolPattern = new RegExp("^\\p{S}$", "u");
let UppercaseLetterPattern = new RegExp("^\\p{Lu}$", "u");
let LowercaseLetterPattern = new RegExp("^\\p{Ll}$", "u");
let TitlecaseLetterPattern = new RegExp("^\\p{Lt}$", "u");
let ModifierLetterPattern = new RegExp("^\\p{Lm}$", "u");
let OtherLetterPattern = new RegExp("^\\p{Lo}$", "u");
let NonSpacingMarkPattern = new RegExp("^\\p{Mn}$", "u");
let SpacingCombiningMarkPattern = new RegExp("^\\p{Mc}$", "u");
let EnclosingMarkPattern = new RegExp("^\\p{Me}$", "u");
let DecimalDigitNumberPattern = new RegExp("^\\p{Nd}$", "u");
let LetterNumberPattern = new RegExp("^\\p{Nl}$", "u");
let OtherNumberPattern = new RegExp("^\\p{No}$", "u");
let SpaceSeparatorPattern = new RegExp("^\\p{Zs}$", "u");
let LineSeparatorPattern = new RegExp("^\\p{Zl}$", "u");
let ParagraphSeparatorPattern = new RegExp("^\\p{Zp}$", "u");
let ControlPattern = new RegExp("^\\p{Cc}$", "u");
let FormatPattern = new RegExp("^\\p{Cf}$", "u");
let PrivateUsePattern = new RegExp("^\\p{Co}$", "u");
let ConnectorPunctuationPattern = new RegExp("^\\p{Pc}$", "u");
let DashPunctuationPattern = new RegExp("^\\p{Pd}$", "u");
let OpenPunctuationPattern = new RegExp("^\\p{Ps}$", "u");
let ClosePunctuationPattern = new RegExp("^\\p{Pe}$", "u");
let InitialQuotePunctuationPattern = new RegExp("^\\p{Pi}$", "u");
let FinalQuotePunctuationPattern = new RegExp("^\\p{Pf}$", "u");
let OtherPunctuationPattern = new RegExp("^\\p{Po}$", "u");
let MathSymbolPattern = new RegExp("^\\p{Sm}$", "u");
let CurrencySymbolPattern = new RegExp("^\\p{Sc}$", "u");
let ModifierSymbolPattern = new RegExp("^\\p{Sk}$", "u");
let OtherSymbolPattern = new RegExp("^\\p{So}$", "u");
function CompareCore(left, right) {
  let leftChar = _5ad63706a889c294(left, 0);
  let rightChar = _5ad63706a889c294(right, 0);
  return leftChar.charCodeAt(0) < rightChar.charCodeAt(0) ? -1 : leftChar.charCodeAt(0) > rightChar.charCodeAt(0) ? 1 : 0;
}
function GetCodeUnit(value) {
  return value.charCodeAt(0);
}
function GetCodeUnitFromChar(value) {
  return value.charCodeAt(0);
}
function ToUpperCore(value) {
  if (value === "ı")
    return "I";
  let result = value.toUpperCase();
  return result.length === 1 ? result : value;
}
function ToLowerCore(value) {
  if (value === "İ")
    return "i";
  let result = value.toLowerCase();
  return result.length === 1 ? result : value;
}
function IsControlCode(code) {
  return code < 32 || code >= 127 && code <= 159;
}
function IsWhiteSpaceCode(code) {
  return code >= 9 && code <= 13 || code === 32 || code === 133 || code === 160 || code === 5760 || code >= 8192 && code <= 8202 || code === 8232 || code === 8233 || code === 8239 || code === 8287 || code === 12288;
}
function GetCharacterAt(value, index) {
  if (value === null)
    throw new Error("ArgumentNullException");
  if (Math.floor(index) !== index || index < 0 || index >= value.length)
    throw new Error("ArgumentOutOfRangeException");
  return _5ad63706a889c294(value, index);
}
function GetUnicodeCategoryCore(value) {
  let codeUnit = GetCodeUnit(value);
  if (codeUnit >= 55296 && codeUnit <= 57343)
    return 16;
  if (UppercaseLetterPattern.test(value))
    return 0;
  if (LowercaseLetterPattern.test(value))
    return 1;
  if (TitlecaseLetterPattern.test(value))
    return 2;
  if (ModifierLetterPattern.test(value))
    return 3;
  if (OtherLetterPattern.test(value))
    return 4;
  if (NonSpacingMarkPattern.test(value))
    return 5;
  if (SpacingCombiningMarkPattern.test(value))
    return 6;
  if (EnclosingMarkPattern.test(value))
    return 7;
  if (DecimalDigitNumberPattern.test(value))
    return 8;
  if (LetterNumberPattern.test(value))
    return 9;
  if (OtherNumberPattern.test(value))
    return 10;
  if (SpaceSeparatorPattern.test(value))
    return 11;
  if (LineSeparatorPattern.test(value))
    return 12;
  if (ParagraphSeparatorPattern.test(value))
    return 13;
  if (ControlPattern.test(value))
    return 14;
  if (FormatPattern.test(value))
    return 15;
  if (PrivateUsePattern.test(value))
    return 17;
  if (ConnectorPunctuationPattern.test(value))
    return 18;
  if (DashPunctuationPattern.test(value))
    return 19;
  if (OpenPunctuationPattern.test(value))
    return 20;
  if (ClosePunctuationPattern.test(value))
    return 21;
  if (InitialQuotePunctuationPattern.test(value))
    return 22;
  if (FinalQuotePunctuationPattern.test(value))
    return 23;
  if (OtherPunctuationPattern.test(value))
    return 24;
  if (MathSymbolPattern.test(value))
    return 25;
  if (CurrencySymbolPattern.test(value))
    return 26;
  if (ModifierSymbolPattern.test(value))
    return 27;
  if (OtherSymbolPattern.test(value))
    return 28;
  return 29;
}
/*jazor:clr-member char.CompareTo(object)*/
export function _ddf9c5affdc041df(instance, value) {
  if (value === null)
    return 1;
  if (typeof value !== "string")
    throw new Error("ArgumentException: Object must be of type Char.");
  return CompareCore(instance, value);
}
/*jazor:clr-member static char.Parse(string)*/
export function _d89999df761a6d2e(s) {
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  if (s.length !== 1)
    throw new Error("FormatException: String must be exactly one character long.");
  return s.substring(0, 0 + 1);
}
/*jazor:clr-member static char.TryParse(string, out char)*/
export function _9450f84427428db0(s, result) {
  if (s !== null && s.length === 1)
    return [true, s.substring(0, 0 + 1)];
  return [false, "\0"];
}
/*jazor:clr-member static char.IsDigit(char)*/
export function _91a882221d295c32(c) {
  return DecimalDigitNumberPattern.test(c);
}
/*jazor:clr-member static char.IsLetter(char)*/
export function _38721338a529a8d7(c) {
  return LetterPattern.test(c);
}
/*jazor:clr-member static char.IsWhiteSpace(char)*/
export function _16e351e6f7b127f7(c) {
  return IsWhiteSpaceCode(GetCodeUnit(c));
}
/*jazor:clr-member static char.IsUpper(char)*/
export function _7d70d8021ab255a8(c) {
  return UppercaseLetterPattern.test(c);
}
/*jazor:clr-member static char.IsLower(char)*/
export function _b344d14ce0e33570(c) {
  return LowercaseLetterPattern.test(c);
}
/*jazor:clr-member static char.IsPunctuation(char)*/
export function _ce3de1c060963041(c) {
  return PunctuationPattern.test(c);
}
/*jazor:clr-member static char.IsLetterOrDigit(char)*/
export function _49432dd2165d98f0(c) {
  return LetterPattern.test(c) || DecimalDigitNumberPattern.test(c);
}
/*jazor:clr-member static char.ToUpper(char, System.Globalization.CultureInfo)*/
export function _dd41639bb00c83ab(c, culture) {
  return ToUpperCore(c);
}
/*jazor:clr-member static char.ToUpper(char)*/
export function _2713512e6f5a9312(c) {
  return ToUpperCore(c);
}
/*jazor:clr-member static char.ToUpperInvariant(char)*/
export function _b0c91aa30cd2a5f7(c) {
  return ToUpperCore(c);
}
/*jazor:clr-member static char.ToLower(char, System.Globalization.CultureInfo)*/
export function _b81ddeb8c6240b72(c, culture) {
  return ToLowerCore(c);
}
/*jazor:clr-member static char.ToLower(char)*/
export function _b91d21a936e68017(c) {
  return ToLowerCore(c);
}
/*jazor:clr-member static char.ToLowerInvariant(char)*/
export function _76274ed9d45c0127(c) {
  return ToLowerCore(c);
}
/*jazor:clr-member static char.IsControl(char)*/
export function _c12d0a40e2ed8650(c) {
  return IsControlCode(GetCodeUnit(c));
}
/*jazor:clr-member static char.IsControl(string, int)*/
export function _68e189abbb5497dc(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = GetCodeUnitFromChar(_5ad63706a889c294(s, index));
  return IsControlCode(c);
}
/*jazor:clr-member static char.IsDigit(string, int)*/
export function _52eb020022da112b(s, index) {
  return DecimalDigitNumberPattern.test(GetCharacterAt(s, index));
}
/*jazor:clr-member static char.IsLetter(string, int)*/
export function _e7ee64c732d21cd5(s, index) {
  return LetterPattern.test(GetCharacterAt(s, index));
}
/*jazor:clr-member static char.IsLetterOrDigit(string, int)*/
export function _d752ce4eaadf7612(s, index) {
  let value = GetCharacterAt(s, index);
  return LetterPattern.test(value) || DecimalDigitNumberPattern.test(value);
}
/*jazor:clr-member static char.IsLower(string, int)*/
export function _6ebe08db86ea37a2(s, index) {
  return LowercaseLetterPattern.test(GetCharacterAt(s, index));
}
/*jazor:clr-member static char.IsNumber(char)*/
export function _77e97c648607e65e(c) {
  return NumberPattern.test(c);
}
/*jazor:clr-member static char.IsNumber(string, int)*/
export function _5180e5acb1d4bcb0(s, index) {
  return NumberPattern.test(GetCharacterAt(s, index));
}
/*jazor:clr-member static char.IsPunctuation(string, int)*/
export function _5f7e394ed1d09372(s, index) {
  return PunctuationPattern.test(GetCharacterAt(s, index));
}
/*jazor:clr-member static char.IsSeparator(char)*/
export function _066fc76a18dc824f(c) {
  return SeparatorPattern.test(c);
}
/*jazor:clr-member static char.IsSeparator(string, int)*/
export function _3d391ade47da71a6(s, index) {
  return SeparatorPattern.test(GetCharacterAt(s, index));
}
/*jazor:clr-member static char.IsSurrogate(string, int)*/
export function _bca1b50c85e48723(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = GetCodeUnitFromChar(_5ad63706a889c294(s, index));
  return c >= 55296 && c <= 57343;
}
/*jazor:clr-member static char.IsSymbol(char)*/
export function _0f18b1b6d2524322(c) {
  return SymbolPattern.test(c);
}
/*jazor:clr-member static char.IsSymbol(string, int)*/
export function _16587492d280e91d(s, index) {
  return SymbolPattern.test(GetCharacterAt(s, index));
}
/*jazor:clr-member static char.IsUpper(string, int)*/
export function _1ae24de44f4b499e(s, index) {
  return UppercaseLetterPattern.test(GetCharacterAt(s, index));
}
/*jazor:clr-member static char.IsWhiteSpace(string, int)*/
export function _a21dd6de62be7b75(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  return IsWhiteSpaceCode(GetCodeUnitFromChar(_5ad63706a889c294(s, index)));
}
/*jazor:clr-member static char.GetUnicodeCategory(char)*/
export function _226cc4ffd552fcf9(c) {
  return GetUnicodeCategoryCore(c);
}
/*jazor:clr-member static char.GetUnicodeCategory(string, int)*/
export function _e41ad686bd01aff1(s, index) {
  return GetUnicodeCategoryCore(GetCharacterAt(s, index));
}
/*jazor:clr-member static char.GetNumericValue(char)*/
export function _d86c1e9964250116(c) {
  let code = GetCodeUnit(c);
  if (code >= 48 && code <= 57)
    return code - 48;
  return -1;
}
/*jazor:clr-member static char.GetNumericValue(string, int)*/
export function _938251f1b1fc7bc8(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = GetCodeUnitFromChar(_5ad63706a889c294(s, index));
  if (c >= 48 && c <= 57)
    return c - 48;
  return -1;
}
/*jazor:clr-member static char.IsHighSurrogate(string, int)*/
export function _311485d1745ce294(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = GetCodeUnitFromChar(_5ad63706a889c294(s, index));
  return c >= 55296 && c <= 56319;
}
/*jazor:clr-member static char.IsLowSurrogate(string, int)*/
export function _1d56cdc9a261e948(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = GetCodeUnitFromChar(_5ad63706a889c294(s, index));
  return c >= 56320 && c <= 57343;
}
/*jazor:clr-member static char.IsSurrogatePair(string, int)*/
export function _27c9fca9c829cc5e(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length - 1)
    return false;
  let c1 = GetCodeUnitFromChar(_5ad63706a889c294(s, index));
  let c2 = GetCodeUnitFromChar(_5ad63706a889c294(s, index + 1));
  return c1 >= 55296 && c1 <= 56319 && (c2 >= 56320 && c2 <= 57343);
}
/*jazor:clr-member static char.ConvertToUtf32(string, int)*/
export function _d9f7c3c03ea64580(s, index) {
  if (s === null)
    throw new Error("ArgumentNullException");
  if (index < 0 || index >= s.length)
    throw new Error("ArgumentOutOfRangeException");
  let c = GetCodeUnitFromChar(_5ad63706a889c294(s, index));
  if (c >= 55296 && c <= 56319) {
    if (index + 1 >= s.length)
      throw new Error("ArgumentException: Missing low surrogate");
    let low = GetCodeUnitFromChar(_5ad63706a889c294(s, index + 1));
    if (low < 56320 || low > 57343)
      throw new Error("ArgumentException: Invalid low surrogate");
    return (c - 55296 << 10) + (low - 56320) + 65536;
  }
  return c;
}
