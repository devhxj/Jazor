import { DoubleModule } from "System/DoubleModule.js";
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
function createParts(unscaled, scale) {
  return [unscaled, scale];
}
function getUnscaled(parts) {
  return parts[0];
}
function getScale(parts) {
  return parts[1];
}
function createNumberSymbols(groupSeparator, decimalSeparator, primaryGroupSize, secondaryGroupSize) {
  return [groupSeparator, decimalSeparator, primaryGroupSize, secondaryGroupSize];
}
function getGroupSeparator(symbols) {
  return symbols[0];
}
function getDecimalSeparator(symbols) {
  return symbols[1];
}
function getPrimaryGroupSize(symbols) {
  return symbols[2];
}
function getSecondaryGroupSize(symbols) {
  return symbols[3];
}
function hasStyle(style, flag) {
  let integerStyle = style;
  let integerFlag = flag;
  return (integerStyle & integerFlag) === integerFlag;
}
function getNumberStylesValue(style) {
  let numberStyle, enumStyle;
  if (typeof style === "number" && (numberStyle = style, true))
    return numberStyle | 0;
  if (typeof style === "number" && (enumStyle = style, true))
    return Number(enumStyle);
  throw new Error("ArgumentException: Invalid NumberStyles value.");
}
function validateDecimalNumberStyles(style) {
  if (Math.floor(style) !== style || style < 0)
    throw new Error("ArgumentException: An undefined NumberStyles value is not supported.");
  if (hasStyle(style, allowHexSpecifierStyle) || hasStyle(style, allowBinarySpecifierStyle))
    throw new Error("ArgumentException: The number style AllowHexSpecifier or AllowBinarySpecifier is not supported on floating point data types.");
}
function getNumberSymbols(provider) {
  let locale, numberFormat;
  if (typeof provider === "string" && (locale = provider, true)) {
    if (locale.length === 0)
      return createNumberSymbols(",", ".", 3, 3);
    return GetNumberSymbols(new NumberFormat(locale));
  }
  if (provider instanceof NumberFormat && (numberFormat = provider, true))
    return GetNumberSymbols(numberFormat);
  return GetNumberSymbols(new NumberFormat);
}
function GetNumberSymbols(numberFormat) {
  let groupSeparator = ",";
  let decimalSeparator = ".";
  let integerParts = [];
  let parts = numberFormat.formatToParts(123456789.1);
  for (let i = 0; i < parts.length; i++) {
    let part = parts[i];
    if (part.type === "group" && groupSeparator.length === 0)
      groupSeparator = part.value;
    else if (part.type === "group")
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
  return createNumberSymbols(groupSeparator, decimalSeparator, primaryGroupSize, secondaryGroupSize);
}
function pow10(exponent) {
  let result = BigInt(1);
  for (let i = 0; i < exponent; i++)
    result *= BigInt(10);
  return result;
}
function maxNumber(left, right) {
  return left >= right ? left : right;
}
function repeatZero(count) {
  let text = "";
  for (let i = 0; i < count; i++)
    text += "0";
  return text;
}
function stripLeadingZeros(digits) {
  while (digits.length > 1 && _5ad63706a889c294(digits, 0) === "0")
    digits = digits.substring(1);
  return digits;
}
function normalizeParts(unscaled, scale) {
  if (unscaled === BigInt.zero)
    return createParts(BigInt.zero, 0);
  while (scale > 0 && unscaled % BigInt(10) === BigInt.zero) {
    unscaled /= BigInt(10);
    scale--;
  }
  if (scale < 0 || scale > maxFractionDigits)
    throw new Error("OverflowException: Value was either too large or too small for a Decimal.");
  let absolute = unscaled < BigInt.zero ? -unscaled : unscaled;
  if (absolute > maxDecimalUnscaled)
    throw new Error("OverflowException: Value was either too large or too small for a Decimal.");
  return createParts(unscaled, scale);
}
function parseDecimal(value, allowExponent = true) {
  let s = value.trim();
  if (s.length === 0)
    throw new Error("FormatException: String was not recognized as a valid Decimal.");
  let negative = false;
  if (_5ad63706a889c294(s, 0) === "+" || _5ad63706a889c294(s, 0) === "-") {
    negative = _5ad63706a889c294(s, 0) === "-";
    s = s.substring(1);
    if (s.length === 0)
      throw new Error(`FormatException: String '${value}' was not recognized as a valid Decimal.`);
  }
  let exponent = 0;
  let exponentIndex = s.indexOf("e");
  if (exponentIndex < 0)
    exponentIndex = s.indexOf("E");
  if (exponentIndex >= 0) {
    if (!allowExponent)
      throw new Error(`FormatException: String '${value}' was not recognized as a valid Decimal.`);
    if (exponentIndex === 0 || exponentIndex === s.length - 1)
      throw new Error(`FormatException: String '${value}' was not recognized as a valid Decimal.`);
    let exponentText = s.substring(exponentIndex + 1);
    let exponentValue = Number(exponentText);
    if (isNaN(exponentValue) || Math.floor(exponentValue) !== exponentValue)
      throw new Error(`FormatException: String '${value}' was not recognized as a valid Decimal.`);
    exponent = exponentValue;
    s = s.substring(0, 0 + exponentIndex);
    if (s.length === 0)
      throw new Error(`FormatException: String '${value}' was not recognized as a valid Decimal.`);
  }
  let dotIndex = s.indexOf(".");
  if (dotIndex >= 0 && s.indexOf(".", dotIndex + 1) >= 0)
    throw new Error(`FormatException: String '${value}' was not recognized as a valid Decimal.`);
  let integerDigits = dotIndex >= 0 ? s.substring(0, 0 + dotIndex) : s;
  let fractionDigits = dotIndex >= 0 ? s.substring(dotIndex + 1) : "";
  if (integerDigits.length === 0 && fractionDigits.length === 0)
    throw new Error(`FormatException: String '${value}' was not recognized as a valid Decimal.`);
  if (integerDigits.length === 0)
    integerDigits = "0";
  let digits = integerDigits + fractionDigits;
  if (digits.length === 0)
    throw new Error(`FormatException: String '${value}' was not recognized as a valid Decimal.`);
  for (let i = 0; i < digits.length; i++) {
    let c = _5ad63706a889c294(digits, i);
    if (c < "0" || c > "9")
      throw new Error(`FormatException: String '${value}' was not recognized as a valid Decimal.`);
  }
  digits = stripLeadingZeros(digits);
  let scale = fractionDigits.length - exponent;
  if (scale < 0) {
    digits += repeatZero(-scale);
    scale = 0;
  }
  let unscaled = BigInt(digits);
  if (negative && unscaled !== BigInt.zero)
    unscaled = -unscaled;
  return normalizeParts(unscaled, scale);
}
function normalizeDecimal(value) {
  let parts = parseDecimal(value);
  return formatDecimal(getUnscaled(parts), getScale(parts));
}
function removeAllOccurrences(value, token) {
  return token.length === 0 ? value : value.replaceAll(token, "");
}
function replaceDecimalSeparator(value, decimalSeparator) {
  if (decimalSeparator.length === 0 || decimalSeparator === ".")
    return value;
  return value.replaceAll(decimalSeparator, ".");
}
function normalizeExternalDecimalText(value, style, provider) {
  validateDecimalNumberStyles(style);
  let text = value;
  let allowLeadingWhite = hasStyle(style, allowLeadingWhiteStyle);
  let allowTrailingWhite = hasStyle(style, allowTrailingWhiteStyle);
  if (allowLeadingWhite && allowTrailingWhite)
    text = text.trim();
  else if (allowLeadingWhite)
    text = text.trimStart();
  else if (allowTrailingWhite)
    text = text.trimEnd();
  if (text.length === 0)
    throw new Error(`FormatException: String '${value}' was not recognized as a valid Decimal.`);
  let negative = false;
  if (hasStyle(style, allowParenthesesStyle) && text.length >= 2 && _5ad63706a889c294(text, 0) === "(" && _5ad63706a889c294(text, text.length - 1) === ")") {
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
    throw new Error(`FormatException: String '${value}' was not recognized as a valid Decimal.`);
  if (_5ad63706a889c294(text, 0) === "+" || _5ad63706a889c294(text, 0) === "-") {
    if (!hasStyle(style, allowLeadingSignStyle))
      throw new Error(`FormatException: String '${value}' was not recognized as a valid Decimal.`);
    negative = _5ad63706a889c294(text, 0) === "-" ? !negative : negative;
    text = text.substring(1);
  }
  if (text.length === 0)
    throw new Error(`FormatException: String '${value}' was not recognized as a valid Decimal.`);
  if (_5ad63706a889c294(text, text.length - 1) === "+" || _5ad63706a889c294(text, text.length - 1) === "-") {
    if (!hasStyle(style, allowTrailingSignStyle))
      throw new Error(`FormatException: String '${value}' was not recognized as a valid Decimal.`);
    negative = _5ad63706a889c294(text, text.length - 1) === "-" ? !negative : negative;
    text = text.substring(0, 0 + (text.length - 1));
  }
  if (text.length === 0)
    throw new Error(`FormatException: String '${value}' was not recognized as a valid Decimal.`);
  let symbols = getNumberSymbols(provider);
  let groupSeparator = getGroupSeparator(symbols);
  let decimalSeparator = getDecimalSeparator(symbols);
  if (groupSeparator.length !== 0) {
    if (hasStyle(style, allowThousandsStyle))
      text = removeAllOccurrences(text, groupSeparator);
    else if (text.indexOf(groupSeparator) >= 0)
      throw new Error(`FormatException: String '${value}' was not recognized as a valid Decimal.`);
  }
  if (decimalSeparator !== ".") {
    if (text.indexOf(decimalSeparator) >= 0) {
      if (!hasStyle(style, allowDecimalPointStyle))
        throw new Error(`FormatException: String '${value}' was not recognized as a valid Decimal.`);
      text = replaceDecimalSeparator(text, decimalSeparator);
    }
  }
  if (!hasStyle(style, allowDecimalPointStyle) && text.indexOf(".") >= 0)
    throw new Error(`FormatException: String '${value}' was not recognized as a valid Decimal.`);
  if (negative)
    text = "-" + text;
  return text;
}
function parseDecimalExternal(value, style, provider) {
  let normalized = normalizeExternalDecimalText(value, style, provider);
  let parts = parseDecimal(normalized, hasStyle(style, allowExponentStyle));
  return formatDecimal(getUnscaled(parts), getScale(parts));
}
function formatDecimal(unscaled, scale) {
  let normalized = normalizeParts(unscaled, scale);
  unscaled = getUnscaled(normalized);
  scale = getScale(normalized);
  if (unscaled === BigInt.zero)
    return "0";
  let negative = unscaled < BigInt.zero;
  let absolute = negative ? -unscaled : unscaled;
  let digits = absolute.toString();
  if (scale === 0)
    return negative ? "-" + digits : digits;
  if (digits.length <= scale)
    digits = repeatZero(scale - digits.length + 1) + digits;
  let split = digits.length - scale;
  let text = digits.substring(0, 0 + split) + "." + digits.substring(split);
  return negative ? "-" + text : text;
}
function formatDecimalToScale(value, scale) {
  let parts = parseDecimal(value);
  let unscaled = getUnscaled(parts);
  let negative = unscaled < BigInt.zero;
  let absolute = negative ? -unscaled : unscaled;
  let digits = absolute.toString();
  if (scale === 0)
    return negative ? "-" + digits : digits;
  if (digits.length <= scale)
    digits = repeatZero(scale - digits.length + 1) + digits;
  let split = digits.length - scale;
  let text = digits.substring(0, 0 + split) + "." + digits.substring(split);
  return negative ? "-" + text : text;
}
function insertGroupSeparators(integerDigits, separator, primaryGroupSize, secondaryGroupSize) {
  if (separator.length === 0 || integerDigits.length <= primaryGroupSize)
    return integerDigits;
  let groups = [];
  let end = integerDigits.length;
  let size = primaryGroupSize;
  while (end > size) {
    groups.push(integerDigits.substring(end - size, end - size + size));
    end -= size;
    size = secondaryGroupSize;
  }
  let result = integerDigits.substring(0, 0 + end);
  for (let i = groups.length - 1; i >= 0; i--)
    result += separator + groups[i];
  return result;
}
function applyNumberSeparators(value, provider) {
  let symbols = getNumberSymbols(provider);
  let groupSeparator = getGroupSeparator(symbols);
  let decimalSeparator = getDecimalSeparator(symbols);
  let primaryGroupSize = getPrimaryGroupSize(symbols);
  let secondaryGroupSize = getSecondaryGroupSize(symbols);
  let sign = "";
  let digits = value;
  if (_5ad63706a889c294(digits, 0) === "-") {
    sign = "-";
    digits = digits.substring(1);
  }
  let dotIndex = digits.indexOf(".");
  let integerDigits = dotIndex >= 0 ? digits.substring(0, 0 + dotIndex) : digits;
  let fractionDigits = dotIndex >= 0 ? digits.substring(dotIndex + 1) : "";
  let groupedInteger = insertGroupSeparators(integerDigits, groupSeparator, primaryGroupSize, secondaryGroupSize);
  if (fractionDigits.length === 0)
    return sign + groupedInteger;
  return sign + groupedInteger + decimalSeparator + fractionDigits;
}
function parsePrecision(format, defaultValue) {
  if (format.length === 1)
    return defaultValue;
  let precisionText = format.substring(1);
  for (let i = 0; i < precisionText.length; i++) {
    let c = _5ad63706a889c294(precisionText, i);
    if (c < "0" || c > "9")
      throw new Error("FormatException: Format specifier was invalid.");
  }
  return Number(precisionText);
}
function isSimpleCustomDecimalFormat(format) {
  for (let i = 0; i < format.length; i++) {
    let c = _5ad63706a889c294(format, i);
    if (c !== "0" && c !== "#" && c !== "." && c !== ",")
      return false;
  }
  return true;
}
function formatDecimalWithFormat(value, format, provider) {
  if (format === null || format.length === 0)
    return normalizeDecimal(value);
  let specifier = _5ad63706a889c294(format, 0);
  if ((specifier === "G" || specifier === "g") && format.length === 1)
    return normalizeDecimal(value);
  if (specifier === "F" || specifier === "f") {
    let precision = parsePrecision(format, defaultFixedPrecision);
    return formatDecimalToScale(roundDecimal(value, precision), precision);
  }
  if (specifier === "N" || specifier === "n") {
    let precision = parsePrecision(format, defaultNumberPrecision);
    return applyNumberSeparators(formatDecimalToScale(roundDecimal(value, precision), precision), provider);
  }
  if (isSimpleCustomDecimalFormat(format)) {
    let dotIndex = format.indexOf(".");
    let scale = dotIndex < 0 ? 0 : format.length - dotIndex - 1;
    let formatted = formatDecimalToScale(roundDecimal(value, scale), scale);
    return format.indexOf(",") >= 0 ? applyNumberSeparators(formatted, provider) : formatted;
  }
  throw new Error("FormatException: Format specifier was invalid.");
}
function createDecimalFromNumber(value) {
  if (!DoubleModule._aed2927097617729(value))
    throw new Error("OverflowException: Value was either too large or too small for a Decimal.");
  return normalizeDecimal(value.toString());
}
function alignUnscaled(value, targetScale) {
  let scale = getScale(value);
  if (targetScale <= scale)
    return getUnscaled(value);
  return getUnscaled(value) * pow10(targetScale - scale);
}
function truncateToIntegralValue(value) {
  let parts = parseDecimal(value);
  let scale = getScale(parts);
  if (scale === 0)
    return getUnscaled(parts);
  return getUnscaled(parts) / pow10(scale);
}
function toCheckedNumber(value, min, max, typeName) {
  let integral = truncateToIntegralValue(value);
  if (integral < min || integral > max)
    throw new Error(`OverflowException: Value was either too large or too small for a ${typeName}.`);
  return Number(integral);
}
function toCheckedBigInt(value, min, max, typeName) {
  let integral = truncateToIntegralValue(value);
  if (integral < min || integral > max)
    throw new Error(`OverflowException: Value was either too large or too small for a ${typeName}.`);
  return integral;
}
function getMidpointRoundingValue(mode) {
  let numberMode, enumMode;
  if (typeof mode === "number" && (numberMode = mode, true))
    return numberMode;
  if (typeof mode === "number" && (enumMode = mode, true))
    return Number(enumMode);
  throw new Error("ArgumentException: Invalid MidpointRounding value.");
}
function roundDecimal(value, decimals, mode = null) {
  if (Math.floor(decimals) !== decimals || decimals < 0 || decimals > maxFractionDigits)
    throw new Error("ArgumentOutOfRangeException: Decimal digits must be between 0 and 28.");
  let modeValue = mode === null ? Number(0) : getMidpointRoundingValue(mode);
  if (modeValue < 0 || modeValue > 4 || Math.floor(modeValue) !== modeValue)
    throw new Error("ArgumentException: Invalid MidpointRounding value.");
  let parts = parseDecimal(value);
  let scale = getScale(parts);
  let unscaled = getUnscaled(parts);
  if (scale <= decimals)
    return formatDecimal(unscaled, scale);
  let trimScale = scale - decimals;
  let divisor = pow10(trimScale);
  let quotient = unscaled / divisor;
  let remainder = unscaled % divisor;
  if (remainder === BigInt.zero)
    return formatDecimal(quotient, decimals);
  let negative = unscaled < BigInt.zero;
  if (modeValue === 2)
    return formatDecimal(quotient, decimals);
  if (modeValue === 3)
    return formatDecimal(negative ? quotient - BigInt(1) : quotient, decimals);
  if (modeValue === 4)
    return formatDecimal(negative ? quotient : quotient + BigInt(1), decimals);
  let absoluteRemainder = negative ? -remainder : remainder;
  let comparison = absoluteRemainder * BigInt(2) - divisor;
  if (comparison < BigInt.zero)
    return formatDecimal(quotient, decimals);
  let step = negative ? -BigInt(1) : BigInt(1);
  if (comparison > BigInt.zero || modeValue === 1)
    return formatDecimal(quotient + step, decimals);
  return quotient % BigInt(2) === BigInt.zero ? formatDecimal(quotient, decimals) : formatDecimal(quotient + step, decimals);
}
function compareDecimal(left, right) {
  let a = parseDecimal(left);
  let b = parseDecimal(right);
  let targetScale = maxNumber(getScale(a), getScale(b));
  let leftValue = alignUnscaled(a, targetScale);
  let rightValue = alignUnscaled(b, targetScale);
  if (leftValue < rightValue)
    return -1;
  if (leftValue > rightValue)
    return 1;
  return 0;
}
function addDecimal(left, right) {
  let a = parseDecimal(left);
  let b = parseDecimal(right);
  let targetScale = maxNumber(getScale(a), getScale(b));
  return formatDecimal(alignUnscaled(a, targetScale) + alignUnscaled(b, targetScale), targetScale);
}
function subtractDecimal(left, right) {
  return addDecimal(left, negateDecimal(right));
}
function negateDecimal(value) {
  let parts = parseDecimal(value);
  return formatDecimal(-getUnscaled(parts), getScale(parts));
}
function multiplyDecimal(left, right) {
  let a = parseDecimal(left);
  let b = parseDecimal(right);
  return formatDecimal(getUnscaled(a) * getUnscaled(b), getScale(a) + getScale(b));
}
function divideAndRound(numerator, denominator) {
  let quotient = numerator / denominator;
  let remainder = numerator % denominator;
  if (remainder === BigInt.zero)
    return quotient;
  let absoluteRemainder = remainder < BigInt.zero ? -remainder : remainder;
  let absoluteDenominator = denominator < BigInt.zero ? -denominator : denominator;
  let comparison = absoluteRemainder * BigInt(2) - absoluteDenominator;
  if (comparison < BigInt.zero)
    return quotient;
  let step = numerator < BigInt.zero ? -BigInt(1) : BigInt(1);
  if (comparison > BigInt.zero)
    return quotient + step;
  return quotient % BigInt(2) === BigInt.zero ? quotient : quotient + step;
}
function divideDecimal(left, right) {
  let a = parseDecimal(left);
  let b = parseDecimal(right);
  if (getUnscaled(b) === BigInt.zero)
    throw new Error("DivideByZeroException: Attempted to divide by zero.");
  let scaleDelta = maxFractionDigits + getScale(b) - getScale(a);
  let numerator = getUnscaled(a);
  let denominator = getUnscaled(b);
  if (scaleDelta >= 0)
    numerator *= pow10(scaleDelta);
  else
    denominator *= pow10(-scaleDelta);
  return formatDecimal(divideAndRound(numerator, denominator), maxFractionDigits);
}
function remainderDecimal(left, right) {
  let a = parseDecimal(left);
  let b = parseDecimal(right);
  if (getUnscaled(b) === BigInt.zero)
    throw new Error("DivideByZeroException: Attempted to divide by zero.");
  let targetScale = maxNumber(getScale(a), getScale(b));
  return formatDecimal(alignUnscaled(a, targetScale) % alignUnscaled(b, targetScale), targetScale);
}
function floorDecimal(value) {
  let parts = parseDecimal(value);
  let scale = getScale(parts);
  let unscaled = getUnscaled(parts);
  if (scale === 0)
    return formatDecimal(unscaled, 0);
  let divisor = pow10(scale);
  let quotient = unscaled / divisor;
  let remainder = unscaled % divisor;
  if (remainder !== BigInt.zero && unscaled < BigInt.zero)
    quotient -= BigInt(1);
  return formatDecimal(quotient, 0);
}
function ceilingDecimal(value) {
  let parts = parseDecimal(value);
  let scale = getScale(parts);
  let unscaled = getUnscaled(parts);
  if (scale === 0)
    return formatDecimal(unscaled, 0);
  let divisor = pow10(scale);
  let quotient = unscaled / divisor;
  let remainder = unscaled % divisor;
  if (remainder !== BigInt.zero && unscaled > BigInt.zero)
    quotient += BigInt(1);
  return formatDecimal(quotient, 0);
}
function truncateDecimal(value) {
  let parts = parseDecimal(value);
  let scale = getScale(parts);
  let unscaled = getUnscaled(parts);
  if (scale === 0)
    return formatDecimal(unscaled, 0);
  return formatDecimal(unscaled / pow10(scale), 0);
}
function absDecimal(value) {
  let parts = parseDecimal(value);
  let unscaled = getUnscaled(parts);
  return formatDecimal(unscaled < BigInt.zero ? -unscaled : unscaled, getScale(parts));
}
function signDecimal(value) {
  let parts = parseDecimal(value);
  let unscaled = getUnscaled(parts);
  if (unscaled < BigInt.zero)
    return -1;
  if (unscaled > BigInt.zero)
    return 1;
  return 0;
}
function isIntegerDecimal(value) {
  return getScale(parseDecimal(value)) === 0;
}
function getStringHashCode(text) {
  let hash = 0;
  for (let i = 0; i < text.length; i++)
    hash = (hash << 5) - hash + _5ad63706a889c294(text, i);
  return hash | 0;
}
export function _5faf9ddf65d02495() {
  return "0";
}
export function _3db06a98834e6ef8() {
  return "1";
}
export function _9311127a9ca2b91d() {
  return "-1";
}
export function _6a4e5f697d4fc607() {
  return "79228162514264337593543950335";
}
export function _cc6392a7d6df1e14() {
  return "-79228162514264337593543950335";
}
export function _2f7f0d9035a4bbf6(value) {
  return createDecimalFromNumber(value);
}
export function _cb7c7a937d3b8460(value) {
  return createDecimalFromNumber(value);
}
export function _db7e7c8def75fee8(instance) {
  return getScale(parseDecimal(instance));
}
export function _f73258f14e05c790(d1, d2) {
  return addDecimal(d1, d2);
}
export function _84028a6e79626057(d) {
  return ceilingDecimal(d);
}
export function _c11e0aef6b5ccf1e(d1, d2) {
  return compareDecimal(d1, d2);
}
export function _ff0e77ab6566e092(instance, value) {
  if (value === null)
    return 1;
  let other = value;
  if (other === null)
    throw new Error("ArgumentException: Object must be of type Decimal.");
  return _ca8a78810233056c(instance, other);
}
export function _ca8a78810233056c(instance, value) {
  return compareDecimal(instance, value);
}
export function _f5c1c0a2a040b000(d1, d2) {
  return divideDecimal(d1, d2);
}
export function _8abe47785e51f122(instance, value) {
  let other = value;
  return other !== null && compareDecimal(instance, other) === 0;
}
export function _3dfd87d9d2f35e11(instance, value) {
  return compareDecimal(instance, value) === 0;
}
export function _f58659c33299d2b1(instance) {
  return getStringHashCode(normalizeDecimal(instance));
}
export function _b25c4446c28ed255(d1, d2) {
  return compareDecimal(d1, d2) === 0;
}
export function _518facaaeeb29ead(d) {
  return floorDecimal(d);
}
export function _65a0e4fe8ccdd829(instance) {
  return normalizeDecimal(instance);
}
export function _af32d07083f1da07(instance, format) {
  return formatDecimalWithFormat(instance, format, null);
}
export function _6234ba988b3e006d(instance, provider) {
  return formatDecimalWithFormat(instance, null, provider);
}
export function _b1e6a06111674f0c(instance, format, provider) {
  return formatDecimalWithFormat(instance, format, provider);
}
export function _91a2436283a24315(s) {
  return parseDecimalExternal(s, numberStyleNumber, null);
}
export function _79a0e8ede29256cc(s, style) {
  return parseDecimalExternal(s, getNumberStylesValue(style), null);
}
export function _01be2a34fe2cda4e(s, provider) {
  return parseDecimalExternal(s, numberStyleNumber, provider);
}
export function _f525a420b2d600ec(s, style, provider) {
  return parseDecimalExternal(s, getNumberStylesValue(style), provider);
}
export function _8e0c949ee2411c7f(s, style, provider) {
  return parseDecimalExternal(s, getNumberStylesValue(style), provider);
}
export function _e96278809bb50e35(s, result) {
  if (s === null || s.length === 0)
    return [false, "0"];
  try {
    return [true, parseDecimalExternal(s, numberStyleNumber, null)];
  } catch {
    return [false, "0"];
  }
}
export function _5f6432cf52162431(s, result) {
  return _e96278809bb50e35(s, result);
}
export function _b4ecd2424c9a371e(s, style, provider, result) {
  let styleValue = getNumberStylesValue(style);
  validateDecimalNumberStyles(styleValue);
  if (s === null || s.length === 0)
    return [false, "0"];
  try {
    return [true, parseDecimalExternal(s, styleValue, provider)];
  } catch {
    return [false, "0"];
  }
}
export function _ed6b24306e2ef5cd(s, style, provider, result) {
  return _b4ecd2424c9a371e(s, style, provider, result);
}
export function _700359e0de148ee3(d1, d2) {
  return remainderDecimal(d1, d2);
}
export function _d5be5da3d4effe96(d1, d2) {
  return multiplyDecimal(d1, d2);
}
export function _26945a698afa2a91(d) {
  return negateDecimal(d);
}
export function _4a816369b59f1ca3(d) {
  return roundDecimal(d, 0);
}
export function _bc3a974d51c694ab(d, decimals) {
  return roundDecimal(d, decimals);
}
export function _a334f7e82122cfc2(d, mode) {
  return roundDecimal(d, 0, mode);
}
export function _09ee3a4652dbe73c(d, decimals, mode) {
  return roundDecimal(d, decimals, mode);
}
export function _3e80f2d9cf753d05(d1, d2) {
  return subtractDecimal(d1, d2);
}
export function _d2aabede7e0207c1(value) {
  return toCheckedNumber(value, BigInt.zero, byteMaxValue, "Byte");
}
export function _175bf5ee849fcf8f(value) {
  return toCheckedNumber(value, sByteMinValue, sByteMaxValue, "SByte");
}
export function _5df8c6a064c50c5f(value) {
  return toCheckedNumber(value, int16MinValue, int16MaxValue, "Int16");
}
export function _cfbbd251b43c99f4(d) {
  return Number(normalizeDecimal(d));
}
export function _ad71e0d1a8679244(d) {
  return toCheckedNumber(d, int32MinValue, int32MaxValue, "Int32");
}
export function _7a077e2e1baba462(d) {
  return toCheckedBigInt(d, int64MinValue, int64MaxValue, "Int64");
}
export function _21bc553743dd324b(value) {
  return toCheckedNumber(value, BigInt.zero, uInt16MaxValue, "UInt16");
}
export function _c975b2e5b2f4c009(d) {
  return toCheckedNumber(d, BigInt.zero, uInt32MaxValue, "UInt32");
}
export function _9b15def492d41a4a(d) {
  return toCheckedBigInt(d, BigInt.zero, uInt64MaxValue, "UInt64");
}
export function _1450e4ab34b1a945(d) {
  return Number(normalizeDecimal(d));
}
export function _be8b149ea0e1d76b(d) {
  return truncateDecimal(d);
}
export function _c605c67b2cd1973c(value) {
  return value.toString() ?? "";
}
export function _e8d5240b7aa52784(value) {
  return value.toString() ?? "";
}
export function _8635fe57a74e1249(value) {
  return value.toString() ?? "";
}
export function _7c3cfa0de18bd43c(value) {
  return value.toString() ?? "";
}
export function _d4af042bf014fd51(value) {
  return value.toString() ?? "";
}
export function _f5a5d600ccd38777(value) {
  return value.toString() ?? "";
}
export function _d8b659cd861d2409(value) {
  return value.toString() ?? "";
}
export function _23103e069358ca06(value) {
  return value.toString() ?? "";
}
export function _7ab8c627f74cb718(value) {
  return value.toString() ?? "";
}
export function _f456cac2ae523add(value) {
  return createDecimalFromNumber(value);
}
export function _8f3a66f6dc828dff(value) {
  return createDecimalFromNumber(value);
}
export function _a8bfc1feb93c39cb(value) {
  return _d2aabede7e0207c1(value);
}
export function _824c1dbd3e6691ba(value) {
  return _175bf5ee849fcf8f(value);
}
export function _e2c93b47df7960a8(value) {
  return toCheckedNumber(value, BigInt.zero, uInt16MaxValue, "Char");
}
export function _8f4ca64a21fb08cc(value) {
  return _5df8c6a064c50c5f(value);
}
export function _3e209c4283c6e05e(value) {
  return _21bc553743dd324b(value);
}
export function _bc03e302b86b6800(value) {
  return _ad71e0d1a8679244(value);
}
export function _dea1c1c9c8f2b495(value) {
  return _c975b2e5b2f4c009(value);
}
export function _df6860f57d568704(value) {
  return _7a077e2e1baba462(value);
}
export function _047386be34a2d276(value) {
  return _9b15def492d41a4a(value);
}
export function _2de5f5a183f9455b(value) {
  return _1450e4ab34b1a945(value);
}
export function _2db2eb304fe215ee(value) {
  return _cfbbd251b43c99f4(value);
}
export function _53fb6447e19a3943(d) {
  return normalizeDecimal(d);
}
export function _ec128cb5140788f6(d) {
  return negateDecimal(d);
}
export function _20e1c565f1757f95(d) {
  return addDecimal(d, "1");
}
export function _92103936e252998e(d) {
  return subtractDecimal(d, "1");
}
export function _6916013808c205d4(d1, d2) {
  return addDecimal(d1, d2);
}
export function _7b8c963ebbb0237b(d1, d2) {
  return subtractDecimal(d1, d2);
}
export function _5794746a3d1c5c7d(d1, d2) {
  return multiplyDecimal(d1, d2);
}
export function _18540fea4c4d81f3(d1, d2) {
  return divideDecimal(d1, d2);
}
export function _cf5ffdcf799ce372(d1, d2) {
  return remainderDecimal(d1, d2);
}
export function _9831be72bebc3a57(d1, d2) {
  return compareDecimal(d1, d2) === 0;
}
export function _6e351e0d21e0ccd9(d1, d2) {
  return compareDecimal(d1, d2) !== 0;
}
export function _9e3b1978bc32f62a(d1, d2) {
  return compareDecimal(d1, d2) < 0;
}
export function _01544ed3b8bf9a49(d1, d2) {
  return compareDecimal(d1, d2) <= 0;
}
export function _bb8c4bd3620de56b(d1, d2) {
  return compareDecimal(d1, d2) > 0;
}
export function _325daf3875076acb(d1, d2) {
  return compareDecimal(d1, d2) >= 0;
}
export function _e886400fbfdbdaaa(value, min, max) {
  if (compareDecimal(value, min) < 0)
    return normalizeDecimal(min);
  if (compareDecimal(value, max) > 0)
    return normalizeDecimal(max);
  return normalizeDecimal(value);
}
export function _30df447725c40575(value, sign) {
  let absolute = absDecimal(value);
  return signDecimal(sign) < 0 ? negateDecimal(absolute) : absolute;
}
export function _872018e11335480a(x, y) {
  return compareDecimal(x, y) >= 0 ? normalizeDecimal(x) : normalizeDecimal(y);
}
export function _ceb21f954af742e7(x, y) {
  return compareDecimal(x, y) <= 0 ? normalizeDecimal(x) : normalizeDecimal(y);
}
export function _ed803cf9c8c052f1(d) {
  return signDecimal(d);
}
export function _e85678b4de2283e8(value) {
  return absDecimal(value);
}
export function _b80d517d733633a6(value) {
  try {
    return value === normalizeDecimal(value);
  } catch {
    return false;
  }
}
export function _9d28fa751d24ce2e(value) {
  let parts = parseDecimal(value);
  return getScale(parts) === 0 && getUnscaled(parts) % BigInt(2) === BigInt.zero;
}
export function _e79590278b446432(value) {
  return isIntegerDecimal(value);
}
export function _1ad42f1c78dbe014(value) {
  return signDecimal(value) < 0;
}
export function _38587400d9c44cb5(value) {
  let parts = parseDecimal(value);
  return getScale(parts) === 0 && getUnscaled(parts) % BigInt(2) !== BigInt.zero;
}
export function _03c325899b0e33f0(value) {
  return signDecimal(value) >= 0;
}
export function _becce0ac49342bb2(x, y) {
  let ax = absDecimal(x);
  let ay = absDecimal(y);
  let comparison = compareDecimal(ax, ay);
  if (comparison > 0)
    return normalizeDecimal(x);
  if (comparison < 0)
    return normalizeDecimal(y);
  return compareDecimal(x, y) >= 0 ? normalizeDecimal(x) : normalizeDecimal(y);
}
export function _5df17b0a512de878(x, y) {
  let ax = absDecimal(x);
  let ay = absDecimal(y);
  let comparison = compareDecimal(ax, ay);
  if (comparison < 0)
    return normalizeDecimal(x);
  if (comparison > 0)
    return normalizeDecimal(y);
  return compareDecimal(x, y) <= 0 ? normalizeDecimal(x) : normalizeDecimal(y);
}
export function _a3ffdb214a9c82a0(s, provider, result) {
  if (s === null || s.length === 0)
    return [false, "0"];
  try {
    return [true, parseDecimalExternal(s, numberStyleNumber, provider)];
  } catch {
    return [false, "0"];
  }
}
export function _c644fa2b15360347(s, provider) {
  return parseDecimalExternal(s, numberStyleNumber, provider);
}
export function _7ac8df441c1485cf(s, provider, result) {
  return _a3ffdb214a9c82a0(s, provider, result);
}
export const DecimalModule = {
  get_MaxFractionDigits,
  get_MaxDecimalUnscaled,
  get_Int64MinValue,
  get_Int64MaxValue,
  get_UInt64MaxValue,
  get_Int32MinValue,
  get_Int32MaxValue,
  get_UInt32MaxValue,
  get_Int16MinValue,
  get_Int16MaxValue,
  get_UInt16MaxValue,
  get_SByteMinValue,
  get_SByteMaxValue,
  get_ByteMaxValue,
  get_AllowLeadingWhiteStyle,
  get_AllowTrailingWhiteStyle,
  get_AllowLeadingSignStyle,
  get_AllowTrailingSignStyle,
  get_AllowParenthesesStyle,
  get_AllowDecimalPointStyle,
  get_AllowThousandsStyle,
  get_AllowExponentStyle,
  get_AllowCurrencySymbolStyle,
  get_AllowHexSpecifierStyle,
  get_AllowBinarySpecifierStyle,
  get_NumberStyleNumber,
  get_DefaultFixedPrecision,
  get_DefaultNumberPrecision,
  createParts,
  getUnscaled,
  getScale,
  createNumberSymbols,
  getGroupSeparator,
  getDecimalSeparator,
  getPrimaryGroupSize,
  getSecondaryGroupSize,
  hasStyle,
  getNumberStylesValue,
  validateDecimalNumberStyles,
  getNumberSymbols,
  getNumberSymbols: GetNumberSymbols,
  pow10,
  maxNumber,
  repeatZero,
  stripLeadingZeros,
  normalizeParts,
  parseDecimal,
  normalizeDecimal,
  removeAllOccurrences,
  replaceDecimalSeparator,
  normalizeExternalDecimalText,
  parseDecimalExternal,
  formatDecimal,
  formatDecimalToScale,
  insertGroupSeparators,
  applyNumberSeparators,
  parsePrecision,
  isSimpleCustomDecimalFormat,
  formatDecimalWithFormat,
  createDecimalFromNumber,
  alignUnscaled,
  truncateToIntegralValue,
  toCheckedNumber,
  toCheckedBigInt,
  getMidpointRoundingValue,
  roundDecimal,
  compareDecimal,
  addDecimal,
  subtractDecimal,
  negateDecimal,
  multiplyDecimal,
  divideAndRound,
  divideDecimal,
  remainderDecimal,
  floorDecimal,
  ceilingDecimal,
  truncateDecimal,
  absDecimal,
  signDecimal,
  isIntegerDecimal,
  getStringHashCode,
  _5faf9ddf65d02495,
  _3db06a98834e6ef8,
  _9311127a9ca2b91d,
  _6a4e5f697d4fc607,
  _cc6392a7d6df1e14,
  _2f7f0d9035a4bbf6,
  _cb7c7a937d3b8460,
  _db7e7c8def75fee8,
  _f73258f14e05c790,
  _84028a6e79626057,
  _c11e0aef6b5ccf1e,
  _ff0e77ab6566e092,
  _ca8a78810233056c,
  _f5c1c0a2a040b000,
  _8abe47785e51f122,
  _3dfd87d9d2f35e11,
  _f58659c33299d2b1,
  _b25c4446c28ed255,
  _518facaaeeb29ead,
  _65a0e4fe8ccdd829,
  _af32d07083f1da07,
  _6234ba988b3e006d,
  _b1e6a06111674f0c,
  _91a2436283a24315,
  _79a0e8ede29256cc,
  _01be2a34fe2cda4e,
  _f525a420b2d600ec,
  _8e0c949ee2411c7f,
  _e96278809bb50e35,
  _5f6432cf52162431,
  _b4ecd2424c9a371e,
  _ed6b24306e2ef5cd,
  _700359e0de148ee3,
  _d5be5da3d4effe96,
  _26945a698afa2a91,
  _4a816369b59f1ca3,
  _bc3a974d51c694ab,
  _a334f7e82122cfc2,
  _09ee3a4652dbe73c,
  _3e80f2d9cf753d05,
  _d2aabede7e0207c1,
  _175bf5ee849fcf8f,
  _5df8c6a064c50c5f,
  _cfbbd251b43c99f4,
  _ad71e0d1a8679244,
  _7a077e2e1baba462,
  _21bc553743dd324b,
  _c975b2e5b2f4c009,
  _9b15def492d41a4a,
  _1450e4ab34b1a945,
  _be8b149ea0e1d76b,
  _c605c67b2cd1973c,
  _e8d5240b7aa52784,
  _8635fe57a74e1249,
  _7c3cfa0de18bd43c,
  _d4af042bf014fd51,
  _f5a5d600ccd38777,
  _d8b659cd861d2409,
  _23103e069358ca06,
  _7ab8c627f74cb718,
  _f456cac2ae523add,
  _8f3a66f6dc828dff,
  _a8bfc1feb93c39cb,
  _824c1dbd3e6691ba,
  _e2c93b47df7960a8,
  _8f4ca64a21fb08cc,
  _3e209c4283c6e05e,
  _bc03e302b86b6800,
  _dea1c1c9c8f2b495,
  _df6860f57d568704,
  _047386be34a2d276,
  _2de5f5a183f9455b,
  _2db2eb304fe215ee,
  _53fb6447e19a3943,
  _ec128cb5140788f6,
  _20e1c565f1757f95,
  _92103936e252998e,
  _6916013808c205d4,
  _7b8c963ebbb0237b,
  _5794746a3d1c5c7d,
  _18540fea4c4d81f3,
  _cf5ffdcf799ce372,
  _9831be72bebc3a57,
  _6e351e0d21e0ccd9,
  _9e3b1978bc32f62a,
  _01544ed3b8bf9a49,
  _bb8c4bd3620de56b,
  _325daf3875076acb,
  _e886400fbfdbdaaa,
  _30df447725c40575,
  _872018e11335480a,
  _ceb21f954af742e7,
  _ed803cf9c8c052f1,
  _e85678b4de2283e8,
  _b80d517d733633a6,
  _9d28fa751d24ce2e,
  _e79590278b446432,
  _1ad42f1c78dbe014,
  _38587400d9c44cb5,
  _03c325899b0e33f0,
  _becce0ac49342bb2,
  _5df17b0a512de878,
  _a3ffdb214a9c82a0,
  _c644fa2b15360347,
  _7ac8df441c1485cf
};
