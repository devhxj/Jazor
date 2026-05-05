export function _a108636b79b7c8d2(instance, value) {
  let bigIntValue;
  if (value === null)
    return 1;
  if (typeof value === "bigint" && (bigIntValue = value, true))
    return instance < bigIntValue ? -1 : instance > bigIntValue ? 1 : 0;
  throw new Error("ArgumentException: Object must be of type Int64.");
}
export function _4174bb5b72e448a6(s) {
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  let trimmed = s.trim();
  try {
    let result = BigInt(trimmed);
    let minValue = BigInt("-9223372036854775808");
    let maxValue = BigInt("9223372036854775807");
    if (result < minValue || result > maxValue)
      throw new Error(`OverflowException: Value '${s}' was either too large or too small for an Int64.`);
    return result;
  } catch {
    throw new Error(`FormatException: String '${s}' was not recognized as a valid Int64.`);
  }
}
export function _2cba636c245c1675(s, result) {
  if (s === null)
    return [false, BigInt.zero];
  let trimmed = s.trim();
  try {
    let parsed = BigInt(trimmed);
    let minValue = BigInt("-9223372036854775808");
    let maxValue = BigInt("9223372036854775807");
    if (parsed < minValue || parsed > maxValue)
      return [false, BigInt.zero];
    return [true, parsed];
  } catch {
    return [false, BigInt.zero];
  }
}
export function _28273cd350760efe(left, right) {
  if (right === BigInt.zero)
    throw new Error("DivideByZeroException");
  let quotient = left / right;
  let remainder = left % right;
  return { quotient: quotient, remainder: remainder };
}
export function _77fd605bbb6ce669(value) {
  let count = BigInt.zero;
  let v = value;
  while (v > BigInt.zero) {
    count = count + (v & BigInt.one);
    v = v >> BigInt.one;
  }
  return count;
}
export function _62ef461b6a515b85(value, rotateAmount) {
  let amount = BigInt(rotateAmount % 64);
  if (amount < BigInt.zero)
    amount = amount + BigInt(64);
  return value << amount | value >> BigInt(64) - amount;
}
export function _6a70bc88f689ce73(value, rotateAmount) {
  let amount = BigInt(rotateAmount % 64);
  if (amount < BigInt.zero)
    amount = amount + BigInt(64);
  return value >> amount | value << BigInt(64) - amount;
}
export function _df6d7288bc845b53(value) {
  if (value === BigInt.zero)
    return BigInt(64);
  let count = BigInt.zero;
  let v = value;
  while ((v & BigInt.one) === BigInt.zero) {
    v = v >> BigInt.one;
    count = count + BigInt.one;
  }
  return count;
}
export function _9618dc0d855ee729(x, y) {
  let absX = x < BigInt.zero ? -x : x;
  let absY = y < BigInt.zero ? -y : y;
  if (absX > absY)
    return x;
  if (absX < absY)
    return y;
  return x > y ? x : y;
}
export function _bfad1ee52075b36e(x, y) {
  let absX = x < BigInt.zero ? -x : x;
  let absY = y < BigInt.zero ? -y : y;
  if (absX < absY)
    return x;
  if (absX > absY)
    return y;
  return x < y ? x : y;
}
export const Int64Module = {
  _a108636b79b7c8d2,
  _4174bb5b72e448a6,
  _2cba636c245c1675,
  _28273cd350760efe,
  _77fd605bbb6ce669,
  _62ef461b6a515b85,
  _6a70bc88f689ce73,
  _df6d7288bc845b53,
  _9618dc0d855ee729,
  _bfad1ee52075b36e
};
