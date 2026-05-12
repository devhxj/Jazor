function isInfinityCore(value) {
  return Object.is(value, Number.POSITIVE_INFINITY) || Object.is(value, Number.NEGATIVE_INFINITY);
}
function isFiniteCore(value) {
  return !isNaN(value) && !isInfinityCore(value);
}
function areEqualCore(left, right) {
  if (isNaN(left) || isNaN(right))
    return isNaN(left) && isNaN(right);
  return !(left < right) && !(left > right);
}
function compareCore(left, right) {
  if (isNaN(left))
    return isNaN(right) ? 0 : -1;
  if (isNaN(right))
    return 1;
  if (left < right)
    return -1;
  if (left > right)
    return 1;
  return 0;
}
function isPow2Core(value) {
  if (!isFiniteCore(value) || value <= 0)
    return false;
  let exponent = Math.log2(value);
  return isFiniteCore(exponent) && Math.floor(exponent) === exponent;
}
export function signCore(value) {
  if (isNaN(value))
    throw new Error("ArithmeticException: Function does not accept floating point Not-a-Number values.");
  if (value > 0)
    return 1;
  if (value < 0)
    return -1;
  return 0;
}
function maxMagnitudeCore(x, y) {
  if (isNaN(x) || isNaN(y))
    return Number.NaN;
  let absX = Math.abs(x);
  let absY = Math.abs(y);
  if (absX > absY)
    return x;
  if (absX < absY)
    return y;
  return Math.max(x, y);
}
function minMagnitudeCore(x, y) {
  if (isNaN(x) || isNaN(y))
    return Number.NaN;
  let absX = Math.abs(x);
  let absY = Math.abs(y);
  if (absX < absY)
    return x;
  if (absX > absY)
    return y;
  return Math.min(x, y);
}
function maxMagnitudeNumberCore(x, y) {
  if (isNaN(x))
    return y;
  if (isNaN(y))
    return x;
  return maxMagnitudeCore(x, y);
}
function minMagnitudeNumberCore(x, y) {
  if (isNaN(x))
    return y;
  if (isNaN(y))
    return x;
  return minMagnitudeCore(x, y);
}
export function _b0d483b6deae2278(instance, value) {
  if (value === null)
    return 1;
  if (typeof value !== "number")
    throw new Error("ArgumentException: Object must be of type Double.");
  return compareCore(instance, value);
}
export function _b5f97a04bba189b0(instance, obj) {
  if (obj === null || typeof obj !== "number")
    return false;
  return areEqualCore(instance, obj);
}
export function _5810f85a3710b88d(s) {
  if (s === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  let trimmed = s.trim();
  if (trimmed.length === 0)
    throw new Error("FormatException: The input string was not in a correct format.");
  let result = Number(trimmed);
  if (isNaN(result))
    throw new Error(`FormatException: The input string '${s}' was not in a correct format.`);
  return result;
}
export function _a29d389185c5e37d(s, result) {
  if (s === null || s.length === 0)
    return [false, 0];
  try {
    let trimmed = s.trim();
    if (trimmed.length === 0)
      return [false, 0];
    let val = Number(trimmed);
    if (isNaN(val))
      return [false, 0];
    return [true, val];
  } catch {
    return [false, 0];
  }
}
export function _0f9f49a802919a8f(value) {
  return isPow2Core(value);
}
export function _eee146c74a9bc322(value) {
  return signCore(value);
}
export function _b6202851542d164c(x, y) {
  return maxMagnitudeCore(x, y);
}
export function _7f7b38b043f3f42f(x, y) {
  return maxMagnitudeNumberCore(x, y);
}
export function _bb1daa880a2ad14e(x, y) {
  return minMagnitudeCore(x, y);
}
export function _315c6cdfa11efcf2(x, y) {
  return minMagnitudeNumberCore(x, y);
}
export function _f1644d5121fae09c(s, provider, result) {
  if (s === null || s.length === 0)
    return [false, 0];
  try {
    let trimmed = s.trim();
    if (trimmed.length === 0)
      return [false, 0];
    let val = Number(trimmed);
    if (isNaN(val))
      return [false, 0];
    return [true, val];
  } catch {
    return [false, 0];
  }
}
export function _bc56189e3e1f8a22(x) {
  return { sin: Math.sin(x), cos: Math.cos(x) };
}
export function _0f4aeef5d225794d(x) {
  let angle = x * Math.PI;
  return { sinPi: Math.sin(angle), cosPi: Math.cos(angle) };
}
