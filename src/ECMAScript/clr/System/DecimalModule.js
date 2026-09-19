import { IsFiniteCore } from "System/DoubleModule.js";
import { DecodeUtf8OrThrowFormat, GetStringHashCode, TryDecodeUtf8 } from "System/RuntimeModule.js";
import { _5ad63706a889c294 } from "System/StringModule.js";
function get_MaxFractionDigits() {
  return 28;
}
function get_MaxDecimalUnscaled() {
  return BigInt("79228162514264337593543950335");
}
function get_Int64MinValue() {
  return BigInt("-9223372036854775808");
}
function get_Int64MaxValue() {
  return BigInt("9223372036854775807");
}
function get_UInt64MaxValue() {
  return BigInt("18446744073709551615");
}
function get_Int32MinValue() {
  return BigInt("-2147483648");
}
function get_Int32MaxValue() {
  return BigInt("2147483647");
}
function get_UInt32MaxValue() {
  return BigInt("4294967295");
}
function get_Int16MinValue() {
  return BigInt("-32768");
}
function get_Int16MaxValue() {
  return BigInt("32767");
}
function get_UInt16MaxValue() {
  return BigInt("65535");
}
function get_SByteMinValue() {
  return BigInt("-128");
}
function get_SByteMaxValue() {
  return BigInt("127");
}
function get_ByteMaxValue() {
  return BigInt("255");
}
function get_AllowLeadingWhiteStyle() {
  return Number(1);
}
function get_AllowTrailingWhiteStyle() {
  return Number(2);
}
function get_AllowLeadingSignStyle() {
  return Number(4);
}
function get_AllowTrailingSignStyle() {
  return Number(8);
}
function get_AllowParenthesesStyle() {
  return Number(16);
}
function get_AllowDecimalPointStyle() {
  return Number(32);
}
function get_AllowThousandsStyle() {
  return Number(64);
}
function get_AllowExponentStyle() {
  return Number(128);
}
function get_AllowCurrencySymbolStyle() {
  return Number(256);
}
function get_AllowHexSpecifierStyle() {
  return Number(512);
}
function get_AllowBinarySpecifierStyle() {
  return Number(1024);
}
function get_NumberStyleNumber() {
  return Number(111);
}
function get_DefaultFixedPrecision() {
  return 2;
}
function get_DefaultNumberPrecision() {
  return 2;
}
function CreateParts(unscaled, scale) {
  return [unscaled, scale];
}
function GetUnscaled(parts) {
  return parts[0];
}
function GetScale(parts) {
  return parts[1];
}
function CreateNumberSymbols(groupSeparator, decimalSeparator, primaryGroupSize, secondaryGroupSize) {
  return [groupSeparator, decimalSeparator, primaryGroupSize, secondaryGroupSize];
}
function GetGroupSeparator(symbols) {
  return symbols[0];
}
function GetDecimalSeparator(symbols) {
  return symbols[1];
}
function GetPrimaryGroupSize(symbols) {
  return symbols[2];
}
function GetSecondaryGroupSize(symbols) {
  return symbols[3];
}
function HasStyle(style, flag) {
  let integerStyle = style;
  let integerFlag = flag;
  return (integerStyle & integerFlag) === integerFlag;
}
function GetNumberStylesValue(style) {
  let numberStyle;
  if (typeof style === "number" && (numberStyle = style, true))
    return numberStyle | 0;
  throw new Error("ArgumentException: Invalid NumberStyles value.");
}
function ValidateDecimalNumberStyles(style) {
  if (Math.floor(style) !== style || style < 0)
    throw new Error("ArgumentException: An undefined NumberStyles value is not supported.");
  if (HasStyle(style, get_AllowHexSpecifierStyle()) || HasStyle(style, get_AllowBinarySpecifierStyle()))
    throw new Error("ArgumentException: The number style AllowHexSpecifier or AllowBinarySpecifier is not supported on floating point data types.");
}
function GetNumberSymbols_8771b2b52b61df8a(provider) {
  let locale, numberFormat;
  if (typeof provider === "string" && (locale = provider, true)) {
    if (locale.length === 0)
      return CreateNumberSymbols(",", ".", 3, 3);
    return GetNumberSymbols_f3969c917b1d7baa(new Intl.NumberFormat(locale));
  }
  if (provider instanceof Intl.NumberFormat && (numberFormat = provider, true))
    return GetNumberSymbols_f3969c917b1d7baa(numberFormat);
  return GetNumberSymbols_f3969c917b1d7baa(new Intl.NumberFormat);
}
function GetNumberSymbols_f3969c917b1d7baa(numberFormat) {
  let groupSeparator = ",";
  let decimalSeparator = ".";
  let integerParts = new Array;
  let parts = numberFormat.formatToParts(123456789.1);
  for (let i = 0; i < parts.length; i++) {
    let part = parts[i];
    if (part.type === "group")
      groupSeparator = part.value;
    else if (part.type === "decimal")
      decimalSeparator = part.value;
    else if (part.type === "integer")
      integerParts.push(part.value.length);
  }
  let primaryGroupSize = 3;
  let secondaryGroupSize = 3;
  if (integerParts.length > 0) {
    primaryGroupSize = integerParts[integerParts.length - 1];
    secondaryGroupSize = integerParts.length > 1 ? integerParts[integerParts.length - 2] : primaryGroupSize;
  }
  return CreateNumberSymbols(groupSeparator, decimalSeparator, primaryGroupSize, secondaryGroupSize);
}
function Pow10(exponent) {
  let result = BigInt(1);
  for (let i = 0; i < exponent; i++)
    result *= BigInt(10);
  return result;
}
function MaxNumber(left, right) {
  return left >= right ? left : right;
}
function RepeatZero(count) {
  let parts = new Array;
  for (let i = 0; i < count; i++)
    parts.push("0");
  return parts.join("");
}
function StripLeadingZeros(digits) {
  while (digits.length > 1 && _5ad63706a889c294(digits, 0).charCodeAt(0) === "0".charCodeAt(0))
    digits = digits.substring(1);
  return digits;
}
function PreserveParts(unscaled, scale) {
  let absolute = unscaled < 0n ? -unscaled : unscaled;
  while (scale > 0 && (scale > get_MaxFractionDigits() || absolute > get_MaxDecimalUnscaled()) && unscaled % BigInt(10) === 0n) {
    unscaled /= BigInt(10);
    scale--;
    absolute = unscaled < 0n ? -unscaled : unscaled;
  }
  if (scale < 0 || scale > get_MaxFractionDigits())
    throw new Error("OverflowException: Value was either too large or too small for a Decimal.");
  if (absolute > get_MaxDecimalUnscaled())
    throw new Error("OverflowException: Value was either too large or too small for a Decimal.");
  return CreateParts(unscaled, scale);
}
function NormalizeParts(unscaled, scale) {
  let parts = PreserveParts(unscaled, scale);
  unscaled = GetUnscaled(parts);
  scale = GetScale(parts);
  while (scale > 0 && unscaled % BigInt(10) === 0n) {
    unscaled /= BigInt(10);
    scale--;
  }
  return CreateParts(unscaled, scale);
}
function ParseDecimal(value, allowExponent = true) {
  let s = value.trim();
  if (s.length === 0)
    throw new Error("FormatException: String was not recognized as a valid Decimal.");
  let negative = false;
  if (_5ad63706a889c294(s, 0).charCodeAt(0) === "+".charCodeAt(0) || _5ad63706a889c294(s, 0).charCodeAt(0) === "-".charCodeAt(0)) {
    negative = _5ad63706a889c294(s, 0).charCodeAt(0) === "-".charCodeAt(0);
    s = s.substring(1);
    if (s.length === 0)
      throw new Error(`FormatException: String '${value ?? ""}' was not recognized as a valid Decimal.`);
  }
  let exponent = 0;
  let exponentIndex = s.indexOf("e");
  if (exponentIndex < 0)
    exponentIndex = s.indexOf("E");
  if (exponentIndex >= 0) {
    if (!allowExponent)
      throw new Error(`FormatException: String '${value ?? ""}' was not recognized as a valid Decimal.`);
    if (exponentIndex === 0 || exponentIndex === s.length - 1)
      throw new Error(`FormatException: String '${value ?? ""}' was not recognized as a valid Decimal.`);
    let exponentText = s.substring(exponentIndex + 1);
    let exponentValue = Number(exponentText);
    if (isNaN(exponentValue) || !IsFiniteCore(exponentValue) || Math.floor(exponentValue) !== exponentValue || exponentValue < -100 || exponentValue > 100)
      throw new Error(`FormatException: String '${value ?? ""}' was not recognized as a valid Decimal.`);
    exponent = exponentValue;
    s = s.substring(0, 0 + exponentIndex);
    if (s.length === 0)
      throw new Error(`FormatException: String '${value ?? ""}' was not recognized as a valid Decimal.`);
  }
  let dotIndex = s.indexOf(".");
  if (dotIndex >= 0 && s.indexOf(".", dotIndex + 1) >= 0)
    throw new Error(`FormatException: String '${value ?? ""}' was not recognized as a valid Decimal.`);
  let integerDigits = dotIndex >= 0 ? s.substring(0, 0 + dotIndex) : s;
  let fractionDigits = dotIndex >= 0 ? s.substring(dotIndex + 1) : "";
  if (integerDigits.length === 0 && fractionDigits.length === 0)
    throw new Error(`FormatException: String '${value ?? ""}' was not recognized as a valid Decimal.`);
  if (integerDigits.length === 0)
    integerDigits = "0";
  let digits = integerDigits + fractionDigits;
  if (digits.length === 0)
    throw new Error(`FormatException: String '${value ?? ""}' was not recognized as a valid Decimal.`);
  for (let i = 0; i < digits.length; i++) {
    let c = _5ad63706a889c294(digits, i);
    if (c.charCodeAt(0) < "0".charCodeAt(0) || c.charCodeAt(0) > "9".charCodeAt(0))
      throw new Error(`FormatException: String '${value ?? ""}' was not recognized as a valid Decimal.`);
  }
  digits = StripLeadingZeros(digits);
  let scale = fractionDigits.length - exponent;
  if (scale < 0) {
    digits += RepeatZero(-scale);
    scale = 0;
  }
  let unscaled = BigInt(digits);
  if (negative && unscaled !== 0n)
    unscaled = -unscaled;
  return PreserveParts(unscaled, scale);
}
function NormalizeDecimal(value) {
  let parts = ParseDecimal(value);
  return FormatNormalizedDecimal(GetUnscaled(parts), GetScale(parts));
}
function RemoveAllOccurrences(value, token) {
  return token.length === 0 ? value : value.replaceAll(token, "");
}
function ReplaceDecimalSeparator(value, decimalSeparator) {
  if (decimalSeparator.length === 0 || decimalSeparator === ".")
    return value;
  return value.replaceAll(decimalSeparator, ".");
}
function NormalizeExternalDecimalText(value, style, provider) {
  ValidateDecimalNumberStyles(style);
  let text = value;
  let allowLeadingWhite = HasStyle(style, get_AllowLeadingWhiteStyle());
  let allowTrailingWhite = HasStyle(style, get_AllowTrailingWhiteStyle());
  if (allowLeadingWhite && allowTrailingWhite)
    text = text.trim();
  else if (allowLeadingWhite)
    text = text.trimStart();
  else if (allowTrailingWhite)
    text = text.trimEnd();
  if (text.length === 0)
    throw new Error(`FormatException: String '${value ?? ""}' was not recognized as a valid Decimal.`);
  let negative = false;
  if (HasStyle(style, get_AllowParenthesesStyle()) && text.length >= 2 && _5ad63706a889c294(text, 0).charCodeAt(0) === "(".charCodeAt(0) && _5ad63706a889c294(text, text.length - 1).charCodeAt(0) === ")".charCodeAt(0)) {
    negative = true;
    text = text.substring(1, 1 + (text.length - 2));
    if (allowLeadingWhite && allowTrailingWhite)
      text = text.trim();
    else if (allowLeadingWhite)
      text = text.trimStart();
    else if (allowTrailingWhite)
      text = text.trimEnd();
  }
  if (text.length === 0)
    throw new Error(`FormatException: String '${value ?? ""}' was not recognized as a valid Decimal.`);
  if (_5ad63706a889c294(text, 0).charCodeAt(0) === "+".charCodeAt(0) || _5ad63706a889c294(text, 0).charCodeAt(0) === "-".charCodeAt(0)) {
    if (!HasStyle(style, get_AllowLeadingSignStyle()))
      throw new Error(`FormatException: String '${value ?? ""}' was not recognized as a valid Decimal.`);
    negative = _5ad63706a889c294(text, 0).charCodeAt(0) === "-".charCodeAt(0) ? !negative : negative;
    text = text.substring(1);
  }
  if (text.length === 0)
    throw new Error(`FormatException: String '${value ?? ""}' was not recognized as a valid Decimal.`);
  if (_5ad63706a889c294(text, text.length - 1).charCodeAt(0) === "+".charCodeAt(0) || _5ad63706a889c294(text, text.length - 1).charCodeAt(0) === "-".charCodeAt(0)) {
    if (!HasStyle(style, get_AllowTrailingSignStyle()))
      throw new Error(`FormatException: String '${value ?? ""}' was not recognized as a valid Decimal.`);
    negative = _5ad63706a889c294(text, text.length - 1).charCodeAt(0) === "-".charCodeAt(0) ? !negative : negative;
    text = text.substring(0, 0 + (text.length - 1));
  }
  if (text.length === 0)
    throw new Error(`FormatException: String '${value ?? ""}' was not recognized as a valid Decimal.`);
  let symbols = GetNumberSymbols_8771b2b52b61df8a(provider);
  let groupSeparator = GetGroupSeparator(symbols);
  let decimalSeparator = GetDecimalSeparator(symbols);
  if (groupSeparator.length !== 0) {
    if (HasStyle(style, get_AllowThousandsStyle()))
      text = RemoveAllOccurrences(text, groupSeparator);
    else if (text.indexOf(groupSeparator) >= 0)
      throw new Error(`FormatException: String '${value ?? ""}' was not recognized as a valid Decimal.`);
  }
  if (decimalSeparator !== ".") {
    if (text.indexOf(decimalSeparator) >= 0) {
      if (!HasStyle(style, get_AllowDecimalPointStyle()))
        throw new Error(`FormatException: String '${value ?? ""}' was not recognized as a valid Decimal.`);
      text = ReplaceDecimalSeparator(text, decimalSeparator);
    }
  }
  if (!HasStyle(style, get_AllowDecimalPointStyle()) && text.indexOf(".") >= 0)
    throw new Error(`FormatException: String '${value ?? ""}' was not recognized as a valid Decimal.`);
  if (negative)
    text = "-" + text;
  return text;
}
function ParseDecimalExternal(value, style, provider) {
  if (value === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  let normalized = NormalizeExternalDecimalText(value, style, provider);
  let parts = ParseDecimal(normalized, HasStyle(style, get_AllowExponentStyle()));
  return FormatDecimal(GetUnscaled(parts), GetScale(parts));
}
function FormatDecimal(unscaled, scale) {
  let preserved = PreserveParts(unscaled, scale);
  unscaled = GetUnscaled(preserved);
  scale = GetScale(preserved);
  let negative = unscaled < 0n;
  let absolute = negative ? -unscaled : unscaled;
  let digits = absolute.toString();
  if (scale === 0)
    return negative ? "-" + digits : digits;
  if (digits.length <= scale)
    digits = RepeatZero(scale - digits.length + 1) + digits;
  let split = digits.length - scale;
  let text = digits.substring(0, 0 + split) + "." + digits.substring(split);
  return negative ? "-" + text : text;
}
function FormatNormalizedDecimal(unscaled, scale) {
  let normalized = NormalizeParts(unscaled, scale);
  return FormatDecimal(GetUnscaled(normalized), GetScale(normalized));
}
function PreserveDecimal(value) {
  let parts = ParseDecimal(value);
  return FormatDecimal(GetUnscaled(parts), GetScale(parts));
}
function FormatDecimalToScale(value, scale) {
  let parts = ParseDecimal(value);
  let unscaled = GetUnscaled(parts);
  let sourceScale = GetScale(parts);
  if (sourceScale < scale)
    unscaled *= Pow10(scale - sourceScale);
  else if (sourceScale > scale)
    unscaled /= Pow10(sourceScale - scale);
  let negative = unscaled < 0n;
  let absolute = negative ? -unscaled : unscaled;
  let digits = absolute.toString();
  if (scale === 0)
    return negative ? "-" + digits : digits;
  if (digits.length <= scale)
    digits = RepeatZero(scale - digits.length + 1) + digits;
  let split = digits.length - scale;
  let text = digits.substring(0, 0 + split) + "." + digits.substring(split);
  return negative ? "-" + text : text;
}
function InsertGroupSeparators(integerDigits, separator, primaryGroupSize, secondaryGroupSize) {
  if (separator.length === 0 || integerDigits.length <= primaryGroupSize)
    return integerDigits;
  let groups = new Array;
  let end = integerDigits.length;
  let size = primaryGroupSize;
  while (end > size) {
    groups.push(((__jz_arg0, __jz_arg1, __jz_arg2) => __jz_arg0.substring(__jz_arg1, __jz_arg1 + __jz_arg2))(integerDigits, end - size, size));
    end -= size;
    size = secondaryGroupSize;
  }
  let result = integerDigits.substring(0, 0 + end);
  for (let i = groups.length - 1; i >= 0; i--)
    result += separator + groups[i];
  return result;
}
function ApplyNumberSeparators(value, provider) {
  let symbols = GetNumberSymbols_8771b2b52b61df8a(provider);
  let groupSeparator = GetGroupSeparator(symbols);
  let decimalSeparator = GetDecimalSeparator(symbols);
  let primaryGroupSize = GetPrimaryGroupSize(symbols);
  let secondaryGroupSize = GetSecondaryGroupSize(symbols);
  let sign = "";
  let digits = value;
  if (_5ad63706a889c294(digits, 0).charCodeAt(0) === "-".charCodeAt(0)) {
    sign = "-";
    digits = digits.substring(1);
  }
  let dotIndex = digits.indexOf(".");
  let integerDigits = dotIndex >= 0 ? digits.substring(0, 0 + dotIndex) : digits;
  let fractionDigits = dotIndex >= 0 ? digits.substring(dotIndex + 1) : "";
  let groupedInteger = InsertGroupSeparators(integerDigits, groupSeparator, primaryGroupSize, secondaryGroupSize);
  if (fractionDigits.length === 0)
    return sign + groupedInteger;
  return sign + groupedInteger + decimalSeparator + fractionDigits;
}
function ParsePrecision(format, defaultValue) {
  if (format.length === 1)
    return defaultValue;
  let precisionText = format.substring(1);
  for (let i = 0; i < precisionText.length; i++) {
    let c = _5ad63706a889c294(precisionText, i);
    if (c.charCodeAt(0) < "0".charCodeAt(0) || c.charCodeAt(0) > "9".charCodeAt(0))
      throw new Error("FormatException: Format specifier was invalid.");
  }
  return Number(precisionText);
}
function IsSimpleCustomDecimalFormat(format) {
  for (let i = 0; i < format.length; i++) {
    let c = _5ad63706a889c294(format, i);
    if (c.charCodeAt(0) !== "0".charCodeAt(0) && c.charCodeAt(0) !== "#".charCodeAt(0) && c.charCodeAt(0) !== ".".charCodeAt(0) && c.charCodeAt(0) !== ",".charCodeAt(0))
      return false;
  }
  return true;
}
function FormatDecimalWithFormat(value, format, provider) {
  if (format === null || format.length === 0)
    return PreserveDecimal(value);
  let specifier = _5ad63706a889c294(format, 0);
  if ((specifier.charCodeAt(0) === "G".charCodeAt(0) || specifier.charCodeAt(0) === "g".charCodeAt(0)) && format.length === 1)
    return PreserveDecimal(value);
  if (specifier.charCodeAt(0) === "F".charCodeAt(0) || specifier.charCodeAt(0) === "f".charCodeAt(0)) {
    let precision = ParsePrecision(format, get_DefaultFixedPrecision());
    return FormatDecimalToScale(RoundDecimal(value, precision), precision);
  }
  if (specifier.charCodeAt(0) === "N".charCodeAt(0) || specifier.charCodeAt(0) === "n".charCodeAt(0)) {
    let precision = ParsePrecision(format, get_DefaultNumberPrecision());
    return ApplyNumberSeparators(FormatDecimalToScale(RoundDecimal(value, precision), precision), provider);
  }
  if (IsSimpleCustomDecimalFormat(format)) {
    let dotIndex = format.indexOf(".");
    let scale = dotIndex < 0 ? 0 : format.length - dotIndex - 1;
    let formatted = FormatDecimalToScale(RoundDecimal(value, scale), scale);
    return format.indexOf(",") >= 0 ? ApplyNumberSeparators(formatted, provider) : formatted;
  }
  throw new Error("FormatException: Format specifier was invalid.");
}
function CreateDecimalFromNumber(value) {
  if (!IsFiniteCore(value))
    throw new Error("OverflowException: Value was either too large or too small for a Decimal.");
  return NormalizeDecimal(value.toString());
}
function AlignUnscaled(value, targetScale) {
  let scale = GetScale(value);
  if (targetScale <= scale)
    return GetUnscaled(value);
  return GetUnscaled(value) * Pow10(targetScale - scale);
}
function TruncateToIntegralValue(value) {
  let parts = ParseDecimal(value);
  let scale = GetScale(parts);
  if (scale === 0)
    return GetUnscaled(parts);
  return GetUnscaled(parts) / Pow10(scale);
}
function ToCheckedNumber(value, min, max, typeName) {
  let integral = TruncateToIntegralValue(value);
  if (integral < min || integral > max)
    throw new Error(`OverflowException: Value was either too large or too small for a ${typeName ?? ""}.`);
  return Number(integral);
}
function ToCheckedBigInt(value, min, max, typeName) {
  let integral = TruncateToIntegralValue(value);
  if (integral < min || integral > max)
    throw new Error(`OverflowException: Value was either too large or too small for a ${typeName ?? ""}.`);
  return integral;
}
function FromOACurrencyCore(currency) {
  let scale = 4;
  while (currency !== 0n && scale > 0 && currency % BigInt(10) === 0n) {
    currency /= BigInt(10);
    scale--;
  }
  return FormatDecimal(currency, scale);
}
function ToOACurrencyCore(value) {
  let parts = ParseDecimal(value);
  let unscaled = GetUnscaled(parts);
  let scale = GetScale(parts);
  let currency;
  if (scale <= 4)
    currency = unscaled * Pow10(4 - scale);
  else
    currency = DivideAndRound(unscaled, Pow10(scale - 4));
  if (currency < get_Int64MinValue() || currency > get_Int64MaxValue())
    throw new Error("OverflowException: Value was either too large or too small for a Currency.");
  return currency;
}
function GetMidpointRoundingValue(mode) {
  let numberMode;
  if (typeof mode === "number" && (numberMode = mode, true))
    return numberMode;
  throw new Error("ArgumentException: Invalid MidpointRounding value.");
}
function RoundDecimal(value, decimals, mode = null) {
  if (Math.floor(decimals) !== decimals || decimals < 0 || decimals > get_MaxFractionDigits())
    throw new Error("ArgumentOutOfRangeException: Decimal digits must be between 0 and 28.");
  let modeValue = mode === null ? Number(0) : GetMidpointRoundingValue(mode);
  if (modeValue < 0 || modeValue > 4 || Math.floor(modeValue) !== modeValue)
    throw new Error("ArgumentException: Invalid MidpointRounding value.");
  let parts = ParseDecimal(value);
  let scale = GetScale(parts);
  let unscaled = GetUnscaled(parts);
  if (scale <= decimals)
    return FormatDecimal(unscaled, scale);
  let trimScale = scale - decimals;
  let divisor = Pow10(trimScale);
  let quotient = unscaled / divisor;
  let remainder = unscaled % divisor;
  if (remainder === 0n)
    return FormatDecimal(quotient, decimals);
  let negative = unscaled < 0n;
  if (modeValue === 2)
    return FormatDecimal(quotient, decimals);
  if (modeValue === 3)
    return FormatDecimal(negative ? quotient - BigInt(1) : quotient, decimals);
  if (modeValue === 4)
    return FormatDecimal(negative ? quotient : quotient + BigInt(1), decimals);
  let absoluteRemainder = negative ? -remainder : remainder;
  let comparison = absoluteRemainder * BigInt(2) - divisor;
  if (comparison < 0n)
    return FormatDecimal(quotient, decimals);
  let step = negative ? -BigInt(1) : BigInt(1);
  if (comparison > 0n || modeValue === 1)
    return FormatDecimal(quotient + step, decimals);
  return quotient % BigInt(2) === 0n ? FormatDecimal(quotient, decimals) : FormatDecimal(quotient + step, decimals);
}
function CompareDecimal(left, right) {
  let a = ParseDecimal(left);
  let b = ParseDecimal(right);
  let targetScale = MaxNumber(GetScale(a), GetScale(b));
  let leftValue = AlignUnscaled(a, targetScale);
  let rightValue = AlignUnscaled(b, targetScale);
  if (leftValue < rightValue)
    return -1;
  if (leftValue > rightValue)
    return 1;
  return 0;
}
function AddDecimal(left, right) {
  let a = ParseDecimal(left);
  let b = ParseDecimal(right);
  let targetScale = MaxNumber(GetScale(a), GetScale(b));
  return FormatDecimal(AlignUnscaled(a, targetScale) + AlignUnscaled(b, targetScale), targetScale);
}
function SubtractDecimal(left, right) {
  return AddDecimal(left, NegateDecimal(right));
}
function NegateDecimal(value) {
  let parts = ParseDecimal(value);
  return FormatDecimal(-GetUnscaled(parts), GetScale(parts));
}
function MultiplyDecimal(left, right) {
  let a = ParseDecimal(left);
  let b = ParseDecimal(right);
  return FormatDecimal(GetUnscaled(a) * GetUnscaled(b), GetScale(a) + GetScale(b));
}
function DivideAndRound(numerator, denominator) {
  let quotient = numerator / denominator;
  let remainder = numerator % denominator;
  if (remainder === 0n)
    return quotient;
  let absoluteRemainder = remainder < 0n ? -remainder : remainder;
  let absoluteDenominator = denominator < 0n ? -denominator : denominator;
  let comparison = absoluteRemainder * BigInt(2) - absoluteDenominator;
  if (comparison < 0n)
    return quotient;
  let step = numerator < 0n ? -BigInt(1) : BigInt(1);
  if (comparison > 0n)
    return quotient + step;
  return quotient % BigInt(2) === 0n ? quotient : quotient + step;
}
function DivideDecimal(left, right) {
  let a = ParseDecimal(left);
  let b = ParseDecimal(right);
  if (GetUnscaled(b) === 0n)
    throw new Error("DivideByZeroException: Attempted to divide by zero.");
  let scaleDelta = get_MaxFractionDigits() + GetScale(b) - GetScale(a);
  let numerator = GetUnscaled(a);
  let denominator = GetUnscaled(b);
  if (scaleDelta >= 0)
    numerator *= Pow10(scaleDelta);
  else
    denominator *= Pow10(-scaleDelta);
  return FormatNormalizedDecimal(DivideAndRound(numerator, denominator), get_MaxFractionDigits());
}
function RemainderDecimal(left, right) {
  let a = ParseDecimal(left);
  let b = ParseDecimal(right);
  if (GetUnscaled(b) === 0n)
    throw new Error("DivideByZeroException: Attempted to divide by zero.");
  let targetScale = MaxNumber(GetScale(a), GetScale(b));
  return FormatDecimal(AlignUnscaled(a, targetScale) % AlignUnscaled(b, targetScale), targetScale);
}
function FloorDecimal(value) {
  let parts = ParseDecimal(value);
  let scale = GetScale(parts);
  let unscaled = GetUnscaled(parts);
  if (scale === 0)
    return FormatDecimal(unscaled, 0);
  let divisor = Pow10(scale);
  let quotient = unscaled / divisor;
  let remainder = unscaled % divisor;
  if (remainder !== 0n && unscaled < 0n)
    quotient -= BigInt(1);
  return FormatDecimal(quotient, 0);
}
function CeilingDecimal(value) {
  let parts = ParseDecimal(value);
  let scale = GetScale(parts);
  let unscaled = GetUnscaled(parts);
  if (scale === 0)
    return FormatDecimal(unscaled, 0);
  let divisor = Pow10(scale);
  let quotient = unscaled / divisor;
  let remainder = unscaled % divisor;
  if (remainder !== 0n && unscaled > 0n)
    quotient += BigInt(1);
  return FormatDecimal(quotient, 0);
}
function TruncateDecimal(value) {
  let parts = ParseDecimal(value);
  let scale = GetScale(parts);
  let unscaled = GetUnscaled(parts);
  if (scale === 0)
    return FormatDecimal(unscaled, 0);
  return FormatDecimal(unscaled / Pow10(scale), 0);
}
function AbsDecimal(value) {
  let parts = ParseDecimal(value);
  let unscaled = GetUnscaled(parts);
  return FormatDecimal(unscaled < 0n ? -unscaled : unscaled, GetScale(parts));
}
function SignDecimal(value) {
  let parts = ParseDecimal(value);
  let unscaled = GetUnscaled(parts);
  if (unscaled < 0n)
    return -1;
  if (unscaled > 0n)
    return 1;
  return 0;
}
function IsIntegerDecimal(value) {
  let parts = ParseDecimal(value);
  let scale = GetScale(parts);
  return scale === 0 || GetUnscaled(parts) % Pow10(scale) === 0n;
}
/*jazor:clr-member static readonly decimal.Zero*/
export function _5faf9ddf65d02495() {
  return "0";
}
/*jazor:clr-member static readonly decimal.One*/
export function _3db06a98834e6ef8() {
  return "1";
}
/*jazor:clr-member static readonly decimal.MinusOne*/
export function _9311127a9ca2b91d() {
  return "-1";
}
/*jazor:clr-member static readonly decimal.MaxValue*/
export function _6a4e5f697d4fc607() {
  return "79228162514264337593543950335";
}
/*jazor:clr-member static readonly decimal.MinValue*/
export function _cc6392a7d6df1e14() {
  return "-79228162514264337593543950335";
}
/*jazor:clr-member decimal.Decimal(float)*/
export function _2f7f0d9035a4bbf6(value) {
  return CreateDecimalFromNumber(value);
}
/*jazor:clr-member decimal.Decimal(double)*/
export function _cb7c7a937d3b8460(value) {
  return CreateDecimalFromNumber(value);
}
/*jazor:clr-member static decimal.FromOACurrency(long)*/
export function _6cd0f8dfbedd7209(cy) {
  return FromOACurrencyCore(cy);
}
/*jazor:clr-member static decimal.ToOACurrency(decimal)*/
export function _5d257b5cc33cdaeb(value) {
  return ToOACurrencyCore(value);
}
/*jazor:clr-member decimal.Scale.get*/
export function _db7e7c8def75fee8(instance) {
  return GetScale(ParseDecimal(instance));
}
/*jazor:clr-member static decimal.Add(decimal, decimal)*/
export function _f73258f14e05c790(d1, d2) {
  return AddDecimal(d1, d2);
}
/*jazor:clr-member static decimal.Ceiling(decimal)*/
export function _84028a6e79626057(d) {
  return CeilingDecimal(d);
}
/*jazor:clr-member static decimal.Compare(decimal, decimal)*/
export function _c11e0aef6b5ccf1e(d1, d2) {
  return CompareDecimal(d1, d2);
}
/*jazor:clr-member decimal.CompareTo(object)*/
export function _ff0e77ab6566e092(instance, value) {
  let other;
  if (value === null)
    return 1;
  if (!(typeof value === "string" && (other = value, true)))
    throw new Error("ArgumentException: Object must be of type Decimal.");
  return _ca8a78810233056c(instance, other);
}
/*jazor:clr-member decimal.CompareTo(decimal)*/
export function _ca8a78810233056c(instance, value) {
  return CompareDecimal(instance, value);
}
/*jazor:clr-member static decimal.Divide(decimal, decimal)*/
export function _f5c1c0a2a040b000(d1, d2) {
  return DivideDecimal(d1, d2);
}
/*jazor:clr-member override decimal.Equals(object)*/
export function _8abe47785e51f122(instance, value) {
  let other;
  return typeof value === "string" && (other = value, true) && CompareDecimal(instance, other) === 0;
}
/*jazor:clr-member decimal.Equals(decimal)*/
export function _3dfd87d9d2f35e11(instance, value) {
  return CompareDecimal(instance, value) === 0;
}
/*jazor:clr-member override decimal.GetHashCode()*/
export function _f58659c33299d2b1(instance) {
  return GetStringHashCode(NormalizeDecimal(instance), 0);
}
/*jazor:clr-member static decimal.Equals(decimal, decimal)*/
export function _b25c4446c28ed255(d1, d2) {
  return CompareDecimal(d1, d2) === 0;
}
/*jazor:clr-member static decimal.Floor(decimal)*/
export function _518facaaeeb29ead(d) {
  return FloorDecimal(d);
}
/*jazor:clr-member override decimal.ToString()*/
export function _65a0e4fe8ccdd829(instance) {
  return PreserveDecimal(instance);
}
/*jazor:clr-member decimal.ToString(string)*/
export function _af32d07083f1da07(instance, format) {
  return FormatDecimalWithFormat(instance, format, null);
}
/*jazor:clr-member decimal.ToString(System.IFormatProvider)*/
export function _6234ba988b3e006d(instance, provider) {
  return FormatDecimalWithFormat(instance, null, provider);
}
/*jazor:clr-member decimal.ToString(string, System.IFormatProvider)*/
export function _b1e6a06111674f0c(instance, format, provider) {
  return FormatDecimalWithFormat(instance, format, provider);
}
/*jazor:clr-member static decimal.Parse(string)*/
export function _91a2436283a24315(s) {
  return ParseDecimalExternal(s, get_NumberStyleNumber(), null);
}
/*jazor:clr-member static decimal.Parse(string, System.Globalization.NumberStyles)*/
export function _79a0e8ede29256cc(s, style) {
  return ParseDecimalExternal(s, GetNumberStylesValue(style), null);
}
/*jazor:clr-member static decimal.Parse(string, System.IFormatProvider)*/
export function _01be2a34fe2cda4e(s, provider) {
  return ParseDecimalExternal(s, get_NumberStyleNumber(), provider);
}
/*jazor:clr-member static decimal.Parse(string, System.Globalization.NumberStyles, System.IFormatProvider)*/
export function _f525a420b2d600ec(s, style, provider) {
  return ParseDecimalExternal(s, GetNumberStylesValue(style), provider);
}
/*jazor:clr-member static decimal.Parse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider)*/
export function _8e0c949ee2411c7f(s, style, provider) {
  return ParseDecimalExternal(s, GetNumberStylesValue(style), provider);
}
/*jazor:clr-member static decimal.TryParse(string, out decimal)*/
export function _e96278809bb50e35(s, result) {
  if (s === null || s.length === 0)
    return [false, "0"];
  try {
    return [true, ParseDecimalExternal(s, get_NumberStyleNumber(), null)];
  } catch {
    return [false, "0"];
  }
}
/*jazor:clr-member static decimal.TryParse(System.ReadOnlySpan<char>, out decimal)*/
export function _5f6432cf52162431(s, result) {
  return _e96278809bb50e35(s, result);
}
/*jazor:clr-member static decimal.TryParse(System.ReadOnlySpan<byte>, out decimal)*/
export function _0111d7c27998205b(utf8Text, result) {
  return _e96278809bb50e35(TryDecodeUtf8(utf8Text), result);
}
/*jazor:clr-member static decimal.TryParse(string, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)*/
export function _b4ecd2424c9a371e(s, style, provider, result) {
  let styleValue = GetNumberStylesValue(style);
  ValidateDecimalNumberStyles(styleValue);
  if (s === null || s.length === 0)
    return [false, "0"];
  try {
    return [true, ParseDecimalExternal(s, styleValue, provider)];
  } catch {
    return [false, "0"];
  }
}
/*jazor:clr-member static decimal.TryParse(System.ReadOnlySpan<char>, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)*/
export function _ed6b24306e2ef5cd(s, style, provider, result) {
  return _b4ecd2424c9a371e(s, style, provider, result);
}
/*jazor:clr-member static decimal.Remainder(decimal, decimal)*/
export function _700359e0de148ee3(d1, d2) {
  return RemainderDecimal(d1, d2);
}
/*jazor:clr-member static decimal.Multiply(decimal, decimal)*/
export function _d5be5da3d4effe96(d1, d2) {
  return MultiplyDecimal(d1, d2);
}
/*jazor:clr-member static decimal.Negate(decimal)*/
export function _26945a698afa2a91(d) {
  return NegateDecimal(d);
}
/*jazor:clr-member static decimal.Round(decimal)*/
export function _4a816369b59f1ca3(d) {
  return RoundDecimal(d, 0);
}
/*jazor:clr-member static decimal.Round(decimal, int)*/
export function _bc3a974d51c694ab(d, decimals) {
  return RoundDecimal(d, decimals);
}
/*jazor:clr-member static decimal.Round(decimal, System.MidpointRounding)*/
export function _a334f7e82122cfc2(d, mode) {
  return RoundDecimal(d, 0, mode);
}
/*jazor:clr-member static decimal.Round(decimal, int, System.MidpointRounding)*/
export function _09ee3a4652dbe73c(d, decimals, mode) {
  return RoundDecimal(d, decimals, mode);
}
/*jazor:clr-member static decimal.Subtract(decimal, decimal)*/
export function _3e80f2d9cf753d05(d1, d2) {
  return SubtractDecimal(d1, d2);
}
/*jazor:clr-member static decimal.ToByte(decimal)*/
export function _d2aabede7e0207c1(value) {
  return ToCheckedNumber(value, 0n, get_ByteMaxValue(), "Byte");
}
/*jazor:clr-member static decimal.ToSByte(decimal)*/
export function _175bf5ee849fcf8f(value) {
  return ToCheckedNumber(value, get_SByteMinValue(), get_SByteMaxValue(), "SByte");
}
/*jazor:clr-member static decimal.ToInt16(decimal)*/
export function _5df8c6a064c50c5f(value) {
  return ToCheckedNumber(value, get_Int16MinValue(), get_Int16MaxValue(), "Int16");
}
/*jazor:clr-member static decimal.ToDouble(decimal)*/
export function _cfbbd251b43c99f4(d) {
  return Number(NormalizeDecimal(d));
}
/*jazor:clr-member static decimal.ToInt32(decimal)*/
export function _ad71e0d1a8679244(d) {
  return ToCheckedNumber(d, get_Int32MinValue(), get_Int32MaxValue(), "Int32");
}
/*jazor:clr-member static decimal.ToInt64(decimal)*/
export function _7a077e2e1baba462(d) {
  return ToCheckedBigInt(d, get_Int64MinValue(), get_Int64MaxValue(), "Int64");
}
/*jazor:clr-member static decimal.ToUInt16(decimal)*/
export function _21bc553743dd324b(value) {
  return ToCheckedNumber(value, 0n, get_UInt16MaxValue(), "UInt16");
}
/*jazor:clr-member static decimal.ToUInt32(decimal)*/
export function _c975b2e5b2f4c009(d) {
  return ToCheckedNumber(d, 0n, get_UInt32MaxValue(), "UInt32");
}
/*jazor:clr-member static decimal.ToUInt64(decimal)*/
export function _9b15def492d41a4a(d) {
  return ToCheckedBigInt(d, 0n, get_UInt64MaxValue(), "UInt64");
}
/*jazor:clr-member static decimal.ToSingle(decimal)*/
export function _1450e4ab34b1a945(d) {
  return Number(NormalizeDecimal(d));
}
/*jazor:clr-member static decimal.Truncate(decimal)*/
export function _be8b149ea0e1d76b(d) {
  return TruncateDecimal(d);
}
/*jazor:clr-member static decimal.implicit operator decimal(byte)*/
export function _c605c67b2cd1973c(value) {
  return value.toString() ?? "";
}
/*jazor:clr-member static decimal.implicit operator decimal(sbyte)*/
export function _e8d5240b7aa52784(value) {
  return value.toString() ?? "";
}
/*jazor:clr-member static decimal.implicit operator decimal(short)*/
export function _8635fe57a74e1249(value) {
  return value.toString() ?? "";
}
/*jazor:clr-member static decimal.implicit operator decimal(ushort)*/
export function _7c3cfa0de18bd43c(value) {
  return value.toString() ?? "";
}
/*jazor:clr-member static decimal.implicit operator decimal(char)*/
export function _d4af042bf014fd51(value) {
  return value.toString() ?? "";
}
/*jazor:clr-member static decimal.implicit operator decimal(int)*/
export function _f5a5d600ccd38777(value) {
  return value.toString() ?? "";
}
/*jazor:clr-member static decimal.implicit operator decimal(uint)*/
export function _d8b659cd861d2409(value) {
  return value.toString() ?? "";
}
/*jazor:clr-member static decimal.implicit operator decimal(long)*/
export function _23103e069358ca06(value) {
  return value.toString() ?? "";
}
/*jazor:clr-member static decimal.implicit operator decimal(ulong)*/
export function _7ab8c627f74cb718(value) {
  return value.toString() ?? "";
}
/*jazor:clr-member static decimal.explicit operator decimal(float)*/
export function _f456cac2ae523add(value) {
  return CreateDecimalFromNumber(value);
}
/*jazor:clr-member static decimal.explicit operator decimal(double)*/
export function _8f3a66f6dc828dff(value) {
  return CreateDecimalFromNumber(value);
}
/*jazor:clr-member static decimal.explicit operator byte(decimal)*/
export function _a8bfc1feb93c39cb(value) {
  return _d2aabede7e0207c1(value);
}
/*jazor:clr-member static decimal.explicit operator sbyte(decimal)*/
export function _824c1dbd3e6691ba(value) {
  return _175bf5ee849fcf8f(value);
}
/*jazor:clr-member static decimal.explicit operator char(decimal)*/
export function _e2c93b47df7960a8(value) {
  return ToCheckedNumber(value, 0n, get_UInt16MaxValue(), "Char");
}
/*jazor:clr-member static decimal.explicit operator short(decimal)*/
export function _8f4ca64a21fb08cc(value) {
  return _5df8c6a064c50c5f(value);
}
/*jazor:clr-member static decimal.explicit operator ushort(decimal)*/
export function _3e209c4283c6e05e(value) {
  return _21bc553743dd324b(value);
}
/*jazor:clr-member static decimal.explicit operator int(decimal)*/
export function _bc03e302b86b6800(value) {
  return _ad71e0d1a8679244(value);
}
/*jazor:clr-member static decimal.explicit operator uint(decimal)*/
export function _dea1c1c9c8f2b495(value) {
  return _c975b2e5b2f4c009(value);
}
/*jazor:clr-member static decimal.explicit operator long(decimal)*/
export function _df6860f57d568704(value) {
  return _7a077e2e1baba462(value);
}
/*jazor:clr-member static decimal.explicit operator ulong(decimal)*/
export function _047386be34a2d276(value) {
  return _9b15def492d41a4a(value);
}
/*jazor:clr-member static decimal.explicit operator float(decimal)*/
export function _2de5f5a183f9455b(value) {
  return _1450e4ab34b1a945(value);
}
/*jazor:clr-member static decimal.explicit operator double(decimal)*/
export function _2db2eb304fe215ee(value) {
  return _cfbbd251b43c99f4(value);
}
/*jazor:clr-member static decimal.operator +(decimal)*/
export function _53fb6447e19a3943(d) {
  return PreserveDecimal(d);
}
/*jazor:clr-member static decimal.operator -(decimal)*/
export function _ec128cb5140788f6(d) {
  return NegateDecimal(d);
}
/*jazor:clr-member static decimal.operator ++(decimal)*/
export function _20e1c565f1757f95(d) {
  return AddDecimal(d, "1");
}
/*jazor:clr-member static decimal.operator --(decimal)*/
export function _92103936e252998e(d) {
  return SubtractDecimal(d, "1");
}
/*jazor:clr-member static decimal.operator +(decimal, decimal)*/
export function _6916013808c205d4(d1, d2) {
  return AddDecimal(d1, d2);
}
/*jazor:clr-member static decimal.operator -(decimal, decimal)*/
export function _7b8c963ebbb0237b(d1, d2) {
  return SubtractDecimal(d1, d2);
}
/*jazor:clr-member static decimal.operator *(decimal, decimal)*/
export function _5794746a3d1c5c7d(d1, d2) {
  return MultiplyDecimal(d1, d2);
}
/*jazor:clr-member static decimal.operator /(decimal, decimal)*/
export function _18540fea4c4d81f3(d1, d2) {
  return DivideDecimal(d1, d2);
}
/*jazor:clr-member static decimal.operator %(decimal, decimal)*/
export function _cf5ffdcf799ce372(d1, d2) {
  return RemainderDecimal(d1, d2);
}
/*jazor:clr-member static decimal.operator ==(decimal, decimal)*/
export function _9831be72bebc3a57(d1, d2) {
  return CompareDecimal(d1, d2) === 0;
}
/*jazor:clr-member static decimal.operator !=(decimal, decimal)*/
export function _6e351e0d21e0ccd9(d1, d2) {
  return CompareDecimal(d1, d2) !== 0;
}
/*jazor:clr-member static decimal.operator <(decimal, decimal)*/
export function _9e3b1978bc32f62a(d1, d2) {
  return CompareDecimal(d1, d2) < 0;
}
/*jazor:clr-member static decimal.operator <=(decimal, decimal)*/
export function _01544ed3b8bf9a49(d1, d2) {
  return CompareDecimal(d1, d2) <= 0;
}
/*jazor:clr-member static decimal.operator >(decimal, decimal)*/
export function _bb8c4bd3620de56b(d1, d2) {
  return CompareDecimal(d1, d2) > 0;
}
/*jazor:clr-member static decimal.operator >=(decimal, decimal)*/
export function _325daf3875076acb(d1, d2) {
  return CompareDecimal(d1, d2) >= 0;
}
/*jazor:clr-member static decimal.Clamp(decimal, decimal, decimal)*/
export function _e886400fbfdbdaaa(value, min, max) {
  if (CompareDecimal(min, max) > 0)
    throw new Error("ArgumentException: min must be less than or equal to max.");
  if (CompareDecimal(value, min) < 0)
    return PreserveDecimal(min);
  if (CompareDecimal(value, max) > 0)
    return PreserveDecimal(max);
  return PreserveDecimal(value);
}
/*jazor:clr-member static decimal.CopySign(decimal, decimal)*/
export function _30df447725c40575(value, sign) {
  let absolute = AbsDecimal(value);
  return SignDecimal(sign) < 0 ? NegateDecimal(absolute) : absolute;
}
/*jazor:clr-member static decimal.Max(decimal, decimal)*/
export function _872018e11335480a(x, y) {
  return CompareDecimal(x, y) >= 0 ? PreserveDecimal(x) : PreserveDecimal(y);
}
/*jazor:clr-member static decimal.Min(decimal, decimal)*/
export function _ceb21f954af742e7(x, y) {
  return CompareDecimal(x, y) < 0 ? PreserveDecimal(x) : PreserveDecimal(y);
}
/*jazor:clr-member static decimal.Sign(decimal)*/
export function _ed803cf9c8c052f1(d) {
  return SignDecimal(d);
}
/*jazor:clr-member static decimal.Abs(decimal)*/
export function _e85678b4de2283e8(value) {
  return AbsDecimal(value);
}
/*jazor:clr-member static decimal.IsCanonical(decimal)*/
export function _b80d517d733633a6(value) {
  try {
    return value === NormalizeDecimal(value);
  } catch {
    return false;
  }
}
/*jazor:clr-member static decimal.IsEvenInteger(decimal)*/
export function _9d28fa751d24ce2e(value) {
  return IsIntegerDecimal(value) && TruncateToIntegralValue(value) % BigInt(2) === 0n;
}
/*jazor:clr-member static decimal.IsInteger(decimal)*/
export function _e79590278b446432(value) {
  return IsIntegerDecimal(value);
}
/*jazor:clr-member static decimal.IsNegative(decimal)*/
export function _1ad42f1c78dbe014(value) {
  return SignDecimal(value) < 0;
}
/*jazor:clr-member static decimal.IsOddInteger(decimal)*/
export function _38587400d9c44cb5(value) {
  return IsIntegerDecimal(value) && TruncateToIntegralValue(value) % BigInt(2) !== 0n;
}
/*jazor:clr-member static decimal.IsPositive(decimal)*/
export function _03c325899b0e33f0(value) {
  return SignDecimal(value) >= 0;
}
/*jazor:clr-member static decimal.MaxMagnitude(decimal, decimal)*/
export function _becce0ac49342bb2(x, y) {
  let ax = AbsDecimal(x);
  let ay = AbsDecimal(y);
  let comparison = CompareDecimal(ax, ay);
  if (comparison > 0)
    return PreserveDecimal(x);
  if (comparison < 0)
    return PreserveDecimal(y);
  return CompareDecimal(x, y) >= 0 ? PreserveDecimal(x) : PreserveDecimal(y);
}
/*jazor:clr-member static decimal.MinMagnitude(decimal, decimal)*/
export function _5df17b0a512de878(x, y) {
  let ax = AbsDecimal(x);
  let ay = AbsDecimal(y);
  let comparison = CompareDecimal(ax, ay);
  if (comparison < 0)
    return PreserveDecimal(x);
  if (comparison > 0)
    return PreserveDecimal(y);
  return CompareDecimal(x, y) <= 0 ? PreserveDecimal(x) : PreserveDecimal(y);
}
/*jazor:clr-member static decimal.TryParse(string, System.IFormatProvider, out decimal)*/
export function _a3ffdb214a9c82a0(s, provider, result) {
  if (s === null || s.length === 0)
    return [false, "0"];
  try {
    return [true, ParseDecimalExternal(s, get_NumberStyleNumber(), provider)];
  } catch {
    return [false, "0"];
  }
}
/*jazor:clr-member static decimal.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)*/
export function _c644fa2b15360347(s, provider) {
  return ParseDecimalExternal(s, get_NumberStyleNumber(), provider);
}
/*jazor:clr-member static decimal.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out decimal)*/
export function _7ac8df441c1485cf(s, provider, result) {
  return _a3ffdb214a9c82a0(s, provider, result);
}
/*jazor:clr-member static decimal.Parse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider)*/
export function _e81acb76373d457e(utf8Text, style, provider) {
  return _f525a420b2d600ec(DecodeUtf8OrThrowFormat(utf8Text), style, provider);
}
/*jazor:clr-member static decimal.TryParse(System.ReadOnlySpan<byte>, System.Globalization.NumberStyles, System.IFormatProvider, out decimal)*/
export function _acbda6e104ca3de4(utf8Text, style, provider, result) {
  return _b4ecd2424c9a371e(TryDecodeUtf8(utf8Text), style, provider, result);
}
/*jazor:clr-member static decimal.Parse(System.ReadOnlySpan<byte>, System.IFormatProvider)*/
export function _d3d821054d142668(utf8Text, provider) {
  return _01be2a34fe2cda4e(DecodeUtf8OrThrowFormat(utf8Text), provider);
}
/*jazor:clr-member static decimal.TryParse(System.ReadOnlySpan<byte>, System.IFormatProvider, out decimal)*/
export function _8122c647766e18ff(utf8Text, provider, result) {
  return _a3ffdb214a9c82a0(TryDecodeUtf8(utf8Text), provider, result);
}
