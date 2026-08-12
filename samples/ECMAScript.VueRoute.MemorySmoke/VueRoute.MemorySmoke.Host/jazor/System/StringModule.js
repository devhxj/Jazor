import { _e1d2ba750a2788cb } from "System/Collections/Generic/HashSetT1Module.js";
import { _189761f781df8770 as i$70ad112aa124fe1f, _5ad63706a889c294 as i$8578349aab59a79b } from "System/StringModule.js";
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
export function _9d940114ace1198f(strA, strB, comparisonType) {
  return _20874c0b43640318(strA, strB, isOrdinalIgnoreCase(comparisonType));
}
export function _d78fb9d76fca75e4(strA, indexA, strB, indexB, length, comparisonType) {
  let sliceA = sliceOrEmpty(strA, indexA, length);
  let sliceB = sliceOrEmpty(strB, indexB, length);
  return _20874c0b43640318(sliceA, sliceB, isOrdinalIgnoreCase(comparisonType));
}
export function _a55d307de6e31c7b(strA, strB) {
  return _e16eea9fe3891a62(strA, strB);
}
export function _629b0613344d82e7(instance, value) {
  if (instance === null)
    throw new Error("NullReferenceException: instance is null.");
  if (value === null)
    return 1;
  if (typeof value !== "string")
    throw new Error("ArgumentException: Object must be of type String.");
  return _380e7c7649d703f0(instance, value);
}
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
export function _946b7129a48c8114(instance, value, comparisonType) {
  return isOrdinalIgnoreCase(comparisonType) ? instance.toLowerCase().endsWith(value.toLowerCase()) : instance.endsWith(value);
}
export function _f8e1e01e8c17e8bb(instance, value, comparisonType) {
  let __cacc$905e4cd31567fbdea2cfc432, __cacc$9f97d082d9091d20e016d2e0;
  return isOrdinalIgnoreCase(comparisonType) ? (__cacc$905e4cd31567fbdea2cfc432 = instance, __cacc$905e4cd31567fbdea2cfc432 == null ? undefined : __cacc$905e4cd31567fbdea2cfc432.toLowerCase()) === (__cacc$9f97d082d9091d20e016d2e0 = value, __cacc$9f97d082d9091d20e016d2e0 == null ? undefined : __cacc$9f97d082d9091d20e016d2e0.toLowerCase()) : instance === value;
}
export function _b7c36408f0f172e9(a, b, comparisonType) {
  let __cacc$2affe46bf2fb91a17a4d7645, __cacc$7cb1558b7c38adebb7f6a027;
  return isOrdinalIgnoreCase(comparisonType) ? (__cacc$2affe46bf2fb91a17a4d7645 = a, __cacc$2affe46bf2fb91a17a4d7645 == null ? undefined : __cacc$2affe46bf2fb91a17a4d7645.toLowerCase()) === (__cacc$7cb1558b7c38adebb7f6a027 = b, __cacc$7cb1558b7c38adebb7f6a027 == null ? undefined : __cacc$7cb1558b7c38adebb7f6a027.toLowerCase()) : a === b;
}
function isOrdinalIgnoreCase(comparisonType) {
  let value;
  return typeof comparisonType === "number" && (value = comparisonType, true) && value === 5;
}
function sliceOrEmpty(value, start, length) {
  if (!value)
    return value ?? "";
  if (start >= value.length || length <= 0)
    return "";
  let available = value.length - start;
  let take = length < available ? length : available;
  return value.substring(start, start + take);
}
export function _0333a0fd5f67d8a0(instance, value, comparisonType) {
  return isOrdinalIgnoreCase(comparisonType) ? instance.toLowerCase().startsWith(value.toLowerCase()) : instance.startsWith(value);
}
export function _5ad63706a889c294(instance, index) {
  if (index < 0 || index >= instance.length)
    throw new Error("IndexOutOfRangeException: index is out of range.");
  return instance.charAt(index);
}
export function _980dff69bc3b8afa(format, arg0) {
  let __cacc$6c5a1df247e0d35c25d250d5;
  if (format === null)
    throw new Error("ArgumentNullException: Format string cannot be null.");
  return format.replaceAll("{0}", (__cacc$6c5a1df247e0d35c25d250d5 = arg0, __cacc$6c5a1df247e0d35c25d250d5 == null ? undefined : __cacc$6c5a1df247e0d35c25d250d5.toString()) ?? "");
}
export function _8606f3cc36d1f8ed(format, arg0, arg1) {
  let __cacc$8caa225053705ff503d089e2, __cacc$2cc0360e81fab0113acfbf9a;
  if (format === null)
    throw new Error("ArgumentNullException: Format string cannot be null.");
  return format.replaceAll("{0}", (__cacc$8caa225053705ff503d089e2 = arg0, __cacc$8caa225053705ff503d089e2 == null ? undefined : __cacc$8caa225053705ff503d089e2.toString()) ?? "").replaceAll("{1}", (__cacc$2cc0360e81fab0113acfbf9a = arg1, __cacc$2cc0360e81fab0113acfbf9a == null ? undefined : __cacc$2cc0360e81fab0113acfbf9a.toString()) ?? "");
}
export function _cda0978188193522(format, arg0, arg1, arg2) {
  let __cacc$e9f4b2492705d734a3261463, __cacc$c232816ecae85a31afe1e7b5, __cacc$49e379a67ceb08e9823192e6;
  if (format === null)
    throw new Error("ArgumentNullException: Format string cannot be null.");
  return format.replaceAll("{0}", (__cacc$e9f4b2492705d734a3261463 = arg0, __cacc$e9f4b2492705d734a3261463 == null ? undefined : __cacc$e9f4b2492705d734a3261463.toString()) ?? "").replaceAll("{1}", (__cacc$c232816ecae85a31afe1e7b5 = arg1, __cacc$c232816ecae85a31afe1e7b5 == null ? undefined : __cacc$c232816ecae85a31afe1e7b5.toString()) ?? "").replaceAll("{2}", (__cacc$49e379a67ceb08e9823192e6 = arg2, __cacc$49e379a67ceb08e9823192e6 == null ? undefined : __cacc$49e379a67ceb08e9823192e6.toString()) ?? "");
}
export function _99b8bed2ce27774c(format, args) {
  if (format === null)
    throw new Error("ArgumentNullException: Format string cannot be null.");
  let result = format;
  for (let i = 0; i < args.length; i++) {
    let __cacc$dbef86d1aa0d48e246c56d24;
    result = result.replaceAll("{" + i + "}", (__cacc$dbef86d1aa0d48e246c56d24 = args[i], __cacc$dbef86d1aa0d48e246c56d24 == null ? undefined : __cacc$dbef86d1aa0d48e246c56d24.toString()) ?? "");
  }
  return result;
}
export function _8a7510653022a974(instance, oldValue, newValue, comparisonType) {
  return isOrdinalIgnoreCase(comparisonType) ? replaceAllIgnoreCase(instance, oldValue, newValue ?? "") : instance.replaceAll(oldValue, newValue ?? "");
}
export function _7d7cb13bbbbb83c8(instance, oldChar, newChar) {
  return instance.replaceAll(oldChar.toString(), newChar.toString());
}
export function _d8080c573d45b4b4(instance, separator, options) {
  return applySplitOptions(i$70ad112aa124fe1f(instance, separator.toString(), 0), options);
}
export function _aaa73a4811837ec7(instance, separator, count, options) {
  let splitOptions;
  if (count <= 0)
    return [];
  if (count === 1)
    return applySplitOptions([instance], options);
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
export function _62c8810ea13dba45(instance, separator) {
  let singleSeparator, separators;
  if (separator === null)
    return instance.split(RegExp("\\s+"), null);
  if (typeof separator === "string" && (singleSeparator = separator, true))
    return instance.split(RegExp(buildSplitCharClassPattern(singleSeparator)), null);
  if (Array.isArray(separator) && (separators = separator, true))
    return instance.split(RegExp(BuildSplitCharClassPattern(separators)), null);
  return instance.split(RegExp("\\s+"), null);
}
function buildSplitCharClassPattern(separator) {
  if (separator.length === 0)
    return "\\s+";
  let pattern = "[";
  for (let i = 0; i < separator.length; i++)
    pattern += escapeRegexCharClassChar(separator.substring(i, i + 1));
  return pattern + "]";
}
function BuildSplitCharClassPattern(separators) {
  if (separators.length === 0)
    return "\\s+";
  let pattern = "[";
  let hasSeparator = false;
  for (let i = 0; i < separators.length; i++) {
    let separator = separators[i];
    if (separator === null || separator.length === 0)
      continue;
    hasSeparator = true;
    for (let j = 0; j < separator.length; j++)
      pattern += escapeRegexCharClassChar(separator.substring(j, j + 1));
  }
  return hasSeparator ? pattern + "]" : "\\s+";
}
function escapeRegexCharClassChar(ch) {
  return (() => {
    const __swexpr$da15a416a4b563fe08c14254 = ch;
    if (__swexpr$da15a416a4b563fe08c14254 === "\\")
      return "\\\\";
    if (__swexpr$da15a416a4b563fe08c14254 === "]")
      return "\\]";
    if (__swexpr$da15a416a4b563fe08c14254 === "^")
      return "\\^";
    if (__swexpr$da15a416a4b563fe08c14254 === "-")
      return "\\-";
    return ch;
  })();
}
function applySplitOptions(parts, options) {
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
function splitByCharSetWithLimitAndOptions(instance, separator, count, options) {
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
    return applySplitOptions([instance], options);
  let any = normalizeCharSet(separator);
  let result = new Array;
  let start = 0;
  for (let i = 0; i < instance.length && result.length < count - 1; i++) {
    if (!any.has(i$8578349aab59a79b(instance, i)))
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
function splitByStringsWithLimitAndOptions(instance, separator, count, options) {
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
    return applySplitOptions([instance], options);
  let separators = normalizeStringSeparators(separator);
  let result = new Array;
  let start = 0;
  while (result.length < count - 1) {
    let bestIndex = -1;
    let bestSeparator = null;
    for (let i = 0; i < separators.length; i++) {
      let item = separators[i];
      if (item === null)
        continue;
      let index = instance.indexOf(item, start);
      if (index < 0)
        continue;
      if (bestIndex < 0 || index < bestIndex) {
        bestIndex = index;
        bestSeparator = item;
      }
    }
    if (bestIndex < 0 || bestSeparator === null)
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
function replaceAllIgnoreCase(instance, oldValue, newValue) {
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
function normalizeStringSeparators(separator) {
  let result = new Array;
  (() => {
    let single, many;
    const __swpat$b0aa07bf10fc324d6d779b23 = separator;
    if (typeof __swpat$b0aa07bf10fc324d6d779b23 === "string" && (single = __swpat$b0aa07bf10fc324d6d779b23, true) && single.length !== 0) {
      result.push(single);
      return;
    }
    if (Array.isArray(__swpat$b0aa07bf10fc324d6d779b23) && (many = __swpat$b0aa07bf10fc324d6d779b23, true)) {
      for (let i = 0; i < many.length; i++) {
        let item = many[i];
        if (!(item === null) && item.length !== 0)
          result.push(item);
      }
      return;
    }
  })();
  return result;
}
export function _5417a93b3075813a(instance, separator) {
  return _62c8810ea13dba45(instance, separator);
}
export function _d03d120228c0c4ed(instance, separator, count) {
  return splitByCharSetWithLimitAndOptions(instance, separator, count, 0);
}
export function _25c1f15b0ed2cb6e(instance, separator, options) {
  return applySplitOptions(_62c8810ea13dba45(instance, separator), options);
}
export function _c8e5ceed33c6c638(instance, separator, count, options) {
  return splitByCharSetWithLimitAndOptions(instance, separator, count, options);
}
export function _189761f781df8770(instance, separator, options) {
  return applySplitOptions(i$70ad112aa124fe1f(instance, separator, 0), options);
}
export function _96eb0a23afa7fdfb(instance, separator, count, options) {
  let splitOptions;
  if (count <= 0)
    return [];
  if (count === 1)
    return applySplitOptions([instance], options);
  if (!separator)
    return applySplitOptions(i$70ad112aa124fe1f(instance, separator, 0), options);
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
export function _fff99c96206a241e(instance, separator, options) {
  return splitByStringsWithLimitAndOptions(instance, separator, instance.length + 1, options);
}
export function _f3c7edcc7cc89a4a(instance, separator, count, options) {
  return splitByStringsWithLimitAndOptions(instance, separator, count, options);
}
export function _5d7e005b9dcb67de(instance, trimChar) {
  let token = trimChar.toString() ?? "";
  if (token.length === 0)
    return instance;
  let start = 0;
  let end = instance.length - 1;
  while (start <= end && i$8578349aab59a79b(instance, start) === token)
    start++;
  while (end >= start && i$8578349aab59a79b(instance, end) === token)
    end--;
  return start > end ? "" : instance.substring(start, start + (end - start + 1));
}
export function _c6c444b4e71e14f7(instance, trimChars) {
  let any = normalizeCharSet(trimChars);
  let start = 0;
  let end = instance.length - 1;
  while (start <= end && any.has(i$8578349aab59a79b(instance, start)))
    start++;
  while (end >= start && any.has(i$8578349aab59a79b(instance, end)))
    end--;
  return start > end ? "" : instance.substring(start, start + (end - start + 1));
}
export function _561fe737e62cf332(instance, trimChar) {
  let token = trimChar.toString() ?? "";
  if (token.length === 0)
    return instance;
  let start = 0;
  while (start < instance.length && i$8578349aab59a79b(instance, start) === token)
    start++;
  return start === 0 ? instance : instance.substring(start);
}
export function _98731360726c6976(instance, trimChars) {
  let any = normalizeCharSet(trimChars);
  let start = 0;
  while (start < instance.length && any.has(i$8578349aab59a79b(instance, start)))
    start++;
  return start === 0 ? instance : instance.substring(start);
}
export function _eb362a090d734099(instance, trimChar) {
  let token = trimChar.toString() ?? "";
  if (token.length === 0)
    return instance;
  let end = instance.length - 1;
  while (end >= 0 && i$8578349aab59a79b(instance, end) === token)
    end--;
  return end === instance.length - 1 ? instance : end < 0 ? "" : instance.substring(0, 0 + (end + 1));
}
export function _a62862c1fbaa21c3(instance, trimChars) {
  let any = normalizeCharSet(trimChars);
  let end = instance.length - 1;
  while (end >= 0 && any.has(i$8578349aab59a79b(instance, end)))
    end--;
  return end === instance.length - 1 ? instance : end < 0 ? "" : instance.substring(0, 0 + (end + 1));
}
export function _d52d7114d5c1b839(instance, value, comparisonType) {
  return isOrdinalIgnoreCase(comparisonType) ? instance.toLowerCase().includes(value.toLowerCase()) : instance.includes(value);
}
export function _16d4b2b4de019fb2(instance, value, comparisonType) {
  let token = value.toString();
  return isOrdinalIgnoreCase(comparisonType) ? instance.toLowerCase().includes(token.toLowerCase()) : instance.includes(token);
}
export function _5331447e2c855a66(instance, value, comparisonType) {
  let token = value.toString();
  return isOrdinalIgnoreCase(comparisonType) ? instance.toLowerCase().indexOf(token.toLowerCase()) : instance.indexOf(token);
}
export function _d2873e605fbed764(instance, value, startIndex, count) {
  let target = value.toString();
  let end = startIndex + count;
  for (let i = startIndex; i < end && i < instance.length; i++) {
    if (i$8578349aab59a79b(instance, i) === target)
      return i;
  }
  return -1;
}
export function _69b749a1c6cbae78(instance, anyOf) {
  let any = normalizeCharSet(anyOf);
  for (let i = 0; i < instance.length; i++) {
    let current = i$8578349aab59a79b(instance, i);
    if (any.has(current))
      return i;
  }
  return -1;
}
export function _63633a5f3b85c5a9(instance, anyOf, startIndex) {
  let any = normalizeCharSet(anyOf);
  for (let i = startIndex; i < instance.length; i++) {
    let current = i$8578349aab59a79b(instance, i);
    if (any.has(current))
      return i;
  }
  return -1;
}
export function _cb863079aae72451(instance, anyOf, startIndex, count) {
  let any = normalizeCharSet(anyOf);
  let end = startIndex + count;
  for (let i = startIndex; i < end && i < instance.length; i++) {
    let current = i$8578349aab59a79b(instance, i);
    if (any.has(current))
      return i;
  }
  return -1;
}
export function _ff549d811898fb56(instance, value, startIndex, count) {
  let end = startIndex + count - value.length;
  for (let i = startIndex; i <= end && i + value.length <= instance.length; i++) {
    if (instance.substring(i, i + value.length) === value)
      return i;
  }
  return -1;
}
export function _3ae4900da2b07b27(instance, value, comparisonType) {
  return isOrdinalIgnoreCase(comparisonType) ? instance.toLowerCase().indexOf(value.toLowerCase()) : instance.indexOf(value);
}
export function _2fabe2b831abe71e(instance, value, startIndex, comparisonType) {
  return isOrdinalIgnoreCase(comparisonType) ? instance.toLowerCase().indexOf(value.toLowerCase(), startIndex) : instance.indexOf(value, startIndex);
}
export function _ab22561fc42166db(instance, value, startIndex, count, comparisonType) {
  return isOrdinalIgnoreCase(comparisonType) ? _ff549d811898fb56(instance.toLowerCase(), value.toLowerCase(), startIndex, count) : _ff549d811898fb56(instance, value, startIndex, count);
}
export function _dbdd57f8d259ce66(instance, value, startIndex, count) {
  let target = value.toString();
  let end = startIndex >= instance.length ? Number(instance.length - 1) : startIndex;
  let begin = end - count + 1;
  if (begin < 0)
    begin = 0;
  for (let i = end; i >= begin; i--) {
    if (i$8578349aab59a79b(instance, i) === target)
      return i;
  }
  return -1;
}
export function _c0212f4213a99019(instance, anyOf) {
  let any = normalizeCharSet(anyOf);
  for (let i = instance.length - 1; i >= 0; i--) {
    let current = i$8578349aab59a79b(instance, i);
    if (any.has(current))
      return i;
  }
  return -1;
}
function normalizeCharSet(anyOf) {
  let set = new Set;
  (() => {
    let single, many;
    const __swpat$7d7ef8eca298015bad704949 = anyOf;
    if (typeof __swpat$7d7ef8eca298015bad704949 === "string" && (single = __swpat$7d7ef8eca298015bad704949, true)) {
      for (let i = 0; i < single.length; i++)
        _e1d2ba750a2788cb(set, i$8578349aab59a79b(single, i));
      return;
    }
    if (Array.isArray(__swpat$7d7ef8eca298015bad704949) && (many = __swpat$7d7ef8eca298015bad704949, true)) {
      for (let i = 0; i < many.length; i++) {
        let item = many[i];
        if (!item)
          continue;
        for (let j = 0; j < item.length; j++)
          _e1d2ba750a2788cb(set, i$8578349aab59a79b(item, j));
      }
      return;
    }
  })();
  return set;
}
export function _c401e64318e768c4(instance, anyOf, startIndex) {
  let any = normalizeCharSet(anyOf);
  let index = startIndex >= instance.length ? Number(instance.length - 1) : startIndex;
  for (let i = index; i >= 0; i--) {
    let current = i$8578349aab59a79b(instance, i);
    if (any.has(current))
      return i;
  }
  return -1;
}
export function _3c17fcef5615e7a3(instance, anyOf, startIndex, count) {
  let any = normalizeCharSet(anyOf);
  let end = startIndex >= instance.length ? Number(instance.length - 1) : startIndex;
  let begin = end - count + 1;
  if (begin < 0)
    begin = 0;
  for (let i = end; i >= begin; i--) {
    let current = i$8578349aab59a79b(instance, i);
    if (any.has(current))
      return i;
  }
  return -1;
}
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
export function _78449c135e18c4bc(instance, value, comparisonType) {
  return isOrdinalIgnoreCase(comparisonType) ? instance.toLowerCase().lastIndexOf(value.toLowerCase()) : instance.lastIndexOf(value);
}
export function _359dbce44ce4a4da(instance, value, startIndex, comparisonType) {
  return isOrdinalIgnoreCase(comparisonType) ? instance.toLowerCase().lastIndexOf(value.toLowerCase(), startIndex) : instance.lastIndexOf(value, startIndex);
}
export function _c911a06f021bd138(instance, value, startIndex, count, comparisonType) {
  return isOrdinalIgnoreCase(comparisonType) ? _c4ee024d06ee238c(instance.toLowerCase(), value.toLowerCase(), startIndex, count) : _c4ee024d06ee238c(instance, value, startIndex, count);
}
