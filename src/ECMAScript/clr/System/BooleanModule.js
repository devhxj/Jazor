function CompareCore(left, right) {
  return left === right ? 0 : left ? 1 : -1;
}
/*jazor:clr-member bool.CompareTo(object)*/
export function _f877237b160159b0(instance, obj) {
  if (obj === null)
    return 1;
  if (typeof obj !== "boolean")
    throw new Error("ArgumentException: Object must be of type Boolean.");
  return CompareCore(instance, obj);
}
/*jazor:clr-member static bool.Parse(string)*/
export function _5dbf54319ebc8dfe(value) {
  if (value === null)
    throw new Error("ArgumentNullException: String cannot be null.");
  let str = value.trim().toLowerCase();
  if (str === "true")
    return true;
  else if (str === "false")
    return false;
  else
    throw new Error(`FormatException: String '${value ?? ""}' was not recognized as a valid Boolean.`);
}
/*jazor:clr-member static bool.Parse(System.ReadOnlySpan<char>)*/
export function _c3ccfdf8f687d2bf(value) {
  return _5dbf54319ebc8dfe(value);
}
/*jazor:clr-member static bool.TryParse(string, out bool)*/
export function _dada4bbdacd7aa19(value, result) {
  if (value === null)
    return [false, false];
  let str = value.trim().toLowerCase();
  if (str === "true")
    return [true, true];
  else if (str === "false")
    return [true, false];
  return [false, false];
}
/*jazor:clr-member static bool.TryParse(System.ReadOnlySpan<char>, out bool)*/
export function _619c4d1c94319558(value, result) {
  return _dada4bbdacd7aa19(value, result);
}
