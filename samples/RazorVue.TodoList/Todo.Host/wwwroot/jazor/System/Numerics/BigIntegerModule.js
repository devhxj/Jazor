import { _5ad63706a889c294 } from "System/StringModule.js";
export function _c1e724fa6dbf63eb(value) {
  if (value.length === 0)
    return BigInt.zero;
  let buffer = new ArrayBuffer(value.length);
  let array = new Uint8Array(buffer, null, null);
  let view = new DataView(array.buffer, array.byteOffset, array.byteLength);
  let result = BigInt.zero;
  let i = 0;
  for (; i + 8 <= value.length; i += 8)
    result = result << BigInt(64) | view.getBigUint64(i, false);
  if (i < value.length) {
    let remaining = BigInt.zero;
    for (; i < value.length; i++)
      remaining = remaining << BigInt(8) | BigInt(value[i]);
    result = result << BigInt((value.length - i) * 8) | remaining;
  }
  return result;
}
export function _9c321a7400e5ff9b(value, isUnsigned, isBigEndian) {
  if (value.length === 0)
    return BigInt.zero;
  if (value.length === 1) {
    if (isUnsigned) {
      return BigInt(value[0]);
    }
    else {
      return (value[0] & 128) === 0 ? BigInt(value[0]) : BigInt(value[0]) - BigInt(256);
    }
  }
  if (value.length <= 8) {
    let buffer = new ArrayBuffer(value.length);
    let view = new DataView(buffer, null, null);
    value.forEach((item, index) => {
      view.setUint8(index, item);
      return;
    });
    if (value.length === 2)
      return isUnsigned ? BigInt(view.getUint16(0, !isBigEndian)) : BigInt(view.getInt16(0, !isBigEndian));
    if (value.length === 4)
      return isUnsigned ? BigInt(view.getUint32(0, !isBigEndian)) : BigInt(view.getInt32(0, !isBigEndian));
    if (value.length === 8)
      return isUnsigned ? view.getBigUint64(0, !isBigEndian) : view.getBigInt64(0, !isBigEndian);
    return ProcessNonStandardLength(value, isUnsigned, isBigEndian);
  }
  return ProcessNonStandardLength(value, isUnsigned, isBigEndian);
  function ProcessNonStandardLength(bytes, isUnsigned, isBigEndian) {
    let processedBytes = bytes.slice(0);
    if (isBigEndian) {
      processedBytes.reverse();
    }
    let result = BuildBigIntFromLEBytes(processedBytes);
    if (!isUnsigned && (processedBytes[processedBytes.length - 1] & 128) !== 0) {
      let bitWidth = BigInt(processedBytes.length) * BigInt(8);
      let offset = BigInt.one << bitWidth;
      result -= offset;
    }
    return result;
  }
  function BuildBigIntFromLEBytes(littleEndianBytes) {
    let result = BigInt.zero;
    for (let i = littleEndianBytes.length - 1; i >= 0; i--) {
      result = result << BigInt(8) | BigInt(littleEndianBytes[i] & 255);
    }
    return result;
  }
}
export function _734290a188c5bc5a(instance) {
  if (instance > BigInt.zero)
    return 1;
  if (instance < BigInt.zero)
    return -1;
  return 0;
}
export function _155212572c9a3297(value) {
  if (value === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  let trimmed = value.trim();
  if (trimmed.length === 0)
    throw new Error("FormatException: The input string was not in a correct format.");
  try {
    return BigInt(trimmed);
  } catch {
    throw new Error(`FormatException: The input string '${value}' was not in a correct format.`);
  }
}
export function _59acea2facdaa757(value, result) {
  if (value === null || value.length === 0)
    return [false, BigInt.zero];
  try {
    let trimmed = value.trim();
    if (trimmed.length === 0)
      return [false, BigInt.zero];
    return [true, BigInt(trimmed)];
  } catch {
    return [false, BigInt.zero];
  }
}
export function _598611fb2b8a064a(dividend, divisor, remainder) {
  if (divisor === BigInt.zero)
    throw new RangeError("Division by zero");
  let quotient = dividend / divisor;
  let rem = dividend % divisor;
  return [quotient, rem];
}
export function _fb5a811e7a32a324(value) {
  if (value <= BigInt.zero)
    throw new Error("Logarithm is undefined for non-positive numbers");
  let str = value.toString();
  let exponent = str.length - 1;
  let mantissa = Number(str.substring(0, 0 + 15));
  return Math.log(mantissa) + exponent * Math.log(10);
}
export function _acb5aef300c8db0c(value, baseValue) {
  if (value <= BigInt.zero)
    throw new RangeError("Logarithm is undefined for non-positive numbers");
  if (baseValue <= 0 || baseValue === 1)
    throw new RangeError("Base must be positive and not equal to 1");
  if (value === BigInt.one)
    return 0;
  if (baseValue === Math.E)
    return Math.log(Number(value));
  if (value <= Number.MAX_SAFE_INTEGER)
    return Math.log(Number(value)) / Math.log(baseValue);
  let str = value.toString();
  let digitCount = str.length;
  let significantDigits = str.substring(0, 0 + 15);
  let mantissa = parseFloat(significantDigits) / Math.pow(10, significantDigits.length - 1);
  let exponent = digitCount - 1;
  let lnValue = Math.log(mantissa) + exponent * Math.LN10;
  let lnBase = Math.log(baseValue);
  return lnValue / lnBase;
}
export function _f276cbd7c3b305ea(value) {
  if (value <= BigInt.zero)
    throw new RangeError("Logarithm is undefined for non-positive numbers");
  if (value === BigInt.one)
    return 0;
  let str = value.toString();
  return str.length <= 15 ? Math.log10(Number(value)) : Math.log10(Number(str.substring(0, 0 + 15))) + (str.length - 15);
}
export function _7555649a5efc7b79(left, right) {
  let a = left < BigInt.zero ? -left : left;
  let b = right < BigInt.zero ? -right : right;
  if (a === BigInt.zero)
    return b;
  if (b === BigInt.zero)
    return a;
  while (b !== BigInt.zero) {
    let temp = b;
    b = a % b;
    a = temp;
  }
  return a;
}
export function _ec6961a106ca5bf3(value, exponent, modulus) {
  if (modulus === BigInt.one)
    return BigInt.zero;
  let result = BigInt.one;
  let val = value % modulus;
  let exp = exponent;
  while (exp > BigInt.zero) {
    if (exp % BigInt.two === BigInt.one)
      result = result * val % modulus;
    exp >>= BigInt.one;
    val = val * val % modulus;
  }
  return result;
}
export function _31cf4d89164dee40(value, exponent) {
  if (exponent < 0 || !Number.isInteger(exponent))
    throw new RangeError("The exponent must be a non-negative integer");
  let result = BigInt.one;
  let current = value;
  let exp = exponent;
  while (exp > 0) {
    if (exp % 2 === 1)
      result *= current;
    current *= current;
    exp = Math.floor(exp / 2);
  }
  return result;
}
export function _9f7b3705890bed98(instance, obj) {
  let bigIntValue;
  if (obj === null)
    return 1;
  if (typeof obj === "bigint" && (bigIntValue = obj, true))
    return instance < bigIntValue ? -1 : instance > bigIntValue ? 1 : 0;
  throw new Error("ArgumentException: Object must be of type BigInteger.");
}
export function _ca46777d5c8cc9b9(instance) {
  if (instance === BigInt.zero)
    return [0];
  let isNegative = instance < BigInt.zero;
  let value = isNegative ? -instance : instance;
  let bytes = [];
  while (value > BigInt.zero) {
    bytes.unshift(Number(value & BigInt(255)));
    value >>= BigInt(8);
  }
  if (isNegative) {
    for (let i = 0; i < bytes.length; i++)
      bytes[i] = ~bytes[i] & 255;
    let carry = 1;
    for (let i = bytes.length - 1; i >= 0 && carry > 0; i--) {
      let sum = bytes[i] + carry;
      bytes[i] = sum & 255;
      carry = sum >> 8;
    }
    if ((bytes[0] & 128) === 0)
      bytes.unshift(255);
  }
  return bytes;
}
export function _11ed9d474ccf2419(instance, isUnsigned, isBigEndian) {
  if (instance === BigInt.zero)
    return [0];
  let isNegative = !isUnsigned && instance < BigInt.zero;
  let value = isNegative ? -instance - BigInt.one : instance;
  let bytes = [];
  let bitLength = 0;
  let temp = value;
  while (temp > BigInt.zero) {
    bitLength++;
    temp >>= BigInt.one;
  }
  let minLength = isNegative ? Math.ceil((bitLength + 1) / 8) : Math.ceil(bitLength / 8);
  let byteLength = Math.max(minLength, 1);
  for (let i = 0; i < byteLength; i++) {
    let b = Number(value & BigInt(255));
    if (isBigEndian)
      bytes.unshift(b);
    else
      bytes.push(b);
    value >>= BigInt(8);
  }
  if (isNegative) {
    for (let i = 0; i < bytes.length; i++)
      bytes[i] = ~bytes[i] & 255;
    if (isBigEndian && (bytes[0] & 128) === 0)
      bytes.unshift(255);
    else if (!isBigEndian && (bytes[bytes.length - 1] & 128) === 0)
      bytes.push(255);
  }
  return bytes;
}
export function _76ae4e496fc976fd(instance, destination, bytesWritten, isUnsigned, isBigEndian) {
  let requiredBytes = 1;
  if (instance !== BigInt.zero) {
    let isNegative = !isUnsigned && instance < BigInt.zero;
    let value = isNegative ? isUnsigned ? instance : -instance - BigInt.one : instance;
    let bitLength = 0;
    while (value > BigInt.zero) {
      bitLength++;
      value >>= BigInt.one;
    }
    requiredBytes = isUnsigned ? Math.max(1, Math.ceil(bitLength / 8)) : Math.max(1, Math.ceil((bitLength + 1) / 8));
  }
  if (destination.length < requiredBytes)
    return [false, 0];
  let bytes = [];
  if (instance === BigInt.zero)
    bytes.push(0);
  else {
    let isNegative = !isUnsigned && instance < BigInt.zero;
    let value = isNegative ? -instance - BigInt.one : instance;
    let byteCount = requiredBytes;
    while (byteCount-- > 0) {
      let b = Number(value & BigInt(255));
      if (isBigEndian)
        bytes.unshift(b);
      else
        bytes.push(b);
      value >>= BigInt(8);
    }
    if (isNegative) {
      for (let i = 0; i < bytes.length; i++)
        bytes[i] = ~bytes[i] & 255;
      if (isBigEndian && (bytes[0] & 128) === 0) {
        bytes.unshift(255);
        requiredBytes++;
      }
      else if (!isBigEndian && (bytes[bytes.length - 1] & 128) === 0) {
        bytes.push(255);
        requiredBytes++;
      }
    }
  }
  if (bytes.length > destination.length)
    return [false, 0];
  for (let i = 0; i < bytes.length; i++)
    destination[i] = bytes[i];
  let fillByte = !isUnsigned && instance < BigInt.zero ? 255 : 0;
  for (let i = bytes.length; i < destination.length; i++)
    destination[i] = fillByte;
  return [true, bytes.length];
}
export function _c1393b267008395c(instance, isUnsigned) {
  if (instance === BigInt.zero)
    return 1;
  let isNegative = !isUnsigned && instance < BigInt.zero;
  let value = isNegative ? -instance : instance;
  let bitLength = 0;
  while (value > BigInt.zero) {
    bitLength++;
    value >>= BigInt.one;
  }
  if (isUnsigned)
    return Math.max(1, Math.ceil(bitLength / 8));
  else
    return isNegative ? Math.max(1, Math.ceil((bitLength + 1) / 8)) : Math.max(1, Math.ceil(bitLength / 8));
}
export function _fe4c3211e57446e7(instance, provider) {
  if (provider === null)
    return instance.toString();
  let isNegative = instance < BigInt.zero;
  let absValue = isNegative ? -instance : instance;
  let strValue = absValue.toString();
  try {
    if (absValue <= BigInt(Number.MAX_SAFE_INTEGER)) {
      let formatted = provider.format(Number(absValue));
      return isNegative ? `-${formatted}` : formatted;
    }
    let sample = provider.format(1000.1);
    let groupChar = sample.includes("1,000") ? "," : sample.includes("1.000") ? "." : sample.includes("1 000") ? " " : ",";
    let result = "";
    let i = strValue.length;
    let groupCount = 0;
    while (i > 0) {
      if (groupCount > 0 && groupCount % 3 === 0) {
        result = groupChar + result;
      }
      result = _5ad63706a889c294(strValue, --i) + result;
      groupCount++;
    }
    return isNegative ? `-${result}` : result;
  } catch {
    return instance.toString();
  }
}
export function _41fe76dfb4ee2ab2(instance) {
  if (instance === BigInt.zero)
    return BigInt.zero;
  let isNegative = instance < BigInt.zero;
  let value = isNegative ? -instance - BigInt.one : instance;
  let bitLength = BigInt.zero;
  while (value > BigInt.zero) {
    bitLength += BigInt.one;
    value >>= BigInt.one;
  }
  return bitLength;
}
export function _22a21ffe19479f32(left, right) {
  if (right === BigInt.zero)
    throw new RangeError("Division by zero");
  let quotient = left / right;
  let remainder = left % right;
  return [quotient, remainder];
}
export function _276680abacb93277(value) {
  if (value === BigInt.zero)
    return BigInt.zero;
  return BigInt.zero;
}
export function _5e476c376aca56ae(value) {
  if (value === BigInt.zero)
    return BigInt.zero;
  let count = BigInt.zero;
  let n = value < BigInt.zero ? -value - BigInt.one : value;
  while (n > BigInt.zero) {
    n &= n - BigInt.one;
    count += BigInt.one;
  }
  return count;
}
export function _ae7b1dd18af32f04(value, rotateAmount) {
  if (value === BigInt.zero)
    return BigInt.zero;
  let bitLength = Number(_41fe76dfb4ee2ab2(value));
  let ra = rotateAmount % bitLength;
  if (ra < 0)
    ra += bitLength;
  if (ra === 0)
    return value;
  let mask = (BigInt.one << BigInt(ra)) - BigInt.one;
  let rotatedOutBits = value >> BigInt(bitLength - ra) & mask;
  let result = (value << BigInt(ra) | rotatedOutBits) & (BigInt.one << BigInt(bitLength)) - BigInt.one;
  return result;
}
export function _dc8cc860511e78b3(value, rotateAmount) {
  if (rotateAmount === 0)
    return value;
  if (value === BigInt.zero)
    return BigInt.zero;
  let bitLength = Number(_41fe76dfb4ee2ab2(value));
  if (rotateAmount < 0) {
    let absAmount = -rotateAmount;
    absAmount %= bitLength;
    if (absAmount === 0)
      return value;
    return value << BigInt(absAmount) | value >> BigInt(bitLength - absAmount);
  }
  let ra = rotateAmount % bitLength;
  if (ra === 0)
    return value;
  let rightPart = value >> BigInt(ra);
  let leftPart = value & (BigInt.one << BigInt(ra)) - BigInt.one;
  let rotated = leftPart << BigInt(bitLength - ra) | rightPart;
  return rotated;
}
export function _696502aae4b6e182(value) {
  if (value === BigInt.zero)
    return BigInt.zero;
  let count = BigInt.zero;
  let temp = value;
  while ((temp & BigInt.one) === BigInt.zero) {
    count++;
    temp >>= BigInt.one;
  }
  return count;
}
export function _c0651d019a4b12b1(value) {
  if (value <= BigInt.zero)
    return false;
  let minusOne = value - BigInt.one;
  let result = (value & minusOne) === BigInt.zero;
  return result;
}
export function _c29a05a989ec3b33(value) {
  if (value <= BigInt.zero)
    throw new RangeError("value must be positive");
  let result = BigInt.zero;
  let temp = value;
  while (temp > BigInt.one) {
    result++;
    temp >>= BigInt.one;
  }
  return result;
}
export function _8548cc83c4d947f5(value, min, max) {
  if (min > max)
    throw new RangeError("min must be less than or equal to max");
  let result = value;
  if (result < min)
    result = min;
  if (result > max)
    result = max;
  return result;
}
export function _8cbca5624f4a6cc0(value) {
  let b, n, s, bl;
  if (typeof value === "bigint" && (b = value, true))
    return b;
  if (typeof value === "number" && (n = value, true)) {
    if (!Number.isInteger(n))
      throw new RangeError("Value must be an integer");
    if (n < Number.MIN_SAFE_INTEGER || n > Number.MAX_SAFE_INTEGER)
      throw new RangeError("Value is outside safe integer range");
    return BigInt(n);
  }
  if (typeof value === "string" && (s = value, true)) {
    let trimmed = s.trim();
    if (!RegExp("^-?\\d+$").test(trimmed))
      throw new RangeError("String must represent a valid integer");
    try {
      return BigInt(trimmed);
    } catch {
      throw new RangeError("Invalid integer string");
    }
  }
  if (typeof value === "boolean" && (bl = value, true))
    return bl ? BigInt.one : BigInt.zero;
  if (value === null)
    throw new RangeError("Value cannot be null or undefined");
  throw new RangeError("Unsupported type for conversion to BigInt");
}
export function _d305de2c64e85995(x, y) {
  let absX = x < BigInt.zero ? -x : x;
  let absY = y < BigInt.zero ? -y : y;
  if (absX > absY)
    return x;
  if (absX < absY)
    return y;
  return x > y ? x : y;
}
export function _fef56ccd17b22e88(x, y) {
  let absX = x < BigInt.zero ? -x : x;
  let absY = y < BigInt.zero ? -y : y;
  if (absX < absY)
    return x;
  if (absX > absY)
    return y;
  return x < y ? x : y;
}
export function _49adf7adfc1228f8(value, shiftAmount) {
  if (shiftAmount < 0)
    throw new RangeError("Shift amount must be non-negative");
  let shift = BigInt(shiftAmount);
  if (value >= BigInt.zero)
    return value >> shift;
  throw new Error("Unsigned right shift (>>>) is not supported for BigInt in JavaScript");
}
export const BigIntegerModule = {
  _c1e724fa6dbf63eb,
  _9c321a7400e5ff9b,
  _734290a188c5bc5a,
  _155212572c9a3297,
  _59acea2facdaa757,
  _598611fb2b8a064a,
  _fb5a811e7a32a324,
  _acb5aef300c8db0c,
  _f276cbd7c3b305ea,
  _7555649a5efc7b79,
  _ec6961a106ca5bf3,
  _31cf4d89164dee40,
  _9f7b3705890bed98,
  _ca46777d5c8cc9b9,
  _11ed9d474ccf2419,
  _76ae4e496fc976fd,
  _c1393b267008395c,
  _fe4c3211e57446e7,
  _41fe76dfb4ee2ab2,
  _22a21ffe19479f32,
  _276680abacb93277,
  _5e476c376aca56ae,
  _ae7b1dd18af32f04,
  _dc8cc860511e78b3,
  _696502aae4b6e182,
  _c0651d019a4b12b1,
  _c29a05a989ec3b33,
  _8548cc83c4d947f5,
  _8cbca5624f4a6cc0,
  _d305de2c64e85995,
  _fef56ccd17b22e88,
  _49adf7adfc1228f8
};
