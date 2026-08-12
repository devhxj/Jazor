export function _b50ba86b85d8ac33(instance, value) {
  let bigIntValue;
  if (value === null)
    return 1;
  if (typeof value === "bigint" && (bigIntValue = value, true))
    return instance < bigIntValue ? -1 : instance > bigIntValue ? 1 : 0;
  throw new Error("ArgumentException: Object must be of type UInt64.");
}
export function _ab08b15d1ba56047(s) {
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  let trimmed = s.trim();
  try {
    let result = BigInt(trimmed);
    let minValue = BigInt("0");
    let maxValue = BigInt("18446744073709551615");
    if (result < minValue || result > maxValue)
      throw new Error(`OverflowException: Value '${s}' was either too large or too small for a UInt64.`);
    return result;
  } catch {
    throw new Error(`FormatException: String '${s}' was not recognized as a valid UInt64.`);
  }
}
export function _a2771534d71206bd(s, result) {
  if (s === null)
    return [false, 0n];
  let trimmed = s.trim();
  try {
    let parsed = BigInt(trimmed);
    let minValue = BigInt("0");
    let maxValue = BigInt("18446744073709551615");
    if (parsed < minValue || parsed > maxValue)
      return [false, 0n];
    return [true, parsed];
  } catch {
    return [false, 0n];
  }
}
