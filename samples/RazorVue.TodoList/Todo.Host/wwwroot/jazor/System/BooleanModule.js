function compareCore(left, right) {
  return left === right ? 0 : left ? 1 : -1;
}
export function _f877237b160159b0(instance, obj) {
  if (obj === null)
    return 1;
  if (typeof obj !== "boolean")
    throw new Error("ArgumentException: Object must be of type Boolean.");
  return compareCore(instance, obj);
}
export function _5dbf54319ebc8dfe(value) {
  let __cacc$8beb36bd214c5ac941abccee;
  let str = __cacc$8beb36bd214c5ac941abccee = value.trim(), __cacc$8beb36bd214c5ac941abccee == null ? undefined : __cacc$8beb36bd214c5ac941abccee.toLowerCase();
  if (str === "true")
    return true;
  else if (str === "false")
    return false;
  else
    throw new Error(`FormatException: String '${value}' was not recognized as a valid Boolean.`);
}
export function _c3ccfdf8f687d2bf(value) {
  return _5dbf54319ebc8dfe(value);
}
export function _dada4bbdacd7aa19(value, result) {
  let __cacc$2291717227130be354a63be9;
  let str = __cacc$2291717227130be354a63be9 = value.trim(), __cacc$2291717227130be354a63be9 == null ? undefined : __cacc$2291717227130be354a63be9.toLowerCase();
  if (str === "true")
    return [true, true];
  else if (str === "false")
    return [true, false];
  return [false, false];
}
export function _619c4d1c94319558(value, result) {
  return _dada4bbdacd7aa19(value, result);
}
export const BooleanModule = {
  compareCore,
  _f877237b160159b0,
  _5dbf54319ebc8dfe,
  _c3ccfdf8f687d2bf,
  _dada4bbdacd7aa19,
  _619c4d1c94319558
};
