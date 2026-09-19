import { EqualsCore } from "System/Collections/Generic/EqualityComparerT1Module.js";
import { MaterializeReadOnlyCharSpan } from "System/RuntimeModule.js";
import { _5ad63706a889c294 } from "System/StringModule.js";
/*jazor:clr-member System.ReadOnlySpan<char>.Trim()*/
export function trim(source) {
  return MaterializeReadOnlyCharSpan(source).trim();
}
/*jazor:clr-member System.ReadOnlySpan<char>.Trim(char)*/
export function trimCharacter(source, trimChar) {
  return trimCharacters(source, trimChar.toString());
}
/*jazor:clr-member System.ReadOnlySpan<char>.Trim(System.ReadOnlySpan<char>)*/
export function trimCharacters(source, trimChars) {
  return TrimCharacterSet(source, trimChars, true, true);
}
/*jazor:clr-member System.ReadOnlySpan<char>.TrimStart()*/
function TrimStart(source) {
  return MaterializeReadOnlyCharSpan(source).trimStart();
}
export { TrimStart as trimStart };
/*jazor:clr-member System.ReadOnlySpan<char>.TrimStart(char)*/
export function trimStartCharacter(source, trimChar) {
  return TrimCharacterSet(source, trimChar.toString(), true, false);
}
/*jazor:clr-member System.ReadOnlySpan<char>.TrimStart(System.ReadOnlySpan<char>)*/
export function trimStartCharacters(source, trimChars) {
  return TrimCharacterSet(source, trimChars, true, false);
}
/*jazor:clr-member System.ReadOnlySpan<char>.TrimEnd()*/
function TrimEnd(source) {
  return MaterializeReadOnlyCharSpan(source).trimEnd();
}
export { TrimEnd as trimEnd };
/*jazor:clr-member System.ReadOnlySpan<char>.TrimEnd(char)*/
export function trimEndCharacter(source, trimChar) {
  return TrimCharacterSet(source, trimChar.toString(), false, true);
}
/*jazor:clr-member System.ReadOnlySpan<char>.TrimEnd(System.ReadOnlySpan<char>)*/
export function trimEndCharacters(source, trimChars) {
  return TrimCharacterSet(source, trimChars, false, true);
}
/*jazor:clr-member static System.MemoryExtensions.Contains<T>(System.ReadOnlySpan<T>, T)*/
export function _a4ed2b50c69946de(source, value) {
  if (source === null)
    throw new Error("ArgumentNullException: source is null");
  for (let index = 0; index < source.length; index++) {
    if (EqualsCore(source[index], value))
      return true;
  }
  return false;
}
/*jazor:clr-member System.ReadOnlySpan<T>.SequenceEqual<T>(System.ReadOnlySpan<T>)*/
export function sequenceEqual(first, second) {
  if (first === null)
    throw new Error("ArgumentNullException: first is null");
  if (second === null)
    throw new Error("ArgumentNullException: second is null");
  if (first.length !== second.length)
    return false;
  for (let index = 0; index < first.length; index++) {
    if (!EqualsCore(first[index], second[index]))
      return false;
  }
  return true;
}
function TrimCharacterSet(source, trimChars, trimStart, trimEnd) {
  let text = MaterializeReadOnlyCharSpan(source);
  let characters = MaterializeReadOnlyCharSpan(trimChars);
  if (characters.length === 0)
    return text;
  let start = 0;
  let end = text.length - 1;
  if (trimStart) {
    while (start <= end && characters.indexOf(_5ad63706a889c294(text, start)) >= 0)
      start++;
  }
  if (trimEnd) {
    while (end >= start && characters.indexOf(_5ad63706a889c294(text, end)) >= 0)
      end--;
  }
  if (start === 0 && end === text.length - 1)
    return text;
  return start > end ? "" : text.substring(start, start + (end - start + 1));
}
