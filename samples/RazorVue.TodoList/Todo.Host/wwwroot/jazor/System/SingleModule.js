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
function isFiniteCore(value) {
  return !isNaN(value) && value !== Number.POSITIVE_INFINITY && value !== Number.NEGATIVE_INFINITY;
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
export function _0b80f2f2f1a3c1a6(instance, value) {
  if (value === null)
    return 1;
  if (typeof value !== "number")
    throw new Error("ArgumentException: Object must be of type Single.");
  return compareCore(instance, value);
}
export function _eb69b50c7032a809(instance, obj) {
  if (obj === null || typeof obj !== "number")
    return false;
  return areEqualCore(instance, obj);
}
export function _d0492a7790d81596(s) {
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
export function _ced8b209dbd75890(s, result) {
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
export function _0dcf89ab5d6bd60c(value) {
  return isPow2Core(value);
}
export function _323a6b94e62b2729(value) {
  return signCore(value);
}
export function _7c146ff0a50e958f(x, y) {
  return maxMagnitudeCore(x, y);
}
export function _b7b1d7781578b7e0(x, y) {
  return maxMagnitudeNumberCore(x, y);
}
export function _e5a7b14f707c69f7(x, y) {
  return minMagnitudeCore(x, y);
}
export function _4a2ec5d010e27cb1(x, y) {
  return minMagnitudeNumberCore(x, y);
}
export function _9905e3952bca67bc(x) {
  return { sin: Math.sin(x), cos: Math.cos(x) };
}
export function _2c792a5d6ef88cd1(x) {
  let angle = x * Math.PI;
  return { sinPi: Math.sin(angle), cosPi: Math.cos(angle) };
}
