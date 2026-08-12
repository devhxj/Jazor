import { _5ad63706a889c294 } from "System/StringModule.js";
export function _c1e724fa6dbf63eb(value) {
  if (value.length === 0)
    return 0n;
  let buffer = new ArrayBuffer(value.length);
  let array = new Uint8Array(buffer);
  let view = new DataView(array.buffer, array.byteOffset, array.byteLength);
  let result = 0n;
  let i = 0;
  for (; i + 8 <= value.length; i += 8)
    result = result << BigInt(64) | view.getBigUint64(i, false);
  if (i < value.length) {
    let remaining = 0n;
    for (; i < value.length; i++)
      remaining = remaining << BigInt(8) | BigInt(value[i]);
    result = result << BigInt((value.length - i) * 8) | remaining;
  }
  return result;
}
export function _9c321a7400e5ff9b(value, isUnsigned, isBigEndian) {
  if (value.length === 0)
    return 0n;
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
    let view = new DataView(buffer);
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
      let offset = 1n << bitWidth;
      result -= offset;
    }
    return result;
  }
  function BuildBigIntFromLEBytes(littleEndianBytes) {
    let result = 0n;
    for (let i = littleEndianBytes.length - 1; i >= 0; i--) {
      result = result << BigInt(8) | BigInt(littleEndianBytes[i] & 255);
    }
    return result;
  }
}
export function _734290a188c5bc5a(instance) {
  if (instance > 0n)
    return 1;
  if (instance < 0n)
    return -1;
  return 0;
}
function getBitLengthCore(instance) {
  if (instance === 0n)
    return 0n;
  let isNegative = instance < 0n;
  let value = isNegative ? -instance - 1n : instance;
  let bitLength = 0n;
  while (value > 0n) {
    bitLength += 1n;
    value >>= 1n;
  }
  return bitLength;
}
function computePositiveLog(value, baseValue) {
  if (value <= BigInt(Number.MAX_SAFE_INTEGER))
    return Math.log(Number(value)) / Math.log(baseValue);
  let bitLength = Number(getBitLengthCore(value));
  let shift = bitLength - 64;
  let x = shift > 0 ? value >> BigInt(shift) : value << BigInt(-shift);
  return Math.log(Number(x)) / Math.log(baseValue) + shift / (Math.log(baseValue) / Math.log(2));
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
    return [false, 0n];
  try {
    let trimmed = value.trim();
    if (trimmed.length === 0)
      return [false, 0n];
    return [true, BigInt(trimmed)];
  } catch {
    return [false, 0n];
  }
}
export function _598611fb2b8a064a(dividend, divisor, remainder) {
  if (divisor === 0n)
    throw new RangeError("Division by zero");
  let quotient = dividend / divisor;
  let rem = dividend % divisor;
  return [quotient, rem];
}
export function _fb5a811e7a32a324(value) {
  return _acb5aef300c8db0c(value, Math.E);
}
export function _acb5aef300c8db0c(value, baseValue) {
  if (value < 0n || baseValue === 1)
    return Number.NaN;
  if (baseValue === Number.POSITIVE_INFINITY)
    return value === 1n ? 0 : Number.NaN;
  if (baseValue === 0 && value !== 1n)
    return Number.NaN;
  if (value === 0n)
    return Math.log(0) / Math.log(baseValue);
  return computePositiveLog(value, baseValue);
}
export function _f276cbd7c3b305ea(value) {
  return _acb5aef300c8db0c(value, 10);
}
export function _7555649a5efc7b79(left, right) {
  let a = left < 0n ? -left : left;
  let b = right < 0n ? -right : right;
  if (a === 0n)
    return b;
  if (b === 0n)
    return a;
  while (b !== 0n) {
    let temp = b;
    b = a % b;
    a = temp;
  }
  return a;
}
export function _ec6961a106ca5bf3(value, exponent, modulus) {
  if (exponent < 0n)
    throw new Error("ArgumentOutOfRangeException: Exponent must be non-negative.");
  if (modulus === 0n)
    throw new Error("DivideByZeroException");
  let modulusMagnitude = modulus < 0n ? -modulus : modulus;
  if (modulusMagnitude === 1n)
    return 0n;
  let result = 1n;
  let negativeResult = value < 0n && (exponent & 1n) === 1n;
  let val = (value < 0n ? -value : value) % modulusMagnitude;
  let exp = exponent;
  while (exp > 0n) {
    if ((exp & 1n) === 1n)
      result = result * val % modulusMagnitude;
    exp >>= 1n;
    val = val * val % modulusMagnitude;
  }
  return negativeResult && result !== 0n ? -result : result;
}
export function _31cf4d89164dee40(value, exponent) {
  if (exponent < 0 || !Number.isInteger(exponent))
    throw new RangeError("The exponent must be a non-negative integer");
  let result = 1n;
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
  if (instance === 0n)
    return [0];
  let isNegative = instance < 0n;
  let value = isNegative ? -instance : instance;
  let bytes = new Array;
  while (value > 0n) {
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
  if (instance === 0n)
    return [0];
  let isNegative = !isUnsigned && instance < 0n;
  let value = isNegative ? -instance - 1n : instance;
  let bytes = new Array;
  let bitLength = 0;
  let temp = value;
  while (temp > 0n) {
    bitLength++;
    temp >>= 1n;
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
  if (instance !== 0n) {
    let isNegative = !isUnsigned && instance < 0n;
    let value = isNegative ? isUnsigned ? instance : -instance - 1n : instance;
    let bitLength = 0;
    while (value > 0n) {
      bitLength++;
      value >>= 1n;
    }
    requiredBytes = isUnsigned ? Math.max(1, Math.ceil(bitLength / 8)) : Math.max(1, Math.ceil((bitLength + 1) / 8));
  }
  if (destination.length < requiredBytes)
    return [false, 0];
  let bytes = new Array;
  if (instance === 0n)
    bytes.push(0);
  else {
    let isNegative = !isUnsigned && instance < 0n;
    let value = isNegative ? -instance - 1n : instance;
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
  let fillByte = !isUnsigned && instance < 0n ? 255 : 0;
  for (let i = bytes.length; i < destination.length; i++)
    destination[i] = fillByte;
  return [true, bytes.length];
}
export function _c1393b267008395c(instance, isUnsigned) {
  if (instance === 0n)
    return 1;
  let isNegative = !isUnsigned && instance < 0n;
  let value = isNegative ? -instance : instance;
  let bitLength = 0;
  while (value > 0n) {
    bitLength++;
    value >>= 1n;
  }
  if (isUnsigned)
    return Math.max(1, Math.ceil(bitLength / 8));
  else
    return isNegative ? Math.max(1, Math.ceil((bitLength + 1) / 8)) : Math.max(1, Math.ceil(bitLength / 8));
}
export function _fe4c3211e57446e7(instance, provider) {
  if (provider === null)
    return instance.toString();
  let isNegative = instance < 0n;
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
  return getBitLengthCore(instance);
}
export function _22a21ffe19479f32(left, right) {
  if (right === 0n)
    throw new RangeError("Division by zero");
  let quotient = left / right;
  let remainder = left % right;
  return [quotient, remainder];
}
export function _276680abacb93277(value) {
  if (value === 0n)
    return BigInt(32);
  if (value < 0n)
    return 0n;
  let remainder = getBitLengthCore(value) % BigInt(32);
  if (remainder === 0n)
    return 0n;
  return BigInt(32) - remainder;
}
export function _5e476c376aca56ae(value) {
  if (value === 0n)
    return 0n;
  let count = 0n;
  let n = value < 0n ? -value - 1n : value;
  while (n > 0n) {
    n &= n - 1n;
    count += 1n;
  }
  return count;
}
export function _ae7b1dd18af32f04(value, rotateAmount) {
  if (value === 0n)
    return 0n;
  let bitLength = Number(_41fe76dfb4ee2ab2(value));
  let ra = rotateAmount % bitLength;
  if (ra < 0)
    ra += bitLength;
  if (ra === 0)
    return value;
  let mask = (1n << BigInt(ra)) - 1n;
  let rotatedOutBits = value >> BigInt(bitLength - ra) & mask;
  let result = (value << BigInt(ra) | rotatedOutBits) & (1n << BigInt(bitLength)) - 1n;
  return result;
}
export function _dc8cc860511e78b3(value, rotateAmount) {
  if (rotateAmount === 0)
    return value;
  if (value === 0n)
    return 0n;
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
  let leftPart = value & (1n << BigInt(ra)) - 1n;
  let rotated = leftPart << BigInt(bitLength - ra) | rightPart;
  return rotated;
}
export function _696502aae4b6e182(value) {
  if (value === 0n)
    return 0n;
  let count = 0n;
  let temp = value;
  while ((temp & 1n) === 0n) {
    count++;
    temp >>= 1n;
  }
  return count;
}
export function _c0651d019a4b12b1(value) {
  if (value <= 0n)
    return false;
  let minusOne = value - 1n;
  let result = (value & minusOne) === 0n;
  return result;
}
export function _c29a05a989ec3b33(value) {
  if (value <= 0n)
    throw new RangeError("value must be positive");
  let result = 0n;
  let temp = value;
  while (temp > 1n) {
    result++;
    temp >>= 1n;
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
    return bl ? 1n : 0n;
  if (value === null)
    throw new RangeError("Value cannot be null or undefined");
  throw new RangeError("Unsupported type for conversion to BigInt");
}
export function _d305de2c64e85995(x, y) {
  let absX = x < 0n ? -x : x;
  let absY = y < 0n ? -y : y;
  if (absX > absY)
    return x;
  if (absX < absY)
    return y;
  return x > y ? x : y;
}
export function _fef56ccd17b22e88(x, y) {
  let absX = x < 0n ? -x : x;
  let absY = y < 0n ? -y : y;
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
  if (value >= 0n)
    return value >> shift;
  throw new Error("Unsigned right shift (>>>) is not supported for BigInt in JavaScript");
}
